using FinancialThrottle.Worker;
using FinancialThrottleService.Application.Interfaces;
using FinancialThrottleService.Application.Logic;
using FinancialThrottleService.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// Infrastructure
builder.Services.AddInfrastructure(builder.Configuration);

var useDummyData = builder.Configuration.GetValue<bool>("ThrottleOptions:UseDummyData");

// GroupRetryTracker
builder.Services.AddSingleton<GroupRetryTracker>();

// SendConditionEvaluator
if (useDummyData)
{
    // Dummy — Singleton repository
    builder.Services.AddSingleton<SendConditionEvaluator>(sp =>
    {
        var repo = sp.GetRequiredService<IFinancialRepository>();
        var turkeyDb = builder.Configuration["DatabaseNames:Turkey"] ?? "RAS_STAJ107";
        return new SendConditionEvaluator(repo, turkeyDb);
    });
}
else
{
    // Real DB — Scoped repository
    builder.Services.AddScoped<SendConditionEvaluator>(sp =>
    {
        var repo = sp.GetRequiredService<IFinancialRepository>();
        var turkeyDb = builder.Configuration["DatabaseNames:Turkey"] ?? "RAS_STAJ107";
        return new SendConditionEvaluator(repo, turkeyDb);
    });
}

builder.Services.Configure<ThrottleOptions>(
    builder.Configuration.GetSection("ThrottleOptions"));

// Worker
builder.Services.AddHostedService<Worker>();

// gRPC
builder.Services.AddGrpc();
builder.Services.AddControllers();

var app = builder.Build();

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
        var exception = exceptionHandlerPathFeature?.Error;
        Console.WriteLine($"[FATAL] Unhandled exception: {exception}");
        await Task.CompletedTask;
    });
});

app.UseHttpsRedirection();
app.MapControllers();
app.MapGrpcService<FinancialThrottle.Worker.Grpc.ThrottleStatusService>();

app.Run();