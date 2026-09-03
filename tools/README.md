# tools

## Check-CardReader.ps1

Checks that a machine can read an Emirates ID, in the order the ID Card Toolkit
depends on:

1. `SCardSvr` (Smart Card) and `WUDFsvc` (user-mode driver host) are running
2. A reader is present as a PnP device — and is *currently connected*, not a leftover record
3. The reader is visible through **PC/SC**, which is the same `SCardListReaders` call the
   toolkit makes via `Toolkit.ListReaders()`
4. A card is inserted and answering (ATR)
5. The ICP toolkit agent responds, if `ICAToolkitService.msi` has been installed

Read-only by default:

```powershell
.\tools\Check-CardReader.ps1
```

To start and enable the two services, in an **Administrator** session:

```powershell
.\tools\Check-CardReader.ps1 -Fix
```

If PowerShell blocks the script:

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\Check-CardReader.ps1
```

`SCardSvr` ships as trigger-start, so it can be stopped on a machine with no reader
attached. `-Fix` sets it to Automatic, which is what a reception machine needs — otherwise
the client fails on a fresh boot.


## Test-Vms.ps1

End-to-end smoke test of the API against the real database. Nothing is mocked.

```powershell
dotnet run --project src/DI.Vms.Api      # in one terminal
.\tools\Test-Vms.ps1                    # in another
```

Drives a complete visitor the way reception would - identify, register, check in,
appear in Currently Inside and on the evacuation list, be recognised on a second scan,
check out - and then verifies the controls:

- the plaintext ID number never appears in a list response, only the masked form
- a Security Officer is **refused** the unmasked ID (403), per BRD §22
- checking in twice, and checking out twice, are both rejected
- check-in and check-out both reach the audit trail
- all 11 reports are defined and one runs

Each step prints PASS or FAIL with the reason. Exit code 0 means everything passed, so
it can gate a deployment.

It leaves one checked-out test visitor in the database. Remove it before UAT sign-off if
the data needs to be clean.

Pass `-BaseUrl` if the API is not on `https://localhost:7001`. The self-signed
development certificate is handled on both PowerShell 5.1 and 7.
