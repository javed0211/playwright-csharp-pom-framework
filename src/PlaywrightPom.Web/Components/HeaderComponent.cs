using Microsoft.Playwright;
using PlaywrightPom.Core.Diagnostics;

namespace PlaywrightPom.Web.Components;

public sealed class HeaderComponent
{
    private readonly IPage _page;

    public HeaderComponent(IPage page) => _page = page;

    public ILocator CartLink => _page.Locator(".shopping_cart_link");

    public ILocator Badge => _page.Locator(".shopping_cart_badge");

    public Task OpenCartAsync() =>
        TestSession.Steps.StepAsync("Open cart", () => CartLink.ClickAsync());
}
