using Microsoft.Extensions.DependencyInjection;
using WRS.Domain.Requests;

namespace WRS.Domain;

public static class DomainCompositionExtensions
{
    extension (IServiceCollection services)
    {
        public IServiceCollection AddDomain()
        {
            services.AddScoped<IRequestService, RequestService>();

            return services;
        }
    }
}