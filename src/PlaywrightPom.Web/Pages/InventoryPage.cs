using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.Playwright;
using PlaywrightPom.Core.Diagnostics;
using PlaywrightPom.Web.Components;

namespace PlaywrightPom.Web.Pages;

public sealed class InventoryPage : BasePage
{
    public InventoryPage(IPage page)
        : base(page)
    {
    }

    public override string RelativePath => "/inventory.html";

    public HeaderComponent Header => new(Page);

    public ILocator Item(string productName) =>
        Page.Locator(".inventory_item").Filter(new() { HasText = productName });

    public async Task ExpectLoadedAsync()
    {
        await Assertions.Expect(Page).ToHaveURLAsync(new Regex("inventory\\.html")).ConfigureAwait(false);
        await Assertions.Expect(Page.Locator(".title")).ToHaveTextAsync("Products").ConfigureAwait(false);
    }

    public Task AddAsync(string productName) =>
        TestSession.Steps.StepAsync($"Add {productName}", () => new InventoryItemCard(Item(productName)).AddToCartAsync());

    public Task SortByPriceLowToHighAsync() =>
        TestSession.Steps.StepAsync("Sort by price, low to high", () =>
            Page.Locator("[data-test='product-sort-container']").SelectOptionAsync("lohi"));

    public async Task<IReadOnlyList<decimal>> PricesAsync()
    {
        var texts = await Page.Locator(".inventory_item_price").AllInnerTextsAsync().ConfigureAwait(false);
        return texts.Select(text => decimal.Parse(text.Replace("$", "", StringComparison.Ordinal), CultureInfo.InvariantCulture)).ToArray();
    }
}
