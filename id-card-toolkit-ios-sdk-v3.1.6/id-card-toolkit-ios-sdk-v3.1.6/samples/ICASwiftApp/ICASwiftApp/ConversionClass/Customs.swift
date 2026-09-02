//
//  Customs.swift
//  ICASwiftApp
//
//  Created by doti naresh on 09/03/23.
//

import UIKit

class Customs: NSObject {
    
    class func saveBase64StringToPDF(_ base64String: String, fileName:String)  -> String?{
        
        guard
            var documentsURL = (FileManager.default.urls(for: .documentDirectory, in: .userDomainMask)).last,
            let convertedData = Data(base64Encoded: base64String)
        else {
            return ""
        }
        
        //name your file however you prefer
        documentsURL.appendPathComponent("\(fileName).pdf")
        
        do {
            try convertedData.write(to: documentsURL)
        } catch {
            //handle write error here
        }
        print(documentsURL)
        return "\(documentsURL)"
    }
    
    class  func drawPDFfromURL(url: URL) -> UIImage? {
        guard let document = CGPDFDocument(url as CFURL) else { return nil }
        
        var width: CGFloat = 0
        var height: CGFloat = 0
        
        // calculating overall page size
        for index in 1...1 {//document.numberOfPages
            print("index: \(index)")
            if let page = document.page(at: index) {
                let pageRect = page.getBoxRect(.mediaBox)
                width = max(width, pageRect.width)
                height = height + pageRect.height
            }
        }
        
        // now creating the image
        let renderer = UIGraphicsImageRenderer(size: CGSize(width: width, height: height))
        
        let image = renderer.image { (ctx) in
            ctx.cgContext.scaleBy(x: 1.0, y: -1.0)
            for index in 1...1 {//document.numberOfPages
                
                if let page = document.page(at: index) {
                    let pageRect = page.getBoxRect(.mediaBox)
                    ctx.cgContext.translateBy(x: 0.0, y: -pageRect.height)
                    ctx.cgContext.drawPDFPage(page)
                }
            }
            
            ctx.cgContext.scaleBy(x: 1.0, y: -1.0)
        }
        return image
    }
    
    class  func drawImgsfromURL(url: URL) -> [UIImage]? {
        guard let document = CGPDFDocument(url as CFURL) else { return nil }
        var imgs = [UIImage]()
        // calculating overall page size
        print(document.numberOfPages)
        for index in 1...document.numberOfPages {//document.numberOfPages
            print("index: \(index)")
            var width: CGFloat = 0
            var height: CGFloat = 0
            if let page = document.page(at: index) {
                let pageRect = page.getBoxRect(.mediaBox)
                width = max(width, pageRect.width)
                height = height + pageRect.height
            }
            
            // now creating the image
            let renderer = UIGraphicsImageRenderer(size: CGSize(width: width, height: height))
            let image = renderer.image { (ctx) in
                ctx.cgContext.scaleBy(x: 1.0, y: -1.0)
                
                
                if let page = document.page(at: index) {
                    let pageRect = page.getBoxRect(.mediaBox)
                    ctx.cgContext.translateBy(x: 0.0, y: -pageRect.height)
                    ctx.cgContext.drawPDFPage(page)
                }
                
                ctx.cgContext.scaleBy(x: 1.0, y: -1.0)
            }
            imgs.append(image)
        }
        return imgs
    }
    
    
    class  func deleteFile(imageName :String,type:String) {
        
        let filemanager = FileManager.default
        let documentsPath = NSSearchPathForDirectoriesInDomains(.documentDirectory,.userDomainMask,true)[0] as NSString
        let destinationPath = documentsPath.appendingPathComponent("\(imageName).\(type)")
        do {
            try filemanager.removeItem(atPath: destinationPath)
            print("Local path removed successfully")
        } catch let error as NSError {
            print("------Error",error.debugDescription)
        }
    }
    
    class func getImage (named name : String) -> UIImage? {
        if let imgPath = Bundle.main.path(forResource: name, ofType: ".png") {
            return UIImage(contentsOfFile: imgPath)
        }
        return nil
    }
}
