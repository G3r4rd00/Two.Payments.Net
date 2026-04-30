using Microsoft.Extensions.Logging;
using Two.Payments.Application.Services;
using Two.Payments.Core.Interfaces;
using Two.Payments.Infrastructure;

namespace Two.Payments.Application
{
    /// <summary>
    /// Internal factory for creating <see cref="TwoLimitsService"/> instances.
    /// </summary>
    internal static class TwoLimitsServiceFactory
    {
        internal static ITwoLimitsService Create(ITwoApiTransport transport, ILogger<TwoLimitsService> logger = null)
        {
            return new TwoLimitsService(transport, logger);
        }
    }
}
