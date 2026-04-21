using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Two.Payments.Core.Exceptions;
using Two.Payments.Core.Models;
using Two.Payments.Infrastructure.Configuration;

namespace Two.Payments.Infrastructure
{
    /// <summary>
    /// Low-level HTTP helper that wraps <see cref="HttpClient"/> to communicate
    /// with the Two REST API.  Handles authentication, serialisation and error
    /// mapping.
    /// </summary>
    internal sealed class TwoHttpClient : ITwoApiTransport, IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly TwoOptions _options;
        private readonly ILogger _logger;
        private bool _disposed;

        internal TwoHttpClient(HttpClient httpClient, TwoOptions options, ILogger logger = null)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _logger = logger;

            _httpClient.BaseAddress = new Uri(_options.GetEffectiveBaseUrl() + "/");
            _httpClient.Timeout = _options.Timeout;
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.Add("X-API-Key", _options.ApiKey);
        }

        /// <summary>Sends an HTTP GET request and deserialises the response.</summary>
        public async Task<T> GetAsync<T>(string relativeUrl, CancellationToken cancellationToken)
        {
            return await ExecuteWithRetryAsync<T>(
                () => _httpClient.GetAsync(relativeUrl, cancellationToken),
                cancellationToken).ConfigureAwait(false);
        }

        /// <summary>Sends an HTTP POST request and deserialises the response.</summary>
        public async Task<T> PostAsync<T>(string relativeUrl, object payload, CancellationToken cancellationToken)
        {
            return await ExecuteWithRetryAsync<T>(
                () => _httpClient.PostAsync(relativeUrl, Serialize(payload), cancellationToken),
                cancellationToken).ConfigureAwait(false);
        }

        /// <summary>Sends an HTTP POST request with no response body expected.</summary>
        public async Task PostAsync(string relativeUrl, object payload, CancellationToken cancellationToken)
        {
            await ExecuteWithRetryAsync<object>(
                () => _httpClient.PostAsync(relativeUrl, Serialize(payload), cancellationToken),
                cancellationToken,
                allowEmpty: true).ConfigureAwait(false);
        }

        /// <summary>Sends an HTTP DELETE request with no response body expected.</summary>
        public async Task DeleteAsync(string relativeUrl, CancellationToken cancellationToken)
        {
            await ExecuteWithRetryAsync<object>(
                () => _httpClient.DeleteAsync(relativeUrl, cancellationToken),
                cancellationToken,
                allowEmpty: true).ConfigureAwait(false);
        }

        // ------------------------------------------------------------------ //

        private async Task<T> ExecuteWithRetryAsync<T>(
            Func<Task<HttpResponseMessage>> sendFunc,
            CancellationToken cancellationToken,
            bool allowEmpty = false)
        {
            int attempt = 0;
            int maxAttempts = Math.Max(1, _options.MaxRetryCount + 1);

            while (true)
            {
                attempt++;
                HttpResponseMessage response = null;

                try
                {
                    response = await sendFunc().ConfigureAwait(false);
                }
                catch (HttpRequestException ex) when (attempt < maxAttempts)
                {
                    _logger?.LogWarning(ex,
                        "Two API request failed (attempt {Attempt}/{Max}). Retrying in {Delay}ms.",
                        attempt, maxAttempts, _options.RetryDelay.TotalMilliseconds);

                    await Task.Delay(_options.RetryDelay, cancellationToken).ConfigureAwait(false);
                    continue;
                }

                return await ProcessResponseAsync<T>(response, allowEmpty).ConfigureAwait(false);
            }
        }

        private async Task<T> ProcessResponseAsync<T>(HttpResponseMessage response, bool allowEmpty)
        {
            string body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                if (allowEmpty || string.IsNullOrWhiteSpace(body))
                {
                    return default;
                }

                return JsonConvert.DeserializeObject<T>(body);
            }

            _logger?.LogError(
                "Two API returned {StatusCode}. Response body: {Body}",
                (int)response.StatusCode, body);

            string errorCode = null;
            string errorMessage = $"Two API error: HTTP {(int)response.StatusCode}";

            if (!string.IsNullOrWhiteSpace(body))
            {
                try
                {
                    var apiError = JsonConvert.DeserializeObject<TwoApiError>(body);
                    if (apiError != null)
                    {
                        errorCode = apiError.ErrorCode;
                        errorMessage = apiError.ErrorMessage ?? errorMessage;
                    }
                }
                catch (JsonException)
                {
                    // Body is not valid JSON; use the raw body as the message.
                    errorMessage = body;
                }
            }

            throw new TwoApiException(errorMessage, (int)response.StatusCode, errorCode);
        }

        private static StringContent Serialize(object payload)
        {
            string json = payload != null
                ? JsonConvert.SerializeObject(payload, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                })
                : "{}";

            return new StringContent(json, Encoding.UTF8, "application/json");
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (!_disposed)
            {
                _httpClient?.Dispose();
                _disposed = true;
            }
        }
    }
}
