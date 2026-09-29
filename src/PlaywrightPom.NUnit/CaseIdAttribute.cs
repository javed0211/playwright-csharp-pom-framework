namespace PlaywrightPom.NUnit;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
public sealed class CaseIdAttribute : global::NUnit.Framework.PropertyAttribute
{
    public CaseIdAttribute(string id)
        : base(id)
    {
    }
}
