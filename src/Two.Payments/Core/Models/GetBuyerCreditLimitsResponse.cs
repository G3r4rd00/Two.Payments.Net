using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Two.Payments.Core.Models
{
    /// <summary>
    /// Response returned by the Limits API when retrieving company credit limits.
    /// </summary>
    public class GetBuyerCreditLimitsResponse
    {
        [JsonProperty("buyer_country_code")]
        public string BuyerCountryCode { get; set; }

        [JsonProperty("buyer_organization_number")]
        public string BuyerOrganizationNumber { get; set; }

        [JsonProperty("canonical_id")]
        public string CanonicalId { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("credit_limit")]
        public decimal? CreditLimit { get; set; }

        [JsonProperty("available_credit")]
        public decimal? AvailableCredit { get; set; }

        [JsonProperty("used_credit")]
        public decimal? UsedCredit { get; set; }

        [JsonProperty("limits")]
        public List<BuyerCreditLimitItem> Limits { get; set; }

        [JsonExtensionData]
        public IDictionary<string, JToken> AdditionalData { get; set; }
    }

    /// <summary>
    /// Represents a credit limit entry.
    /// </summary>
    public class BuyerCreditLimitItem
    {
        [JsonProperty("invoice_type")]
        public string InvoiceType { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("credit_limit")]
        public decimal? CreditLimit { get; set; }

        [JsonProperty("available_credit")]
        public decimal? AvailableCredit { get; set; }

        [JsonProperty("used_credit")]
        public decimal? UsedCredit { get; set; }

        [JsonExtensionData]
        public IDictionary<string, JToken> AdditionalData { get; set; }
    }
}
