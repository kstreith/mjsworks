using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.Meta;
using Wyam.Common.Modules;

namespace MjsWorks.AmazonPay.Wyam
{
    public class AmazonPay : IModule
    {
        private string _signingKey;
        private string _awsPublicAccessKey;
        private string _awsSellerId;
        private string _awsLwaClientId;
        private string _purchaseCompletePage;
        private string _customPort;
        private string _priceKey = "Price";
        private string _skuKey = "Sku";
        private bool _isSellerNoteHardwireded = true;
        private string _sellerNoteValue = "Description";
        private string _outputAwsJsonKey = "AmazonPayJson";
        public AmazonPay()
        {
        }

        public string GenerateAmazonPayJson(string staticSigningKey, string awsPublicAccessKey, string awsSellerId, string awsLwaClientId, string returnUrl, string amount, string sellerNote, string sellerOrderId)
        {
            var dict = new SortedDictionary<string, string>
            {
                ["shippingAddressRequired"] = "true",
                ["currencyCode"] = "USD",
                ["paymentAction"] = "Authorize",
                ["accessKey"] = awsPublicAccessKey,
                ["amount"] = amount,
                ["lwaClientId"] = awsLwaClientId,
                ["returnURL"] = returnUrl,
                ["sellerId"] = awsSellerId,
                ["sellerNote"] = sellerNote,
                ["sellerOrderId"] = sellerOrderId
            };

            var urlEntries = new List<string>();
            foreach (var kvPair in dict)
            {
                urlEntries.Add($"{Encode(kvPair.Key)}={Encode(kvPair.Value)}");
            }
            var urlToSign = $"static-site-generation:mjsworks:{string.Join("&", urlEntries)}";

            var signature = GenerateSignature(urlToSign, staticSigningKey);
            dict["static_generation_signature"] = signature;
            return JsonConvert.SerializeObject(dict);
        }

        private string Encode(string key)
        {
            var encoded = Uri.EscapeDataString(key);
            return encoded;
        }

        private string GenerateSignature(string dataToSign, string signingKey)
        {
            var encoding = new ASCIIEncoding();
            byte[] signingKeyBytes = encoding.GetBytes(signingKey);
            byte[] dataToSignBytes = encoding.GetBytes(dataToSign);
            using (var hmacsha256 = new HMACSHA256(signingKeyBytes))
            {
                byte[] hashmessage = hmacsha256.ComputeHash(dataToSignBytes);
                var signature = Convert.ToBase64String(hashmessage);
                return signature;
            }
        }

        public AmazonPay SignWithKey(string signingKey)
        {
            _signingKey = signingKey;
            return this;
        }

        public AmazonPay WithAmazonPayConfig(string awsPublicAccessKey, string awsSellerId, string awsLwaClientId)
        {
            _awsPublicAccessKey = awsPublicAccessKey;
            _awsSellerId = awsSellerId;
            _awsLwaClientId = awsLwaClientId;
            return this;
        }

        public AmazonPay WithPurchaseCompletionPage(string purchaseCompletePage, string customPort = null)
        {
            _purchaseCompletePage = purchaseCompletePage;
            _customPort = customPort;
            return this;
        }

        public AmazonPay PriceFromMetadataKey(string priceKey)
        {
            _priceKey = priceKey;
            return this;
        }

        public AmazonPay SkuFromMetadataKey(string skuKey)
        {
            _skuKey = skuKey;
            return this;
        }

        public AmazonPay WithStaticSellerNote(string sellerNoteText)
        {
            _isSellerNoteHardwireded = true;
            _sellerNoteValue = sellerNoteText;
            return this;
        }

        public AmazonPay SellerNoteFromMetadataKey(string sellerNoteKey)
        {
            _isSellerNoteHardwireded = false;
            _sellerNoteValue = sellerNoteKey;
            return this;
        }

        public AmazonPay OutputMetadataWithKey(string awsPaymentJsonKey)
        {
            _outputAwsJsonKey = awsPaymentJsonKey;
            return this;
        }

        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            if (_signingKey == null)
            {
                throw new ArgumentNullException("signingKey", "SignWithKey not called");
            }
            if (_awsPublicAccessKey == null)
            {
                throw new ArgumentNullException("awsPublicAccessKey", "WithAmazonPayConfig not called");
            }
            if (_awsSellerId == null)
            {
                throw new ArgumentNullException("awsSellerId", "WithAmazonPayConfig not called");
            }
            if (_awsLwaClientId == null)
            {
                throw new ArgumentNullException("awsLwaClientId", "WithAmazonPayConfig not called");
            }
            return inputs.Select(context, doc =>
            {
                var host = doc.String("Host");
                var useHttps = doc.Bool("LinksUseHttps");

                var returnUrl = $"{(useHttps ? "https://" : "http://")}{host}{(_customPort != null ? ":" + _customPort : "")}/{_purchaseCompletePage}";
                var price = doc.Get<double>(_priceKey);
                var formattedPrice = $"{price:F2}";
                var sku = doc.String(_skuKey);
                string sellerNote = null;
                if (_isSellerNoteHardwireded)
                {
                    sellerNote = _sellerNoteValue;
                } else
                {
                    sellerNote = doc.String(_sellerNoteValue);
                }
                var awsPayJson = GenerateAmazonPayJson(
                    _signingKey,
                    _awsPublicAccessKey,
                    _awsSellerId,
                    _awsLwaClientId,
                    returnUrl,
                    formattedPrice,
                    sellerNote,
                    sku);
                return context.GetDocument(doc, new Dictionary<string, object>
                {
                    [_outputAwsJsonKey] = awsPayJson
                });
            });
        }
    }
}
