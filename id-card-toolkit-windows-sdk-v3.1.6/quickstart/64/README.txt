EIDAToolkit Quickstart Folder
=============================

What this folder is
-------------------
This is a ready-to-run folder. Every native library the toolkit needs at
runtime is right here next to the executables, so you can smoke-test the
product without touching the installer, PATH, or the rest of the SDK
layout. The two architecture subfolders are independent:

    quickstart\64\   - 64-bit build (use on x64 Windows)
    quickstart\32\   - 32-bit build (use on x86 Windows, or when you need
                       to test a 32-bit host process on x64 Windows)

Contents
--------
    EIDAToolkit.dll            Core toolkit
    EIDAToolkitAgent.dll       Agent transport (in-process or via service)
    EIDAToolkitService.exe     Windows service (background agent)
    EIDAToolkitConsole.exe     Interactive C/CLI sample
    PCSCLib.dll                PCSC smartcard-reader plugin
    SagemMSO1350.dll           Sagem MSO 1350 fingerprint-reader plugin
    MORPHO_SDK.dll + friends   Morpho SDK runtime (MSO100, MSOSECU, ...)
    msvcp120.dll, msvcr120.dll VC 2013 redist (required by Morpho SDK)
    IDCardToolkit.dll          .NET wrapper (P/Invoke binding)
    IDCardToolkit.xml          .NET XML documentation
    EIDAToolkitApp.exe         Legacy .NET sample (WinForms)
    EIDAToolkitModernApp.exe   Modern .NET sample (WPF, MVVM)
    EIDAToolkit.jar            Java SDK (64-bit folder only)

Running the modern .NET sample
------------------------------
1. Plug in a smartcard reader and insert an Emirates ID card.
2. Open a cmd or PowerShell prompt in this folder:
       cd quickstart\64
       EIDAToolkitModernApp.exe
   The app picks up EIDAToolkit.dll and IDCardToolkit.dll from the
   current directory. No PATH setup, no installer, no registry keys.

Running the C console sample
----------------------------
    cd quickstart\64
    EIDAToolkitConsole.exe

The console is interactive and exposes most of the C API. Type 'help'
at the prompt for the command list.

Config directory is required
----------------------------
The toolkit needs a config directory (containing config_ap, config_ag,
config_li license file, plugin hashes, etc.) before any card operation will
succeed. This quickstart folder does NOT include config files - they
depend on the target environment (test vs production VG, licensed
plugins, TLS certificates).

Point the toolkit at a config directory via the JSON passed to
Initialize(), for example:

    {"config_directory": "C:\\ProgramData\\EIDAToolkit\\config"}

See the ICP EIDAToolkit developer documentation for the full config
directory layout and how to obtain a licensed copy from ICP.

Agent mode requires the service to be running
----------------------------------------------
If the client application is configured for agent mode (application_type
= APP_AGENT in config_ap), EIDAToolkitService.exe must be running in
the background BEFORE the client calls Initialize(). Two ways to start
it:

1. Install it as a Windows service (production):
       EIDAToolkitService.exe -install
       net start "Emirates ID Card Toolkit Service"
   To uninstall:
       net stop  "Emirates ID Card Toolkit Service"
       EIDAToolkitService.exe -remove

2. Run it in-console for quick testing (no install):
       EIDAToolkitService.exe
   The service runs until you Ctrl+C.

In-process mode (application_type = APP_INPROC) does NOT need the
service - the toolkit loads the agent DLL directly into the caller's
process. The modern .NET sample can be configured for either mode.

Architecture mixing is not supported
------------------------------------
Do not mix 64-bit and 32-bit binaries from the two folders. A 64-bit
client (EIDAToolkitModernApp.exe from quickstart\64) must use
EIDAToolkitService.exe from quickstart\64, and likewise for 32-bit.
