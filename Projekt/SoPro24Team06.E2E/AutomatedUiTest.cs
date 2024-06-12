using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace SoPro24Team06.E2E;

public class AutomatedUiTest : IDisposable
{
    private readonly IWebDriver _driver;
    public AutomatedUiTest() => _driver = new ChromeDriver();
    public void Dispose()
    {
        _driver.Quit();
        _driver.Dispose();
    }

    [Fact]
    public void Create_WhenExecuted_ReturnsCreateView()
    {
        _driver.Navigate()
            .GoToUrl("https://www.google.de");
        Assert.Equal("Google", _driver.Title);
    }
}