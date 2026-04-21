using Newtonsoft.Json;

namespace Two.Payments.Core.Models
{
    /// <summary>Represents the buyer's representative (contact person).</summary>
    public class BuyerRepresentative
    {
        /// <summary>First name of the representative.</summary>
        [JsonProperty("first_name")]
        public string FirstName { get; set; }

        /// <summary>Last name of the representative.</summary>
        [JsonProperty("last_name")]
        public string LastName { get; set; }

        /// <summary>Phone number of the representative.</summary>
        [JsonProperty("phone_number")]
        public string PhoneNumber { get; set; }

        /// <summary>Email address of the representative.</summary>
        [JsonProperty("email")]
        public string Email { get; set; }
    }
}
