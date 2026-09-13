using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using UserManagement.Data.Models;
using UserManagement.Services.Interfaces;
using UserManagement.Web.Models;
using UserManagement.Web.Controllers;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace UserManagement.Web.Tests;

public class UserControllerTests
{
    private readonly Mock<IUserService> _userService = new();
    private readonly UsersController _controller;

    public UserControllerTests()
    {
        _controller = new UsersController(_userService.Object);
        _controller.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
    }

    [Fact]
    public async Task List_WhenServiceReturnsUsers_ModelMustContainUsers()
    {
        // Arrange: Initializes objects and sets the value of the data that is passed to the method under test.
        var users = CreateUsers();

        _userService
            .Setup(s => s.GetUsersAsync(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(users);

        // Act: Invokes the method under test with the arranged parameters.
        var result = await _controller.List(cancellationToken: CancellationToken.None);

        // Assert: Verifies that the action of the method under test behaves as expected.
        var model = result.Model.Should().BeOfType<UserListViewModel>().Subject;
        model.IsActive.Should().BeNull();
        model.Items.Should().BeEquivalentTo(users);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task List_WhenFilterProvided_ReturnsOnlyRequestedState(bool isActive)
    {
        var users = CreateUsers("Test", "User", "test@example.com",
            new DateOnly(1990, 1, 1), isActive);

        var expectedUsers = users.Where(x => x.IsActive == isActive);

        _userService
            .Setup(s => s.GetUsersAsync(isActive, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedUsers);

        var result = await _controller.List(isActive, CancellationToken.None);

        var model = result.Model.Should().BeOfType<UserListViewModel>().Subject;
        model.IsActive.Should().Be(isActive);
        model.Items.Should().OnlyContain(x => x.IsActive == isActive);
    }

    [Fact]
    public async Task Details_WhenUserExists_ReturnsMappedModel()
    {
        var user = new UserDataModel(7, "John", "Smith", new DateOnly(1990, 1, 1), "john@example.com", true);
        _userService
            .Setup(s => s.GetUserByIdAsync(7, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _controller.Details(7, CancellationToken.None);

        var view = result.Result.Should().BeOfType<ViewResult>().Subject;
        var model = view.Model.Should().BeOfType<UserDetailsViewModel>().Subject;
        model.Id.Should().Be(7);
        model.Forename.Should().Be("John");
        model.Email.Should().Be("john@example.com");
        model.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Details_WhenUserDoesNotExist_ReturnsNotFound()
    {
        _userService
            .Setup(s => s.GetUserByIdAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserDataModel?)null);

        var result = await _controller.Details(99, CancellationToken.None);

        result.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task EditGet_WhenUserExists_ReturnsMappedModel()
    {
        var user = new UserDataModel(7, "John", "Smith", new DateOnly(1990, 1, 1), "john@example.com", true);
        _userService
            .Setup(s => s.GetUserByIdAsync(7, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _controller.Edit(7, CancellationToken.None);

        var model = result.Should().BeOfType<ViewResult>().Subject.Model.Should().BeOfType<EditUserViewModel>().Subject;
        model.Id.Should().Be(7);
        model.Forename.Should().Be("John");
        model.Email.Should().Be("john@example.com");
    }

    [Fact]
    public async Task EditPost_WhenModelIsValid_UpdatesAndRedirects()
    {
        _userService
            .Setup(s => s.IsEmailAvailableAsync("john@example.com", 7, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var model = new EditUserViewModel
        {
            Forename = "John",
            Surname = "Smith",
            DateOfBirth = new DateOnly(1990, 1, 1),
            Email = "john@example.com",
            IsActive = true
        };

        var result = await _controller.Edit(7, model, CancellationToken.None);

        result.Should().BeOfType<RedirectToActionResult>().Which.ActionName.Should().Be(nameof(UsersController.List));
        _userService.Verify(s => s.UpdateUserAsync(7, "John", "Smith", new DateOnly(1990, 1, 1), "john@example.com", true, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Delete_WhenUserExists_DeletesAndRedirects()
    {
        _userService
            .Setup(s => s.DeleteUserAsync(7, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _controller.Delete(7, CancellationToken.None);

        result.Should().BeOfType<RedirectToActionResult>().Which.ActionName.Should().Be(nameof(UsersController.List));
        _userService.Verify(s => s.DeleteUserAsync(7, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Delete_WhenUserDoesNotExist_ReturnsNotFound()
    {
        _userService
            .Setup(s => s.DeleteUserAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _controller.Delete(99, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    private static IEnumerable<UserDataModel> CreateUsers(string forename = "Johnny", string surname = "User", string email = "juser@example.com", DateOnly? dateOfBirth = null, bool isActive = true)
    {
        var users = new[]
        {
            new UserDataModel(1, forename, surname, dateOfBirth ?? new DateOnly(2000, 1, 2), email, isActive),
            //Inverse of the first user to test filtering
            new UserDataModel(2, surname, forename, dateOfBirth ?? new DateOnly(2000, 2, 1), email, !isActive)
        };

        return users;
    }
}
