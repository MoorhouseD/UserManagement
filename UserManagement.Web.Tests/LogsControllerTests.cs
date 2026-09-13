using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using UserManagement.Data.Models;
using UserManagement.Services.Interfaces;
using UserManagement.Web.Controllers;
using UserManagement.Web.Models;

namespace UserManagement.Web.Tests;

public class LogsControllerTests
{
    private readonly Mock<IUserService> _userService = new();
    private readonly LogsController _controller;

    public LogsControllerTests()
    {
        _controller = new LogsController(_userService.Object);
    }

    [Fact]
    public async Task Index_ReturnsPagedLogsModel()
    {
        var logs = new[]
        {
            new UserActionLogDataModel(1, 7, "John Smith", "Viewed", "User details viewed.", DateTimeOffset.UtcNow)
        };
        _userService.Setup(s => s.GetUserActionLogsAsync(2, 20, It.IsAny<CancellationToken>())).ReturnsAsync(logs);
        _userService.Setup(s => s.GetUserActionLogCountAsync(It.IsAny<CancellationToken>())).ReturnsAsync(21);

        var result = await _controller.Index(2, CancellationToken.None);

        var model = result.Should().BeOfType<ViewResult>().Subject.Model.Should().BeOfType<LogsListViewModel>().Subject;
        model.Page.Should().Be(2);
        model.TotalPages.Should().Be(2);
        model.Items.Should().ContainSingle().Which.Action.Should().Be("Viewed");
    }

    [Fact]
    public async Task Details_WhenLogDoesNotExist_ReturnsNotFound()
    {
        _userService.Setup(s => s.GetUserActionLogByIdAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync((UserActionLogDataModel?)null);

        var result = await _controller.Details(99, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }
}