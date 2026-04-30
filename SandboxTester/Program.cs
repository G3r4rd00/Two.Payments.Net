using System;
using System.Threading.Tasks;
using Two.Payments.Application;
using Two.Payments.Core.Models;
using Two.Payments.Infrastructure.Configuration;

namespace SandboxTester
{
    class Program
    {
        // ⚠️ CONFIGURA TU KEY AQUÍ ⚠️
        private const string SANDBOX_API_KEY = "secret_test_Scl_IqNiT0sXJQTOTOJKgsHZMD7NjqkI3AtmXo-WvfI";

        static async Task Main(string[] args)
        {
            Console.WriteLine("=== Two.Payments Sandbox Tester ===\n");

            try
            {
                var client = TwoClientFactory.Create(new TwoOptions
                {
                    ApiKey = SANDBOX_API_KEY,
                    UseSandbox = true
                });

                Console.WriteLine("✓ Cliente creado con sandbox API key");
                Console.WriteLine("  Endpoint: https://api.sandbox.two.inc/v1\n");

                // Consultar límites de crédito del comprador
                Console.WriteLine("Consultando límites de crédito del comprador...");
                var limits = await client.Limits.GetBuyerCreditLimitsAsync("FR", "FR07411985641");
                if (limits != null)
                {
                    Console.WriteLine("✓ Límites recuperados");
                    Console.WriteLine($"  Country: {limits.BuyerCountryCode ?? "(n/a)"}");
                    Console.WriteLine($"  Organization: {limits.BuyerOrganizationNumber ?? "(n/a)"}");
                    Console.WriteLine($"  Currency: {limits.Currency ?? "(n/a)"}");
                    Console.WriteLine($"  CreditLimit: {limits.CreditLimit?.ToString() ?? "(n/a)"}");
                    Console.WriteLine($"  AvailableCredit: {limits.AvailableCredit?.ToString() ?? "(n/a)"}\n");
                }

                // Crear una orden de prueba
                Console.WriteLine("Creando orden de prueba...");
                var e = new CreateOrderRequest
                {
                    Currency = "EUR",
                    InvoiceType = "DIRECT_INVOICE",
                    GrossAmount = "121.00",
                    NetAmount = "100.00",
                    TaxAmount = "21.00",
                    DiscountAmount = "0.00",
                    DiscountRate = "0.00",
                    TaxRate = "0.21",
                    Buyer = new Buyer
                    {
                        Representative = new BuyerRepresentative
                        {
                            FirstName = "Juan",
                            LastName = "Pérez",
                            Email = "juan.perez@example.com",
                            PhoneNumber = "+34612345678"
                        },
                        Company = new BuyerCompany
                        {
                            CountryPrefix = "ES",
                            OrganizationNumber = "B12345678",
                            CompanyName = "Empresa Demo S.L."
                        }
                    },
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
                        new LineItem
                        {
                            Description = "Producto de Prueba",
                            Quantity = 1,
                            QuantityUnit = "pcs",
                            UnitPrice = "100.00",
                            NetAmount = "100.00",
                            TaxAmount = "21.00",
                            GrossAmount = "121.00",
                            TaxRate = "0.21",
                            TaxClassName = "HIGH",
                            DiscountAmount = "0.00",
                            Type = "PHYSICAL",
                            ProductId = "TEST-001"
                        }
                    },
                    MerchantOrderId = $"TEST-{DateTime.UtcNow:yyyyMMddHHmmss}"
                };
                var order = await client.Orders.CreateOrderAsync(e);

                Console.WriteLine($"✓ Orden creada exitosamente!");
                Console.WriteLine($"  ID: {order.Id}");
                Console.WriteLine($"  Status: {order.Status}");
                Console.WriteLine($"  Currency: {order.Currency}\n");

                // Consultar la orden
                Console.WriteLine("Consultando orden...");
                var retrieved = await client.Orders.GetOrderAsync(order.Id);
                Console.WriteLine($"✓ Orden recuperada: {retrieved.Status}\n");

                Console.WriteLine("=== ¡Tu API key funciona correctamente! ===");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n❌ Error: {ex.Message}");
                Console.WriteLine($"   Type: {ex.GetType().Name}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"   Detalle: {ex.InnerException.Message}");
                }
                Console.WriteLine($"\n   Stack trace:");
                Console.WriteLine(ex.StackTrace);
            }

            Console.WriteLine("\nPresiona cualquier tecla para salir...");
            Console.ReadKey();
        }
    }
}
