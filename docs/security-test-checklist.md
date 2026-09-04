# XSS and SQL injection — audit and test record

**System:** VMS at `https://vms.dipark.com` (UATWEB01, IIS, Windows Authentication).
**Date of audit:** 4 September 2026. **Code audited:** `4b53b88`.

Part 1 is a source audit — for injection questions that is stronger evidence than
black-box probing, because it shows there is no reachable sink rather than that a
particular payload failed. Part 2 is the manual test to run at a desk and record.

---

## Part 1 — Source audit

### SQL injection: no exposure

| Checked | Finding |
|---|---|
| Raw SQL | Two statements, both in `Data/DbBootstrapper.cs` (lines 53, 103). Constant strings against `sys.tables` and `sys.columns`. No user input, no interpolation, no concatenation. |
| `ExecuteSqlRaw` / `FromSqlRaw` | None in the codebase. |
| All other queries | LINQ over EF Core, which parameterises. Verified: the person search, the report's entity and date filters, the entity list. |
| Generated DDL | `DbBootstrapper` builds it from the EF model, not from input. |
| Report filters | `entityId` binds to `int`, dates to date inputs — a non-numeric value fails model binding before reaching a query. |

### Cross-site scripting: no exposure

| Checked | Finding |
|---|---|
| Razor output | `@expression` is HTML-encoded by the framework. Every field displayed on both screens goes through it. |
| `MarkupString` | One use, `NewVisitor.razor:24`. Renders `Steps[n].Icon` — a `private static readonly` array of SVG path constants. Not reachable from input. |
| `innerHTML` / DOM writes | None. `wwwroot/app.js` uses `Blob` + `a.download`; `card-agent.js` writes no DOM. |
| Dynamic `src` | `data:` URLs built server-side with fixed MIME types (`image/jpeg`; PNG from `ImageConverter`). Never `image/svg+xml`, so no script vector. The logo path comes from `BrandAssets` scanning a fixed folder for fixed filenames. |
| Dynamic `href` | None. No `javascript:` URL vector. |

### Already handled, verified rather than assumed

- **XXE** on the agent's XML, which is untrusted by definition:
  `DtdProcessing.Prohibit`, `XmlResolver = null`, and an 8 MB cap
  (`CardResponseParser`).
- **CSV formula injection**: `VisitorReport.Escape` prefixes a leading
  `= + - @ tab CR` with `'`, so a name beginning `=` cannot execute in a spreadsheet.
- **Replay** of a captured card response: the request ID is server-issued, random,
  single-use and expires in five minutes.

### Fixed as a result of the audit (`4b53b88`)

1. **Column widths existed only in `VmsDbContext`.** A form could accept 400 characters
   into a 300-character column and fail at the database, at the desk. `FieldLengths` now
   holds them once; inputs cap on it and every string is fitted to its column on the way
   in, logged when it has to cut.
2. **The person search passed the typed term straight into `LIKE`.** Not injectable — EF
   parameterises it — but a lone `%` matched every person and `_` matched any character.
   Pattern characters are now escaped with an `ESCAPE` clause.
3. **The host picker had no length cap**, so the one field the visitor's host is recorded
   in was the one field that could exceed its column.

### Not an injection finding, but the real exposure

**The application has no authorisation.** IIS Windows Authentication establishes *who* a
request is from; nothing decides *what* they may see. Any domain user who reaches the URL
can read every visitor record — Emirates ID numbers, photographs, dates of birth.
Roles belong in the application. See `docs/deployment.md`.

---

## Part 2 — Manual test to run at a desk

Sign in as normal. Enter each payload, save, then check the report and the export.

### XSS payloads

Enter in **every** free-text field in turn: manual **Full name (English)**, **Full name
(Arabic)**, **Nationality**, **Person to visit**, and **Purpose → Other → details**.

```
<script>alert('xss')</script>
"><img src=x onerror=alert('xss')>
<svg/onload=alert('xss')>
'"><b>bold</b>
javascript:alert('xss')
</td></tr><tr><td>injected
```

**Pass:** the text appears on screen exactly as typed, angle brackets and all. No dialog.
No bold text. No broken table layout on the report. Nothing appears in the browser
console.

**Fail:** a dialog, rendered HTML, or a mangled report row.

### SQL injection payloads

Same fields, plus the **Person to visit** search box (where the query is built):

```
' OR '1'='1
'; DROP TABLE vms.VisitorEntry; --
admin'--
1' UNION SELECT NULL,NULL,NULL--
' OR 1=1; EXEC xp_cmdshell 'dir'--
```

**Pass:** stored and displayed as literal text. No error page. The report row count goes
up by exactly one per save. `vms.VisitorEntry` still exists.

Confirm on the server:

```sql
USE VMS;
SELECT COUNT(*) AS Rows FROM vms.VisitorEntry;
SELECT TOP 10 Id, IdNumber, FullNameEnglish, PersonToVisit, PurposeOther
FROM vms.VisitorEntry ORDER BY Id DESC;
```

The payloads should be sitting in the columns verbatim.

### LIKE pattern characters (the fix in `4b53b88`)

In the **Person to visit** box type each of `%`, `_`, `[a-z]`, `\`.

**Pass:** no suggestions, or only genuine matches for that literal text.
**Fail (pre-fix behaviour):** `%` returns a list of unrelated people.

### Field lengths

Paste 500 characters of `A` into the manual **Full name (English)**.

**Pass:** the field stops accepting at 300 and the record saves.
**Fail:** a database error on save.

### CSV export

Save a visitor whose name is `=1+1` or `=cmd|'/c calc'!A1`, export, open in Excel.

**Pass:** the cell shows the text, prefixed with an apostrophe. No formula evaluates, no
prompt to enable content.

### Result

| Test | Result | Tested by | Date |
|---|---|---|---|
| XSS — visitor information fields | | | |
| XSS — visit details fields | | | |
| SQL injection — stored fields | | | |
| SQL injection — person search | | | |
| LIKE pattern characters | | | |
| Field length limits | | | |
| CSV formula injection | | | |

Delete the test entries afterwards, and note that they were deleted:

```sql
DELETE FROM vms.VisitorEntry WHERE FullNameEnglish LIKE '%script%' OR FullNameEnglish LIKE '%DROP%';
```
