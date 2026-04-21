using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Two.Payments.Application;
using Two.Payments.Application.Services;
using Two.Payments.Core.Interfaces;
using Two.Payments.Infrastructure;
using Two.Payments.Infrastructure.Configuration;

namespace Two.Payments.Extensions
{
    /// <summary>
    /// Extension methods for registering Two payment services in a
    /// Microsoft.Extensions.DependencyInjection <see cref="IServiceCollection"/>.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds Two payment services to the service collection.
        /// </summary>
        /// <param name="services">The service collection to configure.</param>
        /// <param name="configure">A delegate to configure <see cref="TwoOptions"/>.</param>
        /// <returns>The updated service collection.</returns>
        /// <example>
        /// <code>
        /// services.AddTwoPayments(o =>
        /// {
        ///     o.ApiKey    = Configuration["Two:ApiKey"];
        ///     o.UseSandbox = true;
        /// });
        /// </code>
        /// </example>
        public static IServiceCollection AddTwoPayments(
            this IServiceCollection services,
            Action<TwoOptions> configure)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (configure == null) throw new ArgumentNullException(nameof(configure));

            var options = new TwoOptions();
            configure(options);
            options.Validate();

            services.TryAddSingleton(options);

            // Register a typed HttpClient; configuration is applied inside TwoHttpClient's constructor.
            services.AddHttpClient<TwoHttpClient>();

            services.TryAddTransient<ITwoOrderService>(sp =>
            {
                var transport = sp.GetRequiredService<TwoHttpClient>();
                var logger = sp.GetService<ILogger<TwoOrderService>>();
                return TwoOrderServiceFactory.Create(transport, logger);
            });

            services.TryAddTransient<ITwoClient>(sp =>
            {
                var orderService = sp.GetRequiredService<ITwoOrderService>();
                return new TwoClient(orderService);
            });

            return services;
        }
    }
}
