(function () {
    OffAmazonPayments.Button("AmazonPayButton", "A3IO5ZO9PFIM3P", {
        type: "hostedPayment",
        hostedParametersProvider: function(done) {
             fetch('https://mjsworksfuncs.azurewebsites.net/api/GetPurchaseSignature?code=TcvVgMV7zMoa7CkJl6acjliFI032ZEYjA8yVxgYJIp1pBUNGv3vsqg==&clientId=default',
                 { method: 'POST',
                 body: JSON.stringify(mjsworks.purchaseConfig),
                 headers: { 'content-type': 'application/json' }
             }).then(function (response) {
               if (response.status === 200) {
                 return response.json();
               }
               return "failed";          
             }).then(function (responseObj) {
               if (responseObj === "failed") {
                 alert('There was a problem processing your request');
               } else {
                 done(responseObj);
               }
             });
        }
    });
})();