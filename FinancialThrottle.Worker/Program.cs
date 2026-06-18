using FinancialThrottleService.Application.Logic;
using FinancialThrottleService.Infrastructure;
using FinancialThrottle.Worker;
using FinancialThrottleService.Application.Interfaces;
using FinancialThrottleService.Application.Logic;
using Microsoft.AspNetCore.Builder;
using FinancialThrottleService.Infrastructure.Persistence;

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

// Worker
builder.Services.AddHostedService<Worker>();

// gRPC
builder.Services.AddGrpc();

var app = builder.Build();

app.Run();