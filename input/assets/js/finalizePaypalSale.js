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
        if (data.orderId) {
            setText("#orderNumber", data.orderId);
            show("#successfulPurchase");
            return;
        }
        show("#technicalIssuesPurchase");
        if (technicalCode) {
            setText("#technicalCode", technicalCode);
        }
    }
    function validateResult(data) {
        displayMessage(data, true);
    }
    if (!window.fetch) {
        displayMessage(null, null, "E300");
    }
    else {
        var data = parseQuery();
        validateResult(data);      
    }
})();