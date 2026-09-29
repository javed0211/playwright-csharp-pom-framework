using Microsoft.Playwright;

namespace PlaywrightPom.Web.Pages;

public sealed class CheckoutOverviewPage : BasePage
{
    public CheckoutOverviewPage(IPage page)
        : base(page)
    {
    }

    public override string RelativePath => "/checkout-step-two.html";

    public ILocator ItemNames => Page.Locator(".inventory_item_name");

    public async Task ExpectItemAsync(string productName) =>
        await Assertions.Expect(ItemNames.Filter(new() { HasText = productName })).ToHaveCountAsync(1).ConfigureAwait(false);

    public Task FinishAsync() => ClickAsync(Page.Locator("#finish"), "Finish checkout");
}
