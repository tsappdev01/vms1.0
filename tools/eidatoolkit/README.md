# Toolkit configuration

## What `config_ap` is

The .NET sample reads a file named **`config_ap` from its current working directory** and
passes the contents to the `Toolkit` constructor:

```csharp
string configFilePath = "config_ap";
if (File.Exists(configFilePath)) configParams = File.ReadAllText(configFilePath);
ToolkitObj = new Toolkit(processMode, configParams);
```

If the file is absent, `configParams` is **null**, the toolkit falls back to a default
config location, and initialisation fails with a message about whichever config file it
found there — typically `Invalid config (config_li) data`. That message names a symptom,
not the cause.

`quickstart\64` deliberately ships without config files, because they depend on the target
environment. Its README lists what a config directory must contain:

> `config_ap`, `config_ag`, `config_li` license file, plugin hashes, etc.

## What this repository has

| File | Present | Purpose |
|---|---|---|
| `config_ap` | **No** | Application config — the string passed to `Toolkit()` |
| `config_ag` | **No** | Agent config |
| `config_li` | Yes | Licence |
| `config_pg` | Yes | Plugin hashes |
| `config_vg_qa` | Yes | Validation Gateway — **QA** |
| `config_tk_qa` | Yes | Toolkit — **QA** |
| `config_lv_qa` | Yes | **QA** |

`config_ap` is plain text and can be written locally. `config_ag` and the non-QA variants
must come from ICP.

## Try this first

Copy one of the examples here to `config_ap` (no extension) in the folder you run the
sample from, and correct `config_directory` to the absolute path of the
`IDCARDOFFLINE_config_2026-07-29` folder that directly contains `config_li`:

```powershell
cd id-card-toolkit-windows-sdk-v3.1.6\quickstart\64
copy ..\..\..\tools\eidatoolkit\config_ap.example config_ap
notepad config_ap          # fix config_directory
mkdir C:\ProgramData\EIDAToolkit\logs -Force
.\EIDAToolkitModernApp.exe
```

Two formats are provided because the samples disagree: the quickstart README shows JSON,
while the Android sample builds newline-separated `key = value` pairs. Try
`config_ap.example` first; if initialisation still fails, try
`config_ap.keyvalue.example`.

Prefer `EIDAToolkitModernApp.exe` over `EIDAToolkitApp.exe` — the README calls the former
the current sample and the latter legacy.

## If it still fails

Then the bundle itself is incomplete, and this is a question for ICP rather than a
configuration problem to solve locally. Ask them for:

1. A **complete** config bundle including `config_ap` and `config_ag`.
2. **Production** variants of the `_qa` files, for go-live.
3. Confirmation that the **Service Provider licence** in `config_li` is valid, and for
   which environment and which registered devices.

Worth asking at the same time, since all three block go-live and have procurement lead
times: the SP licence expiry, the config certificate expiry, and whether the Validation
Gateway is reachable from the DIP network.
