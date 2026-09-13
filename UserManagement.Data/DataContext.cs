using System;
using Microsoft.EntityFrameworkCore;
using UserManagement.Data.Entities;

namespace UserManagement.Data;

public class DataContext(DbContextOptions<DataContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    public DbSet<UserActionLog> UserActionLogs => Set<UserActionLog>();

    protected override void OnModelCreating(ModelBuilder model)
    {
        model.Entity<User>()
            .HasIndex(user => user.NormalizedEmail)
            .IsUnique();

        model.Entity<User>().HasData(
        [
            new User { Id = 1, Forename = "Peter", Surname = "Loew", DateOfBirth = new DateOnly(1978, 4, 12), Email = "ploew@example.com", NormalizedEmail = "PLOEW@EXAMPLE.COM", IsActive = true },
            new User { Id = 2, Forename = "Benjamin Franklin", Surname = "Gates", DateOfBirth = new DateOnly(1965, 10, 28), Email = "bfgates@example.com", NormalizedEmail = "BFGATES@EXAMPLE.COM", IsActive = true },
            new User { Id = 3, Forename = "Castor", Surname = "Troy", DateOfBirth = new DateOnly(1989, 2, 7), Email = "ctroy@example.com", NormalizedEmail = "CTROY@EXAMPLE.COM", IsActive = false },
            new User { Id = 4, Forename = "Memphis", Surname = "Raines", DateOfBirth = new DateOnly(1972, 8, 19), Email = "mraines@example.com", NormalizedEmail = "MRAINES@EXAMPLE.COM", IsActive = true },
            new User { Id = 5, Forename = "Stanley", Surname = "Goodspeed", DateOfBirth = new DateOnly(1958, 12, 3), Email = "sgodspeed@example.com", NormalizedEmail = "SGODSPEED@EXAMPLE.COM", IsActive = true },
            new User { Id = 6, Forename = "H.I.", Surname = "McDunnough", DateOfBirth = new DateOnly(1981, 6, 25), Email = "himcdunnough@example.com", NormalizedEmail = "HIMCDUNNOUGH@EXAMPLE.COM", IsActive = true },
            new User { Id = 7, Forename = "Cameron", Surname = "Poe", DateOfBirth = new DateOnly(1992, 1, 16), Email = "cpoe@example.com", NormalizedEmail = "CPOE@EXAMPLE.COM", IsActive = false },
            new User { Id = 8, Forename = "Edward", Surname = "Malus", DateOfBirth = new DateOnly(1986, 9, 30), Email = "emalus@example.com", NormalizedEmail = "EMALUS@EXAMPLE.COM", IsActive = false },
            new User { Id = 9, Forename = "Damon", Surname = "Macready", DateOfBirth = new DateOnly(1975, 3, 21), Email = "dmacready@example.com", NormalizedEmail = "DMACREADY@EXAMPLE.COM", IsActive = false },
            new User { Id = 10, Forename = "Johnny", Surname = "Blaze", DateOfBirth = new DateOnly(1990, 11, 8), Email = "jblaze@example.com", NormalizedEmail = "JBLAZE@EXAMPLE.COM", IsActive = true },
            new User { Id = 11, Forename = "Robin", Surname = "Feld", DateOfBirth = new DateOnly(1983, 7, 14), Email = "rfeld@example.com", NormalizedEmail = "RFELD@EXAMPLE.COM", IsActive = true },
        ]);
    }
}
