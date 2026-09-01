package ae.emiratesid.idcard.toolkit.sample.utils;

import android.os.Environment;

import org.apache.xml.security.keys.KeyInfo;
import org.apache.xml.security.signature.XMLSignature;
import org.apache.xml.security.utils.Constants;
import org.w3c.dom.Attr;
import org.w3c.dom.Document;
import org.w3c.dom.Element;
import org.w3c.dom.Node;
import org.w3c.dom.NodeList;

import java.io.FileInputStream;
import java.io.FileWriter;
import java.io.IOException;
import java.io.InputStream;
import java.security.PublicKey;
import java.security.cert.X509Certificate;
import java.util.ArrayList;

import javax.xml.parsers.DocumentBuilderFactory;

import ae.emiratesid.idcard.toolkit.ToolkitException;
import ae.emiratesid.idcard.toolkit.sample.logger.Logger;

public class XmlUtils {

    public static String validateXMLForRequestHandle(String responseXml , String requestID) throws ToolkitException{

        ArrayList<String> response = validateAndGetElements(responseXml);
        if(response.get(0) != null && response.get(0).equals(requestID)){
            Logger.d("request success");
            return response.get(1);
        }//
        else{
            throw new ToolkitException("RequestID doesn't match");
        }//else
    }//validateXML()

    public static boolean validateXML(String responseXml , String requestID) throws ToolkitException{
        ArrayList<String> response = validateAndGetElements(responseXml);
        if(response.get(0) != null && response.get(0).equals(requestID)){
            Logger.d("request success");
            return true;
        }//
        else{
            throw new ToolkitException("RequestID doesn't match");
        }//else
    }//validateXML()



	private static ArrayList<String> parseXMLForElements(Document doc , String...elements){

        try {
            doc.getDocumentElement().normalize();
            Logger.d("Root element :" + doc.getDocumentElement().getNodeName());
            NodeList nList = doc.getElementsByTagName("Header");
            Logger.d("----------------------------"+nList.getLength());
            return fetchElements(nList , elements);
        } catch (Exception e) {
            e.printStackTrace();
        }//catch()
        return null;

    }//parseVerifyPinXml()

	private static ArrayList<String> fetchElements(NodeList nList , String...elements) {
		ArrayList<String> values =  new ArrayList<>();
        for (int temp = 0; temp < nList.getLength(); temp++) {
            Node nNode = nList.item(temp);
            Logger.d("\nCurrent Element :" + nNode.getNodeName());

            if (nNode.getNodeType() == Node.ELEMENT_NODE) {
                Element eElement = (Element) nNode;
                for(int j =  0 ; j<elements.length ; j++){
                        values.add(eElement
                                .getElementsByTagName(elements[j])
                                .item(0)
                                .getTextContent());
                    }
                }
        }//for()
        return values;
    }//parseHeader()

	private static boolean verifySignature(Document doc) throws Exception {
        boolean valid = false;
        try {
            // parse the XML


            // verify signature
            NodeList nodes = doc.getElementsByTagNameNS(Constants.SignatureSpecNS, "Signature");
            if (nodes.getLength() == 0) {
                return true;
            }
            Logger.d("Signature  found ");

            Element sigElement = (Element) nodes.item(0);
            XMLSignature signature = new XMLSignature(sigElement, "");
            Element element = (Element)doc.getElementsByTagName("Message").item(0);
            Logger.d(doc.getDocumentElement().getTagName());
            Attr attr = element.getAttributeNode( "xml:id");

            Logger.d(element.getAttributes().item(0) +"");
            element.setIdAttributeNode(attr, true);

            KeyInfo ki = signature.getKeyInfo();
            if (ki == null) {
                throw new Exception("Did not find KeyInfo");
            }
            Logger.d( "key info found");
            X509Certificate cert = signature.getKeyInfo().getX509Certificate();
            if (cert == null) {
                PublicKey pk = signature.getKeyInfo().getPublicKey();
                if (pk == null) {
                    throw new Exception("Did not find Certificate or Public Key");
                }
                valid = signature.checkSignatureValue(pk);
            }
            else {
                valid = signature.checkSignatureValue(cert);
            }
            Logger.d("Signature validation result " + valid);
        }
        catch (Exception e) {
            e.printStackTrace();
        }

        return valid;
    }//



    private static String writeToFile(String xml) throws IOException{
		String path = Environment.getExternalStorageDirectory().getAbsolutePath()+"/response.xml";
		FileWriter writer = new FileWriter(path);
		writer.write(xml);
		writer.close();
		return path;
	}//writeToFile()

    private static ArrayList<String> validateAndGetElements(String responseXml) throws ToolkitException {
        ArrayList<String> response =  null;
        if(null == responseXml || responseXml.isEmpty()){
            Logger.d("responseXml is null");
            throw new ToolkitException("responseXml is null");
        }//if()
        else{
            String path;
            try {
                path = XmlUtils.writeToFile(responseXml);
                InputStream in = new FileInputStream(path);
                DocumentBuilderFactory f = DocumentBuilderFactory.newInstance();
                f.setNamespaceAware(true);
                Document doc = f.newDocumentBuilder().parse(in);
                in.close();
                if(!XmlUtils.verifySignature(doc)){
                    Logger.d("Signature validation failed");
                    throw new ToolkitException("Signature validation failed");
                }//if()
                else{
                    response=  XmlUtils.parseXMLForElements(doc , "RequestID", "RequestHandle");
                    return response;
                }//else
            } catch (IOException e) {
                e.printStackTrace();
                throw new ToolkitException(e);
            } catch (Exception e) {
                e.printStackTrace();
                throw new ToolkitException(e);
            }//catch()
        }//else
    }

}
