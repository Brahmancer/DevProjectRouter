using DevProjectServer.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DevProjectServer.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : IdentityDbContext<UserProfile>(options)
    {
        // public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<GitProject> GitProjects { get; set; }
    }
};