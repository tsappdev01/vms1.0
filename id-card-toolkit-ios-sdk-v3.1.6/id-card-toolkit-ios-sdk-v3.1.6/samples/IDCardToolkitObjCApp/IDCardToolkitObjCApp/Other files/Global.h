//
//  Global.h
//  
//
//  Created by Federal Authority For Identity and Citizenship on 1/19/17.
//  Copyright © 2017 Federal Authority For Identity and Citizenship. All rights reserved.
//

#ifndef Global_h
#define Global_h

#define TIMEINTERVEL  3.0
#define SENSOR_TIME_OUT 30

#define INITIALIZE_ROW 0
#define REGISTERDEVICE_ROW 1
#define LISTREADER_ROW 2
#define CONNECTDEVICE_ROW 3
#define DISCONNECT_ROW 4
#define PUBLICDATA_ROW 5
#define PUBLICDATAEF_ROW 6
#define NFC_PUBLICDATA_ROW 7
#define NFC_PUBLICDATAEF_ROW 8
#define FAMILTYBOOK_ROW 9
#define CARDSTATUS_ROW 10
#define CERTIFICATES_ROW 11
#define FINGERDATA_ROW 12
#define PINRESET_ROW 13
#define UNBLOCK_PIN 14
#define BIOAUTH_ROW 15
#define CHECKAUTHBIO_ROW 16
#define SIGNDATA_ROW 17
#define PKIAUTH_ROW 18
#define CONFIG_CERT_EXPDATE_ROW 19
//#define UPDATEDATA_ROW 20
//#define READDATA_ROW 21
#define CSN_ROW 20
#define LICENSE_ROW 21
#define TOOLKITVERSION_ROW 22
#define READEREMIRATED_ID_ROW 23
#define DEVICEID_ROW 24
#define INTERFACETYPE_ROW 25
#define CELANUP_ROW 26

#import <IDCardToolkit/IDCardToolkit.h>
#import "AlertView.h"
#import "Model.h"
#import "MBProgressHUD.h"
#import "MainViewController.h"
#import "InitilizeViewController.h"
#import "ListReadersViewController.h"
#import "ConnectViewController.h"
#import "PublicDataViewController.h"
#import "PublicDataEFViewController.h"
#import "SetNFCdetailsViewController.h"
#import "NFCPublicDataViewController.h"
#import "NFCPublicDataEFViewController.h"
#import "FamilyBookViewController.h"
#import "CheckCardViewController.h"
#import "PkiCertificatesViewController.h"
#import "PinResetViewController.h"
#import "FingerDataViewController.h"
#import "BioAuthenticationViewController.h"
#import "CheckAndAuthBioViewController.h"
#import "SignDataViewController.h"
#import "PKIAuthViewController.h"
#import "ConfigCertExpiryDateViewController.h"
#import "CardSerialNumViewController.h"
#import "LicenseExpiryViewController.h"
#import "UnblockPINViewController.h"
#import "DisconnectViewController.h"
#import "ToolkitVersionViewController.h"
#import "ReaderEmiratesIDViewController.h"
#import "DeviceIDViewController.h"
#import "InterfaceTypeViewController.h"
#import "CleanUpViewController.h"

#import "Utils.h"
#import "AppDelegate.h"
#import "PublicDataParse.h"
#import "FamilyBookParse.h"
#import "HealthDataParse.h"
#import "CustomTableViewCell.h"
#import "MainTableViewCell.h"

#define ALERT  @ "Alert !"
#define DEVICE_CONNECT  @ "Please wait Connecting to device..."
#define DEVICE_CONNECTED  @ "Device connected"
#define DEVICE_DISCONNECTING  @ "Please wait Disconnecting device..."
#define DISCONNECT_DEVICE  @"Device disconnected"
#define INITIALIZE  @ "Initialize Successfully"
#define LISTREADERS  @"Please wait Searching readers......."
#define READING_CARDDEATILS  @ "Please wait reading Card details..."
#define CHECKCARD_STATUS  @ "Please wait Checking card status...."
#define FETCH_CERTIFICATES  @ "Please wait fetching certificates...."
#define FINGERDATA  @ "Please wait fetching finger details...."
#define RESETPIN  @"Please wait resetting pin...."
#define UNBLOCKPIN  @ "Please wait Unblocking pin...."
#define BIOAUTH  @ "Please wait Authenticating Biometric....."
#define SIGNDATA  @ "Please wait Signing data....."
#define VERIFYDATA  @"Please wait Verifying Signed data....."
#define PKIAUTH  @ "Please wait Authenticating PKI....."
#define READERNAME  @ "Please wait Searching readers....."
#define LOADINGDATA  @ "Please wait....."
#define CLEANUP  @ "Toolkit CleanUp Successfully"
#define NFC_NOT_SUPPORT  @ "Requires iOS 13.0 or above version to perfrom NFC on iPhone"

#define REQUEST_HANDLE_EMPTY @"Request_handle is empty"
#define ENCODE_PIN_EMPTY @"EncodePin is empty"
#define ENCODE_USERID_EMPTY @"Encoded UserId is empty"
#define ENCODE_PASSWORD_EMPTY @"Encoded Password is empty"

#define ENTER_DEVICE_USERNAME  @ "Please Enter Device User name"
#define ENTER_DEVICE_PASSWORD  @ "Please Enter Device Password"
#define ENTER_DEVICE_REF_ID  @ "Please Enter Device Reference ID"

#define ENTER_PIN  @ "Please Enter PIN"
#define CONFIRM_PIN  @ "Please Enter Confirm PIN"
#define MISMATCH_PIN  @ "Does not Match Enter PIN and Confirm PIN"
#define MIN_PIN  @ "Please Enter Minimum 4 Digits"
#define MAX_PIN  @ "Please Enter Minimum 4 digits and Maximum 16 digits"
#define PINREST_SUCCESS  @"Pin Reset successfully"
#define SIGNATURE_VERIFIED  @ "Signature successfully Verified"
#define FIELDEMPTY  @"Field not to be empty"
#define CARD_UNBLOCK_SUCCESS  @ "Card Unblock success"


#endif /* Global_h */
