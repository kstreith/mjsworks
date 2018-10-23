using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using Wyam.Common.Documents;
using Wyam.Common.Meta;
using Wyam.Testing.Documents;
using Wyam.Testing.Execution;
using Xunit;
using FluentAssertions;
using System;

namespace MjsWorks.AmazonPay.Wyam.Test
{
    public class AmazonPayTest
    {
        [Fact]
        public void Execute_Works()
        {
            //Arrange
            var pay = new AmazonPay();
            pay.SignWithKey("fakeKey")
                .WithAmazonPayConfig("fake-public-key", "fake-seller-id", "fake-lwa-client-id")
                .WithStaticSellerNote("Original painting").WithPurchaseCompletionPage("finalizedSale.html");
            var testContext = new TestExecutionContext();
            testContext.Settings["Host"] = "localhost";
            var testDocument = new TestDocument(new Dictionary<string, object>
            {
                ["Price"] = (double)5,
                ["Sku"] = "fake-sku"
            });

            //Act
            var outputDoc = pay.Execute(new List<IDocument> { testDocument }, testContext).First();

            //Assert
            outputDoc.Should().NotBeNull();
            var awsJson = outputDoc.String("AmazonPayJson");
            awsJson.Should().NotBeNull();
            var awsResult = JsonConvert.DeserializeObject<Dictionary<string, string>>(awsJson);
            awsResult.Should().Contain("accessKey", "fake-public-key");
            awsResult.Should().Contain("amount", "5.00");
            awsResult.Should().Contain("currencyCode", "USD");
            awsResult.Should().Contain("lwaClientId", "fake-lwa-client-id");
            awsResult.Should().Contain("paymentAction", "Authorize");
            awsResult.Should().Contain("returnURL", "http://localhost/finalizedSale.html");
            awsResult.Should().Contain("sellerId", "fake-seller-id");
            awsResult.Should().Contain("sellerNote", "Original painting");
            awsResult.Should().Contain("sellerOrderId", "fake-sku");
            awsResult.Should().Contain("shippingAddressRequired", "true");
            awsResult.Should().Contain("static_generation_signature", "TPsVpSlieh13pEq3ssb6CrkQO10eu/InnyD3XqsyVMQ=");
        }

        [Fact]
        public void GenerateAmazonPayJson_Works()
        {
            var module = new AmazonPay();

            var jsonPayload = module.GenerateAmazonPayJson(
                staticSigningKey: "PXwaHZRatHPiIPpaKPwuGRRZFOTvwhEs-fake",
                awsPublicAccessKey: "fake-public-key",
                awsSellerId: "fake-seller-id",
                awsLwaClientId: "fake-lwa-client-id",
                returnUrl: "http://localhost:8080/finalizedSale.html",
                amount: "300.00",
                sellerNote: "Original encaustic painting",
                sellerOrderId: "p17029e");

            Assert.Equal(@"{""accessKey"":""fake-public-key"",""amount"":""300.00"",""currencyCode"":""USD"",""lwaClientId"":""fake-lwa-client-id"",""paymentAction"":""Authorize"",""returnURL"":""http://localhost:8080/finalizedSale.html"",""sellerId"":""fake-seller-id"",""sellerNote"":""Original encaustic painting"",""sellerOrderId"":""p17029e"",""shippingAddressRequired"":""true"",""static_generation_signature"":""MIDqs1O/r1R3faxgSaJwtIKsVY/cHPcIyaJ3yAowjDw=""}", jsonPayload);
        }
    }
}
