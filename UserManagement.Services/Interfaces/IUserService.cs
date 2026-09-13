using System;
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
    Task<UserDataModel?> GetUserByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<bool> IsEmailAvailableAsync(string email, CancellationToken cancellationToken = default);
    Task<long> CreateUserAsync(string forename, string surname, DateOnly dateOfBirth, string email, bool isActive, CancellationToken cancellationToken = default);
}
