using FinancialThrottle.Grpc;
using FinancialThrottleService.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddGrpcClient<ThrottleService.ThrottleServiceClient>(o =>
{
    o.Address = new Uri("http://localhost:5000");
});

builder.Services.AddControllers();
var app = builder.Build();

app.MapControllers();

app.Run();
