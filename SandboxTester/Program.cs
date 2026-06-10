using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
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
                throw new FileNotFoundException("The .env file with the API key was not found.");

            foreach (var line in File.ReadAllLines(envPath))
            {
                if (line.StartsWith("SANDBOX_API_KEY="))
                    return line.Substring("SANDBOX_API_KEY=".Length).Trim();
            }

            throw new Exception("SANDBOX_API_KEY was not found in the .env file.");
        }

        static async Task Main(string[] args)
        {
            Console.WriteLine("=== Two.Payments Sandbox Repro ===\n");
            Console.WriteLine("Select an order example:");
            Console.WriteLine("  1) Order with items at 21% and 4% (mixed tax)");
            Console.WriteLine("  2) Order with items only at 21%");
            Console.WriteLine("  3) Order without taxes");
            Console.Write("Option [1-3]: ");

            var option = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(option))
                option = "1";

            CreateOrderRequest request = new CreateOrderRequest();
            try
            {
                var client = TwoClientFactory.Create(new TwoOptions
                {
                    ApiKey = GetSandboxApiKey(),
                    UseSandbox = true
                });

                request = BuildRequest(option.Trim());
                var order = await client.Orders.CreateOrderAsync(request);

                Console.WriteLine("\n✅ Order created:");
                Console.WriteLine($"  Id: {order?.Id}");
                Console.WriteLine($"  Status: {order?.Status}");
            }
            catch (Exception ex)
            {
                // Serialize the request to JSON to make analysis easier
                var json = JsonConvert.SerializeObject(request, Formatting.Indented);
                Console.WriteLine("\n❌ Exception while creating order:");
                Console.WriteLine($"Message: {ex.Message}");
                Console.WriteLine($"Type: {ex.GetType().Name}");
                if (ex.InnerException != null)
                    Console.WriteLine($"Inner: {ex.InnerException.Message}");

                Console.WriteLine("\nStackTrace:");
                Console.WriteLine(ex.StackTrace);
            }

            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }

        private static CreateOrderRequest BuildRequest(string option)
        {
            return option switch
            {
                "2" => BuildOrderOnly21(),
                "3" => BuildOrderNoTaxes(),
                _ => BuildOrderMixedTaxes()
            };
        }

        private static CreateOrderRequest CreateBaseRequest(string merchantOrderId, string grossAmount, string netAmount, string taxAmount, string taxRate, List<LineItem> lineItems)
        {
            return new CreateOrderRequest
            {
                Currency = "EUR",
                InvoiceType = "DIRECT_INVOICE",
                GrossAmount = grossAmount,
                NetAmount = netAmount,
                TaxAmount = taxAmount,
                DiscountAmount = "0.00",
                DiscountRate = "0.00",
                TaxRate = taxRate,
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
                LineItems = lineItems,
                MerchantOrderId = merchantOrderId,
                MerchantUrls = new MerchantUrls
                {
                    MerchantConfirmationUrl = "http://localhost:63636/es/pago/twookcallback/",
                    MerchantCancelOrderUrl = "http://localhost:63636/es/pago/twokocallback/"
                }
            };
        }

        private static CreateOrderRequest BuildOrderMixedTaxes()
        {
            return CreateBaseRequest(
                merchantOrderId: "X-MIX-001",
                grossAmount: "44.35",
                netAmount: "39.69",
                taxAmount: "4.66",
                taxRate: "0.21",
                lineItems: new List<LineItem>
                {
                    new LineItem(
                        name: "Product 21%",
                        description: "Item with 21% VAT",
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
                        name: "Product 4%",
                        description: "Item with 4% VAT",
                        quantity: 4,
                        unitPrice: "5.41",
                        taxRate: "0.04",
                        taxClassName: "LOW",
                        type: "PHYSICAL"
                    )
                    {
                        GrossAmount = "22.51",
                        NetAmount = "21.64",
                        TaxAmount = "0.87",
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
                });
        }

        private static CreateOrderRequest BuildOrderOnly21()
        {
            return CreateBaseRequest(
                merchantOrderId: "X-21-001",
                grossAmount: "27.25",
                netAmount: "22.52",
                taxAmount: "4.73",
                taxRate: "0.21",
                lineItems: new List<LineItem>
                {
                    new LineItem(
                        name: "Product A 21%",
                        description: "First item with 21% VAT",
                        quantity: 1,
                        unitPrice: "12.00",
                        taxRate: "0.21",
                        taxClassName: "HIGH",
                        type: "PHYSICAL"
                    )
                    {
                        GrossAmount = "14.52",
                        NetAmount = "12.00",
                        TaxAmount = "2.52",
                        DiscountAmount = "0.00",
                        ProductId = "A-21"
                    },
                    new LineItem(
                        name: "Product B 21%",
                        description: "Second item with 21% VAT",
                        quantity: 1,
                        unitPrice: "10.52",
                        taxRate: "0.21",
                        taxClassName: "HIGH",
                        type: "PHYSICAL"
                    )
                    {
                        GrossAmount = "12.73",
                        NetAmount = "10.52",
                        TaxAmount = "2.21",
                        DiscountAmount = "0.00",
                        ProductId = "B-21"
                    }
                });
        }

        private static CreateOrderRequest BuildOrderNoTaxes()
        {
            return CreateBaseRequest(
                merchantOrderId: "X-NO-TAX-001",
                grossAmount: "30.00",
                netAmount: "30.00",
                taxAmount: "0.00",
                taxRate: "0",
                lineItems: new List<LineItem>
                {
                    new LineItem(
                        name: "Tax-free product A",
                        description: "Item without taxes",
                        quantity: 1,
                        unitPrice: "15.00",
                        taxRate: "0.00",
                        taxClassName: "NONE",
                        type: "PHYSICAL"
                    )
                    {
                        GrossAmount = "15.00",
                        NetAmount = "15.00",
                        TaxAmount = "0.00",
                        DiscountAmount = "0.00",
                        ProductId = "NO-TAX-A",
                        TaxCode = "ES_IVA_ZERO"
                    },
                    new LineItem(
                        name: "Tax-free product B",
                        description: "Item without taxes",
                        quantity: 1,
                        unitPrice: "15.00",
                        taxRate: "0.00",
                        taxClassName: "NONE",
                        type: "PHYSICAL"
                    )
                    {
                        GrossAmount = "15.00",
                        NetAmount = "15.00",
                        TaxAmount = "0.00",
                        DiscountAmount = "0.00",
                        ProductId = "NO-TAX-B",
                        TaxCode = "ES_IVA_ZERO"
                    }
                });
        }
    }
}