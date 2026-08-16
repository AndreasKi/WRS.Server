using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using WRS.Domain.Infrastructure;
using WRS.Infrastructure.Valkey;

namespace WRS.Infrastructure;

public static class InfrastructureCompositionExtensions
{
    extension (IServiceCollection services)
    {
        public IServiceCollection AddInfrastructure(IConfiguration configuration)
        {
            var valkeyOptions = new ValkeyOptions
            {
                Configuration = configuration.GetValue<string>($"{ValkeyOptions.SectionName}:Configuration")
                    ?? throw new InvalidOperationException("Valkey configuration is missing."),
                RequestKeyPrefix = configuration.GetValue<string>($"{ValkeyOptions.SectionName}:RequestKeyPrefix") ?? "requests"
            };

            services.AddSingleton(valkeyOptions);
            services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(valkeyOptions.Configuration));
            services.AddScoped<ITransaction>(serviceProvider =>
                serviceProvider.GetRequiredService<IConnectionMultiplexer>().GetDatabase().CreateTransaction());
            services.AddScoped<IRequestRepository, RequestRepository>();

            return services;
        }
    }
}