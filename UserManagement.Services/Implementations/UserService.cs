using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UserManagement.Data.Models;
using UserManagement.Data.Services;
using UserManagement.Services.Interfaces;

namespace UserManagement.Services.Implementations;

public class UserService(IUserManagementDataService dataAccess, TimeProvider timeProvider) : IUserService
{
    private readonly IUserManagementDataService _dataAccess = dataAccess;
    private readonly TimeProvider _timeProvider = timeProvider;

    /// <summary>
    /// Return users by active state
    /// </summary>
    /// <param name="isActive"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<IEnumerable<UserDataModel>> GetUsersAsync(bool? isActive, CancellationToken cancellationToken = default)
        => await _dataAccess.GetUsersAsync(isActive, cancellationToken);

    public async Task<UserDataModel?> GetUserByIdAsync(long id, CancellationToken cancellationToken = default)
        => await _dataAccess.GetUserByIdAsync(id, cancellationToken);

    public Task<bool> IsEmailAvailableAsync(string email, CancellationToken cancellationToken = default)
        => IsEmailAvailableAsync(email, excludingUserId: null, cancellationToken);

    public async Task<bool> IsEmailAvailableAsync(string email, long? excludingUserId, CancellationToken cancellationToken = default)
        => await IsEmailAvailableForUserAsync(email, excludingUserId, cancellationToken);

    private async Task<bool> IsEmailAvailableForUserAsync(string email, long? excludingUserId, CancellationToken cancellationToken = default)
    {
        var normalized = email.Trim();
        var exists = excludingUserId.HasValue
            ? await _dataAccess.EmailExistsAsync(normalized, excludingUserId.Value, cancellationToken)
            : await _dataAccess.EmailExistsAsync(normalized, cancellationToken);

        return !exists;
    }

    public async Task<long> CreateUserAsync(string forename, string surname, DateOnly dateOfBirth, string email, bool isActive, CancellationToken cancellationToken = default)
    {
        var trimmedForename = forename.Trim();
        var trimmedSurname = surname.Trim();
        var trimmedEmail = email.Trim();

        if (string.IsNullOrWhiteSpace(trimmedForename))
        {
            throw new ArgumentException("Forename is required.", nameof(forename));
        }

        if (string.IsNullOrWhiteSpace(trimmedSurname))
        {
            throw new ArgumentException("Surname is required.", nameof(surname));
        }

        if (string.IsNullOrWhiteSpace(trimmedEmail))
        {
            throw new ArgumentException("Email is required.", nameof(email));
        }

        if (dateOfBirth >= DateOnly.FromDateTime(_timeProvider.GetUtcNow().DateTime))
        {
            throw new ArgumentOutOfRangeException(nameof(dateOfBirth), "Date of birth must be in the past.");
        }

        if (!await IsEmailAvailableAsync(trimmedEmail, cancellationToken: cancellationToken))
        {
            throw new ArgumentException("A user with this email already exists.", nameof(email));
        }

        return await _dataAccess.CreateUserAsync(trimmedForename, trimmedSurname, dateOfBirth, trimmedEmail, isActive, cancellationToken);
    }

    public async Task UpdateUserAsync(long id, string forename, string surname, DateOnly dateOfBirth, string email, bool isActive, CancellationToken cancellationToken = default)
    {
        var trimmedForename = forename.Trim();
        var trimmedSurname = surname.Trim();
        var trimmedEmail = email.Trim();

        ValidateUser(trimmedForename, trimmedSurname, dateOfBirth, trimmedEmail);

        if (!await IsEmailAvailableAsync(trimmedEmail, id, cancellationToken))
        {
            throw new ArgumentException("A user with this email already exists.", nameof(email));
        }

        if (!await _dataAccess.UpdateUserAsync(id, trimmedForename, trimmedSurname, dateOfBirth, trimmedEmail, isActive, cancellationToken))
        {
            throw new KeyNotFoundException($"User with id {id} was not found.");
        }
    }

    public Task<bool> DeleteUserAsync(long id, CancellationToken cancellationToken = default)
        => _dataAccess.DeleteUserAsync(id, cancellationToken);

    public async Task LogUserActionAsync(long userId, string userName, string action, string details, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(action))
        {
            throw new ArgumentException("Action is required.", nameof(action));
        }

        if (string.IsNullOrWhiteSpace(details))
        {
            throw new ArgumentException("Details are required.", nameof(details));
        }

        await _dataAccess.CreateUserActionLogAsync(
            userId,
            userName,
            action,
            details,
            _timeProvider.GetUtcNow(),
            cancellationToken);
    }

    public async Task<IEnumerable<UserActionLogDataModel>> GetUserActionLogsAsync(long userId, CancellationToken cancellationToken = default)
        => await _dataAccess.GetUserActionLogsAsync(userId, cancellationToken);

    public async Task<IReadOnlyList<UserActionLogDataModel>> GetUserActionLogsAsync(int page, int pageSize, CancellationToken cancellationToken = default)
        => await _dataAccess.GetUserActionLogsAsync(page, pageSize, cancellationToken);

    public Task<int> GetUserActionLogCountAsync(CancellationToken cancellationToken = default)
        => _dataAccess.GetUserActionLogCountAsync(cancellationToken);

    public Task<UserActionLogDataModel?> GetUserActionLogByIdAsync(long id, CancellationToken cancellationToken = default)
        => _dataAccess.GetUserActionLogByIdAsync(id, cancellationToken);

    private void ValidateUser(string forename, string surname, DateOnly dateOfBirth, string email)
    {
        if (string.IsNullOrWhiteSpace(forename))
        {
            throw new ArgumentException("Forename is required.", nameof(forename));
        }

        if (string.IsNullOrWhiteSpace(surname))
        {
            throw new ArgumentException("Surname is required.", nameof(surname));
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Email is required.", nameof(email));
        }

        if (dateOfBirth >= DateOnly.FromDateTime(_timeProvider.GetUtcNow().DateTime))
        {
            throw new ArgumentOutOfRangeException(nameof(dateOfBirth), "Date of birth must be in the past.");
        }
    }
}
