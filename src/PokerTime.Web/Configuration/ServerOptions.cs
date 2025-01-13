namespace PokerTime.Web.Configuration {
    using System.Diagnostics.CodeAnalysis;

    [ExcludeFromCodeCoverage] // Configuration does not need to be automated tested
    public class ServerOptions {
        [SuppressMessage("Design", "CA1056:Uri properties should not be strings", Justification = "User configurable")]
        public string BaseUrl { get; set; }
    }
}
