using System.Diagnostics;
using System.Reflection;
using System.Text;
using AspNetCoreRateLimit;
using FluentValidation;
using LoyaltyApi.Config;
using LoyaltyApi.Data;
using LoyaltyApi.Middlewares;
using LoyaltyApi.Models;
using LoyaltyApi.Repositories;
using LoyaltyApi.Services;
using LoyaltyApi.Utilities;
using LoyaltyApi.Validators;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;

namespace LoyaltyApi
{
    public class Startup(IWebHostEnvironment env,
    IConfiguration configuration)
    {
        public void ConfigureServices(IServiceCollection services)
        {
            Log.Logger.Information("Setting configurations");

            services.AddHttpContextAccessor();
            services.Configure<JwtOptions>(configuration.GetSection("JwtOptions"));
            services.Configure<FacebookOptions>(configuration.GetSection("FacebookOptions"));
            services.Configure<Config.GoogleOptions>(configuration.GetSection("GoogleOptions"));

            services.Configure<API>(configuration.GetSection("API"));

            services.Configure<ApiKey>(configuration.GetSection("ApiKey"));
            services.Configure<EmailOptions>(configuration.GetSection("EmailOptions"));
            services.Configure<AdminOptions>(configuration.GetSection("AdminOptions"));
            services.Configure<FrontendOptions>(configuration.GetSection("FrontendOptions"));
            services.AddMemoryCache();
            services.Configure<IpRateLimitOptions>(configuration.GetSection("IpRateLimiting"));
            services.AddInMemoryRateLimiting();
            services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

            // Add HttpClient factory for better socket management
            services.AddHttpClient();

            // Configure named HttpClient for API calls with optimized settings
            services.AddHttpClient("ApiClient", client =>
            {
                client.Timeout = TimeSpan.FromSeconds(30);
                client.DefaultRequestHeaders.Add("User-Agent", "Rock-Loyalty-System/1.0");
            })
            .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler()
            {
                MaxConnectionsPerServer = 100, // Increased for high concurrent usage
                PooledConnectionLifetime = TimeSpan.FromMinutes(15), // Longer lifetime for efficiency
                PooledConnectionIdleTimeout = TimeSpan.FromMinutes(5), // Close idle connections after 2 minutes
                ConnectTimeout = TimeSpan.FromSeconds(10) // Fail fast if server is unresponsive
            });

            Log.Logger.Information("Configuring configurations done");

            Log.Logger.Information("Configuring controllers");
            services.AddControllers(options =>
            {
                // Add the ValidateModel action filter globally
                options.Filters.Add<LoyaltyApi.filters.ValidateModelAttribute>();
            });

            // Register FluentValidation
            services.AddValidatorsFromAssemblyContaining<LoginRequestBodyValidator>();

            Log.Logger.Information("Configuring controllers done");

            Log.Logger.Information("Configuring services in container");

            services.AddScoped<ITokenRepository, TokenRepository>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IUserService, UserService>();
            if (!env.IsEnvironment("Frontend"))
            {
                services.AddScoped<IUserRepository, UserRepository>();
                services.AddScoped<IVoucherRepository, VoucherRepository>();
            }
            else
            {
                services.AddScoped<IUserRepository, UserFrontendRepository>();
                services.AddScoped<IVoucherRepository, VoucherFrontendRepository>();
            }
            services.AddScoped<IVoucherService, VoucherService>();
            services.AddScoped<IRestaurantRepository, RestaurantRepository>();
            services.AddScoped<IRestaurantService, RestaurantService>();
            services.AddScoped<ICreditPointsTransactionRepository, CreditPointsTransactionRepository>();
            services.AddScoped(provider =>

              new OAuth2Service(new HttpClient())
            );
            services.AddScoped<ApiUtility>();
            services.AddScoped<VoucherUtility>();
            services.AddScoped<CreditPointsUtility>();
            services.AddScoped<TokenUtility>();
            services.AddScoped<ParserUtility>();
            services.AddScoped<EmailService>();
            services.AddScoped<ICreditPointsTransactionDetailRepository, CreditPointsTransactionDetailRepository>();
            services.AddScoped<ICreditPointsTransactionRepository, CreditPointsTransactionRepository>();
            services.AddScoped<ICreditPointsTransactionService, CreditPointsTransactionService>();
            services.AddScoped<IPasswordHasher<Password>, PasswordHasher<Password>>();
            services.AddScoped<IPasswordRepository, PasswordRepository>();
            services.AddScoped<IPasswordService, PasswordService>();

            // FluentEmail configuration
            var emailFromAddress = Environment.GetEnvironmentVariable("EmailOptions__FromAddress");
            var emailFromName = Environment.GetEnvironmentVariable("EmailOptions__FromName");
            var emailSmtpServer = Environment.GetEnvironmentVariable("EmailOptions__SmtpServer");
            var emailSmtpPort = Environment.GetEnvironmentVariable("EmailOptions__SmtpPort");
            var emailUsername = Environment.GetEnvironmentVariable("EmailOptions__Username");
            var emailPassword = Environment.GetEnvironmentVariable("EmailOptions__Password");

            Log.Logger.Information($"Email config: FromAddress={emailFromAddress}, FromName={emailFromName}, SmtpServer={emailSmtpServer}, SmtpPort={emailSmtpPort}, Username={emailUsername}");

            if (string.IsNullOrWhiteSpace(emailFromAddress) || string.IsNullOrWhiteSpace(emailFromName) || string.IsNullOrWhiteSpace(emailSmtpServer) || string.IsNullOrWhiteSpace(emailSmtpPort) || string.IsNullOrWhiteSpace(emailUsername) || string.IsNullOrWhiteSpace(emailPassword))
                throw new ArgumentNullException("One or more email config values are missing. Check .env and configuration loading.");

            services
                .AddFluentEmail(emailFromAddress, emailFromName)
                .AddSmtpSender(
                    emailSmtpServer,
                    int.Parse(emailSmtpPort),
                    emailUsername,
                    emailPassword
                );

            Log.Logger.Information("Configuring services in container done");


            Log.Logger.Information("Configuring database");
            // Database setup
            if (env.IsEnvironment("Testing"))
            {
                Log.Logger.Information("Setting up SQLite database");

                services.AddDbContext<RockDbContext>(options =>
                    options.UseSqlite("Data Source=DikaRockDbContext.db"));

                Log.Logger.Information("Setting up SQLite database done");
            }
            else if (env.IsEnvironment("Frontend"))
            {
                Log.Logger.Information("Setting up SQLite database for FRONTEND");

                // Frontend environment needs both contexts
                services.AddDbContext<FrontendDbContext>(options =>
                    options.UseSqlite("Data Source=DikaFrontend.db"));

                Log.Logger.Information("Setting up SQLite database done");
            }
            else if (env.IsDevelopment())
            {
                Log.Logger.Information("Setting up MySQL database for DEVELOPMENT");

                services.AddDbContext<RockDbContext>(options =>
                {
                    options.UseMySql(configuration.GetSection("ConnectionStrings:DefaultConnection").Value,
                    new MySqlServerVersion(new Version(8, 0, 29)));
                });
                Log.Logger.Information("Setting up MySQL database done");
            }

            Log.Logger.Information("Configuring database done");

            Log.Logger.Information("Configuring authentication");

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                var jwtOptions = configuration.GetSection("JwtOptions").Get<JwtOptions>();
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions?.SigningKey ?? throw new InvalidOperationException("JWT signing key not found"))),
                };
            })
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddGoogle(options =>
            {
                options.ClientId = configuration["GoogleOptions:ClientId"]! ?? throw new InvalidOperationException("Google ClientId not found");
                options.ClientSecret = configuration["GoogleOptions:ClientSecret"]! ?? throw new InvalidOperationException("Google ClientSecret not found");
            });
            Log.Logger.Information("Configuring authentication done");

            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(options =>
            {
                // Get the path to the XML documentation file
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

                // Include the XML documentation in Swagger
                options.IncludeXmlComments(xmlPath);

                // Add JWT Bearer authentication to Swagger
                options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                    Description = "Enter 'Bearer' [space] and then your valid token in the text input below.\n\nExample: \"Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9\""
                });

                options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
                {
                    {
                        new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                        {
                            Reference = new Microsoft.OpenApi.Models.OpenApiReference
                            {
                                Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] {}
                    }
                });
            });
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAngularClient", builder =>
                {
                    var frontendOptions = configuration.GetSection("FrontendOptions").Get<FrontendOptions>() ?? throw new ArgumentException("Frontend options missing");
                    builder.WithOrigins(frontendOptions.BaseUrl)
                    .AllowAnyHeader()
                    .AllowCredentials()
                    .AllowAnyMethod();
                });
            });
            // Remove the AddExceptionHandler line - we'll use middleware instead
        }
        public void Configure(WebApplication app, IWebHostEnvironment env, DbContext dbContext)
        {
            Log.Logger.Information("Configuring web application");

            // Use custom exception handling middleware
            app.UseMiddleware<GlobalExceptionHandler>();

            app.UseIpRateLimiting();

            if (env.IsDevelopment() || env.IsEnvironment("Frontend") || env.IsEnvironment("Testing"))
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // app.UseMiddleware<ApiKeyValidatorMiddleware>();

            if (env.IsEnvironment("Testing") || env.IsEnvironment("Frontend"))
            {
                AddMigrationsAndUpdateDatabase(dbContext);
            }

            app.UseStatusCodePages(async context =>
            {
                if (context.HttpContext.Response.StatusCode == StatusCodes.Status404NotFound)
                {
                    // You can render a custom view for 404 errors
                    context.HttpContext.Response.ContentType = "text/html";
                    await context.HttpContext.Response.WriteAsync("<h1>404 - Page Not Found</h1>");
                }
            });
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseCors("AllowAngularClient");
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            Log.Logger.Information("Configuring web application done");
        }
        private static void AddMigrationsAndUpdateDatabase(DbContext dbContext)
        {
            Log.Logger.Information("Adding migrations and updating database");

            // Simply apply any pending migrations
            dbContext.Database.Migrate();

            Log.Logger.Information("Adding migrations and updating database done");
        }
    }
}