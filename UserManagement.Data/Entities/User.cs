using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserManagement.Data.Entities;

public class User
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    [Required, StringLength(100, MinimumLength = 1)]
    public string Forename { get; set; } = default!;

    [Required, StringLength(100, MinimumLength = 1)]
    public string Surname { get; set; } = default!;

    [DataType(DataType.Date)]
    public DateOnly DateOfBirth { get; set; }

    [Required, EmailAddress, StringLength(254, MinimumLength = 3)]
    public string Email { get; set; } = default!;

    [Required, StringLength(254)]
    public string NormalizedEmail { get; set; } = default!;

    public bool IsActive { get; set; }
}
