var crypto = require('crypto');
var querystring = require('querystring');

module.exports = function (context, data) {
    context.log('Webhook was triggered!');

    var request = data;
    function generateUrlToSign(urlPrefix, reqParams) {
        var urlToSign = urlPrefix;
        for (var key of Object.keys(reqParams).sort()) {
            urlToSign += querystring.escape(key) + "=" + querystring.escape(reqParams[key]) + "&";
        }
        urlToSign = urlToSign.slice(0, -1);
        return urlToSign;
    }

    function generateSignature(dataToSign, sharedSecret) {
        var signature = crypto.createHmac("sha256", sharedSecret).update(dataToSign).digest("base64");
        return signature;
    }
    function validateIncomingData(request) {
        var data = JSON.parse(JSON.stringify(request));
        var static_generation_signature = data.static_generation_signature;
        delete data.static_generation_signature;

        var staticGenStringToSign = generateUrlToSign("static-site-generation:mjsworks:", data);
        var staticGenSharedSecret = process.env["MjsStaticGenerationSig"];
        var calculatedStaticSignature = generateSignature(staticGenStringToSign, staticGenSharedSecret);

        context.log('staticGenStringToSign', staticGenStringToSign);
        context.log('input', JSON.stringify(data));
        context.log('calculatedStaticSignature', calculatedStaticSignature);
        context.log('static_generation_signature', static_generation_signature);
        if (calculatedStaticSignature !== static_generation_signature) {
            throw new Error("Invalid signature");
        }
        
        return data;
    }
    try {
        var response = validateIncomingData(request);
        context.log('incoming data validated');
        var awsUrlToSign = generateUrlToSign("POST\npayments.amazon.com\n/\n", response);
        var awsSharedSecret = process.env["MjsWorksPurchaseSig"];
        var awsSignature = generateSignature(awsUrlToSign, awsSharedSecret);
        response.signature = awsSignature;
        context.res = {
            body: response,
        };

        context.done();
    } catch (e) {
        context.log('handled exception', e);
        context.res = {
            body: 'Validation failed',
            status: 400
        }
        context.done();
    }
}
