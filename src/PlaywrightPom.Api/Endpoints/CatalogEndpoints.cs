using PlaywrightPom.Api.Client;
using PlaywrightPom.Api.Models;

namespace PlaywrightPom.Api.Endpoints;

public sealed class AuthEndpoint
{
    private readonly ApiClient _client;

    public AuthEndpoint(ApiClient client) => _client = client;

    public Task<ApiResponse<AuthToken>> LoginAsync(string username, string password) =>
        _client.PostAsync<AuthToken>("/auth/login", new LoginCredentials
        {
            Username = username,
            Password = password,
            ExpiresInMins = 30,
        });

    public Task<ApiResponse<UserProfile>> MeAsync() => _client.GetAsync<UserProfile>("/auth/me");
}

public sealed class ProductsEndpoint
{
    private readonly ApiClient _client;

    public ProductsEndpoint(ApiClient client) => _client = client;

    public Task<ApiResponse<ProductList>> ListAsync(int limit = 10, int skip = 0) =>
        _client.GetAsync<ProductList>($"/products?limit={limit}&skip={skip}");

    public Task<ApiResponse<Product>> GetAsync(int id) => _client.GetAsync<Product>($"/products/{id}");

    public Task<ApiResponse<ProductList>> SearchAsync(string query) =>
        _client.GetAsync<ProductList>($"/products/search?q={Uri.EscapeDataString(query)}");
}

public sealed class CartsEndpoint
{
    private readonly ApiClient _client;

    public CartsEndpoint(ApiClient client) => _client = client;

    public Task<ApiResponse<CartList>> ListAsync(int limit = 5) =>
        _client.GetAsync<CartList>($"/carts?limit={limit}");
}
