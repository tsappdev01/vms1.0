using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Xml;

using AE.EmiratesId.IdCard;
using AE.EmiratesId.IdCard.DataModels;

namespace EIDAToolkitModernApp.Services
{
    /// <summary>
    /// Service layer wrapping all IDCardToolkit operations
    /// </summary>
    public class ToolkitService
    {
        private Toolkit _toolkit;
        private CardReader _selectedReader;
        private CardReader[] _readers;
        private FingerData[] _fingerData;
        private bool _isInitialized;
        private bool _isConnected;

        public bool IsInitialized
        {
            get { return _isInitialized; }
        }

        public bool IsConnected
        {
            get { return _isConnected; }
        }

        public string ReaderName
        {
            get { return _selectedReader != null ? _selectedReader.Name : string.Empty; }
        }

        public FingerData[] FingerDataArray
        {
            get { return _fingerData; }
        }

        public string LastValidationWarning { get; private set; }

        /// <summary>
        /// Initialize the toolkit with optional config params
        /// </summary>
        public void Initialize(bool processMode)
        {
            string configParams = null;
            string configFilePath = "config_ap";

            if (File.Exists(configFilePath))
            {
                configParams = File.ReadAllText(configFilePath);
            }

            _toolkit = new Toolkit(processMode, configParams);
            _isInitialized = true;
        }

        /// <summary>
        /// Get the toolkit version string
        /// </summary>
        public string GetToolkitVersion()
        {
            EnsureInitialized();
            return _toolkit.GetToolkitVersion();
        }

        /// <summary>
        /// List available smart card readers
        /// </summary>
        public CardReader[] ListReaders()
        {
            EnsureInitialized();
            _readers = _toolkit.ListReaders();
            return _readers;
        }

        /// <summary>
        /// Get the first reader with an Emirates ID card inserted
        /// </summary>
        public CardReader GetReaderWithEmiratesId()
        {
            EnsureInitialized();
            _selectedReader = _toolkit.GetReaderWithEmiratesId();
            return _selectedReader;
        }

        /// <summary>
        /// Select a reader from the list by index
        /// </summary>
        public void SelectReader(int index)
        {
            if (_readers == null || index < 0 || index >= _readers.Length)
            {
                throw new InvalidOperationException("Invalid reader index");
            }

            // Don't replace the active connected reader
            if (_isConnected && _selectedReader != null)
            {
                return;
            }

            _selectedReader = _readers[index];
        }

        /// <summary>
        /// Connect to the selected reader
        /// </summary>
        public void Connect()
        {
            EnsureInitialized();
            EnsureReaderSelected();
            _selectedReader.Connect();
            _isConnected = true;
        }

        /// <summary>
        /// Disconnect from the current reader
        /// </summary>
        public void Disconnect()
        {
            EnsureInitialized();

            if (_isConnected && _selectedReader != null)
            {
                _selectedReader.Disconnect();
            }

            _isConnected = false;
        }

        /// <summary>
        /// Cleanup the toolkit and release all resources
        /// </summary>
        public void Cleanup()
        {
            if (_toolkit != null)
            {
                try { _toolkit.Cleanup(); } catch { }
            }

            _isInitialized = false;
            _isConnected = false;
            _selectedReader = null;
            _readers = null;
            _fingerData = null;
            _toolkit = null;
        }

        /// <summary>
        /// Get the card serial number
        /// </summary>
        public string GetCSN()
        {
            EnsureConnected();
            return _selectedReader.GetCSN();
        }

        /// <summary>
        /// Get the card interface type
        /// </summary>
        public string GetInterfaceType()
        {
            EnsureConnected();
            int interfaceType = _selectedReader.GetInterfaceType();
            return Convert.ToString((InterfaceType)interfaceType);
        }

        /// <summary>
        /// Read public data from the card
        /// </summary>
        public CardPublicData ReadPublicData(bool readNonModifiable, bool readModifiable,
            bool readPhoto, bool readSignature, bool readAddress)
        {
            EnsureConnected();
            string requestId = GenerateRequestId();

            CardPublicData data = _selectedReader.ReadPublicData(
                requestId, readNonModifiable, readModifiable,
                readPhoto, readSignature, readAddress);

            ValidateResponse(requestId, data.XmlString);

            return data;
        }

        /// <summary>
        /// Check card status
        /// </summary>
        public ToolkitResponse CheckCardStatus()
        {
            EnsureConnected();
            string requestId = GenerateRequestId();

            ToolkitResponse response = _selectedReader.CheckCardStatus(requestId);

            ValidateResponse(requestId, response.XmlString);

            return response;
        }

        /// <summary>
        /// Parse MRZ data string
        /// </summary>
        public MRZData ParseMRZData(string mrzString)
        {
            EnsureInitialized();
            return _toolkit.ParseMRZData(mrzString);
        }

        /// <summary>
        /// Get finger data from the card
        /// </summary>
        public FingerData[] GetFingerData()
        {
            EnsureConnected();
            _fingerData = _selectedReader.GetFingerData();
            return _fingerData;
        }

        /// <summary>
        /// Authenticate biometric on server
        /// </summary>
        public ToolkitResponse AuthenticateBiometricOnServer(FingerIndex fingerIndex, int sensorTimeout)
        {
            EnsureConnected();
            string requestId = GenerateRequestId();

            ToolkitResponse response = _selectedReader.AuthenticateBiometricOnServer(
                requestId, fingerIndex, sensorTimeout);

            ValidateResponse(requestId, response.XmlString);

            return response;
        }

        /// <summary>
        /// Reset the card PIN
        /// </summary>
        public ToolkitResponse ResetPin(string pin, FingerData fingerData, int sensorTimeout)
        {
            EnsureConnected();
            string requestId = GenerateRequestId();
            string requestHandle = _selectedReader.PrepareRequest(requestId);
            string encodedPin = EncryptWithDataProtectionKey(requestHandle, pin);

            ToolkitResponse response = _selectedReader.ResetPin(
                encodedPin, fingerData, sensorTimeout);

            if (!string.IsNullOrEmpty(response.XmlString))
            {
                ValidateResponse(requestId, response.XmlString);
            }

            return response;
        }

        /// <summary>
        /// Unblock the card PIN
        /// </summary>
        public ToolkitResponse UnblockPin(string pin, FingerData fingerData, int sensorTimeout)
        {
            EnsureConnected();
            string requestId = GenerateRequestId();
            string requestHandle = _selectedReader.PrepareRequest(requestId);
            string encodedPin = EncryptWithDataProtectionKey(requestHandle, pin);

            ToolkitResponse response = _selectedReader.UnblockPin(
                encodedPin, fingerData, sensorTimeout);

            if (!string.IsNullOrEmpty(response.XmlString))
            {
                ValidateResponse(requestId, response.XmlString);
            }

            return response;
        }

        /// <summary>
        /// Get PKI certificates from the card
        /// </summary>
        public CardCertificates GetPkiCertificates(string pin)
        {
            EnsureConnected();
            string requestId = GenerateRequestId();
            string requestHandle = _selectedReader.PrepareRequest(requestId);
            string encodedPin = EncryptWithDataProtectionKey(requestHandle, pin);

            CardCertificates certs = _selectedReader.GetPkiCertificates(encodedPin);

            ValidateResponse(requestId, certs.XmlString);

            return certs;
        }

        /// <summary>
        /// Read family book data from the card (requires PIN)
        /// </summary>
        public CardFamilyBookData ReadFamilyBookData(string pin)
        {
            EnsureConnected();
            string requestId = GenerateRequestId();
            string requestHandle = _selectedReader.PrepareRequest(requestId);
            string encodedPin = EncryptWithDataProtectionKey(requestHandle, pin);

            CardFamilyBookData data = _selectedReader.ReadFamilyBookData(encodedPin);

            ValidateResponse(requestId, data.XmlString);

            return data;
        }

        /// <summary>
        /// Read public data EF (Elementary File) from the card
        /// </summary>
        public byte[] ReadPublicDataEF(PublicDataEFType efType, bool validateSignature)
        {
            EnsureConnected();
            return _selectedReader.ReadPublicDataEF(efType, validateSignature);
        }

        /// <summary>
        /// Parse EF data bytes into a string
        /// </summary>
        public string ParseEFData(byte[] efData)
        {
            return CardReader.ParseEFData(efData);
        }

        /// <summary>
        /// Get the license expiry date
        /// </summary>
        public string GetLicenseExpiryDate()
        {
            EnsureInitialized();
            return _toolkit.GetLicenseExpiryDate();
        }

        /// <summary>
        /// Get config certificate expiry date
        /// </summary>
        public ConfigCertExpiryDate GetConfigCertificateExpiryDate()
        {
            EnsureInitialized();
            return _toolkit.GetConfigCertificateExpiryDate();
        }

        /// <summary>
        /// Get device ID
        /// </summary>
        public string GetDeviceId()
        {
            EnsureInitialized();
            return _toolkit.GetDeviceId();
        }

        /// <summary>
        /// Register the device with Validation Gateway
        /// </summary>
        public RegisterDeviceResponse RegisterDevice(string userId, string password, string deviceReferenceId)
        {
            EnsureInitialized();
            string requestId = GenerateRequestId();
            string requestHandle = _toolkit.PrepareRequest(requestId);
            string encodedUserId = EncryptWithDataProtectionKey(requestHandle, userId);
            string encodedPassword = EncryptWithDataProtectionKey(requestHandle, password);

            RegisterDeviceResponse response = _toolkit.RegisterDevice(
                encodedUserId, encodedPassword, deviceReferenceId);

            ValidateResponse(requestId, response.XmlString);

            return response;
        }

        /// <summary>
        /// Authenticate card and biometric with Validation Gateway
        /// </summary>
        public ToolkitResponse AuthenticateCardAndBiometric(FingerIndex fingerIndex, int sensorTimeout)
        {
            EnsureConnected();
            string requestId = GenerateRequestId();

            ToolkitResponse response = _selectedReader.AuthenticateCardAndBiometric(
                requestId, fingerIndex, sensorTimeout);

            ValidateResponse(requestId, response.XmlString);

            return response;
        }

        /// <summary>
        /// Authenticate PKI with PIN
        /// </summary>
        public ToolkitResponse AuthenticatePki(string pin)
        {
            EnsureConnected();
            string requestId = GenerateRequestId();
            string requestHandle = _selectedReader.PrepareRequest(requestId);
            string encodedPin = EncryptWithDataProtectionKey(requestHandle, pin);

            ToolkitResponse response = _selectedReader.AuthenticatePki(encodedPin);

            ValidateResponse(requestId, response.XmlString);

            return response;
        }

        /// <summary>
        /// Sign data with authentication certificate (SignChallenge)
        /// </summary>
        public SignatureResponse SignDataWithAuthCert(byte[] data, bool isHash, string pin)
        {
            EnsureConnected();
            string requestId = GenerateRequestId();
            string requestHandle = _selectedReader.PrepareRequest(requestId);
            string encodedPin = EncryptWithDataProtectionKey(requestHandle, pin);

            SignatureResponse response = _selectedReader.SignChallenge(
                data, isHash, encodedPin);

            ValidateResponse(requestId, response.XmlString);

            return response;
        }

        /// <summary>
        /// Sign data with signing certificate (SignData)
        /// </summary>
        public SignatureResponse SignDataWithSignCert(byte[] data, bool isHash, string pin)
        {
            EnsureConnected();
            string requestId = GenerateRequestId();
            string requestHandle = _selectedReader.PrepareRequest(requestId);
            string encodedPin = EncryptWithDataProtectionKey(requestHandle, pin);

            SignatureResponse response = _selectedReader.SignData(
                data, isHash, encodedPin);

            ValidateResponse(requestId, response.XmlString);

            return response;
        }

        /// <summary>
        /// Verify a digital signature
        /// </summary>
        public void VerifySignatureData(byte[] data, bool isHash, byte[] signature, byte[] certificate)
        {
            EnsureConnected();
            _selectedReader.VerifySignature(data, isHash, signature, certificate);
        }

        /// <summary>
        /// Sign a file with CAdES-B (detached)
        /// </summary>
        public ToolkitResponse CadesSign(string inputFilePath, string pin)
        {
            EnsureConnected();
            string requestId = GenerateRequestId();
            string requestHandle = _selectedReader.PrepareRequest(requestId);
            string encodedPin = EncryptWithDataProtectionKey(requestHandle, pin);

            SigningContext sc = new SigningContext();
            sc.SignatureLevel = SignatureLevel.BaselineB;
            sc.PackagingMode = PackagingMode.Detached;
            sc.EncodedPin = encodedPin;

            ToolkitResponse response =
                _selectedReader.CadesSign(sc, inputFilePath);

            ValidateResponse(requestId, response.XmlString);

            return response;
        }

        /// <summary>
        /// Verify a CAdES detached signature
        /// </summary>
        public string CadesVerify(string inputFilePath, byte[] signature)
        {
            EnsureConnected();

            VerificationContext vc = new VerificationContext();
            vc.IsDeattached = true;
            vc.ReportType = ReportType.Detailed;

            return _selectedReader.CadesVerify(vc, inputFilePath, signature);
        }

        /// <summary>
        /// Sign an XML file with XAdES-B (enveloped)
        /// </summary>
        public ToolkitResponse XadesSign(
            string xmlFilePath, string signedXmlFilePath, string pin)
        {
            EnsureConnected();
            string requestId = GenerateRequestId();
            string requestHandle = _selectedReader.PrepareRequest(requestId);
            string encodedPin = EncryptWithDataProtectionKey(requestHandle, pin);

            SigningContext sc = new SigningContext();
            sc.SignatureLevel = SignatureLevel.BaselineB;
            sc.PackagingMode = PackagingMode.Enveloped;
            sc.EncodedPin = encodedPin;

            ToolkitResponse response =
                _selectedReader.XadesSign(sc, xmlFilePath, signedXmlFilePath);

            if (!string.IsNullOrEmpty(response.XmlString))
            {
                ValidateResponse(requestId, response.XmlString);
            }

            return response;
        }

        /// <summary>
        /// Sign a PDF file with PAdES-B (visible signature)
        /// </summary>
        public ToolkitResponse PadesSign(
            string pdfFilePath, string signedPdfFilePath, string pin,
            PadesSignParams padesParams)
        {
            EnsureConnected();
            string requestId = GenerateRequestId();
            string requestHandle = _selectedReader.PrepareRequest(requestId);
            string encodedPin = EncryptWithDataProtectionKey(requestHandle, pin);

            SigningContext sc = new SigningContext();
            sc.SignatureLevel = SignatureLevel.BaselineB;
            sc.PackagingMode = PackagingMode.Enveloped;
            sc.EncodedPin = encodedPin;

            ToolkitResponse response =
                _selectedReader.PadesSign(
                    sc, pdfFilePath, signedPdfFilePath, padesParams);

            if (!string.IsNullOrEmpty(response.XmlString))
            {
                ValidateResponse(requestId, response.XmlString);
            }

            return response;
        }

        /// <summary>
        /// Set NFC authentication parameters
        /// </summary>
        public void SetNfcAuthenticationParameters(string cardNumber, string dateOfBirth, string expiryDate)
        {
            EnsureConnected();
            _selectedReader.SetNfcAuthenticationParameters(cardNumber, dateOfBirth, expiryDate);
        }

        /// <summary>
        /// Validate a toolkit XML response using SignatureValidator
        /// </summary>
        public ToolkitResponseData ValidateToolkitResponse(string xmlResponse,
            byte[] certificateData, byte[] certificateChain)
        {
            SignatureValidator validator = new SignatureValidator(certificateData, certificateChain);
            return validator.ValidateToolkitResponse(xmlResponse);
        }

        // ---- Helper Methods ----

        private void EnsureInitialized()
        {
            if (!_isInitialized || _toolkit == null)
            {
                throw new InvalidOperationException("Toolkit is not initialized. Please initialize the toolkit first.");
            }
        }

        private void EnsureReaderSelected()
        {
            if (_selectedReader == null)
            {
                throw new InvalidOperationException("No reader selected. Please select a reader first.");
            }
        }

        private void EnsureConnected()
        {
            EnsureInitialized();

            if (_selectedReader == null || !_isConnected)
            {
                throw new InvalidOperationException("Card is not connected. Please connect to the card first.");
            }
        }

        /// <summary>
        /// Generate a cryptographically random request ID
        /// </summary>
        public static string GenerateRequestId()
        {
            byte[] randBytes = new byte[40];

            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(randBytes);
            }

            return Convert.ToBase64String(randBytes);
        }

        /// <summary>
        /// RSA-PKCS#1v1.5 encrypt (requestHandle || plaintext) with the data protection key.
        /// Used for PINs, passwords, user IDs -- any sensitive field the service decrypts server-side.
        /// </summary>
        public string EncryptWithDataProtectionKey(string requestHandle, string plaintext)
        {
            byte[] decodedRequestHandle = Convert.FromBase64String(requestHandle);
            byte[] plaintextBytes = Encoding.ASCII.GetBytes(plaintext);
            byte[] wrappedData = new byte[decodedRequestHandle.Length + plaintextBytes.Length];

            decodedRequestHandle.CopyTo(wrappedData, 0);
            plaintextBytes.CopyTo(wrappedData, decodedRequestHandle.Length);

            DataProtectionKey dataProtection = _toolkit.GetDataProtectionKey();

            using (RSACryptoServiceProvider rsa = DecodeX509PublicKey(dataProtection.PublicKey))
            {
                if (rsa == null)
                {
                    return null;
                }

                return Convert.ToBase64String(rsa.Encrypt(wrappedData, false));
            }
        }

        /// <summary>
        /// Verify the XML response signature
        /// </summary>
        public static bool VerifySignature(string response)
        {
            try
            {
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.XmlResolver = null;
                xmlDocument.PreserveWhitespace = false;
                xmlDocument.LoadXml(response);

                SignedXml signedXml = new SignedXml(xmlDocument);
                XmlNodeList nodeList = xmlDocument.GetElementsByTagName("Signature",
                    "http://www.w3.org/2000/09/xmldsig#");

                if (nodeList.Count == 0)
                {
                    return false;
                }

                signedXml.LoadXml((XmlElement)nodeList[0]);
                return signedXml.CheckSignature();
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Compare request ID from the response with the original
        /// </summary>
        public static bool CompareRequestId(string requestId, string response)
        {
            try
            {
                XmlReaderSettings settings = new XmlReaderSettings();
                settings.DtdProcessing = DtdProcessing.Prohibit;
                settings.XmlResolver = null;

                using (XmlReader xmlReader = XmlReader.Create(new StringReader(response), settings))
                {
                    xmlReader.ReadToFollowing("RequestID");
                    string requestIdResponse = xmlReader.ReadElementContentAsString();
                    return string.Compare(requestId, requestIdResponse) == 0;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Validate response request ID and signature.
        /// Request ID mismatch throws; signature failure sets LastValidationWarning (non-blocking).
        /// </summary>
        private void ValidateResponse(string requestId, string xmlString)
        {
            LastValidationWarning = null;

            if (string.IsNullOrEmpty(xmlString))
            {
                return;
            }

            if (!CompareRequestId(requestId, xmlString))
            {
                throw new InvalidOperationException("Request ID verification failed, response may be tampered");
            }

            if (!VerifySignature(xmlString))
            {
                LastValidationWarning = "Warning: Response signature verification failed";
            }
        }

        /// <summary>
        /// Decode X.509 public key from DER-encoded SubjectPublicKeyInfo
        /// </summary>
        private static RSACryptoServiceProvider DecodeX509PublicKey(byte[] x509key)
        {
            byte[] seqOID = { 0x30, 0x0D, 0x06, 0x09, 0x2A, 0x86, 0x48, 0x86, 0xF7,
                0x0D, 0x01, 0x01, 0x01, 0x05, 0x00 };
            byte[] seq = new byte[15];

            using (MemoryStream mem = new MemoryStream(x509key))
            using (BinaryReader binr = new BinaryReader(mem))
            {
                try
                {
                    ushort twobytes = binr.ReadUInt16();

                    if (twobytes == 0x8130)
                    {
                        binr.ReadByte();
                    }
                    else if (twobytes == 0x8230)
                    {
                        binr.ReadInt16();
                    }
                    else
                    {
                        return null;
                    }

                    seq = binr.ReadBytes(15);

                    if (!CompareByteArrays(seq, seqOID))
                    {
                        return null;
                    }

                    twobytes = binr.ReadUInt16();

                    if (twobytes == 0x8103)
                    {
                        binr.ReadByte();
                    }
                    else if (twobytes == 0x8203)
                    {
                        binr.ReadInt16();
                    }
                    else
                    {
                        return null;
                    }

                    byte bt = binr.ReadByte();

                    if (bt != 0x00)
                    {
                        return null;
                    }

                    twobytes = binr.ReadUInt16();

                    if (twobytes == 0x8130)
                    {
                        binr.ReadByte();
                    }
                    else if (twobytes == 0x8230)
                    {
                        binr.ReadInt16();
                    }
                    else
                    {
                        return null;
                    }

                    twobytes = binr.ReadUInt16();
                    byte lowbyte = 0x00;
                    byte highbyte = 0x00;

                    if (twobytes == 0x8102)
                    {
                        lowbyte = binr.ReadByte();
                    }
                    else if (twobytes == 0x8202)
                    {
                        highbyte = binr.ReadByte();
                        lowbyte = binr.ReadByte();
                    }
                    else
                    {
                        return null;
                    }

                    byte[] modint = { lowbyte, highbyte, 0x00, 0x00 };
                    int modsize = BitConverter.ToInt32(modint, 0);

                    byte firstbyte = binr.ReadByte();
                    binr.BaseStream.Seek(-1, SeekOrigin.Current);

                    if (firstbyte == 0x00)
                    {
                        binr.ReadByte();
                        modsize -= 1;
                    }

                    byte[] modulus = binr.ReadBytes(modsize);

                    if (binr.ReadByte() != 0x02)
                    {
                        return null;
                    }

                    int expbytes = (int)binr.ReadByte();
                    byte[] exponent = binr.ReadBytes(expbytes);

                    RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
                    RSAParameters rsaKeyInfo = new RSAParameters();
                    rsaKeyInfo.Modulus = modulus;
                    rsaKeyInfo.Exponent = exponent;
                    rsa.ImportParameters(rsaKeyInfo);

                    return rsa;
                }
                catch
                {
                    return null;
                }
            }
        }

        private static bool CompareByteArrays(byte[] a, byte[] b)
        {
            if (a.Length != b.Length)
            {
                return false;
            }

            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] != b[i])
                {
                    return false;
                }
            }

            return true;
        }
    }
}
