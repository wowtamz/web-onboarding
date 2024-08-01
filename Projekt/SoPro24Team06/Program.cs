using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"))
);

builder
    .Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
        options.SignIn.RequireConfirmedAccount = false
    )
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Ende: Neu DbContext


/*
builder
    .Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false; // email confirmation!
    })
    .AddEntityFrameworkStores<UserDbContext>()
    .AddDefaultTokenProviders();
*/

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60); // Session-Timeout
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // Adjust based on your environment
});

var app = builder.Build();

// Seed roles and users
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();
    var context = services.GetRequiredService<ApplicationDbContext>();
    await context.Database.MigrateAsync();
    await SeedData.Initialize(userManager, roleManager, context);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

/*
else
{
    app.UseDeveloperExceptionPage();
}
*/

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseSession();

app.UseMiddleware<LogoutOnLockoutMiddleware>();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}"
    );
});

app.MapRazorPages();

app.Run();
