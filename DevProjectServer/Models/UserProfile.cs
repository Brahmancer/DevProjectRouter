using Microsoft.AspNetCore.Identity;
namespace DevProjectServer.Models;

public class UserProfile : IdentityUser
{
    // public int Id { get; set; }
    public string? GithubUsername { get; set; }
    public string? GitHubAccessToken { get; set; }
    public DateTime? GitHubTokenExpiresAt { get; set; }
    
    // Password1!
}