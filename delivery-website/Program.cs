using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using delivery_website.Data;
using delivery_website.Repositories.Interfaces;
using delivery_website.Repositories.Implementations;
using delivery_website.Services.Interfaces;
using delivery_website.Services.Implementations;
using delivery_website.Models.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 0;

    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = false;
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Configure cookie settings
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromDays(30);
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.SlidingExpiration = true;
});

// Configure Email Settings
builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("EmailSettings"));

builder.Services.AddScoped<IRestaurantRepository, RestaurantRepository>();

builder.Services.AddScoped<IRestaurantService, RestaurantService>();
builder.Services.AddScoped<IEmailService, EmailService>();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Seed Database and Roles
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        var canConnect = await context.Database.CanConnectAsync();
        Console.WriteLine(canConnect ? "✓ База даних підключена!" : "✗ Помилка підключення до бази даних!");

        if (canConnect)
        {
            await context.Database.MigrateAsync();
            Console.WriteLine("✓ Міграції застосовано");

            // Seed Roles
            await SeedRoles(roleManager);

            // Seed Data
            await DbSeeder.SeedAsync(context, userManager);
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"✗ Помилка: {ex.Message}");
        Console.WriteLine($"Stack trace: {ex.StackTrace}");
    }
}

static async Task SeedRoles(RoleManager<IdentityRole> roleManager)
{
    string[] roles = { "Admin", "Customer", "RestaurantOwner" };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
            Console.WriteLine($"✓ Створено роль: {role}");
        }
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();