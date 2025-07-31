using DevProjectServer.Data;
using DevProjectServer.Models;
using DevProjectServer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DevProjectServer.Controllers;

[Authorize]
public class GitHubController : Controller
{
    private readonly UserManager<UserProfile> _userManager;
    private readonly ApplicationDbContext _dbContext;
    private readonly IGitHubService _gitHubService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<GitHubController> _logger;

    public GitHubController(
        UserManager<UserProfile> userManager,
        ApplicationDbContext dbContext,
        IGitHubService gitHubService,
        IConfiguration configuration,
        ILogger<GitHubController> logger)
    {
        _userManager = userManager;
        _dbContext = dbContext;
        _gitHubService = gitHubService;
        _configuration = configuration;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Connect()
    {
        var clientId = _configuration["GitHub:ClientId"];
        if (string.IsNullOrEmpty(clientId))
        {
            return View("Error", "GitHub OAuth is not configured. Please set GitHub:ClientId in appsettings.json");
        }

        var redirectUri = Url.Action("Callback", "GitHub", null, Request.Scheme, Request.Host.Value) ?? "";
        var githubAuthUrl = $"https://github.com/login/oauth/authorize?client_id={clientId}&redirect_uri={Uri.EscapeDataString(redirectUri)}&scope=repo,read:user";
        
        return Redirect(githubAuthUrl);
    }

    [HttpGet]
    public async Task<IActionResult> Callback(string code)
    {
        if (string.IsNullOrEmpty(code))
        {
            return RedirectToAction("Index", "Home");
        }

        try
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return NotFound();
            }

            // Exchange code for access token
            var accessToken = await ExchangeCodeForTokenAsync(code);
            if (string.IsNullOrEmpty(accessToken))
            {
                return View("Error", "Failed to authenticate with GitHub");
            }

            // Get user info from GitHub
            var githubUser = await GetGitHubUserAsync(accessToken);
            if (githubUser == null)
            {
                return View("Error", "Failed to get GitHub user information");
            }

            // Update user profile
            currentUser.GitHubAccessToken = accessToken;
            currentUser.GithubUsername = githubUser.Login;
            currentUser.GitHubTokenExpiresAt = DateTime.UtcNow.AddDays(60); // GitHub tokens typically last 60 days

            await _userManager.UpdateAsync(currentUser);

            TempData["SuccessMessage"] = $"Successfully connected to GitHub as {githubUser.Login}";
            return RedirectToAction("Index", "Home");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during GitHub OAuth callback");
            return View("Error", "An error occurred during GitHub authentication");
        }
    }

    [HttpPost]
    public async Task<IActionResult> SyncRepositories()
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return Json(new { success = false, message = "User not found" });
        }

        if (string.IsNullOrEmpty(currentUser.GitHubAccessToken))
        {
            return Json(new { success = false, message = "GitHub not connected. Please connect your GitHub account first." });
        }

        try
        {
            var repositories = await _gitHubService.GetUserRepositoriesAsync(currentUser.GitHubAccessToken);
            
            // Clear existing projects for this user
            var existingProjects = await _dbContext.GitProjects
                .Where(p => p.UserProfileId == currentUser.Id)
                .ToListAsync();
            
            _dbContext.GitProjects.RemoveRange(existingProjects);

            // Add new projects from GitHub
            foreach (var repo in repositories.Where(r => !r.Private))
            {
                var gitProject = new GitProject
                {
                    Name = repo.Name,
                    RepoUrl = repo.HtmlUrl,
                    Description = repo.Description ?? "",
                    UserProfileId = currentUser.Id
                };

                _dbContext.GitProjects.Add(gitProject);
            }

            await _dbContext.SaveChangesAsync();

            return Json(new { success = true, message = $"Synced {repositories.Count(r => !r.Private)} repositories from GitHub" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing repositories for user {UserId}", currentUser.Id);
            return Json(new { success = false, message = "Failed to sync repositories from GitHub" });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetReadme(int projectId)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return Json(new { success = false, message = "User not found" });
        }

        var project = await _dbContext.GitProjects
            .FirstOrDefaultAsync(p => p.Id == projectId && p.UserProfileId == currentUser.Id);

        if (project == null)
        {
            return Json(new { success = false, message = "Project not found" });
        }

        if (string.IsNullOrEmpty(currentUser.GitHubAccessToken))
        {
            return Json(new { success = false, message = "GitHub not connected" });
        }

        try
        {
            // Extract owner and repo from URL
            var uri = new Uri(project.RepoUrl);
            var segments = uri.Segments;
            var owner = segments[1].TrimEnd('/');
            var repo = segments[2].TrimEnd('/');

            var readmeContent = await _gitHubService.GetReadmeContentAsync(currentUser.GitHubAccessToken, owner, repo);
            
            if (readmeContent != null)
            {
                // Convert markdown to HTML (you might want to use a library like Markdig)
                var htmlContent = ConvertMarkdownToHtml(readmeContent);
                
                // Update the cached version
                project.ReadmeHtml = htmlContent;
                await _dbContext.SaveChangesAsync();

                return Json(new { success = true, html = htmlContent });
            }

            return Json(new { success = false, message = "No README found for this repository" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching README for project {ProjectId}", projectId);
            return Json(new { success = false, message = "Failed to fetch README" });
        }
    }

    private async Task<string?> ExchangeCodeForTokenAsync(string code)
    {
        var clientId = _configuration["GitHub:ClientId"];
        var clientSecret = _configuration["GitHub:ClientSecret"];
        var redirectUri = Url.Action("Callback", "GitHub", null, Request.Scheme, Request.Host.Value);

        using var httpClient = new HttpClient();
        var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("client_id", clientId ?? ""),
            new KeyValuePair<string, string>("client_secret", clientSecret ?? ""),
            new KeyValuePair<string, string>("code", code ?? ""),
            new KeyValuePair<string, string>("redirect_uri", redirectUri)
        });

        var response = await httpClient.PostAsync("https://github.com/login/oauth/access_token", content);
        var responseContent = await response.Content.ReadAsStringAsync();

        // Parse the response (it's in format: access_token=xxx&token_type=bearer)
        var parameters = responseContent.Split('&')
            .Select(p => p.Split('='))
            .ToDictionary(p => p[0], p => p[1]);

        return parameters.GetValueOrDefault("access_token");
    }

    private async Task<GitHubUser?> GetGitHubUserAsync(string accessToken)
    {
        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("User-Agent", "DevProjectServer");
        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

        var response = await httpClient.GetAsync("https://api.github.com/user");
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            return System.Text.Json.JsonSerializer.Deserialize<GitHubUser>(content, new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }

        return null;
    }

    private string ConvertMarkdownToHtml(string markdown)
    {
        // Simple markdown to HTML conversion for basic formatting
        // In a real application, you'd use a library like Markdig
        var html = markdown
            .Replace("# ", "<h1>").Replace("\n# ", "</h1>\n<h1>")
            .Replace("## ", "<h2>").Replace("\n## ", "</h2>\n<h2>")
            .Replace("### ", "<h3>").Replace("\n### ", "</h3>\n<h3>")
            .Replace("**", "<strong>").Replace("**", "</strong>")
            .Replace("*", "<em>").Replace("*", "</em>")
            .Replace("`", "<code>").Replace("`", "</code>")
            .Replace("\n", "<br>");

        return html;
    }
}

public class GitHubUser
{
    public string Login { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string AvatarUrl { get; set; } = string.Empty;
} 