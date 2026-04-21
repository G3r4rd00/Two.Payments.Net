using System.Threading;
using System.Threading.Tasks;
using Two.Payments.Core.Models;

namespace Two.Payments.Core.Interfaces
{
    /// <summary>
    /// Main entry-point interface for the Two payment client.
    /// Aggregates all Two API service operations.
    /// </summary>
    public interface ITwoClient
    {
        /// <summary>Provides access to order-related operations.</summary>
        ITwoOrderService Orders { get; }
    }
}
