using System.Collections.Generic;
using Newtonsoft.Json;
using Two.Payments.Infrastructure.Serialization;

namespace Two.Payments.Core.Models
{
    /// <summary>Request payload for creating a new order via the Two API.</summary>
    public class CreateOrderRequest
    {
        /// <summary>ISO 4217 currency code (e.g. "GBP", "NOK", "EUR").</summary>
        [JsonProperty("currency")]
        public string Currency { get; set; }

        /// <summary>
        /// Type of invoice to generate.
        /// Common values: "DIRECT_INVOICE", "FUNDED_INVOICE".
        /// </summary>
        [JsonProperty("invoice_type")]
        public string InvoiceType { get; set; }

        /// <summary>Gross (total) order amount including tax, as a string (e.g. "400.00").</summary>
        [JsonProperty("gross_amount")]
        [JsonConverter(typeof(StringNumberJsonConverter))]
        public string GrossAmount { get; set; }

        /// <summary>Net order amount excluding tax, as a string.</summary>
        [JsonProperty("net_amount")]
        [JsonConverter(typeof(StringNumberJsonConverter))]
        public string NetAmount { get; set; }

        /// <summary>Tax amount, as a string.</summary>
        [JsonProperty("tax_amount")]
        [JsonConverter(typeof(StringNumberJsonConverter))]
        public string TaxAmount { get; set; }

        /// <summary>Discount amount applied to the order, as a string.</summary>
        [JsonProperty("discount_amount")]
        [JsonConverter(typeof(StringNumberJsonConverter))]
        public string DiscountAmount { get; set; }

        /// <summary>Discount rate as a decimal string (e.g. "0.10" for 10%).</summary>
        [JsonProperty("discount_rate")]
        [JsonConverter(typeof(StringNumberJsonConverter))]
        public string DiscountRate { get; set; }

        /// <summary>Tax rate as a decimal string (e.g. "0.20" for 20%).</summary>
        [JsonProperty("tax_rate")]
        [JsonConverter(typeof(StringNumberJsonConverter))]
        public string TaxRate { get; set; }

        /// <summary>Buyer information (company and contact person).</summary>
        [JsonProperty("buyer")]
        public Buyer Buyer { get; set; }

        /// <summary>Billing address for the order.</summary>
        [JsonProperty("billing_address", NullValueHandling = NullValueHandling.Ignore)]
        public BillingAddress BillingAddress { get; set; }

        /// <summary>Shipping address for the order.</summary>
        [JsonProperty("shipping_address", NullValueHandling = NullValueHandling.Ignore)]
        public BillingAddress ShippingAddress { get; set; }

        /// <summary>Line items included in the order.</summary>
        [JsonProperty("line_items")]
        public List<LineItem> LineItems { get; set; }

        /// <summary>Optional merchant-supplied order reference.</summary>
        [JsonProperty("merchant_order_id", NullValueHandling = NullValueHandling.Ignore)]
        public string MerchantOrderId { get; set; }

        /// <summary>Optional merchant-supplied additional reference.</summary>
        [JsonProperty("merchant_additional_info", NullValueHandling = NullValueHandling.Ignore)]
        public string MerchantAdditionalInfo { get; set; }

        /// <summary>URL to redirect the buyer to after order confirmation.</summary>
        [JsonProperty("merchant_urls", NullValueHandling = NullValueHandling.Ignore)]
        public MerchantUrls MerchantUrls { get; set; }
    }

    /// <summary>Billing address for the order.</summary>
    public class BillingAddress
    {
        private string _streetAddress;
        private string _country;

        /// <summary>Organization or company name for billing.</summary>
        [JsonProperty("organization_name", NullValueHandling = NullValueHandling.Ignore)]
        public string OrganizationName { get; set; }

        /// <summary>Street address.</summary>
        [JsonProperty("street_address", NullValueHandling = NullValueHandling.Ignore)]
        public string StreetAddress
        {
            get => _streetAddress;
            set => _streetAddress = value;
        }

        /// <summary>Legacy alias for street address.</summary>
        [JsonIgnore]
        public string Address
        {
            get => _streetAddress;
            set => _streetAddress = value;
        }

        /// <summary>Postal or ZIP code.</summary>
        [JsonProperty("postal_code", NullValueHandling = NullValueHandling.Ignore)]
        public string PostalCode { get; set; }

        /// <summary>City or locality.</summary>
        [JsonProperty("city", NullValueHandling = NullValueHandling.Ignore)]
        public string City { get; set; }

        /// <summary>Country code expected by Two.</summary>
        [JsonProperty("country", NullValueHandling = NullValueHandling.Ignore)]
        public string Country
        {
            get => _country;
            set => _country = value;
        }

        /// <summary>Legacy alias for country.</summary>
        [JsonIgnore]
        public string CountryPrefix
        {
            get => _country;
            set => _country = value;
        }
    }

    /// <summary>Callback URLs provided by the merchant.</summary>
    public class MerchantUrls
    {
        /// <summary>URL to redirect the buyer after successful confirmation.</summary>
        [JsonProperty("merchant_confirmation_url", NullValueHandling = NullValueHandling.Ignore)]
        public string MerchantConfirmationUrl { get; set; }

        /// <summary>URL to redirect the buyer if they cancel the checkout.</summary>
        [JsonProperty("merchant_cancel_order_url", NullValueHandling = NullValueHandling.Ignore)]
        public string MerchantCancelOrderUrl { get; set; }
    }
}
