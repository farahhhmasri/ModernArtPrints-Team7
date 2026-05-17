using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ModernArtPrints.Data;
using ModernArtPrints.Models;
using Stripe;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders()
    .AddDefaultUI();
builder.Services.AddControllersWithViews();


// Register a distributed cache required by the session middleware
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
});



var app = builder.Build();
StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];



// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthentication();

app.UseAuthorization();

// seeding roles and default admin
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    string[] roles = { "Admin", "User" };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    // creating the Admin
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    string AdminEmail = "lailaharb004@gmail.com";
    var adminUser = await userManager.FindByEmailAsync(AdminEmail);

    if (adminUser == null)
    {
        var newAdmin = new ApplicationUser
        {
            FullName = "Laila Harb",
            Address = "Amman, Jordan",
            ProfileImageUrl = "~/uploads/users/AdminPic.jpeg",
            UserName = AdminEmail,
            Email = AdminEmail,
            EmailConfirmed = true
        };

        var createResult = await userManager.CreateAsync(newAdmin, "Hguer@2000hu");
        if (createResult.Succeeded)
        {
            // re-fetch the user from the store to ensure the Id is populated
            var createdAdmin = await userManager.FindByEmailAsync(AdminEmail);
            if (createdAdmin != null)
            {
                await userManager.AddToRoleAsync(createdAdmin, "Admin");
            }
        }
    }


    // Adding user to the system
    string UserEmail = "farahhh.adel@gmail.com";
    var User = await userManager.FindByEmailAsync(UserEmail);

    if (User == null)
    {
        var user = new ApplicationUser
        {
            FullName = "Farah Adel",
            Address = "Amman, Jordan",
            ProfileImageUrl = "~/uploads/users/AdminPic.jpeg",
            UserName = UserEmail,
            Email = UserEmail,
            EmailConfirmed = true
        };

        var createResult = await userManager.CreateAsync(user, "NoorIsTheBest&zuj22");
        if (createResult.Succeeded)
        {
            var createdUser = await userManager.FindByEmailAsync(UserEmail);
            if (createdUser != null)
            {
                await userManager.AddToRoleAsync(createdUser, "User");
            }
        }
    }

}

app.UseSession();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=User}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
