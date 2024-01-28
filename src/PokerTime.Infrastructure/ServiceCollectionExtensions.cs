namespace PokerTime.Infrastructure {
    using Common;
    using Microsoft.Extensions.DependencyInjection;

    public static class ServiceCollectionExtensions {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services) =>
            services.AddSingleton<ISystemClock, MachineSystemClock>();
    }
}
