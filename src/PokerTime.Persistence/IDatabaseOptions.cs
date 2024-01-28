namespace PokerTime.Persistence {
    public interface IDatabaseOptions {
        string CreateConnectionString();
        DatabaseProvider DatabaseProvider { get; }
    }


    public enum DatabaseProvider {
        SqlServer,
        Sqlite
    }
}
