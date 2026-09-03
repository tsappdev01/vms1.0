# DI.Vms.CardBridge

A small process that runs on the reception machine, owns the ID Card Toolkit, and
exposes the reader to the browser portal on loopback.

## Why this exists

`IDCardToolkit.dll` is a **.NET Framework 4.7.2, x64** assembly that P/Invokes the native
`EIDAToolkit.dll`. A browser cannot call it. There were two ways to bridge that gap:

1. ICP's JavaScript agent over WebSocket (`ICAToolkitService.msi` + `eidatoolkit.js`)
2. This bridge, wrapping the same .NET API the vendor sample uses

The second was chosen because it reuses **exactly the path already proven to read a card
on this hardware**. The JavaScript agent's request and response envelopes are still
unconfirmed, and guessing them would mean debugging two integrations at once.

## Build and run

Windows only, and deliberately **not** part of `DI.Vms.sln` — a net472 project would fail
the Linux CI build.

```powershell
dotnet build src\DI.Vms.CardBridge\DI.Vms.CardBridge.csproj -c Release
.\src\DI.Vms.CardBridge\bin\Release\net472\DI.Vms.CardBridge.exe
```

It finds the toolkit config directory by looking for a folder containing both `config_li`
and `config_ag`. Pass an explicit path to override:

```powershell
DI.Vms.CardBridge.exe "C:\Claude.AI\vms1.0\IDCARDOFFLINE_config_2026-04-14\IDCARDOFFLINE_ag_config_2026-04-14"
```

If `HttpListener` refuses to bind, grant the URL once:

```powershell
netsh http add urlacl url=http://127.0.0.1:9100/ user=%USERNAME%
```

## Endpoints

Bound to **127.0.0.1 only** — the card reader must never be reachable from the network.
CORS is restricted to the portal's origins.

### `GET /reader/status`

```jsonc
{
  "available": true,
  "readerName": "ACS ACR39U ICC Reader 0",
  "detail": "Reader ready with a card inserted.",
  "toolkitVersion": "3.1.6",
  "licenseExpiry": "2027-07-29+04:00",
  "configDirectory": "C:\\...\\IDCARDOFFLINE_ag_config_2026-04-14"
}
```

### `POST /reader/read`

```jsonc
{ "photo": true }
```

Returns the chip fields, plus the Validation Gateway verdicts:

```jsonc
{
  "idNumber": "784...", "cardNumber": "...",
  "name": "...", "nameArabic": "...",
  "nationality": "...", "nationalityCode": "...",
  "dateOfBirth": "01/01/1990", "expiryDate": "15/04/2028",
  "gender": "M", "idType": "...", "photoBase64": "...",
  "verification": { "isGenuine": true, "cardStatus": "Valid", "vgAvailable": true, "verifiedAtUtc": "..." }
}
```

The gateway calls are reported rather than thrown: a gateway problem must not discard an
otherwise good read. `vgAvailable: false` means the visit should be recorded
`VerificationPending`.

**Address and the holder's signature image are deliberately not read.** BRD §3 says
capture only what visitor management needs, and the acknowledgement signature is drawn
fresh at check-in.

## Connecting the portal

```
# src/DI.Vms.Web/.env
VITE_CARD_READER=bridge
```

Then reload the portal. **New Visitor → Read Emirates ID** now uses the real card.

## Not yet verified

This has never been compiled or run — it was written in an environment with no Windows,
no .NET and no reader. Field names were taken from the vendor sample
(`PublicDataUserControl.xaml.cs`) and the API surface from `IDCardToolkit.XML`, so they
are read from the SDK rather than guessed, but the first run should be treated as a spike.

Most likely first-run problems, in order: the native `EIDAToolkit.dll` and its plugins not
being on the path (run from, or copy the DLLs into, the output folder), the `HttpListener`
URL ACL, and date formats differing from the assumed `DD/MM/YYYY`.
