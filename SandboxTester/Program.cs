using System;
using System.Threading.Tasks;
using Two.Payments.Application;
using Two.Payments.Core.Models;
using Two.Payments.Infrastructure.Configuration;
using System.IO; // Para leer el archivo .env

namespace SandboxTester
{
    class Program
    {
        // Lee la API Key desde el archivo .env
        private static string GetSandboxApiKey()
        {
            var envPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".env");
            if (!File.Exists(envPath))
                throw new FileNotFoundException("No se encontró el archivo .env con la API Key.");
            foreach (var line in File.ReadAllLines(envPath))
            {
                if (line.StartsWith("SANDBOX_API_KEY="))
                    return line.Substring("SANDBOX_API_KEY=".Length).Trim();
            }
            throw new Exception("No se encontró SANDBOX_API_KEY en el archivo .env.");
        }

        static async Task Main(string[] args)
        {
            Console.WriteLine("=== Two.Payments Sandbox Tester ===\n");

            try
            {
                var client = TwoClientFactory.Create(new TwoOptions
                {
                    ApiKey = GetSandboxApiKey(),
                    UseSandbox = true
                });

                Console.WriteLine("✓ Cliente creado con sandbox API key");
                Console.WriteLine("  Endpoint: https://api.sandbox.two.inc/v1\n");

                var representative = new BuyerRepresentative
                {
                    FirstName = "Nombre",
                    LastName = "Apellidos",
                    PhoneNumber = "+34666555888",
                    Email = "fff@gmail.com"
                };
                var company = new BuyerCompany
                {
                    CountryPrefix = "ES",
                    OrganizationNumber = "43136113Y",
                    CompanyName = "BLX"
                };
                var billingAddress = new BillingAddress
                {
                    OrganizationName = "BLX",
                    StreetAddress = "Calle Facturación 789",
                    PostalCode = "28081",
                    City = "Madrid",
                    Country = "ES"
                };
                // Crear la orden exactamente como en el JSON proporcionado
                Console.WriteLine("Creando orden de prueba...");
                var e = new CreateOrderRequest
                {
                    Currency = "EUR",
                    InvoiceType = "DIRECT_INVOICE",
                    GrossAmount = "11.93",
                    NetAmount = "9.86",
                    TaxAmount = "2.07",
                    DiscountAmount = "0.00",
                    DiscountRate = "0.00",
                    TaxRate = "0.21",
                    Buyer = new Buyer(representative, company),
                    BillingAddress = billingAddress,
                    LineItems = new System.Collections.Generic.List<LineItem>
                    {
                        new LineItem(
                            name: "FACIAL MOISTURISING LOTION for normal to dry skin 52 ml",
                            description: "FACIAL MOISTURISING LOTION for normal to dry skin 52 ml",
                            quantity: 1,
                            unitPrice: "8.60",
                            taxRate: "0.21",
                            taxClassName: "GENERAL",
                            type: "PHYSICAL"
                        ){
                            ProductId = "124261"
                        },
                        new LineItem(
                            name: "DISCREET compresa incontinencia normal 12 u",
                            description: "DISCREET compresa incontinencia normal 12 u",
                            quantity: 1,
                            unitPrice: "1.26",
                            taxRate: "0.21",
                            taxClassName: "GENERAL",
                            type: "PHYSICAL"
                        ){
                         ProductId = "74993"
                        }
                    },
                    MerchantOrderId = "X141127978",
                    MerchantUrls = new MerchantUrls
                    {
                        MerchantConfirmationUrl = "http://localhost:63636/es/pago/twookcallback/",
                        MerchantCancelOrderUrl = "http://localhost:63636/es/pago/twokocallback/"
                    }
                };

                var order = await client.Orders.CreateOrderAsync(e);

                Console.WriteLine($"✓ Orden creada exitosamente!");
                Console.WriteLine($"  ID: {order.Id}");
                Console.WriteLine($"  Status: {order.Status}");
                Console.WriteLine($"  Currency: {order.Currency}\n");

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
