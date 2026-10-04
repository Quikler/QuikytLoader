using System.Data;

namespace QuikytLoader.Infrastructure.Persistence;

public interface IDbConnectionFactory
{
    /// <summary>
    /// Gets an open database connection with initialized schema
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>An open database connection</returns>
    Task<IDbConnection> GetConnectionAsync(CancellationToken cancellationToken = default);
}
