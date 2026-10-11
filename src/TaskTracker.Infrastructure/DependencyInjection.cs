using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TaskTracker.Application.Abstractions;
using TaskTracker.Domain.Projects;
using TaskTracker.Infrastructure.Persistence;
using TaskTracker.Infrastructure.Persistence.Repositories;

namespace TaskTracker.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString) =>
        services
            .AddDbContext<TaskTrackerDbContext>(options => options.UseSqlite(connectionString))
            .AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<TaskTrackerDbContext>())
            .AddScoped<IProjectRepository, ProjectRepository>();
}
