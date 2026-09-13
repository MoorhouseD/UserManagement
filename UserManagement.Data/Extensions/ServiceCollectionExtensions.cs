using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using UserManagement.Data.Services;

namespace UserManagement.Data.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDataAccess(this IServiceCollection services, IConfiguration configuration)
        => services
            .AddDbContext<DataContext>(options => options.UseNpgsql(
                configuration.GetConnectionString("UserManagement")
                ?? throw new InvalidOperationException("ConnectionStrings:UserManagement is required.")))
            .AddScoped<IUserManagementDataService, UserManagementDataService>();
}
