SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

/* =============================================================================
   Home address captured from the Emirates ID chip.

   NOTE ON SCOPE. BRD section 3 recommends capturing only what visitor management
   needs, and an earlier revision therefore did not read the address. Capturing it
   is a deliberate business decision taken on 2026-09-03. It widens the personal
   data held per visitor, so it also widens what the retention job in section 22
   must erase - the columns below are covered by the same purge.

   Stored as discrete columns rather than a blob so the retention job can clear
   them, and so a report can show a readable address without parsing.

   Re-runnable.
   ============================================================================= */

IF COL_LENGTH(N'vms.Visitor', N'AddressEmirate') IS NULL
BEGIN
    ALTER TABLE vms.Visitor ADD
        AddressEmirate     NVARCHAR(100) NULL,
        AddressCity        NVARCHAR(100) NULL,
        AddressArea        NVARCHAR(150) NULL,
        AddressStreet      NVARCHAR(200) NULL,
        AddressBuilding    NVARCHAR(200) NULL,
        AddressFlat        NVARCHAR(50)  NULL,
        AddressPoBox       NVARCHAR(50)  NULL,
        AddressMobile      NVARCHAR(50)  NULL,
        AddressEmail       NVARCHAR(256) NULL;
END
GO
