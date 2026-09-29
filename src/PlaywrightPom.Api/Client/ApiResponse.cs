namespace PlaywrightPom.Api.Client;

public sealed class ApiResponse<T>
{
    public required int Status { get; init; }

    public required string Raw { get; init; }

    public T? Body { get; init; }

    public required IReadOnlyDictionary<string, string> Headers { get; init; }

    public bool IsSuccess => Status is >= 200 and < 300;

    public ApiResponse<T> ShouldHaveStatus(int status)
    {
        if (Status != status)
        {
            throw new ApiAssertionException(
                $"Expected HTTP {status} but received {Status}. Body: {Truncate(Raw)}");
        }

        return this;
    }

    public async Task<ApiResponse<T>> ShouldMatchSchemaAsync(string schemaFileName)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "contracts", schemaFileName);
        await SchemaValidator.EnsureValidAsync(Raw, path).ConfigureAwait(false);
        return this;
    }

    private static string Truncate(string value) =>
        value.Length <= 800 ? value : value[..800] + "...";
}

public sealed class ApiAssertionException : Exception
{
    public ApiAssertionException(string message)
        : base(message)
    {
    }
}
