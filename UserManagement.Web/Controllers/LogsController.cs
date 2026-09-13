using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UserManagement.Services.Interfaces;
using UserManagement.Web.Models;

namespace UserManagement.Web.Controllers;

public class LogsController(IUserService userService) : Controller
{
    private const int _pageSize = 20;
    private readonly IUserService _userService = userService;

    [HttpGet("logs")]
    public async Task<IActionResult> Index(int page = 1, CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        var logs = await _userService.GetUserActionLogsAsync(page, _pageSize, cancellationToken);
        var totalCount = await _userService.GetUserActionLogCountAsync(cancellationToken);

        return View(new LogsListViewModel
        {
            Items = logs.Select(UserActionLogViewModel.FromData).ToList(),
            Page = page,
            PageSize = _pageSize,
            TotalCount = totalCount
        });
    }

    [HttpGet("logs/{id:long}")]
    public async Task<IActionResult> Details(long id, CancellationToken cancellationToken = default)
    {
        var log = await _userService.GetUserActionLogByIdAsync(id, cancellationToken);
        if (log is null)
        {
            return NotFound();
        }

        return View(UserActionLogViewModel.FromData(log));
    }
}