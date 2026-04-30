using System;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using Two.Payments.Application.Services;
using Two.Payments.Core.Interfaces;
using Two.Payments.Infrastructure;
using Two.Payments.Infrastructure.Configuration;

namespace Two.Payments.Application
{
    /// <summary>
    /// Convenience factory for creating a <see cref="TwoClient"/> without a
    /// dependency-injection container.
    /// </summary>
    /// <example>
    /// <code>
    /// var client = TwoClientFactory.Create(new TwoOptions
    /// {
    ///     ApiKey    = "your-api-key",
    ///     UseSandbox = true
    /// });
    /// </code>
    /// </example>
    public static class TwoClientFactory
    {
        /// <summary>
        /// Creates a fully configured <see cref="ITwoClient"/> using default settings.
        /// </summary>
        /// <param name="options">Connection and authentication options.</param>
        /// <param name="logger">Optional logger factory for diagnostic output.</param>
        /// <returns>A ready-to-use <see cref="ITwoClient"/> instance.</returns>
        public static ITwoClient Create(TwoOptions options, ILoggerFactory logger = null)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            options.Validate();

            var httpClient = new HttpClient();
            var twoHttp = new TwoHttpClient(
                httpClient,
                options,
                logger?.CreateLogger<TwoHttpClient>());

            var orderService = TwoOrderServiceFactory.Create(
                twoHttp,
                logger?.CreateLogger<TwoOrderService>());

            var limitsService = TwoLimitsServiceFactory.Create(
                twoHttp,
                logger?.CreateLogger<TwoLimitsService>());

            return new TwoClient(orderService, limitsService);
        }

        /// <summary>
        /// Creates a fully configured <see cref="ITwoClient"/> using a pre-built
        /// <see cref="HttpClient"/> (useful for testing or custom message handlers).
        /// </summary>
        /// <param name="httpClient">HttpClient to use for requests.</param>
        /// <param name="options">Connection and authentication options.</param>
        /// <param name="logger">Optional logger factory.</param>
        public static ITwoClient Create(HttpClient httpClient, TwoOptions options, ILoggerFactory logger = null)
        {
            if (httpClient == null) throw new ArgumentNullException(nameof(httpClient));
            if (options == null) throw new ArgumentNullException(nameof(options));
            options.Validate();

            var twoHttp = new TwoHttpClient(
                httpClient,
                options,
                logger?.CreateLogger<TwoHttpClient>());

            var orderService = TwoOrderServiceFactory.Create(
                twoHttp,
                logger?.CreateLogger<TwoOrderService>());

            var limitsService = TwoLimitsServiceFactory.Create(
                twoHttp,
                logger?.CreateLogger<TwoLimitsService>());

            return new TwoClient(orderService, limitsService);
        }
    }
}
