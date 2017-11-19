(function () {
    function parseQuery() {
        var queryString = window.location.search;
        var query = {};
        var pairs = (queryString[0] === '?' ? queryString.substr(1) : queryString).split('&');
        for (var i = 0; i < pairs.length; i++) {
            var pair = pairs[i].split('=');
            query[decodeURIComponent(pair[0])] = decodeURIComponent(pair[1] || '');
        }
        return query;
    }
    function displayMessage(data, validMessage) {
        if (!validMessage) {
            $("#technicalIssuesPurchase").show();
            return;            
        }
        if (data.resultCode === "Failure" && data.failureCode === "BuyerAbandoned") {
            $("#abandonedPurchase").show();
            return;
        }
        if (data.resultCode === "Failure" && data.failureCode === "AmazonRejected") {
            $("#rejectedPurchase").show();
            return;
        }
        if (data.resultCode === "Success" && data.orderReferenceId) {
            $("#orderNumber").text(data.orderReferenceId);
            $("#successfulPurchase").show();
            return;
        }
        $("#technicalIssuesPurchase").show();        
    }
    function validateResult(data) {
        data.mode = mjsworks.mode;
        fetch('https://mjsworksfuncs.azurewebsites.net/api/ValidateReceiptSignature?code=Aj52Rvg73Hj0J5equro2cUg9fINaDW7QybjoQdDMpO/dhQbQl416Ng==&clientId=default', {
            method: 'POST',
            body: JSON.stringify(data),
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
                var originalSignature = window.decodeURIComponent(data.signature);
                var validated = !!(originalSignature === responseObj.signature);
                displayMessage(data, validated);
            }
        });
    }
    var data = parseQuery();
    validateResult(data);      
})();