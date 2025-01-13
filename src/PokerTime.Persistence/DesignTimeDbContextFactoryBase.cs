namespace PokerTime.Persistence {
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using Common;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Design;
    using Microsoft.Extensions.Configuration;

    [ExcludeFromCodeCoverage]
    public abstract class DesignTimeDbContextFactoryBase<TContext> :
        IDesignTimeDbContextFactory<TContext> where TContext : DbContext {
        private const string AspNetCoreEnvironment = "ASPNETCORE_ENVIRONMENT";

        public TContext CreateDbContext(string[] args) {
            var basePath = Directory.GetCurrentDirectory() + string.Format(Culture.Invariant, "{0}..{0}PokerTime.Web", Path.DirectorySeparatorChar);
            return Create(basePath, Environment.GetEnvironmentVariable(AspNetCoreEnvironment));
        }

        protected abstract TContext CreateNewInstance(DbContextOptions<TContext> options);

        private TContext Create(string basePath, string environmentName) {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json")
                .AddJsonFile("appsettings.local.json", optional: true)
                .AddJsonFile($"appsettings.{environmentName}.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var databaseOptions = (IDatabaseOptions)configuration.GetSection("Database").Get(Type.GetType("PokerTime.Web.Configuration.DatabaseOptions, PokerTime.Web", true));
            var connectionString = databaseOptions.CreateConnectionString();

            return Create(connectionString);
        }

        private TContext Create(string connectionString) {
            if (string.IsNullOrEmpty(connectionString)) {
                throw new ArgumentException($"Connection string is null or empty.", nameof(connectionString));
            }

            Console.WriteLine($"DesignTimeDbContextFactoryBase.Create(string): Connection string: '{connectionString}'.");

            var optionsBuilder = new DbContextOptionsBuilder<TContext>();

            optionsBuilder.UseSqlServer(connectionString);

            return CreateNewInstance(optionsBuilder.Options);
        }
    }
}
