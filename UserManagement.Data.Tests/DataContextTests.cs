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

    [Fact]
    public async Task UpdateUserAsync_WhenUserExists_UpdatesAllFields()
    {
        var context = CreateContext();
        var entity = context.Users.First();
        var dataService = new UserManagementDataService(context);

        var updated = await dataService.UpdateUserAsync(
            entity.Id,
            "Updated",
            "Person",
            new DateOnly(1988, 4, 12),
            "updated@example.com",
            false,
            TestContext.Current.CancellationToken);

        updated.Should().BeTrue();
        var result = await dataService.GetUserByIdAsync(entity.Id, TestContext.Current.CancellationToken);
        result.Should().BeEquivalentTo(new
        {
            entity.Id,
            Forename = "Updated",
            Surname = "Person",
            DateOfBirth = new DateOnly(1988, 4, 12),
            Email = "updated@example.com",
            IsActive = false
        });
    }

    [Fact]
    public async Task DeleteUserAsync_WhenUserExists_RemovesUser()
    {
        var context = CreateContext();
        var entity = context.Users.First();
        var dataService = new UserManagementDataService(context);

        var deleted = await dataService.DeleteUserAsync(entity.Id, TestContext.Current.CancellationToken);

        deleted.Should().BeTrue();
        (await dataService.GetUserByIdAsync(entity.Id, TestContext.Current.CancellationToken)).Should().BeNull();
    }

    [Fact]
    public async Task UserActionLogsAsync_WhenCreated_ReturnsNewestLogsAndSupportsPaging()
    {
        var context = CreateContext();
        var dataService = new UserManagementDataService(context);
        var firstTime = new DateTimeOffset(2026, 1, 1, 10, 0, 0, TimeSpan.Zero);
        var secondTime = firstTime.AddMinutes(1);

        await dataService.CreateUserActionLogAsync(1, "John Smith", "Created", "User account created.", firstTime, TestContext.Current.CancellationToken);
        var created = await dataService.CreateUserActionLogAsync(1, "John Smith", "Viewed", "User details viewed.", secondTime, TestContext.Current.CancellationToken);

        var userLogs = await dataService.GetUserActionLogsAsync(1, TestContext.Current.CancellationToken);
        var page = await dataService.GetUserActionLogsAsync(2, 1, TestContext.Current.CancellationToken);
        var details = await dataService.GetUserActionLogByIdAsync(created.Id, TestContext.Current.CancellationToken);

        userLogs.Select(log => log.Action).Should().ContainInOrder("Viewed", "Created");
        page.Should().ContainSingle().Which.Action.Should().Be("Created");
        details.Should().BeEquivalentTo(created);
        (await dataService.GetUserActionLogCountAsync(TestContext.Current.CancellationToken)).Should().Be(2);
    }
}
