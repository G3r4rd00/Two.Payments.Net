using System;

namespace Two.Payments.Core.Exceptions
{
    /// <summary>
    /// Represents an error returned by the Two API.
    /// </summary>
    public class TwoApiException : Exception
    {
        /// <summary>Gets the HTTP status code returned by the API.</summary>
        public int StatusCode { get; }

        /// <summary>Gets the error code returned by the API, if available.</summary>
        public string ErrorCode { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="TwoApiException"/>.
        /// </summary>
        /// <param name="message">Human-readable error message.</param>
        /// <param name="statusCode">HTTP status code.</param>
        /// <param name="errorCode">API-specific error code.</param>
        public TwoApiException(string message, int statusCode, string errorCode = null)
            : base(message)
        {
            StatusCode = statusCode;
            ErrorCode = errorCode;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="TwoApiException"/> with an inner exception.
        /// </summary>
        public TwoApiException(string message, int statusCode, string errorCode, Exception innerException)
            : base(message, innerException)
        {
            StatusCode = statusCode;
            ErrorCode = errorCode;
        }
    }
}
