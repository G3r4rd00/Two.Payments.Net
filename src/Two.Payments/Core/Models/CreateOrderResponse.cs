using System.Collections.Generic;
using Newtonsoft.Json;

namespace Two.Payments.Core.Models
{
    /// <summary>Response returned by the Two API after creating an order.</summary>
    public class CreateOrderResponse
    {
        /// <summary>Unique identifier assigned to the order by Two.</summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// Current status of the order (e.g. "APPROVED", "REJECTED", "PENDING").
        /// </summary>
        [JsonProperty("status")]
        public string Status { get; set; }

        /// <summary>ISO 4217 currency code.</summary>
        [JsonProperty("currency")]
        public string Currency { get; set; }

        /// <summary>Invoice type used for this order.</summary>
        [JsonProperty("invoice_type")]
        public string InvoiceType { get; set; }

        /// <summary>Gross amount of the order.</summary>
        [JsonProperty("gross_amount")]
        public string GrossAmount { get; set; }

        /// <summary>Net amount of the order.</summary>
        [JsonProperty("net_amount")]
        public string NetAmount { get; set; }

        /// <summary>Tax amount of the order.</summary>
        [JsonProperty("tax_amount")]
        public string TaxAmount { get; set; }

        /// <summary>Buyer details echoed back from the request.</summary>
        [JsonProperty("buyer")]
        public Buyer Buyer { get; set; }

        /// <summary>Line items echoed back from the request.</summary>
        [JsonProperty("line_items")]
        public List<LineItem> LineItems { get; set; }

        /// <summary>
        /// URL to which the buyer should be redirected to complete payment (if applicable).
        /// </summary>
        [JsonProperty("payment_url")]
        public string PaymentUrl { get; set; }

        /// <summary>Merchant-supplied order reference, if provided.</summary>
        [JsonProperty("merchant_order_id")]
        public string MerchantOrderId { get; set; }
    }
}
