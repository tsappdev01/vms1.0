using System;
using System.Text;
using System.Windows;
using System.Xml;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;

using AE.EmiratesId.IdCard;
using AE.EmiratesId.IdCard.DataModels;

namespace EIDAToolkitApp
{
    public static class Helper
    {
        /// <summary>
        /// Generate Request ID
        /// </summary>
        /// <returns>Request ID</returns>
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
        /// Verify signature of XML response
        /// </summary>
        /// <param name="response">XML response to verify</param>
        /// <returns>True if verified else false</returns>
        public static bool VerifySignature(string response)
        {
            bool isVerified = false;

            try
            {
                // Create a new Xml Document.
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.XmlResolver = null;
                xmlDocument.PreserveWhitespace = false;

                // Load the passed Xml string into the document.
                xmlDocument.LoadXml(response);

                SignedXmlWithId signedXmlWithId = new SignedXmlWithId(xmlDocument);

                // Find the "Signature" node and create a new XmlNodeList object.
                XmlNodeList nodeList = xmlDocument.GetElementsByTagName("Signature",
                    "http://www.w3.org/2000/09/xmldsig#");

                // Load the signature node.
                signedXmlWithId.LoadXml((XmlElement)nodeList[0]);

                // Verify signature
                isVerified = signedXmlWithId.CheckSignature();
            }
            catch (XmlException ex)
            {
                MessageBox.Show("XmlException: "+ex.Message, "Error");
                isVerified = false;
            }
            catch (ArgumentNullException ex)
            {
                MessageBox.Show("ArgumentNullException: "+ex.Message, "Error");
                isVerified = false;
            }
            catch (CryptographicException ex)
            {
                MessageBox.Show("CryptographicException: "+ex.Message, "Error");
                isVerified = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception: " +ex.Message, "Error");
                isVerified = false;
            }
            return isVerified;
        }

        /// <summary>
        /// Compare Request ID of XML response with application Request ID
        /// </summary>
        /// <param name="requestId">Request ID from application</param>
        /// <param name="response">XML response</param>
        /// <returns>True if compared successfully else false</returns>
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

                    // Compare request id
                    if (0 != string.Compare(requestId, requestIdResponse))
                    {
                        return false;
                    }
                }
            }
            catch (ArgumentNullException ex)
            {
                MessageBox.Show("ArgumentNullException: "+ex.Message, "Error");
                return false;
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show("ArgumentException: "+ex.Message, "Error");
                return false;
            }
            catch (XmlException ex)
            {
                MessageBox.Show("XmlException: "+ex.Message, "Error");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Performs encryption and base64 encoding
        /// </summary>
        /// <param name="requestHandle">Request handle</param>
        /// <param name="param">Parameter to encrypt and encode</param>
        /// <returns>Encrypted and base64 encoded string</returns>
        public static string Base64Encryption(string requestHandle, string param, 
            Toolkit toolkit)
        {
            byte[] decodedRequestHandle = Convert.FromBase64String(requestHandle);
            byte[] paramByteArray = Encoding.ASCII.GetBytes(param);
            byte[] wrappedData = 
                new byte[decodedRequestHandle.Length + paramByteArray.Length];
            decodedRequestHandle.CopyTo(wrappedData, 0);
            paramByteArray.CopyTo(wrappedData, decodedRequestHandle.Length);
            string base64String = RSA(wrappedData, toolkit);
            return base64String;
        }

        /// <summary>
        /// Decode and extracts the modulus and exponent of the public key
        /// </summary>
        /// <param name="x509key">Public key</param>
        /// <returns></returns>
        public static RSACryptoServiceProvider DecodeX509PublicKey(byte[] x509key)
        {
            // encoded OID sequence for  PKCS #1 rsaEncryption szOID_RSA_RSA = "1.2.840.113549.1.1.1"
            byte[] seqOID = { 0x30, 0x0D, 0x06, 0x09, 0x2A, 0x86, 0x48, 0x86, 0xF7,
                0x0D, 0x01, 0x01, 0x01, 0x05, 0x00 };
            byte[] seq = new byte[15];
            // ---------  Set up stream to read the asn.1 encoded SubjectPublicKeyInfo blob  ------
            MemoryStream mem = new MemoryStream(x509key);
            BinaryReader binr = new BinaryReader(mem);    //wrap Memory Stream with BinaryReader for easy reading
            byte bt = 0;
            ushort twobytes = 0;

            try
            {
                twobytes = binr.ReadUInt16();
                if (twobytes == 0x8130) //data read as little endian order (actual data order for Sequence is 30 81)
                    binr.ReadByte();    //advance 1 byte
                else if (twobytes == 0x8230)
                    binr.ReadInt16();   //advance 2 bytes
                else
                    return null;

                seq = binr.ReadBytes(15);       //read the Sequence OID
                if (!CompareByteArrays(seq, seqOID))    //make sure Sequence for OID is correct
                    return null;

                twobytes = binr.ReadUInt16();
                if (twobytes == 0x8103) //data read as little endian order (actual data order for Bit String is 03 81)
                    binr.ReadByte();    //advance 1 byte
                else if (twobytes == 0x8203)
                    binr.ReadInt16();   //advance 2 bytes
                else
                    return null;

                bt = binr.ReadByte();
                if (bt != 0x00)     //expect null byte next
                    return null;

                twobytes = binr.ReadUInt16();
                if (twobytes == 0x8130) //data read as little endian order (actual data order for Sequence is 30 81)
                    binr.ReadByte();    //advance 1 byte
                else if (twobytes == 0x8230)
                    binr.ReadInt16();   //advance 2 bytes
                else
                    return null;

                twobytes = binr.ReadUInt16();
                byte lowbyte = 0x00;
                byte highbyte = 0x00;

                if (twobytes == 0x8102) //data read as little endian order (actual data order for Integer is 02 81)
                    lowbyte = binr.ReadByte();  // read next bytes which is bytes in modulus
                else if (twobytes == 0x8202)
                {
                    highbyte = binr.ReadByte(); //advance 2 bytes
                    lowbyte = binr.ReadByte();
                }
                else
                    return null;
                byte[] modint = { lowbyte, highbyte, 0x00, 0x00 };   //reverse byte order since asn.1 key uses big endian order
                int modsize = BitConverter.ToInt32(modint, 0);

                byte firstbyte = binr.ReadByte();
                binr.BaseStream.Seek(-1, SeekOrigin.Current);

                if (firstbyte == 0x00)
                {   //if first byte (highest order) of modulus is zero, don't include it
                    binr.ReadByte();    //skip this null byte
                    modsize -= 1;   //reduce modulus buffer size by 1
                }

                byte[] modulus = binr.ReadBytes(modsize);   //read the modulus bytes

                if (binr.ReadByte() != 0x02)            //expect an Integer for the exponent data
                    return null;
                int expbytes = (int)binr.ReadByte();        // should only need one byte for actual exponent data (for all useful values)
                byte[] exponent = binr.ReadBytes(expbytes);
                              
                // Create RSACryptoServiceProvider instance and initialize with public key
                RSACryptoServiceProvider RSA = new RSACryptoServiceProvider();
                RSAParameters RSAKeyInfo = new RSAParameters();
                RSAKeyInfo.Modulus = modulus;
                RSAKeyInfo.Exponent = exponent;
                RSA.ImportParameters(RSAKeyInfo);
                return RSA;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception: "+ex.Message);
                return null;
            }

            finally { binr.Close(); }
        }

        /// <summary>
        /// RSA Encryption
        /// </summary>
        /// <param name="wrappedData"></param>
        /// <returns></returns>
        private static string RSA(byte[] wrappedData, Toolkit toolkit)
        {
            DataProtectionKey dataProtection = toolkit.GetDataProtectionKey();

            using (RSACryptoServiceProvider rsa = DecodeX509PublicKey(dataProtection.PublicKey))
            {
                if (rsa == null)
                    return null;

                return (Convert.ToBase64String(rsa.Encrypt(wrappedData, false)));
            }
        }

        /// <summary>
        /// Compare two byte arrays
        /// </summary>
        /// <param name="a">First byte array</param>
        /// <param name="b">Second byte array</param>
        /// <returns>true if equal else false</returns>
        private static bool CompareByteArrays(byte[] a, byte[] b)
        {
            if (a.Length != b.Length)
                return false;
            int i = 0;
            foreach (byte c in a)
            {
                if (c != b[i])
                    return false;
                i++;
            }
            return true;
        }
    }

    public class SignedXmlWithId : SignedXml
    {
        public SignedXmlWithId(XmlDocument xml) : base(xml)
        {
        }

        public SignedXmlWithId(XmlElement xmlElement)
            : base(xmlElement)
        {
        }

        public override XmlElement GetIdElement(XmlDocument doc, string id)
        {
            // check to see if it's a standard ID reference
            XmlElement idElem = base.GetIdElement(doc, id);

            if (idElem == null)
            {
                var nsmgr = new XmlNamespaceManager(doc.NameTable);
                nsmgr.AddNamespace("rate", "http://www.emiratesid.ae/vg");
                idElem = doc.SelectSingleNode("//rate:Message", nsmgr) as XmlElement;
            }

            return idElem;
        }
    }
}
