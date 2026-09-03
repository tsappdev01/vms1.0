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
        if (bytes is null || bytes.Length < 4) return null;

        // Already browser-renderable.
        if (bytes[0] == 0x89 && bytes[1] == 0x50) return DataUrl(bytes, "image/png");
        if (bytes[0] == 0xFF && bytes[1] == 0xD8) return DataUrl(bytes, "image/jpeg");

        var isTiff = (bytes[0] == 0x49 && bytes[1] == 0x49 && bytes[2] == 0x2A)
                  || (bytes[0] == 0x4D && bytes[1] == 0x4D && bytes[3] == 0x2A);

        if (!isTiff) return null;

        try
        {
            using var input = new MemoryStream(bytes);
            using var image = Image.FromStream(input);
            using var output = new MemoryStream();
            image.Save(output, ImageFormat.Png);
            return DataUrl(output.ToArray(), "image/png");
        }
        catch (Exception ex)
        {
            logger?.LogWarning(ex, "Could not convert the signature image to PNG");
            return null;
        }
    }

    private static string DataUrl(byte[] bytes, string mime) =>
        $"data:{mime};base64,{Convert.ToBase64String(bytes)}";
}
