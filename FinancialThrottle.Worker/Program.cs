using FinancialThrottle.Worker;
using FinancialThrottleService.Application.Interfaces;
using FinancialThrottleService.Application.Logic;
using FinancialThrottleService.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Infrastructure
builder.Services.AddInfrastructure(builder.Configuration);

// GroupRetryTracker
builder.Services.AddSingleton<GroupRetryTracker>();

// SendConditionEvaluator — turkeyDb config'den gelir
builder.Services.AddSingleton<SendConditionEvaluator>(sp =>
{
    var repo = sp.GetRequiredService<IFinancialRepository>();
    var turkeyDb = builder.Configuration["DatabaseNames:Turkey"] ?? "RAS_STAJ107";
    return new SendConditionEvaluator(repo, turkeyDb);
});
builder.Services.Configure<ThrottleOptions>(
    builder.Configuration.GetSection("ThrottleOptions"));
// Worker
builder.Services.AddHostedService<Worker>();

// gRPC
builder.Services.AddGrpc();
builder.Services.AddControllers();

var app = builder.Build();



app.UseHttpsRedirection();

app.MapControllers();
app.MapGrpcService<FinancialThrottle.Worker.Grpc.ThrottleStatusService>();

app.Run();