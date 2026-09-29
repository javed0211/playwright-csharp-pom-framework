using Microsoft.Playwright;

namespace PlaywrightPom.Web.Components;

public sealed class InventoryItemCard : UiComponent
{
    public InventoryItemCard(ILocator root)
        : base(root)
    {
    }

    public ILocator Name => Root.Locator(".inventory_item_name");

    public ILocator Price => Root.Locator(".inventory_item_price");

    public ILocator AddButton => Root.GetByRole(AriaRole.Button, new() { Name = "Add to cart" });

    public ILocator RemoveButton => Root.GetByRole(AriaRole.Button, new() { Name = "Remove" });

    public Task AddToCartAsync() => AddButton.ClickAsync();

    public Task RemoveAsync() => RemoveButton.ClickAsync();
}
