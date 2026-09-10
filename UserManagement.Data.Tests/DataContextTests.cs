using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UserManagement.Data.Entities;
using UserManagement.Data.Services;

namespace UserManagement.Data.Tests;

public class DataContextTests
{
    private static DataContext CreateContext(string? databaseName = null)
    {
        var context = new DataContext(new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName ?? Guid.NewGuid().ToString())
            .Options);

        context.Database.EnsureCreated();
        return context;
    }

    [Fact]
    public async Task GetUsersAsync_WhenNewEntityAdded_MustIncludeNewEntity()
    {
        var context = CreateContext();
        var entity = new User
        {
            Forename = "Brand New",
            Surname = "User",
            DateOfBirth = new DateOnly(1990, 1, 1),
            Email = "brandnewuser@example.com",
            NormalizedEmail = "BRANDNEWUSER@EXAMPLE.COM",
            IsActive = true
        };

        context.Users.Add(entity);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await new UserManagementDataService(context).GetUsersAsync(null, TestContext.Current.CancellationToken);

        result.Should().Contain(user => user.Email == entity.Email);
    }

    [Fact]
    public async Task GetUsersAsync_WhenDeleted_MustNotIncludeDeletedEntity()
    {
        var context = CreateContext();
        var entity = context.Users.First();
        context.Users.Remove(entity);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await new UserManagementDataService(context).GetUsersAsync(null, TestContext.Current.CancellationToken);

        result.Should().NotContain(user => user.Email == entity.Email);
    }

    [Fact]
    public async Task GetUsersAsync_WhenNewEntityAdded_HasAllFieldsPopulated()
    {
        var context = CreateContext();

        var entity = new User
        {
            Forename = "John",
            Surname = "Smith",
            DateOfBirth = new DateOnly(1990, 1, 1),
            Email = "john.smith@email.com",
            NormalizedEmail = "JOHN.SMITH@EMAIL.COM",
            IsActive = true
        };

        context.Users.Add(entity);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await new UserManagementDataService(context).GetUsersAsync(null, TestContext.Current.CancellationToken);

        result.Should().Contain(user => user.Email == entity.Email)
            .Which.Should().BeEquivalentTo(new
            {
                entity.Id,
                entity.Forename,
                entity.Surname,
                entity.DateOfBirth,
                entity.Email,
                entity.IsActive
            });
    }

    [Fact]
    public async Task GetUsersAsync_WhenFilterIsActive_OnlyReturnsActiveUsers()
    {
        var context = CreateContext();
        var dataService = new UserManagementDataService(context);

        var result = await dataService.GetUsersAsync(true, TestContext.Current.CancellationToken);

        result.Should().OnlyContain(user => user.IsActive);
    }

    [Fact]
    public async Task GetUsersAsync_WhenContextsAreSeparate_DoesNotLeakData()
    {
        var alpha = CreateContext("alpha");
        var beta = CreateContext("beta");

        alpha.Users.Add(new User
        {
            Forename = "Alpha",
            Surname = "User",
            DateOfBirth = new DateOnly(1990, 5, 1),
            Email = "alpha@example.com",
            NormalizedEmail = "ALPHA@EXAMPLE.COM",
            IsActive = true
        });
        await alpha.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await new UserManagementDataService(beta).GetUsersAsync(null, TestContext.Current.CancellationToken);

        result.Should().NotContain(user => user.Email == "alpha@example.com");
    }
}
