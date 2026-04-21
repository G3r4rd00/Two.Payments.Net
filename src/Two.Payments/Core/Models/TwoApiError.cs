using Newtonsoft.Json;

namespace Two.Payments.Core.Models
{
    /// <summary>Represents an error payload returned by the Two API.</summary>
    public class TwoApiError
    {
        /// <summary>Short error code identifying the type of error.</summary>
        [JsonProperty("error_code")]
        public string ErrorCode { get; set; }

        /// <summary>Human-readable description of the error.</summary>
        [JsonProperty("error_message")]
        public string ErrorMessage { get; set; }

        /// <summary>Optional additional detail or context about the error.</summary>
        [JsonProperty("error_details")]
        public string ErrorDetails { get; set; }
    }
}
