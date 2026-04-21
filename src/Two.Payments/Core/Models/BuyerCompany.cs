using Newtonsoft.Json;

namespace Two.Payments.Core.Models
{
    /// <summary>Represents the buyer's company information.</summary>
    public class BuyerCompany
    {
        /// <summary>Two-letter ISO 3166-1 alpha-2 country code (e.g. "GB", "NO").</summary>
        [JsonProperty("country_prefix")]
        public string CountryPrefix { get; set; }

        /// <summary>Organization or registration number of the company.</summary>
        [JsonProperty("organization_number")]
        public string OrganizationNumber { get; set; }

        /// <summary>Legal name of the company.</summary>
        [JsonProperty("company_name")]
        public string CompanyName { get; set; }
    }
}
