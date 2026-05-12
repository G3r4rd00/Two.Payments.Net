using System;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using Two.Payments.Application;
using Two.Payments.Core.Models;
using Two.Payments.Infrastructure.Configuration;

namespace SandboxTester
{
    class Program
    {
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
            Console.WriteLine("=== Two.Payments Sandbox Repro ===\n");

            try
            {
                var client = TwoClientFactory.Create(new TwoOptions
                {
                    ApiKey = GetSandboxApiKey(),
                    UseSandbox = true
                });

                // Construir la petición exactamente como en el log
                var request = new CreateOrderRequest
                {
                    Currency = "EUR",
                    InvoiceType = "DIRECT_INVOICE",
                    GrossAmount = "48.04",
                    NetAmount = "39.70",
                    TaxAmount = "8.34",
                    DiscountAmount = "0.00",
                    DiscountRate = "0.00",
                    TaxRate = "0.21",
                    Buyer = new Buyer(
                        new BuyerRepresentative
                        {
                            FirstName = "Name",
                            LastName = "Ape",
                            PhoneNumber = "+34666555888",
                            Email = "eeee@gmail.com"
                        },
                        new BuyerCompany
                        {
                            CountryPrefix = "ES",
                            OrganizationNumber = "43136516F",
                            CompanyName = "BLX"
                        }
                    ),
                    BillingAddress = new BillingAddress
                    {
                        OrganizationName = "BLX",
                        StreetAddress = "asdf",
                        PostalCode = "07006",
                        City = "asdf",
                        Country = "ES"
                    },
                    ShippingAddress = new BillingAddress
                    {
                        OrganizationName = "BLX",
                        StreetAddress = "asdf",
                        PostalCode = "07006",
                        City = "asdf",
                        Country = "ES"
                    },
                    LineItems = new System.Collections.Generic.List<LineItem>
                    {
                        new LineItem(
                            name: "GEL LIMPIADOR espumoso 1000 ml",
                            description: "GEL LIMPIADOR espumoso 1000 ml",
                            quantity: 1,
                            unitPrice: "13.26",
                            taxRate: "0.21",
                            taxClassName: "HIGH",
                            type: "PHYSICAL"
                        )
                        {
                            GrossAmount = "16.04",
                            NetAmount = "13.26",
                            TaxAmount = "2.78",
                            DiscountAmount = "0.00",
                            ProductId = "124249"
                        },
                        new LineItem(
                            name: "DEO traitement anti-transpirant 48h roll-on 50 ml",
                            description: "DEO traitement anti-transpirant 48h roll-on 50 ml",
                            quantity: 4,
                            unitPrice: "5.41",
                            taxRate: "0.21",
                            taxClassName: "HIGH",
                            type: "PHYSICAL"
                        )
                        {
                            GrossAmount = "26.20",
                            NetAmount = "21.65",
                            TaxAmount = "4.55",
                            DiscountAmount = "0.00",
                            ProductId = "73661"
                        },
                        new LineItem(
                            name: "Shipping cost",
                            description: "Shipping cost",
                            quantity: 1,
                            unitPrice: "4.79",
                            taxRate: "0.21",
                            taxClassName: "HIGH",
                            type: "SHIPPING_FEE" 
                        )
                        {
                            GrossAmount = "5.80",
                            NetAmount = "4.79",
                            TaxAmount = "1.01",
                            DiscountAmount = "0.00",
                            ProductId = "SHIPPING"
                        }
                    },
                    MerchantOrderId = "X141127979",
                    MerchantUrls = new MerchantUrls
                    {
                        MerchantConfirmationUrl = "http://localhost:63636/es/pago/twookcallback/",
                        MerchantCancelOrderUrl = "http://localhost:63636/es/pago/twokocallback/"
                    }
                };


                // Llamada al API
                var order = await client.Orders.CreateOrderAsync(request);

                Console.WriteLine("\n✅ Orden creada:");
                Console.WriteLine($"  Id: {order?.Id}");
                Console.WriteLine($"  Status: {order?.Status}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n❌ Excepción al crear orden:");
                Console.WriteLine($"Message: {ex.Message}");
                Console.WriteLine($"Type: {ex.GetType().Name}");
                if (ex.InnerException != null)
                    Console.WriteLine($"Inner: {ex.InnerException.Message}");

                // Si es TwoApiException es probable que contenga código y body
                Console.WriteLine("\nStackTrace:");
                Console.WriteLine(ex.StackTrace);
            }

            Console.WriteLine("\nPresiona cualquier tecla para salir...");
            Console.ReadKey();
        }
    }
}