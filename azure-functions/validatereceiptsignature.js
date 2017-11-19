var crypto = require('crypto');
var querystring = require('querystring');

module.exports = function (context, data) {
    context.log('Webhook was triggered!');

    //resultCode=Success
    //&signature=DbooxKLL2RFPzeo20mqQUgDxlXjBQcpWu7QTr4Ux%252B%252FA%253D
    //&sellerId=A3IO5ZO9PFIM3P&AWSAccessKeyId=AKIAIM3SZ2U2PKXWGYYA
    //&SignatureMethod=HmacSHA256
    //&SignatureVersion=2
    //&orderReferenceId=S01-8318015-3290404
    //&amount=32.11
    //&currencyCode=USD
    //&paymentAction=Authorize
    //&sellerOrderId=sku1000
    // Check if we got first/last properties
    var objectToValidate = JSON.parse(JSON.stringify(data));
    delete objectToValidate.mode;
    delete objectToValidate.signature;
    //delete objectToValidate.SignatureMethod;
    //delete objectToValidate.SignatureVersion;

    function generateUrlToSign(reqParams, mode) {
        var urlToSign = "GET\n"; 
        if (mode === "local") {
            urlToSign += "localhost:8080\n"
            urlToSign += "/finalizedSale.html\n";
        } else {
            throw new Error("Invalid mode used " + mode);
        }
        for (var key of Object.keys(reqParams).sort()) {
            urlToSign += querystring.escape(key) + "=" + querystring.escape(reqParams[key]) + "&";
        }
        urlToSign = urlToSign.slice(0, -1);
        return urlToSign;
    }

    function generateSignature(dataToSign) {
        var sharedSecret = process.env["MjsWorksPurchaseSig"];
        var signature = crypto.createHmac("sha256", sharedSecret).update(dataToSign).digest("base64");
        return signature;
    }

    var urlToSign = generateUrlToSign(objectToValidate, data.mode);
    context.log('urlToSign ' + urlToSign)
    var signature = generateSignature(urlToSign);
    var response = {};
    response.signature = signature;
    // Check if we got first/last properties
    context.res = {
        body: response
    };
    
    context.done();
}
