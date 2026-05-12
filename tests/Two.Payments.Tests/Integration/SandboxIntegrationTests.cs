using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Two.Payments.Application;
using Two.Payments.Core.Interfaces;
using Two.Payments.Core.Models;
using Two.Payments.Infrastructure.Configuration;
using Xunit;

namespace Two.Payments.Tests.Integration
{
    /// <summary>
    /// Tests de integración con el sandbox real de Two.inc.
    /// </summary>
    public class SandboxIntegrationTests
    {
        private const string SANDBOX_API_KEY = "secret_test_Scl_IqNiT0sXJQTOTOJKgsHZMD7NjqkI3AtmXo-WvfI";

        [Fact(Skip = "Requiere API key de sandbox válida y datos de merchant compatibles")]
        public async Task CreateOrder_WithRealSandboxApi_ShouldSucceed()
        {
            // Arrange
            var client = CreateSandboxClient();
            var request = BuildSpanishOrderRequest();

            // Act
            var order = await client.Orders.CreateOrderAsync(request);

            // Assert
            Assert.NotNull(order);
            Assert.NotNull(order.Id);
            Assert.Equal("PENDING", order.Status);
            Assert.Equal("EUR", order.Currency);

            // Opcional: limpiar - cancelar la orden creada
            try
            {
                await client.Orders.CancelOrderAsync(order.Id);
            }
            catch
            {
                // Ignorar errores de limpieza
            }
        }

        [Fact(Skip = "Requiere API key de sandbox válida y datos de merchant compatibles")]
        public async Task CreateOrder_ThenGetOrder_WithRealSandboxApi_ShouldReturnCreatedOrder()
        {
            // Arrange
            var client = CreateSandboxClient();
            var request = BuildSpanishOrderRequest();

            // Act
            var created = await client.Orders.CreateOrderAsync(request);
            var retrieved = await client.Orders.GetOrderAsync(created.Id);

            // Assert
            Assert.NotNull(retrieved);
            Assert.Equal(created.Id, retrieved.Id);
            Assert.Equal("EUR", retrieved.Currency);

            // Opcional: limpiar - cancelar la orden creada
            try
            {
                await client.Orders.CancelOrderAsync(created.Id);
            }
            catch
            {
                // Ignorar errores de limpieza
            }
        }

        [Fact(Skip = "Requiere API key de sandbox válida")]
        public async Task GetOrder_WithInvalidOrderId_ShouldThrowException()
        {
            // Arrange
            var client = CreateSandboxClient();
            var invalidOrderId = "ord_invalid_test_123";

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Core.Exceptions.TwoApiException>(
                () => client.Orders.GetOrderAsync(invalidOrderId));

            Assert.Equal(404, exception.StatusCode);
        }

        [Fact(Skip = "Requires valid sandbox API key and merchant data")]
        public async Task CreateOrder_WithValidLineItemType_ShouldSucceed()
        {
            // Arrange
            var client = CreateSandboxClient();
            var request = BuildSpanishOrderRequest();

            // Modify request to include a valid LineItem type
            request.LineItems.Add(new LineItem(
                name: "Shipping cost",
                description: "Shipping cost",
                quantity: 1,
                unitPrice: "4.79",
                taxRate: "0.21",
                taxClassName: "HIGH",
                type: "SHIPPING_FEE"
            ));

            // Act
            var order = await client.Orders.CreateOrderAsync(request);

            // Assert
            Assert.NotNull(order);
            Assert.NotNull(order.Id);
            Assert.Equal("PENDING", order.Status);
            Assert.Equal("EUR", order.Currency);
        }

        private static CreateOrderRequest BuildSpanishOrderRequest()
        {
            return new CreateOrderRequest
            {
                Currency = "EUR",
                InvoiceType = "DIRECT_INVOICE",
                GrossAmount = "121.00",
                NetAmount = "100.00",
                TaxAmount = "21.00",
                DiscountAmount = "0.00",
                DiscountRate = "0.00",
                TaxRate = "0.21",
                Buyer = new Buyer(
                    new BuyerRepresentative
                    {
                        FirstName = "Juan",
                        LastName = "Pérez",
                        Email = "juan.perez@example.com",
                        PhoneNumber = "+34612345678"
                    },
                    new BuyerCompany
                    {
                        CountryPrefix = "ES",
                        OrganizationNumber = "B12345678",
                        CompanyName = "Empresa Demo S.L."
                    }
                ),
                BillingAddress = new BillingAddress
                {
                    OrganizationName = "Empresa Demo S.L.",
                    StreetAddress = "Gran Vía 1",
                    PostalCode = "28013",
                    City = "Madrid",
                    Country = "ES"
                },
                ShippingAddress = new BillingAddress
                {
                    OrganizationName = "Empresa Demo S.L.",
                    StreetAddress = "Gran Vía 1",
                    PostalCode = "28013",
                    City = "Madrid",
                    Country = "ES"
                },
                LineItems = new System.Collections.Generic.List<LineItem>
                {
                    new LineItem(
                        name: "Test Product",
                        description: "Producto de prueba",
                        quantity: 1,
                        unitPrice: "100.00",
                        taxRate: "0.21",
                        taxClassName: "HIGH",
                        type: "PHYSICAL"
                    )
                },
                MerchantOrderId = $"TEST-{DateTime.UtcNow:yyyyMMddHHmmss}"
            };
        }

        private ITwoClient CreateSandboxClient()
        {
            var options = new TwoOptions
            {
                ApiKey = SANDBOX_API_KEY,
                UseSandbox = true,
                Timeout = TimeSpan.FromSeconds(30)
            };

            return TwoClientFactory.Create(options);
        }
    }
}
