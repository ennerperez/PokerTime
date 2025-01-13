namespace PokerTime.Web.Tests.Integration.Common {
    using Microsoft.Extensions.DependencyInjection;

    public interface IAppFixture {
        PokerTimeAppFactory App { get; set; }
        void OnInitialized() { }
        void OnConfiguringTestServices(IServiceCollection services) { }
    }
}
