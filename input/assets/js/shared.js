var mjsworks = mjsworks || {};
(function (e) {
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
        var domNodes = document.querySelectorAll(selector);
        if (!domNodes || !domNodes.length) {
            return;
        }
        for(var i = 0; i < domNodes.length; ++i) {
            var domNode = domNodes[i];
            domNode.style.display = "block";
        }
    }
    function hide(selector) {
        var domNode = document.querySelector(selector);
        if (!domNode) {
            return;
        }
        domNode.style.display = "none";
    }
    e.parseQuery = parseQuery;
    e.show = show;
    e.hide = hide;
}(mjsworks));