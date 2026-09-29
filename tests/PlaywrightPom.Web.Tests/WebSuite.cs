using Bogus;
using Microsoft.Playwright;
using PlaywrightPom.Core;
using PlaywrightPom.NUnit;
using PlaywrightPom.Web.Flows;
using PlaywrightPom.Web.Network;
using PlaywrightPom.Web.Pages;
using Shouldly;

namespace PlaywrightPom.Web.Tests;

[SetUpFixture]
public sealed class WebTestRun
{
    [OneTimeSetUp]
    public void Start() => FrameworkRuntime.EnsureStarted("web");

    [OneTimeTearDown]
    public void Finish() => FrameworkRuntime.Finish("web");
}

[TestFixture]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
[Parallelizable(ParallelScope.Self)]
[Category(Traits.Web)]
public sealed class LoginTests : BaseUiTest
{
    [Test]
    [Category(Traits.Smoke)]
    [Category(Traits.Critical)]
    [CaseId("WEB-001")]
    public async Task StandardUser_CanSignIn_AndSeeInventory()
    {
        var login = new LoginPage(Page);
        await login.OpenAsync();
        await login.SignInAsync(Settings.Web.DefaultUser, Settings.Web.DefaultPassword);

        var inventory = new InventoryPage(Page);
        await inventory.ExpectLoadedAsync();
        (await inventory.PricesAsync()).Count.ShouldBeGreaterThan(1);
    }

    [Test]
    [Category(Traits.Regression)]
    [CaseId("WEB-002")]
    public async Task LockedOutUser_SeesLockMessage()
    {
        var login = new LoginPage(Page);
        await login.OpenAsync();
        await login.SignInAsync(Settings.Web.LockedOutUser, Settings.Web.DefaultPassword);

        var error = await login.ReadErrorAsync();
        error.ShouldContain("locked out");
    }

    [Test]
    [Category(Traits.Regression)]
    [CaseId("WEB-003")]
    public async Task UnknownPassword_SeesCredentialMessage()
    {
        var login = new LoginPage(Page);
        await login.OpenAsync();
        await login.SignInAsync(Settings.Web.DefaultUser, Settings.Web.InvalidPassword);

        var error = await login.ReadErrorAsync();
        error.ShouldContain("do not match");
    }
}

[TestFixture]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
[Parallelizable(ParallelScope.Self)]
[Category(Traits.Web)]
[Category(Traits.Regression)]
public sealed class InventoryTests : BaseUiTest
{
    [Test]
    [CaseId("WEB-004")]
    public async Task SortByPriceLowToHigh_OrdersTheCatalogue()
    {
        var login = new LoginPage(Page);
        await login.OpenAsync();
        await login.SignInAsync(Settings.Web.DefaultUser, Settings.Web.DefaultPassword);

        var inventory = new InventoryPage(Page);
        await inventory.ExpectLoadedAsync();
        await inventory.SortByPriceLowToHighAsync();
        var prices = await inventory.PricesAsync();

        prices.Count.ShouldBeGreaterThan(1);
        prices.ShouldBe(prices.OrderBy(price => price).ToArray());
    }
}

[TestFixture]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
[Parallelizable(ParallelScope.Self)]
[Category(Traits.Web)]
public sealed class CheckoutTests : BaseUiTest
{
    [Test]
    [Retry(2)]
    [Category(Traits.Smoke)]
    [Category(Traits.Critical)]
    [CaseId("WEB-005")]
    public async Task TwoProducts_CompletePurchase()
    {
        var faker = new Faker();
        var confirmation = await new PurchaseFlow(Page).PurchaseAsync(
            Settings.Web.DefaultUser,
            Settings.Web.DefaultPassword,
            ["Sauce Labs Backpack", "Sauce Labs Bike Light"],
            new Customer(faker.Name.FirstName(), faker.Name.LastName(), "90210"));

        confirmation.ShouldContain("Thank you for your order");
    }

    [Test]
    [Category(Traits.Regression)]
    [CaseId("WEB-006")]
    public async Task RemoveFromCart_ClearsTheLineAndBadge()
    {
        var login = new LoginPage(Page);
        await login.OpenAsync();
        await login.SignInAsync(Settings.Web.DefaultUser, Settings.Web.DefaultPassword);

        var inventory = new InventoryPage(Page);
        await inventory.ExpectLoadedAsync();
        await inventory.AddAsync("Sauce Labs Backpack");
        await inventory.Header.OpenCartAsync();

        var cart = new CartPage(Page);
        await cart.ExpectLoadedAsync();
        await cart.RemoveAsync("Sauce Labs Backpack");
        await cart.ExpectCountAsync(0);
        await Expect(inventory.Header.Badge).ToHaveCountAsync(0);
    }
}

[TestFixture]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
[Parallelizable(ParallelScope.Self)]
[Category(Traits.Web)]
[Category(Traits.Regression)]
public sealed class NetworkResilienceTests : BaseUiTest
{
    [Test]
    [CaseId("WEB-007")]
    public async Task Inventory_RemainsUsable_WhenImagesAreBlocked()
    {
        var traffic = new TrafficLog();
        traffic.Attach(Page);
        await NetworkControls.AbortImagesAsync(Page);

        var login = new LoginPage(Page);
        await login.OpenAsync();
        await login.SignInAsync(Settings.Web.DefaultUser, Settings.Web.DefaultPassword);

        var inventory = new InventoryPage(Page);
        await inventory.ExpectLoadedAsync();
        traffic.Urls.ShouldContain(url => url.Contains("saucedemo.com", StringComparison.OrdinalIgnoreCase));
    }
}
