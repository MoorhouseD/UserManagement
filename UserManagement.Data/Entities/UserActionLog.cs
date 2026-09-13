using System;
using System.ComponentModel.DataAnnotations;

namespace UserManagement.Data.Entities;

public class UserActionLog
{
    [Key]
    public long Id { get; set; }

    public long UserId { get; set; }

    [Required, StringLength(200)]
    public string UserName { get; set; } = default!;

    [Required, StringLength(50)]
    public string Action { get; set; } = default!;

    [Required, StringLength(1000)]
    public string Details { get; set; } = default!;

    public DateTimeOffset OccurredAtUtc { get; set; }
}