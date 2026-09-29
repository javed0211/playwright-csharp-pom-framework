using Microsoft.Playwright;
using PlaywrightPom.Web.Flows;

namespace PlaywrightPom.Web.Pages;

public sealed class CheckoutInformationPage : BasePage
{
    public CheckoutInformationPage(IPage page)
        : base(page)
    {
    }

    public override string RelativePath => "/checkout-step-one.html";

    public ILocator FirstName => Page.Locator("#first-name");

    public ILocator LastName => Page.Locator("#last-name");

    public ILocator PostalCode => Page.Locator("#postal-code");

    public async Task ContinueAsync(Customer customer)
    {
        await FillAsync(FirstName, customer.FirstName, $"First name {customer.FirstName}").ConfigureAwait(false);
        await FillAsync(LastName, customer.LastName, $"Last name {customer.LastName}").ConfigureAwait(false);
        await FillAsync(PostalCode, customer.PostalCode, "Postal code entered").ConfigureAwait(false);
        await ClickAsync(Page.Locator("#continue"), "Continue to overview").ConfigureAwait(false);
    }
}
