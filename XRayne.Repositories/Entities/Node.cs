using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using XRayne.Contracts.Enums;

namespace XRayne.Repositories.Entities;

[Table("nodes")]
public class NodeEntity : CreateUpdateEntity
{
    [Key]
    public long Id { get; set; }

    [MaxLength(64)]
    public required string Name { get; set; }

    [MaxLength(64)]
    public required string Address { get; set; }

    public int Port { get; set; }

    public int ApiPort { get; set; }

    [MaxLength(512)]
    public string? SSHKey { get; set; }

    [MaxLength(256)]
    public string? Password { get; set; }

    [MaxLength(256)]
    public required string WorkingDirectory { get; set; }

    [MaxLength(512)]
    public string Note { get; set; } = string.Empty;

    [MaxLength(24)]
    public string? XrayVersion { get; set; }

    public DateTime LastStatusChange { get; set; }

    public NodeStatus Status { get; set; } = NodeStatus.Connecting;

    public SSHAuthType AuthType { get; set; } = SSHAuthType.Password;

    public string? Message { get; set; }

    // relation tables
    public AdminAccount Admin { get; set; } = null!;

    public List<InboundEntity> Inbounds { get; set; } = new();

    public List<OutboundEntity> Outbounds { get; set; } = new();

    public List<RoutingRuleEntity> RoutingRules { get; set; } = new();

    public List<CertificateEntity> Certificates { get; set; } = new();

    public List<GeoResourceEntity> GeoResources { get; set; } = new();
}
