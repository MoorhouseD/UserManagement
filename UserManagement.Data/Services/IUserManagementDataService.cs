using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UserManagement.Data.Models;

namespace UserManagement.Data.Services;

public interface IUserManagementDataService
{
    Task<IReadOnlyList<UserDataModel>> GetUsersAsync(bool? isActive, CancellationToken cancellationToken = default);
}
