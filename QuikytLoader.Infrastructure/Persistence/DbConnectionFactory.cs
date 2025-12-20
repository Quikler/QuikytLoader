using Dapper;
using Microsoft.Data.Sqlite;
using QuikytLoader.Application.Interfaces.Repositories;

namespace QuikytLoader.Infrastructure.Persistence;

/// <summary>
/// Factory for managing SQLite database connections and schema initialization.
/// Database file is created automatically by SQLite when first connection is opened.
/// </summary>
internal class DbConnectionFactory : IDbConnectionFactory
{
    private readonly string _dbPath;
    private readonly string _connectionString;

    public DbConnectionFactory()
    {
        // Store database in XDG config directory alongside settings.json
        var configDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".config",
            "QuikytLoader"
        );

        Directory.CreateDirectory(configDir);
        _dbPath = Path.Combine(configDir, "history.db");
        _connectionString = $"Data Source={_dbPath}";
    }

    /// <summary>
    /// Gets an open SQLite connection with initialized schema.
    /// SQLite automatically creates the database file on first connection if it doesn't exist.
    /// </summary>
    public async Task<SqliteConnection> GetConnectionAsync(CancellationToken cancellationToken = default)
    {
        var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        // Ensure schema exists (idempotent - safe to run multiple times)
        await InitializeSchemaAsync(connection);

        // Set restrictive file permissions on Linux (user read/write only)
        if (OperatingSystem.IsLinux() && File.Exists(_dbPath))
            File.SetUnixFileMode(_dbPath, UnixFileMode.UserRead | UnixFileMode.UserWrite);

        return connection;
    }

    /// <summary>
    /// Initializes the database schema.
    /// CREATE TABLE IF NOT EXISTS is idempotent and fast when table already exists.
    /// </summary>
    private static async Task InitializeSchemaAsync(SqliteConnection connection)
    {
        const string createTableSql = """
            CREATE TABLE IF NOT EXISTS DownloadHistory (
                YouTubeId TEXT PRIMARY KEY CHECK(length(YouTubeId) = 11 AND trim(YouTubeId) != ''),
                VideoTitle TEXT NOT NULL,
                DownloadedAt TEXT NOT NULL
            )
            """;

        await connection.ExecuteAsync(createTableSql);
    }
}
