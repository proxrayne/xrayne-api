using System.ComponentModel.DataAnnotations;
using System.Net;

namespace XRayne.Api.Requests;

public sealed class UpdatePanelSettingsRequest : IValidatableObject
{
    [BindIpValidation]
    public string? BindIp { get; set; }

    [MaxLength(256)]
    public string? Domain { get; set; }

    [Range(1, 65535)]
    public int Port { get; set; } = 5097;

    [Required]
    [RegularExpression(@"^/$|^/.+/$", ErrorMessage = "WebBasePath must start with '/' and end with '/'.")]
    [MaxLength(256)]
    public string WebBasePath { get; set; } = "/";

    [Range(1, int.MaxValue)]
    public int SessionLifetimeMinutes { get; set; } = 7200;

    [TrustedProxyCidrsValidation]
    [MaxLength(1024)]
    public string? TrustedProxyCidrs { get; set; }

    [MaxLength(1024)]
    public string? CertificatesDirectory { get; set; }

    [MaxLength(1024)]
    public string? GeoResourcesDirectory { get; set; }

    [MaxLength(1024)]
    public string? PanelCertPublicKeyPath { get; set; }

    [MaxLength(1024)]
    public string? PanelCertPrivateKeyPath { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var hasPublicKey = !string.IsNullOrWhiteSpace(PanelCertPublicKeyPath);
        var hasPrivateKey = !string.IsNullOrWhiteSpace(PanelCertPrivateKeyPath);
        if (hasPublicKey != hasPrivateKey)
        {
            yield return new ValidationResult(
                "Panel certificate public and private key paths must be provided together (or both left empty).",
                [nameof(PanelCertPublicKeyPath), nameof(PanelCertPrivateKeyPath)]);
        }
    }
}

[AttributeUsage(AttributeTargets.Property)]
internal sealed class BindIpValidationAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext context)
    {
        if (value is not string raw || string.IsNullOrWhiteSpace(raw))
        {
            return ValidationResult.Success;
        }

        return IPAddress.TryParse(raw, out _)
            ? ValidationResult.Success
            : new ValidationResult($"'{raw}' is not a valid IP address.", [context.MemberName!]);
    }
}

[AttributeUsage(AttributeTargets.Property)]
internal sealed class TrustedProxyCidrsValidationAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext context)
    {
        if (value is not string raw || string.IsNullOrWhiteSpace(raw))
        {
            return ValidationResult.Success;
        }

        foreach (var entry in raw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (!IPNetwork.TryParse(entry, out _))
            {
                return new ValidationResult($"'{entry}' is not a valid CIDR.", [context.MemberName!]);
            }
        }

        return ValidationResult.Success;
    }
}
