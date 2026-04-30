using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Two.Payments.Core.Interfaces;
using Two.Payments.Core.Models;
using Two.Payments.Infrastructure;

namespace Two.Payments.Application.Services
{
    /// <summary>
    /// Implements <see cref="ITwoLimitsService"/> using the Two Limits API.
    /// </summary>
    public sealed class TwoLimitsService : ITwoLimitsService
    {
        private readonly ITwoApiTransport _http;
        private readonly ILogger<TwoLimitsService> _logger;

        internal TwoLimitsService(ITwoApiTransport http, ILogger<TwoLimitsService> logger = null)
        {
            _http = http ?? throw new ArgumentNullException(nameof(http));
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<GetBuyerCreditLimitsResponse> GetBuyerCreditLimitsAsync(
            string buyerCountryCode,
            string buyerOrganizationNumber,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(buyerCountryCode))
                throw new ArgumentException("buyerCountryCode must not be null or empty.", nameof(buyerCountryCode));

            if (string.IsNullOrWhiteSpace(buyerOrganizationNumber))
                throw new ArgumentException("buyerOrganizationNumber must not be null or empty.", nameof(buyerOrganizationNumber));

            var country = Uri.EscapeDataString(buyerCountryCode.Trim());
            var organizationNumber = Uri.EscapeDataString(buyerOrganizationNumber.Trim());
            var relativeUrl = $"/limits/v1/company/{country}/{organizationNumber}";

            _logger?.LogInformation(
                "Retrieving buyer credit limits. Country={Country} OrganizationNumber={OrganizationNumber}",
                buyerCountryCode,
                buyerOrganizationNumber);

            var response = await _http
                .GetAsync<GetBuyerCreditLimitsResponse>(relativeUrl, cancellationToken)
                .ConfigureAwait(false);

            _logger?.LogInformation(
                "Retrieved buyer credit limits. Country={Country} OrganizationNumber={OrganizationNumber}",
                buyerCountryCode,
                buyerOrganizationNumber);

            return response;
        }
    }
}
