using Microsoft.Extensions.Logging;
using Two.Payments.Application.Services;
using Two.Payments.Core.Interfaces;
using Two.Payments.Infrastructure;

namespace Two.Payments.Application
{
    /// <summary>
    /// Internal factory that creates <see cref="TwoOrderService"/> instances.
    /// Used by both <see cref="TwoClientFactory"/> and DI registration to avoid
    /// leaking internal types into the public API surface.
    /// </summary>
    internal static class TwoOrderServiceFactory
    {
        internal static ITwoOrderService Create(ITwoApiTransport transport, ILogger<TwoOrderService> logger = null)
        {
            return new TwoOrderService(transport, logger);
        }
    }
}
