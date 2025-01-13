namespace PokerTime.Domain {
    using Services;
    using Microsoft.Extensions.DependencyInjection;

    public static class ServiceCollectionExtensions {
        public static IServiceCollection AddDomain(this IServiceCollection services) {
            services.AddTransient<IPassphraseService, PassphraseService>();

            return services;
        }
    }
}
