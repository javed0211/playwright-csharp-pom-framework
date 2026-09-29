using Microsoft.Playwright;
using PlaywrightPom.Core.Diagnostics;
using PlaywrightPom.Web.Components;

namespace PlaywrightPom.Web.Pages;

public sealed class CartPage : BasePage
{
    public CartPage(IPage page)
        : base(page)
    {
    }

    public override string RelativePath => "/cart.html";

    public ILocator Items => Page.Locator(".cart_item");

    public ILocator Item(string productName) => Items.Filter(new() { HasText = productName });

    public async Task ExpectLoadedAsync() =>
        await Assertions.Expect(Page.Locator(".title")).ToHaveTextAsync("Your Cart").ConfigureAwait(false);

    public async Task ExpectCountAsync(int count) =>
        await Assertions.Expect(Items).ToHaveCountAsync(count).ConfigureAwait(false);

    public Task RemoveAsync(string productName) =>
        TestSession.Steps.StepAsync($"Remove {productName}", () => new InventoryItemCard(Item(productName)).RemoveAsync());

    public Task CheckoutAsync() => ClickAsync(Page.Locator("#checkout"), "Checkout");
}
