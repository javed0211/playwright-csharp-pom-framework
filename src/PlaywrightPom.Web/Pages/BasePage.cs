using Microsoft.Playwright;
using PlaywrightPom.Core.Diagnostics;

namespace PlaywrightPom.Web.Pages;

public abstract class BasePage
{
    protected BasePage(IPage page) => Page = page;

    protected IPage Page { get; }

    public abstract string RelativePath { get; }

    public Task OpenAsync() =>
        TestSession.Steps.StepAsync($"Open {RelativePath}", () =>
            Page.GotoAsync(RelativePath, new PageGotoOptions
            {
                WaitUntil = WaitUntilState.DOMContentLoaded,
            }));

    protected Task ClickAsync(ILocator locator, string step) =>
        TestSession.Steps.StepAsync(step, () => locator.ClickAsync());

    protected Task FillAsync(ILocator locator, string value, string step) =>
        TestSession.Steps.StepAsync(step, () => locator.FillAsync(value));
}
