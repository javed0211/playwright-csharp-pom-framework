using PlaywrightPom.Api.Client;
using PlaywrightPom.Api.Endpoints;
using PlaywrightPom.Api.Models;
using PlaywrightPom.Core;
using PlaywrightPom.NUnit;
using Shouldly;

namespace PlaywrightPom.Api.Tests;

[SetUpFixture]
public sealed class ApiTestRun
{
    [OneTimeSetUp]
    public void Start() => FrameworkRuntime.EnsureStarted("api");

    [OneTimeTearDown]
    public void Finish() => FrameworkRuntime.Finish("api");
}

[TestFixture]
[Parallelizable(ParallelScope.Self)]
[Category(Traits.Api)]
public sealed class AuthTests : BaseApiTest
{
    [Test]
    [Category(Traits.Smoke)]
    [Category(Traits.Critical)]
    [CaseId("API-001")]
    public async Task ValidCredentials_ReturnAToken()
    {
        var response = await new AuthEndpoint(Client).LoginAsync(
            FrameworkRuntime.Settings.Api.Username,
            FrameworkRuntime.Settings.Api.Password);

        var token = response.ShouldHaveStatus(200);
        await token.ShouldMatchSchemaAsync("auth-token.schema.json");
        token.Body.ShouldNotBeNull();
        token.Body.Username.ShouldBe(FrameworkRuntime.Settings.Api.Username);
        token.Body.AccessToken.Length.ShouldBeGreaterThan(20);
    }

    [Test]
    [Category(Traits.Regression)]
    [CaseId("API-002")]
    public async Task InvalidCredentials_AreRejected()
    {
        var response = await Client.PostAsync<ApiError>("/auth/login", new LoginCredentials
        {
            Username = FrameworkRuntime.Settings.Api.Username,
            Password = "wrong-password",
        });

        var rejected = response.ShouldHaveStatus(400);
        rejected.Body.ShouldNotBeNull();
        rejected.Body!.Message.ShouldContain("Invalid credentials");
    }

    [Test]
    [Category(Traits.Regression)]
    [CaseId("API-003")]
    public async Task MissingToken_CannotReadTheCurrentUser()
    {
        var response = await new AuthEndpoint(Client).MeAsync();
        response.ShouldHaveStatus(401);
    }
}

[TestFixture]
[Parallelizable(ParallelScope.Self)]
[Category(Traits.Api)]
public sealed class ProductTests : BaseApiTest
{
    [Test]
    [Category(Traits.Contract)]
    [Category(Traits.Smoke)]
    [CaseId("API-004")]
    public async Task ProductList_MatchesThePublishedContract()
    {
        var response = await new ProductsEndpoint(Client).ListAsync(limit: 5);
        var page = response.ShouldHaveStatus(200);
        await page.ShouldMatchSchemaAsync("product-list.schema.json");
        page.Body.ShouldNotBeNull();
        page.Body.Products.Count.ShouldBe(5);
        page.Body.Total.ShouldBeGreaterThanOrEqualTo(page.Body.Products.Count);
        page.Body.Products.ShouldAllBe(product => product.Id > 0 && product.Title.Length > 0);
    }

    [Test]
    [Category(Traits.Contract)]
    [Category(Traits.Regression)]
    [CaseId("API-005")]
    public async Task ProductById_MatchesTheProductContract()
    {
        var response = await new ProductsEndpoint(Client).GetAsync(1);
        var product = response.ShouldHaveStatus(200);
        await product.ShouldMatchSchemaAsync("product.schema.json");
        product.Body.ShouldNotBeNull();
        product.Body.Id.ShouldBe(1);
        product.Body.Price.ShouldBeGreaterThan(0);
    }

    [Test]
    [Category(Traits.Regression)]
    [CaseId("API-006")]
    public async Task Search_ReturnsProducts()
    {
        var response = await new ProductsEndpoint(Client).SearchAsync("phone");
        var page = response.ShouldHaveStatus(200);
        page.Body.ShouldNotBeNull();
        page.Body.Products.Count.ShouldBeGreaterThan(0);
        page.Body.Products.ShouldAllBe(product => !string.IsNullOrWhiteSpace(product.Title));
    }

    [Test]
    [Category(Traits.Regression)]
    [CaseId("API-007")]
    public async Task MissingProduct_ReturnsNotFound()
    {
        var response = await new ProductsEndpoint(Client).GetAsync(9_999_999);
        response.ShouldHaveStatus(404);
    }
}

[TestFixture]
[Parallelizable(ParallelScope.Self)]
[Category(Traits.Api)]
public sealed class CartAndProfileTests : BaseApiTest
{
    [Test]
    [Category(Traits.Contract)]
    [Category(Traits.Regression)]
    [CaseId("API-008")]
    public async Task CartList_MatchesThePagingContract()
    {
        var response = await new CartsEndpoint(Client).ListAsync(limit: 2);
        var carts = response.ShouldHaveStatus(200);
        await carts.ShouldMatchSchemaAsync("cart-list.schema.json");
        carts.Body.ShouldNotBeNull();
        carts.Body.Carts.Count.ShouldBe(2);
        carts.Body.Carts.ShouldAllBe(cart => cart.Products.Count > 0);
    }

    [Test]
    [Category(Traits.Smoke)]
    [Category(Traits.Critical)]
    [CaseId("API-009")]
    public async Task AuthenticatedUser_CanReadProfile()
    {
        var auth = new AuthEndpoint(Client);
        var login = await auth.LoginAsync(
            FrameworkRuntime.Settings.Api.Username,
            FrameworkRuntime.Settings.Api.Password);
        var token = login.ShouldHaveStatus(200).Body;
        token.ShouldNotBeNull();

        await using var authenticated = await Factory.CreateAsync(token.AccessToken);
        var me = await new AuthEndpoint(authenticated).MeAsync();
        var profile = me.ShouldHaveStatus(200).Body;
        profile.ShouldNotBeNull();
        profile.Username.ShouldBe(FrameworkRuntime.Settings.Api.Username);
        profile.Email.ShouldContain("@");
    }
}
