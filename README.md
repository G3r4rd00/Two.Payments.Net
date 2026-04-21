# Two.Payments.Net

Lightweight and extensible .NET client for the [Two](https://two.inc) B2B payment platform. Designed for developers who need a simple way to integrate B2B payments, manage orders, and handle BNPL workflows in their applications.

[![NuGet](https://img.shields.io/nuget/v/Two.Payments.svg)](https://www.nuget.org/packages/Two.Payments)
[![.NET Standard 2.0](https://img.shields.io/badge/.NET%20Standard-2.0-blue)](https://docs.microsoft.com/en-us/dotnet/standard/net-standard)

---

## Features

- ✅ Authentication via API key (`X-API-Key`)
- ✅ Create B2B orders / checkout sessions
- ✅ Retrieve order status
- ✅ Confirm & cancel orders
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
    ApiKey    = "your-api-key",
    UseSandbox = true   // set to false for production
});

// Create an order
var order = await client.Orders.CreateOrderAsync(new CreateOrderRequest
{
    Currency     = "GBP",
    InvoiceType  = "DIRECT_INVOICE",
    GrossAmount  = "120.00",
    NetAmount    = "100.00",
    TaxAmount    = "20.00",
    DiscountAmount = "0.00",
    DiscountRate = "0.00",
    TaxRate      = "0.20",
    Buyer = new Buyer
    {
        Representative = new BuyerRepresentative
        {
            FirstName   = "Jane",
            LastName    = "Smith",
            Email       = "jane.smith@acme.com",
            PhoneNumber = "07700900001"
        },
        Company = new BuyerCompany
        {
            CountryPrefix      = "GB",
            OrganizationNumber = "12345678",
            CompanyName        = "Acme Ltd"
        }
    }
});

Console.WriteLine($"Order {order.Id} created. Status: {order.Status}");

// Retrieve the order
var retrieved = await client.Orders.GetOrderAsync(order.Id);
Console.WriteLine($"Current status: {retrieved.Status}");

// Confirm the order once goods are dispatched
await client.Orders.ConfirmOrderAsync(order.Id);
```

### With Dependency Injection (ASP.NET Core / Generic Host)

```csharp
// In Startup.cs / Program.cs
services.AddTwoPayments(o =>
{
    o.ApiKey    = Configuration["Two:ApiKey"];
    o.UseSandbox = bool.Parse(Configuration["Two:UseSandbox"] ?? "false");
});

// Inject ITwoClient wherever you need it
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

## Configuration

| Property        | Type       | Default     | Description                                      |
|-----------------|------------|-------------|--------------------------------------------------|
| `ApiKey`        | `string`   | *(required)*| Your Two API key                                 |
| `UseSandbox`    | `bool`     | `false`     | Use the sandbox endpoint                         |
| `BaseUrl`       | `string`   | `null`      | Override the base URL (optional)                 |
| `Timeout`       | `TimeSpan` | 30 seconds  | Per-request HTTP timeout                         |
| `MaxRetryCount` | `int`      | `0`         | Number of retries on transient failures          |
| `RetryDelay`    | `TimeSpan` | 1 second    | Delay between retry attempts                     |

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

---

## Project Structure

```
src/
└── Two.Payments/
    ├── Core/
    │   ├── Exceptions/     TwoApiException
    │   ├── Interfaces/     ITwoClient, ITwoOrderService
    │   └── Models/         CreateOrderRequest, CreateOrderResponse,
    │                       GetOrderResponse, Buyer, LineItem, …
    ├── Infrastructure/
    │   ├── Configuration/  TwoOptions
    │   └── TwoHttpClient   (internal HTTP wrapper)
    ├── Application/
    │   ├── Services/       TwoOrderService
    │   ├── TwoClient       (ITwoClient implementation)
    │   └── TwoClientFactory
    └── Extensions/
        └── ServiceCollectionExtensions

tests/
└── Two.Payments.Tests/
    └── Services/           TwoOrderServiceTests (xUnit + MockHttp)
```

---

## License

MIT

