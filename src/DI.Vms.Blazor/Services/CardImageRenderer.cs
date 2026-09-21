using System.Security;
using System.Text;

namespace DI.Vms.Blazor.Services;

/// <summary>
/// Renders the card that was read as a self-contained image, stored with the visit.
///
/// Why the server draws it rather than the browser capturing what is on screen: the tablet
/// has no HTML card. Its screen is Compose, so a browser screenshot library would give the
/// web desk an artefact and the tablet none, and the two halves of one visitor log would
/// stop matching. Drawing it here means both clients produce exactly the same record
/// without either of them cooperating - and it is drawn from the data the server parsed out
/// of the signed response, not from anything a client sent separately.
///
/// SVG rather than PNG, for three reasons that all point the same way: no image library and
/// so no new dependency, nothing that needs a native codec on a Linux Web App, and it
/// stays sharp when somebody prints it. It is self-contained - the photograph is embedded -
/// so the file is meaningful on its own, years later, with no database behind it.
///
/// It is a RECORD, not a reproduction. The banner at the foot says so on the face of the
/// image, because an artefact that looks like an identity document and is not should say
/// which it is, to the auditor who finds it and to anyone it is ever shown to.
/// </summary>
public static class CardImageRenderer
{
    public const string ContentType = "image/svg+xml";

    /* Ten pixels per millimetre of an ID-1 card, so the proportions are the card's own
       and a print at 100% is life size. */
    private const int Width = 856;
    private const int Height = 540;

    /// <summary>
    /// The card as SVG, or null when there is not enough to draw one.
    ///
    /// This guard is about data, not provenance: whether a typed entry should have an
    /// image at all is the caller's decision, because only the caller knows how the record
    /// arrived. Both save paths answer no.
    /// </summary>
    public static byte[]? Render(CardData card)
    {
        if (string.IsNullOrWhiteSpace(card.IdNumber)) return null;

        var svg = new StringBuilder(8 * 1024);

        svg.Append($"""
            <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 {Width} {Height}" width="{Width}" height="{Height}" role="img">
            <title>Emirates ID record - {X(card.FullNameEnglish)}</title>
            <defs>
              <style>
                .l  {{ font: 11px 'Segoe UI', Tahoma, Arial, sans-serif; fill: #7b8794; }}
                .v  {{ font: 600 17px 'Segoe UI', Tahoma, Arial, sans-serif; fill: #16232e; }}
                .vs {{ font: 600 14px 'Segoe UI', Tahoma, Arial, sans-serif; fill: #16232e; }}
                .ar {{ font: 15px 'Segoe UI', Tahoma, Arial, sans-serif; fill: #16232e; direction: rtl; }}
                .hd {{ font: 700 15px 'Segoe UI', Tahoma, Arial, sans-serif; fill: #0a3255; }}
                .hs {{ font: 9px 'Segoe UI', Tahoma, Arial, sans-serif; fill: #5a6b7b; }}
                .fp {{ font: 10px 'Segoe UI', Tahoma, Arial, sans-serif; fill: #6b7a88; }}
              </style>
            </defs>

            <rect width="{Width}" height="{Height}" rx="14" fill="#f7f9fb" stroke="#d7dfe6" />
            <rect x="0" y="0" width="{Width}" height="70" rx="14" fill="#ffffff" />
            <rect x="0" y="56" width="{Width}" height="14" fill="#ffffff" />
            <line x1="0" y1="70" x2="{Width}" y2="70" stroke="#e2e8ee" />

            <text class="hd" x="28" y="30">UNITED ARAB EMIRATES</text>
            <text class="hs" x="28" y="48">Federal Authority for Identity &amp; Citizenship, Customs &amp; Port Security</text>
            <text class="hd" x="{Width - 28}" y="30" text-anchor="end" direction="rtl">&#1575;&#1604;&#1573;&#1605;&#1575;&#1585;&#1575;&#1578; &#1575;&#1604;&#1593;&#1585;&#1576;&#1610;&#1577; &#1575;&#1604;&#1605;&#1578;&#1581;&#1583;&#1577;</text>

            """);

        // Photograph. Absent on a card that was read without one; the frame stays, so the
        // layout does not shift and the gap is visibly a gap.
        svg.Append($"""<rect x="28" y="96" width="150" height="188" rx="6" fill="#eef2f6" stroke="#d7dfe6" />""");

        if (card.Photo is { Length: > 0 } photo)
        {
            var data = Convert.ToBase64String(photo);
            svg.Append($"""
                <image x="28" y="96" width="150" height="188" preserveAspectRatio="xMidYMid slice"
                       href="data:image/jpeg;base64,{data}" />
                <rect x="28" y="96" width="150" height="188" rx="6" fill="none" stroke="#d7dfe6" />
                """);
        }
        else
        {
            svg.Append("""<text class="l" x="103" y="196" text-anchor="middle">No photograph</text>""");
        }

        // Left column of the data block, beside the photograph.
        var x = 210;
        Field(svg, x, 118, "ID Number", card.IdNumber);
        Field(svg, x, 176, "Name", card.FullNameEnglish);

        if (!string.IsNullOrWhiteSpace(card.FullNameArabic))
        {
            svg.Append($"""<text class="ar" x="{Width - 28}" y="176" text-anchor="end">{X(card.FullNameArabic)}</text>""");
        }

        Field(svg, x, 240, "Date of Birth", card.DateOfBirth);
        Field(svg, x + 240, 240, "Sex", card.Gender);
        Field(svg, x, 302, "Nationality", card.NationalityEnglish);

        if (!string.IsNullOrWhiteSpace(card.NationalityArabic))
        {
            svg.Append($"""<text class="ar" x="{Width - 28}" y="302" text-anchor="end">{X(card.NationalityArabic)}</text>""");
        }

        Field(svg, x, 364, "Issuing Date", card.IssueDate);
        Field(svg, x + 240, 364, "Expiry Date", card.ExpiryDate);

        svg.Append($"""
            <line x1="28" y1="452" x2="{Width - 28}" y2="452" stroke="#e2e8ee" />
            <text class="l" x="28" y="476">Card Number</text>
            <text class="vs" x="122" y="476">{X(card.CardNumber)}</text>
            """);

        /* The provenance banner. It is the difference between a record and a replica, and
           it is on the image rather than only in the database because the image is the
           thing that gets exported, emailed and printed. */
        var stamp = DateTimeOffset.UtcNow.ToString("d MMM yyyy HH:mm 'UTC'");
        var provenance = card.SignatureWarning is null
            ? "Read from the chip and verified"
            : "Read from the chip - signature not verified";

        svg.Append($"""
            <rect x="0" y="{Height - 46}" width="{Width}" height="46" fill="#0a3255" />
            <text class="fp" x="28" y="{Height - 26}" fill="#c6d4e0">DI Visitor Management - record of an Emirates ID chip read. Not an identity document.</text>
            <text class="fp" x="28" y="{Height - 10}" fill="#8fa6b8">{X(provenance)} - {stamp}</text>
            </svg>
            """);

        return Encoding.UTF8.GetBytes(svg.ToString());
    }

    private static void Field(StringBuilder svg, int x, int y, string label, string? value)
    {
        svg.Append($"""
            <text class="l" x="{x}" y="{y}">{X(label)}</text>
            <text class="v" x="{x}" y="{y + 24}">{X(string.IsNullOrWhiteSpace(value) ? "—" : value)}</text>
            """);
    }

    /// <summary>
    /// XML escaping, on every value without exception.
    ///
    /// A name carrying an ampersand would otherwise produce a document no parser will open,
    /// and one carrying a tag would produce a document that means something else. Both are
    /// reachable from a chip, and the second is the one that matters.
    /// </summary>
    private static string X(string? value) =>
        string.IsNullOrEmpty(value) ? string.Empty : SecurityElement.Escape(value)!;
}
