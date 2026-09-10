using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UserManagement.Data.Models;
using UserManagement.Data.Services;
using UserManagement.Services.Interfaces;

namespace UserManagement.Services.Implementations;

public class UserService(IUserManagementDataService dataAccess) : IUserService
{
    private readonly IUserManagementDataService _dataAccess = dataAccess;

    /// <summary>
    /// Return users by active state
    /// </summary>
    /// <param name="isActive"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<IEnumerable<UserDataModel>> GetUsersAsync(bool? isActive, CancellationToken cancellationToken = default)
        => await _dataAccess.GetUsersAsync(isActive, cancellationToken);
}
