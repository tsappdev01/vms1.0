
var ToolkitOB = null;
var readerClass = null;
var readerName = null;
var options = {
	"debugEnabled": true,
	"agent_tls_enabled": false,
	"agent_host_name": "toolkitagent.emiratesid.ae"
};
var LOG = console.log.bind(console);

var errorHandlerCB = function (err) {

    LOG("errorHandlerCB");

	readerClass = null;
	ToolkitOB = null;

    if (null !== err) {
		alert('errorHandler ERROR : ' + err);
	}
}

var closeHandlerCB = function (response) {

    LOG("closeHandlerCB");
    ToolkitOB = null;
	readerClass = null;
}

var onOpenHandlerCB = function (response, error) {

    LOG("onOpenHandlerCB, response: " + response);
    if (error) {
		ToolkitOB = null;
    }
}

function Initialize() {

	try {

		if (ToolkitOB !== null) {
            return 'WebSocket is already active ...';
		}

		ToolkitOB = new Toolkit(
			onOpenHandlerCB,
			closeHandlerCB,
			errorHandlerCB,
			options
		);

        LOG("ToolkitOB created");

	} catch (e) {
		alert("Webcomponent Initialization Failed, Details: " + e);
	}
}

function ListReaders() {

	try {

		if (ToolkitOB == null) {
            return 'Toolkit not initialized...';
		}

        ToolkitOB.listReaders(function(response, error) {

            LOG("listReader CallBack");
        
            if (error !== null) {
                alert(error.message || error.description);
                ToolkitOB = null;
            } else {
        
                var readerList = response;
                if (readerList && 0 < readerList.length) {
                    readerClass = readerList[0];
                } else {
                    return 'No readers found';
                }
            }
        });

	} catch (e) {
		alert("Webcomponent Initialization Failed, Details: " + e);
	}
}

function Connect() {

	try {

		if ((ToolkitOB == null) || (readerClass == null)) {
            return 'Toolkit not initialized...';
		}

        readerClass.connect(function(response, error) {

            if (null !== error) {
                alert(error.code + ' : ' + error.message);
                return;
            }
        
            LOG("readerClass.connect successful");
        });

	} catch (e) {
		alert("Webcomponent Initialization Failed, Details: " + e);
	}
}

function generateRandomString(length) {
	var text = "";
	var possible = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
	for (var i = 0; i < length; i++) {
		text += possible.charAt(Math.floor(Math.random() * possible.length));
	}
	return text;
}

function CheckCardStatus() {

	if ((null === readerClass) || (undefined === readerClass)) {
		alert('ERROR : Reader is not initiaized.');
		return;
	}
    
	var randomStr = generateRandomString(40);
	var requestId = btoa(randomStr);
	readerClass.checkCardStatus(requestId, function(response, error) {

        if (error !== null) {
            LOG(error.message);
            return;
        }

        if ((response.xmlString !== null) &&
        (response.xmlString !== undefined)) {
            document.querySelector("#response").innerHTML = response.xmlString;
        }
    });
}

function AuthenticateFaceWithID() {

    if (ToolkitOB == null) {
        return 'Toolkit not initialized...';
    }

	var randomStr = generateRandomString(40);
	var requestId = btoa(randomStr);
	ToolkitOB.authenticateFaceWithID(requestId, "784197840906685", false,
     function(response, error) {

        if (error !== null) {
            LOG(error.message);
            return;
        }

        if ((response.xmlString !== null) &&
        (response.xmlString !== undefined)) {
            document.querySelector("#response").innerHTML = response.xmlString;
        }
    });
}

function AuthenticateFaceWithPassport() {

    if (ToolkitOB == null) {
        return 'Toolkit not initialized...';
    }

	var randomStr = generateRandomString(40);
	var requestId = btoa(randomStr);
	ToolkitOB.authenticateFaceWithPassport(requestId, "Z4394094",
    "IND", "2028-04-23", "1978-05-31", false,
     function(response, error) {

        if (error !== null) {
            LOG(error.message);
            return;
        }

        if ((response.xmlString !== null) &&
        (response.xmlString !== undefined)) {
            document.querySelector("#response").innerHTML = response.xmlString;
        }
    });
}

function Disconnect() {

	if ((null === readerClass) || (undefined === readerClass)) {
		alert('ERROR : Reader is not initiaized.');
		return;
	}

	readerClass.disconnect(function (response, error) {

        if (error !== null) {
            LOG(error.message);
            return;
        }

        LOG("readerClass.disconnect successful");
    });
}

function Cleanup() {

    if ((ToolkitOB == null) || (readerClass == null)) {
        return 'Toolkit not initialized...';
    }

    readerClass = null;
    ToolkitOB.cleanup();
}
