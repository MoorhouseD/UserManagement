using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using UserManagement.Data;

namespace UserManagement.Web.Health;

public sealed class DatabaseHealthCheck(DataContext dataContext) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var canConnect = await dataContext.Database.CanConnectAsync(cancellationToken);
        return canConnect
            ? HealthCheckResult.Healthy("The database is available.")
            : HealthCheckResult.Unhealthy("The database is unavailable.");
    }
}
