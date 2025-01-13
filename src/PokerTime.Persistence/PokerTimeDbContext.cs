namespace PokerTime.Persistence {
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Application.Common.Abstractions;
    using Conventions;
    using Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

    [ExcludeFromCodeCoverage] // No use testing the database context
    public sealed class PokerTimeDbContext : DbContext, IPokerTimeDbContext, IEntityStateFacilitator {
        private const string SqliteProvider = "Microsoft.EntityFrameworkCore.Sqlite";

        private readonly DbContextOptions _options;
        private readonly IDatabaseOptions _databaseOptions;

#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public PokerTimeDbContext(DbContextOptions options) : base(options) {
            _options = options;
        }

        public PokerTimeDbContext(IDatabaseOptions databaseOptions) {
            _databaseOptions = databaseOptions;
        }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
            // Use connection string if available
            if (_databaseOptions != null) {
                switch (_databaseOptions.DatabaseProvider) {
                    case DatabaseProvider.SqlServer:
                        optionsBuilder.UseSqlServer(_databaseOptions.CreateConnectionString(), sql => sql.EnableRetryOnFailure());
                        break;
                    case DatabaseProvider.Sqlite:
                        SqliteConfigurator.ConfigureDbContext(optionsBuilder, _databaseOptions);
                        break;
                    default:
                        throw new InvalidOperationException($"Invalid database provider: {_databaseOptions.DatabaseProvider}");
                }
            }

            // Error logging (DEBUG only)
#if DEBUG
            optionsBuilder.EnableDetailedErrors();
            optionsBuilder.EnableSensitiveDataLogging();
#endif
        }

        public DbSet<PredefinedParticipantColor> PredefinedParticipantColors { get; set; }
        public DbSet<Participant> Participants { get; set; }
        public DbSet<Estimation> Estimations { get; set; }
        public DbSet<Symbol> Symbols { get; set; }
        public DbSet<SymbolSet> SymbolSets { get; set; }
        public DbSet<UserStory> UserStories { get; set; }
        public DbSet<Session> Sessions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            if (modelBuilder == null) throw new ArgumentNullException(nameof(modelBuilder));

            // Configurations
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(PokerTimeDbContext).Assembly);

            // Conventions
            modelBuilder.RemovePluralizingTableNameConvention();

            // Fix datetime offset support for integration tests
            // See: https://blog.dangl.me/archive/handling-datetimeoffset-in-sqlite-with-entity-framework-core/
            if (Database.ProviderName == SqliteProvider) {
                // SQLite does not have proper support for DateTimeOffset via Entity Framework Core, see the limitations
                // here: https://docs.microsoft.com/en-us/ef/core/providers/sqlite/limitations#query-limitations
                // To work around this, when the Sqlite database provider is used, all model properties of type DateTimeOffset
                // use the DateTimeOffsetToBinaryConverter
                // Based on: https://github.com/aspnet/EntityFrameworkCore/issues/10784#issuecomment-415769754
                // This only supports millisecond precision, but should be sufficient for most use cases.
                foreach (var entityType in modelBuilder.Model.GetEntityTypes()) {
                    var properties = entityType.ClrType.GetProperties().Where(p => p.PropertyType == typeof(DateTimeOffset));
                    foreach (var property in properties) {
                        if (entityType.IsOwned() == false) {
                            modelBuilder
                                .Entity(entityType.Name)
                                .Property(property.Name)
                                .HasConversion(new DateTimeOffsetToBinaryConverter());
                        }
                    }
                }
            }
        }

        public Task Reload(object entity, CancellationToken cancellationToken) {
            var entry = Entry(entity);
            return entry.ReloadAsync(cancellationToken);
        }

        public IPokerTimeDbContext CreateForEditContext() => _databaseOptions != null ? new PokerTimeDbContext(_databaseOptions) : new PokerTimeDbContext(_options);

        public void Initialize() {
            if (Database.ProviderName == SqliteProvider) {
                Database.EnsureCreated();
            }
            else {
                Database.Migrate();
            }
        }
    }
}
