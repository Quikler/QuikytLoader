using Microsoft.Data.Sqlite;

namespace QuikytLoader.Infrastructure.Persistence;

/// <summary>
/// Factory for creating database connections
/// </summary>
internal interface IDbConnectionFactory
{
    /// <summary>
    /// Gets an open SQLite connection with initialized schema
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>An open SQLite connection</returns>
    Task<SqliteConnection> GetConnectionAsync(CancellationToken cancellationToken = default);
}
