using Microsoft.Extensions.DependencyInjection;
using UserManagement.Data.Services;

namespace UserManagement.Data.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDataAccess(this IServiceCollection services)
        => services
            .AddEntityFramework()
            .AddScoped<IUserManagementDataService, UserManagementDataService>();

    public static IServiceCollection AddEntityFramework(this IServiceCollection services)
        => services.AddScoped<IDataContext, DataContext>();
}
