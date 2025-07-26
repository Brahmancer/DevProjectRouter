using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace DevProjectServer.Models;

public class GitProject
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string RepoUrl { get; set; }
    public string Description { get; set; }
    public string ReadmeHtml { get; set; } // Optionally cache the rendered README
    
    // Foreign key to UserProfile
    public string UserProfileId { get; set; } // Foreign key
    
    public UserProfile UserProfile { get; set; } // Navigation property
}