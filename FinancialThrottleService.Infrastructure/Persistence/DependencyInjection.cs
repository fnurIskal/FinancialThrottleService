using FinancialThrottleService.Application.Interfaces;
using FinancialThrottleService.Infrastructure.Logging;
using FinancialThrottleService.Infrastructure.Models.Generated;
using FinancialThrottleService.Infrastructure.Models.Generated.RAS;
using FinancialThrottleService.Infrastructure.Models.Generated.RAS107;
using FinancialThrottleService.Infrastructure.Persistence.Dummy;
using FinancialThrottleService.Infrastructure.Persistence.Sql;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
namespace FinancialThrottleService.Infrastructure

{

    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
       this IServiceCollection services,
       IConfiguration configuration)
        {
            var useDummyData = configuration.GetValue<bool>("ThrottleOptions:UseDummyData");

            services.AddDbContext<RasStajContext>(options =>
               options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));


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
                services.AddScoped<IFinancialRepository, SqlFinancialRepository>();
                services.AddSingleton<ISecurityPriorityClient, DummySecurityPriorityClient>();
                services.AddSingleton<IFinancialTransactionApi, DummyFinancialTransactionApi>();
                services.AddSingleton<IEmailQueueRepository, DummyEmailQueueRepository>();
            }

            services.Configure<FinancialThrottleService.Infrastructure.Logging.MongoOptions>(
      options => configuration.GetSection("MongoDB").Bind(options));

            services.AddSingleton<ILogRepository, MongoLogRepository>();

            return services;
        }
    }
}
