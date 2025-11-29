using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

namespace QuikytLoader.Services;

/// <summary>
/// Service for managing SQLite database connections.
/// </summary>
public interface IDbConnectionService
{
    /// <summary>
    /// Gets an open SQLite connection with initialized schema.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An open SQLite connection.</returns>
    Task<SqliteConnection> GetConnectionAsync(CancellationToken cancellationToken = default);
}
