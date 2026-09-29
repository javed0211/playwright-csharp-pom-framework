using Microsoft.Playwright;
using PlaywrightPom.Web.Pages;

namespace PlaywrightPom.Web.Flows;

public sealed class PurchaseFlow
{
    private readonly IPage _page;

    public PurchaseFlow(IPage page) => _page = page;

    public async Task<string> PurchaseAsync(
        string username,
        string password,
        IReadOnlyList<string> products,
        Customer customer)
    {
        var login = new LoginPage(_page);
        await login.OpenAsync().ConfigureAwait(false);
        await login.SignInAsync(username, password).ConfigureAwait(false);

        var inventory = new InventoryPage(_page);
        await inventory.ExpectLoadedAsync().ConfigureAwait(false);
        foreach (var product in products)
        {
            await inventory.AddAsync(product).ConfigureAwait(false);
        }

        await Assertions.Expect(inventory.Header.Badge).ToHaveTextAsync(products.Count.ToString()).ConfigureAwait(false);
        await inventory.Header.OpenCartAsync().ConfigureAwait(false);

        var cart = new CartPage(_page);
        await cart.ExpectLoadedAsync().ConfigureAwait(false);
        await cart.ExpectCountAsync(products.Count).ConfigureAwait(false);
        await cart.CheckoutAsync().ConfigureAwait(false);

        var information = new CheckoutInformationPage(_page);
        await information.ContinueAsync(customer).ConfigureAwait(false);

        var overview = new CheckoutOverviewPage(_page);
        foreach (var product in products)
        {
            await overview.ExpectItemAsync(product).ConfigureAwait(false);
        }

        await overview.FinishAsync().ConfigureAwait(false);
        return await new CheckoutCompletePage(_page).ConfirmationAsync().ConfigureAwait(false);
    }
}
