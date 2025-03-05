using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Lab8.Enrollment.Common.Dtos;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Lab8.Enrollment.API.Middleware;

public class TokenValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IMemoryCache _cache;
    private readonly JwtSettings _jwtSettings;
    private readonly ILogger<TokenValidationMiddleware> _logger;

    public TokenValidationMiddleware(
        RequestDelegate next,
        IMemoryCache cache,
        IOptions<JwtSettings> jwtSettings,
        ILogger<TokenValidationMiddleware> logger)
    {
        _next = next;
        _cache = cache;
        _jwtSettings = jwtSettings.Value;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
        string token = null;

        if (authHeader != null && authHeader.StartsWith("Bearer "))
        {
            token = authHeader.Substring("Bearer ".Length).Trim();
        }

        if (string.IsNullOrEmpty(token))
        {
            if (!_cache.TryGetValue("AuthToken", out token))
            {
                _logger.LogWarning("No token found in header or cache");
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Token not found.");
                return;
            }
        }

        // Validate the token and extract claims
        var principal = ValidateAndExtractClaims(token);
        if (principal == null)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Invalid token.");
            return;
        }

        context.User = principal;

        await _next(context);
    }

    private ClaimsPrincipal ValidateAndExtractClaims(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(token) as JwtSecurityToken;
            
            if (jsonToken == null)
            {
                _logger.LogError("Unable to read token");
                return null;
            }

            var issuerClaim = jsonToken.Claims.FirstOrDefault(c => c.Type == "iss");
            var actualIssuer = issuerClaim?.Value ?? "unknown";

            _logger.LogInformation($"Extracted Issuer: {actualIssuer}");
            _logger.LogInformation($"Token Audiences: {string.Join(", ", jsonToken.Audiences)}");

            var key = Encoding.UTF8.GetBytes(_jwtSettings.Key);
            
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = false, 
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidAudience = _jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ClockSkew = TimeSpan.FromMinutes(5)
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
            
            var claims = principal.Claims.ToList();

            // Log claims
            foreach (var claim in claims)
            {
                _logger.LogInformation($"Claim - Type: {claim.Type}, Value: {claim.Value}");
            }

            return principal;
        }
        catch (Exception ex)
        {
            // Log detailed error information
            _logger.LogError(ex, $"Token validation failed. Error: {ex.Message}");
            return null;
        }
    }
}