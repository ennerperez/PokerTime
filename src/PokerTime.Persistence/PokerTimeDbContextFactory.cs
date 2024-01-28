namespace PokerTime.Persistence {
    using Microsoft.EntityFrameworkCore;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "EF Core infra")]
    internal class PokerTimeDbContextFactory : DesignTimeDbContextFactoryBase<PokerTimeDbContext> {
        protected override PokerTimeDbContext CreateNewInstance(DbContextOptions<PokerTimeDbContext> options) => new PokerTimeDbContext(options);
    }
}
