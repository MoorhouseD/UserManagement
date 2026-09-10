using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UserManagement.Data.Models;

namespace UserManagement.Services.Interfaces;

public interface IUserService
{
    /// <summary>
    /// Return users by active state
    /// </summary>
    /// <param name="isActive"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IEnumerable<UserDataModel>> GetUsersAsync(bool? isActive, CancellationToken cancellationToken = default);
}
