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
