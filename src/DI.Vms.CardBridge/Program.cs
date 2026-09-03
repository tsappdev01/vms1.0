using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Xml;
using System.Threading;
using System.Web.Script.Serialization;
using AE.EmiratesId.IdCard;

namespace DI.Vms.CardBridge
{
    /// <summary>
    /// A local bridge between the browser reception client and the Emirates ID reader.
    ///
    /// The portal cannot call IDCardToolkit.dll directly - it is a native-backed .NET
    /// Framework assembly. This process runs on the reception machine, owns the toolkit,
    /// and exposes two endpoints on loopback that the portal calls.
    ///
    /// It binds 127.0.0.1 only. The card reader must never be reachable from the network.
    /// </summary>
    internal static class Program
    {
        private const string Prefix = "http://127.0.0.1:9100/";

        private static Toolkit _toolkit;
        private static readonly object Gate = new object();
        private static readonly JavaScriptSerializer Json = new JavaScriptSerializer { MaxJsonLength = 32 * 1024 * 1024 };

        /// <summary>Origins allowed to call the bridge. The portal in dev, and its deployed host.</summary>
        private static readonly string[] AllowedOrigins =
        {
            "http://localhost:5173",
            "http://127.0.0.1:5173",
            "http://localhost:4173",
        };

        private static int Main(string[] args)
        {
            var configDirectory = args.Length > 0 ? args[0] : FindConfigDirectory();

            if (configDirectory == null)
            {
                Console.Error.WriteLine("Could not find a toolkit config directory (one containing config_li).");
                Console.Error.WriteLine("Pass it as the first argument:");
                Console.Error.WriteLine(@"  DI.Vms.CardBridge.exe C:\path\to\IDCARDOFFLINE_config_2026-04-14\...");
                return 1;
            }

            Console.WriteLine("DI VMS card bridge");
            Console.WriteLine("Config directory : " + configDirectory);

            try
            {
                Initialise(configDirectory);
                Console.WriteLine("Toolkit          : " + _toolkit.GetToolkitVersion());
                Console.WriteLine("Licence expires  : " + _toolkit.GetLicenseExpiryDate());
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Toolkit initialisation failed: " + ex.Message);
                Console.Error.WriteLine("The bridge will still start, and /reader/status will report the problem.");
            }

            using (var listener = new HttpListener())
            {
                listener.Prefixes.Add(Prefix);

                try
                {
                    listener.Start();
                }
                catch (HttpListenerException ex)
                {
                    Console.Error.WriteLine("Could not listen on " + Prefix + ": " + ex.Message);
                    Console.Error.WriteLine("If this is an access error, either run as Administrator once, or grant the URL:");
                    Console.Error.WriteLine("  netsh http add urlacl url=" + Prefix + " user=" + Environment.UserName);
                    return 1;
                }

                Console.WriteLine("Listening        : " + Prefix);
                Console.WriteLine("Press Ctrl+C to stop.");
                Console.WriteLine();

                while (listener.IsListening)
                {
                    HttpListenerContext context;
                    try { context = listener.GetContext(); }
                    catch (HttpListenerException) { break; }

                    ThreadPool.QueueUserWorkItem(_ => Handle(context, configDirectory));
                }
            }

            return 0;
        }

        private static void Initialise(string configDirectory)
        {
            lock (Gate)
            {
                if (_toolkit != null) return;

                var logDirectory = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                    "EIDAToolkit", "logs");
                Directory.CreateDirectory(logDirectory);

                // Same shape the vendor sample passes, which is the configuration proven to work.
                var config =
                    "{\"config_directory\":\"" + configDirectory.Replace("\\", "\\\\") + "\"," +
                    "\"log_directory\":\"" + logDirectory.Replace("\\", "\\\\") + "\"," +
                    "\"application_type\":\"APP_INPROC\"," +
                    "\"read_publicdata_offline\":true}";

                _toolkit = new Toolkit(true, config);
            }
        }

        private static void Handle(HttpListenerContext context, string configDirectory)
        {
            var request = context.Request;
            var response = context.Response;

            var origin = request.Headers["Origin"];
            if (origin != null && Array.IndexOf(AllowedOrigins, origin) >= 0)
            {
                response.AddHeader("Access-Control-Allow-Origin", origin);
                response.AddHeader("Access-Control-Allow-Headers", "Content-Type");
                response.AddHeader("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
            }

            if (request.HttpMethod == "OPTIONS")
            {
                response.StatusCode = 204;
                response.Close();
                return;
            }

            try
            {
                var path = request.Url.AbsolutePath.TrimEnd('/');

                if (path == "/reader/status")
                {
                    Write(response, 200, Status(configDirectory));
                }
                else if (path == "/reader/read" && request.HttpMethod == "POST")
                {
                    Write(response, 200, Read(request));
                }
                else
                {
                    Write(response, 404, new Dictionary<string, object> { { "error", "Not found." } });
                }
            }
            catch (ToolkitException ex)
            {
                // The toolkit's own status text is more useful than a generic message.
                Write(response, 502, new Dictionary<string, object>
                {
                    { "error", ex.Message },
                    { "code", ex.Code },
                });
            }
            catch (Exception ex)
            {
                Write(response, 500, new Dictionary<string, object> { { "error", ex.Message } });
            }
        }

        private static Dictionary<string, object> Status(string configDirectory)
        {
            var result = new Dictionary<string, object>
            {
                { "configDirectory", configDirectory },
                { "toolkitInitialised", _toolkit != null },
            };

            if (_toolkit == null)
            {
                result["available"] = false;
                result["detail"] = "The toolkit is not initialised. Check the config directory and the licence.";
                return result;
            }

            result["toolkitVersion"] = _toolkit.GetToolkitVersion();

            try { result["licenseExpiry"] = _toolkit.GetLicenseExpiryDate(); }
            catch (Exception ex) { result["licenseExpiry"] = "unavailable: " + ex.Message; }

            try
            {
                var reader = _toolkit.GetReaderWithEmiratesId();
                result["available"] = true;
                result["readerName"] = reader.Name;
                result["detail"] = "Reader ready with a card inserted.";
            }
            catch (Exception ex)
            {
                result["available"] = false;
                result["readerName"] = null;
                result["detail"] = ex.Message;
            }

            return result;
        }

        private static Dictionary<string, object> Read(HttpListenerRequest request)
        {
            if (_toolkit == null)
            {
                throw new InvalidOperationException("The toolkit is not initialised.");
            }

            var body = new Dictionary<string, object>();
            using (var reader = new StreamReader(request.InputStream, request.ContentEncoding ?? Encoding.UTF8))
            {
                var raw = reader.ReadToEnd();
                if (!string.IsNullOrWhiteSpace(raw))
                {
                    body = Json.Deserialize<Dictionary<string, object>>(raw) ?? body;
                }
            }

            var wantPhoto = !body.ContainsKey("photo") || Convert.ToBoolean(body["photo"]);

            // The holder's signature and address are deliberately not read. BRD 3 says
            // capture only what visitor management needs, and the acknowledgement
            // signature is drawn fresh at check-in.
            var requestId = GenerateRequestId();

            var cardReader = _toolkit.GetReaderWithEmiratesId();
            cardReader.Connect();

            try
            {
                var data = cardReader.ReadPublicData(requestId, true, false, wantPhoto, false, false);

                // The response must echo our request id and carry a valid signature.
                // Without this an identity read is only as trustworthy as the channel
                // it arrived on, which for a security system is not good enough.
                var signatureWarning = ValidateResponse(requestId, data.XmlString);

                var nonModifiable = data.NonModifiablePublicData;
                var result = new Dictionary<string, object>
                {
                    { "idNumber", data.IdNumber },
                    { "cardNumber", data.CardNumber },
                    { "name", nonModifiable == null ? null : nonModifiable.FullNameEnglish },
                    { "nameArabic", nonModifiable == null ? null : nonModifiable.FullNameArabic },
                    { "nationality", nonModifiable == null ? null : nonModifiable.NationalityEnglish },
                    { "nationalityCode", nonModifiable == null ? null : nonModifiable.NationalityCode },
                    { "dateOfBirth", nonModifiable == null ? null : nonModifiable.DateOfBirth },
                    { "expiryDate", nonModifiable == null ? null : nonModifiable.ExpiryDate },
                    { "issueDate", nonModifiable == null ? null : nonModifiable.IssueDate },
                    { "gender", nonModifiable == null ? null : nonModifiable.Gender },
                    { "idType", nonModifiable == null ? null : nonModifiable.IdType },
                    { "photoBase64", null },
                    { "signatureWarning", signatureWarning },
                };

                if (wantPhoto && data.CardHolderPhoto != null)
                {
                    var bytes = data.CardHolderPhoto.ToArray();
                    if (bytes.Length > 0)
                    {
                        result["photoBase64"] = Convert.ToBase64String(bytes);
                    }
                }

                /* Card verification. These reach the Validation Gateway, so they can fail
                   independently of the read; a gateway problem must not lose an otherwise
                   good read, so each is reported rather than thrown. */
                var verification = new Dictionary<string, object>
                {
                    { "isGenuine", null },
                    { "cardStatus", "Unknown" },
                    { "vgAvailable", false },
                    { "verifiedAtUtc", DateTime.UtcNow.ToString("o") },
                };

                try
                {
                    var genuineRequestId = GenerateRequestId();
                    var genuine = cardReader.IsCardGenuine(genuineRequestId);
                    ValidateResponse(genuineRequestId, genuine == null ? null : genuine.XmlString);

                    var verdict = StatusOf(genuine);
                    verification["isGenuine"] = verdict != null &&
                        verdict.IndexOf("genuine", StringComparison.OrdinalIgnoreCase) >= 0;
                    verification["genuineStatus"] = verdict;
                    verification["vgAvailable"] = true;
                }
                catch (ToolkitException ex)
                {
                    verification["verificationError"] = ex.Message;
                    verification["verificationVgResponse"] = ex.VGResponse;
                }
                catch (Exception ex)
                {
                    verification["verificationError"] = ex.Message;
                }

                try
                {
                    var statusRequestId = GenerateRequestId();
                    var status = cardReader.CheckCardStatus(statusRequestId);
                    ValidateResponse(statusRequestId, status == null ? null : status.XmlString);

                    verification["cardStatus"] = StatusOf(status) ?? "Unknown";
                    verification["vgAvailable"] = true;
                }
                catch (ToolkitException ex)
                {
                    verification["statusError"] = ex.Message;
                    verification["statusVgResponse"] = ex.VGResponse;
                }
                catch (Exception ex)
                {
                    verification["statusError"] = ex.Message;
                }

                result["verification"] = verification;
                return result;
            }
            finally
            {
                try { cardReader.Disconnect(); } catch { /* the card may already be out */ }
            }
        }

        /// <summary>The card status string, falling back to the numeric response status.</summary>
        private static string StatusOf(ToolkitResponse response)
        {
            if (response == null) return null;
            return !string.IsNullOrEmpty(response.Status)
                ? response.Status
                : response.ResponseStatus.ToString();
        }

        /// <summary>40 cryptographically random bytes, as the vendor sample uses.</summary>
        private static string GenerateRequestId()
        {
            var bytes = new byte[40];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(bytes);
            }
            return Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// Checks the toolkit's XML response against the request that produced it.
        ///
        /// A mismatched request id means the response does not belong to this request, so
        /// it is rejected outright. A failed signature is returned as a warning rather
        /// than thrown, matching the vendor sample: CheckSignature() validates against the
        /// key carried inside the response, so on its own it proves internal consistency
        /// rather than provenance. Pinning it to the licence's server certificate would be
        /// the stronger control, and is not possible until ICP supply ServerTLSCert.
        /// </summary>
        private static string ValidateResponse(string requestId, string xml)
        {
            if (string.IsNullOrEmpty(xml)) return null;

            if (!RequestIdMatches(requestId, xml))
            {
                throw new InvalidOperationException(
                    "Request ID verification failed - the response may have been tampered with.");
            }

            return VerifySignature(xml) ? null : "Response signature verification failed.";
        }

        private static bool RequestIdMatches(string requestId, string xml)
        {
            try
            {
                // DTD processing off and no resolver: this XML is parsed before it is
                // trusted, so it must not be able to fetch anything.
                var settings = new XmlReaderSettings
                {
                    DtdProcessing = DtdProcessing.Prohibit,
                    XmlResolver = null,
                };

                using (var reader = XmlReader.Create(new StringReader(xml), settings))
                {
                    if (!reader.ReadToFollowing("RequestID")) return false;
                    return string.Equals(requestId, reader.ReadElementContentAsString(), StringComparison.Ordinal);
                }
            }
            catch
            {
                return false;
            }
        }

        private static bool VerifySignature(string xml)
        {
            try
            {
                var document = new XmlDocument { XmlResolver = null, PreserveWhitespace = false };
                document.LoadXml(xml);

                var nodes = document.GetElementsByTagName("Signature", "http://www.w3.org/2000/09/xmldsig#");
                if (nodes.Count == 0) return false;

                var signedXml = new SignedXml(document);
                signedXml.LoadXml((XmlElement)nodes[0]);
                return signedXml.CheckSignature();
            }
            catch
            {
                return false;
            }
        }

        private static void Write(HttpListenerResponse response, int status, object payload)
        {
            var bytes = Encoding.UTF8.GetBytes(Json.Serialize(payload));
            response.StatusCode = status;
            response.ContentType = "application/json";
            response.ContentLength64 = bytes.Length;
            response.OutputStream.Write(bytes, 0, bytes.Length);
            response.Close();
        }

        /// <summary>Locates a config directory by finding config_li, preferring a complete bundle.</summary>
        private static string FindConfigDirectory()
        {
            var here = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);

            for (var dir = here; dir != null; dir = dir.Parent)
            {
                var found = SearchFor(dir, 0);
                if (found != null) return found;
            }

            return null;
        }

        private static string SearchFor(DirectoryInfo root, int depth)
        {
            if (depth > 4) return null;

            try
            {
                foreach (var candidate in root.GetDirectories("IDCARDOFFLINE*"))
                {
                    foreach (var inner in new[] { candidate }.Concat(candidate.GetDirectories()))
                    {
                        if (File.Exists(Path.Combine(inner.FullName, "config_li")) &&
                            File.Exists(Path.Combine(inner.FullName, "config_ag")))
                        {
                            return inner.FullName;
                        }
                    }
                }
            }
            catch (UnauthorizedAccessException) { }

            return null;
        }
    }
}
