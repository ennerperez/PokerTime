namespace PokerTime.Persistence.Conventions {
    using Microsoft.EntityFrameworkCore;

    internal static class PluralizationConventions {
        public static void RemovePluralizingTableNameConvention(this ModelBuilder modelBuilder) {
            foreach (var entity in modelBuilder.Model.GetEntityTypes()) {
                if (!entity.IsOwned()) {
                    entity.SetTableName(entity.DisplayName());
                }
            }
        }
    }
}
