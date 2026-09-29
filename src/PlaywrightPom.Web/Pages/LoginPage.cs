using Microsoft.Playwright;
using PlaywrightPom.Core.Diagnostics;

namespace PlaywrightPom.Web.Pages;

public sealed class LoginPage : BasePage
{
    public LoginPage(IPage page)
        : base(page)
    {
    }

    public override string RelativePath => "/";

    public ILocator UserName => Page.Locator("#user-name");

    public ILocator Password => Page.Locator("#password");

    public ILocator LoginButton => Page.Locator("#login-button");

    public ILocator Error => Page.Locator("[data-test='error']");

    public Task SignInAsync(string username, string password) =>
        TestSession.Steps.StepAsync($"Sign in as {username}", async () =>
        {
            await UserName.FillAsync(username).ConfigureAwait(false);
            await Password.FillAsync(password).ConfigureAwait(false);
            await LoginButton.ClickAsync().ConfigureAwait(false);
        });

    public async Task<string> ReadErrorAsync()
    {
        await Assertions.Expect(Error).ToBeVisibleAsync().ConfigureAwait(false);
        return (await Error.InnerTextAsync().ConfigureAwait(false)).Trim();
    }
}
