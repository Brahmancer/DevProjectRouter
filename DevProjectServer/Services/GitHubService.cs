using System.Text.Json;
using DevProjectServer.Models;

namespace DevProjectServer.Services;

public interface IGitHubService
{
    Task<List<GitHubRepository>> GetUserRepositoriesAsync(string accessToken);
    Task<string?> GetReadmeContentAsync(string accessToken, string owner, string repo);
    Task<List<GitHubCommit>> GetRepositoryCommitsAsync(string accessToken, string owner, string repo);
}

public class GitHubService : IGitHubService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<GitHubService> _logger;

    public GitHubService(HttpClient httpClient, ILogger<GitHubService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _httpClient.BaseAddress = new Uri("https://api.github.com/");
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "DevProjectServer");
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3+json");
    }

    public async Task<List<GitHubRepository>> GetUserRepositoriesAsync(string accessToken)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.GetAsync("user/repos?sort=updated&per_page=100");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var repositories = JsonSerializer.Deserialize<List<GitHubRepository>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return repositories ?? new List<GitHubRepository>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching user repositories from GitHub");
            throw;
        }
    }

    public async Task<string?> GetReadmeContentAsync(string accessToken, string owner, string repo)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.GetAsync($"repos/{owner}/{repo}/readme");
            
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null; // No README found
            }

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var readmeResponse = JsonSerializer.Deserialize<GitHubReadmeResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (readmeResponse?.Content != null)
            {
                // Decode base64 content
                var bytes = Convert.FromBase64String(readmeResponse.Content.Replace("\n", ""));
                return System.Text.Encoding.UTF8.GetString(bytes);
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching README content for {Owner}/{Repo}", owner, repo);
            return null;
        }
    }

    public async Task<List<GitHubCommit>> GetRepositoryCommitsAsync(string accessToken, string owner, string repo)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.GetAsync($"repos/{owner}/{repo}/commits?per_page=100");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var commits = JsonSerializer.Deserialize<List<GitHubCommit>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return commits ?? new List<GitHubCommit>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching commits for {Owner}/{Repo}", owner, repo);
            return new List<GitHubCommit>();
        }
    }
}

// GitHub API Response Models
public class GitHubRepository
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string HtmlUrl { get; set; } = string.Empty;
    public string CloneUrl { get; set; } = string.Empty;
    public bool Private { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int StargazersCount { get; set; }
    public int ForksCount { get; set; }
    public string Language { get; set; } = string.Empty;
    public GitHubOwner Owner { get; set; } = new();
}

public class GitHubOwner
{
    public string Login { get; set; } = string.Empty;
    public string AvatarUrl { get; set; } = string.Empty;
}

public class GitHubCommit
{
    public string Sha { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public GitHubCommitAuthor Author { get; set; } = new();
}

public class GitHubCommitAuthor
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime Date { get; set; }
}

public class GitHubReadmeResponse
{
    public string Content { get; set; } = string.Empty;
    public string Encoding { get; set; } = string.Empty;
} 