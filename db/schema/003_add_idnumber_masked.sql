/* =============================================================================
   Store the masked ID number alongside the ciphertext.

   Without it, rendering any visitor list means decrypting every row just to mask
   it again for display. Storing the masked form at registration keeps the common
   read path free of plaintext entirely: only the permission-gated, audited unmask
   endpoint ever decrypts.

   Re-runnable.

   NOTE ON SET OPTIONS
   vms.Visitor carries filtered indexes (IX_Visitor_Company, IX_Visitor_IdExpiry).
   SQL Server refuses INSERT/UPDATE/DELETE against such a table unless
   QUOTED_IDENTIFIER is ON, and sqlcmd connects with it OFF by default - which is
   why the UPDATE below fails with Msg 1934 without this. SET options are scoped to
   the batch, so each batch containing DML sets them explicitly. This makes the
   script correct however it is invoked, rather than depending on a sqlcmd -I flag
   the next person will forget.
   ============================================================================= */

SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

IF COL_LENGTH(N'vms.Visitor', N'IdNumberMasked') IS NULL
BEGIN
    ALTER TABLE vms.Visitor ADD IdNumberMasked NVARCHAR(30) NULL;
END
GO

SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

/* Backfill any pre-existing rows with a safe placeholder; the real masked value
   cannot be derived without decrypting, which this script deliberately does not do. */
UPDATE vms.Visitor SET IdNumberMasked = N'(not recorded)' WHERE IdNumberMasked IS NULL;
GO

SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

IF EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'vms.Visitor')
      AND name = N'IdNumberMasked'
      AND is_nullable = 1
)
BEGIN
    ALTER TABLE vms.Visitor ALTER COLUMN IdNumberMasked NVARCHAR(30) NOT NULL;
END
GO
