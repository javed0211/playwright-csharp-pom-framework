namespace PlaywrightPom.Core.Assertions;

public sealed class SoftAssert
{
    private readonly List<string> _failures = [];

    public IReadOnlyList<string> Failures => _failures;

    public void Check(bool condition, string message)
    {
        if (!condition)
        {
            _failures.Add(message);
        }
    }

    public void AssertAll()
    {
        if (_failures.Count == 0)
        {
            return;
        }

        throw new AssertionAggregateException(_failures);
    }
}

public sealed class AssertionAggregateException : Exception
{
    public AssertionAggregateException(IReadOnlyList<string> failures)
        : base(string.Join(Environment.NewLine, failures))
    {
        Failures = failures;
    }

    public IReadOnlyList<string> Failures { get; }
}
