using FinancialThrottleService.Application.Interfaces;
using FinancialThrottleService.Infrastructure.Logging;
using FinancialThrottleService.Infrastructure.Models.Generated;
using FinancialThrottleService.Infrastructure.Models.Generated.RAS;
using FinancialThrottleService.Infrastructure.Models.Generated.RAS107;
using FinancialThrottleService.Infrastructure.Models.Generated.RAS107_PROD;
using FinancialThrottleService.Infrastructure.Models.Generated.RAS32501;
using FinancialThrottleService.Infrastructure.Models.Generated.RAS32501_PROD;
using FinancialThrottleService.Infrastructure.Persistence.Dummy;
using FinancialThrottleService.Infrastructure.Persistence.Sql;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

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

            services.AddDbContext<RasStaj107Context>(options =>
                options.UseSqlServer(configuration.GetConnectionString("RasStaj107")));

            services.AddDbContext<RasStaj32501Context>(options =>
                options.UseSqlServer(configuration.GetConnectionString("RasStaj32501")));

            services.AddDbContext<Ras107Context>(options =>
              options.UseSqlServer(configuration.GetConnectionString("Ras107")));

            services.AddDbContext<Ras32501Context>(options =>
              options.UseSqlServer(configuration.GetConnectionString("Ras32501")));

            if (useDummyData)
            {
                services.AddSingleton<IFinancialRepository, DummyFinancialRepository>();
                services.AddSingleton<ISecurityPriorityClient, DummySecurityPriorityClient>();
                services.AddSingleton<IFinancialTransactionApi, DummyFinancialTransactionApi>();
                services.AddSingleton<IEmailQueueRepository, DummyEmailQueueRepository>();
            }
            else
            {
                services.AddScoped<IEmailQueueRepository, SqlEmailQueueRepository>();
                services.AddScoped<IFinancialRepository, SqlFinancialRepository>();
                services.AddScoped<ISecurityPriorityClient, SqlSecurityPriorityClient>();
                services.AddSingleton<IFinancialTransactionApi, SqlFinancialTransactionApi>();
            }

            var mongoSection = configuration.GetSection("MongoDB");
            var mongoConnectionString = mongoSection["ConnectionString"]
                ?? "mongodb://localhost:27017";

            var mongoDatabaseName = mongoSection["DatabaseName"]
                ?? "financial_throttle_logs";

            services.AddSingleton<IMongoClient>(sp =>
                new MongoClient(mongoConnectionString));

            services.AddSingleton<IMongoDatabase>(sp =>
                sp.GetRequiredService<IMongoClient>().GetDatabase(mongoDatabaseName));

            services.AddSingleton<ILogRepository, MongoLogRepository>();

            return services;
        }
    }
}