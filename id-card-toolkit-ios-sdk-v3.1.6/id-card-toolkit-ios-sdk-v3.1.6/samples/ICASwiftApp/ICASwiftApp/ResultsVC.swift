//
//  ResultsVC.swift
//  Toolkitsample
//
//  Created by Prabhakar Bunga on 11/03/24.
//

import UIKit

class ResultsVC: UIViewController {
    
    @IBOutlet weak var scrollView: UITableView!
    @IBOutlet weak var listSuperView: UIView!
    @IBOutlet weak var detailsView: UIView!
    @IBOutlet weak var img_bg_view: UIView!
    @IBOutlet weak var profile_imgView: UIImageView!
    @IBOutlet weak var idNumberLbl: UILabel!
    @IBOutlet weak var cardNumberLbl: UILabel!
    @IBOutlet weak var issuingDateLbl: UILabel!
    @IBOutlet weak var nameLbl: UILabel!
    @IBOutlet weak var nameArabicLbl: UILabel!
    @IBOutlet weak var dobLbl: UILabel!
    @IBOutlet weak var cardExpiryLbl: UILabel!
    @IBOutlet weak var nationalityLbl: UILabel!
    @IBOutlet weak var nationalityArabic: UILabel!
   
    @IBOutlet weak var numberView: UIView!
    @IBOutlet weak var cardNumberView: UIView!
    @IBOutlet weak var issueDateView: UIView!
    @IBOutlet weak var nameView: UIView!
    @IBOutlet weak var nameArabicView: UIView!
    @IBOutlet weak var dobView: UIView!
    @IBOutlet weak var doeView: UIView!
    @IBOutlet weak var nationalityView: UIView!
    @IBOutlet weak var nationalityArabicView: UIView!
    
    @IBOutlet weak var showDocBtn: UIButton!
    
    var card_public_data: CardPublicData!
  
    override func viewDidLoad() {
        super.viewDidLoad()

        
        img_bg_view.layer.cornerRadius = img_bg_view.layer.frame.width/2;
        img_bg_view.layer.masksToBounds = true;
        profile_imgView.layer.cornerRadius = profile_imgView.layer.frame.width/2;
        profile_imgView.layer.masksToBounds = true;
        
        
        // Do any additional setup after loading the view.
        idNumberLbl.text = card_public_data.getIdNumber()
        cardNumberLbl.text = card_public_data.getCardNumber()
        issuingDateLbl.text = card_public_data.getNonModifiablePublicData()?.getIssueDate()
        nameLbl.text = card_public_data.getNonModifiablePublicData()?.getFullNameEnglish().replace(",", with: " ")
        nameArabicLbl.text = card_public_data.getNonModifiablePublicData()?.getFullNameArabic()
        dobLbl.text = card_public_data.getNonModifiablePublicData()?.getDateOfBirth()
        cardExpiryLbl.text = card_public_data.getNonModifiablePublicData()?.getExpiryDate()
        nationalityLbl.text = card_public_data.getNonModifiablePublicData()?.getNationalityEnglish()
        nationalityArabic.text = card_public_data.getNonModifiablePublicData()?.getNationalityArabic()
//        card_public_data.getHolderEmiratesIdImage()
        
        let photoData: NSData = NSData(bytes: card_public_data.getCardHolderPhoto(), length: Int(card_public_data.getCardHolderPhotoLength()))
        let pPhotoImage = UIImage(data: photoData as Data)
        profile_imgView.image = pPhotoImage
        
        showDocBtn.layer.cornerRadius = showDocBtn.layer.frame.width/2
        
        setStyleForView(view: numberView)
        setStyleForView(view: cardNumberView)
        setStyleForView(view: nameView)
        setStyleForView(view: nameArabicView)
        setStyleForView(view: dobView)
        setStyleForView(view: doeView)
        setStyleForView(view: issueDateView)
        setStyleForView(view: nationalityView)
        setStyleForView(view: nationalityArabicView)
    }
    
    func setStyleForView(view: UIView) {
        view.layer.borderWidth = 2
        view.layer.cornerRadius = 10
        view.layer.borderColor = UIColor(named: "cust_blue_color")?.cgColor
    }
    

    /*
    // MARK: - Navigation

    // In a storyboard-based application, you will often want to do a little preparation before navigation
    override func prepare(for segue: UIStoryboardSegue, sender: Any?) {
        // Get the new view controller using segue.destination.
        // Pass the selected object to the new view controller.
    }
    */
    @IBAction func backBtnAction(_ sender: Any) {
        self.dismiss(animated: true)
    }
    
  
    @IBAction func showDocBtnACtion(_ sender: Any) {
        let storyBoard = UIStoryboard(name: "Main", bundle: nil)
        let ResultImgViewModel = storyBoard.instantiateViewController(withIdentifier: "ResultImgViewModel") as! ResultImgViewModel
        ResultImgViewModel.card_public_data = card_public_data
        self.present(ResultImgViewModel, animated:true, completion:nil)
    }
}
