using System.Runtime.InteropServices;
using System.Text;

namespace DI.Vms.Blazor.Services;

/// <summary>
/// Repairs card text that the toolkit binding handed us in the wrong encoding.
///
/// The native toolkit stores every attribute in a <c>char[]</c> holding UTF-8 bytes. The
/// managed binding reads it with <c>Marshal.PtrToStringAnsi</c> - which decodes with the
/// process ANSI code page, mangling anything non-ASCII - and then repairs it by encoding
/// back through <c>Encoding.GetEncoding(0)</c> and decoding as UTF-8.
///
/// That repair holds on .NET Framework, where code page 0 is the OS ANSI code page. The
/// binding is net472 and the vendor samples are too, so it was never exercised anywhere
/// else. On .NET 8 <c>GetEncoding(0)</c> is UTF-8, so the round trip is the identity and
/// the mangled string survives: an Arabic name arrives as "Ù†ÙŠØ± Ø¬ÙˆØ§Ø¯" instead of
/// "نير جواد".
///
/// So the repair is done here instead. It is conservative: the result is kept only when
/// the text really was UTF-8 read as ANSI, so text that is already correct - and a future
/// binding that fixes this itself - passes through untouched.
/// </summary>
internal static class CardText
{
    private static readonly Encoding StrictUtf8 = new UTF8Encoding(false, throwOnInvalidBytes: true);

    /// <summary>
    /// Maps each character the ANSI decode can produce back to the byte that produced it,
    /// or null when there is nothing to repair. See <see cref="BuildAnsiInverse"/> for why
    /// this is built from the decoder rather than using the encoder directly.
    /// </summary>
    private static readonly Dictionary<char, byte>? AnsiInverse = BuildAnsiInverse();

    /// <summary>
    /// Returns <paramref name="value"/> with a UTF-8-read-as-ANSI mangling undone, or
    /// unchanged if it does not carry one. Never throws.
    /// </summary>
    public static string? Fix(string? value)
    {
        if (AnsiInverse is null || string.IsNullOrEmpty(value)) return value;

        // ASCII survives every single-byte code page unchanged, so it cannot be mangled.
        var nonAscii = false;
        foreach (var c in value)
        {
            if (c > 0x7f) { nonAscii = true; break; }
        }
        if (!nonAscii) return value;

        var bytes = new byte[value.Length];
        for (var i = 0; i < value.Length; i++)
        {
            /* An unmappable character cannot have come through this code page, so the text
               was never mangled - it is real non-ANSI text. Arabic that already decoded
               correctly lands here, which is what makes this safe to run over every
               field unconditionally. */
            if (!AnsiInverse.TryGetValue(value[i], out bytes[i])) return value;
        }

        try
        {
            return StrictUtf8.GetString(bytes);
        }
        catch (DecoderFallbackException)
        {
            /* The bytes are not valid UTF-8, so reading them as UTF-8 would be the
               corruption rather than the cure. A genuinely accented Latin-1 name - where
               the accented byte has no UTF-8 continuation after it - lands here. */
            return value;
        }
    }

    /// <summary>
    /// Inverts the ANSI decode by decoding every single byte, rather than asking the
    /// encoder to map characters back.
    ///
    /// The two are not symmetric where it matters. Windows-1252 leaves five bytes
    /// undefined - 0x81, 0x8D, 0x8F, 0x90 and 0x9D - and the marshaller's decode passes
    /// them through as the control character of the same value, but the encoder is not
    /// obliged to accept those characters back. 0x81 is the second byte of "ف" (U+0641),
    /// a common Arabic letter, so an asymmetry there would quietly refuse to repair a
    /// large share of real names. Inverting the decode cannot disagree with the decode.
    /// </summary>
    private static Dictionary<char, byte>? BuildAnsiInverse()
    {
        try
        {
            // .NET 8 carries only ASCII, Latin-1 and the Unicode encodings; the ANSI code
            // pages live in this provider. Registering is idempotent and process-wide.
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var codePage = (int)GetACP();

            /* 65001 means the process treats ANSI as UTF-8 - which a Windows 10 1903+
               application manifest can ask for - so the marshaller decoded correctly and
               there is nothing to undo. 0 should not happen, and is not a code page. */
            if (codePage is 65001 or 0) return null;

            var ansi = Encoding.GetEncoding(codePage);

            // A multi-byte ANSI code page cannot be inverted a byte at a time, and would
            // have mangled the text differently anyway. Leaving it alone is the safe answer.
            if (!ansi.IsSingleByte) return null;

            var inverse = new Dictionary<char, byte>(256);
            var one = new byte[1];

            for (var b = 0; b <= 0xff; b++)
            {
                one[0] = (byte)b;
                var decoded = ansi.GetString(one);

                // Undefined positions decode to a replacement character, and two bytes
                // must never claim the same character: first mapping wins.
                if (decoded.Length != 1 || decoded[0] == '\uFFFD') continue;
                _ = inverse.TryAdd(decoded[0], (byte)b);
            }

            /* Whatever the table above left unmapped is an undefined position, which the
               marshaller's CP_ACP decode surfaces as the same-valued control character. */
            for (var b = 0x80; b <= 0xff; b++)
            {
                _ = inverse.TryAdd((char)b, (byte)b);
            }

            return inverse;
        }
        catch
        {
            // Unknown code page, or the provider is unavailable. Leave text as it came.
            return null;
        }
    }

    [DllImport("kernel32.dll")]
    private static extern uint GetACP();
}
