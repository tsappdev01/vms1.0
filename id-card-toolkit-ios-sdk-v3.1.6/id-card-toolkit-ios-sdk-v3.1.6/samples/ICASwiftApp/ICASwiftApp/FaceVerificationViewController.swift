//
//  FaceVerificationViewController.swift
//  ICASwiftApp
//
//  Created by doti naresh on 28/10/22.
//

import UIKit
import IDCardToolkit

class FaceVerificationViewController: UIViewController,UITextFieldDelegate, AuthenticateDelegate {
    
    @IBOutlet var view_FaceAuthantication: UIView!
    @IBOutlet weak var img_User: UIImageView!
    @IBOutlet weak var subView: UIView!
    @IBOutlet weak var cardNumberLabel: UILabel!
    @IBOutlet weak var dobLabel: UILabel!
    @IBOutlet weak var expiryDateLabel: UILabel!
    @IBOutlet weak var passportCountryLabel: UILabel!
    @IBOutlet weak var cardNumberText: UITextField!
    @IBOutlet weak var dobYYText: UITextField!
    @IBOutlet weak var dobMMText: UITextField!
    @IBOutlet weak var dobDDText: UITextField!
    @IBOutlet weak var expiryDateYYText: UITextField!
    @IBOutlet weak var expiryDateMMText: UITextField!
    @IBOutlet weak var expiryDateDDText: UITextField!
    @IBOutlet weak var passportCountryText: UITextField!
    @IBOutlet weak var slash1Label: UILabel!
    @IBOutlet weak var slash2Label: UILabel!
    @IBOutlet weak var slash3Label: UILabel!
    @IBOutlet weak var slash4Label: UILabel!
    
    @IBOutlet weak var verifyFaceWithIDButton: UIButton!
    @IBOutlet weak var verifyFaceWithPassportButton: UIButton!
    @IBOutlet weak var captureFaceButton: UIButton!
    @IBOutlet weak var verifyFaceButton: UIButton!
    
    @IBOutlet var view_popUpInformation: UIView!
    @IBOutlet weak var tf_name_info: UITextField!
    @IBOutlet weak var tf_CardNumber_info: UITextField!
    @IBOutlet weak var tf_IDN_info: UITextField!
    @IBOutlet weak var img_infoCardHolder: UIImageView!
    @IBOutlet weak var img_info_Card_HolderSign: UIImageView!
    @IBOutlet weak var img_Info_ResidentCard: UIImageView!
    @IBOutlet weak var img_Info_EmiratesIDCard: UIImageView!
    @IBOutlet weak var txt_XmlData: UITextView!
    @IBOutlet weak var idnLabel: UILabel!
    
    var isMrzSelected = false
    var isMrzIDCaptured = false
    
    var documentNumber = ""
    var dob_YY = ""
    var dob_MM = ""
    var dob_DD = ""
    var expiryDate_YY = ""
    var expiryDate_MM = ""
    var expiryDate_DD = ""
    var countryCode = ""
    
    var getLivenessImage = UIImage()
    var images: [UIImage] = []
    var isFaceSDKInitilized: Bool = false
    
    
    var isCameraFacingBack: Bool = false
    var isCameraTorchOn: Bool = false
    var isSmileLiveness: Bool = false
    
    override func viewDidLoad() {
        super.viewDidLoad()
        
        txt_XmlData.layer.cornerRadius = 7.5
        txt_XmlData.layer.borderWidth = 2.0
        txt_XmlData.layer.borderColor = UIColor.gray.cgColor
        
        view_popUpInformation.isHidden = true
        view_popUpInformation.frame = CGRect(x: 0, y: 0, width: self.view.frame.size.width, height: self.view.frame.size.height)
        self.view.addSubview(view_popUpInformation)
        captureFaceButton.isHidden = true
        cardNumberText.text = ""
        dobYYText.text = ""
        dobMMText.text = ""
        dobDDText.text = ""
        expiryDateYYText.text = ""
        expiryDateMMText.text = ""
        expiryDateDDText.text = ""
        passportCountryText.text = ""
        
        // Do any additional setup after loading the view.
        
        self.view_FaceAuthantication.isHidden = true
        self.view_FaceAuthantication.frame = CGRect.init(x: 0, y: 0, width: self.view.frame.size.width, height: self.view.frame.size.height)
        self.view.addSubview(view_FaceAuthantication)
        
        self.subView.isHidden = true
        
        self.setAlphaNumeric(cardNumberText)
        self.setNumberPad(dobYYText);
        self.setNumberPad(dobMMText);
        self.setNumberPad(dobDDText);
        self.setNumberPad(expiryDateYYText);
        self.setNumberPad(expiryDateMMText);
        self.setNumberPad(expiryDateDDText);
        self.setAlphaNumeric(passportCountryText)
        
        dobYYText.delegate = self
        dobMMText.delegate = self
        dobDDText.delegate = self
        expiryDateYYText.delegate = self
        expiryDateMMText.delegate = self
        expiryDateDDText.delegate = self
        
        dobYYText.addTarget(self, action: #selector(self.textFieldDidChange(textField:)), for: UIControl.Event.editingChanged)
        dobMMText.addTarget(self, action: #selector(self.textFieldDidChange(textField:)), for: UIControl.Event.editingChanged)
        dobDDText.addTarget(self, action: #selector(self.textFieldDidChange(textField:)), for: UIControl.Event.editingChanged)
        expiryDateYYText.addTarget(self, action: #selector(self.textFieldDidChange(textField:)), for: UIControl.Event.editingChanged)
        expiryDateMMText.addTarget(self, action: #selector(self.textFieldDidChange(textField:)), for: UIControl.Event.editingChanged)
        expiryDateDDText.addTarget(self, action: #selector(self.textFieldDidChange(textField:)), for: UIControl.Event.editingChanged)
        
        if isMrzSelected == true {
            if isMrzIDCaptured  == true {
                cardNumberText.text = documentNumber
                
                self.subView.isHidden = false
                self.dobLabel.isHidden = true
                self.expiryDateLabel.isHidden = true
                self.passportCountryLabel.isHidden = true
                self.dobYYText.isHidden = true
                self.dobMMText.isHidden = true
                self.dobDDText.isHidden = true
                self.expiryDateYYText.isHidden = true
                self.expiryDateMMText.isHidden = true
                self.expiryDateDDText.isHidden = true
                self.slash1Label.isHidden = true
                self.slash2Label.isHidden = true
                self.slash3Label.isHidden = true
                self.slash4Label.isHidden = true
                self.dobLabel.isHidden = true
                self.expiryDateLabel.isHidden = true
                self.passportCountryLabel.isHidden = true
                self.passportCountryText.isHidden = true
                self.cardNumberLabel.text = "ID Number"
                self.cardNumberText.placeholder = "ID Number"
                
                self.verifyFaceWithIDButton.isHidden = true
                self.verifyFaceWithPassportButton.isHidden = true
                captureFaceButton.isHidden = false
            }
            else {
                cardNumberText.text = documentNumber
                dobYYText.text = dob_YY
                dobMMText.text = dob_MM
                dobDDText.text = dob_DD
                expiryDateYYText.text = expiryDate_YY
                expiryDateMMText.text = expiryDate_MM
                expiryDateDDText.text = expiryDate_DD
                passportCountryText.text = countryCode
                
                self.subView.isHidden = false
                self.dobLabel.isHidden = false
                self.expiryDateLabel.isHidden = false
                self.passportCountryLabel.isHidden = false
                self.dobYYText.isHidden = false
                self.dobMMText.isHidden = false
                self.dobDDText.isHidden = false
                self.expiryDateYYText.isHidden = false
                self.expiryDateMMText.isHidden = false
                self.expiryDateDDText.isHidden = false
                self.slash1Label.isHidden = false
                self.slash2Label.isHidden = false
                self.slash3Label.isHidden = false
                self.slash4Label.isHidden = false
                self.dobLabel.isHidden = false
                self.expiryDateLabel.isHidden = false
                self.passportCountryLabel.isHidden = false
                self.passportCountryText.isHidden = false
                self.cardNumberLabel.text = "Passport Number"
                self.cardNumberText.placeholder = "Passport Number"
                
                self.verifyFaceWithIDButton.isHidden = true
                self.verifyFaceWithPassportButton.isHidden = true
                captureFaceButton.isHidden = false
            }
        }

        ToolkitFace.shared.delegate = self
    }
    
    private func cropFaces() {
        guard let image = self.img_User.image else { return }
        //  activityIndicator.startAnimating()
        DispatchQueue.global().async {
            // `type` in this method can be face, barcode or text
            image.detector.crop(type: .face) { result in
                DispatchQueue.main.async { [weak self] in
                    switch result {
                    case .success(let croppedImages):
                        // When the `Vision` successfully find type of object you set and successfuly crops it.
                        self?.images = croppedImages
                        self?.img_User.image = self?.images[0]
                        self!.getLivenessImage = (self?.images[0])!
                        //                         self!.saveImage(image: self!.getLivenessImage, ImageName: "Compressed2.jpeg")
                    case .notFound:
                        // When the image doesn't contain any type of object you did set, `result` will be `.notFound`.
                        print("Not Found")
                    case .failure(let error):
                        // When the any error occured, `result` will be `failure`.
                        print(error.localizedDescription)
                    }
                    // self?.activityIndicator.stopAnimating()
                }
            }
        }
    }
    
    @IBAction func btn_hidePopUpinformation(_ sender: Any) {
        view_popUpInformation.isHidden = true
        
    }
    
    @IBAction func verifyFaceWithIDButtonAction(_ sender: Any) {
        // create the alert
        let alert = UIAlertController(title: "Scan ID Card Back MRZ", message: "Select the option to get Card Details", preferredStyle: UIAlertController.Style.alert)
        
        alert.addAction(UIAlertAction(title: "MRZ Scanning", style: UIAlertAction.Style.default, handler: { action in
            let vc = self.storyboard?.instantiateViewController(withIdentifier: "PassportMRZScanViewController") as! PassportMRZScanViewController
            vc.isCapturedDetailsForFace = true
            vc.isMrzIDCaptured = true
            self.navigationController?.pushViewController(vc, animated: true)
        }))
        
        alert.addAction(UIAlertAction(title: "Manual Entry", style: UIAlertAction.Style.default, handler: { action in
            self.subView.isHidden = false
            
            self.dobLabel.isHidden = true
            self.expiryDateLabel.isHidden = true
            self.passportCountryLabel.isHidden = true
            self.dobYYText.isHidden = true
            self.dobMMText.isHidden = true
            self.dobDDText.isHidden = true
            self.expiryDateYYText.isHidden = true
            self.expiryDateMMText.isHidden = true
            self.expiryDateDDText.isHidden = true
            self.slash1Label.isHidden = true
            self.slash2Label.isHidden = true
            self.slash3Label.isHidden = true
            self.slash4Label.isHidden = true
            self.dobLabel.isHidden = true
            self.expiryDateLabel.isHidden = true
            self.passportCountryLabel.isHidden = true
            self.passportCountryText.isHidden = true
            self.cardNumberLabel.text = "ID Number"
            self.cardNumberText.placeholder = "ID Number"
            
            self.verifyFaceWithIDButton.isHidden = true
            self.verifyFaceWithPassportButton.isHidden = true
            self.isMrzIDCaptured = true
            self.captureFaceButton.isHidden = false
        }))
        
        alert.addAction(UIAlertAction(title: "Cancel", style: UIAlertAction.Style.destructive, handler: nil))
        
        // show the alert
        self.present(alert, animated: true, completion: nil)
    }
    
    @IBAction func verifyFaceWithPassportButtonAction(_ sender: Any) {
        // create the alert
        let alert = UIAlertController(title: "Scan Passport MRZ", message: "Select the option to get Passport Details", preferredStyle: UIAlertController.Style.alert)
        
        alert.addAction(UIAlertAction(title: "MRZ Scanning", style: UIAlertAction.Style.default, handler: { action in
            let vc = self.storyboard?.instantiateViewController(withIdentifier: "PassportMRZScanViewController") as! PassportMRZScanViewController
            vc.isCapturedDetailsForFace = true
            vc.isMrzIDCaptured = false
            self.navigationController?.pushViewController(vc, animated: true)
        }))
        
        alert.addAction(UIAlertAction(title: "Manual Entry", style: UIAlertAction.Style.default, handler: { action in
            self.subView.isHidden = false
            
            self.dobLabel.isHidden = false
            self.expiryDateLabel.isHidden = false
            self.passportCountryLabel.isHidden = false
            self.dobYYText.isHidden = false
            self.dobMMText.isHidden = false
            self.dobDDText.isHidden = false
            self.expiryDateYYText.isHidden = false
            self.expiryDateMMText.isHidden = false
            self.expiryDateDDText.isHidden = false
            self.slash1Label.isHidden = false
            self.slash2Label.isHidden = false
            self.slash3Label.isHidden = false
            self.slash4Label.isHidden = false
            self.dobLabel.isHidden = false
            self.expiryDateLabel.isHidden = false
            self.passportCountryLabel.isHidden = false
            self.passportCountryText.isHidden = false
            self.cardNumberLabel.text = "Passport Number"
            self.cardNumberText.placeholder = "Passport Number"
            
            self.verifyFaceWithIDButton.isHidden = true
            self.verifyFaceWithPassportButton.isHidden = true
            self.captureFaceButton.isHidden = false
        }))
        
        alert.addAction(UIAlertAction(title: "Cancel", style: UIAlertAction.Style.destructive, handler: nil))
        
        // show the alert
        self.present(alert, animated: true, completion: nil)
    }
    
    @IBAction func captureFaceButtonAction(_ sender: Any) {
        print("isMrzIDCaptured \(isMrzIDCaptured)")
        
        if toolkit == nil  {
            self.view.makeToast(Constants.toolkitNotInitilized)
            return
        }//if
        if(isMrzIDCaptured) {
            documentNumber = cardNumberText.text!
        } else {
            documentNumber = cardNumberText.text ?? ""
            dob_YY =  dobYYText.text ?? ""
            dob_MM = dobMMText.text ?? ""
            dob_DD = dobDDText.text ?? ""
            expiryDate_YY = expiryDateYYText.text ?? ""
            expiryDate_MM = expiryDateMMText.text ?? ""
            expiryDate_DD = expiryDateDDText.text ?? ""
            countryCode = passportCountryText.text ?? ""
            
        }
        //utils.showProgressBar(Constants.DeviceConnect, andView: self.view)
        DispatchQueue.global(qos: .background).async {
            do {
                //toolkit?.delegate = self
                DispatchQueue.main.async {
                    self.livenessModeAlert()
                }// update UI main queue
            }//do
            catch let error as NSError{
                DispatchQueue.main.async {
                    utils.dismissProgressBar()
                    let err="\(error.domain)\n\(error.code)\n\(error.userInfo)"
                    AlertView.showAlertTitle(Constants.Alert, withMessage:err,onView: self)
                }// update UI main queue
            }//catch
        }//background queue
    }
    
    @IBAction func cancelFaceAuthantication(_ sender: Any) {
        self.view_FaceAuthantication.isHidden = true
    }
    
    @IBAction func recaptureAction(_ sender: Any) {
        self.view_FaceAuthantication.isHidden = true
    }
    
    @IBAction func verifyFaceButtonAction(_ sender: Any) {
        
        Customs.deleteFile(imageName: "emiratesID", type: ".pdf")
        Customs.deleteFile(imageName: "residenceID", type: ".pdf")
        
        if toolkit == nil  {
            self.view.makeToast(Constants.toolkitNotInitilized)
            return
        }//if
        
        var documentNumber = ""
        var passportcountry = ""
        var expirtyDate = ""
        var dobDate = ""
        
        self.view_FaceAuthantication.isHidden = true
        var imagetoBase64String = String()
        imagetoBase64String = self.convertImageToBase64String(img: getLivenessImage)
        
        //        print("imagetoBase64String \(imagetoBase64String)")
        
        utils.showProgressBar(Constants.FaceValidate, andView: self.view)
        
        if self.isMrzIDCaptured == true {
            documentNumber  = self.cardNumberText.text!
            self.idnLabel.text = "IDN"
        }
        else {
            self.idnLabel.text = "Passport Number"
            documentNumber  = self.cardNumberText.text!
            passportcountry = passportCountryText.text!
            expirtyDate = "\(expiryDateYYText.text!) -\(expiryDateMMText.text!)-\(expiryDateDDText.text!)"
            dobDate = "\(dobYYText.text!)-\(dobMMText.text!)-\(dobDDText.text!)"
            print("expirtyDate \(expirtyDate)")//yyyyMMDD
            print("dobDate \(dobDate)")//yyyyMMDD
        }
        
        DispatchQueue.global(qos: .background).async {
            do {
                if self.isMrzIDCaptured == true { ///ID
                    ///******************************
                    //cardPublicData =  try toolkit?.verifyFaceOnServerUsingID(documentNumber, faceimage: imagetoBase64String, isdigitalDocs: true)
                    //isdigitalDocs is true will receive Digital ID, Residence ID Card in response
                }
                else {//Passport
                    ///*********************
                    //cardPublicData =  try toolkit?.verifyFaceOnServerUsingPassport(documentNumber, passportCountry: passportcountry, passportExpiryDate:"\(expirtyDate)" , dateOfBirth: "\(dobDate)", faceimage: imagetoBase64String, isdigitalDocs: true)
                    //isdigitalDocs is true will receive Digital ID, Residence ID Card in response
                }
                
                DispatchQueue.main.async {
                    utils.dismissProgressBar()
                    let responseDic =  cardPublicData.getXmlString()
                    
                    //Started paring the xml string
                    let data: Data? = responseDic.data(using: .utf8)
                    let resultObject = XMLParserHelper.parseXMLString(responseDic)
                    let responseStatus = XMLParserHelper.getDataAtPath("ValidationGatewayResponse.Message.Body.ResponseStatus", fromResultObject: resultObject)
                    print("responseStatus \(responseStatus)")
                    let MatchStatus = XMLParserHelper.getDataAtPath("ValidationGatewayResponse.Message.Body.MatchStatus", fromResultObject: resultObject)
                    print("MatchStatus \(MatchStatus)")
                    
                    let idn = XMLParserHelper.getDataAtPath("ValidationGatewayResponse.Message.Header.IDNumber", fromResultObject: resultObject)
                    print("idn \(idn)")
                    
                    
                    let fullName = XMLParserHelper.getDataAtPath("ValidationGatewayResponse.Message.Body.PublicData.NonModifiableData.FullNameEnglish", fromResultObject: resultObject)
                    print("fullName \(fullName)")
                    
                    let nationalityEnglish = XMLParserHelper.getDataAtPath("ValidationGatewayResponse.Message.Body.PublicData.NonModifiableData.NationalityEnglish", fromResultObject: resultObject)
                    print("nationalityEnglish \(nationalityEnglish)")
                    
                    let passport = XMLParserHelper.getDataAtPath("ValidationGatewayResponse.Message.Body.PublicData.ModifiableData.PassportNumber", fromResultObject: resultObject)
                    print("passport \(passport)")
                    
                    let cardHolderPhoto = XMLParserHelper.getDataAtPath("ValidationGatewayResponse.Message.Body.PublicData.CardHolderPhoto", fromResultObject: resultObject)
                    //                  print("cardHolderPhoto \(cardHolderPhoto)")
                    
                    let holderSignatureImage = XMLParserHelper.getDataAtPath("ValidationGatewayResponse.Message.Body.PublicData.HolderSignatureImage", fromResultObject: resultObject)
                    //                  print("holderSignatureImage \(holderSignatureImage)")
                    
                    let holderEmiratesIdpdf = XMLParserHelper.getDataAtPath("ValidationGatewayResponse.Message.Body.PublicData.HolderEmiratesIdImage", fromResultObject: resultObject)
                    if holderEmiratesIdpdf != nil  {
                        let url = Customs.saveBase64StringToPDF(holderEmiratesIdpdf as! String, fileName: "emiratesID")
                        if url != ""{
                            let emiratesIDUrl = URL(string: url!)
                            self.img_Info_EmiratesIDCard.image = Customs.drawPDFfromURL(url: emiratesIDUrl!)
                        }
                    }
                    
                    let holderResidencepdf = XMLParserHelper.getDataAtPath("ValidationGatewayResponse.Message.Body.PublicData.HolderResidenceImage", fromResultObject: resultObject)
                    if holderResidencepdf != nil  {
                        let url = Customs.saveBase64StringToPDF(holderResidencepdf as! String, fileName: "residenceID")
                        if url != ""{
                            let holderResidenceIDUrl = URL(string: url!)
                            self.img_Info_ResidentCard.image = Customs.drawPDFfromURL(url: holderResidenceIDUrl!)
                        }
                    }
                    
//                    self.view_popUpInformation.isHidden = false
                    let storyBoard = UIStoryboard(name: "Main", bundle: nil)
                    let ResultsVC = storyBoard.instantiateViewController(withIdentifier: "ResultsVC") as! ResultsVC
                    self.present(ResultsVC, animated:true, completion:nil)
                    
                    self.tf_name_info.text = "\(fullName ?? "")"
                    self.tf_CardNumber_info.text = "\(nationalityEnglish ?? "")"
                    if self.isMrzIDCaptured == true { ///ID
                        self.tf_IDN_info.text = "\(idn ?? "")"
                    }
                    else {
                        self.tf_IDN_info.text = "\(passport ?? "")"
                    }
                    self.txt_XmlData.text = "\(responseDic)"
                    
                    let cardHolder = String(describing: cardHolderPhoto!)
                    var result:Int32
                    let en_cardHolderPhoto = XMLParserHelper.convert(cardHolder)
                    var cardphotoByte: UnsafeMutablePointer<UInt8>?=nil
                    var cardphotoByteLength: UInt32 = 0
                    
                    result = Int32(XMLParserHelper.base64Decode(en_cardHolderPhoto!, length: UInt32(cardHolder.count), decodedData: &cardphotoByte, decodedLength: &cardphotoByteLength))
                    let photoData: NSData = NSData(bytes: cardphotoByte, length: Int(cardphotoByteLength))
                    let pPhotoImage = UIImage(data: photoData as Data)
                    self.img_infoCardHolder.image = pPhotoImage
                    
                    if holderSignatureImage != nil  {
                        let holderSignature = String(describing: holderSignatureImage!)
                        let en_holderSignature = XMLParserHelper.convert(holderSignature)
                        var holderSignatureImageByte: UnsafeMutablePointer<UInt8>?=nil
                        var holderSignatureImageByteLength: UInt32 = 0
                        
                        result = Int32(XMLParserHelper.base64Decode(en_holderSignature!, length: UInt32(holderSignature.count), decodedData: &holderSignatureImageByte, decodedLength: &holderSignatureImageByteLength))
                        
                        let signData: NSData = NSData(bytes: holderSignatureImageByte, length: Int(holderSignatureImageByteLength))
                        let pSignImage = UIImage(data: signData as Data)
                        self.img_info_Card_HolderSign.image = pSignImage
                    }
                    
                    //                  print("responseDic \(responseDic)")
                    //                  AlertView.showAlertTitle(Constants.Alert, withMessage: "\(responseDic)", onView: self)
                }
            }//do
            catch let error as NSError{
                DispatchQueue.main.async {
                    utils.dismissProgressBar()
                    let err="\(error.domain)\n\(error.code)\n\(error.userInfo)"
                    AlertView.showAlertTitle(Constants.Alert, withMessage:err,onView: self)
                }// update UI main queue
            }//catch
        }//background queue
    }
    
    
    func convertImageToBase64String (img: UIImage) -> String {
        let imageData:NSData = img.jpegData(compressionQuality: 0.01)! as NSData //UIImagePNGRepresentation(img)
        let imgString = imageData.base64EncodedString(options: .init(rawValue: 0))
        return imgString
    }
    
    func convertBase64StringToImage(_ base64String: String) -> UIImage? {
        guard let imageData = Data(base64Encoded: base64String) else { return nil }
        return UIImage(data: imageData)
    }
    
    func faceCaptureSuccess(_ image: UIImage) {
        DispatchQueue.main.async {
            
            if self.isMrzIDCaptured == true {
                self.verifyFaceButton.setTitle("Verify Face with ID", for: .normal)
            }
            else {
                self.verifyFaceButton.setTitle("Verify Face with Passport", for: .normal)
            }
            
            self.view_FaceAuthantication.isHidden = false
            self.img_User.image = image
            print("Found Image")
            
            
            var compressedImage = UIImage()
            if let imageData = self.img_User.image!.jpeg(.lowest) {
                compressedImage  = UIImage(data: imageData)!
                self.img_User.image = compressedImage
                //                self.saveImage(image: compressedImage, ImageName: "Compressed1.jpeg")
                self.cropFaces()
            }
        }
    }
    
    func faceCaptureFailed(_ error: String) {
        DispatchQueue.main.async {
            self.view_FaceAuthantication.isHidden = true
            AlertView.showAlertTitle(Constants.Alert, withMessage:error,onView: self)
        }
    }
    
    func faceCaptureHandlerCancel(_ error: String) {
        DispatchQueue.main.async {
            self.view_FaceAuthantication.isHidden = true
            AlertView.showAlertTitle(Constants.Alert, withMessage:error,onView: self)
        }
    }
    
    @IBAction func btn_BackAction(_ sender: Any) {
        navigationController?.popViewController(animated: true)
    }
    
    func setNumberPad(_ customtextfiled:UITextField) {
        customtextfiled.keyboardType = .numberPad
        customtextfiled.keyboardAppearance = .light
        let confirmPinNumbertoolbar = UIToolbar(frame: CGRect(x: CGFloat(0), y: CGFloat(0), width: CGFloat(320), height: CGFloat(50)))
        confirmPinNumbertoolbar.barStyle = .blackTranslucent
        confirmPinNumbertoolbar.items = [UIBarButtonItem(title: "Cancel", style: .done, target: self, action: #selector(doneWithNumberPad)), UIBarButtonItem(barButtonSystemItem: .flexibleSpace, target: nil, action: nil), UIBarButtonItem(title: "Done", style: .done, target: self, action: #selector(doneWithNumberPad))]
        customtextfiled.inputAccessoryView = confirmPinNumbertoolbar
    }
    
    func setAlphaNumeric(_ customtextfiled:UITextField) {
        
        customtextfiled.keyboardType = .namePhonePad
        customtextfiled.keyboardAppearance = .light
        let confirmPinNumbertoolbar = UIToolbar(frame: CGRect(x: CGFloat(0), y: CGFloat(0), width: CGFloat(320), height: CGFloat(50)))
        confirmPinNumbertoolbar.barStyle = .blackTranslucent
        confirmPinNumbertoolbar.items = [UIBarButtonItem(title: "Cancel", style: .done, target: self, action: #selector(doneWithNumberPad)), UIBarButtonItem(barButtonSystemItem: .flexibleSpace, target: nil, action: nil), UIBarButtonItem(title: "Done", style: .done, target: self, action: #selector(doneWithNumberPad))]
        customtextfiled.inputAccessoryView = confirmPinNumbertoolbar
    }
    
    @objc func doneWithNumberPad() {
        self.dobYYText.resignFirstResponder()
        self.dobMMText.resignFirstResponder()
        self.dobDDText.resignFirstResponder()
        self.expiryDateYYText.resignFirstResponder()
        self.expiryDateMMText.resignFirstResponder()
        self.expiryDateDDText.resignFirstResponder()
        self.cardNumberText.resignFirstResponder()
        self.passportCountryText.resignFirstResponder()
    }
    
    @objc func textFieldDidChange(textField: UITextField){
        let text = textField.text
        if  text?.count == 2 {
            switch textField{
                
            case dobMMText:
                dobDDText.becomeFirstResponder()
                
            case dobDDText:
                expiryDateYYText.becomeFirstResponder()
                
            case expiryDateYYText:
                expiryDateMMText.becomeFirstResponder()
                
            case expiryDateMMText:
                expiryDateDDText.becomeFirstResponder()
                
            case expiryDateDDText:
                expiryDateDDText.resignFirstResponder()
            default:
                break
            }
        }
        if  text?.count == 0 {
            switch textField{
                
            case dobYYText:
                dobYYText.becomeFirstResponder()
                
            case dobMMText:
                dobYYText.becomeFirstResponder()
                
            case dobDDText:
                dobMMText.becomeFirstResponder()
                
            case expiryDateYYText:
                dobDDText.becomeFirstResponder()
                
            case expiryDateMMText:
                expiryDateYYText.becomeFirstResponder()
                
            case expiryDateDDText:
                expiryDateMMText.becomeFirstResponder()
                
            default:
                break
            }
        }
        else{
        }
    }
    
}

extension FaceVerificationViewController {
    
    func faceVerification() {
        DispatchQueue.main.async {
                if(self.isMrzIDCaptured) {
                    print(self.documentNumber)
                    ToolkitFace.shared.authenticateFaceWithID(idNum: self.documentNumber, withDocs: false, viewController: self)
                } else {
                    print("\(self.documentNumber), passportCountry: \(self.countryCode), passportExpiryDate: \(self.expiryDate_YY)-\(self.expiryDate_MM)-\(self.expiryDate_DD), dateOfBirth: \(self.dob_YY)-\(self.dob_MM)-\(self.dob_DD)")
                    ToolkitFace.shared.authenticateFaceWithPassport(passportNumber: self.documentNumber, passportCountry: self.countryCode, passportDoe: "\(self.expiryDate_YY)-\(self.expiryDate_MM)-\(self.expiryDate_DD)", passportDob: "\(self.dob_YY)-\(self.dob_MM)-\(self.dob_DD)", withDocs: false, viewController: self)
                }
            }
    }
    
    func onAuthenticateFaceResult(status: Int, message: String, cardPublicData: IDCardToolkit.CardPublicData?) {
        if(cardPublicData == nil) {
            AlertView.showAlertTitle(Constants.Alert, withMessage: message,onView: self)
            return
        }
        DispatchQueue.main.async {
            do {
                DispatchQueue.main.async {
                    let responseDic =  cardPublicData!.getXmlString()
                    //Started paring the xml string
                    let data: Data? = responseDic.data(using: .utf8)
                    let resultObject = XMLParserHelper.parseXMLString(responseDic)
                    
                    let responseStatus = XMLParserHelper.getDataAtPath("ValidationGatewayResponse.Message.Body.ResponseStatus", fromResultObject: resultObject)
                    print("responseStatus \(responseStatus)")
                    
                    let MatchStatus = XMLParserHelper.getDataAtPath("ValidationGatewayResponse.Message.Body.MatchStatus", fromResultObject: resultObject)
                    print("MatchStatus \(MatchStatus)")
                    print("resultObject \(resultObject)")
                    
                    let mStatus = MatchStatus as? String
                    if(mStatus != nil) {
                        if(mStatus!.contains("LowScoreMatch")){
                            AlertView.showAlertTitle(Constants.Alert, withMessage:"LowScoreMatch",onView: self)
                            return
                        }
                    }
                    
                    let idn = XMLParserHelper.getDataAtPath("ValidationGatewayResponse.Message.Header.IDNumber", fromResultObject: resultObject)
                    print("idn \(idn)")
                    
                    
                    let fullName = XMLParserHelper.getDataAtPath("ValidationGatewayResponse.Message.Body.PublicData.NonModifiableData.FullNameEnglish", fromResultObject: resultObject)
                    print("fullName \(fullName)")
                    
                    let nationalityEnglish = XMLParserHelper.getDataAtPath("ValidationGatewayResponse.Message.Body.PublicData.NonModifiableData.NationalityEnglish", fromResultObject: resultObject)
                    print("nationalityEnglish \(nationalityEnglish)")
                    
                    let passport = XMLParserHelper.getDataAtPath("ValidationGatewayResponse.Message.Body.PublicData.ModifiableData.PassportNumber", fromResultObject: resultObject)
                    print("passport \(passport)")
                    
                    let cardHolderPhoto = XMLParserHelper.getDataAtPath("ValidationGatewayResponse.Message.Body.PublicData.CardHolderPhoto", fromResultObject: resultObject)
                    //                  print("cardHolderPhoto \(cardHolderPhoto)")
                    
                    let holderSignatureImage = XMLParserHelper.getDataAtPath("ValidationGatewayResponse.Message.Body.PublicData.HolderSignatureImage", fromResultObject: resultObject)
                    //                  print("holderSignatureImage \(holderSignatureImage)")
                    
                    let holderEmiratesIdpdf = XMLParserHelper.getDataAtPath("ValidationGatewayResponse.Message.Body.PublicData.HolderEmiratesIdImage", fromResultObject: resultObject)
                    if holderEmiratesIdpdf != nil  {
                        let url = Customs.saveBase64StringToPDF(holderEmiratesIdpdf as! String, fileName: "emiratesID")
                        if url != ""{
                            let emiratesIDUrl = URL(string: url!)
                            self.img_Info_EmiratesIDCard.image = Customs.drawPDFfromURL(url: emiratesIDUrl!)
                        }
                    }
                    
                    let holderResidencepdf = XMLParserHelper.getDataAtPath("ValidationGatewayResponse.Message.Body.PublicData.HolderResidenceImage", fromResultObject: resultObject)
                    if holderResidencepdf != nil  {
                        let url = Customs.saveBase64StringToPDF(holderResidencepdf as! String, fileName: "residenceID")
                        if url != ""{
                            let holderResidenceIDUrl = URL(string: url!)
                            self.img_Info_ResidentCard.image = Customs.drawPDFfromURL(url: holderResidenceIDUrl!)
                        }
                    }
                    
//                    self.view_popUpInformation.isHidden = false
                    DispatchQueue.main.asyncAfter(deadline: .now() + 0.5) {
                        // your code here
                        let storyBoard = UIStoryboard(name: "Main", bundle: nil)
                        let ResultsVC = storyBoard.instantiateViewController(withIdentifier: "ResultsVC") as! ResultsVC
                        ResultsVC.card_public_data = cardPublicData
                        self.present(ResultsVC, animated: true)
                    }

                    self.tf_name_info.text = "\(fullName ?? "")"
                    self.tf_CardNumber_info.text = "\(nationalityEnglish ?? "")"
                    if self.isMrzIDCaptured == true { ///ID
                        self.tf_IDN_info.text = "\(idn ?? "")"
                    }
                    else {
                        self.tf_IDN_info.text = "\(passport ?? "")"
                    }
                    self.txt_XmlData.text = "\(responseDic)"
                    
                    let cardHolder = String(describing: cardHolderPhoto!)
                    var result:Int32
                    let en_cardHolderPhoto = XMLParserHelper.convert(cardHolder)
                    var cardphotoByte: UnsafeMutablePointer<UInt8>?=nil
                    var cardphotoByteLength: UInt32 = 0
                    
                    result = Int32(XMLParserHelper.base64Decode(en_cardHolderPhoto!, length: UInt32(cardHolder.count), decodedData: &cardphotoByte, decodedLength: &cardphotoByteLength))
                    let photoData: NSData = NSData(bytes: cardphotoByte, length: Int(cardphotoByteLength))
                    let pPhotoImage = UIImage(data: photoData as Data)
                    self.img_infoCardHolder.image = pPhotoImage
                    
                    if holderSignatureImage != nil  {
                        let holderSignature = String(describing: holderSignatureImage!)
                        let en_holderSignature = XMLParserHelper.convert(holderSignature)
                        var holderSignatureImageByte: UnsafeMutablePointer<UInt8>?=nil
                        var holderSignatureImageByteLength: UInt32 = 0
                        
                        result = Int32(XMLParserHelper.base64Decode(en_holderSignature!, length: UInt32(holderSignature.count), decodedData: &holderSignatureImageByte, decodedLength: &holderSignatureImageByteLength))
                        
                        let signData: NSData = NSData(bytes: holderSignatureImageByte, length: Int(holderSignatureImageByteLength))
                        let pSignImage = UIImage(data: signData as Data)
                        self.img_info_Card_HolderSign.image = pSignImage
                    }
                }
            }//do
            catch let error as NSError{
                DispatchQueue.main.async {
                    utils.dismissProgressBar()
                    let err="\(error.domain)\n\(error.code)\n\(error.userInfo)"
                    AlertView.showAlertTitle(Constants.Alert, withMessage:err,onView: self)
                }// update UI main queue
            }//catch
        }
    }
}


extension FaceVerificationViewController {
    func cameraFacingAlert() {
        let alert = UIAlertController(title: "Camera Facing", message: "Select the camera", preferredStyle: .alert)
        alert.addAction(UIAlertAction(title: "Front Camera", style: .default, handler: { action in
            ToolkitFace.shared.setCameraMode(mode: .FRONT_CAMERA)
            self.faceVerification()
        }))
        alert.addAction(UIAlertAction(title: "Back Camera", style: .default, handler: { action in
            ToolkitFace.shared.setCameraMode(mode: .BACK_CAMERA)
            self.cameraTorchAlert()
        }))
        alert.addAction(UIAlertAction(title: "Cancel", style: .cancel, handler: { action in
            
        }))
        self.present(alert, animated: true, completion: nil)
    }
    func cameraTorchAlert() {
        let alert = UIAlertController(title: "Back Camera Flash", message: "Select the flash mode", preferredStyle: .alert)
        alert.addAction(UIAlertAction(title: "ON", style: .default, handler: { action in
            ToolkitFace.shared.setFlash(mode: .FLASH_ON)
            self.faceVerification()
        }))
        alert.addAction(UIAlertAction(title: "OFF", style: .default, handler: { action in
            ToolkitFace.shared.setFlash(mode: .FLASH_OFF)
            self.faceVerification()
        }))
        alert.addAction(UIAlertAction(title: "Cancel", style: .cancel, handler: { action in
            
        }))
        self.present(alert, animated: true, completion: nil)
    }
    
    func livenessModeAlert() {
        let alert = UIAlertController(title: "Liveness Mode", message: "Select the mode of capture", preferredStyle: .alert)
        alert.addAction(UIAlertAction(title: "SmileLiveness capture", style: .default, handler: { action in
            ToolkitFace.shared.setLivenessMode(mode: .SMILE_LIVENESS)
            self.cameraFacingAlert()
        }))
        alert.addAction(UIAlertAction(title: "Face Auto Capture", style: .default, handler: { action in
            ToolkitFace.shared.setLivenessMode(mode: .AUTO_CAPTURE)
            self.cameraFacingAlert()
        }))
        alert.addAction(UIAlertAction(title: "MagnifEye Liveness Capture", style: .default, handler: { action in
            ToolkitFace.shared.setLivenessMode(mode: .MAGNIFEYE_LIVENESS)
            self.cameraFacingAlert()
        }))
        alert.addAction(UIAlertAction(title: "Cancel", style: .cancel, handler: { action in
            
        }))
        self.present(alert, animated: true, completion: nil)
    }
}
