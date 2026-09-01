"use strict";
var xmlResponse = null;
var xmlDiv = null;
var ToolkitOB = null;
var readerClass = null;
this.fingerData = null;
this.verifyxmldata = null;
var self = this;
var javaService = '';
this.IsNfc = false;
var localAddress = window.location.href;
var options = {
	"jnlp_address": javaService + "IDCardToolkitService.jnlp",
	"debugEnabled": true,
	"agent_tls_enabled": false,
	"agent_host_name": "toolkitagent.emiratesid.ae"
};

var IsSam = {
	sam_secure_messaging: true
};

//options.toolkitConfig = 'vg_url = http://your-vg-server/ValidationGatewayService\n'
options.toolkitConfig = 'vg_connection_timeout = 60 \n';
options.toolkitConfig += 'log_level = "INFO" \n';
options.toolkitConfig += 'log_performance_time = true \n';

var PUBLIC_DATA_EF_TYPE = {
	public_data_ef_type: ''
}

/**
 * Error handler call back function.
 * This function is executed if any error occurred in the web socket communication.
 * This function is passed as a error call back function while initializing the web socket.
 * 
 * @param err error details
 */
var errorHandlerCB = function (err) {
	readerClass = null;
	ToolkitOB = null;
	if (null !== err) {
		// hideLoader();
		alert('errorHandler ERROR : ' + err);
	}
}

/**
 * Close handler call back function.
 * This function is executed when web socket connection is closed.
 * This function is passed as a close call back function while initializing the web socket.
 * 
 * @param response response details
 */
var closeHandlerCB = function (response) {
	// hideLoader();
	ToolkitOB = null;
	readerClass = null;
	if (null !== response && undefined == response) { }
	changeButtonState(true);
	document.getElementById("workAreaDiv").style.display = 'none';
}

/**
 * open handler call back function.
 * This function is executed when web socket connection is opened/established successfully.
 * This function is passed as a onOpen call back function while initializing the web socket.
 *  
 */
var onOpenHandlerCB = function (response, error) {
	// hideLoader();
	if (error === null) {
		/**
		 * call the list reader function and pass listReaderCB to be executed
		 * after the response is received from server
		 */
		if (IsSam.sam_secure_messaging) {
			ToolkitOB.getReaderWithEmiratesId(listReaderCB);
		} else {
			ToolkitOB.listReaders(listReaderCB);
		}
	} else {
		ToolkitOB = null;
	}
}

/**
 * listReader handler call back function.
 * This function is executed when response is received from server for listReader request.
 * This function is passed as a listReader call back function while sending listReader request.
 * 
 * @param response describing response from server.
 *  response object has one field 'data' which contains a string in the json format.
 *  This string can be converted to json object by using JSON.parse(response.data);
 * 
 */
var listReaderCB = function (response, error) {
	if (error !== null) {
		alert(error.message || error.description);
		ToolkitOB = null;
		displayProgress('Initializing Web Socket Failed. Reader Not Connected ...');
		hideLoader();
	} else {
		var readerName = null;
		var readerList = response;
		if (IsSam.sam_secure_messaging) {
			readerClass = readerList;
		} else {
			if (readerList && 0 < readerList.length) {
				readerClass = readerList[0];
			} else {
				return 'No readers found';
			}
		}

		displayProgress('Initializing Web Socket Success ...');
		displayProgress('Connecting to reader ...');
		
		/**
		 * call the connect reader function and pass connectReaderCB to be executed
		 * after the response is received from server
		 */
		var ret = readerClass.connect(connectReaderCB);
		if ('' !== ret) {
			/* disable all buttons till request is processed */
			changeButtonState(true);
		}
	}
}

/**
 * connectReader handler call back function.
 * This function is executed when response is received from server for connectReader request.
 * This function is passed as a connectReader call back function while sending connectReader request.
 * 
 * @param response describing response from server.
 *  response object has one field 'data' which contains a string in the json format.
 *  This string can be converted to json object by using JSON.parse(response.data);
 * 
 */
var connectReaderCB = function (response, error) {
	if (null !== error) {
		alert(error.code + ' : ' + error.message);
		ToolkitOB = null;
		displayProgress("Card Not Connected, Connect failed ...");
		hideLoader();
		return;
	}

	readerClass.getInterfaceType(getInterfaceCB);
	document.getElementById("workAreaDiv").style.display = 'block';
	displayProgress("Card Connected, Connect Success ...");
}

/**
 * This function is used to get Interface of the reader
 */
var getInterfaceCB = function (response, error) {
	if (null !== error) {
		alert(error.code + ' : ' + error.message);
		ToolkitOB = null;
		return;
	}
	if (response === 2) {
		self.IsNfc = true;
		alert("Initialize Success. First Set NFC Parameters.");
	} else {
		self.IsNfc = false;
	}

	hideLoader();
	/* enable all buttons */
	changeButtonState(false);
}

/**
 * This function is used to initialize the PublicDataWebComponent
 */
function Initialize() {
	try {
		/* Ensures only one connection is open at a time */
		if (ToolkitOB !== null) {
			/*  enable all buttons  */
			if (readerClass !== null) {
				changeButtonState(false);
				hideLoader();
				return 'WebSocket is already active ...';
			}
		}

		/*  if
		 provide the call backs */
		showLoader();
		ToolkitOB = new Toolkit(
			onOpenHandlerCB, /* reference to onOpen call back function */
			closeHandlerCB, /* reference to onClose call back function */
			errorHandlerCB, /* reference to onError call back function */
			options /* options */
		);
		displayProgress('Initializing Web Socket ...');
	} catch (e) {
		// hideLoader();
		alert("Webcomponent Initialization Failed, Details: " + e);
	}
}

/**
 * This function is used to read the public data from first reader
 * found.
 */
function DisplayPublicData(nfc) {
	if (null === readerClass || undefined === readerClass) {
		alert('ERROR : Reader is not initiaized.');
		return;
	}
	/*  disable all buttons till request is processed */
	changeButtonState(true);
	showLoader();
	displayProgress('Reading public data...');
	/*  generate the random string */
	var randomStr = generateRandomString(40);
	/* convert randomString to base64 */
	var requestId = btoa(randomStr);
	/**
	 * call the read public data function and pass readPublicDataCB to be executed
	 * after the response is received from server
	 */
	var address = true;
	if (self.IsNfc) {
		address = false;
	}
	document.getElementById('res').value = "";
	readerClass.readPublicData(
		requestId,
		true,
		true,
		true,
		true,
		address,
		readPublicDataCB);
	changeButtonState(true);
}

/**
 * readPublicData handler call back function.
 * This function is executed when response is received from server for readPublicData request.
 * This function is passed as a readPublicData call back function while sending readPublicData request.
 * 
 * @param response describing response from server.
 *  response object has one field 'data' which contains a string in the json format.
 *  This string can be converted to json object by using JSON.parse(response.data);
 * 
 */
var readPublicDataCB = function (response, error) {
	hideLoader();
	if (error === null) {
		displayData(response, 'readPublicDataDiv');
		if (response.xmlString !== null && response.xmlString !== undefined) {
			document.getElementById("vxs").style.display = "block";
			self.verifyxmldata = response.xmlString;
		}
	} else {
		alert(error.message);
		changeButtonState(false);
	}
	/* enable all buttons as request is completed */
	changeButtonState(false);
}

/**
 * This function is to check card status
 */
function CheckCardStatus() {
	if (null === readerClass || undefined === readerClass) {
		alert('ERROR : Reader is not initiaized.');
		return;
	}
	document.getElementById('res').value = "";
	displayProgress('Checking Card Status...');
	showLoader();
	var randomStr = generateRandomString(40);
	var requestId = btoa(randomStr);
	readerClass.checkCardStatus(requestId, CheckCardStatusCB);
	changeButtonState(true);
}

var CheckCardStatusCB = function (response, error) {
	showDiv("cardStatusDiv");
	hideLoader();
	changeButtonState(false);
	if (error !== null) {
		document.getElementById("cardStatusTxtBx").style.color = "red";
		document.getElementById("cardStatusTxtBx").value = error.message;
		document.getElementById("vxs").style.display = "block";
		self.verifyxmldata = error.toolkit_response;
		return;
	}

	document.getElementById("cardStatusTxtBx").style.color = "green";
	document.getElementById("cardStatusTxtBx").value = "Card Is Valid";
	document.getElementById("cardStatusTxtXMlrow").style.display = null;
	document.getElementById("cardStatusTxtXML").value = response.xmlString;
	if (response.xmlString !== null && response.xmlString !== undefined) {
		document.getElementById("vxs").style.display = "block";
		self.verifyxmldata = response.xmlString;
	}
	changeButtonState(false);
}

/**
 * This function is to read certificates
 */
function ReadCertificate() {
	displayProgress("Reading Certificates Data");
	document.getElementById("vxs").style.display = "none";
	document.getElementById('res').style.display = "none";
	showDiv("readCertsDiv");
}

function ReadCertificates() {
	if (null === readerClass || undefined === readerClass) {
		alert('ERROR : Reader is not initiaized.');
		return;
	}

	changeButtonState(true);
	var pin = document.getElementById('rd_cert_pin').value;
	if (pin == null || undefined == pin || '' == pin || pin.length < 4) {
		alert('Please enter valid pin.');
		return;
	}

	showLoader();
	document.getElementById('res').value = "";
	PrepareRequest(function (requestHandle) {
		if (requestHandle === undefined || requestHandle === null) {
			var encodedPin = pin;
			readerClass.getPkiCertificates(encodedPin, ReadCertificatesCB);
		} else {
			ToolkitOB.getDataProtectionKey(
				function (response, error) {
					var encodedPin = encodePinOnServer(pin, requestHandle, response.publicKey);
					if (encodedPin == -1) {
						hideLoader();
						changeButtonState(false);
						alert('Failed to Encrypt data');
						return;
					}

					readerClass.getPkiCertificates(encodedPin, ReadCertificatesCB);
				})
		}
	});
}

var ReadCertificatesCB = function (response, error) {
	hideLoader();
	if (null !== error) {
		if (error.attemptsLeft) {
			error.message = error.message + "   " + "Attemptsleft:" + error.attemptsLeft;
		}
		alert(error.message);
		if (error.toolkit_response !== null && error.toolkit_response !== undefined) {
			document.getElementById("vxs").style.display = "block";
			self.verifyxmldata = error.toolkit_response;
		}
		changeButtonState(false);
		return;
	}

	var result = response;
	if ('fail' === result.status) {
		return result.error + ' : ' + result.description;
		changeButtonState(false);
	}

	document.getElementById("signCertTextArea").value = result.signingCertificate;
	document.getElementById("authCertTextArea").value = result.authenticationCertificate;
	if (result.xmlString !== null && result.xmlString !== undefined) {
		document.getElementById("vxs").style.display = "block";
		self.verifyxmldata = result.xmlString;
	}

	/*  enable all buttons as request is completed */
	changeButtonState(false);
}

/**
 * This function is to get finger indexes
 */
function GetFingerIndex() {
	if (null === readerClass || undefined === readerClass) {
		alert('ERROR : Reader is not initiaized.');
		return;
	}

	showDiv("fingerIndexDiv");
	showLoader();
	changeButtonState(true);
	document.getElementById("vxs").style.display = "none";
	document.getElementById('res').style.display = "none";
	readerClass.getFingerData(GetFingerIndexCB)
	changeButtonState(true);
	return;
}

var GetFingerIndexCB = function (response, error) {
	hideLoader();
	if (null !== error) {
		alert(error.message);
		changeButtonState(false);
		return;
	}
	document.getElementById("fingerIndexTextArea").value = response[0].fingerIndex + "\n" + response[1].fingerIndex;
	changeButtonState(false);
}

/**
 * This function is to verify biometric
 */
function VerifyBio() {
	document.getElementById("vxs").style.display = "none";
	document.getElementById('res').style.display = "none";
	if (null === readerClass || undefined === readerClass) {
		alert('ERROR : Reader is not initiaized.');
		return;
	}

	showDiv("verifyBioDiv");
	changeButtonState(false);
	showLoader();
	document.getElementById('res').value = "";
	readerClass.getFingerData(
		function (response, error) {
			hideLoader();
			if (error !== null) {
				alert(error.message);
				changeButtonState(false);
				return;
			}
			var result = response;
			if ('fail' === result.status) {
				return result.error + ' : ' + result.description;
			}
			/* set result of getFingerIndex to local variable so that it can be while verifying biometric */
			self.fingerData = result;
			var selectBox = document.getElementById("verifyBioFingerSelect");
			if (selectBox.options.length > 1) {
				selectBox.removeChild(selectBox.options[2]);
				selectBox.removeChild(selectBox.options[1]);
			}
			var option1 = document.createElement("option");
			var opt1 = result[0].fingerIndex;
			option1.text = opt1;
			selectBox.add(option1);
			var option2 = document.createElement("option");
			option2.text = result[1].fingerIndex;
			selectBox.add(option2);
			changeButtonState(false);
		})
}

function VerifyBioSubmit() {
	if (null === readerClass || undefined === readerClass) {
		alert('ERROR : Reader is not initiaized.');
		return;
	}

	var selectedFinger = document.getElementById("verifyBioFingerSelect").value
	if ('Select Finger' == selectedFinger || undefined == selectedFinger) {
		alert('Please select a finger.');
		return;
	}

	/*  disable all buttons till request is processe */
	changeButtonState(true);
	showLoader();
	displayProgress('Verifying biometric ...');
	var sensor_timeout = 30; /* seconds */
	var randomStr = generateRandomString(40);
	var requestId = btoa(randomStr);
	readerClass.authenticateBiometricOnServer(requestId, selectedFinger, sensor_timeout, VerifyBioCB);
}

var VerifyBioCB = function (response, error) {
	hideLoader();
	if (null !== error) {
		changeButtonState(false);
		document.getElementById("verifyBioTxtBx").value = error.message;
		if (error.toolkit_response !== null && error.toolkit_response !== undefined) {
			document.getElementById("vxs").style.display = "block";
			document.getElementById("verifyBioTxtBx").style.color = "red";
			self.verifyxmldata = error.toolkit_response;
		}
		if (self.IsNfc) {
			nfcMenu();
		}
		return;
	}
	result = response;
	document.getElementById("verifyBioTxtBx").style.color = "green";
	document.getElementById("verifyBioTxtBx").value = "Successful.";
	document.getElementById("verifyBioTxtBx").type = "text";
	if (self.IsNfc) {
		nfcMenu();
	}
	/* disable all buttons till request is processed */
	if (result.xmlString !== null && result.xmlString !== undefined) {
		document.getElementById("vxs").style.display = "block";
		self.verifyxmldata = result.xmlString;
	}
	changeButtonState(false);
}

function PKIAuth() {
	document.getElementById("vxs").style.display = "none";
	document.getElementById('res').style.display = "none";
	showDiv("pkiAuthDiv");
}

function PKIAuthSubmit() {
	if (null === readerClass || undefined === readerClass) {
		alert('ERROR : Reader is not initiaized.');
		return;
	}
	var pin = document.getElementById("pkiAuthTxtBx").value;
	if (pin == null || undefined == pin || '' == pin || pin.length < 4) {
		alert('Please enter valid pin.');
		return;
	}
	changeButtonState(true);
	showLoader();
	PrepareRequest(function (requestHandle) {
		if (requestHandle === undefined || requestHandle === null) {
			var encodedPin = pin;
			readerClass.authenticatePki(encodedPin, PKIAuthCB);
		} else {
			ToolkitOB.getDataProtectionKey(
				function (response, error) {
					var encodedPin = encodePinOnServer(pin, requestHandle, response.publicKey);
					if (encodedPin == -1) {
						hideLoader();
						changeButtonState(false);
						alert('Failed to Encrypt data');
						return;
					}
					readerClass.authenticatePki(encodedPin, PKIAuthCB);
				})
		}
	});
}

var PKIAuthCB = function (response, error) {
	hideLoader();
	changeButtonState(false);
	if (error !== null) {
		document.getElementById("pkiAuthResultTxtBx").type = "text";
		document.getElementById("pkiAuthResultTxtBx").style.color = "red";
		if (error.attemptsLeft) {
			error.message = error.message + "   " + "Attemptsleft:" + error.attemptsLeft;
		}
		alert(error.message);
		if (error.toolkit_response !== null && error.toolkit_response) {
			document.getElementById("vxs").style.display = "block";
			self.verifyxmldata = error.toolkit_response;
		}
		document.getElementById("pkiAuthResultTxtBx").value = error.message;
		return;
	}
	document.getElementById("pkiAuthResultTxtBx").type = "text";
	document.getElementById("pkiAuthResultTxtBx").style.color = "green";
	document.getElementById("pkiAuthResultTxtBx").value = "Valid Auth Cert";
	if (response.xmlString !== null && response.xmlString !== undefined) {
		document.getElementById("vxs").style.display = "block";
		self.verifyxmldata = response.xmlString;
	}
	changeButtonState(false);
}

/**
 * This function is to show sign data div
 */
function SignData() {
	document.getElementById("vxs").style.display = "none";
	document.getElementById('res').style.display = "none";
	showDiv("signDataDiv");
}

/**
 * This function is to sign data
 */
function SignDataSubmit() {
	if (null === readerClass || undefined === readerClass) {
		alert('ERROR : Reader is not initiaized.');
		return;
	}
	var dataHashed = document.getElementById("signDataHashedSelect").value;
	if (undefined === dataHashed || '' === dataHashed) {
		alert('Please select type of data.');
		return;
	}
	var data = document.getElementById("dataTextArea").value;
	if (undefined === data || '' === data) {
		alert('Please enter valid data.');
		return;
	}
	var pin = prompt("Please enter your pin", "");
	if (pin == null || undefined == pin || '' == pin || pin.length < 4) {
		alert('Please enter valid pin.');
		return;
	}
	var result = null;
	var readerName = null;
	/* disable all buttons till request is processed */
	changeButtonState(true);
	showLoader();
	PrepareRequest(function (requestHandle) {
		if (requestHandle === undefined || requestHandle === null) {
			var encodedPin = pin;
			readerClass.signData(data, parseInt(dataHashed), encodedPin, SignDataCB);
		} else {
			ToolkitOB.getDataProtectionKey(
				function (response, error) {
					var encodedPin = encodePinOnServer(pin, requestHandle, response.publicKey);
					if (encodedPin == -1) {
						hideLoader();
						changeButtonState(false);
						alert('Failed to Encrypt data');
						return;
					}
					readerClass.signData(data, parseInt(dataHashed), encodedPin, SignDataCB);
				})
		}
	});
}

var SignDataCB = function (response, error) {
	hideLoader();
	if (error !== null) {
		if (error.attemptsLeft) {
			error.message = error.message + "   " + "Attemptsleft:" + error.attemptsLeft;
		}
		alert(error.message);
		if (error.toolkit_response !== null && error.toolkit_response !== undefined) {
			document.getElementById("vxs").style.display = "block";
			self.verifyxmldata = error.toolkit_response;
		}
		changeButtonState(false);
		return;
	}
	var result = response;
	var resultData = '';
	if ('fail' === result.status) {
		resultData = result.error + ' : ' + result.description + ' : ' + result.attemptsLeft;
		document.getElementById("resultTextArea").value = resultData;
		changeButtonState(false);
		return;
	}
	resultData = result.signature;
	document.getElementById("resultTextArea").value = resultData;
	if (result.xmlString !== null && result.xmlString !== undefined) {
		document.getElementById("vxs").style.display = "block";
		self.verifyxmldata = result.xmlString;
	}
	changeButtonState(false);
}

function SignChallengeData() {
	document.getElementById("vxs").style.display = "none";
	document.getElementById('res').style.display = "none";
	showDiv("signChallangeDataDiv");
}
/**
 * This function is to sign data
 */
function SignChallengeDataSubmit() {
	if (null === readerClass || undefined === readerClass) {
		alert('ERROR : Reader is not initiaized.');
		return;
	}
	var dataHashed = document.getElementById("challengesignDataHashedSelect").value;
	if (undefined === dataHashed || '' === dataHashed) {
		alert('Please select type of data.');
		return;
	}
	var data = document.getElementById("challengedataTextArea").value;
	if (undefined === data || '' === data) {
		alert('Please enter valid data.');
		return;
	}
	var pin = prompt("Please enter your pin", "");
	if (pin == null || undefined == pin || '' == pin || pin.length < 4) {
		alert('Please enter valid pin.');
		return;
	}
	var result = null;
	var readerName = null;
	/*  disable all buttons till request is processed */
	changeButtonState(true);
	showLoader();
	PrepareRequest(function (requestHandle) {
		if (requestHandle === undefined || requestHandle === null) {
			var encodedPin = pin;
			readerClass.signChallenge(data, parseInt(dataHashed), encodedPin, SignChallangeDataCB);
		} else {
			ToolkitOB.getDataProtectionKey(
				function (response, error) {
					var encodedPin = encodePinOnServer(pin, requestHandle, response.publicKey);
					if (encodedPin == -1) {
						hideLoader();
						changeButtonState(false);
						alert('Failed to Encrypt data');
						return;
					}
					readerClass.signChallenge(data, parseInt(dataHashed), encodedPin, SignChallangeDataCB);
				})
		}
	});
}
var SignChallangeDataCB = function (response, error) {
	hideLoader();
	if (error !== null) {
		if (error.attemptsLeft) {
			error.message = error.message + "   " + "Attemptsleft:" + error.attemptsLeft;
		}
		alert(error.message);
		changeButtonState(false);
		return;
	}
	var result = response;
	document.getElementById("challengeresultTextArea").value = result.signature;
	/* enable all buttons as request is processed */
	if (result.xmlString !== null && result.xmlString !== undefined) {
		document.getElementById("vxs").style.display = "block";
		self.verifyxmldata = result.xmlString;
	}
	changeButtonState(false);
}
/**
 * This function is to sign data
 */
function VerifySignature() {
	document.getElementById("vxs").style.display = "none";
	document.getElementById('res').style.display = "none";
	showDiv("verifyDataDiv");
}

function VerifyDataSubmit() {
	if (null === readerClass || undefined === readerClass) {
		alert('ERROR : Reader is not initiaized.');
		return;
	}
	var dataHashed = document.getElementById("verifyDataHashedSelect").value;
	if (undefined === dataHashed || '' === dataHashed) {
		alert('Please select type of data.');
		return;
	}
	var certType = document.getElementById("verifyDataCertSelect").value;
	if (undefined === certType || '' === certType) {
		alert('Please select type of data.');
		return;
	}
	var originalData = document.getElementById("originalDataTextArea").value;
	if (undefined === originalData || '' === originalData) {
		alert('Please enter valid original data.');
		return;
	}
	var signedData = document.getElementById("signedDataTextArea").value;
	if (undefined === signedData || '' === signedData) {
		alert('Please enter valid signed data.');
		return;
	}
	var pin = prompt("Please enter your pin", "");
	if (pin == null || undefined == pin || '' == pin || pin.length < 4) {
		alert('Please enter valid pin.');
		return;
	}
	var certData = "";
	showLoader();
	PrepareRequest(function (requestHandle) {
		if (requestHandle === undefined || requestHandle === null) {
			var encodedPin = pin;
			readerClass.getPkiCertificates(encodedPin, function (response, error) {
				if (certType == 1) {
					certData = response.signingCertificate;
				} else {
					certData = response.authenticationCertificate;
				}
				changeButtonState(true);
				readerClass.verifySignature(originalData, parseInt(dataHashed), signedData, certData, VerifyDataCB);
				// changeButtonState(true);
			});
		} else {
			ToolkitOB.getDataProtectionKey(
				function (response, error) {
					var encodedPin = encodePinOnServer(pin, requestHandle, response.publicKey);
					if (encodedPin == -1) {
						hideLoader();
						changeButtonState(false);
						alert('Failed to Encrypt data');
						return;
					}
					readerClass.getPkiCertificates(encodedPin, function (response, error) {
						if (certType == 1) {
							certData = response.signingCertificate;
						} else {
							certData = response.authenticationCertificate;
						}
						changeButtonState(true);
						readerClass.verifySignature(originalData, parseInt(dataHashed), signedData, certData, VerifyDataCB);
					})
					// changeButtonState(true);
				});
		}

	});
	/* disable all buttons till request is processed */

}
var VerifyDataCB = function (response, error) {
	hideLoader();
	if (error !== null) {
		if (error.attemptsLeft) {
			error.message = error.message + "   " + "Attemptsleft:" + error.attemptsLeft;
		}
		alert(error.message);
		document.getElementById("verifyDataTxtBx").value = error.message;
		document.getElementById("verifyDataTxtBx").type = "text";
		document.getElementById("verifyDataTxtBx").style.color = "red";
		changeButtonState(false);
		return;
	}
	var resultData = "Verification Successful."
	document.getElementById("verifyDataTxtBx").value = resultData;
	document.getElementById("verifyDataTxtBx").type = "text";
	document.getElementById("verifyDataTxtBx").style.color = "green";
	changeButtonState(false);
}
/**
 * This function is to reset pin
 */
function pinReset() {
	document.getElementById("vxs").style.display = "none";
	document.getElementById('res').style.display = "none";
	showDiv("pinResetDiv");
	var result = null;
	var readerName = null;
	if (null === readerClass || undefined === readerClass) {
		alert('ERROR : Reader is not initiaized.');
		return;
	}
	/* disable all buttons till request is processed */
	changeButtonState(true);
	displayProgress('Getting finger indexes...');
	readerClass.getFingerData(
		function (response, error) {
			if (error !== null) {
				alert(error.message);
				changeButtonState(false);
				return;
			}
			var result = response;
			if ('fail' === result.status) {
				return result.error + ' : ' + result.description;
			}
			/* set result of getFingerIndex to local variable so that it can be while verifying biometric */
			self.fingerData = result;
			var selectBox = document.getElementById("resetPINFingerSelect");
			if (selectBox.options.length > 1) {
				selectBox.removeChild(selectBox.options[2]);
				selectBox.removeChild(selectBox.options[1]);
			}
			var option1 = document.createElement("option");
			var opt1 = result[0].fingerIndex;
			option1.text = opt1;
			selectBox.add(option1);
			var option2 = document.createElement("option");
			option2.text = result[1].fingerIndex;
			selectBox.add(option2);
			changeButtonState(false);
		})
}

/**
 * This function is to verify biometric
 */
function pinResetSubmit() {
	if (null === readerClass || undefined === readerClass) {
		alert('ERROR : Reader is not initiaized.');
		return;
	}
	var pin = document.getElementById("pinResetTxtBx").value;
	if (undefined === pin || '' === pin || pin.length < 4) {
		alert('Please provide valid pin .');
		return;
	}
	var selectedFinger = document.getElementById("resetPINFingerSelect").value
	if ("Select Finger" == selectedFinger || undefined == selectedFinger) {
		alert('Please select a finger.');
		return;
	}
	/* disable all buttons till request is processed */
	changeButtonState(true);
	displayProgress('Resetting PIN ...');

	var index = 0;
	var indexId = 0;
	/* get finger index from selectedFinger */
	for (let i = 0; i < self.fingerData.length; i++) {
		if (self.fingerData[i].fingerIndex === selectedFinger) {
			index = self.fingerData[i];
			break;
		}
	}
	var sensor_timeout = 30; /*  seconds */
	showLoader();
	PrepareRequest(function (requestHandle) {
		if (requestHandle === undefined || requestHandle === null) {
			var encodedPin = pin;
			readerClass.resetPin(encodedPin, index, sensor_timeout, pinResetCB);
		} else {
			ToolkitOB.getDataProtectionKey(
				function (response, error) {
					var encodedPin = encodePinOnServer(pin, requestHandle, response.publicKey);
					if (encodedPin == -1) {
						hideLoader();
						changeButtonState(false);
						alert('Failed to Encrypt data');
						return;
					}
					readerClass.resetPin(encodedPin, index, sensor_timeout, pinResetCB);
				})
		}
	});
}
var pinResetCB = function (response, error) {
	hideLoader();
	if (error !== null) {
		if (error.attemptsLeft) {
			error.message = error.message + "   " + "Attemptsleft:" + error.attemptsLeft;
		}
		document.getElementById("pinResetBioTxtBx").style.color = "red";
		document.getElementById("pinResetBioTxtBx").value = error.message;
		if (error.toolkit_response !== null && error.toolkit_response !== undefined) {
			document.getElementById("vxs").style.display = "block";
			self.verifyxmldata = error.toolkit_response;
		}
		changeButtonState(false);
		return;
	}
	result = response;
	/* check if there is any error in response */
	document.getElementById("pinResetBioTxtBx").style.color = "green";
	document.getElementById("pinResetBioTxtBx").value = "Successful.";
	document.getElementById("pinResetBioTxtBx").type = "text";
	if (result.xmlString !== null && result.xmlString !== undefined) {
		document.getElementById("vxs").style.display = "block";
		self.verifyxmldata = result.xmlString;
	}
	/* enable all buttons as request is processed */
	changeButtonState(false);
}
/**
 * This function is used to disconnect web socket connection
 * 
 */
function disconnectWS() {
	if (null === readerClass || undefined === readerClass) {
		alert('ERROR : Reader is not initiaized.');
		return;
	}
	/**
	 * call the disconnect reader function and pass disconnectCB to be executed
	 * after the response is received from server
	 */
	showLoader();
	readerClass.disconnect(disconnectCB);
}
/**
 * disconnect handler call back function.
 * This function is executed when response is received from server for disConnectReader request.
 * This function is passed as a disconnectCB call back function while sending disConnectReader request.
 * 
 * @param response describing response from server.
 *  response object has one field 'data' which contains a string in the json format.
 *  This string can be converted to json object by using JSON.parse(response.data);
 * 
 */
var disconnectCB = function (response, error) {
	hideLoader();
	if (error !== null) {
		alert(error.message);
		changeButtonState(false);
		return;
	}
	var result = response;
	ToolkitOB.cleanup();
	if ('fail' === result) {
		return result.error + ' : ' + result.description;
	}
	changeButtonState(false);
}

/**
 * This function is to hide all the divs and only show a div
 * specified by divName
 * 
 * @param divName div to show/block
 */
function showDiv(divName) {
	var divs = document.getElementsByClassName('public-data-div');
	for (let i = 0; i <= divs.length - 1; i++) {
		divs[i].style.display = 'none';
	}
	if ('' !== divName) {
		document.getElementById(divName).style.display = 'block';
		document.getElementById(divName).style.display = 'block';
	}
}
/**
 * This function is to change button's accessibility and css class.
 * 
 */
function changeButtonState(flag) {
	if (flag == false) {
		if (self.IsNfc) {
			document.getElementById('disconnectBtn').disabled = false;
			document.getElementById('setNfcParamsBtn').disabled = false;
			return;
		}
	}
	var buttons = document.getElementsByClassName("buttonInitial");
	for (let i = 0; i <= buttons.length - 1; i++) {
		buttons[i].disabled = flag;
	}
	if (self.IsNfc === false) {
		document.getElementById('setNfcParamsBtn').disabled = true;
	}
}
/**
 * This function is to display progress in progress box.
 */
function displayProgress(msg) {
	msg = msg + '\n' + document.getElementById("prgssText").value;
	document.getElementById("prgssText").value = msg;
}
/**
 * This function is to show unblock pin div
 */
function PinUnblock() {
	document.getElementById("vxs").style.display = "none";
	document.getElementById('res').style.display = "none";
	showDiv("unBlockPinDiv");
	if (null === readerClass || undefined === readerClass) {
		alert('ERROR : Reader is not initiaized.');
		return;
	}
	changeButtonState(true);
	readerClass.getFingerData(
		function (response, error) {
			if (error !== null) {
				alert(error.message);
				changeButtonState(false);
				return;
			}
			var result = response;
			if ('fail' === result.status) {
				return result.error + ' : ' + result.description;
			}
			/* set result of getFingerIndex to local variable so that it can be while verifying biometric */
			self.fingerData = result;
			var selectBox = document.getElementById("unBlockPinFingerSelect");
			if (selectBox.options.length > 1) {
				selectBox.removeChild(selectBox.options[2]);
				selectBox.removeChild(selectBox.options[1]);
			}
			var option1 = document.createElement("option");
			var opt1 = result[0].fingerIndex;
			option1.text = opt1;
			selectBox.add(option1);
			var option2 = document.createElement("option");
			option2.text = result[1].fingerIndex;
			selectBox.add(option2);
			changeButtonState(false);
		})
}

function UnBlockPinSubmit() {
	try {

		var pin = document.getElementById("unBlockPinTxtBx").value;
		if (undefined === pin || '' === pin || pin.length < 4) {
			alert('Please provide valid pin .');
			return;
		}
		var selectedFinger = document.getElementById("unBlockPinFingerSelect").value;
		if (undefined === selectedFinger || "Select Finger" === selectedFinger) {
			alert('Please select a finger.');
			return;
		}
		displayProgress('Matching biometric ...');
		if (null === readerClass || undefined === readerClass) {
			alert('ERROR : Reader is not initiaized.');
			return;
		}
		var index;
		for (let i = 0; i < self.fingerData.length; i++) {
			if (self.fingerData[i].fingerIndex === selectedFinger) {
				index = self.fingerData[i];
				break;
			}
		}
		showLoader();
		PrepareRequest(function (requestHandle) {
			if (requestHandle === undefined || requestHandle === null) {
				var encodedPin = pin;
				var sensor_timeout = 30; /*  seconds */
				readerClass.unblockPin(
					encodedPin,
					index,
					sensor_timeout,
					function (response, error) {
						hideLoader();
						if (error) {
							if (error.attemptsLeft) {
								error.message = error.message + "   " + "Attemptsleft:" + error.attemptsLeft;
							}
							document.getElementById("unBlockPinBioTxtBx").style.color = "red";
							document.getElementById("unBlockPinBioTxtBx").value = error.message || "Failed try again later.";
							document.getElementById("unBlockPinBioTxtBx").type = "text";
							if (error.toolkit_response !== null && error.toolkit_response !== undefined) {
								document.getElementById("vxs").style.display = "block";
								self.verifyxmldata = error.toolkit_response;
							}
							return;
						}
						var result = response;
						document.getElementById("unBlockPinBioTxtBx").style.color = "green";
						document.getElementById("unBlockPinBioTxtBx").value = "Successful.";
						document.getElementById("unBlockPinBioTxtBx").type = "text";
						if (result.xmlString !== null && result.xmlString !== undefined) {
							document.getElementById("vxs").style.display = "block";
							self.verifyxmldata = result.xmlString;
						}
						//self.fingerData = null;
						/* enable all buttons as request is processed */
						changeButtonState(false);
					});
			} else {
				ToolkitOB.getDataProtectionKey(
					function (response, error) {
						var encodedPin = encodePinOnServer(pin, requestHandle, response.publicKey);
						if (encodedPin == -1) {
							hideLoader();
							changeButtonState(false);
							alert('Failed to Encrypt data');
							return;
						}
						var sensor_timeout = 30; /*  seconds */
						readerClass.unblockPin(
							encodedPin,
							index,
							sensor_timeout,
							function (response, error) {
								hideLoader();
								if (error) {
									if (error.attemptsLeft) {
										error.message = error.message + "   " + "Attemptsleft:" + error.attemptsLeft;
									}
									document.getElementById("unBlockPinBioTxtBx").style.color = "red";
									document.getElementById("unBlockPinBioTxtBx").value = error.message || "Failed try again later.";
									document.getElementById("unBlockPinBioTxtBx").type = "text";
									if (error.toolkit_response !== null && error.toolkit_response !== undefined) {
										document.getElementById("vxs").style.display = "block";
										self.verifyxmldata = error.toolkit_response;
									}
									return;
								}
								var result = response;
								document.getElementById("unBlockPinBioTxtBx").style.color = "green";
								document.getElementById("unBlockPinBioTxtBx").value = "Successful.";
								document.getElementById("unBlockPinBioTxtBx").type = "text";
								if (result.xmlString !== null && result.xmlString !== undefined) {
									document.getElementById("vxs").style.display = "block";
									self.verifyxmldata = result.xmlString;
								}
								//self.fingerData = null;
								/* enable all buttons as request is processed */
								changeButtonState(false);
							});
					})

			}

		});
	} catch (e) {
		hideLoader();
		return "An exception occured in unblockPIN." + e;
	}
}

/**
 * This function is to show card genuine div
 */
function CardGenuine() {
	if (null === readerClass || undefined === readerClass) {
		alert('ERROR : Reader is not initiaized.');
		return;
	}
	showDiv("cardGenuineDiv");
	/*  disable all buttons till request is processe */
	changeButtonState(true);
	showLoader();
	var randomStr = generateRandomString(40);
	var requestId = btoa(randomStr);
	readerClass.isCardGenuine(requestId, CardGenuineCB);
}
var CardGenuineCB = function (response, error) {
	hideLoader();
	if (null !== error) {
		document.getElementById("cardGenuineTxtBx").style.color = "red";
		document.getElementById("cardGenuineTxtBx").value = "Failed. : " + error.message;
		document.getElementById("cardGenuineTxtBx").type = "text";
		changeButtonState(false);
		return;
	}
	result = JSON.parse(response.data);
	/*  display success message */
	document.getElementById("cardGenuineTxtBx").style.color = "green";
	document.getElementById("cardGenuineTxtBx").value = "Successful.";
	document.getElementById("cardGenuineTxtBx").type = "text";
	/*  reset the fingerData to null */
	self.fingerData = null;
	/*  enable all buttons as request is processed */
	changeButtonState(false);
}

function signDSSTypes() {
	document.getElementById("vxs").style.display = "none";
	document.getElementById('res').style.display = "none";
	showDiv("signDSSDiv");
}
function verifyDSSTypes() {
	document.getElementById("vxs").style.display = "none";
	document.getElementById('res').style.display = "none";
	showDiv("verifyDSSDiv");
}

function SignXMLFunc() {
	document.getElementById("vxs").style.display = "none";
	document.getElementById('res').style.display = "none";
	showDiv("signXADESDiv");
}

var prepareRequestCB = function (response, error) {
	var userName = prompt('enter username', '');
	var password = prompt('password', '');
	var deviceRefID = prompt('Device Reference Id', '')
	if (null == error) {
		var requestHandle = response;
		if (requestHandle == null || requestHandle == undefined) {
			var userNameEncoded = userName;
			var passwordEncoded = password;
			ToolkitOB.registerDevice(userNameEncoded, passwordEncoded, deviceRefID, registerDeviceCB);
		} else {

			ToolkitOB.getDataProtectionKey(
				function (response, error) {
					var userNameEncoded = encryptParamasOnServer(userName, requestHandle, response.publicKey);
					var passwordEncoded = encryptParamasOnServer(password, requestHandle, response.publicKey);
					ToolkitOB.registerDevice(userNameEncoded, passwordEncoded, deviceRefID, registerDeviceCB);

				}// cb

			)// getDataProtectionKey()
		}
	} else {
		hideLoader();
		alert(error.message);
	}
}

function ToolkitVersion() {
	if (null === readerClass || undefined === readerClass) {
		alert("The Webcomponent is not initialized.");
		return;
	}
	showLoader();
	ToolkitOB.getToolkitVersion(ToolkitVersionCB);
}

var ToolkitVersionCB = function (response, error) {
	hideLoader();
	if (error !== null) {
		alert(error.message);
		changeButtonState(false);
		return;
	}

	var result = response;
	if ('Fail' === result.status) {
		alert("Error while getting Toolkit Version :" + result.error + ' : ' + result.description);
	}

	// var str = result.etc_major + "." +
	// 	result.etc_minor + "." +
	// 	result.etc_patch;
	alert("Toolkit Version :" + result);
}

function generateRandomString(length) {
	var text = "";
	var possible = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
	for (var i = 0; i < length; i++) {
		text += possible.charAt(Math.floor(Math.random() * possible.length));
	}
	return text;
}
var registerDeviceCB = function (response, error) {
	hideLoader();
	if (null !== response) {
		alert("Registration Successfull Your ID is:" + response.deviceRegistrationID);
	} else {
		alert(error.description || error.message);
	}
}

function registerDevice() {
	/* generate the random string */
	var randomStr = generateRandomString(40);
	/*  convert randomString to base64 */
	var requestId = btoa(randomStr);
	showLoader();
	ToolkitOB.prepareRequest(requestId, prepareRequestCB);
}

function deviceId() {
	if (null === ToolkitOB || undefined === ToolkitOB) {
		alert('ERROR : Application is not initiaized.');
		return;
	}
	changeButtonState(true);
	ToolkitOB.getDeviceId(deviceIdCB);
}

var deviceIdCB = function (response, error) {
	if (error) {
		changeButtonState(false);
		alert(error.message);
		// self.IsNfc = true;
		changeButtonState(false);
		return;
	}
	changeButtonState(false);
	alert(response);
}

function setVerifyXml() {
	verifyXML(self.verifyxmldata);
}

function removeTable() {
	var tbl = document.getElementById('verifyxmltbl');
	if (tbl) tbl.parentNode.removeChild(tbl);
}

function verifyXML(xml) {
	var ValidateXML = verifyXMlOnServer(xml);
	// var msgbox = document.getElementById('verifyxmlmsg');
	if (ValidateXML.status === 'SUCCESS') {
		document.getElementById('res').style.display = "block";
		document.getElementById('res').value = ValidateXML.message;
		document.getElementById('res').style.color = "Green";
	} else {
		// alert(ValidateXML);
		document.getElementById('res').style.display = "block";
		document.getElementById('res').value = ValidateXML.message;
		document.getElementById('res').style.color = "red";
	}
}

function getEidaDate(value) {
	var dates = value.split('-');
	dates[0] = dates[0].slice(-2);
	value = dates.join("");
	return value;
}

function PrepareRequest(callback) {
	if (null === readerClass || undefined === readerClass) {
		alert('ERROR : Reader is not initiaized.');
		return;
	}
	var randomStr = generateRandomString(40);
	var requestId = btoa(randomStr);
	readerClass.prepareRequest(requestId,
		function (response, error) {
			if (null == error) {
				var requestHandle = response;
				callback(requestHandle);
			} else {
				console.log(error);
				hideLoader();
				alert(error.message);
				// throw error
			}
		}
	);
}

function VerifyToolkitResponse() {
	document.getElementById("vxs").style.display = "none";
	document.getElementById('res').style.display = "none";
	showDiv("VerifyToolkitResponseDiv");
}

function verifyToolkitResponseSubmitBtn() {

	let certDataPath = document.getElementById("verifyResponseCertPathTxtBx").value;
	let certChainDataPath = document.getElementById("verifyResponseCertChainPathTxtBx").value;
	let toolkitResponse = document.getElementById("verifyResponseTextarea").value;
	if (toolkitResponse === null && toolkitResponse == undefined) {
		alert("Please provide toolkitResponse");
	} else {
		changeButtonState(true);
		ToolkitOB.getverifyToolkitResponse(toolkitResponse, certDataPath, certChainDataPath, verifyToolkitResponseSubmitBtnCB);
	}
}

var verifyToolkitResponseSubmitBtnCB = function (response, error) {
	if (null !== error) {
		changeButtonState(false);
		alert(error.message);
		if (error.toolkit_response !== null && error.toolkit_response !== undefined) {
			document.getElementById("vxs").style.display = "block";
			self.verifyxmldata = error.toolkit_response;
		}
		changeButtonState(false);
		return;
	}
	showDiv("verifyToolkitResponseDiv2");
	if (response.status == "fail" && response.validation_status == 1) {
		alert(response.validation_message);
	}
	if (response.status == "fail" && response.validation_status == -1) {
		alert(response.errormessage);
	}
	if (response.status == "success") {
		document.getElementById("Service_Data").innerHTML = response.service;
		document.getElementById("Action_data").innerHTML = response.action;
		document.getElementById("CSN_data").innerHTML = response.csn;
		document.getElementById("CardNumber_data").innerHTML = response.cardnumber;
		document.getElementById("IdNumber_data").innerHTML = response.idnumber;
		document.getElementById("TimeStamp_data").innerHTML = response.time_stamp;
	}
	changeButtonState(false);
}

function publicDataEfType() {
	document.getElementById("vxs").style.display = "none";
	document.getElementById('res').style.display = "none";
	showDiv("publicDataEfTypeDiv");
}

function publicDataEfTypeData() {
	if (null === readerClass || undefined === readerClass) {
		alert('ERROR : Reader is not initiaized.');
		return;
	}
	var public_data_ef_type = document.getElementById("PublicDataEfTypeSelect").value;
	if (undefined === public_data_ef_type || "Select One" === public_data_ef_type) {
		alert("Please Select EF Type");
		return;
	}

	changeButtonState(true);
	readerClass.readPublicDataEF(public_data_ef_type, readPublicDataEFCB);
}

var readPublicDataEFCB = function (response, error) {
	if (null !== error) {
		changeButtonState(false);
		alert(error.message);
		changeButtonState(false);
		return;
	}
	result = response;
	alert(result.ef_raw_data);
	parsedEFData(result.ef_raw_data);
	if (self.IsNfc) {
		nfcMenu();
	}
	changeButtonState(false);
}

function parsedEFData(ef_data) {
	parseEFData(ef_data, parsedEFDataCB);
}

var parsedEFDataCB = function (response, error) {
	if (null !== error) {
		changeButtonState(false);
		alert(error.message);
		changeButtonState(false);
		return;
	}
	result = response;
	alert("ParsedData :: " + result.response);
	if (self.IsNfc) {
		nfcMenu();
	}
	changeButtonState(false);
}

function getCSN() {
	if (null === readerClass || undefined === readerClass) {
		alert('ERROR : Reader is not initiaized.');
		return;
	}
	changeButtonState(true);
	readerClass.getCardSerialNumber(csnCB);
}

var csnCB = function (response, error) {
	hideLoader();
	if (null !== error) {
		changeButtonState(false);
		alert(error.message);
		changeButtonState(false);
		return;
	}
	result = response;
	alert(result.CSN);
	changeButtonState(false);
}

function getLicenseExpiryDate() {
	changeButtonState(true);
	ToolkitOB.getLicenseExpiryDate(getLicenseExpiryDateCB);
}

var getLicenseExpiryDateCB = function (response, error) {
	hideLoader();
	if (null !== error) {
		changeButtonState(false);
		alert(error.message);
		changeButtonState(false);
		return;
	}
	result = response;
	alert(result.expirydate);
	changeButtonState(false);
}

function getReadData() {
	document.getElementById("vxs").style.display = "none";
	document.getElementById('res').style.display = "none";
	showDiv("ReadDataDiv")
}

function readDataFileTypeData() {
	if (null === readerClass || undefined === readerClass) {
		alert('ERROR : Reader is not initiaized.');
		return;
	}
	changeButtonState(true);
	if (null === readerClass || undefined === readerClass) {
		alert('ERROR : Websocket is not initilaized.');
		return;
	}
	var read_data_file_type = document.getElementById("readDataFileTypeSelect").value;
	var randomStr = generateRandomString(40);
	var requestId = btoa(randomStr);
	changeButtonState(true);
	readerClass.readData(requestId, read_data_file_type, readDataFileTypeDataCB);
}

var readDataFileTypeDataCB = function (response, error) {
	if (null !== error) {
		changeButtonState(false);
		alert(error.message);
		changeButtonState(false);
		return;
	}

	document.getElementById('displayReadData').style.display = null;
	if (null !== error) {
		alert(error.message);
		changeButtonState(false);
		return;
	}
	for (let i = 0; i < response.resource.length; i++) {
		dataBindDom(response.resource[i], 'Resources');
	}
	dataBindDom(response.OrganDonor, 'OrganDonar');
	changeButtonState(false);
}

function getUpdateData() {
	document.getElementById("vxs").style.display = "none";
	document.getElementById('res').style.display = "none";
	showDiv("UpdateDataDiv")
}

function updateDataFileTypeData() {
	if (null === readerClass || undefined === readerClass) {
		alert('ERROR : Reader is not initiaized.');
		return;
	}
	changeButtonState(true);
	if (null === readerClass || undefined === readerClass) {
		alert('ERROR : Websocket is not initilaized.');
		return;
	}
	var update_data_file_type = document.getElementById("updateDataFileTypeSelect").value;
	var randomStr = generateRandomString(40);
	var requestId = btoa(randomStr);
	readerClass.updateData(requestId, update_data_file_type, updateDataFileTypeDataCB);

}

var updateDataFileTypeDataCB = function (response, error) {
	hideLoader();
	if (error !== null) {
		alert(error.message);
		changeButtonState(false);
		return;
	}
	showDiv("updateDataDiv");
	document.getElementById("updateDataTxtXMlrow").style.display = null;
	document.getElementById("updateDataTxtXML").value = response.xmlString;
	changeButtonState(false);
}

function getConfigFilesExpiryDates() {
	document.getElementById("vxs").style.display = "none";
	document.getElementById('res').style.display = "none";
	changeButtonState(true);
	ToolkitOB.getConfigFilesExpiryDates(getConfigFilesExpiryDatesCB);
}
var getConfigFilesExpiryDatesCB = function (response, error) {
	hideLoader();
	if (null !== error) {
		changeButtonState(false);
		alert(error.message);
		changeButtonState(false);
		return;
	}
	if (!(null == response.config_lv_cert_expiry && undefined == response.config_lv_cert_expiry))
		document.getElementById('configLvCertExpiry_date').innerHTML = response.config_lv_cert_expiry;

	if (!(null == response.config_vg_cert_expiry && undefined == response.config_vg_cert_expiry))
		document.getElementById('configVgCertExpiry_date').innerHTML = response.config_vg_cert_expiry;

	if (!(null == response.config_ag_cert_expiry && undefined == response.config_ag_cert_expiry))
		document.getElementById('configAgCertExpiry_date').innerHTML = response.config_ag_cert_expiry;

	if (!(null == response.license_expiry && undefined == response.license_expiry))
		document.getElementById('licenseExpiry_date').innerHTML = response.license_expiry;

	if (!(null == response.server_tls_cert_expiry && undefined == response.server_tls_cert_expiry))
		document.getElementById('serverTlsCertExpiry_date').innerHTML = response.server_tls_cert_expiry;


	showDiv("configExpireyDatesDiv");
	changeButtonState(false);
}

function GetReaderNameAndSerialNumber(){
	if (null === readerClass || undefined === readerClass) {
		alert('ERROR : Reader is not initiaized.');
		return;
	}
	var ReaderName = readerClass.getReaderName();
	var ReaderSerialNumber = readerClass.getReaderSerialNumber();
	document.getElementById('readerNameId').value = ReaderName;
	document.getElementById('readerSerialNumberId').value = ReaderSerialNumber;
	showDiv("ReaderNameAndSerialNumberDiv");
}