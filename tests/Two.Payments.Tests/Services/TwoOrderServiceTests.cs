using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
        public void CreateOrderRequest_SerializesNumericFields_AsJsonNumbers()
        {
            var request = new CreateOrderRequest
            {
                Currency = "GBP",
                InvoiceType = "DIRECT_INVOICE",
                GrossAmount = "120.00",
                NetAmount = "100.00",
                TaxAmount = "20.00",
                DiscountAmount = "0.00",
                DiscountRate = "0.00",
                TaxRate = "0.20",
                BillingAddress = new BillingAddress
                {
                    Address = "221B Baker Street",
                    PostalCode = "NW1 6XE",
                    City = "London",
                    CountryPrefix = "GB",
                    OrganizationName = "Acme Ltd"
                },
                ShippingAddress = new BillingAddress
                {
                    StreetAddress = "221B Baker Street",
                    PostalCode = "NW1 6XE",
                    City = "London",
                    Country = "GB",
                    OrganizationName = "Acme Ltd"
                },
                LineItems = new System.Collections.Generic.List<LineItem>
                {
                    new LineItem
                    {
                        Description = "Test Product",
                        Quantity = 1,
                        QuantityUnit = "pcs",
                        UnitPrice = "100.00",
                        NetAmount = "100.00",
                        TaxAmount = "20.00",
                        GrossAmount = "120.00",
                        TaxRate = "0.20",
                        DiscountAmount = "0.00",
                        Type = "PHYSICAL",
                        ProductId = "TEST-001"
                    }
                }
            };

            var json = JsonConvert.SerializeObject(request, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });

            var body = JObject.Parse(json);

            Assert.NotEqual(JTokenType.String, body["gross_amount"].Type);
            Assert.NotEqual(JTokenType.String, body["net_amount"].Type);
            Assert.NotEqual(JTokenType.String, body["tax_amount"].Type);
            Assert.NotEqual(JTokenType.String, body["discount_amount"].Type);
            Assert.NotEqual(JTokenType.String, body["discount_rate"].Type);
            Assert.NotEqual(JTokenType.String, body["tax_rate"].Type);
            Assert.Equal("Acme Ltd", (string)body["billing_address"]["organization_name"]);
            Assert.Equal("221B Baker Street", (string)body["billing_address"]["street_address"]);
            Assert.Equal("NW1 6XE", (string)body["billing_address"]["postal_code"]);
            Assert.Equal("London", (string)body["billing_address"]["city"]);
            Assert.Equal("GB", (string)body["billing_address"]["country"]);
            Assert.Equal("Acme Ltd", (string)body["shipping_address"]["organization_name"]);
            Assert.Equal("221B Baker Street", (string)body["shipping_address"]["street_address"]);
            Assert.Equal("GB", (string)body["shipping_address"]["country"]);
            Assert.Equal("Test Product", (string)body["line_items"][0]["name"]);
            Assert.Equal("Test Product", (string)body["line_items"][0]["description"]);
            Assert.Equal("pcs", (string)body["line_items"][0]["quantity_unit"]);
            Assert.NotEqual(JTokenType.String, body["line_items"][0]["unit_price"].Type);
            Assert.NotEqual(JTokenType.String, body["line_items"][0]["net_amount"].Type);
            Assert.NotEqual(JTokenType.String, body["line_items"][0]["tax_amount"].Type);
            Assert.NotEqual(JTokenType.String, body["line_items"][0]["gross_amount"].Type);
            Assert.NotEqual(JTokenType.String, body["line_items"][0]["tax_rate"].Type);
            Assert.NotEqual(JTokenType.String, body["line_items"][0]["discount_amount"].Type);
        }

        [Fact]
        public async Task CreateOrderAsync_PopulatesRequiredLineItemFields_WhenMissing()
        {
            var mockHttp = new MockHttpMessageHandler();

            mockHttp
                .When(HttpMethod.Post, "https://api.sandbox.two.inc/v1/order")
                .With(request =>
                {
                    var body = request.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    var json = JObject.Parse(body);
                    var lineItem = json["line_items"][0];
                    var billingAddress = json["billing_address"];
                    var shippingAddress = json["shipping_address"];

                    return (string)lineItem["name"] == "Test Product"
                        && (string)lineItem["description"] == "Test Product"
                        && (string)lineItem["quantity_unit"] == "pcs"
                        && (decimal)lineItem["net_amount"] == 100.00m
                        && (decimal)lineItem["tax_amount"] == 20.00m
                        && (decimal)lineItem["gross_amount"] == 120.00m
                        && (string)billingAddress["organization_name"] == "Acme Ltd"
                        && (string)billingAddress["street_address"] == "221B Baker Street"
                        && (string)billingAddress["country"] == "GB"
                        && (string)shippingAddress["organization_name"] == "Acme Ltd"
                        && (string)shippingAddress["street_address"] == "221B Baker Street"
                        && (string)shippingAddress["country"] == "GB";
                })
                .Respond(HttpStatusCode.OK, "application/json",
                    JsonConvert.SerializeObject(new CreateOrderResponse
                    {
                        Id = OrderId,
                        Status = "PENDING",
                        Currency = "GBP"
                    }));

            var client = BuildClient(mockHttp);

            var request = new CreateOrderRequest
            {
                Currency = "GBP",
                InvoiceType = "DIRECT_INVOICE",
                GrossAmount = "120.00",
                NetAmount = "100.00",
                TaxAmount = "20.00",
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
                },
                BillingAddress = new BillingAddress
                {
                    Address = "221B Baker Street",
                    PostalCode = "NW1 6XE",
                    City = "London",
                    CountryPrefix = "GB"
                },
                LineItems = new System.Collections.Generic.List<LineItem>
                {
                    new LineItem
                    {
                        Name = "Test Product",
                        Quantity = 1,
                        UnitPrice = "100.00",
                        TaxRate = "0.20",
                        DiscountAmount = "0.00",
                        Type = "PHYSICAL",
                        ProductId = "TEST-001"
                    }
                }
            };

            var result = await client.Orders.CreateOrderAsync(request);

            Assert.NotNull(result);
            Assert.Equal(OrderId, result.Id);
        }

        [Fact]
        public async Task CreateOrderAsync_UsesExistingShippingAddress_WhenProvided()
        {
            var mockHttp = new MockHttpMessageHandler();

            mockHttp
                .When(HttpMethod.Post, "https://api.sandbox.two.inc/v1/order")
                .With(request =>
                {
                    var body = request.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    var json = JObject.Parse(body);
                    var billingAddress = json["billing_address"];
                    var shippingAddress = json["shipping_address"];

                    return (string)billingAddress["street_address"] == "Billing Street 1"
                        && (string)shippingAddress["street_address"] == "Shipping Street 2"
                        && (string)shippingAddress["organization_name"] == "Warehouse Ltd"
                        && (string)shippingAddress["country"] == "GB";
                })
                .Respond(HttpStatusCode.OK, "application/json",
                    JsonConvert.SerializeObject(new CreateOrderResponse
                    {
                        Id = OrderId,
                        Status = "PENDING",
                        Currency = "GBP"
                    }));

            var client = BuildClient(mockHttp);

            var request = new CreateOrderRequest
            {
                Currency = "GBP",
                InvoiceType = "DIRECT_INVOICE",
                GrossAmount = "120.00",
                NetAmount = "100.00",
                TaxAmount = "20.00",
                Buyer = new Buyer
                {
                    Company = new BuyerCompany
                    {
                        CountryPrefix = "GB",
                        OrganizationNumber = "12345678",
                        CompanyName = "Acme Ltd"
                    }
                },
                BillingAddress = new BillingAddress
                {
                    Address = "Billing Street 1",
                    PostalCode = "NW1 6XE",
                    City = "London",
                    CountryPrefix = "GB"
                },
                ShippingAddress = new BillingAddress
                {
                    OrganizationName = "Warehouse Ltd",
                    StreetAddress = "Shipping Street 2",
                    PostalCode = "SW1A 1AA",
                    City = "London",
                    Country = "GB"
                },
                LineItems = new System.Collections.Generic.List<LineItem>
                {
                    new LineItem
                    {
                        Name = "Test Product",
                        Quantity = 1,
                        UnitPrice = "100.00",
                        TaxRate = "0.20",
                        DiscountAmount = "0.00",
                        Type = "PHYSICAL",
                        ProductId = "TEST-001"
                    }
                }
            };

            var result = await client.Orders.CreateOrderAsync(request);

            Assert.NotNull(result);
            Assert.Equal(OrderId, result.Id);
        }

        [Fact]
        public async Task CreateOrderAsync_ComputesLineItemAmounts_FromQuantityTaxAndDiscount()
        {
            var mockHttp = new MockHttpMessageHandler();

            mockHttp
                .When(HttpMethod.Post, "https://api.sandbox.two.inc/v1/order")
                .With(request =>
                {
                    var body = request.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    var json = JObject.Parse(body);
                    var lineItem = json["line_items"][0];

                    return (decimal)lineItem["net_amount"] == 190.00m
                        && (decimal)lineItem["tax_amount"] == 38.00m
                        && (decimal)lineItem["gross_amount"] == 228.00m;
                })
                .Respond(HttpStatusCode.OK, "application/json",
                    JsonConvert.SerializeObject(new CreateOrderResponse
                    {
                        Id = OrderId,
                        Status = "PENDING",
                        Currency = "GBP"
                    }));

            var client = BuildClient(mockHttp);

            var request = new CreateOrderRequest
            {
                Currency = "GBP",
                InvoiceType = "DIRECT_INVOICE",
                GrossAmount = "228.00",
                NetAmount = "190.00",
                TaxAmount = "38.00",
                LineItems = new System.Collections.Generic.List<LineItem>
                {
                    new LineItem
                    {
                        Name = "Bulk Product",
                        Quantity = 2,
                        QuantityUnit = "pcs",
                        UnitPrice = "100.00",
                        TaxRate = "0.20",
                        DiscountAmount = "10.00",
                        Type = "PHYSICAL",
                        ProductId = "TEST-002"
                    }
                }
            };

            var result = await client.Orders.CreateOrderAsync(request);

            Assert.NotNull(result);
            Assert.Equal(OrderId, result.Id);
        }

        [Fact]
        public async Task CreateOrderAsync_ReturnsCreatedOrder_WhenApiSucceeds()
        {
            var mockHttp = new MockHttpMessageHandler();
            var expectedResponse = new CreateOrderResponse
            {
                Id = OrderId,
                Status = "PENDING",
                Currency = "GBP"
            };

            mockHttp
                .When(HttpMethod.Post, "https://api.sandbox.two.inc/v1/order")
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

            var result = await client.Orders.CreateOrderAsync(request);

            Assert.NotNull(result);
            Assert.Equal(OrderId, result.Id);
            Assert.Equal("PENDING", result.Status);
            Assert.Equal("GBP", result.Currency);
        }

        [Fact]
        public async Task GetOrderAsync_ReturnsOrder_WhenApiSucceeds()
        {
            var mockHttp = new MockHttpMessageHandler();
            var expectedResponse = new GetOrderResponse
            {
                Id = OrderId,
                Status = "APPROVED",
                Currency = "NOK"
            };

            mockHttp
                .When(HttpMethod.Get, $"https://api.sandbox.two.inc/v1/order/{OrderId}")
                .Respond(HttpStatusCode.OK, "application/json",
                    JsonConvert.SerializeObject(expectedResponse));

            var client = BuildClient(mockHttp);

            var result = await client.Orders.GetOrderAsync(OrderId);

            Assert.NotNull(result);
            Assert.Equal(OrderId, result.Id);
            Assert.Equal("APPROVED", result.Status);
        }

        [Fact]
        public async Task GetOrderAsync_ThrowsTwoApiException_WhenApiReturns404()
        {
            var mockHttp = new MockHttpMessageHandler();

            mockHttp
                .When(HttpMethod.Get, $"https://api.sandbox.two.inc/v1/order/{OrderId}")
                .Respond(HttpStatusCode.NotFound, "application/json",
                    JsonConvert.SerializeObject(new TwoApiError
                    {
                        ErrorCode = "ORDER_NOT_FOUND",
                        ErrorMessage = "Order not found"
                    }));

            var client = BuildClient(mockHttp);

            var ex = await Assert.ThrowsAsync<Core.Exceptions.TwoApiException>(
                () => client.Orders.GetOrderAsync(OrderId));

            Assert.Equal(404, ex.StatusCode);
            Assert.Equal("ORDER_NOT_FOUND", ex.ErrorCode);
        }

        [Fact]
        public async Task ConfirmOrderAsync_CompletesWithoutException_WhenApiSucceeds()
        {
            var mockHttp = new MockHttpMessageHandler();

            mockHttp
                .When(HttpMethod.Post, $"https://api.sandbox.two.inc/v1/order/{OrderId}/confirm")
                .Respond(HttpStatusCode.OK, "application/json", "{}");

            var client = BuildClient(mockHttp);

            await client.Orders.ConfirmOrderAsync(OrderId);
        }

        [Fact]
        public async Task CancelOrderAsync_CompletesWithoutException_WhenApiSucceeds()
        {
            var mockHttp = new MockHttpMessageHandler();

            mockHttp
                .When(HttpMethod.Delete, $"https://api.sandbox.two.inc/v1/order/{OrderId}")
                .Respond(HttpStatusCode.OK, "application/json", "{}");

            var client = BuildClient(mockHttp);

            await client.Orders.CancelOrderAsync(OrderId);
        }

        [Fact]
        public async Task CreateOrderAsync_ThrowsTwoApiException_WhenApiReturnsError()
        {
            var mockHttp = new MockHttpMessageHandler();

            mockHttp
                .When(HttpMethod.Post, "https://api.sandbox.two.inc/v1/order")
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

            var ex = await Assert.ThrowsAsync<Core.Exceptions.TwoApiException>(
                () => client.Orders.CreateOrderAsync(request));

            Assert.Equal(422, ex.StatusCode);
            Assert.Equal("VALIDATION_ERROR", ex.ErrorCode);
        }

        [Fact]
        public async Task CreateOrderAsync_ThrowsTwoApiException_WithDetailMessage_WhenApiReturnsProblemDetails()
        {
            var mockHttp = new MockHttpMessageHandler();

            mockHttp
                .When(HttpMethod.Post, "https://api.sandbox.two.inc/v1/order")
                .Respond(HttpStatusCode.BadRequest, "application/json",
                    "{\"detail\":\"invoice_type, country_prefix and organization_number must match merchant setup\"}");

            var client = BuildClient(mockHttp);

            var request = new CreateOrderRequest
            {
                Currency = "GBP",
                GrossAmount = "100.00"
            };

            var ex = await Assert.ThrowsAsync<Core.Exceptions.TwoApiException>(
                () => client.Orders.CreateOrderAsync(request));

            Assert.Equal(400, ex.StatusCode);
            Assert.Contains("must match merchant setup", ex.Message);
        }

        [Fact]
        public void TwoOptions_GetEffectiveBaseUrl_ReturnsSandboxUrl_WhenUseSandboxIsTrue()
        {
            var opts = new TwoOptions { ApiKey = "key", UseSandbox = true };
            Assert.Equal("https://api.sandbox.two.inc/v1", opts.GetEffectiveBaseUrl());
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

        [Fact]
        public void CreateOrderResponse_DeserializesLineItemQuantity_WhenApiReturnsFloat()
        {
            var json = @"{
                'id':'ord_abc123',
                'status':'PENDING',
                'currency':'EUR',
                'line_items':[{
                    'name':'Producto',
                    'description':'Producto',
                    'quantity':1.0,
                    'quantity_unit':'pcs',
                    'unit_price':100.0,
                    'net_amount':100.0,
                    'tax_amount':21.0,
                    'gross_amount':121.0,
                    'tax_rate':0.21,
                    'discount_amount':0.0,
                    'type':'PHYSICAL',
                    'product_id':'TEST-001'
                }]
            }";

            var response = JsonConvert.DeserializeObject<CreateOrderResponse>(json);

            Assert.NotNull(response);
            Assert.Single(response.LineItems);
            Assert.Equal(1, response.LineItems[0].Quantity);
        }

        [Fact]
        public void GetOrderResponse_DeserializesLineItemQuantity_WhenApiReturnsFloat()
        {
            var json = @"{
                'id':'ord_abc123',
                'status':'VERIFIED',
                'currency':'EUR',
                'line_items':[{
                    'name':'Producto',
                    'description':'Producto',
                    'quantity':1.0,
                    'quantity_unit':'pcs',
                    'unit_price':100.0,
                    'net_amount':100.0,
                    'tax_amount':21.0,
                    'gross_amount':121.0,
                    'tax_rate':0.21,
                    'discount_amount':0.0,
                    'type':'PHYSICAL',
                    'product_id':'TEST-001'
                }]
            }";

            var response = JsonConvert.DeserializeObject<GetOrderResponse>(json);

            Assert.NotNull(response);
            Assert.Single(response.LineItems);
            Assert.Equal(1, response.LineItems[0].Quantity);
        }

        [Fact]
        public void CreateOrderResponse_RoundsLineItemQuantity_WhenApiReturnsDecimal()
        {
            var json = @"{
                'id':'ord_abc123',
                'status':'PENDING',
                'currency':'EUR',
                'line_items':[{
                    'name':'Producto',
                    'description':'Producto',
                    'quantity':1.5,
                    'quantity_unit':'pcs',
                    'unit_price':100.0,
                    'net_amount':100.0,
                    'tax_amount':21.0,
                    'gross_amount':121.0,
                    'tax_rate':0.21,
                    'discount_amount':0.0,
                    'type':'PHYSICAL',
                    'product_id':'TEST-001'
                }]
            }";

            var response = JsonConvert.DeserializeObject<CreateOrderResponse>(json);

            Assert.NotNull(response);
            Assert.Single(response.LineItems);
            Assert.Equal(2, response.LineItems[0].Quantity);
        }
    }
}
