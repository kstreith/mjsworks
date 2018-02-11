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
    function show(selector) {
        var domNode = document.querySelector(selector);
        if (!domNode) {
            return;
        }
        domNode.style.display = "block";
    }
    function hide(selector) {
        var domNode = document.querySelector(selector);
        if (!domNode) {
            return;
        }
        domNode.style.display = "none";
    }
    function setText(selector, text) {
        var domNode = document.querySelector(selector);
        if (!domNode) {
            return;
        }
        domNode.innerText = text;        
    }
    function displayMessage(data, validMessage, technicalCode) {
        hide("#loading");
        if (!validMessage) {
            show("#technicalIssuesPurchase");
            if (technicalCode) {
                setText("#technicalCode", technicalCode);
            }
            return;
        }
        if (data.resultCode === "Failure" && data.failureCode === "BuyerAbandoned") {
            show("#abandonedPurchase");
            return;
        }
        if (data.resultCode === "Failure" && data.failureCode === "AmazonRejected") {
            show("#rejectedPurchase");
            return;
        }
        if (data.resultCode === "Success" && data.orderReferenceId) {
            setText("#orderNumber", data.orderReferenceId);
            show("#successfulPurchase");
            return;
        }
        show("#technicalIssuesPurchase");
        if (technicalCode) {
            setText("#technicalCode", technicalCode);
        }
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
                displayMessage(null, null, "E200");
            } else {
                var originalSignature = window.decodeURIComponent(data.signature);
                var validated = !!(originalSignature === responseObj.signature);
                displayMessage(data, validated, validated ? null : "E400");
            }
        });
    }
    if (!window.fetch) {
        displayMessage(null, null, "E300");
    }
    else {
        var data = parseQuery();
        validateResult(data);      
    }
})();