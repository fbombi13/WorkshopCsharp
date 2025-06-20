using System.Collections.ObjectModel;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using WebDriverManager;
using WebDriverManager.DriverConfigs.Impl;
namespace Workshop;

[TestFixture]
public class EpamWorkshopSearchTests
{ 
    private WebDriver _driver;

    private static readonly string _BASE_URL = "https://epam.com/";

    private readonly By _careersLink = By.LinkText("Careers");
    private readonly By _searchFormWrapper = By.Id("jobSearchFilterForm");
    private readonly By _searchFormKeyword = By.Id("new_form_job_search-keyword");
    private readonly By _searchFormLocation = By.ClassName("recruiting-search__location");
    private readonly By _searchFormLocationAllLocations = By.CssSelector("li[title='All Locations']");
    private readonly By _searchFormLocationLabel = By.XPath("//label[@for='new_form_job_search-location']");
    private readonly By _searchFormRemoteCheckbox = By.XPath("//label[normalize-space()='Remote']");
    private readonly By _searchFormFindButton = By.XPath("//form[@id='jobSearchFilterForm'] //button[normalize-space()='Find']");
    private readonly By _lastSearchResult = By.CssSelector(".search-result__item:last-of-type");
    private readonly By _resultHeadTitle = By.CssSelector(".heading-5");
    private readonly By _resultViewApplyButton = By.CssSelector(".search-result__item-controls a");
    private readonly By _vacancyJobTitle = By.ClassName("vacancy-details-23__job-title");

    private readonly By _magnifierIcon = By.CssSelector(".header-search__button");
    private readonly By _seachInput = By.CssSelector(".search-results__input-holder [name='q']");
    private readonly By _findButton = By.CssSelector(".search-results__action-section .custom-search-button");
    private readonly By _articlesFound = By.TagName("article");
    private readonly By _preloader = By.ClassName("preloader");

    [SetUp]
    public void Setup()
    {
        new DriverManager().SetUpDriver(new ChromeConfig());
        _driver = new ChromeDriver();
        _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(30);
        _driver.Manage().Window.Maximize();
        _driver.Navigate().GoToUrl(_BASE_URL);
    }

    [TearDown]
    public void TearDown()
    {
        if (_driver != null)
        {
            _driver.Quit();
            _driver.Dispose();
        }
    }

    public WebDriverWait WaitFor(WebDriver driver, TimeSpan timeout, TimeSpan polling)
    {
        return new WebDriverWait(driver, timeout)
        {
            PollingInterval = polling
        };
    }

    [Test]
    public void UserSearchPosition()
    {
        WebDriverWait wait = new(_driver, TimeSpan.FromSeconds(10))
        {
            PollingInterval = TimeSpan.FromMilliseconds(500)
        };
        _driver.FindElement(_careersLink).Click();
        var searchForm = _driver.FindElement(_searchFormWrapper);
        searchForm.FindElement(_searchFormKeyword).SendKeys("Java");
        searchForm.FindElement(_searchFormLocation).Click();
        searchForm.FindElement(_searchFormLocationAllLocations).Click();
        searchForm.FindElement(_searchFormLocationLabel).Click();
        searchForm.FindElement(_searchFormRemoteCheckbox).Click();
        searchForm.FindElement(_searchFormFindButton).Click();
        WaitFor(_driver, TimeSpan.FromSeconds(10), TimeSpan.FromMilliseconds(100))
            .Until((driver) =>
                driver.Url.Contains("job-listings")
                && _driver.FindElement(_lastSearchResult).Displayed
            );
        IWebElement foundItem = _driver.FindElement(_lastSearchResult);
        string role = foundItem.FindElement(_resultHeadTitle).Text;
        Console.Out.WriteLine("role: " + role);
        foundItem.FindElement(_resultViewApplyButton).Click();
        var selectedRole = _driver.FindElement(_vacancyJobTitle).Text.Split("\n")[0];
        Assert.That(selectedRole, Is.EqualTo(role));
    }

    [Test]
    [TestCase("\"BLOCKCHAIN\"")]
    [TestCase("\"Cloud\"")]
    public void GlobalSearch(string searchParam)
    {
        _driver.FindElement(_magnifierIcon).Click();
        _driver.FindElement(_seachInput).SendKeys(searchParam);
        _driver.FindElement(_findButton).Click();
        WaitFor(_driver, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(1))
            .Until((driver) => !driver.FindElement(_preloader).Displayed);
        ReadOnlyCollection<IWebElement> articles = _driver.FindElements(_articlesFound);
        List<string> invalidItems = articles
            .Where(article => !article.Text.Contains(searchParam.Replace("\"", ""), StringComparison.CurrentCultureIgnoreCase))
            .Select(article => article.Text)
            .ToList();
        Assert.That(invalidItems, Is.Empty, $"Searching by {searchParam} - These results DO NOT INCLUDE: {string.Join(',', invalidItems)}");
    }
}