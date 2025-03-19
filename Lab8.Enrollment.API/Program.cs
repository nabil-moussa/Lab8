
using System.Text;
using Lab8.Enrollment.API.Middleware;
using Lab8.Enrollment.Application;
using Lab8.Enrollment.Common.Dtos;
using Lab8.Enrollment.Common.Interfaces;
using Lab8.Enrollment.Infrastructure;
using Lab8.Enrollment.Infrastructure.Multitenancy;
using Lab8.Enrollment.Persistence;
using Lab8.Enrollment.Persistence.Context;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Lab8.Enrollment.Infrastructure.RabbitMQ;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMemoryCache();

builder.Services.AddDbContext<EnrollmentDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly("Lab8.Enrollment.Persistence")));
builder.Services.AddSingleton<RabbitMQPublisher>();
builder.Services.AddSingleton<RabbitMQConsumer>();
builder.Services.AddMultitenancy(builder.Configuration);

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));

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
        policy.RequireRole("admin")); 
    
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
    var dbContext = scope.ServiceProvider.GetRequiredService<EnrollmentDbContext>();

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
app.UseTenantContext();

app.UseAuthorization();

app.MapControllers();

app.Run();

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