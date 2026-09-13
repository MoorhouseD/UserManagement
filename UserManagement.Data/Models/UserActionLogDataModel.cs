using System;

namespace UserManagement.Data.Models;

public record UserActionLogDataModel(
    long Id,
    long UserId,
    string UserName,
    string Action,
    string Details,
    DateTimeOffset OccurredAtUtc);