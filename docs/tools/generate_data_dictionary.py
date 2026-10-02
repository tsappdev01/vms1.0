#!/usr/bin/env python3
"""
Writes docs/controlled/03-data-dictionary.md from the EF model.

DI-IT-POL-AIDEV-001 §6 requires a Database Schema / Data Dictionary, and §9 is explicit
about why it must be generated: "Hand-written documentation drifts from reality within
months, which is why it is rarely trusted during a handover."

A hand-written dictionary here would drift the first time somebody adds a column, and it
would drift silently, because nothing would disagree with it. This reads the three files
that actually define the schema - there is no fourth, and no hand-written DDL for these
tables, because Data/DbBootstrapper.cs creates them from this same model at startup:

    Data/Entities.cs      the classes, their properties and the comments explaining them
    Data/VmsDbContext.cs  table names, column widths, nullability, indexes, foreign keys
    Data/FieldLengths.cs  the widths themselves, named rather than repeated

Run it after any change to those files:

    python3 docs/tools/generate_data_dictionary.py

It fails loudly rather than writing a half-right document. A class with no ToTable, a
HasMaxLength naming a property that does not exist, a FieldLengths constant that is not
defined - each stops it with the reason, because a data dictionary that is quietly
incomplete is worse than one that is missing: somebody trusts it.
"""

from __future__ import annotations

import re
import sys
from dataclasses import dataclass, field
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
DATA = ROOT / "src" / "DI.Vms.Blazor" / "Data"
OUT = ROOT / "docs" / "controlled" / "03-data-dictionary.md"

ENTITIES = DATA / "Entities.cs"
CONTEXT = DATA / "VmsDbContext.cs"
LENGTHS = DATA / "FieldLengths.cs"


def fail(why: str) -> None:
    sys.exit(f"generate_data_dictionary.py: {why}")


# --------------------------------------------------------------------------- source model


@dataclass
class Column:
    name: str
    clr: str
    """The C# type as written, e.g. "string", "int?", "byte[]"."""

    required_keyword: bool
    """`required` on the property - a value must be supplied to construct the object."""

    nullable_clr: bool
    """A trailing ? on the type."""

    summary: str = ""
    """The first sentence of the /// <summary>, which is the column's description."""

    max_length: int | None = None
    is_required: bool = False
    """IsRequired() in the model configuration, which is what makes the column NOT NULL."""

    default: str | None = None
    navigation: bool = False


@dataclass
class Table:
    cls: str
    name: str | None = None
    summary: str = ""
    columns: list[Column] = field(default_factory=list)
    key: str | None = None
    indexes: list[str] = field(default_factory=list)
    foreign_keys: list[str] = field(default_factory=list)

    def column(self, name: str) -> Column | None:
        return next((c for c in self.columns if c.name == name), None)


# --------------------------------------------------------------------------- parsing


def read(path: Path) -> str:
    if not path.is_file():
        fail(f"{path.relative_to(ROOT)} is not there. Has the project moved?")
    return path.read_text(encoding="utf-8")


def field_lengths() -> dict[str, int]:
    """The named widths. `public const int Name = 300;`"""
    found = dict(
        (m.group(1), int(m.group(2)))
        for m in re.finditer(r"public\s+const\s+int\s+(\w+)\s*=\s*(\d+)\s*;", read(LENGTHS))
    )

    if not found:
        fail("no `public const int` found in FieldLengths.cs - has its shape changed?")

    return found


def first_sentence(doc: str) -> str:
    """
    One line out of an XML doc comment.

    The comments in Entities.cs are prose, often several paragraphs explaining why a column
    exists at all. A dictionary wants the first sentence; the file itself is where somebody
    goes for the argument.
    """
    # Each line is `    /// text`. The marker goes first; without that every summary comes
    # out with "/// ///" in front of it, which is how the first run of this looked.
    text = " ".join(
        re.sub(r"^\s*///\s?", "", line) for line in doc.splitlines()
    )
    text = re.sub(r"<see\s+cref=\"[A-Za-z.]*?(\w+)\"\s*/>", r"`\1`", text)
    text = re.sub(r"<c>(.*?)</c>", r"`\1`", text)
    text = re.sub(r"</?(summary|para|b|i)>", " ", text)
    text = re.sub(r"\s+", " ", text).strip()

    cut = re.search(r"(?<!\be\.g)(?<!\bi\.e)\.(\s|$)", text)
    if cut:
        text = text[: cut.start() + 1]

    return text.strip()


# A property: optional doc comment, then `public [required] <type> <Name> { get; set; }`.
PROPERTY = re.compile(
    r"(?P<doc>(?:^[ \t]*///.*\n)*)"
    r"^[ \t]*public\s+(?P<req>required\s+)?(?P<type>[\w.<>\[\]]+\??)\s+(?P<name>\w+)\s*"
    r"\{\s*get;\s*set;\s*\}",
    re.MULTILINE,
)

CLASS = re.compile(
    r"(?P<doc>(?:^[ \t]*///.*\n)*)^public\s+class\s+(?P<name>\w+)", re.MULTILINE
)


def entity_classes() -> dict[str, Table]:
    source = read(ENTITIES)
    starts = list(CLASS.finditer(source))

    if not starts:
        fail("no `public class` found in Entities.cs - has its shape changed?")

    tables: dict[str, Table] = {}

    for i, start in enumerate(starts):
        end = starts[i + 1].start() if i + 1 < len(starts) else len(source)
        body = source[start.end() : end]

        table = Table(cls=start.group("name"), summary=first_sentence(start.group("doc")))

        for prop in PROPERTY.finditer(body):
            clr = prop.group("type")

            # A reference to another entity, or a collection of them, is a navigation
            # rather than a column: it is how EF joins, and there is no such column in
            # SQL Server. The foreign key beside it is the column, and is listed.
            navigation = clr.rstrip("?") in {s.group("name") for s in starts}

            table.columns.append(
                Column(
                    name=prop.group("name"),
                    clr=clr,
                    required_keyword=bool(prop.group("req")),
                    nullable_clr=clr.endswith("?"),
                    summary=first_sentence(prop.group("doc")),
                    navigation=navigation,
                )
            )

        if not table.columns:
            fail(f"class {table.cls} in Entities.cs has no properties - check the parser.")

        tables[table.cls] = table

    return tables


def apply_configuration(tables: dict[str, Table], widths: dict[str, int]) -> None:
    """
    Reads OnModelCreating and writes what it says onto the columns.

    Each `b.Entity<X>(e => { ... });` block is taken whole and then read line by line, so a
    HasMaxLength in one entity cannot be attributed to another.
    """
    source = read(CONTEXT)

    blocks = list(re.finditer(r"b\.Entity<(\w+)>\(e\s*=>", source))
    if not blocks:
        fail("no `b.Entity<...>` blocks found in VmsDbContext.cs - has its shape changed?")

    for i, block in enumerate(blocks):
        cls = block.group(1)
        end = blocks[i + 1].start() if i + 1 < len(blocks) else len(source)
        body = source[block.end() : end]

        table = tables.get(cls)
        if table is None:
            fail(f"VmsDbContext configures {cls}, which is not a class in Entities.cs.")

        if m := re.search(r'e\.ToTable\("(\w+)"\)', body):
            table.name = m.group(1)

        if m := re.search(r"e\.HasKey\(x\s*=>\s*x\.(\w+)\)", body):
            table.key = m.group(1)

        def width(raw: str) -> int:
            if raw.isdigit():
                return int(raw)
            name = raw.split(".")[-1]
            if name not in widths:
                fail(f"FieldLengths.{name} is used by {cls} but is not defined.")
            return widths[name]

        def column(name: str) -> Column:
            found = table.column(name)
            if found is None:
                fail(f"VmsDbContext configures {cls}.{name}, which is not a property of it.")
            return found

        # e.Property(x => x.Name).HasMaxLength(200).IsRequired();
        for m in re.finditer(
            r"e\.Property\(x\s*=>\s*x\.(\w+)\)((?:\s*\.\w+\([^)]*\))*)\s*;", body
        ):
            col = column(m.group(1))
            calls = m.group(2)

            if w := re.search(r"HasMaxLength\(([\w.]+)\)", calls):
                col.max_length = width(w.group(1))
            if "IsRequired()" in calls:
                col.is_required = True
            if d := re.search(r"HasDefaultValue\(([^)]*)\)", calls):
                col.default = d.group(1).strip()

        # The block that gives every plain card field the same width:
        #   foreach (var name in new[] { nameof(X.A), ... }) { e.Property(name).Has... }
        for m in re.finditer(
            r"foreach\s*\(var\s+name\s+in\s+new\[\]\s*\{(?P<names>.*?)\}\s*\)\s*"
            r"\{\s*e\.Property\(name\)\.HasMaxLength\((?P<width>[\w.]+)\)",
            body,
            re.DOTALL,
        ):
            w = width(m.group("width"))
            names = re.findall(r"nameof\(\w+\.(\w+)\)", m.group("names"))
            if not names:
                fail(f"the foreach block in {cls} names no properties - check the parser.")
            for name in names:
                column(name).max_length = w

        for m in re.finditer(
            r"e\.HasIndex\((?P<on>x\s*=>\s*(?:x\.\w+|new\s*\{[^}]*\}))\)(?P<calls>(?:\s*\.\w+\([^)]*\))*)",
            body,
        ):
            on = re.findall(r"x\.(\w+)", m.group("on"))
            for name in on:
                column(name)

            calls = m.group("calls")
            parts = [", ".join(on)]
            if ".IsUnique()" in calls:
                parts.append("unique")
            if f := re.search(r'HasFilter\("([^"]*)"\)', calls):
                parts.append(f"filtered: `{f.group(1)}`")
            if n := re.search(r'HasDatabaseName\("([^"]*)"\)', calls):
                parts.append(f"named `{n.group(1)}`")

            table.indexes.append(" — ".join(parts))

        for m in re.finditer(
            r"e\.HasOne\(x\s*=>\s*x\.(?P<nav>\w+)\)(?P<calls>(?:\s*\.\w+(?:<[\w]+>)?\([^)]*\))*)",
            body,
        ):
            calls = m.group("calls")
            key = re.search(r"HasForeignKey(?:<\w+>)?\(x\s*=>\s*x\.(\w+)\)", calls)
            on_delete = re.search(r"OnDelete\(DeleteBehavior\.(\w+)\)", calls)

            if key is None:
                continue

            column(key.group(1))
            behaviour = on_delete.group(1) if on_delete else "default (NoAction here)"
            table.foreign_keys.append(f"`{key.group(1)}` → `{m.group('nav')}`, on delete {behaviour}")

    for table in tables.values():
        if table.name is None:
            fail(
                f"class {table.cls} has no ToTable in VmsDbContext. Either it is not a table, "
                "in which case take it out of Entities.cs, or the mapping is missing."
            )


# --------------------------------------------------------------------------- output


def sql_type(col: Column) -> str:
    """
    What SQL Server gets, which is what EF Core 8's SQL Server provider maps these to.

    Only the types this model actually uses are listed. An unrecognised one stops the run
    rather than being guessed at, because a data dictionary's whole job is the exact type.
    """
    bare = col.clr.rstrip("?")

    if bare == "string":
        return f"nvarchar({col.max_length})" if col.max_length else "nvarchar(max)"
    if bare == "int":
        return "int"
    if bare == "bool":
        return "bit"
    if bare == "byte[]":
        return "varbinary(max)"
    if bare == "DateTimeOffset":
        return "datetimeoffset"

    fail(f"no SQL type is known for `{col.clr}` ({col.name}). Add it to sql_type().")
    raise AssertionError  # unreachable; keeps type checkers quiet


def nullability(col: Column, table: Table) -> str:
    if col.name == "Id" or col.name == table.key:
        return "NOT NULL"
    if col.is_required or col.required_keyword:
        return "NOT NULL"
    return "NULL" if col.nullable_clr else "NOT NULL"


def document(tables: dict[str, Table]) -> str:
    out: list[str] = []
    w = out.append

    w("# Data Dictionary")
    w("")
    w("**Visitor Management System · Dubai Investments PJSC**  ")
    w("DI-IT-POL-AIDEV-001 §6 — Database Schema / Data Dictionary (Controlled, required)")
    w("")
    w("> Generated from the EF Core model by `docs/tools/generate_data_dictionary.py`.")
    w("> Do not edit this file. Change `src/DI.Vms.Blazor/Data/` and run the generator again.")
    w("")
    w("Database **VMS**, schema **vms**, on SQL Server.")
    w("")
    w("The tables are created from this model at startup by `Data/DbBootstrapper.cs`, so the")
    w("model is the only definition of the schema and this document cannot disagree with the")
    w("database unless somebody changed the database by hand. Columns that the bootstrapper")
    w("will not add on its own — a required column, a widened type, a rename, a drop — need a")
    w("script in `db/`, and startup refuses to run until one has been applied.")
    w("")

    w("## Tables")
    w("")
    w("| Table | Purpose |")
    w("|---|---|")
    for table in tables.values():
        w(f"| `vms.{table.name}` | {table.summary or '—'} |")
    w("")

    for table in tables.values():
        w(f"## `vms.{table.name}`")
        w("")
        if table.summary:
            w(table.summary)
            w("")

        key = table.key or "Id"
        w(f"Primary key: `{key}`"
          + (" (identity)" if key == "Id"
             else " — the owning row's key, so there is at most one of these per owner."))
        w("")

        w("| Column | Type | Null | Notes |")
        w("|---|---|---|---|")

        for col in table.columns:
            if col.navigation:
                continue

            notes = col.summary
            if col.default is not None:
                notes = (notes + " " if notes else "") + f"Database default `{col.default}`."

            w(
                f"| `{col.name}` | {sql_type(col)} | {nullability(col, table)} "
                f"| {notes or '—'} |"
            )
        w("")

        navigations = [c for c in table.columns if c.navigation]
        if navigations:
            w("Navigations (no column of their own): "
              + ", ".join(f"`{c.name}`" for c in navigations)
              + ".")
            w("")

        if table.foreign_keys:
            w("**Foreign keys**")
            w("")
            for fk in table.foreign_keys:
                w(f"- {fk}")
            w("")

        if table.indexes:
            w("**Indexes** (beyond the primary key)")
            w("")
            for index in table.indexes:
                w(f"- {index}")
            w("")

    w("## What is not in here")
    w("")
    w("- **The entity list itself.** `vms.Entity` is reference data maintained by script in")
    w("  `db/`, not seeded from code, so that the group's companies can change without a")
    w("  rebuild. Its rows are not part of the schema.")
    w("- **Audit tables.** There are none. Visits cannot be edited or deleted through the")
    w("  application, so the row is its own record; see `09-access-control-matrix.md`.")
    w("- **Retention.** Nothing deletes a visit. See `08-data-classification.md`.")
    w("")

    return "\n".join(out) + "\n"


def main() -> None:
    widths = field_lengths()
    tables = entity_classes()
    apply_configuration(tables, widths)

    OUT.parent.mkdir(parents=True, exist_ok=True)
    OUT.write_text(document(tables), encoding="utf-8")

    columns = sum(len([c for c in t.columns if not c.navigation]) for t in tables.values())
    print(f"{OUT.relative_to(ROOT)}: {len(tables)} tables, {columns} columns.")


if __name__ == "__main__":
    main()
