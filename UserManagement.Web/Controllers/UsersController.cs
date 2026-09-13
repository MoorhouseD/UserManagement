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
            var userId = await _userService.CreateUserAsync(
                model.Forename,
                model.Surname,
                model.DateOfBirth,
                model.Email,
                model.IsActive,
                cancellationToken);

            await _userService.LogUserActionAsync(
                userId,
                $"{model.Forename.Trim()} {model.Surname.Trim()}",
                "Created",
                "User account created.",
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

    [HttpGet("edit/{id:long}")]
    public async Task<IActionResult> Edit(long id, CancellationToken cancellationToken)
    {
        var user = await _userService.GetUserByIdAsync(id, cancellationToken);
        if (user is null)
        {
            return NotFound();
        }

        return View(new EditUserViewModel
        {
            Id = user.Id,
            Forename = user.Forename,
            Surname = user.Surname,
            DateOfBirth = user.DateOfBirth,
            Email = user.Email,
            IsActive = user.IsActive
        });
    }

    [HttpPost("edit/{id:long}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(long id, EditUserViewModel model, CancellationToken cancellationToken)
    {
        model.Id = id;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (!await _userService.IsEmailAvailableAsync(model.Email, id, cancellationToken))
        {
            ModelState.AddModelError(nameof(EditUserViewModel.Email), "A user with this email already exists.");
            return View(model);
        }

        try
        {
            await _userService.UpdateUserAsync(
                id,
                model.Forename,
                model.Surname,
                model.DateOfBirth,
                model.Email,
                model.IsActive,
                cancellationToken);

            await _userService.LogUserActionAsync(
                id,
                $"{model.Forename.Trim()} {model.Surname.Trim()}",
                "Updated",
                "User account updated.",
                cancellationToken);

            TempData["SuccessMessage"] = "User updated successfully.";
            return RedirectToAction(nameof(List));
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(ex.ParamName ?? nameof(EditUserViewModel.Email), ex.Message);
            return View(model);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost("delete/{id:long}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(long id, CancellationToken cancellationToken)
    {
        if (!await _userService.DeleteUserAsync(id, cancellationToken))
        {
            return NotFound();
        }

        await _userService.LogUserActionAsync(
            id,
            $"User #{id}",
            "Deleted",
            "User account deleted.",
            cancellationToken);

        TempData["SuccessMessage"] = "User deleted successfully.";
        return RedirectToAction(nameof(List));
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<UserDetailsViewModel>> Details(long id, CancellationToken cancellationToken)
    {
        var user = await _userService.GetUserByIdAsync(id, cancellationToken);

        if (user is null)
        {
            return NotFound();
        }

        await _userService.LogUserActionAsync(
            user.Id,
            $"{user.Forename} {user.Surname}",
            "Viewed",
            "User details viewed.",
            cancellationToken);

        var model = new UserDetailsViewModel(
            Id: user.Id,
            Forename: user.Forename,
            Surname: user.Surname,
            DateOfBirth: user.DateOfBirth,
            Email: user.Email,
            IsActive: user.IsActive)
        {
            ActionLogs = (await _userService.GetUserActionLogsAsync(id, cancellationToken))
                .Select(UserActionLogViewModel.FromData)
                .ToList()
        };

        return View(model);
    }
}
