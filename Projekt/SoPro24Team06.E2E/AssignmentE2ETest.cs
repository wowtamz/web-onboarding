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
