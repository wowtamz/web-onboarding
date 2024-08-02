# Projekt Setup:

## Correct Packages and Versions to Use

+  Chrome Web Drive:
   Most current version:
   `dotnet add SoPro24Team06.E2E package Selenium.WebDriver.ChromeDriver`
+  Microsoft.AspNetCore.Mvc.Testing
   Version 6.0.32
   `dotnet add SoPro24Team06.E2E package Selenium.WebDriver.ChromeDriver`
+  Microsoft.EntityFrameworkCore.InMemory
   Version="7.0.5"
   `dotnet add SoPro24Team06 package Microsoft.EntityFrameworkCore.InMemory --version 7.0.5`

## Make Sure Test are not Parallelized

create in both the main project and the test project a file named ``xunit.runner.json ``with the following content:

````json
{
	"parallelize": false
}
````

This is important so that every test uses its own factory and not the one of another test.

## Make Sure the Program.cs file is correctly configured:

current working program.cs

````c#
using System.Data.Common;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Sqlite;
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
if (builder.Environment.IsEnvironment("Testing") == false)
{
    throw new Exception("Enviroment is not testing");
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"))
    );
}
else
{
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseInMemoryDatabase("TestDatabase")
    );
    // builder.Services.AddSingleton<DbConnection>(container =>
    // {
    //     var connection = new SqliteConnection("DataSource=:memory:");
    //     connection.Open();

    //     return connection;
    // });

    // builder.Services.AddDbContext<ApplicationDbContext>(
    //     (container, options) =>
    //     {
    //         var connection = container.GetRequiredService<DbConnection>();
    //         options.UseSqlite(connection);
    //     }
    // );
}

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
    if (app.Environment.IsEnvironment("Testing"))
    {
        await context.Database.EnsureCreatedAsync();
    }
    else
    {
        await context.Database.MigrateAsync();
        //await context.Database.MigrateAsync();
        await SeedData.Initialize(userManager, roleManager, context);
    }
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
````

# Create WebApplicationFactory:

````c#
using System.Data.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SoPro24Team06.Data;
using SoPro24Team06.Models;

public class CustomWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup>
    where TStartup : class
{
    private readonly string _baseAddress = "https://localhost:7003";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseUrls("https://localhost:7003");
        builder.ConfigureServices(services =>
        {
            // get old Database Connection
            var descriptor = services.SingleOrDefault(d =>
                d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>)
            );
            // if foun remove old Database Connection
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            //add new Db Discriptor
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase("TestDatabase")
            );
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var dummyHost = builder.Build();

        builder.ConfigureWebHost(webHostBuilder => webHostBuilder.UseKestrel());

        var host = builder.Build();
        host.Start();

        return dummyHost;
    }
}
````

Achtung im Internet ist es verdammt schwer infos dazu zu finden,
und wenn man chatgpt fragt dann braucht man die IHost Override funktion nicht, aber das ist nur wahr wenn man die Test nicht über einen Browser durchführen möchte!



# Create your own Test:

````c#
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SoPro24Team06.Models;

namespace SoPro24Team06.E2E
{
    public class AssignmentE2ETest
        : IClassFixture<CustomWebApplicationFactory<Program>>,
            IDisposable
    {
        private readonly CustomWebApplicationFactory<Program> _factory;
        private readonly ChromeDriver _driver;
        private readonly WebDriverWait _wait;

        public AssignmentE2ETest()
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");
            _factory = new CustomWebApplicationFactory<Program>();
            var options = new ChromeOptions();
            //options.AddArguments("--headless");
            options.AddArguments("--disable-dev-shm-usage");
            options.AddArguments("--no-sandbox");
            options.AddArguments("--ignore-certificate-errors");
            options.AddArguments("--allow-insecure-localhost");
            options.AddArguments("--window-size=1920,1080");

            var service = ChromeDriverService.CreateDefaultService();
            _driver = new ChromeDriver(service, options);

            _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(30)); // Increase wait time
        }

        [Fact]
        public async Task TestIfIdentityWorks()
        {
            HttpClient client = _factory.CreateDefaultClient();
            //await TestIfIsRunning(client);
            string userName = "testuser@example.com";
            string password = "Password123!";
            using (var scope = _factory.Services.CreateScope())
            {
                // Resolve the UserManager and other services from the scope
                var userManager = scope.ServiceProvider.GetRequiredService<
                    UserManager<ApplicationUser>
                >();
                var roleManager = scope.ServiceProvider.GetRequiredService<
                    RoleManager<ApplicationRole>
                >();

                // Your test logic here
                var user = new ApplicationUser
                {
                    FullName = "Test User",
                    UserName = userName,
                    Email = userName
                };
                var result = await userManager.CreateAsync(user, password);
                userManager.AddToRoleAsync(user, "Administrator");

                if (!result.Succeeded)
                {
                    throw new InvalidOperationException(
                        $"Failed to create test user: {string.Join(", ", result.Errors.Select(e => e.Description))}"
                    );
                }
                Assert.True(result.Succeeded);
            }
            //Login("admin@gmx.de", "Admin1!");
            Login(userName, password);
        }

        public void Login(string userName, string password)
        {
            _driver.Navigate().GoToUrl("https://localhost:7003/");

            if (_driver.Url.Contains("Identity/Account/Login"))
            {
                try
                {
                    try
                    {
                        var emailElement = _wait.Until(
                            SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(
                                By.Id("Input_Email")
                            )
                        );
                        emailElement.SendKeys(userName);
                    }
                    catch
                    {
                        throw new Exception("Failed to find userName Field");
                    }
                    Task.Delay(2000);

                    try
                    {
                        var passwordElement = _wait.Until(
                            SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(
                                By.Id("Input_Password")
                            )
                        );
                        passwordElement.SendKeys(password);
                    }
                    catch
                    {
                        throw new Exception("Failed to find Password Field");
                    }
                    Task.Delay(2000);

                    IWebElement loginButton;
                    try
                    {
                        loginButton = _wait.Until(
                            SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(
                                By.CssSelector("button[aria-label='login-submit']")
                            )
                        );
                    }
                    catch
                    {
                        throw new Exception("Failed to find Login Button Field");
                    }

                    bool isElementObscured = (bool)
                        ((IJavaScriptExecutor)_driver).ExecuteScript(
                            "return !(arguments[0].getBoundingClientRect().top >= 0 && arguments[0].getBoundingClientRect().bottom <= (window.innerHeight || document.documentElement.clientHeight));",
                            loginButton
                        );

                    if (isElementObscured)
                    {
                        throw new Exception("Login button is obscured by another element.");
                    }

                    try
                    {
                        loginButton.Click();
                    }
                    catch (Exception e)
                    {
                        throw new Exception("Failed to click login button" + e.Message);
                    }
                    Task.Delay(20000);
                    if (_driver.Url.Contains("Identity/Account/Login"))
                    {
                        throw new Exception("not redirected to Assignment Screen");
                    }
                }
                catch (WebDriverTimeoutException ex)
                {
                    throw new Exception("Failed to find an element during login.", ex);
                }
            }
        }

        public async Task TestIfIsRunning(HttpClient client)
        {
            bool isRunning = false;
            for (int i = 0; i < 4; i++)
            {
                try
                {
                    var response = await client.GetAsync("/");
                    if (response.IsSuccessStatusCode)
                    {
                        isRunning = true;
                        break;
                    }
                }
                catch { }
                await Task.Delay(10000);
            }
            if (isRunning == false)
            {
                throw new Exception("client not running");
            }
        }

        public void Dispose()
        {
            _driver.Close();
            _driver.Dispose();
            _factory.Dispose();
        }
    }
}
	
````

It is absolut important to dispose of the driver and factory
Auch muss man darauf achten das man RoleManager, UserManager und Context nur auf die Art und weise verwendet wie im Beispiel und auf keinen Fall außerhalb des Using bereches!
Sonst funktioneirt es einfach nicht !