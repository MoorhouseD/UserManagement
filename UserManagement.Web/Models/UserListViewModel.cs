using System;
using System.Collections.Generic;
using UserManagement.Data.Models;

namespace UserManagement.Web.Models;

public class UserListViewModel
{
    public List<UserListItemViewModel> Items { get; set; } = [];
    public bool? IsActive { get; set; }
}

public record UserListItemViewModel(long Id, string Forename, string Surname,
                                    string Email, DateOnly DateOfBirth, bool IsActive);

public record UserActionLogViewModel(
    long Id,
    long UserId,
    string UserName,
    string Action,
    string Details,
    DateTimeOffset OccurredAtUtc)
{
    public static UserActionLogViewModel FromData(UserActionLogDataModel log)
        => new(log.Id, log.UserId, log.UserName, log.Action, log.Details, log.OccurredAtUtc);
}

public class LogsListViewModel
{
    public IReadOnlyList<UserActionLogViewModel> Items { get; init; } = [];
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}
