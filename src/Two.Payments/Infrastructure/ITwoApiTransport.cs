using System.Threading;
using System.Threading.Tasks;

namespace Two.Payments.Infrastructure
{
    /// <summary>
    /// Internal contract for the low-level HTTP transport used by service classes.
    /// Abstracted to support testability without exposing implementation details.
    /// </summary>
    internal interface ITwoApiTransport
    {
        Task<T> GetAsync<T>(string relativeUrl, CancellationToken cancellationToken);
        Task<T> PostAsync<T>(string relativeUrl, object payload, CancellationToken cancellationToken);
        Task PostAsync(string relativeUrl, object payload, CancellationToken cancellationToken);
        Task DeleteAsync(string relativeUrl, CancellationToken cancellationToken);
    }
}
