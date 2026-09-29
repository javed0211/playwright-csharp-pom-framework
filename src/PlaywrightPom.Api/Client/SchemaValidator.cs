using NJsonSchema;

namespace PlaywrightPom.Api.Client;

public static class SchemaValidator
{
    public static async Task EnsureValidAsync(string json, string schemaPath)
    {
        if (!File.Exists(schemaPath))
        {
            throw new FileNotFoundException($"JSON schema was not found at '{schemaPath}'.", schemaPath);
        }

        var schema = await JsonSchema.FromFileAsync(schemaPath).ConfigureAwait(false);
        var errors = schema.Validate(json);
        if (errors.Count == 0)
        {
            return;
        }

        var details = string.Join(
            Environment.NewLine,
            errors.Take(12).Select(error => $"{error.Path}: {error.Kind}"));
        throw new ApiAssertionException($"Response did not match schema '{Path.GetFileName(schemaPath)}'.{Environment.NewLine}{details}");
    }
}
