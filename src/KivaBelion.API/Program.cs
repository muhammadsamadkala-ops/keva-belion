using System.Threading.RateLimiting;
using KivaBelion.API.Common;
using KivaBelion.API.Data;
using KivaBelion.API.Data.Entities;
using KivaBelion.API.HealthChecks;
using KivaBelion.API.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// ---------- Services (dependency injection) ----------
builder.Services.AddControllers();
builder.Services.AddOpenApi();                        // machine-readable API description (Development only, see below)

// Every error becomes the same standard JSON shape ("problem details").
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// Database access (EF Core + PostgreSQL). The connection string comes from User Secrets / environment variables.
builder.Services.AddDbContext<KivaBelionDbContext>((serviceProvider, options) =>
    options.UseNpgsql(serviceProvider.GetRequiredService<IConfiguration>().GetConnectionString("Default")));

// ---------- Admin sign-in (Phase C1) ----------
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddOptions<JwtSettings>().BindConfiguration("Jwt");   // read lazily so every settings source is included
builder.Services.AddOptions<PasswordHasherOptions>().Configure(o => o.IterationCount = 210_000);
builder.Services.AddSingleton<IPasswordHasher<AdminUser>, PasswordHasher<AdminUser>>();
builder.Services.AddScoped<IAdminUserStore, EfAdminUserStore>();
builder.Services.AddScoped<AdminAuthService>();
builder.Services.AddSingleton<ITokenService, TokenService>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
builder.Services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
    .Configure<IOptions<JwtSettings>>((options, jwt) =>
    {
        var settings = jwt.Value;
        options.MapInboundClaims = false;                 // keep claim names exactly as issued ("role", "email" ...)
        options.RequireHttpsMetadata = false;             // no external authority is used
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = settings.Issuer,
            ValidateAudience = true,
            ValidAudience = settings.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30),
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(settings.SigningKey)),
            ValidAlgorithms = new[] { SecurityAlgorithms.HmacSha256 },
            RoleClaimType = "role",
            NameClaimType = "name"
        };
    });

builder.Services.AddAuthorizationBuilder()
    .AddPolicy(AuthorizationPolicies.AdminOnly, policy => policy.RequireRole(nameof(AdminRole.Admin)))
    .AddPolicy(AuthorizationPolicies.StaffOrAdmin, policy => policy.RequireRole(nameof(AdminRole.Admin), nameof(AdminRole.Staff)));

// Health checks: "live" = the app runs, "ready" = the app can also reach the database.
builder.Services.AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>("database", tags: new[] { "ready" });

// CORS: only the websites listed in "Cors:AllowedOrigins" may call this API from a browser.
builder.Services.AddCors(options =>
{
    // The list is read later (not now) so that settings from every source are included.
    options.AddPolicy(CorsPolicies.Frontend, policy => { });
});
builder.Services.AddOptions<Microsoft.AspNetCore.Cors.Infrastructure.CorsOptions>()
    .Configure<IConfiguration>((options, config) =>
    {
        var origins = StartupChecks.GetAllowedOrigins(config);
        options.AddPolicy(CorsPolicies.Frontend, policy =>
            policy.WithOrigins(origins)
                  .WithMethods("GET", "POST", "PUT", "PATCH", "DELETE")
                  .WithHeaders("Content-Type", "Authorization", "Idempotency-Key", "X-Correlation-Id")
                  .WithExposedHeaders("X-Correlation-Id"));
    });

// Rate limiting: a simple safety net against floods (120 requests per minute per IP address).
// Login has its own stricter limit below; checkout limits come later.
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // Stricter limit for admin login: a few attempts per minute per IP address (setting RateLimiting:LoginPermitPerMinute).
    options.AddPolicy("login", context =>
    {
        var permits = context.RequestServices.GetRequiredService<IConfiguration>().GetValue("RateLimiting:LoginPermitPerMinute", 5);
        return RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions { PermitLimit = permits, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 });
    });
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 120,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));
});

var app = builder.Build();

// ---------- Fail fast with a clear message if settings are wrong ----------
StartupChecks.Validate(app.Configuration, app.Environment, app.Logger);

// ---------- One-time command: create the first admin (see scripts\create-admin.ps1) ----------
if (args.FirstOrDefault() == AdminBootstrap.Command)
{
    Environment.ExitCode = await AdminBootstrap.CreateAdminAsync(app.Services, args);
    return;
}

// ---------- Request pipeline (order matters) ----------
app.UseMiddleware<CorrelationIdMiddleware>();         // gives every request an id that appears in logs and responses
app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseExceptionHandler();                            // turns unexpected errors into a safe JSON 500
app.UseStatusCodePages();                             // turns 404 / 405 etc. into the same JSON shape

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseCors(CorsPolicies.Frontend);
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();                                 // http://localhost:5080/openapi/v1.json
}

app.MapControllers();
app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });
app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = check => check.Tags.Contains("ready") });

app.Run();

// Makes the Program class visible to the automated tests.
public partial class Program { }
