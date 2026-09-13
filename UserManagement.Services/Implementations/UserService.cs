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

    public async Task<bool> IsEmailAvailableAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalized = email.Trim();
        return !await _dataAccess.EmailExistsAsync(normalized, cancellationToken);
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

        if (!await IsEmailAvailableAsync(trimmedEmail, cancellationToken))
        {
            throw new ArgumentException("A user with this email already exists.", nameof(email));
        }

        return await _dataAccess.CreateUserAsync(trimmedForename, trimmedSurname, dateOfBirth, trimmedEmail, isActive, cancellationToken);
    }
}
