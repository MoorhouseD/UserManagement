using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UserManagement.Data.Models;

namespace UserManagement.Data.Services;

public interface IUserManagementDataService
{
    Task<IReadOnlyList<UserDataModel>> GetUsersAsync(bool? isActive, CancellationToken cancellationToken = default);
    Task<UserDataModel?> GetUserByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);
    Task<long> CreateUserAsync(string forename, string surname, DateOnly dateOfBirth, string email, bool isActive, CancellationToken cancellationToken = default);
}
