using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UserManagement.Data.Entities;
using UserManagement.Services.Interfaces;
using UserManagement.Web.Models;

namespace UserManagement.Web.Controllers;

[Route("users")]
public class UsersController(IUserService userService) : Controller
{
    private readonly IUserService _userService = userService;

    [HttpGet("List")]
    public async Task<ViewResult> List(bool? isActive = null, CancellationToken cancellationToken = default)
    {
        IEnumerable<User> users;

        users = await _userService.GetUsersAsync(isActive, cancellationToken);

        var items = users.Select(p => new UserListItemViewModel(
            Id: p.Id,
            Forename: p.Forename,
            Surname: p.Surname,
            Email: p.Email,
            DateOfBirth: p.DateOfBirth,
            IsActive: p.IsActive
        ));

        var model = new UserListViewModel
        {
            Items = [.. items],
            IsActive = isActive
        };

        return View(model);
    }
}
