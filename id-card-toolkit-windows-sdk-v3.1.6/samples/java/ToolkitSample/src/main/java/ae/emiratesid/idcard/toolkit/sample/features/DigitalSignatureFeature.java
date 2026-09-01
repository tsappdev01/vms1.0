package ae.emiratesid.idcard.toolkit.sample.features;

import java.io.File;
import java.nio.charset.Charset;
import java.nio.file.Files;
import java.nio.file.Paths;
import java.util.Base64;
import java.util.Scanner;

import com.google.gson.JsonObject;
import com.google.gson.JsonParser;

import ae.emiratesid.idcard.toolkit.CardReader;
import ae.emiratesid.idcard.toolkit.ToolkitException;
import ae.emiratesid.idcard.toolkit.datamodel.PadesSigningContext;
import ae.emiratesid.idcard.toolkit.datamodel.SigningContext;
import ae.emiratesid.idcard.toolkit.datamodel.SigningContext.PackagingMode;
import ae.emiratesid.idcard.toolkit.datamodel.ToolkitResponse;
import ae.emiratesid.idcard.toolkit.datamodel.VerificationContext;
import ae.emiratesid.idcard.toolkit.sample.BaseFeature;
import ae.emiratesid.idcard.toolkit.sample.GsonUtils;
import ae.emiratesid.idcard.toolkit.sample.ToolkitSession;
import ae.emiratesid.idcard.toolkit.sample.XmlHelper;

/**
 * Digital signature operations: XAdES, PAdES, and CAdES signing and verification.
 */
public class DigitalSignatureFeature extends BaseFeature {

    private static final Charset UTF_8 = Charset.forName("UTF-8");
    private static final String INPUT_PATH_KEY = "filePath";
    private static final String OUTPUT_PATH_KEY = "signedFilePath";

    public DigitalSignatureFeature(ToolkitSession session, Scanner scanner) {
        super(session, scanner);
    }

    /**
     * Signs an XML document using XAdES.
     * Prompts for JSON params file path, reads the file, parses signing context,
     * encrypts PIN, calls xadesSign, and verifies the response signature.
     */
    public void signXml() throws Exception {
        String jsonParams = readJsonParamsFile("Enter XAdES signature params JSON file path: ");
        JsonObject jsonObject = parseJsonObject(jsonParams);

        String inputFilePath = requireKey(jsonObject, INPUT_PATH_KEY);
        String signedFilePath = requireKey(jsonObject, OUTPUT_PATH_KEY);

        String pin = readInput("Enter PIN: ");
        String encodedPin = prepareAndEncryptPin(pin);

        SigningContext signingContext = GsonUtils.parseJson(jsonParams, SigningContext.class);
        signingContext.setEncodedPin(encodedPin);

        CardReader cardReader = session.getCardReader();
        ToolkitResponse response = cardReader.xadesSign(signingContext, inputFilePath, signedFilePath);

        verifyToolkitResponse(response);

        if (signingContext.getPackagingMode() == PackagingMode.DETACHED) {
            byte[] detachedSignature = response.getDetachedSignature();
            if (detachedSignature != null && detachedSignature.length > 0) {
                System.out.println("Detached Signature: " + Base64.getEncoder().encodeToString(detachedSignature));
            }
        }

        System.out.println("XML signed successfully.");
    }

    /**
     * Signs a PDF document using PAdES.
     * Prompts for JSON params file path, reads the file, parses signing context,
     * encrypts PIN, calls padesSign, and verifies the response signature.
     */
    public void signPdf() throws Exception {
        String jsonParams = readJsonParamsFile("Enter PAdES signature params JSON file path: ");
        JsonObject jsonObject = parseJsonObject(jsonParams);

        String pdfFilePath = requireKey(jsonObject, INPUT_PATH_KEY);
        String signedPdfFilePath = requireKey(jsonObject, OUTPUT_PATH_KEY);

        String pin = readInput("Enter PIN: ");
        String encodedPin = prepareAndEncryptPin(pin);

        PadesSigningContext padesContext = GsonUtils.parseJson(jsonParams, PadesSigningContext.class);
        padesContext.setEncodedPin(encodedPin);

        CardReader cardReader = session.getCardReader();
        ToolkitResponse response = cardReader.padesSign(padesContext, pdfFilePath, signedPdfFilePath);

        verifyToolkitResponse(response);

        System.out.println("PDF signed successfully.");
    }

    /**
     * Signs a document using CAdES (detached only).
     * Prompts for JSON params file path, reads the file, parses signing context,
     * encrypts PIN, calls cadesSign, and prints the detached signature as Base64.
     */
    public void signDocument() throws Exception {
        String jsonParams = readJsonParamsFile("Enter CAdES signature params JSON file path: ");
        JsonObject jsonObject = parseJsonObject(jsonParams);

        String inputFilePath = requireKey(jsonObject, INPUT_PATH_KEY);

        String pin = readInput("Enter PIN: ");
        String encodedPin = prepareAndEncryptPin(pin);

        SigningContext signingContext = GsonUtils.parseJson(jsonParams, SigningContext.class);
        signingContext.setEncodedPin(encodedPin);

        CardReader cardReader = session.getCardReader();
        ToolkitResponse response = cardReader.cadesSign(signingContext, inputFilePath);

        verifyToolkitResponse(response);

        if (signingContext.getPackagingMode() == PackagingMode.DETACHED) {
            byte[] detachedSignature = response.getDetachedSignature();
            if (detachedSignature != null && detachedSignature.length > 0) {
                System.out.println("Detached Signature: " + Base64.getEncoder().encodeToString(detachedSignature));
            }
        }

        System.out.println("Document signed successfully.");
    }

    /**
     * Verifies an XAdES-signed XML document.
     * Prompts for JSON params file path, parses verification context,
     * calls xadesVerify, and prints the verification report.
     */
    public void verifyXml() throws Exception {
        String jsonParams = readJsonParamsFile("Enter XAdES verification params JSON file path: ");
        JsonObject jsonObject = parseJsonObject(jsonParams);

        String filePath = requireKey(jsonObject, INPUT_PATH_KEY);

        VerificationContext verificationContext = GsonUtils.parseJson(jsonParams, VerificationContext.class);

        CardReader cardReader = session.getCardReader();
        String report = cardReader.xadesVerify(verificationContext, filePath);

        if (report == null || report.isEmpty()) {
            System.out.println("Verification returned no report.");
        }
        else {
            System.out.println("Verification Report: " + report);
        }
    }

    /**
     * Verifies a PAdES-signed PDF document.
     * Prompts for JSON params file path, parses verification context,
     * calls padesVerify, and prints the verification report.
     */
    public void verifyPdf() throws Exception {
        String jsonParams = readJsonParamsFile("Enter PAdES verification params JSON file path: ");
        JsonObject jsonObject = parseJsonObject(jsonParams);

        String filePath = requireKey(jsonObject, INPUT_PATH_KEY);

        VerificationContext verificationContext = GsonUtils.parseJson(jsonParams, VerificationContext.class);

        CardReader cardReader = session.getCardReader();
        String report = cardReader.padesVerify(verificationContext, filePath);

        if (report == null || report.isEmpty()) {
            System.out.println("Verification returned no report.");
        }
        else {
            System.out.println("Verification Report: " + report);
        }
    }

    /**
     * Verifies a CAdES-signed document.
     * Prompts for JSON params file path and the Base64-encoded detached signature,
     * parses verification context, calls cadesVerify, and prints the report.
     */
    public void verifyDocument() throws Exception {
        String jsonParams = readJsonParamsFile("Enter CAdES verification params JSON file path: ");
        JsonObject jsonObject = parseJsonObject(jsonParams);

        String filePath = requireKey(jsonObject, INPUT_PATH_KEY);

        String signatureBase64 = readInput("Enter Base64-encoded signature string: ");
        byte[] signature = Base64.getDecoder().decode(signatureBase64);

        VerificationContext verificationContext = GsonUtils.parseJson(jsonParams, VerificationContext.class);

        CardReader cardReader = session.getCardReader();
        String report = cardReader.cadesVerify(verificationContext, filePath, signature);

        if (report == null || report.isEmpty()) {
            System.out.println("Verification returned no report.");
        }
        else {
            System.out.println("Verification Report: " + report);
        }
    }

    // ------------------------------------------------------------------
    // Internal helpers
    // ------------------------------------------------------------------

    private String readJsonParamsFile(String prompt) throws Exception {
        String path = readInput(prompt);
        if (path.isEmpty()) {
            throw new ToolkitException("JSON params file path must not be empty");
        }

        String absolutePath = new File(path).getAbsolutePath();
        byte[] bytes = Files.readAllBytes(Paths.get(absolutePath));
        return new String(bytes, UTF_8);
    }

    private JsonObject parseJsonObject(String jsonString) {
        return new JsonParser().parse(jsonString).getAsJsonObject();
    }

    private String requireKey(JsonObject json, String key) throws Exception {
        if (!json.has(key)) {
            throw new ToolkitException("Required key '" + key + "' not found in JSON params file");
        }

        return json.get(key).getAsString();
    }

    private void verifyToolkitResponse(ToolkitResponse response) throws ToolkitException {
        if (response == null) {
            return;
        }

        String xmlString = response.toXmlString();
        if (xmlString != null && !xmlString.isEmpty()) {
            if (!XmlHelper.verifySignature(xmlString)) {
                throw new ToolkitException("Response signature validation failed");
            }
        }
    }
}
