using System.Threading.RateLimiting;
using Host.Modularity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.OpenApi;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog((context, log) => log.ReadFrom.Configuration(context.Configuration).WriteTo.Console());
var modules = ModuleLoader.LoadModules();
foreach (var module in modules) module.RegisterServices(builder.Services, builder.Configuration);
var controllers = builder.Services.AddControllers();
foreach (var module in modules) controllers.AddApplicationPart(module.GetType().Assembly);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    foreach (var module in modules)
        options.SwaggerDoc(module.ApiGroupName, new OpenApiInfo { Title = module.Name, Version = "v1" });
});
builder.Services.AddHealthChecks();
builder.Services.AddProblemDetails();
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("auth", context => RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions { PermitLimit = 20, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));
});
var app = builder.Build();
app.UseExceptionHandler();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        foreach (var module in modules) options.SwaggerEndpoint($"/swagger/{module.ApiGroupName}/swagger.json", module.Name);
    });
}
app.UseSerilogRequestLogging();
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();
foreach (var module in modules) module.UseMiddleware(app);
app.MapControllers();
app.MapHealthChecks("/health");
foreach (var module in modules) module.MapEndpoints(app);
app.Run();
