using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace DevProjectServer.Models;

public class GitProject
{
    public int Id { get; set; }
    [Required]
    public required string Name { get; set; }
    
    [Required]
    [Url]
    public required string RepoUrl { get; set; }
    public required string Description { get; set; }
    public string? ReadmeHtml { get; set; } // Optionally cache the rendered README
    
    // Foreign key to UserProfile
    public required string UserProfileId { get; set; } // Foreign key
    
    public UserProfile? UserProfile { get; set; } // Navigation property
}