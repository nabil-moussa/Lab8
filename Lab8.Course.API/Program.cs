using System.Text;
using Lab8.Course.API.Middleware;
using Lab8.Course.Application;
using Lab8.Course.Common.Dtos;
using Lab8.Course.Infrastructure;
using Lab8.Course.Infrastructure.Multitenancy;
using Lab8.Course.Persistence;
using Lab8.Course.Persistence.Context;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Lab8.Course.Common.Multitenancy;
using Lab8.Course.Infrastructure.RabbitMQ;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMemoryCache();

builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddSingleton<RabbitMQPublisher>();
builder.Services.AddSingleton<RabbitMQConsumer>();builder.Services.AddApplication();
builder.Services.AddMultitenancy(builder.Configuration);

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
builder.Services.AddSingleton<TokenDistributionConsumer>();
builder.Services.AddHostedService<TokenDistributionBackgroundService>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => 
        policy.RequireRole("admin")); // or policy.RequireClaim(ClaimTypes.Role, "Admin")
    
    options.AddPolicy("SameBranch", policy => 
        policy.RequireClaim("BranchId"));
    
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var tenantService = scope.ServiceProvider.GetRequiredService<ITenantService>();
    var dbContext = scope.ServiceProvider.GetRequiredService<CourseDbContext>();

    await dbContext.EnsureSchemaCreatedAsync("default");
    await dbContext.EnsureSchemaCreatedAsync("branch1");
    await dbContext.EnsureSchemaCreatedAsync("branch2");
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseMiddleware<TokenValidationMiddleware>();
app.UseTenantContext();

app.UseAuthorization();

app.MapControllers();

app.Run();

public class TokenDistributionBackgroundService : BackgroundService
{
    private readonly TokenDistributionConsumer _consumer;
    private readonly ILogger<TokenDistributionBackgroundService> _logger;

    public TokenDistributionBackgroundService(
        TokenDistributionConsumer consumer,
        ILogger<TokenDistributionBackgroundService> logger)
    {
        _consumer = consumer;
        _logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try 
        {
            _consumer.StartConsuming();
            _logger.LogInformation("Token distribution consumer started.");
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting token distribution consumer");
            return Task.CompletedTask;
        }
    }
}
public static class MultitenancyServiceExtensions
{
    public static IServiceCollection AddMultitenancy(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        services.AddSingleton<ITenantService, TenantService>();

        var tenants = configuration.GetSection("Tenants").Get<Dictionary<string, string>>();
        if (tenants != null)
        {
            var tenantService = services.BuildServiceProvider().GetRequiredService<ITenantService>();
            foreach (var tenant in tenants)
            {
                tenantService.AddTenant(tenant.Key, tenant.Value);
            }
        }

        return services;
    }
}
