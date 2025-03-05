using Lab8.Enrollment.Persistence.Context;

namespace Lab8.Enrollment.API.Middleware;

public class TenantContextMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TenantContextMiddleware> _logger;

    public TenantContextMiddleware(
        RequestDelegate next, 
        ILogger<TenantContextMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, EnrollmentDbContext dbContext)
    {
        var branchId = context.User.FindFirst("BranchId")?.Value;

        if (string.IsNullOrEmpty(branchId))
        {
            context.Request.Headers.TryGetValue("X-Branch-Id", out var headerBranchId);
            branchId = headerBranchId.FirstOrDefault();
        }

        branchId = string.IsNullOrEmpty(branchId) ? "default" : branchId;

        dbContext.SetBranchId(branchId);

        _logger.LogInformation($"Setting branch context to: {branchId}");

        await _next(context);
    }
}
public static class TenantContextMiddlewareExtensions
{
    public static IApplicationBuilder UseTenantContext(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<TenantContextMiddleware>();
    }
}