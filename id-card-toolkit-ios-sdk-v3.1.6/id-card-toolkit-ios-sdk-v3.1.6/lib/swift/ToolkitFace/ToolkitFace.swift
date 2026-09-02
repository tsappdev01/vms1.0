//
//  ToolkitFace.swift
//  ToolkitFace
//
//  Created by Prabhakar Bunga on 03/05/24.
//

import Foundation

import DotCore
import DotFaceCore
import DotFaceDetectionFast
import DotFaceBackgroundUniformity
import DotFaceExpressionNeutral
import DotCamera

enum FaceCaptureMode {
    case AUTO_CAPTURE, SMILE_LIVENESS, MAGNIFEYE_LIVENESS
}

enum Camerafacing {
    case FRONT_CAMERA, BACK_CAMERA
}

enum CameraFlash {
    case FLASH_ON, FLASH_OFF
}


class ToolkitFace: NSObject, MagnifEyeLivenessViewControllerDelegate {
    
    
    enum ErrorCodes {
        case SUCCESS, ERROR
    }
    
    static let shared = ToolkitFace()
    
    override init() {}
    
    weak var delegate: AuthenticateDelegate?
    
    public var SDK_HANDLE: Int32?
    
    public var _idn: String?
    
    public var face_config: String?
    
    public var _passportNumber: String?
    
    public var _passportCountry: String?
    
    public var _passportDob: String?
    
    public var _passportDoe: String?
    
    public var _withDocs: Bool?
    
    public var withID: Bool = false
    
    private var rawData: String?
    
    var cardPublicData:CardPublicData!
    
    private var activityView: UIActivityIndicatorView!
    
    private var vc: UIViewController?
    
    // MARK: Camera Setting Keys
    public var livenessMode: FaceCaptureMode = .AUTO_CAPTURE
    
    public var cameraMode: Camerafacing = .BACK_CAMERA
    
    public var flashMode: CameraFlash = .FLASH_OFF
    
    // MARK: Authenticate Face with ID
    func authenticateFaceWithID(idNum: String, withDocs: Bool, viewController: UIViewController) {
        
        withID = true
        _idn = idNum
        _withDocs = withDocs
        vc = viewController
        captureFace(viewController: viewController)
    }
    
    // MARK: Authenticate Face with Passport
    func authenticateFaceWithPassport(passportNumber: String, passportCountry: String, passportDoe: String, passportDob: String, withDocs: Bool, viewController: UIViewController) {
        
        withID = false
        _passportNumber = passportNumber
        _passportCountry = passportCountry
        _passportDoe = passportDoe
        _passportDob = passportDob
        _withDocs = withDocs
        vc = viewController
        captureFace(viewController: viewController)
    }
    
    // MARK: Capture Face
    private func captureFace(viewController: UIViewController) {
        
        if(livenessMode == .SMILE_LIVENESS) {
            let smileLivenessConfig = SmileLivenessConfiguration.init(isTorchEnabled: flashMode == .FLASH_OFF ? false : true, cameraFacing: cameraMode == .BACK_CAMERA ? CameraFacing.back : CameraFacing.front)
            let controller = SmileLivenessViewController.create(configuration: smileLivenessConfig, style: .init())
            controller.delegate = self
            controller.modalPresentationStyle = .automatic
            viewController.present(controller, animated: true)
        } else if(livenessMode == .AUTO_CAPTURE) {
            let faceAutoCaptureConfig = FaceAutoCaptureConfiguration.init(isTorchEnabled: flashMode == .FLASH_OFF ? false : true, cameraFacing: cameraMode == .BACK_CAMERA ? CameraFacing.back : CameraFacing.front)
            let controller = FaceAutoCaptureViewController.create(configuration: faceAutoCaptureConfig, style: .init())
            controller.delegate = self
            controller.modalPresentationStyle = .automatic
            viewController.present(controller, animated: true)
        } else if(livenessMode == .MAGNIFEYE_LIVENESS) {
            let magnifEyeLivenessConfig = MagnifEyeLivenessConfiguration.init(isTorchEnabled: flashMode == .FLASH_OFF ? false : true, cameraFacing: cameraMode == .BACK_CAMERA ? CameraFacing.back : CameraFacing.front)
            let controller = MagnifEyeLivenessViewController.create(configuration: magnifEyeLivenessConfig, style: .init())
            controller.delegate = self
            controller.modalPresentationStyle = .automatic
            viewController.present(controller, animated: true)
        }
    }
    
    // MARK: Verify Face
    private func faceVerification(faceImgStr: String) {
        
        DispatchQueue.main.async {
            
            do {
                var handle:Int32
                if(self.SDK_HANDLE == nil) {
                    handle = 0
                } else {
                    handle = self.SDK_HANDLE!
                }
                let liveMode = self.livenessMode == .AUTO_CAPTURE ? 1 : self.livenessMode == .SMILE_LIVENESS ? 2 : 3
                if(self.withID) {
                    self.cardPublicData = try toolkit?.verifyFaceOnServerUsingIDEx(Int32(handle), idn: self._idn ?? "", base64Image: faceImgStr, raw_data: self.rawData ?? "", live_mode: UInt32(liveMode), isdigitalDocs: self._withDocs!)
                } else {
                    self.cardPublicData = try toolkit?.verifyFaceOnServerUsingPassportEx(Int32(handle), passportNumber: self._passportNumber ?? "", passportCountry: self._passportCountry ?? "", passportExpiryDate: self._passportDoe ?? "", dateOfBirth: self._passportDob ?? "", base64Image: faceImgStr, raw_data: self.rawData ?? "", live_mode: UInt32(liveMode), isdigitalDocs: self._withDocs!)
                }
                
                DispatchQueue.main.async {
                    
                    self.dismissActivityIndicatory()
                    print(self.cardPublicData.getStatus())
                    if(self.cardPublicData.getStatus() == "BiometricAuthenticated") {
                        self.delegate?.onAuthenticateFaceResult(status: 1, message: "", cardPublicData: self.cardPublicData)
                    } else {
                        self.delegate?.onAuthenticateFaceResult(status: 0, message: self.cardPublicData.getStatus(), cardPublicData: nil)
                    }
                }
            }  catch let error as NSError{
                
                DispatchQueue.main.async {
                    
                    self.dismissActivityIndicatory()
                    let err="\(error.domain)\n\(error.code)\n\(error.userInfo)"
                    print(err)
                    self.delegate?.onAuthenticateFaceResult(status: 0, message: err, cardPublicData: self.cardPublicData)
                }// update UI main queue
            }//catch
        }
    }
    
    // MARK: Show Loader
    private func showActivityIndicatory() {
        activityView = UIActivityIndicatorView(style: .large)
        activityView.center = vc!.view.center
        vc!.view.addSubview(activityView)
        activityView.startAnimating()
    }
    // MARK: Dismiss Loader
    private func dismissActivityIndicatory() {
        activityView.stopAnimating()
        activityView.removeFromSuperview()
    }
}

extension ToolkitFace: SmileLivenessViewControllerDelegate, FaceAutoCaptureViewControllerDelegate {
    // MARK: DOT SDK CONFIGURATION & INIT
    private func createDotFaceLibraryConfiguration() -> DotFaceLibraryConfiguration {
        return .init(
            modules: [
                DotFaceDetectionFastModule.shared,
                DotFaceBackgroundUniformityModule.shared,
                DotFaceExpressionNeutralModule.shared
            ]
        )
    }
    
    private func createDotSdkConfiguration(license: Data, dotFaceLibraryConfiguration: DotFaceLibraryConfiguration) -> DotSdkConfiguration {
        return DotSdkConfiguration(
            licenseBytes: license,
            libraries: [
                DotFaceLibrary(configuration: dotFaceLibraryConfiguration),
            ]
        )
    }
    
    public func initilizeFaceSDK()-> ErrorCodes {
        if(toolkit == nil) {
            
            print("Toolkit object is null.")
            return ErrorCodes.ERROR
        }
        
        do {
            let faceSDKData = try toolkit?.initFaceSDK(applicationId: Bundle.main.bundleIdentifier!)
            SDK_HANDLE = faceSDKData?.getHandle()
            let face_lic = faceSDKData?.getLicense()
            let lic_data = Data(base64Encoded: face_lic!)
            let dotFaceLibraryConfiguration = createDotFaceLibraryConfiguration()
            let dotSdkConfiguration = createDotSdkConfiguration(license: lic_data!, dotFaceLibraryConfiguration: dotFaceLibraryConfiguration)
            try DotSdk.shared.initialize(configuration: dotSdkConfiguration)
        } catch {
            print("Failed to initialize DotSdk: \(error.localizedDescription)")
            return ErrorCodes.ERROR
        }
        return ErrorCodes.SUCCESS
    }
    
    func smileLivenessViewController(_ viewController: SmileLivenessViewController, finished result: SmileLivenessResult) {
        print(result.description)
        viewController.stopAsync()
        viewController.dismiss(animated: true)
        
        rawData = result.content.base64EncodedString()
        showActivityIndicatory()
        let image = UIImage(cgImage: CGImageFactory.create(bgrRawImage: result.bgrRawImage))
        let faceImgStr = self.convertImageToBase64String(img: image)
        faceVerification(faceImgStr: faceImgStr)
    }
    
    func faceAutoCaptureViewController(_ viewController: DotFaceCore.FaceAutoCaptureViewController, captured result: DotFaceCore.FaceAutoCaptureResult) {
        viewController.stopAsync()
        viewController.dismiss(animated: true)
        rawData = result.content.base64EncodedString()
        print("PassiveLiveness: \(rawData ?? "")")
        showActivityIndicatory()
        let image = UIImage(cgImage: CGImageFactory.create(bgrRawImage: result.bgrRawImage))
        let faceImgStr = self.convertImageToBase64String(img: image)
        print("img_Data: \(faceImgStr)")
        faceVerification(faceImgStr: faceImgStr)
    }
    
    func magnifEyeLivenessViewController(_ viewController: DotFaceCore.MagnifEyeLivenessViewController, finished result: DotFaceCore.MagnifEyeLivenessResult) {
        print(result.description)
        viewController.stopAsync()
        viewController.dismiss(animated: true)
        rawData = result.content.base64EncodedString()
        showActivityIndicatory()
        let image = UIImage(cgImage: CGImageFactory.create(bgrRawImage: result.bgrRawImage))
        let faceImgStr = self.convertImageToBase64String(img: image)
        faceVerification(faceImgStr: faceImgStr)
    }
    
    private func convertImageToBase64String (img: UIImage) -> String {
        let imageData:NSData = img.jpegData(compressionQuality: 0.01)! as NSData
        let imgString = imageData.base64EncodedString(options: .init(rawValue: 0))
        return imgString
    }
    
    func smileLivenessViewControllerViewWillAppear(_ viewController: SmileLivenessViewController) {
        viewController.start()
    }
    
    func faceAutoCaptureViewControllerViewWillAppear(_ viewController: FaceAutoCaptureViewController) {
        viewController.start()
    }
    
    func magnifEyeLivenessViewControllerViewWillAppear(_ viewController: MagnifEyeLivenessViewController) {
        viewController.start()
    }
}

extension ToolkitFace {
    // MARK: Camera Set & Get
    func setCameraMode(mode: Camerafacing) {
        cameraMode = mode
    }
    
    func setLivenessMode(mode: FaceCaptureMode) {
        livenessMode = mode
    }
    
    func setFlash(mode: CameraFlash) {
        flashMode = mode
    }
    
    func getCameraMode()-> Camerafacing {
        return cameraMode
    }
    
    func getLivenessMode()-> FaceCaptureMode {
        return livenessMode
    }
    
    func getFlashMode()-> CameraFlash {
        return flashMode
    }
}


protocol AuthenticateDelegate: AnyObject {
    func onAuthenticateFaceResult(status: Int, message: String, cardPublicData: CardPublicData?)
}
