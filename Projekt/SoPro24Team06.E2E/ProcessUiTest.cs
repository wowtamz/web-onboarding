using SoPro24Team06.Controllers;
using SoPro24Team06.Models;
using SoPro24Team06.ViewModels;
using System;
using System.IO;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using Xunit;

namespace SoPro24Team06.E2E;

public class ProcessUiTest : IDisposable
{
    private readonly string baseurl = "https://localhost:7003/";
    private readonly IWebDriver _driver;
    private readonly WebDriverWait _wait;

    public ProcessUiTest()
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
        _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(60));
    }
    public void Dispose() 
    {
        _driver.Quit(); 
        _driver.Dispose(); 
    }
    
    [Fact] 
    public void StartProcessFromProcessTemplate()
    {
        _driver.Navigate().GoToUrl(baseurl);
        
        // Login
        if (_driver.Url.Contains("Identity/Account/Login"))
        {
            try
            {
                var emailElement = _wait.Until(
                    SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(
                        By.Id("Input_Email")
                    )
                );
                emailElement.SendKeys("user@example.com");

                var passwordElement = _wait.Until(
                    SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(
                        By.Id("Input_Password")
                    )
                );
                passwordElement.SendKeys("User@123");

                var loginButton = _wait.Until(
                    SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(
                        By.CssSelector("[aria-label='login-submit']")
                    )
                );
                loginButton.Click();
            }
            catch (WebDriverTimeoutException exception)
            {
                throw new Exception(
                    "WedDriver timedout during loginAttempt" + exception.Message
                );
            }
        }
        
        // Go to ProcessTemplates
        _driver.Navigate().GoToUrl(baseurl+"ProcessTemplate/");
        if (_driver.Title.Contains("Prozesse"))
        {
            try
            {
                IWebElement linkElement = _wait.Until(driver =>
                {
                    return driver.FindElement(By.XPath("//a[contains(@href, '/Process/Start/')]"));
                });
                
                linkElement.Click();
                
            }
            catch (WebDriverTimeoutException exception)
            {
                throw new Exception(
                    "WedDriver timedout during ProcessTemplate View" + exception.Message
                );
            }
        }

        string processTitle = "";
        
        // Check if at Process Start
        if (_driver.Title.Contains("Vorgang starten"))
        {
            try
            {
                IWebElement inputTitle = _wait.Until(driver =>
                {
                    IWebElement element = driver.FindElement(By.Id("Title"));

                    return element.Displayed ? element : null;
                });

                if (inputTitle != null)
                {
                    processTitle = inputTitle.GetAttribute("value");
                }
                
                // Select WorkerOfReference
                IWebElement dropdownWorkerOfRef = _wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.Id("workerOfRefDropdown")));
                SelectElement selectWorkerOfRef = new SelectElement(dropdownWorkerOfRef);
                selectWorkerOfRef.SelectByIndex(1);
                
                // Select ContractOfRefWorker
                IWebElement dropdownContract = _wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.Id("contractDropdown")));
                SelectElement selectContract = new SelectElement(dropdownContract);
                selectContract.SelectByIndex(1);
                
                // Select DepartmentOfRefWorker
                IWebElement dropdownDepartment = _wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.Id("departmentDropdown")));
                SelectElement selectDepartment = new SelectElement(dropdownDepartment);
                selectDepartment.SelectByIndex(1);
                
                // Click Start Button
                IWebElement startButton = _wait.Until(driver =>
                {
                    var buttons = driver.FindElements(By.TagName("button"));
                    IWebElement button = buttons.FirstOrDefault(b => b.Text.Equals("Starten", StringComparison.OrdinalIgnoreCase));
                
                    return button;
                });
                
                startButton.Click();
                
            }
            catch (NoSuchElementException)
            {
                Console.WriteLine("Element not found.");
            }
            catch (WebDriverTimeoutException exception)
            {
                throw new Exception(
                    "WedDriver timedout during Process Start View" + exception.Message
                );
            }
        }
        
        _driver.Navigate().GoToUrl(baseurl+"Process");
        if (_driver.Title.Contains("Vorgänge"))
        {

            try
            {
                IWebElement tdElement = _wait.Until(driver =>
                {
                    IWebElement element = driver.FindElement(By.XPath($"//td[text()='{processTitle}']"));
                    return element.Displayed ? element : null;
                });
                
                Assert.NotNull(tdElement);
            }
            catch (NoSuchElementException)
            {
                Console.WriteLine("Element not found.");
            }
            catch (WebDriverTimeoutException exception)
            {
                throw new Exception(
                    "WedDriver timedout during Process View" + exception.Message
                );
            }
        }
        
        
    }
    
    [Fact]
    public void StartProcessFromTemplateAddCustomAssignment()
    {
        _driver.Navigate().GoToUrl(baseurl);
        
        // Login
        if (_driver.Url.Contains("Identity/Account/Login"))
        {
            try
            {
                var emailElement = _wait.Until(
                    SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(
                        By.Id("Input_Email")
                    )
                );
                emailElement.SendKeys("user@example.com");

                var passwordElement = _wait.Until(
                    SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(
                        By.Id("Input_Password")
                    )
                );
                passwordElement.SendKeys("User@123");

                var loginButton = _wait.Until(
                    SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(
                        By.CssSelector("[aria-label='login-submit']")
                    )
                );
                loginButton.Click();
            }
            catch (WebDriverTimeoutException exception)
            {
                throw new Exception(
                    "WedDriver timedout during loginAttempt" + exception.Message
                );
            }
        }
        
        // Go to ProcessTemplates
        _driver.Navigate().GoToUrl(baseurl+"ProcessTemplate/");
        if (_driver.Title.Contains("Prozesse"))
        {
            try
            {
                IWebElement linkElement = _wait.Until(driver =>
                {
                    return driver.FindElement(By.XPath("//a[contains(@href, '/Process/Start/')]"));
                });
                
                linkElement.Click();
                
            }
            catch (WebDriverTimeoutException exception)
            {
                throw new Exception(
                    "WedDriver timedout during ProcessTemplate View" + exception.Message
                );
            }
        }

        string processTitle = "";
        
        // Check if at Process Start
        if (_driver.Title.Contains("Vorgang starten"))
        {
            try
            {
                
                IWebElement inputTitle = _wait.Until(driver =>
                {
                    IWebElement element = driver.FindElement(By.Id("Title"));

                    return element.Displayed ? element : null;
                });
                
                if (inputTitle != null)
                {
                    processTitle = inputTitle.GetAttribute("value");
                }
                
                // Select WorkerOfReference
                IWebElement dropdownWorkerOfRef = _wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.Id("workerOfRefDropdown")));
                SelectElement selectWorkerOfRef = new SelectElement(dropdownWorkerOfRef);
                selectWorkerOfRef.SelectByIndex(1);
                
                // Select ContractOfRefWorker
                IWebElement dropdownContract = _wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.Id("contractDropdown")));
                SelectElement selectContract = new SelectElement(dropdownContract);
                selectContract.SelectByIndex(1);
                
                // Select DepartmentOfRefWorker
                IWebElement dropdownDepartment = _wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.Id("departmentDropdown")));
                SelectElement selectDepartment = new SelectElement(dropdownDepartment);
                selectDepartment.SelectByIndex(1);
                
                // Add new Assignment
                IWebElement addAssignmentButton = _wait.Until(driver =>
                {
                    var buttons = driver.FindElements(By.TagName("button"));
                    IWebElement button = buttons.FirstOrDefault(b => b.Text.Equals("Aufgabe hinzufügen", StringComparison.OrdinalIgnoreCase));
                
                    return button;
                });

                if (_driver.Url.Contains("AssignmentTemplate/Create"))
                {
                    try
                    {
                        inputTitle = _wait.Until(driver =>
                        {
                            IWebElement element = driver.FindElement(By.Id("Title"));

                            return element.Displayed ? element : null;
                        });
                        
                        if (inputTitle != null)
                        {
                            inputTitle.Clear();
                            inputTitle.SendKeys("Task 1");
                        }
                        else
                        {
                            _driver.Navigate().GoToUrl(baseurl+"/Process/Start");
                        }
                        
                        IWebElement createButton = _wait.Until(driver =>
                        {
                            var buttons = driver.FindElements(By.TagName("button"));
                            IWebElement button = buttons.FirstOrDefault(b => b.Text.Equals("Erstellen", StringComparison.OrdinalIgnoreCase));
                
                            return button;
                        });
                        
                        createButton.Click();
                    }
                    catch (NoSuchElementException)
                    {
                        Console.WriteLine("Element not found.");
                    }
                    catch (WebDriverTimeoutException exception)
                    {
                        throw new Exception(
                            "WedDriver timedout during ProcessTemplate View" + exception.Message
                        );
                    }
                }
                
                
                // Click Start Button
                IWebElement startButton = _wait.Until(driver =>
                {
                    var buttons = driver.FindElements(By.TagName("button"));
                    IWebElement button = buttons.FirstOrDefault(b => b.Text.Equals("Starten", StringComparison.OrdinalIgnoreCase));
                
                    return button;
                });
                
                startButton.Click();
                
            }
            catch (NoSuchElementException)
            {
                Console.WriteLine("Element not found.");
            }
            catch (WebDriverTimeoutException exception)
            {
                throw new Exception(
                    "WedDriver timedout during ProcessTemplate View" + exception.Message
                );
            }
            
            _driver.Navigate().GoToUrl(baseurl+"Process");
            if (_driver.Title.Contains("Vorgänge"))
            {
                try
                {
                    IWebElement tdElement = _wait.Until(driver =>
                    {
                        IWebElement element = driver.FindElement(By.XPath($"//td[text()='{processTitle}']"));
                        return element.Displayed ? element : null;
                    });
                
                    Assert.NotNull(tdElement);
                }
                catch (NoSuchElementException)
                {
                    Console.WriteLine("Element not found.");
                }
                catch (WebDriverTimeoutException exception)
                {
                    throw new Exception(
                        "WedDriver timedout during Process View" + exception.Message
                    );
                }
            }
        }
    }
}
