using System;

namespace UserManagement.Web.Models;

public class UserListViewModel
{
    public List<UserListItemViewModel> Items { get; set; } = [];
    public bool? IsActive { get; set; }
}

public record UserListItemViewModel(long Id, string Forename, string Surname,
                                    string Email, DateOnly DateOfBirth, bool IsActive);
