using DevProjectServer.Data;
using DevProjectServer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DevProjectServer.Controllers;

[Authorize]
public class GitProjectController(
    ApplicationDbContext dbContext,
    ILogger<GitProjectController> logger,
    UserManager<UserProfile> userManager)
    : Controller
{
    public IActionResult Create()
    {
        return PartialView("_CreateDialog");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([FromBody] GitProject gitProject)
    {
        // Set the current user as the owner
        var currentUser = await userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return NotFound();
        }
        
        // update before the ModelState check otherwise it would fail the ModelState 
        gitProject.UserProfileId = currentUser.Id;
        
        // Ensure that we have a valid object. 
        if (ModelState.IsValid)
        {
            try
            {
                // Add to database
                dbContext.GitProjects.Add(gitProject);
                await dbContext.SaveChangesAsync();
                
                logger.LogInformation("GitProject created successfully for user {UserId}", currentUser.Id);
                
                return Json(new { success = true, message = "Project created successfully!" });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error creating GitProject for user {UserId}", currentUser.Id);
                return Json(new { success = false, message = "An error occurred while creating the project." });
            }
        }

        // Get validation errors for better feedback
        var errors = ModelState.Values
            .SelectMany(v => v.Errors)
            .Select(e => e.ErrorMessage)
            .ToList();
            
        var errorMessage = string.Join(", ", errors);
        logger.LogWarning("Validation failed for GitProject creation: {Errors}", errorMessage);
            
        return Json(new {success = false, message = "Failed to create a Git Project: Invalid data provided"});
    }

    [HttpGet]
    public async Task<IActionResult> GetUserProjects()
    {
        // Set the current user as the owner
        var currentUser = await userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return NotFound();
        }
        
        var userId = currentUser.Id;

        if (string.IsNullOrEmpty(userId))
        {
            return Json(new { success = false, message = "User not authenticated for this modal" });
        }

        var projects = await dbContext.GitProjects
            .Where(p => p.UserProfileId == userId)
            .Select(p => new
            {
                p.Id,
                p.Name,
                p.RepoUrl,
                p.Description,
                p.ReadmeHtml
            })
            .OrderByDescending(p => p.Id)
            .ToListAsync();

        return Json(new { success = true, projects = projects });
    }
    
    [HttpGet]
    public async Task<IActionResult> GetUserProjectsHtml()
    {
        // Set the current user as the owner
        var currentUser = await userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return NotFound();
        }
        
        try
        {
            var userId = currentUser.Id;

            var projects = await dbContext.GitProjects
                .Where(p => p.UserProfileId == userId)
                .OrderByDescending(p => p.Id)
                .ToListAsync();
            
            return PartialView("_ProjectList", projects);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting user project list");
            return Content("<div class='alert alert-danger'>Error loading projects</div>");
        }
    } 
}