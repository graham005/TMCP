
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using TMCP.Models.Data;
using Microsoft.EntityFrameworkCore.Diagnostics;
using TMCP.Models.Services;
using Microsoft.AspNetCore.Identity.UI.Services;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DbContextConnection") ?? throw new InvalidOperationException("Connection string 'DbContextConnection' not found.");;

builder.Services.AddDbContext<TMCPDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DbContextConnection"))
    .ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
});

builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
    .AddEntityFrameworkStores<TMCPDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddRazorPages();

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddTransient<IEmailSender, DummyEmailSender>();

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

using (var scope = app.Services.CreateScope())
{
    var roleManager =
        scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var roles = new[] { "SuperAdmin", "Admin", "TeamManager", "Member" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
    }
}
using (var scope = app.Services.CreateScope())
{
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var hasher = new PasswordHasher<User>();

    string email = "superadmin@tcmp.com";
    string password = hasher.HashPassword(null, "StrongPassword123!");
    string username = "superadmin@tcmp.com";

    if (await userManager.FindByEmailAsync(email) == null)
    {
        // Create the Admin role if it doesn't exist
        if (!await roleManager.RoleExistsAsync("SuperAdmin"))
        {
            await roleManager.CreateAsync(new IdentityRole("SuperAdmin"));
        }

        var user = new User
        {
            UserName = username,
            Email = email,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(user, password);

        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(user, "SuperAdmin");
        }
        else
        {
            foreach (var error in result.Errors)
            {
                Console.WriteLine($"User creation failed: {error.Description}");
            }
        }
    }
}

app.MapStaticAssets();

app.MapRazorPages();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


app.Run();
