using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UserManagement.Data.Entities;
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

    public async Task<UserDataModel?> GetUserByIdAsync(long id, CancellationToken cancellationToken = default)
        => await _dataContext.Users
            .AsNoTracking()
            .Where(user => user.Id == id)
            .Select(user => new UserDataModel(
                user.Id,
                user.Forename,
                user.Surname,
                user.DateOfBirth,
                user.Email,
                user.IsActive))
            .SingleOrDefaultAsync(cancellationToken);

    public Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
        => IsEmailInUseAsync(email, cancellationToken: cancellationToken);

    public async Task<bool> EmailExistsAsync(string email, long? excludingUserId, CancellationToken cancellationToken = default)
        => await IsEmailInUseAsync(email, excludingUserId, cancellationToken);

    private async Task<bool> IsEmailInUseAsync(string email, long? excludingUserId = null, CancellationToken cancellationToken = default)
    {
        var normalized = email.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return false;
        }

        var users = _dataContext.Users
            .AsNoTracking()
            .Where(user => user.NormalizedEmail == normalized.ToUpperInvariant());

        if (excludingUserId.HasValue)
        {
            users = users.Where(user => user.Id != excludingUserId.Value);
        }

        return await users.AnyAsync(cancellationToken);
    }

    public async Task<long> CreateUserAsync(string forename, string surname, DateOnly dateOfBirth, string email, bool isActive, CancellationToken cancellationToken = default)
    {
        var entity = new User
        {
            Forename = forename.Trim(),
            Surname = surname.Trim(),
            DateOfBirth = dateOfBirth,
            Email = email.Trim(),
            NormalizedEmail = email.Trim().ToUpperInvariant(),
            IsActive = isActive
        };

        _dataContext.Users.Add(entity);
        await _dataContext.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }

    public async Task<bool> UpdateUserAsync(long id, string forename, string surname, DateOnly dateOfBirth, string email, bool isActive, CancellationToken cancellationToken = default)
    {
        var entity = await _dataContext.Users.SingleOrDefaultAsync(user => user.Id == id, cancellationToken);
        if (entity is null)
        {
            return false;
        }

        entity.Forename = forename.Trim();
        entity.Surname = surname.Trim();
        entity.DateOfBirth = dateOfBirth;
        entity.Email = email.Trim();
        entity.NormalizedEmail = email.Trim().ToUpperInvariant();
        entity.IsActive = isActive;

        await _dataContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteUserAsync(long id, CancellationToken cancellationToken = default)
    {
        var entity = await _dataContext.Users.SingleOrDefaultAsync(user => user.Id == id, cancellationToken);
        if (entity is null)
        {
            return false;
        }

        _dataContext.Users.Remove(entity);
        await _dataContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}
