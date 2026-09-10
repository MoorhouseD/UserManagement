using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using UserManagement.Data.Services;

namespace UserManagement.Data.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDataAccess(this IServiceCollection services)
        => services
            .AddDbContext<DataContext>(options => options.UseInMemoryDatabase("UserManagement"))
            .AddScoped<IUserManagementDataService, UserManagementDataService>();
}
