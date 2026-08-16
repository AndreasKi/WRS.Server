using Microsoft.Extensions.DependencyInjection;
using WRS.Domain.Infrastructure;
using WRS.Infrastructure.Valkey;

namespace WRS.Infrastructure;

public static class InfrastructureCompositionExtensions
{
    extension (IServiceCollection services)
    {
        public IServiceCollection AddInfrastructure()
        {
            services.AddScoped<IRequestRepository, RequestRepository>();

            return services;
        }
    }
}