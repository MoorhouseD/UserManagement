using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UserManagement.Data.Models;

namespace UserManagement.Data.Services;

public class UserManagementDataService(DataContext dataContext) : IUserManagementDataService
{
    private readonly DataContext _dataContext = dataContext;

    public async Task<IReadOnlyList<UserDataModel>> GetUsersAsync(bool? isActive, CancellationToken cancellationToken = default)
    {
        var users = _dataContext.Users
            .AsNoTracking()
            .AsQueryable();

        if (isActive.HasValue)
        {
            users = users.Where(user => user.IsActive == isActive.Value);
        }

        return await users
            .Select(user => new UserDataModel(
                user.Id,
                user.Forename,
                user.Surname,
                user.DateOfBirth,
                user.Email,
                user.IsActive))
            .ToListAsync(cancellationToken);
    }
}
