namespace XRayne.Contracts.Configurations;

[AttributeUsage(AttributeTargets.Property)]
public sealed class RestartImpactAttribute(RestartImpact impact) : Attribute
{
    public RestartImpact Impact { get; } = impact;
}
