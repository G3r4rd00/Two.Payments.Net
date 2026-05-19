using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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

        /// <summary>Optional structured error details returned by the API.</summary>
        [JsonProperty("error_json")]
        public JArray ErrorJson { get; set; }

        /// <summary>Optional trace identifier returned by the API.</summary>
        [JsonProperty("error_trace_id")]
        public string ErrorTraceId { get; set; }
    }
}
