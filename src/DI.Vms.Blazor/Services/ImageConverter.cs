using System.Drawing;
using System.Drawing.Imaging;

namespace DI.Vms.Blazor.Services;

/// <summary>
/// Converts card images into something a browser will display.
///
/// The holder's signature is stored on the chip as TIFF. WPF renders that happily, which
/// is why the vendor sample shows it, but no browser will - so it is converted to PNG
/// server-side rather than handed to an <c>img</c> tag that would silently show nothing.
/// </summary>
public static class ImageConverter
{
    /// <summary>
    /// Returns a data URL a browser can render, or null when the bytes are not an image
    /// this can handle. Never throws: a signature that will not convert must not stop a
    /// visitor being registered.
    /// </summary>
    public static string? ToDisplayableDataUrl(byte[]? bytes, ILogger? logger = null)
    {
        if (bytes is null || bytes.Length == 0)
        {
            logger?.LogInformation("No signature image on the card.");
            return null;
        }

        /* Logged every read, because a signature that will not display is otherwise
           undiagnosable from a browser: the magic bytes and the length say immediately
           whether the chip gave us something unexpected or the conversion is at fault. */
        logger?.LogInformation(
            "Signature image: {Length} bytes, starts {Magic}, detected {Format}.",
            bytes.Length, Magic(bytes), Detect(bytes));

        var format = Detect(bytes);

        switch (format)
        {
            case "png":
            case "jpeg":
            case "gif":
                // Already renderable; hand it over untouched.
                return DataUrl(bytes, $"image/{format}");

            case "tiff":
            case "bmp":
                break;

            default:
                logger?.LogWarning(
                    "Signature image is not a format this can display ({Magic}); showing a placeholder.",
                    Magic(bytes));
                return null;
        }

        try
        {
            using var input = new MemoryStream(bytes, writable: false);
            using var image = Image.FromStream(input, useEmbeddedColorManagement: false,
                                               validateImageData: true);

            /* Redrawn onto an opaque white bitmap rather than saved straight across. A
               bilevel signature scan carries no colour table a browser can rely on, and
               flattening it here is what makes the PNG predictable. */
            using var flattened = new Bitmap(image.Width, image.Height, PixelFormat.Format24bppRgb);
            using (var g = Graphics.FromImage(flattened))
            {
                g.Clear(Color.White);
                g.DrawImageUnscaled(image, 0, 0);
            }

            using var output = new MemoryStream();
            flattened.Save(output, ImageFormat.Png);
            var png = output.ToArray();

            if (png.Length == 0)
            {
                logger?.LogWarning("Signature conversion produced no bytes; showing a placeholder.");
                return null;
            }

            logger?.LogInformation(
                "Signature converted to PNG: {Width}x{Height}, {Length} bytes.",
                image.Width, image.Height, png.Length);

            return DataUrl(png, "image/png");
        }
        catch (Exception ex)
        {
            logger?.LogWarning(ex, "Could not convert the signature image to PNG");
            return null;
        }
    }

    /// <summary>
    /// The container the bytes actually are, by signature rather than by extension.
    /// Checked in full: a two-byte guess passes non-images through as PNG, and the
    /// browser then shows a broken image instead of the placeholder.
    /// </summary>
    private static string Detect(byte[] b)
    {
        if (b.Length >= 8 && b[0] == 0x89 && b[1] == 0x50 && b[2] == 0x4E && b[3] == 0x47
                          && b[4] == 0x0D && b[5] == 0x0A && b[6] == 0x1A && b[7] == 0x0A)
        {
            return "png";
        }

        if (b.Length >= 3 && b[0] == 0xFF && b[1] == 0xD8 && b[2] == 0xFF) return "jpeg";
        if (b.Length >= 6 && b[0] == 0x47 && b[1] == 0x49 && b[2] == 0x46) return "gif";
        if (b.Length >= 2 && b[0] == 0x42 && b[1] == 0x4D) return "bmp";

        // Little-endian "II*\0" and big-endian "MM\0*".
        if (b.Length >= 4 && b[0] == 0x49 && b[1] == 0x49 && b[2] == 0x2A && b[3] == 0x00) return "tiff";
        if (b.Length >= 4 && b[0] == 0x4D && b[1] == 0x4D && b[2] == 0x00 && b[3] == 0x2A) return "tiff";

        return "unknown";
    }

    private static string Magic(byte[] b) =>
        Convert.ToHexString(b, 0, Math.Min(8, b.Length));

    private static string DataUrl(byte[] bytes, string mime) =>
        $"data:{mime};base64,{Convert.ToBase64String(bytes)}";
}
