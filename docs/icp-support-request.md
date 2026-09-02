# Support request to ICP — toolkit licence activation

Ready to send. Replace every `<…>` placeholder before sending. Attach the toolkit log
(`C:\ProgramData\EIDAToolkit\logs\EIDAToolkit_3.1.6_<date>.log`) — it contains no
cardholder data, only toolkit diagnostics.

---

**Subject:** ID Card Toolkit 3.1.6 — Validation Gateway returns 401 "License not found or not active" (PRE-PRODUCTION licence) — SP `<service provider name>`

Dear ICP Support,

We are integrating the Emirates ID Card Toolkit into a Visitor Management System for
Dubai Investments (DIP office). Card reading is fully functional up to the point of
licence validation, where the Validation Gateway rejects our licence. We need the licence
activated, or a licence issued that the gateway recognises.

**Service Provider details**

| | |
|---|---|
| Service Provider | `<registered SP name>` |
| SP / licence reference | `<licence or agreement reference>` |
| Config bundle supplied | `IDCARDOFFLINE_config_2026-07-29` |
| Technical contact | `<name, email, phone>` |
| Organisation | Dubai Investments — `<department>` |

**Environment**

| | |
|---|---|
| Toolkit | 3.1.6 (build `3.1.6.20260622105947`), Windows x64 |
| Binding | .NET (`IDCardToolkit.dll`), in-process mode |
| Sample used | `quickstart\64\EIDAToolkitApp.exe` (unmodified vendor sample) |
| OS | Windows 11 (10.0.26100), x64 |
| Reader | ACS ACR39U ICC Reader, via `PCSCLib.dll` |
| Card | UAE ID Card version 4, chip type 2, CONTACT interface |
| Licence type reported | **PRE-PRODUCTION** |
| Licence expiry reported | 2027-07-29+04:00 |

**What works**

Toolkit initialises; the reader is detected through the PCSC plugin; the card connects
over the contact interface and is identified as UAE ID Card version 4; `GetCSN` returns
the card serial successfully. `GetToolkitVersion`, `GetLicenseExpiryDate` and
`GetConfigCertificateExpiryDate` all succeed.

**What fails**

`ReadPublicData` fails. From the toolkit log:

```
I [67:874]  License type PRE-PRODUCTION
I [17:1694] Invoking get deviceID API
E [14:1310] The license provided is not authorized for this service.
E [17:1713] Failed to get device information [Error#14:1310:23, "Functionality not supported"]
I [33:1053] Invoking ReadPublicData service
I [7:517]   Received response from Validation Gateway (VG) with status : ["HTTP/1.1 401 "]
W [telemetry] POST rejected: HTTP 401 (enforced), response:
    {"code":401,"message":"License not found or not active","status":"error",
     "timestamp":"2026-09-02T05:53:12.313008631Z"}
E [68:1541] Failed to retrieve HTTP header from received response
E [33:1150] Failed to read public data [Error#68:1541:233, "Failed to get response from server"]
```

`GetDeviceId` fails with *"The license provided is not authorized for this service"*, so we
are also unable to proceed with device registration.

**Requests**

1. **Activate the Service Provider licence** in `config_li`, or issue a licence the
   Validation Gateway recognises. The gateway currently reports
   `401 License not found or not active`, and the licence reports itself as
   PRE-PRODUCTION. Please confirm whether this licence was ever activated for
   Validation Gateway access.

2. **Confirm the environment binding.** Our bundle contains `config_vg_qa`,
   `config_tk_qa` and `config_lv_qa` alongside `config_li` and `config_pg`. Please
   confirm these are the correct matching set for the licence, and which gateway
   environment they target.

3. **Supply the missing configuration files.** The toolkit log reports these as absent:

   | File | Log entry |
   |---|---|
   | `config_pd` | `Failed to load config_pd data [Error#1:135:12, "File could not be opened"]` |
   | `config_sb` | `config_sb file not loaded` |
   | `config_ap` | Not supplied (we authored one locally to set `config_directory`) |
   | `config_ag` | Not supplied |

   Please confirm which of these are required for our use case, and supply them.

4. **Supply the Server TLS certificate and chain.** The licence is missing these nodes:

   ```
   Failed to retrieve XML node ["LicenseType\LicenseDetail\ServerTLSCert"]
   Failed to retrieve XML node ["LicenseType\LicenseDetail\ServerTLSCertChain"]
   ```

   `Get Config Cert Expiry Date` accordingly returns empty values for Server TLS Cert and
   Config AG Cert.

5. **Confirm offline-read behaviour.** We set `read_publicdata_offline = true` and the
   toolkit acknowledges it (`Read public data offline [true]`), yet `ReadPublicData` still
   POSTs to the Validation Gateway and the log marks that POST `(enforced)`. **Does an
   activated licence permit `ReadPublicData` to complete with no gateway call?**

   This matters operationally: our reception desk must keep registering visitors during a
   network outage. If a gateway call is unavoidable on every read, please confirm so, so
   that we design an appropriate fallback.

6. **Device registration procedure.** Once the licence is active, please confirm the
   credentials and process for `RegisterDevice` (the encoded user ID and password), and
   whether each reception workstation must be registered individually.

7. **Production path and lead time.** Please advise what is required to obtain the
   production licence and production configuration bundle, and the expected lead time, so
   we can plan go-live.

**Network**

Please confirm the hostnames and ports the toolkit and Validation Gateway require for
outbound access, so we can raise the necessary firewall requests. We have observed
`toolkitagent.emiratesid.ae` on ports 9004, 9005 and 9020 in the SDK's web samples, and a
local agent health endpoint on `127.0.0.1:9006`.

Attached: toolkit log for the failing session.

Kind regards,
`<name>`
`<title>`, `<company>`
`<email>` · `<phone>`

---

## Notes for whoever sends this

- **The licence expiry is not the problem.** 2027-07-29 is well in the future; a licence
  can be unexpired and still inactive, which is what the gateway is reporting.
- **Item 5 is the one with design consequences.** Everything else is paperwork with a lead
  time. If a gateway call cannot be avoided, reception has a hard network dependency and
  needs a documented manual fallback.
- **Item 3 lists `config_ap` as not supplied.** We wrote one ourselves to set
  `config_directory`; it is not a defect on ICP's side, but worth confirming that
  authoring it locally is the expected practice.
- Attach the log, not screenshots. It names the gateway response verbatim, which is what
  their support will act on.
- The log contains no cardholder data — only toolkit diagnostics, the card serial number
  and the reader name.
