using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using delivery_website.Data;
using delivery_website.Repositories.Interfaces;
using delivery_website.Repositories.Implementations;
using delivery_website.Services.Interfaces;
using delivery_website.Services.Implementations;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Register Repositories
builder.Services.AddScoped<IRestaurantRepository, RestaurantRepository>();

// Register Services
builder.Services.AddScoped<IRestaurantService, RestaurantService>();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Seed Database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();

        var canConnect = await context.Database.CanConnectAsync();
        Console.WriteLine(canConnect ? "✓ База даних підключена!" : "✗ Помилка підключення до бази даних!");

        if (canConnect)
        {
            await context.Database.MigrateAsync();
            Console.WriteLine("✓ Міграції застосовано");

            await DbSeeder.SeedAsync(context, userManager);
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"✗ Помилка: {ex.Message}");
        Console.WriteLine($"Stack trace: {ex.StackTrace}");
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