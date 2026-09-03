#!/usr/bin/env python3
"""Turns the AD export into db/004_seed_people.sql.

Run this whenever a new export lands, rather than editing the SQL by hand:

    python3 db/tools/generate_seed_people.py "db/AD Export 03_07_2026.xlsx"

Reads the .xlsx directly - no openpyxl, no pandas - because the file is a zip of
XML and the four columns we want are plain shared strings. Sheet 1 is the export;
anything else in the workbook is ignored.

COMPANY_TO_ENTITY below is the only judgement in here. It maps the company names
the export uses onto the names in vms.Entity, which are shorter and do not all
exist. Anything unmapped gets a null DiEntityId and is found only when "Search
all entities" is ticked - never dropped.
"""

import html
import re
import sys
import zipfile
from collections import Counter

# The export's company names on the left, vms.Entity names on the right.
COMPANY_TO_ENTITY = {
    "TechSource":                        "TechSource",
    "Dubai Investments PJSC":            "DI",
    "Dubai Investments Park":            "DIP",
    "Dubai Investments Industries":      "DII",
    "Dubai Investment Realestate":       "DIR",
    "Glass LLC":                         "GlassLLC",
    "Danah Bay":                         "DanahBay",
    "Masharie":                          "Masharie",
    "Al Mujama Real Estate":             "ALMujama",
    "Mujama":                            "ALMujama",   # one row; the same company
    "Properties Investment":             "PI",
    "PID Owners Association Management": "PIDOA",
}


def read_sheet(path):
    """The first worksheet, as a list of {column letter: value}."""
    with zipfile.ZipFile(path) as z:
        shared = []
        if "xl/sharedStrings.xml" in z.namelist():
            xml = z.read("xl/sharedStrings.xml").decode("utf-8")
            for si in re.findall(r"<si>(.*?)</si>", xml, re.S):
                shared.append(html.unescape("".join(re.findall(r"<t[^>]*>(.*?)</t>", si, re.S))))

        xml = z.read("xl/worksheets/sheet1.xml").decode("utf-8")

    rows = []
    for row in re.findall(r"<row[^>]*>(.*?)</row>", xml, re.S):
        cells = {}
        for m in re.finditer(r'<c\b([^>]*?)(?:/>|>(.*?)</c>)', row, re.S):
            attrs, body = m.group(1), m.group(2) or ""
            ref = re.search(r'r="([A-Z]+)\d+"', attrs)
            if not ref:
                continue
            kind = re.search(r't="([^"]+)"', attrs)
            value = re.search(r"<v>(.*?)</v>", body, re.S)
            if kind and kind.group(1) == "s" and value:
                cells[ref.group(1)] = shared[int(value.group(1))]
            elif kind and kind.group(1) == "inlineStr":
                cells[ref.group(1)] = html.unescape("".join(re.findall(r"<t[^>]*>(.*?)</t>", body, re.S)))
            elif value:
                cells[ref.group(1)] = html.unescape(value.group(1))
        rows.append(cells)
    return rows


def quote(value):
    """A T-SQL Unicode literal. Doubling the apostrophe is the whole escape."""
    return "N'" + (value or "").replace("'", "''") + "'"


def main():
    source = sys.argv[1] if len(sys.argv) > 1 else "db/AD Export 03_07_2026.xlsx"
    rows = read_sheet(source)

    header = rows[0]
    expected = ["Display Name", "Title", "Email Address", "Company Name"]
    actual = [header.get(c, "").strip() for c in "ABCD"]
    if actual != expected:
        sys.exit(f"Unexpected columns.\n  expected {expected}\n  found    {actual}")

    people = []
    for row in rows[1:]:
        name = row.get("A", "").strip()
        if not name:
            continue                      # the only required column
        people.append((name,
                       row.get("B", "").strip(),
                       row.get("C", "").strip(),
                       row.get("D", "").strip()))

    companies = Counter(p[3] for p in people)
    unmapped = {c: n for c, n in companies.items() if c and c not in COMPANY_TO_ENTITY}

    out = []
    w = out.append

    w("/* " + "=" * 74)
    w("   VMS - the people a visitor can come to see")
    w("   Server UATWEB01, database VMS.")
    w("")
    w(f"   GENERATED from {source} by db/tools/generate_seed_people.py.")
    w("   Do not edit by hand: run the generator against the new export instead.")
    w("")
    w(f"   {len(people)} people across {len(companies)} companies.")
    w("")
    w("   Requires 003_add_people.sql. Re-runnable, and worth re-running: it inserts")
    w("   people who are new, refreshes the title and company of people already there,")
    w("   and re-resolves every DiEntityId - so running it again after adding entities")
    w("   links the people who could not be matched the first time.")
    w("")
    w("   Run it with either of:")
    w("       sqlcmd -S UATWEB01 -E -i db\\004_seed_people.sql")
    w("       - or open it in SSMS against VMS and execute.")
    w("   " + "=" * 74 + " */")
    w("")
    w("USE VMS;")
    w("GO")
    w("")
    w("SET NOCOUNT ON;")
    w("")
    w("DECLARE @people TABLE")
    w("(")
    w("    DisplayName NVARCHAR(200) NOT NULL,")
    w("    Title       NVARCHAR(200) NULL,")
    w("    Email       NVARCHAR(256) NULL,")
    w("    CompanyName NVARCHAR(200) NULL")
    w(");")
    w("")
    w("INSERT INTO @people (DisplayName, Title, Email, CompanyName) VALUES")

    for i, (name, title, email, company) in enumerate(people):
        comma = "," if i < len(people) - 1 else ";"
        w(f"({quote(name)}, {quote(title)}, {quote(email)}, {quote(company)}){comma}")

    w("")
    w("/* The export's company names mapped onto vms.Entity, where the two spell the")
    w("   same company differently. Editable: add a row here and re-run.")
    w("")
    w("   A company with no row here still links if an entity of exactly that name")
    w("   exists - the join falls back to the company name - so adding")
    w("   'Emirates Building System' to vms.Entity and re-running is enough, with no")
    w("   edit to this map.")
    w("")
    w("   Companies in the export with no entity of that name at the time this was")
    w("   generated, largest first:")
    for company, count in sorted(unmapped.items(), key=lambda kv: -kv[1]):
        w(f"     {count:5d}  {company}")
    w(f"   That is {sum(unmapped.values())} of {len(people)} people. They are kept, with a null")
    w("   DiEntityId, and found under \"Search all entities\" rather than dropped. */")
    w("")
    w("DECLARE @map TABLE (CompanyName NVARCHAR(200), EntityName NVARCHAR(200));")
    w("")
    w("INSERT INTO @map (CompanyName, EntityName) VALUES")
    pairs = sorted(COMPANY_TO_ENTITY.items())
    for i, (company, entity) in enumerate(pairs):
        comma = "," if i < len(pairs) - 1 else ";"
        w(f"({quote(company)}, {quote(entity)}){comma}")

    w("")
    w("/* ---- 1. people who are new. Matched on email where there is one, because two")
    w("        people in this export share a display name and neither shares an email. */")
    w("")
    w("INSERT INTO vms.Person (DisplayName, Title, Email, CompanyName)")
    w("SELECT p.DisplayName, NULLIF(p.Title, N''), NULLIF(p.Email, N''), NULLIF(p.CompanyName, N'')")
    w("FROM @people p")
    w("WHERE NOT EXISTS (")
    w("    SELECT 1 FROM vms.Person e")
    w("    WHERE (p.Email <> N'' AND e.Email = p.Email)")
    w("       OR (p.Email = N'' AND e.Email IS NULL AND e.DisplayName = p.DisplayName));")
    w("")
    w("PRINT CONCAT(N'People inserted: ', @@ROWCOUNT);")
    w("GO")
    w("")
    w("/* ---- 2. titles and companies for people already there, since a job changes")
    w("        more often than a name does. */")
    w("")
    w("UPDATE e")
    w("   SET e.Title       = NULLIF(p.Title, N''),")
    w("       e.CompanyName = NULLIF(p.CompanyName, N'')")
    w("FROM vms.Person e")
    w("JOIN @people p ON p.Email <> N'' AND e.Email = p.Email")
    w("WHERE ISNULL(e.Title, N'') <> ISNULL(NULLIF(p.Title, N''), N'')")
    w("   OR ISNULL(e.CompanyName, N'') <> ISNULL(NULLIF(p.CompanyName, N''), N'');")
    w("")
    w("PRINT CONCAT(N'People updated: ', @@ROWCOUNT);")
    w("")
    w("/* ---- 3. the entity link, re-resolved for everyone. */")
    w("")
    w("UPDATE p")
    w("   SET p.DiEntityId = x.Id")
    w("FROM vms.Person p")
    w("LEFT JOIN @map m ON m.CompanyName = p.CompanyName")
    w("LEFT JOIN vms.Entity x ON x.Name = COALESCE(m.EntityName, p.CompanyName)")
    w("WHERE ISNULL(p.DiEntityId, -1) <> ISNULL(x.Id, -1);")
    w("")
    w("PRINT CONCAT(N'Entity links changed: ', @@ROWCOUNT);")
    w("")
    w("SELECT ISNULL(x.Name, N'(no entity)') AS Entity, p.CompanyName, COUNT(*) AS People")
    w("FROM vms.Person p")
    w("LEFT JOIN vms.Entity x ON x.Id = p.DiEntityId")
    w("GROUP BY x.Name, p.CompanyName")
    w("ORDER BY CASE WHEN x.Name IS NULL THEN 1 ELSE 0 END, People DESC;")
    w("GO")

    with open("db/004_seed_people.sql", "w", encoding="utf-8", newline="\n") as f:
        f.write("\n".join(out) + "\n")

    print(f"db/004_seed_people.sql: {len(people)} people, "
          f"{sum(unmapped.values())} without an entity")


if __name__ == "__main__":
    main()
