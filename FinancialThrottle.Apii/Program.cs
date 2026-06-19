using FinancialThrottle.Grpc;
using FinancialThrottleService.Infrastructure;
using FinancialThrottleService.Infrastructure.Models.Generated;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddGrpcClient<ThrottleService.ThrottleServiceClient>(o =>
{
    o.Address = new Uri("http://localhost:5000");
});

builder.Services.AddDbContext<RasStajContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();
var app = builder.Build();

app.MapControllers();

app.Run();
