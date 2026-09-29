using Microsoft.Playwright;

namespace PlaywrightPom.Web.Pages;

public sealed class CheckoutCompletePage : BasePage
{
    public CheckoutCompletePage(IPage page)
        : base(page)
    {
    }

    public override string RelativePath => "/checkout-complete.html";

    public ILocator Header => Page.Locator(".complete-header");

    public async Task<string> ConfirmationAsync()
    {
        await Assertions.Expect(Header).ToBeVisibleAsync().ConfigureAwait(false);
        return (await Header.InnerTextAsync().ConfigureAwait(false)).Trim();
    }
}
