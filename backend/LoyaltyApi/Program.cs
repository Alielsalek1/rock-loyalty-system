using DotNetEnv;
using LoyaltyApi;
using LoyaltyApi.Data;
using Serilog;

Env.Load();

var builder = WebApplication.CreateBuilder();
builder.Configuration.AddEnvironmentVariables();
Log.Logger = new LoggerConfiguration()
           .ReadFrom.Configuration(builder.Configuration)
           .Enrich.FromLogContext()
           .WriteTo.Console()
           .WriteTo.File("Logs/LoyaltyApi-{Date}.log", rollingInterval: RollingInterval.Infinite)
           .CreateLogger();

builder.Host.UseSerilog();
builder.Logging.ClearProviders();
builder.Logging.AddConfiguration(builder.Configuration);
builder.Logging.AddSerilog();

var startup = new Startup(builder.Environment, builder.Configuration);

startup.ConfigureServices(builder.Services);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var frontend = app.Environment.IsEnvironment("Frontend");
    if (frontend)
    {
        var frontendDbContext = scope.ServiceProvider.GetRequiredService<FrontendDbContext>();
        startup.Configure(app, app.Environment, frontendDbContext);
    }
    else
    {
        var rockdDbContext = scope.ServiceProvider.GetRequiredService<RockDbContext>();
        startup.Configure(app, app.Environment, rockdDbContext);
    }

    // Pass the DbContext to Startup's Configure method
}

app.Run();