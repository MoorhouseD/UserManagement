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

    public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalized = email.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return false;
        }

        return await _dataContext.Users
            .AsNoTracking()
            .AnyAsync(user => user.NormalizedEmail == normalized.ToUpperInvariant(), cancellationToken);
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
}
