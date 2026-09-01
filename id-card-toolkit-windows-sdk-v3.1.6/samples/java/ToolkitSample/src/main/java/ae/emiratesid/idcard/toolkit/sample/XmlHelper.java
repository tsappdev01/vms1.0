package ae.emiratesid.idcard.toolkit.sample;

import java.io.StringReader;
import java.security.Key;
import java.security.PublicKey;
import java.security.cert.X509Certificate;
import java.util.Iterator;

import javax.xml.crypto.AlgorithmMethod;
import javax.xml.crypto.KeySelector;
import javax.xml.crypto.KeySelectorException;
import javax.xml.crypto.KeySelectorResult;
import javax.xml.crypto.XMLCryptoContext;
import javax.xml.crypto.XMLStructure;
import javax.xml.crypto.dsig.Reference;
import javax.xml.crypto.dsig.XMLSignature;
import javax.xml.crypto.dsig.XMLSignatureFactory;
import javax.xml.crypto.dsig.dom.DOMValidateContext;
import javax.xml.crypto.dsig.keyinfo.KeyInfo;
import javax.xml.crypto.dsig.keyinfo.X509Data;
import javax.xml.parsers.DocumentBuilder;
import javax.xml.parsers.DocumentBuilderFactory;

import org.w3c.dom.Attr;
import org.w3c.dom.Document;
import org.w3c.dom.Element;
import org.w3c.dom.Node;
import org.w3c.dom.NodeList;
import org.xml.sax.InputSource;

import ae.emiratesid.idcard.toolkit.ToolkitException;

/**
 * Stateless XML utility methods for signature verification and request ID extraction.
 * All methods are static. All XML parsing is XXE-safe.
 */
public final class XmlHelper {

    private XmlHelper() {
    }

    /**
     * Verifies the XML digital signature in the given XML string.
     *
     * @param xmlString the XML document as a string
     * @return true if the signature is valid, false if no signature element is found
     * @throws ToolkitException if signature validation fails
     */
    public static boolean verifySignature(String xmlString) throws ToolkitException {
        try {
            Document doc = parseXml(xmlString, true);
            NodeList signatureNodes = doc.getElementsByTagNameNS(XMLSignature.XMLNS, "Signature");
            if (signatureNodes.getLength() == 0) {
                return false;
            }

            XMLSignatureFactory factory = XMLSignatureFactory.getInstance("DOM");
            DOMValidateContext valContext = new DOMValidateContext(
                    new X509KeySelector(), signatureNodes.item(0));

            // Register the xml:id attribute on the Message element for reference resolution
            Element messageElement = (Element) doc.getElementsByTagName("Message").item(0);
            if (messageElement != null) {
                Attr idAttr = messageElement.getAttributeNode("xml:id");
                if (idAttr != null) {
                    messageElement.setIdAttributeNode(idAttr, true);
                }
            }

            XMLSignature signature = factory.unmarshalXMLSignature(valContext);
            boolean valid = signature.validate(valContext);

            if (!valid) {
                boolean signatureValueValid = signature.getSignatureValue().validate(valContext);
                System.out.println("Signature value valid: " + signatureValueValid);
                Iterator<?> refs = signature.getSignedInfo().getReferences().iterator();
                for (int i = 0; refs.hasNext(); i++) {
                    Reference ref = (Reference) refs.next();
                    boolean refValid = ref.validate(valContext);
                    System.out.println("Reference[" + i + "] valid: " + refValid);
                }
            }

            return valid;
        }
        catch (ToolkitException e) {
            throw e;
        }
        catch (Exception e) {
            throw new ToolkitException("XML signature verification failed: " + e.getMessage());
        }
    }

    /**
     * Extracts the RequestID from the Header element in the given XML string.
     *
     * @param xmlString the XML document as a string
     * @return the RequestID text content, or null if not found
     * @throws ToolkitException if XML parsing fails
     */
    public static String extractRequestId(String xmlString) throws ToolkitException {
        try {
            Document doc = parseXml(xmlString, false);
            NodeList headerNodes = doc.getElementsByTagName("Header");

            for (int i = 0; i < headerNodes.getLength(); i++) {
                Node node = headerNodes.item(i);
                if (node.getNodeType() == Node.ELEMENT_NODE) {
                    Element headerElement = (Element) node;
                    NodeList requestIdNodes = headerElement.getElementsByTagName("RequestID");
                    if (requestIdNodes != null && requestIdNodes.getLength() > 0) {
                        return requestIdNodes.item(0).getTextContent();
                    }
                }
            }

            return null;
        }
        catch (ToolkitException e) {
            throw e;
        }
        catch (Exception e) {
            throw new ToolkitException("Failed to extract RequestID from XML: " + e.getMessage());
        }
    }

    /**
     * Verifies the XML signature and checks that the embedded RequestID matches the expected value.
     *
     * @param xmlString         the XML document as a string
     * @param expectedRequestId the expected request ID to match
     * @throws ToolkitException if signature is invalid or request ID does not match
     */
    public static void verifyAndExtractRequestId(String xmlString, String expectedRequestId) throws ToolkitException {
        if (!verifySignature(xmlString)) {
            throw new ToolkitException("XML signature validation failed");
        }

        String actualRequestId = extractRequestId(xmlString);
        if (actualRequestId == null || !actualRequestId.equals(expectedRequestId)) {
            throw new ToolkitException("RequestID mismatch: expected [" + expectedRequestId
                    + "], got [" + actualRequestId + "]");
        }
    }

    /**
     * Parses an XML string into a Document with XXE protections enabled.
     *
     * @param xmlString      the XML string to parse
     * @param namespaceAware whether namespace awareness is needed
     * @return the parsed Document
     * @throws Exception if parsing fails
     */
    private static Document parseXml(String xmlString, boolean namespaceAware) throws Exception {
        DocumentBuilderFactory factory = DocumentBuilderFactory.newInstance();
        factory.setNamespaceAware(namespaceAware);
        factory.setFeature("http://apache.org/xml/features/disallow-doctype-decl", true);
        factory.setFeature("http://xml.org/sax/features/external-general-entities", false);
        factory.setFeature("http://xml.org/sax/features/external-parameter-entities", false);

        DocumentBuilder builder = factory.newDocumentBuilder();
        return builder.parse(new InputSource(new StringReader(xmlString)));
    }

    /**
     * KeySelector that extracts the public key from X509 certificate data in the XML signature.
     * Supports RSA and DSA keys with SHA1, SHA256, SHA384, and SHA512 signature algorithms.
     */
    private static class X509KeySelector extends KeySelector {

        private static final String XMLDSIG_NS = "http://www.w3.org/2000/09/xmldsig#";
        private static final String XMLDSIG_MORE_NS = "http://www.w3.org/2001/04/xmldsig-more#";

        public KeySelectorResult select(KeyInfo keyInfo, KeySelector.Purpose purpose,
                AlgorithmMethod method, XMLCryptoContext context) throws KeySelectorException {

            Iterator<?> keyInfoContent = keyInfo.getContent().iterator();
            while (keyInfoContent.hasNext()) {
                XMLStructure info = (XMLStructure) keyInfoContent.next();
                if (!(info instanceof X509Data)) {
                    continue;
                }

                X509Data x509Data = (X509Data) info;
                Iterator<?> x509Content = x509Data.getContent().iterator();
                while (x509Content.hasNext()) {
                    Object item = x509Content.next();
                    if (!(item instanceof X509Certificate)) {
                        continue;
                    }

                    final PublicKey publicKey = ((X509Certificate) item).getPublicKey();
                    if (isAlgorithmCompatible(method.getAlgorithm(), publicKey.getAlgorithm())) {
                        return new KeySelectorResult() {
                            public Key getKey() {
                                return publicKey;
                            }
                        };
                    }
                }
            }

            throw new KeySelectorException("No compatible key found in X509 certificate data");
        }

        /**
         * Checks whether the XML signature algorithm URI is compatible with the key algorithm.
         * Supports SHA1, SHA256, SHA384, and SHA512 for both RSA and DSA.
         */
        private boolean isAlgorithmCompatible(String algorithmUri, String keyAlgorithm) {
            if ("RSA".equalsIgnoreCase(keyAlgorithm)) {
                return XMLDSIG_NS.equals(getNamespace(algorithmUri))
                        && algorithmUri.endsWith("#rsa-sha1")
                    || XMLDSIG_MORE_NS.equals(getNamespace(algorithmUri))
                        && (algorithmUri.endsWith("#rsa-sha256")
                            || algorithmUri.endsWith("#rsa-sha384")
                            || algorithmUri.endsWith("#rsa-sha512"));
            }

            if ("DSA".equalsIgnoreCase(keyAlgorithm)) {
                return algorithmUri.endsWith("#dsa-sha1")
                    || algorithmUri.endsWith("#dsa-sha256");
            }

            return false;
        }

        private String getNamespace(String uri) {
            int hashIndex = uri.lastIndexOf('#');
            if (hashIndex >= 0) {
                return uri.substring(0, hashIndex + 1);
            }

            return uri;
        }
    }
}
