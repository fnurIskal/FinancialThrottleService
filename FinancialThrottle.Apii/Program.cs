using FinancialThrottle.Grpc;
using FinancialThrottleService.Infrastructure;
using FinancialThrottleService.Infrastructure.Models.Generated;
using FinancialThrottleService.Infrastructure.Models.Generated.RAS;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddControllers();

builder.Services.AddGrpcClient<ThrottleService.ThrottleServiceClient>(o =>
{
    var url = builder.Configuration["GrpcWorker:Url"] ?? "http://localhost:5000";
    o.Address = new Uri(url);
});

builder.Services.AddDbContext<RasStajContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();


// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Financial Throttle Service API",
        Version = "v1.0",
        Description = "Financial queue processing REST API",
        Contact = new OpenApiContact
        {
            Name = "Development Team",
            Email = "dev@example.com"
        }
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1.0");
        options.RoutePrefix = string.Empty;
    });
}
//app.UseHttpsRedirection();
app.UseCors("Frontend");
app.MapControllers();

app.Run();
