namespace PokerTime.Web.Configuration {
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.Data.SqlClient;
    using Microsoft.Data.Sqlite;
    using Persistence;

    [ExcludeFromCodeCoverage] // Configuration does not need to be automated tested
    public class DatabaseOptions : IDatabaseOptions {
        private string _cachedConnectionString;

        public string Server { get; set; }
        public string Database { get; set; }
        public string UserId { get; set; }
        public string Password { get; set; }
        public bool? Encrypt { get; set; }
        public bool? IntegratedSecurity { get; set; }
        public int? ConnectionTimeout { get; set; }
        public string ConnectionString { get; set; }
        public DatabaseProvider DatabaseProvider { get; set; }

        public string CreateConnectionString() {
            if (_cachedConnectionString != null) {
                return _cachedConnectionString;
            }

            // Create new conn string
            switch (DatabaseProvider) {
                case DatabaseProvider.SqlServer:
                    return CreateSqlServerConnectionString();
                case DatabaseProvider.Sqlite:
                    return CreateSqliteConnectionString();
                default:
                    throw new InvalidOperationException($"Invalid database provider: {DatabaseProvider}");
            }
        }

        private string CreateSqliteConnectionString() {
            var connStringBuilder = new SqliteConnectionStringBuilder();

            // Set values current connection string
            if (Database != null) connStringBuilder.DataSource = Database;
            connStringBuilder.ForeignKeys = true;
            connStringBuilder.Mode = SqliteOpenMode.ReadWriteCreate;
            connStringBuilder.Cache = SqliteCacheMode.Private;

            // Copy current connection string, overriding options here
            if (!string.IsNullOrEmpty(value: ConnectionString)) {
                var srcConnStringBuilder = new SqliteConnectionStringBuilder(connectionString: ConnectionString);
                foreach (string key in srcConnStringBuilder.Keys ??
                                        throw new InvalidOperationException(message: "Invalid connection string")) {
                    if (key != null && !string.IsNullOrEmpty(srcConnStringBuilder[key]?.ToString())) {
                        connStringBuilder[key] = srcConnStringBuilder[key];
                    }
                }
            }

            return connStringBuilder.ToString();
        }

        private string CreateSqlServerConnectionString() {
            var connStringBuilder = new SqlConnectionStringBuilder();

            // Set values current connection string
            if (ConnectionTimeout != null) connStringBuilder.ConnectTimeout = ConnectionTimeout.Value;
            if (Encrypt != null) connStringBuilder.Encrypt = Encrypt.Value;
            if (IntegratedSecurity != null) connStringBuilder.IntegratedSecurity = IntegratedSecurity.Value;
            if (!string.IsNullOrEmpty(value: UserId)) connStringBuilder.UserID = UserId;
            if (!string.IsNullOrEmpty(value: Password)) connStringBuilder.Password = Password;
            if (!string.IsNullOrEmpty(value: Server)) connStringBuilder.DataSource = Server;
            if (!string.IsNullOrEmpty(value: Database)) connStringBuilder.InitialCatalog = Database;

            // Copy current connection string, overriding options here
            if (!string.IsNullOrEmpty(value: ConnectionString)) {
                var srcConnStringBuilder = new SqlConnectionStringBuilder(connectionString: ConnectionString);
                foreach (string key in srcConnStringBuilder.Keys ??
                                        throw new InvalidOperationException(message: "Invalid connection string")) {
                    if (key != null) {
                        connStringBuilder[keyword: key] = srcConnStringBuilder[keyword: key];
                    }
                }
            }

            // Ensure MultipleActiveResultSets
            connStringBuilder.MultipleActiveResultSets = true;

            // Cache and return
            // (thread safety notice: assignment is atomic)
            return _cachedConnectionString = connStringBuilder.ToString();
        }
    }
}
