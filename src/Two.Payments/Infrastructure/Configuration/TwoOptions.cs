using System;

namespace Two.Payments.Infrastructure.Configuration
{
    /// <summary>Configuration options for the Two API client.</summary>
    public class TwoOptions
    {
        /// <summary>
        /// Your Two API key.  
        /// Obtain this from the Two merchant portal under "Developer tools".
        /// </summary>
        public string ApiKey { get; set; }

        /// <summary>
        /// Target environment.  
        /// Set to <c>true</c> to use the sandbox (test) environment;
        /// set to <c>false</c> (default) to use the live production environment.
        /// </summary>
        public bool UseSandbox { get; set; }

        /// <summary>
        /// Optional override for the base URL.  
        /// When null the URL is derived automatically from <see cref="UseSandbox"/>.
        /// </summary>
        public string BaseUrl { get; set; }

        /// <summary>
        /// Timeout for individual HTTP requests.  Defaults to 30 seconds.
        /// </summary>
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);

        /// <summary>
        /// Number of times to retry a failed (transient) request.
        /// Set to 0 (default) to disable retries.
        /// </summary>
        public int MaxRetryCount { get; set; } = 0;

        /// <summary>
        /// Delay between retry attempts.  Defaults to 1 second.
        /// </summary>
        public TimeSpan RetryDelay { get; set; } = TimeSpan.FromSeconds(1);

        /// <summary>
        /// Returns the effective base URL, resolving the sandbox/production
        /// default when no explicit override is set.
        /// </summary>
        public string GetEffectiveBaseUrl()
        {
            if (!string.IsNullOrWhiteSpace(BaseUrl))
            {
                return BaseUrl.TrimEnd('/');
            }

            return UseSandbox
                ? "https://api.sandbox.two.inc/v1"
                : "https://api.two.inc/v1";
        }

        /// <summary>Validates that the required options are present.</summary>
        /// <exception cref="InvalidOperationException">Thrown when <see cref="ApiKey"/> is empty.</exception>
        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(ApiKey))
            {
                throw new InvalidOperationException(
                    "TwoOptions.ApiKey must be set before using the Two client.");
            }
        }
    }
}
