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
    public class TwoOrderServiceTests
    {
        private const string FakeApiKey = "test-api-key";
        private const string OrderId = "ord_abc123";

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
        public async Task CreateOrderAsync_ReturnsCreatedOrder_WhenApiSucceeds()
        {
            // Arrange
            var mockHttp = new MockHttpMessageHandler();
            var expectedResponse = new CreateOrderResponse
            {
                Id = OrderId,
                Status = "PENDING",
                Currency = "GBP"
            };

            mockHttp
                .When(HttpMethod.Post, "https://sandbox.api.two.inc/v1/order")
                .WithHeaders("X-API-Key", FakeApiKey)
                .Respond(HttpStatusCode.OK, "application/json",
                    JsonConvert.SerializeObject(expectedResponse));

            var client = BuildClient(mockHttp);

            var request = new CreateOrderRequest
            {
                Currency = "GBP",
                InvoiceType = "DIRECT_INVOICE",
                GrossAmount = "100.00",
                NetAmount = "80.00",
                TaxAmount = "20.00",
                DiscountAmount = "0.00",
                DiscountRate = "0.00",
                TaxRate = "0.25",
                Buyer = new Buyer
                {
                    Representative = new BuyerRepresentative
                    {
                        FirstName = "John",
                        LastName = "Doe",
                        Email = "john.doe@example.com",
                        PhoneNumber = "07700900000"
                    },
                    Company = new BuyerCompany
                    {
                        CountryPrefix = "GB",
                        OrganizationNumber = "12345678",
                        CompanyName = "Acme Ltd"
                    }
                }
            };

            // Act
            var result = await client.Orders.CreateOrderAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(OrderId, result.Id);
            Assert.Equal("PENDING", result.Status);
            Assert.Equal("GBP", result.Currency);
        }

        [Fact]
        public async Task GetOrderAsync_ReturnsOrder_WhenApiSucceeds()
        {
            // Arrange
            var mockHttp = new MockHttpMessageHandler();
            var expectedResponse = new GetOrderResponse
            {
                Id = OrderId,
                Status = "APPROVED",
                Currency = "NOK"
            };

            mockHttp
                .When(HttpMethod.Get, $"https://sandbox.api.two.inc/v1/order/{OrderId}")
                .Respond(HttpStatusCode.OK, "application/json",
                    JsonConvert.SerializeObject(expectedResponse));

            var client = BuildClient(mockHttp);

            // Act
            var result = await client.Orders.GetOrderAsync(OrderId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(OrderId, result.Id);
            Assert.Equal("APPROVED", result.Status);
        }

        [Fact]
        public async Task GetOrderAsync_ThrowsTwoApiException_WhenApiReturns404()
        {
            // Arrange
            var mockHttp = new MockHttpMessageHandler();

            mockHttp
                .When(HttpMethod.Get, $"https://sandbox.api.two.inc/v1/order/{OrderId}")
                .Respond(HttpStatusCode.NotFound, "application/json",
                    JsonConvert.SerializeObject(new TwoApiError
                    {
                        ErrorCode = "ORDER_NOT_FOUND",
                        ErrorMessage = "Order not found"
                    }));

            var client = BuildClient(mockHttp);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Core.Exceptions.TwoApiException>(
                () => client.Orders.GetOrderAsync(OrderId));

            Assert.Equal(404, ex.StatusCode);
            Assert.Equal("ORDER_NOT_FOUND", ex.ErrorCode);
        }

        [Fact]
        public async Task ConfirmOrderAsync_CompletesWithoutException_WhenApiSucceeds()
        {
            // Arrange
            var mockHttp = new MockHttpMessageHandler();

            mockHttp
                .When(HttpMethod.Post, $"https://sandbox.api.two.inc/v1/order/{OrderId}/confirm")
                .Respond(HttpStatusCode.OK, "application/json", "{}");

            var client = BuildClient(mockHttp);

            // Act & Assert (should not throw)
            await client.Orders.ConfirmOrderAsync(OrderId);
        }

        [Fact]
        public async Task CancelOrderAsync_CompletesWithoutException_WhenApiSucceeds()
        {
            // Arrange
            var mockHttp = new MockHttpMessageHandler();

            mockHttp
                .When(HttpMethod.Delete, $"https://sandbox.api.two.inc/v1/order/{OrderId}")
                .Respond(HttpStatusCode.OK, "application/json", "{}");

            var client = BuildClient(mockHttp);

            // Act & Assert (should not throw)
            await client.Orders.CancelOrderAsync(OrderId);
        }

        [Fact]
        public async Task CreateOrderAsync_ThrowsTwoApiException_WhenApiReturnsError()
        {
            // Arrange
            var mockHttp = new MockHttpMessageHandler();

            mockHttp
                .When(HttpMethod.Post, "https://sandbox.api.two.inc/v1/order")
                .Respond(HttpStatusCode.UnprocessableEntity, "application/json",
                    JsonConvert.SerializeObject(new TwoApiError
                    {
                        ErrorCode = "VALIDATION_ERROR",
                        ErrorMessage = "Invalid buyer company"
                    }));

            var client = BuildClient(mockHttp);

            var request = new CreateOrderRequest
            {
                Currency = "GBP",
                GrossAmount = "100.00"
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Core.Exceptions.TwoApiException>(
                () => client.Orders.CreateOrderAsync(request));

            Assert.Equal(422, ex.StatusCode);
            Assert.Equal("VALIDATION_ERROR", ex.ErrorCode);
        }

        [Fact]
        public void TwoOptions_GetEffectiveBaseUrl_ReturnsSandboxUrl_WhenUseSandboxIsTrue()
        {
            var opts = new TwoOptions { ApiKey = "key", UseSandbox = true };
            Assert.Equal("https://sandbox.api.two.inc/v1", opts.GetEffectiveBaseUrl());
        }

        [Fact]
        public void TwoOptions_GetEffectiveBaseUrl_ReturnsProductionUrl_WhenUseSandboxIsFalse()
        {
            var opts = new TwoOptions { ApiKey = "key", UseSandbox = false };
            Assert.Equal("https://api.two.inc/v1", opts.GetEffectiveBaseUrl());
        }

        [Fact]
        public void TwoOptions_GetEffectiveBaseUrl_ReturnsCustomUrl_WhenSet()
        {
            var opts = new TwoOptions { ApiKey = "key", BaseUrl = "https://my.custom.host/v2/" };
            Assert.Equal("https://my.custom.host/v2", opts.GetEffectiveBaseUrl());
        }

        [Fact]
        public void TwoOptions_Validate_Throws_WhenApiKeyIsEmpty()
        {
            var opts = new TwoOptions { ApiKey = "" };
            Assert.Throws<System.InvalidOperationException>(() => opts.Validate());
        }
    }
}
