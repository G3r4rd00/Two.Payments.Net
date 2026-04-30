using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RichardSzalay.MockHttp;
using Two.Payments.Application;
using Two.Payments.Core.Interfaces;
using Two.Payments.Core.Models;
using Two.Payments.Infrastructure.Configuration;
using Xunit;

namespace Two.Payments.Tests.Services
{
    public class TwoLimitsServiceTests
    {
        private const string FakeApiKey = "test-api-key";

        private static ITwoClient BuildClient(MockHttpMessageHandler mockHttp, bool sandbox = true)
        {
            var options = new TwoOptions
            {
                ApiKey = FakeApiKey,
                UseSandbox = sandbox,
                MaxRetryCount = 0
            };

            var httpClient = mockHttp.ToHttpClient();
            return TwoClientFactory.Create(httpClient, options);
        }

        [Fact]
        public async Task GetBuyerCreditLimitsAsync_ReturnsResponse_WhenApiSucceeds()
        {
            var mockHttp = new MockHttpMessageHandler();
            var expected = new GetBuyerCreditLimitsResponse
            {
                BuyerCountryCode = "ES",
                BuyerOrganizationNumber = "B12345678",
                Currency = "EUR",
                CreditLimit = 10000m,
                AvailableCredit = 7500m,
                UsedCredit = 2500m
            };

            mockHttp
                .When(HttpMethod.Get, "https://api.sandbox.two.inc/limits/v1/company/ES/B12345678")
                .WithHeaders("X-API-Key", FakeApiKey)
                .Respond(HttpStatusCode.OK, "application/json", JsonConvert.SerializeObject(expected));

            var client = BuildClient(mockHttp);

            var result = await client.Limits.GetBuyerCreditLimitsAsync("ES", "B12345678");

            Assert.NotNull(result);
            Assert.Equal("ES", result.BuyerCountryCode);
            Assert.Equal("B12345678", result.BuyerOrganizationNumber);
            Assert.Equal("EUR", result.Currency);
            Assert.Equal(10000m, result.CreditLimit);
            Assert.Equal(7500m, result.AvailableCredit);
            Assert.Equal(2500m, result.UsedCredit);
        }

        [Fact]
        public async Task GetBuyerCreditLimitsAsync_Throws_WhenCountryCodeIsEmpty()
        {
            var client = BuildClient(new MockHttpMessageHandler());

            await Assert.ThrowsAsync<System.ArgumentException>(
                () => client.Limits.GetBuyerCreditLimitsAsync("", "B12345678"));
        }

        [Fact]
        public async Task GetBuyerCreditLimitsAsync_Throws_WhenOrganizationNumberIsEmpty()
        {
            var client = BuildClient(new MockHttpMessageHandler());

            await Assert.ThrowsAsync<System.ArgumentException>(
                () => client.Limits.GetBuyerCreditLimitsAsync("ES", ""));
        }
    }
}
