using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UserManagement.Data;
using UserManagement.Data.Entities;
using UserManagement.Services.Interfaces;

namespace UserManagement.Services.Implementations;

public class UserService(IDataContext dataAccess) : IUserService
{
    private readonly IDataContext _dataAccess = dataAccess;

    /// <summary>
    /// Return users by active state
    /// </summary>
    /// <param name="isActive"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<IEnumerable<User>> GetUsersAsync(bool? isActive, CancellationToken cancellationToken = default)
    {
        var userQuery = _dataAccess.GetAll<User>();

        if (isActive.HasValue)
        {
            userQuery = userQuery.Where(p => p.IsActive == isActive.Value);
        }

        return userQuery.ToList();
    }
}
