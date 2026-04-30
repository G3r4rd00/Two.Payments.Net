# Two.Payments.Net

Lightweight and extensible .NET client for the [Two](https://two.inc) B2B payment platform. Designed for developers who need a simple way to integrate B2B payments, manage orders, handle BNPL workflows, and retrieve buyer credit limits.

[![NuGet](https://img.shields.io/nuget/v/Two.Payments.svg)](https://www.nuget.org/packages/Two.Payments)
[![.NET Standard 2.0](https://img.shields.io/badge/.NET%20Standard-2.0-blue)](https://docs.microsoft.com/en-us/dotnet/standard/net-standard)

---

## Features

- ✅ Authentication via API key (`X-API-Key`)
- ✅ Create B2B orders / checkout sessions
- ✅ Retrieve order status
- ✅ Confirm & cancel orders
- ✅ Retrieve buyer/company credit limits (`Limits API`)
- ✅ Strongly-typed request/response models
- ✅ Structured error handling via `TwoApiException`
- ✅ Optional retry support
- ✅ Optional logging support (`Microsoft.Extensions.Logging`)
- ✅ Dependency Injection support (`Microsoft.Extensions.DependencyInjection`)
- ✅ Compatible with **.NET Standard 2.0**, **.NET Framework 4.7+**, **.NET 6+**

---

## Installation

```sh
dotnet add package Two.Payments
```

---

## Quick Start

### Without Dependency Injection

```csharp
using Two.Payments.Application;
using Two.Payments.Core.Models;
using Two.Payments.Infrastructure.Configuration;

var client = TwoClientFactory.Create(new TwoOptions
{
    ApiKey = "your-api-key",
    UseSandbox = true // set to false for production
});

// 1) Retrieve buyer credit limits
var limits = await client.Limits.GetBuyerCreditLimitsAsync("ES", "B12345678");
Console.WriteLine($"Credit limit: {limits?.CreditLimit}");

// 2) Create an order
var order = await client.Orders.CreateOrderAsync(new CreateOrderRequest
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
            LastName = "Perez",
            Email = "juan.perez@acme.com",
            PhoneNumber = "+34612345678"
        },
        Company = new BuyerCompany
        {
            CountryPrefix = "ES",
            OrganizationNumber = "B12345678",
            CompanyName = "Acme S.L."
        }
    },
    BillingAddress = new BillingAddress
    {
        OrganizationName = "Acme S.L.",
        StreetAddress = "Gran Via 1",
        PostalCode = "28013",
        City = "Madrid",
        Country = "ES"
    },
    ShippingAddress = new BillingAddress
    {
        OrganizationName = "Acme S.L.",
        StreetAddress = "Gran Via 1",
        PostalCode = "28013",
        City = "Madrid",
        Country = "ES"
    },
    LineItems = new System.Collections.Generic.List<LineItem>
    {
        new LineItem
        {
            Description = "Test product",
            Quantity = 1,
            QuantityUnit = "pcs",
            UnitPrice = "100.00",
            NetAmount = "100.00",
            TaxAmount = "21.00",
            GrossAmount = "121.00",
            TaxRate = "0.21",
            DiscountAmount = "0.00",
            Type = "PHYSICAL",
            ProductId = "TEST-001"
        }
    }
});

Console.WriteLine($"Order {order.Id} created. Status: {order.Status}");

// 3) Retrieve the order
var retrieved = await client.Orders.GetOrderAsync(order.Id);
Console.WriteLine($"Current status: {retrieved.Status}");

// 4) Confirm the order once goods are dispatched
await client.Orders.ConfirmOrderAsync(order.Id);
```

### With Dependency Injection (ASP.NET Core / Generic Host)

```csharp
services.AddTwoPayments(o =>
{
    o.ApiKey = Configuration["Two:ApiKey"];
    o.UseSandbox = bool.Parse(Configuration["Two:UseSandbox"] ?? "false");
});

public class OrderController : ControllerBase
{
    private readonly ITwoClient _two;

    public OrderController(ITwoClient two) => _two = two;

    public async Task<IActionResult> PlaceOrder(...)
    {
        var order = await _two.Orders.CreateOrderAsync(...);
        return Ok(order);
    }
}
```

---

## SandboxTester Example

This repository includes `SandboxTester/Program.cs`, which demonstrates:

- retrieving buyer limits
- creating an order with validated `billing_address` and `shipping_address`
- retrieving the created order

Use it as a runnable integration sample against sandbox.

---

## Create Order Notes

The current `CreateOrderRequest` model supports the fields required by recent Two schema validations, including:

- `billing_address` (`organization_name`, `street_address`, `postal_code`, `city`, `country`)
- `shipping_address` (same shape as billing)
- `line_items` with both `name` and `description`, plus amount fields (`net_amount`, `tax_amount`, `gross_amount`) and `quantity_unit`

The client also normalizes missing order fields before sending:

- Copies billing to shipping when `ShippingAddress` is omitted
- Backfills `BillingAddress.OrganizationName` from `Buyer.Company.CompanyName`
- Backfills line item `name/description`
- Computes line item amounts if omitted

Additionally, line item `quantity` deserialization accepts values returned as `1.0` by the API.

---

## Limits API

The client exposes limits operations through:

- `client.Limits.GetBuyerCreditLimitsAsync(buyerCountryCode, buyerOrganizationNumber)`

Endpoint used:

- `GET /limits/v1/company/:buyer_country_code/:buyer_organization_number`

---

## Configuration

| Property        | Type       | Default      | Description                             |
|----------------|------------|--------------|-----------------------------------------|
| `ApiKey`       | `string`   | *(required)* | Your Two API key                        |
| `UseSandbox`   | `bool`     | `false`      | Use the sandbox endpoint                |
| `BaseUrl`      | `string`   | `null`       | Override base URL for Checkout API      |
| `Timeout`      | `TimeSpan` | 30 seconds   | Per-request HTTP timeout                |
| `MaxRetryCount`| `int`      | `0`          | Number of retries on transient failures |
| `RetryDelay`   | `TimeSpan` | 1 second     | Delay between retry attempts            |

> Note: Limits API is called using an absolute path (`/limits/v1/...`) so it works even when the base checkout URL includes `/v1`.

---

## Error Handling

All API errors are surfaced as `TwoApiException`:

```csharp
try
{
    var order = await client.Orders.GetOrderAsync("non-existent-id");
}
catch (TwoApiException ex)
{
    Console.WriteLine($"HTTP {ex.StatusCode}: [{ex.ErrorCode}] {ex.Message}");
}
```

### Common Validation Errors

- `SCHEMA_ERROR`: missing/invalid request fields (often in `line_items`, `billing_address`, `shipping_address`)
- `ORDER_INVALID`: business-rule validation failed (for example invalid tax rate for country)
- `... merchant does not support buyer country ...`: merchant-country mismatch

---

## Project Structure

```text
src/
└── Two.Payments/
    ├── Core/
    │   ├── Exceptions/     TwoApiException
    │   ├── Interfaces/     ITwoClient, ITwoOrderService, ITwoLimitsService
    │   └── Models/         CreateOrderRequest, CreateOrderResponse,
    │                       GetOrderResponse, GetBuyerCreditLimitsResponse,
    │                       Buyer, BillingAddress, LineItem, …
    ├── Infrastructure/
    │   ├── Configuration/  TwoOptions
    │   ├── Serialization/  StringNumberJsonConverter, FlexibleIntJsonConverter
    │   └── TwoHttpClient   (internal HTTP wrapper)
    ├── Application/
    │   ├── Services/       TwoOrderService, TwoLimitsService
    │   ├── TwoClient       (ITwoClient implementation)
    │   └── TwoClientFactory
    └── Extensions/
        └── ServiceCollectionExtensions

tests/
└── Two.Payments.Tests/
    ├── Services/           TwoOrderServiceTests, TwoLimitsServiceTests
    └── Integration/        SandboxIntegrationTests
```

---

## License

MIT

