using Microsoft.Playwright;

namespace PlaywrightPom.Web.Components;

public abstract class UiComponent
{
    protected UiComponent(ILocator root) => Root = root;

    protected ILocator Root { get; }
}
