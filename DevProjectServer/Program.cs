using DevProjectServer.Data;
using DevProjectServer.Models;
using DevProjectServer.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ApplicationDbContext>(options => 
{
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddDefaultIdentity<UserProfile>(options => 
        options.SignIn.RequireConfirmedAccount = false).AddEntityFrameworkStores<ApplicationDbContext>();

// Add GitHub service
builder.Services.AddHttpClient<IGitHubService, GitHubService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.EnsureCreated();
}

app.MapGet("/", context =>
{
    // Check if user is authenticated
    if (context.User.Identity?.IsAuthenticated == true)
    {
        // Authenticated users go to home page
        context.Response.Redirect("/Home");
        return Task.CompletedTask;
    }
    else
    {
        // Unauthenticated users go to login
        context.Response.Redirect("/Identity/Account/Login");
        return Task.CompletedTask;
    }
});

app.Run();