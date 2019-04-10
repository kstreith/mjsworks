(function () {
    if (!window.fetch) {
        var payBtn = document.querySelector("#paypal-button-container");
        if (payBtn) {
          payBtn.innerText = "Purchasing has been disabled until you upgrade your browser."
          payBtn.classList.add("disabledPurchase");
        }
    } else {
        paypal.Buttons({
            createOrder: function(data, actions) {
            // Set up the transaction
            mjsworks.hide("#failedPurchase");
            mjsworks.hide("#processingPurchase");
            return actions.order.create({
                purchase_units: [{
                amount: {
                    value: mjsworks.purchaseConfig.amount
                },
                description: mjsworks.purchaseConfig.sellerNote,
                invoice_id: mjsworks.purchaseConfig.sellerOrderId
                }]
            });
            },
            onApprove: function(data, actions) {
                mjsworks.show("#processingPurchase");
                return actions.order.capture().then(function(details) {
                    mjsworks.hide("#processingPurchase");
                    var receiptUrl = "/finalizedSale.html?orderId=" + window.encodeURIComponent(details.id);
                    window.location.href = receiptUrl;
                }).catch(function () {
                    mjsworks.show("#failedPurchase");
                    mjsworks.hide("#processingPurchase");
                });;
            }
        }).render('#paypal-button-container');    
    }
})();