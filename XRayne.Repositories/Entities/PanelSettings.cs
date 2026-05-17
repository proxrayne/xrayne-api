using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace XRayne.Repositories.Entities;

[Table("panel_settings")]
public sealed class PanelSettings
{
    // Фиксированный Id — таблица singleton, PK гарантирует уникальность
    // и закрывает гонку "проверь-и-вставь" при первом обращении.
    public static readonly Guid SingletonId = new("00000000-0000-0000-0000-000000000001");

    [Key]
    public Guid Id { get; set; } = SingletonId;

    [MaxLength(64)]
    public string? BindIp { get; set; }

    [MaxLength(256)]
    public string? Domain { get; set; }

    public int Port { get; set; } = 5097;

    [Required]
    [MaxLength(256)]
    public string WebBasePath { get; set; } = "/";

    public int SessionLifetimeMinutes { get; set; } = 7200;

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

    public bool PendingRestart { get; set; }

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
