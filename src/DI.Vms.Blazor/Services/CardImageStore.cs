using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using DI.Vms.Blazor.Data;

namespace DI.Vms.Blazor.Services;

/// <summary>
/// Where the image of the card that was read is kept.
///
/// Azure Blob Storage when a storage account is configured, the database column when it is
/// not - and the choice is one setting, so UATWEB01, which has no storage account and is
/// not on the internet, keeps working unchanged.
///
/// Blob is the better home for it in Azure, and the reason is the shape of the data rather
/// than a preference. A card image is 30-40 KB of opaque bytes that nothing queries, joins
/// or indexes. In SQL those bytes are in every backup, every restore and every DTU the
/// database is billed for; a year of a busy desk is a database that is mostly pictures. In
/// blob they are a fraction of the price, they never slow a query down, and a lifecycle
/// rule can move them to cool storage or delete them at an age without a migration.
///
/// Everything else about the visit stays in SQL. The blob holds only what the card looked
/// like; the record of who visited whom is still one row.
/// </summary>
public sealed class CardImageStorageOptions
{
    public const string SectionName = "Storage";

    /// <summary>
    /// The storage account connection string - a credential, so it comes from the Web App's
    /// application settings and never from a file in the repository.
    /// </summary>
    public string ConnectionString { get; init; } = string.Empty;

    /// <summary>The container. Created on first use if it is not there.</summary>
    public string Container { get; init; } = "vms";

    /// <summary>
    /// A prefix inside the container, so a container shared with something else stays
    /// legible. Blob storage has no real folders - this is part of the name.
    /// </summary>
    public string Prefix { get; init; } = "cards";

    public bool Enabled => !string.IsNullOrWhiteSpace(ConnectionString);

    public static CardImageStorageOptions FromConfiguration(IConfiguration config)
    {
        var section = config.GetSection(SectionName);

        string Setting(string name, string fallback) =>
            section[name] is { Length: > 0 } value ? value : fallback;

        return new CardImageStorageOptions
        {
            /* Also read from the connection strings blade, because that is where somebody
               setting up a Web App will naturally put a storage connection string. App
               Service exposes it as CUSTOMCONNSTR_Storage, which configuration surfaces
               under ConnectionStrings:Storage. */
            ConnectionString = Setting("ConnectionString", config.GetConnectionString("Storage") ?? string.Empty),
            Container = Setting("Container", "vms"),
            Prefix = Setting("Prefix", "cards").Trim('/'),
        };
    }
}

/// <summary>
/// Writes and reads the card image, wherever <see cref="CardImageStorageOptions"/> says it
/// lives. Callers deal in a <see cref="VisitorCardImage"/> and never in a container name.
/// </summary>
public sealed class CardImageStore
{
    private readonly CardImageStorageOptions options;
    private readonly ILogger<CardImageStore> log;
    private readonly BlobContainerClient? container;

    /// <summary>
    /// The container is created once, on the first write, and not checked again. Doing it
    /// per save would be a network round trip on every visitor for a thing that changes
    /// once in the life of the deployment.
    /// </summary>
    private readonly SemaphoreSlim creating = new(1, 1);
    private bool containerReady;

    public CardImageStore(CardImageStorageOptions options, ILogger<CardImageStore> log)
    {
        this.options = options;
        this.log = log;

        if (!options.Enabled) return;

        try
        {
            container = new BlobServiceClient(options.ConnectionString)
                .GetBlobContainerClient(options.Container);
        }
        catch (Exception ex)
        {
            /* Not thrown: a malformed storage connection string must not stop reception
               taking visitors. The image is what is lost, and the record is not. */
            log.LogError(ex,
                "Storage:ConnectionString is set but could not be used. Card images will be stored in the database instead.");
        }
    }

    public bool UsesBlobStorage => container is not null;

    /// <summary>
    /// Puts the image where it belongs and returns the row to attach to the visit, or null
    /// if there was no image to store.
    ///
    /// Written before the visit is saved, because the visit needs the blob's name. If the
    /// save then fails, <see cref="DiscardAsync"/> takes the blob back out - an orphan blob
    /// is cheap but it is still litter, and litter nobody can attribute to a visit is
    /// litter nobody will ever delete.
    /// </summary>
    public async Task<VisitorCardImage?> StoreAsync(byte[]? image, string contentType, CancellationToken ct = default)
    {
        if (image is null || image.Length == 0) return null;

        if (container is null)
        {
            return new VisitorCardImage { Image = image, ContentType = contentType };
        }

        /* A GUID, not the visit ID: the visit has no ID until it is saved, and this has to
           be written first. Dated folders so a lifecycle rule can act on an age and so a
           container listing is navigable at a hundred thousand cards. */
        var now = DateTimeOffset.UtcNow;
        var name = $"{options.Prefix}/{now:yyyy}/{now:MM}/{Guid.NewGuid():N}{Extension(contentType)}";

        try
        {
            await EnsureContainerAsync(ct);

            var blob = container.GetBlobClient(name);

            using var content = new MemoryStream(image, writable: false);

            await blob.UploadAsync(content, new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders
                {
                    ContentType = contentType,

                    /* Never render this in a browser tab from the storage URL. The reason is
                       the format: an SVG is a document, it can carry script, and a container
                       served straight to a browser would be running it on the storage
                       account's origin. It is served through /visits/{id}/card instead,
                       which requires the report role and sends a CSP that forbids script. */
                    ContentDisposition = "attachment",
                },
            }, ct);

            return new VisitorCardImage { BlobName = name, ContentType = contentType };
        }
        catch (Exception ex) when (ex is RequestFailedException or IOException)
        {
            /* Fall back to the column rather than lose the image or fail the check-in. The
               visitor is at the desk; a storage outage is not their problem. */
            log.LogError(ex,
                "Could not write the card image to blob storage. It is being stored in the database for this visit.");

            return new VisitorCardImage { Image = image, ContentType = contentType };
        }
    }

    /// <summary>
    /// Removes a blob written for a visit that was not saved after all. Best effort: a
    /// failure here is logged and swallowed, because the caller is already handling the
    /// failure that brought it here and a second exception would hide the first.
    /// </summary>
    public async Task DiscardAsync(VisitorCardImage? record, CancellationToken ct = default)
    {
        if (container is null || record?.BlobName is not { Length: > 0 } name) return;

        try
        {
            await container.GetBlobClient(name).DeleteIfExistsAsync(cancellationToken: ct);
        }
        catch (Exception ex)
        {
            log.LogWarning(ex, "Could not delete the orphaned card image {Blob}.", name);
        }
    }

    /// <summary>
    /// The image itself, from wherever the row says it is. Null when the blob has gone -
    /// which the caller should answer as 404 rather than 500: a lifecycle rule deleting an
    /// old card is a thing that is meant to happen.
    /// </summary>
    public async Task<byte[]?> ReadAsync(VisitorCardImage record, CancellationToken ct = default)
    {
        if (record.Image is { Length: > 0 } stored) return stored;

        if (container is null || record.BlobName is not { Length: > 0 } name)
        {
            if (record.BlobName is { Length: > 0 })
            {
                log.LogWarning(
                    "Visit {Id} has a card image in blob storage but no storage account is configured.",
                    record.VisitorEntryId);
            }

            return null;
        }

        try
        {
            var response = await container.GetBlobClient(name).DownloadContentAsync(ct);
            return response.Value.Content.ToArray();
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            log.LogWarning("The card image for visit {Id} is no longer in storage ({Blob}).", record.VisitorEntryId, name);
            return null;
        }
    }

    private async Task EnsureContainerAsync(CancellationToken ct)
    {
        if (containerReady || container is null) return;

        await creating.WaitAsync(ct);
        try
        {
            if (containerReady) return;

            /* PublicAccessType.None, explicitly. The default on a created container is
               already private, but this holds a visitor's Emirates ID photograph and that
               is not a thing to leave to a default. */
            await container.CreateIfNotExistsAsync(PublicAccessType.None, cancellationToken: ct);
            containerReady = true;
        }
        finally
        {
            creating.Release();
        }
    }

    private static string Extension(string contentType) => contentType switch
    {
        "image/svg+xml" => ".svg",
        "image/png" => ".png",
        "image/jpeg" => ".jpg",
        _ => ".bin",
    };
}
