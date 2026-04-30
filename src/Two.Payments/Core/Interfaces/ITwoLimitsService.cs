using System.Threading;
using System.Threading.Tasks;
using Two.Payments.Core.Models;

namespace Two.Payments.Core.Interfaces
{
    /// <summary>
    /// Provides access to buyer/company credit limit information.
    /// </summary>
    public interface ITwoLimitsService
    {
        /// <summary>
        /// Retrieves buyer company credit limits by country code and organization number.
        /// </summary>
        /// <param name="buyerCountryCode">ISO country code for the buyer company (for example, ES, GB).</param>
        /// <param name="buyerOrganizationNumber">Buyer organization number.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Credit limit response for the buyer company.</returns>
        Task<GetBuyerCreditLimitsResponse> GetBuyerCreditLimitsAsync(
            string buyerCountryCode,
            string buyerOrganizationNumber,
            CancellationToken cancellationToken = default);
    }
}
