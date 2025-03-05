using Lab8.Course.Application.Services;
using Lab8.Course.Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Lab8.Course.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ICourseService, CourseService>();
        return services;
    }
}