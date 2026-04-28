using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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

            Trace($"HTTP client initialized. BaseAddress={_httpClient.BaseAddress} Timeout={_httpClient.Timeout}");
            Trace($"Default headers: Accept=application/json, X-API-Key={_options.ApiKey}");
        }

        /// <summary>Sends an HTTP GET request and deserialises the response.</summary>
        public async Task<T> GetAsync<T>(string relativeUrl, CancellationToken cancellationToken)
        {
            Trace($"Preparing GET {BuildAbsoluteUrl(relativeUrl)}");

            return await ExecuteWithRetryAsync<T>(
                "GET",
                relativeUrl,
                null,
                () => _httpClient.GetAsync(relativeUrl, cancellationToken),
                cancellationToken).ConfigureAwait(false);
        }

        /// <summary>Sends an HTTP POST request and deserialises the response.</summary>
        public async Task<T> PostAsync<T>(string relativeUrl, object payload, CancellationToken cancellationToken)
        {
            string json = SerializePayload(payload);
            Trace($"Preparing POST {BuildAbsoluteUrl(relativeUrl)}");
            Trace($"POST payload: {json}");

            return await ExecuteWithRetryAsync<T>(
                "POST",
                relativeUrl,
                json,
                () => _httpClient.PostAsync(relativeUrl, CreateJsonContent(json), cancellationToken),
                cancellationToken).ConfigureAwait(false);
        }

        /// <summary>Sends an HTTP POST request with no response body expected.</summary>
        public async Task PostAsync(string relativeUrl, object payload, CancellationToken cancellationToken)
        {
            string json = SerializePayload(payload);
            Trace($"Preparing POST {BuildAbsoluteUrl(relativeUrl)}");
            Trace($"POST payload: {json}");

            await ExecuteWithRetryAsync<object>(
                "POST",
                relativeUrl,
                json,
                () => _httpClient.PostAsync(relativeUrl, CreateJsonContent(json), cancellationToken),
                cancellationToken,
                allowEmpty: true).ConfigureAwait(false);
        }

        /// <summary>Sends an HTTP DELETE request with no response body expected.</summary>
        public async Task DeleteAsync(string relativeUrl, CancellationToken cancellationToken)
        {
            Trace($"Preparing DELETE {BuildAbsoluteUrl(relativeUrl)}");

            await ExecuteWithRetryAsync<object>(
                "DELETE",
                relativeUrl,
                null,
                () => _httpClient.DeleteAsync(relativeUrl, cancellationToken),
                cancellationToken,
                allowEmpty: true).ConfigureAwait(false);
        }

        // ------------------------------------------------------------------ //

        private async Task<T> ExecuteWithRetryAsync<T>(
            string method,
            string relativeUrl,
            string payloadJson,
            Func<Task<HttpResponseMessage>> sendFunc,
            CancellationToken cancellationToken,
            bool allowEmpty = false)
        {
            int attempt = 0;
            int maxAttempts = Math.Max(1, _options.MaxRetryCount + 1);
            string absoluteUrl = BuildAbsoluteUrl(relativeUrl);

            while (true)
            {
                attempt++;
                HttpResponseMessage response = null;

                try
                {
                    Trace($"Sending {method} request to {absoluteUrl}. Attempt {attempt}/{maxAttempts}");
                    if (!string.IsNullOrWhiteSpace(payloadJson))
                    {
                        Trace($"Request body on attempt {attempt}: {payloadJson}");
                    }

                    response = await sendFunc().ConfigureAwait(false);
                    Trace($"Received HTTP {(int)response.StatusCode} ({response.StatusCode}) from {absoluteUrl}");
                }
                catch (HttpRequestException ex) when (attempt < maxAttempts)
                {
                    Trace($"HTTP request exception on attempt {attempt}/{maxAttempts}: {ex}");
                    _logger?.LogWarning(ex,
                        "Two API request failed (attempt {Attempt}/{Max}). Retrying in {Delay}ms.",
                        attempt, maxAttempts, _options.RetryDelay.TotalMilliseconds);

                    await Task.Delay(_options.RetryDelay, cancellationToken).ConfigureAwait(false);
                    continue;
                }
                catch (Exception ex)
                {
                    Trace($"Unexpected exception while sending {method} {absoluteUrl}: {ex}");
                    throw;
                }

                return await ProcessResponseAsync<T>(method, absoluteUrl, response, allowEmpty).ConfigureAwait(false);
            }
        }

        private async Task<T> ProcessResponseAsync<T>(string method, string absoluteUrl, HttpResponseMessage response, bool allowEmpty)
        {
            string body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            Trace($"Processing response for {method} {absoluteUrl}");
            Trace($"Response headers: {response.Headers}");
            Trace($"Response body: {(string.IsNullOrWhiteSpace(body) ? "<empty>" : body)}");

            if (response.IsSuccessStatusCode)
            {
                if (allowEmpty)
                {
                    Trace("Successful response with empty body allowed.");
                    return default;
                }

                if (string.IsNullOrWhiteSpace(body))
                {
                    Trace("Successful response with empty body.");
                    return default;
                }

                Trace($"Deserializing successful response to {typeof(T).FullName}");
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
                    var json = JObject.Parse(body);

                    var apiError = json.ToObject<TwoApiError>();
                    if (!string.IsNullOrWhiteSpace(apiError?.ErrorCode))
                    {
                        errorCode = apiError.ErrorCode;
                    }

                    if (!string.IsNullOrWhiteSpace(apiError?.ErrorMessage))
                    {
                        errorMessage = apiError.ErrorMessage;
                    }
                    else
                    {
                        var detail = (string)json["detail"];
                        var message = (string)json["message"];
                        var title = (string)json["title"];

                        errorMessage = !string.IsNullOrWhiteSpace(detail)
                            ? detail
                            : !string.IsNullOrWhiteSpace(message)
                                ? message
                                : !string.IsNullOrWhiteSpace(title)
                                    ? title
                                    : errorMessage;
                    }

                    if (errorMessage.StartsWith("Two API error: HTTP", StringComparison.Ordinal))
                    {
                        errorMessage = $"{errorMessage}. Response: {body}";
                    }
                }
                catch (JsonException)
                {
                    errorMessage = $"{errorMessage}. Response: {body}";
                }
            }

            Trace($"Throwing TwoApiException. StatusCode={(int)response.StatusCode} ErrorCode={errorCode ?? "<null>"} Message={errorMessage}");
            throw new TwoApiException(errorMessage, (int)response.StatusCode, errorCode);
        }

        private string SerializePayload(object payload)
        {
            string json = payload != null
                ? JsonConvert.SerializeObject(payload, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                })
                : "{}";

            _logger?.LogDebug("Request JSON: {Json}", json);
            return json;
        }

        private StringContent CreateJsonContent(string json)
        {
            return new StringContent(json, Encoding.UTF8, "application/json");
        }

        private string BuildAbsoluteUrl(string relativeUrl)
        {
            return new Uri(_httpClient.BaseAddress, relativeUrl).ToString();
        }

        private void Trace(string message)
        {
            var formatted = $"[{DateTime.UtcNow:O}] [TwoHttpClient] {message}";
            Console.WriteLine(formatted);
            _logger?.LogInformation(formatted);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (!_disposed)
            {
                Trace("Disposing HTTP client.");
                _httpClient?.Dispose();
                _disposed = true;
            }
        }
    }
}
