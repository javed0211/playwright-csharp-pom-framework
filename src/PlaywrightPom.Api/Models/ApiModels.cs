namespace PlaywrightPom.Api.Models;

public sealed class LoginCredentials
{
    public string Username { get; set; } = "";

    public string Password { get; set; } = "";

    public int ExpiresInMins { get; set; } = 30;
}

public sealed class AuthToken
{
    public int Id { get; set; }

    public string Username { get; set; } = "";

    public string Email { get; set; } = "";

    public string AccessToken { get; set; } = "";

    public string RefreshToken { get; set; } = "";
}

public sealed class ApiError
{
    public string Message { get; set; } = "";
}

public sealed class UserProfile
{
    public int Id { get; set; }

    public string Username { get; set; } = "";

    public string Email { get; set; } = "";

    public string FirstName { get; set; } = "";

    public string LastName { get; set; } = "";
}

public sealed class Product
{
    public int Id { get; set; }

    public string Title { get; set; } = "";

    public string Description { get; set; } = "";

    public string Category { get; set; } = "";

    public decimal Price { get; set; }

    public double DiscountPercentage { get; set; }

    public double Rating { get; set; }

    public int Stock { get; set; }

    public string? Brand { get; set; }

    public string? Sku { get; set; }
}

public sealed class ProductList
{
    public List<Product> Products { get; set; } = [];

    public int Total { get; set; }

    public int Skip { get; set; }

    public int Limit { get; set; }
}

public sealed class CartLine
{
    public int Id { get; set; }

    public string Title { get; set; } = "";

    public decimal Price { get; set; }

    public int Quantity { get; set; }

    public decimal Total { get; set; }
}

public sealed class Cart
{
    public int Id { get; set; }

    public List<CartLine> Products { get; set; } = [];

    public decimal DiscountedTotal { get; set; }
}

public sealed class CartList
{
    public List<Cart> Carts { get; set; } = [];

    public int Total { get; set; }

    public int Skip { get; set; }

    public int Limit { get; set; }
}
