using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UserManagement.Data.Models;
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
        IEnumerable<UserDataModel> users = await _userService.GetUsersAsync(isActive, cancellationToken);

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

    [HttpGet("create")]
    public IActionResult Create() => View(new CreateUserViewModel { IsActive = true });

    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateUserViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (!await _userService.IsEmailAvailableAsync(model.Email, cancellationToken))
        {
            ModelState.AddModelError(nameof(CreateUserViewModel.Email), "A user with this email already exists.");
            return View(model);
        }

        try
        {
            await _userService.CreateUserAsync(
                model.Forename,
                model.Surname,
                model.DateOfBirth,
                model.Email,
                model.IsActive,
                cancellationToken);

            TempData["SuccessMessage"] = "User created successfully.";
            return RedirectToAction(nameof(List));
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(ex.ParamName ?? nameof(CreateUserViewModel.Email), ex.Message);
            return View(model);
        }
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<UserDetailsViewModel>> Details(long id, CancellationToken cancellationToken)
    {
        var user = await _userService.GetUserByIdAsync(id, cancellationToken);

        if (user is null)
        {
            return NotFound();
        }

        return View(new UserDetailsViewModel(
            Id: user.Id,
            Forename: user.Forename,
            Surname: user.Surname,
            DateOfBirth: user.DateOfBirth,
            Email: user.Email,
            IsActive: user.IsActive));
    }
}
