using Microsoft.Playwright;

namespace TravelUp.IntegrationTests;

[TestFixture]
public class ItemControllerFrontEndTests
{
    private IPage _page;
    private IBrowser _browser;
    private IBrowserContext _context;
    private const int TimeoutMs = 60000; // Increase timeout to 60 seconds
    private const string BaseUrl = "http://localhost:5091"; // Replace with the correct URL for your app

    [SetUp]
    public async Task SetUp()
    {
        var playwright = await Playwright.CreateAsync();
        _browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });

        if (_browser == null)
        {
            Console.WriteLine("Browser instance is null.");
            Assert.Fail("Failed to create browser instance.");
        }

        _context = await _browser.NewContextAsync();
        _page = await _context.NewPageAsync();
    }

    [Test]
    public async Task CreateItem_ShouldDisplayInList()
    {
        // Wait for the app to start and handle delays with retries
        await WaitForAppToStartAsync(BaseUrl, _page);

        // Navigate to the Create Item page
        await _page.GotoAsync($"{BaseUrl}/Item/Create", new PageGotoOptions { Timeout = TimeoutMs });

        // Fill in the form
        await _page.FillAsync("input[name='Name']", "Test Item");
        await _page.FillAsync("input[name='Description']", "Test Description");
        await _page.ClickAsync("input[type='submit']");

        // Wait for navigation to complete
        await _page.WaitForNavigationAsync(new PageWaitForNavigationOptions { Timeout = TimeoutMs });

        // Navigate to the Index page
        await _page.GotoAsync($"{BaseUrl}/Item/Index", new PageGotoOptions { Timeout = TimeoutMs });

        // Check if the item is displayed in the list
        var itemList = await _page.InnerTextAsync("ul.item-list", new PageInnerTextOptions { Timeout = TimeoutMs });
        Assert.IsTrue(itemList.Contains("Test Item"), "The created item should be in the item list.");
    }

    private async Task WaitForAppToStartAsync(string url, IPage page, int retries = 10, int delayMs = 5000)
    {
        for (int i = 0; i < retries; i++)
        {
            try
            {
                // Attempt to visit the app's base URL
                await page.GotoAsync(url, new PageGotoOptions { Timeout = TimeoutMs });
                return; // App is available, return
            }
            catch
            {
                // Wait before retrying
                Console.WriteLine($"Retrying to connect to {url} ({i + 1}/{retries})...");
                await Task.Delay(delayMs);
            }
        }

        Assert.Fail($"Failed to connect to {url} after {retries} retries.");
    }

    [TearDown]
    public async Task TearDown()
    {
        await _browser?.CloseAsync();
    }
}
