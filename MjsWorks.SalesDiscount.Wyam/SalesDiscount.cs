using System.Collections.Generic;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.Meta;
using Wyam.Common.Modules;

namespace MjsWorks.SalesDiscount.Wyam
{
    public class SalesDiscount : IModule
    {
        private string _priceKey = "Price";
        private string _outputOriginalPriceKey = "OriginalPrice";
        private double? _discount = null;
        public SalesDiscount()
        {
        }

        public SalesDiscount WithDiscount(double? discount)
        {
            _discount = discount;
            return this;
        }

        public SalesDiscount FromMetadataKey(string priceKey)
        {
            _priceKey = priceKey;
            return this;
        }

        public SalesDiscount OutputMetadataKey(string originalPriceKey)
        {
            _outputOriginalPriceKey = originalPriceKey;
            return this;
        }

        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            return inputs.Select(context, doc =>
            {
                if (_discount == null)
                {
                    return context.GetDocument(doc, null);
                }
                var originalPrice = doc.Get<double>(_priceKey);
                var price = originalPrice * (1 - _discount);
                return context.GetDocument(doc, new Dictionary<string, object>
                {
                    [_priceKey] = price,
                    [_outputOriginalPriceKey] = originalPrice
                });
            });
        }
    }
}
