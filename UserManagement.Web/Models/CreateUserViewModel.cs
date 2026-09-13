using System;
using System.ComponentModel.DataAnnotations;

namespace UserManagement.Web.Models;

public class CreateUserViewModel
{
    [Required]
    [StringLength(100)]
    public string Forename { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Surname { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Date)]
    public DateOnly DateOfBirth { get; set; }

    [Required]
    [EmailAddress]
    [StringLength(254)]
    public string Email { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}

public record UserDetailsViewModel(
    long Id,
    string Forename,
    string Surname,
    DateOnly DateOfBirth,
    string Email,
    bool IsActive);
