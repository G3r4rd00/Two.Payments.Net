using Newtonsoft.Json;

namespace Two.Payments.Core.Models
{
    /// <summary>Represents the buyer (business customer) in an order.</summary>
    public class Buyer
    {
        /// <summary>Contact person at the buyer's company.</summary>
        [JsonProperty("representative")]
        public BuyerRepresentative Representative { get; set; }

        /// <summary>Company information for the buyer.</summary>
        [JsonProperty("company")]
        public BuyerCompany Company { get; set; }
    }
}
