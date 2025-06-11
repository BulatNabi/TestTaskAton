using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TestTaskAton.Models;
namespace TestTaskAton.Data;
public class ApplicationDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        Guid adminRoleId = Guid.Parse("C100F2E2-1A7E-473B-9A74-1B1E37B24773"); 
        Guid userRoleId = Guid.Parse("A100F2E2-1A7E-473B-9A74-1B1E37B24771");
        List<IdentityRole<Guid>> roles = new List<IdentityRole<Guid>>
        {
            new IdentityRole<Guid>
            {
                Id = adminRoleId, 
                Name = "Admin",
                NormalizedName = "ADMIN"
            },
            new IdentityRole<Guid>
            {
                Id = userRoleId, 
                Name = "User",
                NormalizedName = "USER"
            }
        };
        modelBuilder.Entity<IdentityRole<Guid>>().HasData(roles);
    }
}