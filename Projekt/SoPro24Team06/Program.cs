using System.Data.Common;
using System.IO;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SoPro24Team06.Data;
using SoPro24Team06.Models;

var builder = WebApplication.CreateBuilder(args);

var dataDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Data");
if (!Directory.Exists(dataDirectory))
{
    Directory.CreateDirectory(dataDirectory);
}

/* Alte DbContext
var connectionString = builder.Configuration.GetConnectionString("UserConnection");
builder.Services.AddDbContext<UserDbContext>(options =>
    options.UseSqlite(connectionString + ";Pooling=False")
); // Disable pooling
*/

// Beginn: Neue DbContext
if (!builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"))
    );
}
else
{
    builder.Services.AddSingleton<DbConnection>(container =>
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        return connection;
    });

    builder.Services.AddDbContext<ApplicationDbContext>(
        (container, options) =>
        {
            var connection = container.GetRequiredService<DbConnection>();
            options.UseSqlite(connection);
        }
    );
}

builder
    .Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
    {
        options.Password.RequireDigit = false;
        options.Password.RequiredLength = 6;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireLowercase = false;
        options.Password.RequiredUniqueChars = 1;

        options.SignIn.RequireConfirmedAccount = false;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60); // Session-Timeout
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});

builder
    .Services.AddDataProtection()
    .SetApplicationName("SoPro24Team06")
    .PersistKeysToFileSystem(new DirectoryInfo(@"./keys/"))
    .SetDefaultKeyLifetime(TimeSpan.FromDays(14)); // Change this to invalidate old sessions

var app = builder.Build();

// Seed roles and users
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();
    var context = services.GetRequiredService<ApplicationDbContext>();
    await context.Database.EnsureCreatedAsync();
    //await context.Database.MigrateAsync();
    await SeedData.Initialize(userManager, roleManager, context);

    // Einmalige Invalidierung der Sessions beim Start der Anwendung
    var keyRingPath = Path.Combine(Directory.GetCurrentDirectory(), "keys");
    if (Directory.Exists(keyRingPath))
    {
        Directory.Delete(keyRingPath, true); // Lösche alle vorhandenen Schlüssel
    }
    Directory.CreateDirectory(keyRingPath); // Erstelle den Schlüsselordner neu
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<LogoutOnLockoutMiddleware>();

app.UseSession();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}"
    );
});

app.MapRazorPages();

app.Run();

public partial class Program { }
