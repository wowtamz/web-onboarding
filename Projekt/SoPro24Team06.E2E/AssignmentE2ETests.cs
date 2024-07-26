using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace SoPro24Team06.E2E
{
    public class AssignmentE2ETests : IDisposable
	{ 
		private readonly IWebDriver _driver

		Environment.SetEnvironmentVariable("DISPLAY", ":99");

        
		public AssignmentE2ETest ()
		{
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
	}

	[SetUp]
	public void Setup (user)
	{
		baseUrl = ConfigurationManager.AppSettings["BaseUrl"];
		driver = new ChromeDriver();
		driver.Navigate().GoToUrl(baseUrl + "/login");
	}

	[TearDown]
	public void TearDown()
	{
		driver.Quit();
	}

}
