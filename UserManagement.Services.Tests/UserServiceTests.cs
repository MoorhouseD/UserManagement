using System.Collections.Generic;
using System;
using System.Threading;
using System.Threading.Tasks;
using UserManagement.Data.Models;
using UserManagement.Data.Services;
using UserManagement.Services.Implementations;

namespace UserManagement.Services.Tests;

public class UserServiceTests
{
    private readonly Mock<IUserManagementDataService> _dataContext = new();

    private UserService CreateUserService() => new(_dataContext.Object);

    [Fact]
    public async Task GetAll_WhenContextReturnsEntities_MustReturnSameEntities()
    {
        // Arrange: Initializes objects and sets the value of the data that is passed to the method under test.
        var service = CreateUserService();
        var users = SetupUsers();

        // Act: Invokes the method under test with the arranged parameters.
        var result = await service.GetUsersAsync(null, CancellationToken.None);

        // Assert: Verifies that the action of the method under test behaves as expected.
        result.Should().BeEquivalentTo(users);
    }

    private IReadOnlyList<UserDataModel> SetupUsers(string forename = "Johnny", string surname = "User", string email = "juser@example.com", bool isActive = true)
    {
        var users = new[]
        {
            new UserDataModel(1, forename, surname, new DateOnly(2000, 1, 2), email, isActive)
        };

        _dataContext
            .Setup(s => s.GetUsersAsync(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(users);

        return users;
    }
}
