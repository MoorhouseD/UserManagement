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
    Task<bool> EmailExistsAsync(string email, long? excludingUserId, CancellationToken cancellationToken = default);
    Task<long> CreateUserAsync(string forename, string surname, DateOnly dateOfBirth, string email, bool isActive, CancellationToken cancellationToken = default);
    Task<bool> UpdateUserAsync(long id, string forename, string surname, DateOnly dateOfBirth, string email, bool isActive, CancellationToken cancellationToken = default);
    Task<bool> DeleteUserAsync(long id, CancellationToken cancellationToken = default);
    Task<UserActionLogDataModel> CreateUserActionLogAsync(long userId, string userName, string action, string details, DateTimeOffset occurredAtUtc, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserActionLogDataModel>> GetUserActionLogsAsync(long userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserActionLogDataModel>> GetUserActionLogsAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> GetUserActionLogCountAsync(CancellationToken cancellationToken = default);
    Task<UserActionLogDataModel?> GetUserActionLogByIdAsync(long id, CancellationToken cancellationToken = default);
}
