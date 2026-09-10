using System;

namespace UserManagement.Data.Models;

public record UserDataModel(
    long Id,
    string Forename,
    string Surname,
    DateOnly DateOfBirth,
    string Email,
    bool IsActive);
