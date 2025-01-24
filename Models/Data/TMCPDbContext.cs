using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace TMCP.Models.Data;

public class TMCPDbContext : IdentityDbContext<User>
{
    public TMCPDbContext(DbContextOptions<TMCPDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        ////Seed roles
        //builder.Entity<IdentityRole>().HasData(
        //    new IdentityRole { Id= "1", Name = "SuperAdmin", NormalizedName = "SUPERADMIN"},
        //    new IdentityRole { Id = "2", Name = "Admin", NormalizedName = "ADMIN"},
        //    new IdentityRole { Id = "3", Name = "TeamManager", NormalizedName = "TEAMMANAGER" },
        //    new IdentityRole { Id = "4", Name = "Member", NormalizedName = "MEMBER" }

        //    );

        //var hasher = new PasswordHasher<User>();
        //var superAdmin = new User
        //{
        //    Id = "1",
        //    UserName = "superadmin@tcmp.com",
        //    Email = "superadmin@tcmp.com",
        //    NormalizedEmail = "SUPERADMIN@TCMP.COM",
        //    NormalizedUserName = "SUPERADMIN@TCMP.COM",
        //    EmailConfirmed = true,
        //    PasswordHash = hasher.HashPassword(null, "SuperAdmin@123")
        //};

        //builder.Entity<User>().HasData(superAdmin );

        //builder.Entity<IdentityUserRole<string>>().HasData(
        //    new IdentityUserRole<string> { RoleId = "1", UserId = "1" });

    }
}
