//
//  FaceAuthenticationViewController.swift
//  ICASwiftApp
//
//  Created by Digital Trust Tech on 25/04/22.
//

import UIKit
import CoreNFC
import DotCore
import DotFaceCore


class FaceAuthenticationViewController: UIViewController,NFCTagReaderSessionDelegate {

    
    
    @IBOutlet var view_FaceAuthantication: UIView!
    @IBOutlet weak var img_User: UIImageView!
    
    @IBOutlet var view_popUpInformation: UIView!
    @IBOutlet weak var tf_name_info: UITextField!
    @IBOutlet weak var tf_CardNumber_info: UITextField!
    @IBOutlet weak var tf_IDN_info: UITextField!
    @IBOutlet weak var img_infoCardHolder: UIImageView!
    @IBOutlet weak var img_info_Card_HolderSign: UIImageView!
    @IBOutlet weak var img_Info_ResidentCard: UIImageView!
    @IBOutlet weak var img_Info_EmiratesIDCard: UIImageView!
    @IBOutlet weak var txt_XmlData: UITextView!
    
    var getLivenessImage = UIImage()
    var session: NFCTagReaderSession?
    var images: [UIImage] = []
    
    override func viewDidLoad() {
        super.viewDidLoad()
        
        txt_XmlData.layer.cornerRadius = 7.5
        txt_XmlData.layer.borderWidth = 2.0
        txt_XmlData.layer.borderColor = UIColor.gray.cgColor
   
        view_popUpInformation.isHidden = true
        view_popUpInformation.frame = CGRect(x: 0, y: 0, width: self.view.frame.size.width, height: self.view.frame.size.height)
        self.view.addSubview(view_popUpInformation)

        // Do any additional setup after loading the view.
        
        self.view_FaceAuthantication.isHidden = true
        self.view_FaceAuthantication.frame = CGRect.init(x: 0, y: 0, width: self.view.frame.size.width, height: self.view.frame.size.height)
        self.view.addSubview(view_FaceAuthantication)
        
    }
        
    @IBAction func btn_hidePopUpinformation(_ sender: Any) {
        view_popUpInformation.isHidden = true
    }
        
    
    @IBAction func captureFacewithCardreaderButtonAction(_ sender: Any) {
        
//        if toolkit == nil  {
//            self.view.makeToast(Constants.toolkitNotInitilized)
//            return
//        }//if
//        else if cardreader == nil {
//            self.view.makeToast(Constants.cardreaderNotConnect)
//            return
//        }//else if
//
//        utils.showProgressBar(Constants.DeviceConnect, andView: self.view)
//        DispatchQueue.global(qos: .background).async {
//        do {
//            cardreader?.delegate = self
//             try cardreader?.connect()
//            DispatchQueue.main.async {
//                self.validateCardAndCaptureFace()
//              }// update UI main queue
//            }//do
//           catch let error as NSError{
//              DispatchQueue.main.async {
//                  utils.dismissProgressBar()
//                  let err="\(error.domain)\n\(error.code)\n\(error.userInfo)"
//                  AlertView.showAlertTitle(Constants.Alert, withMessage:err,onView: self)
//                }// update UI main queue
//             }//catch
//          }//background queue
        validateCardAndCaptureFace()
    }
   
    
    func validateCardAndCaptureFace()  {
        DispatchQueue.main.async {
            if(DotSdk.shared.isInitialized) {
                let controller = FaceAutoCaptureViewController.create(configuration: .init(), style: .init())
                controller.modalPresentationStyle = .fullScreen
                controller.delegate = self
                //self.navigationController?.pushViewController(controller, animated: true)
                self.present(controller, animated: true)
            } else {
//                if let scene = UIApplication.shared.connectedScenes.first?.delegate as? SceneDelegate {
//                   // to do
//                    scene.initializeDotSdk()
//                }
                self.view.makeToast("Face SDK is not initialized.")
            }
        }
//        DispatchQueue.global(qos: .background).async {
//            do {
//                let requestId = Utils.generateSecureKey()
//                let controller = try cardreader?.validateCardAndCaptureFace(requestId!)
//                
//                DispatchQueue.main.async {
//                if let controllerData = controller {
//                    print("controller \(controllerData)")
//                    
//                    guard controllerData as? UIViewController != nil  else {
//                        return;
//                    }
//                    DispatchQueue.main.async {
//                        utils.dismissProgressBar()
//                        self.present(controllerData as! UIViewController, animated: true, completion: nil)
//                    }
//                }
//                else {
//                    print("Called nil2")
//                }
//              }
//            }
//            catch let error as NSError{
//                DispatchQueue.main.async {
//                    utils.dismissProgressBar()
//                    let err="\(error.domain)\n\(error.code)\n\(error.userInfo)"
//                    AlertView.showAlertTitle(Constants.Alert, withMessage:err,onView: self)
//                }
//            }//catch
//        }//background
        
    }
    
    @IBAction func captureFaceWithNFC(_ sender: Any) {
        if toolkit == nil {
            self.view.makeToast(Constants.toolkitNotInitilized)
            return
        }//if
        
        if #available(iOS 13.0, *) {
            self.session = NFCTagReaderSession(pollingOption: .iso14443, delegate: self, queue: nil)
            self.session?.alertMessage = "Hold your iPhone near an NFC."
            self.session?.begin()
        } else {
            // Fallback on earlier versions
            AlertView.showAlertTitle(Constants.Alert, withMessage:Constants.NFC_Not_Support,onView: self)
        }
    }
    
    @available(iOS 13.0, *)
     func tagReaderSessionDidBecomeActive(_ session: NFCTagReaderSession) {
         print("tagReaderSessionDidBecomeActive")
         if toolkit == nil {
             DispatchQueue.main.async {
                 self.view.makeToast(Constants.toolkitNotInitilized)
                 session.invalidate()
             }
             return
         }//if
     }
    
     @available(iOS 13.0, *)
     func tagReaderSession(_ session: NFCTagReaderSession, didInvalidateWithError error: Error) {
         print("readerSession:didInvalidateWithError: \(error.localizedDescription)")
         AlertView.showAlertTitle(Constants.Alert, withMessage:error.localizedDescription,onView: self)
     }
     
     @available(iOS 13.0, *)
     func tagReaderSession(_ session: NFCTagReaderSession, didDetect tags: [NFCTag]) {
         print("readerSession:didDetectTags")
         
         if tags.count > 1 {
             let retryInterval = DispatchTimeInterval.milliseconds(500)
             session.alertMessage = "More than 1 tag is detected, please try again"
             DispatchQueue.global().asyncAfter(deadline: .now() + retryInterval, execute: {
                 session.restartPolling()
             })
             return
         }
         
   
         let firstTag = tags.first!
         print("firstTag \(firstTag)")
         
         guard case .iso7816(let iso7816Tag) = firstTag else {
             let retryInterval = DispatchTimeInterval.milliseconds(500)
             session.alertMessage = "A tag that is not iso7816 is detected, please try again with tag iso7816."
             DispatchQueue.global().asyncAfter(deadline: .now() + retryInterval, execute: {
                 session.restartPolling()
             })
             return
         }
         
         print("iso7816Tag identifier \(iso7816Tag.identifier)" )
         print("iso7816Tag initialSelectedAID \(iso7816Tag.initialSelectedAID)" )
         print("iso7816Tag historicalBytes \(iso7816Tag.historicalBytes)" )
         print("iso7816Tag applicationData \(iso7816Tag.applicationData)" )

         if toolkit == nil {
             self.view.makeToast(Constants.toolkitNotInitilized)
             session.invalidate()
             return
         }//if
         
         print("session \(session)")
         print("iso7816Tag \(iso7816Tag)")
         let requestId = Utils.generateSecureKey()
         
       DispatchQueue.global(qos: .background).async {
             
                do {
                 try toolkit?.setNfcTag(session, tag: iso7816Tag)
                 
                 cardreader = try toolkit?.getReaderWithEmiratesId()
                 
                 if cardreader == nil {
                     session.invalidate()
                     return
                 }//if
                 
//                 if !(cardreader?.isConnected())! {
//                     cardreader?.delegate = self
//                     try cardreader?.connect()
//                 }
                                  

             DispatchQueue.main.async {
                 session.alertMessage = "Reading Public Details from card"
             }// update UI main queue
                 
//                    let requestId = Utils.generateSecureKey()
//                    let controller = try cardreader?.validateCardAndCaptureFace(requestId!)
//                    
//                    DispatchQueue.main.async {
//                    if let controllerData = controller {
//                        print("controller \(controllerData)")
//                        
//                        guard controllerData as? UIViewController != nil  else {
//                            return;
//                        }
//                        DispatchQueue.main.async {
//                            utils.dismissProgressBar()
//                            self.present(controllerData as! UIViewController, animated: true, completion: nil)
//                        }
//                    }
//                    else {
//                        print("Called nil2")
//                    }
//                  }
                    
                    self.validateCardAndCaptureFace()

                    DispatchQueue.main.async {
                 session.alertMessage = "Reading Completed, session going to close"
                 session.invalidate()
              }// update UI main queue
                                 
              }//do
         catch let error as NSError{
             DispatchQueue.main.async {
                 utils.dismissProgressBar()
                 session.invalidate()
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

    @IBAction func ValidateFaceOnServerButtonAction(_ sender: Any) {
        
        Customs.deleteFile(imageName: "emiratesID", type: ".pdf")
        Customs.deleteFile(imageName: "residenceID", type: ".pdf")
        
        if toolkit == nil  {
            self.view.makeToast(Constants.toolkitNotInitilized)
            return
        }//if
        else if cardreader == nil {
            self.view.makeToast(Constants.cardreaderNotConnect)
            return
        }//else if

        self.view_FaceAuthantication.isHidden = true
        var imagetoBase64String = String()
        imagetoBase64String = self.convertImageToBase64String(img: getLivenessImage)
        
//         self.saveImage(image: getLivenessImage, ImageName: "ActualImage.jpeg")
//         self.saveImage(image: compressedImage, ImageName: "Compressed3.jpeg")
//        self.debugImage(imageBase64: imagetoBase64String)

        utils.showProgressBar(Constants.FaceValidate, andView: self.view)
        DispatchQueue.global(qos: .background).async {
        do {
            //**************************************
            //response =  try cardreader?.validateFaceOnServer(imagetoBase64String, isdigitalDocs: true)//isdigitalDocs is true will receive Digital ID, Residence ID Card in response
//              DispatchQueue.main.async {
//                  utils.dismissProgressBar()
//                  let responseDic =  response.getXmlString()
//                  print("responseDic \(responseDic)")
////                  let responseDic = response.getResponseDataElement()
////                  let matchStr:String = responseDic["MatchStatus"] as! String
////                  let responseStatusStr:String = responseDic["ResponseStatus"] as! String
////                  AlertView.showAlertTitle(Constants.Alert, withMessage: "\(matchStr) \(responseStatusStr)", onView: self)
//               }
            
              DispatchQueue.main.async {
                  utils.dismissProgressBar()
                  let responseDic =  response.getXmlString()
                  
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

                  let cardHolderPhoto = XMLParserHelper.getDataAtPath("ValidationGatewayResponse.Message.Body.PublicData.CardHolderPhoto", fromResultObject: resultObject)
                  print("cardHolderPhoto \(cardHolderPhoto)")
                  
                  let holderSignatureImage = XMLParserHelper.getDataAtPath("ValidationGatewayResponse.Message.Body.PublicData.HolderSignatureImage", fromResultObject: resultObject)
                  print("holderSignatureImage \(holderSignatureImage)")

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
                  
                  self.view_popUpInformation.isHidden = false
                 
                  self.tf_name_info.text = "\(fullName ?? "")"
                  self.tf_CardNumber_info.text = "\(nationalityEnglish ?? "")"
                  self.tf_IDN_info.text = "\(idn ?? "")"
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
                  
                  let holderSignature = String(describing: holderSignatureImage!)
                  let en_holderSignature = XMLParserHelper.convert(holderSignature)
                  var holderSignatureImageByte: UnsafeMutablePointer<UInt8>?=nil
                  var holderSignatureImageByteLength: UInt32 = 0
                  
                  result = Int32(XMLParserHelper.base64Decode(en_holderSignature!, length: UInt32(holderSignature.count), decodedData: &holderSignatureImageByte, decodedLength: &holderSignatureImageByteLength))
                 
                  let signData: NSData = NSData(bytes: holderSignatureImageByte, length: Int(holderSignatureImageByteLength))
                  let pSignImage = UIImage(data: signData as Data)
                  self.img_info_Card_HolderSign.image = pSignImage
                  
                  
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
    
     func saveImage(image: UIImage, ImageName:String) -> Bool {
        
//        guard let data = image.jpegData(compressionQuality: 1) ?? image.pngData() else {
//            return false
//        }
         guard let data = image.jpegData(compressionQuality: 0.50) else {
            return false
        }
        guard let directory = try? FileManager.default.url(for: .documentDirectory, in: .userDomainMask, appropriateFor: nil, create: false) as NSURL else {
            return false
        }
        do {
            try data.write(to: directory.appendingPathComponent(ImageName)!)
            print("Saved Image")
            return true
        } catch {
            print(error.localizedDescription)
            return false
        }
    }
    
    func debugImage(imageBase64:String) {
        let documentsDirectory = NSSearchPathForDirectoriesInDomains(.documentDirectory, .userDomainMask, true)[0]
        Loggly.logger.directory = documentsDirectory
        Loggly.logger.enableEmojis = false
        Loggly.logger.logFormatType = LogFormatType.Normal
        
        Loggly.logger.logEncodingType = String.Encoding.utf8;
    
        loggly(LogType.Debug, text: imageBase64)
}
    
    @IBAction func btn_BackAction(_ sender: Any) {
        navigationController?.popViewController(animated: true)
    }
    
    func getImage (named name : String) -> UIImage? {
        if let imgPath = Bundle.main.path(forResource: name, ofType: ".png") {
            return UIImage(contentsOfFile: imgPath)
        }
        return nil
    }

}

extension UIImage {
    enum JPEGQuality: CGFloat {
        case lowest  = 0
        case low     = 0.25
        case medium  = 0.5
        case high    = 0.75
        case highest = 1
    }

    /// Returns the data for the specified image in JPEG format.
    /// If the image object’s underlying image data has been purged, calling this function forces that data to be reloaded into memory.
    /// - returns: A data object containing the JPEG data, or nil if there was a problem generating the data. This function may return nil if the image has no data or if the underlying CGImageRef contains data in an unsupported bitmap format.
    func jpeg(_ jpegQuality: JPEGQuality) -> Data? {
        return jpegData(compressionQuality: jpegQuality.rawValue)
    }
}

extension FaceAuthenticationViewController: FaceAutoCaptureViewControllerDelegate {
    
    func faceAutoCaptureViewController(_ viewController: FaceAutoCaptureViewController, captured result: FaceAutoCaptureResult) {
        Task {
            guard let detectedFace = result.detectedFace else {
                //presentErrorAlert(NoFaceDetectedError())
                return
            }
            do {
                let faceAutoCaptureSampleResult = try await DetectedFaceEvaluator().evaluate(detectedFace)
                print( self.convertImageToBase64String(img: faceAutoCaptureSampleResult.image))
                self.getLivenessImage = faceAutoCaptureSampleResult.image
               // navigateToResultViewController(faceAutoCaptureSampleResult)
                viewController.stopAsync()
            } catch {
              //  presentErrorAlert(error)
            }
        }
    }
        
    func faceAutoCaptureViewControllerViewWillAppear(_ viewController: FaceAutoCaptureViewController) {
        viewController.start()
    }
    
    func faceAutoCaptureViewControllerStopped(_ viewController: FaceAutoCaptureViewController) {
        print("process is stopped")
        viewController.dismiss(animated: true)
//        do{
//            try DotSdk.shared.deinitialize()
//        } catch {
//            print("Failed to DE-initialize DotSdk: \(error.localizedDescription)")
//        }
    }
}
