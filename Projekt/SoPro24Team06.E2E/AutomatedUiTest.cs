using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace SoPro24Team06.E2E;

public class AutomatedUiTest : IDisposable
{
    private readonly IWebDriver _driver;

    public AutomatedUiTest()
    {
        Environment.SetEnvironmentVariable("DISPLAY", ":99");

        var options = new ChromeOptions();
        options.AddArguments("--no-sandbox");
        options.AddArguments("--headless");
        options.AddArguments("--disable-gpu");
        options.AddArguments("--disable-dev-shm-usage");
        options.AddArguments("--disable-extensions");
        options.AddArguments("--disable-infobars");
        options.AddArguments("--remote-debugging-port=9222");

        var service = ChromeDriverService.CreateDefaultService();
        service.LogPath = "chromedriver.log";
        service.EnableVerboseLogging = true;

        _driver = new ChromeDriver(service, options);
    }

    [Fact]
    public void Create_WhenExecuted_ReturnsCreateView()
    {
        _driver.Navigate().GoToUrl("https://www.google.de");
        Assert.Equal("Google", _driver.Title);
    }

    public void Dispose()
    {
        _driver.Quit();
        _driver.Dispose();
    }
}
