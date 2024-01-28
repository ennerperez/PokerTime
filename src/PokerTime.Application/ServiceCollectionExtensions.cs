namespace PokerTime.Application {
    using System.Reflection;
    using Common.Behaviours;
    using Common.Security;
    using MediatR;
    using Microsoft.Extensions.DependencyInjection;
    using Notifications;
    using Sessions.Queries.GetSessionStatus;
    using SessionWorkflows.Common;

    public static class ServiceCollectionExtensions {
        public static IServiceCollection AddApplication(this IServiceCollection services) {
            services.AddAutoMapper(Assembly.GetExecutingAssembly());

            var assemblies = new[] { Assembly.GetExecutingAssembly() };
            services.AddMediatR(opts =>
            {
                opts.RegisterServicesFromAssemblies(assemblies);
                opts.AddBehavior(typeof(IPipelineBehavior<,>), typeof(RequestPerformanceBehaviour<,>));
                opts.AddBehavior(typeof(IPipelineBehavior<,>), typeof(RequestValidationBehaviour<,>));
            });


            services.AddScoped<ISessionStatusMapper, SessionStatusMapper>();
            services.AddScoped<ISessionStatusUpdateDispatcher, SessionStatusUpdateDispatcher>();
            services.AddScoped<ISecurityValidator, SecurityValidator>();

            services.AddNotificationDispatchers();

            return services;
        }
    }
}
