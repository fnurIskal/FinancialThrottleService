using FinancialThrottle.Worker;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddHostedService<Worker>();


builder.Services.AddGrpc();

var app = builder.Build();
app.Run();
