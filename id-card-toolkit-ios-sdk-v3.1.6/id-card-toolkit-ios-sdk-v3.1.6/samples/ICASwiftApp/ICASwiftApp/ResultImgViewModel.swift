//
//  ResultImgViewModel.swift
//  Toolkitsample
//
//  Created by Prabhakar Bunga on 12/03/24.
//

import UIKit

class ResultImgViewModel: UIViewController {
    @IBOutlet weak var img1: UIImageView!
    @IBOutlet weak var img1_1: UIImageView!
    @IBOutlet weak var img2: UIImageView!
    var card_public_data: CardPublicData!
    override func viewDidLoad() {
        super.viewDidLoad()

        // Do any additional setup after loading the view.
        let responseDic =  card_public_data.getXmlString()
      //Started paring the xml string
        let data: Data? = responseDic.data(using: .utf8)
        let resultObject = XMLParserHelper.parseXMLString(responseDic)
        
        
        let holderEmiratesIdpdf = XMLParserHelper.getDataAtPath("ValidationGatewayResponse.Message.Body.PublicData.HolderEmiratesIdImage", fromResultObject: resultObject)
        if holderEmiratesIdpdf != nil  {
            let url = Customs.saveBase64StringToPDF(holderEmiratesIdpdf as! String, fileName: "emiratesID")
            if url != "" {
                let emiratesIDUrl = URL(string: url!)
                let imgs = Customs.drawImgsfromURL(url: emiratesIDUrl!)
                self.img1.image = imgs?.first
                self.img1_1.image = imgs?.last
            }
        }
        
        let holderResidencepdf = XMLParserHelper.getDataAtPath("ValidationGatewayResponse.Message.Body.PublicData.HolderResidenceImage", fromResultObject: resultObject)
        if holderResidencepdf != nil  {
            let url = Customs.saveBase64StringToPDF(holderResidencepdf as! String, fileName: "residenceID")
            if url != ""{
                let holderResidenceIDUrl = URL(string: url!)
                self.img2.image = Customs.drawPDFfromURL(url: holderResidenceIDUrl!)
            }
        }
    }
    

    /*
    // MARK: - Navigation

    // In a storyboard-based application, you will often want to do a little preparation before navigation
    override func prepare(for segue: UIStoryboardSegue, sender: Any?) {
        // Get the new view controller using segue.destination.
        // Pass the selected object to the new view controller.
    }
    */
    @IBAction func closeBtnAction(_ sender: Any) {
        self.dismiss(animated: true)
    }
    
}
