using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace UserManagement.Data;

public sealed class DesignTimeDataContextFactory : IDesignTimeDbContextFactory<DataContext>
{
    public DataContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseNpgsql("Host=localhost;Port=5432;Database=usermanagement;Username=postgres;Password=postgres")
            .Options;

        return new DataContext(options);
    }
}