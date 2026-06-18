using FinancialThrottleService.Application.Interfaces;
using FinancialThrottleService.Infrastructure.Logging;
using FinancialThrottleService.Infrastructure.Persistence.Dummy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
namespace FinancialThrottleService.Infrastructure

{

    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
       this IServiceCollection services,
       IConfiguration configuration)
        {
            var useDummyData = configuration.GetValue<bool>("ThrottleOptions:UseDummyData");

            if (useDummyData)
            {
                // Geliştirme ortamı — sahte implementasyonlar
                services.AddSingleton<IFinancialRepository, DummyFinancialRepository>();
                services.AddSingleton<ISecurityPriorityClient, DummySecurityPriorityClient>();
                services.AddSingleton<IFinancialTransactionApi, DummyFinancialTransactionApi>();
                services.AddSingleton<IEmailQueueRepository, DummyEmailQueueRepository>();
            }
            else
            {
                // Üretim ortamı — gerçek implementasyonlar (DB bağlantısı gelince eklenecek)
                // services.AddScoped<IFinancialRepository,     SqlFinancialRepository>();
                // services.AddScoped<ISecurityPriorityClient,  HttpSecurityPriorityClient>();
                // services.AddScoped<IFinancialTransactionApi, HttpFinancialTransactionApi>();
                // services.AddScoped<IEmailQueueRepository,    SqlEmailQueueRepository>();
                throw new InvalidOperationException(
                    "Üretim implementasyonları henüz hazır değil. " +
                    "appsettings.Development.json'da 'UseDummyData': true olmalı.");
            }

            services.Configure<FinancialThrottleService.Infrastructure.Logging.MongoOptions>(
      options => configuration.GetSection("MongoDB").Bind(options));

            services.AddSingleton<ILogRepository, MongoLogRepository>();

            return services;
        }
    }
}
