//
//  CustomTableViewCell.swift
//  ICASwiftApp
//
//  Created by Digital Trust Tech on 05/05/22.
//

import UIKit

class CustomTableViewCell: UITableViewCell {
    
    @IBOutlet var keyData: UILabel!
    @IBOutlet var valueData: UILabel!
    override func awakeFromNib() {
        super.awakeFromNib()
        // Initialization code
    }

    override func setSelected(_ selected: Bool, animated: Bool) {
        super.setSelected(selected, animated: animated)

        // Configure the view for the selected state
    }

}
