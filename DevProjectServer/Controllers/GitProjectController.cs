using DevProjectServer.Data;
using DevProjectServer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DevProjectServer.Controllers;

[Authorize]
public class GitProjectController : Controller
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<GitProjectController> _logger;

    public GitProjectController(ApplicationDbContext dbContext, ILogger<GitProjectController> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public IActionResult Create()
    {
        return PartialView("_CreateDialog");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([FromBody] GitProject gitProject)
    {
        if (ModelState.IsValid)
        {
            try
            {
                // Set the current user as the owner
                gitProject.UserProfileId = User.Identity?.Name;
                
                // Add to database
                _dbContext.GitProjects.Add(gitProject);
                await _dbContext.SaveChangesAsync();
                
                _logger.LogInformation("GitProject created successfully for user {UserId}", User.Identity?.Name);
                
                return Json(new { success = true, message = "Project created successfully!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating GitProject for user {UserId}", User.Identity?.Name);
                return Json(new { success = false, message = "An error occurred while creating the project." });
            }
        }

        // Get validation errors for better feedback
        var errors = ModelState.Values
            .SelectMany(v => v.Errors)
            .Select(e => e.ErrorMessage)
            .ToList();
            
        var errorMessage = string.Join(", ", errors);
        _logger.LogWarning("Validation failed for GitProject creation: {Errors}", errorMessage);
            
        return Json(new {success = false, message = "Failed to create a Git Project: Invalid data provided"});
    }
}