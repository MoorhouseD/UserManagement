using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UserManagement.Data.Models;

namespace UserManagement.Data.Services;

public class UserManagementDataService(IDataContext dataContext) : IUserManagementDataService
{
    private readonly IDataContext _dataContext = dataContext;

    public Task<IReadOnlyList<UserDataModel>> GetUsersAsync(bool? isActive, CancellationToken cancellationToken = default)
    {
        var users = _dataContext.GetAll<Entities.User>();

        if (isActive.HasValue)
        {
            users = users.Where(user => user.IsActive == isActive.Value);
        }

        var result = users
            .Select(user => new UserDataModel(
                user.Id,
                user.Forename,
                user.Surname,
                user.DateOfBirth,
                user.Email,
                user.IsActive))
            .ToList();

        return Task.FromResult<IReadOnlyList<UserDataModel>>(result);
    }
}
