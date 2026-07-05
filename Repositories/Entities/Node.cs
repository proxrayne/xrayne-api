using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Contracts.Enums;
using Contracts.Values;
using Xray.Config.Models;

namespace Repositories.Entities;

[Table("Nodes")]
public class NodeEntity : CreateUpdateEntity
{
    /// <summary>
    /// Gets or sets the database identifier of the remote node.
    /// </summary>
    [Key]
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets the human-readable node name shown in the panel.
    /// </summary>
    [MaxLength(64)]
    public required string Name { get; set; }

    /// <summary>
    /// Gets or sets the public domain name or IP address used to reach the node API.
    /// </summary>
    [MaxLength(64)]
    public required string Address { get; set; }

    /// <summary>
    /// Gets or sets the SSH username used during remote provisioning.
    /// </summary>
    [MaxLength(64)]
    public required string SSHUsername { get; set; }

    /// <summary>
    /// Gets or sets the SSH port used during remote provisioning.
    /// </summary>
    public int Port { get; set; }

    /// <summary>
    /// Gets or sets the HTTPS API port exposed by the remote node service.
    /// </summary>
    public int ApiPort { get; set; }

    /// <summary>
    /// Gets or sets the encrypted API key used by the panel to authenticate with the node.
    /// </summary>
    [MaxLength(2048)]
    public required string EncryptedApiKey { get; set; }

    /// <summary>
    /// Gets or sets a non-secret fingerprint of the node API key for diagnostics.
    /// </summary>
    [MaxLength(64)]
    public required string ApiKeyFingerprint { get; set; }

    /// <summary>
    /// Gets or sets the SSH private key used for provisioning when key authentication is selected.
    /// </summary>
    [MaxLength(512)]
    public string? SSHKey { get; set; }

    /// <summary>
    /// Gets or sets the SSH password used for provisioning when password authentication is selected.
    /// </summary>
    [MaxLength(256)]
    public string? Password { get; set; }

    /// <summary>
    /// Gets or sets the remote working directory where XRayne node files are installed.
    /// </summary>
    [MaxLength(256)]
    public required string WorkingDirectory { get; set; }

    /// <summary>
    /// Gets or sets an operator note for the node.
    /// </summary>
    [MaxLength(512)]
    public string Note { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the base xray-core configuration template used for remote starts.
    /// </summary>
    [Column(TypeName = "jsonb")]
    public XrayConfig ConfigTemplate { get; set; } = NodeConfigTemplateDefaults.Create();

    /// <summary>
    /// Gets or sets the certificate mode, such as domain or IP.
    /// </summary>
    public CertificateMode CertificateMode { get; set; } = CertificateMode.Domain;

    /// <summary>
    /// Gets or sets when the node status last changed.
    /// </summary>
    public DateTime LastStatusChange { get; set; }

    /// <summary>
    /// Gets or sets when the panel last received a successful ping or stream heartbeat.
    /// </summary>
    public DateTimeOffset? LastSeenAt { get; set; }

    /// <summary>
    /// Gets or sets when the panel last established a successful live connection.
    /// </summary>
    public DateTimeOffset? ConnectedAt { get; set; }

    /// <summary>
    /// Gets or sets the number of automatic reconnect attempts in the current failure cycle.
    /// </summary>
    public int ReconnectAttemptCount { get; set; }

    /// <summary>
    /// Gets or sets the current user-facing node connection status.
    /// </summary>
    public NodeStatus Status { get; set; } = NodeStatus.Connecting;

    /// <summary>
    /// Gets or sets the SSH authentication method used for provisioning.
    /// </summary>
    public SSHAuthType AuthType { get; set; } = SSHAuthType.Password;

    /// <summary>
    /// Gets or sets the latest connection or provisioning message.
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Gets or sets the latest remote provisioning progress or failure message.
    /// </summary>
    [MaxLength(1024)]
    public string InstallationMessage { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the administrator that created the node.
    /// </summary>
    public AdminAccount Admin { get; set; } = null!;

    /// <summary>
    /// Gets or sets inbound configurations assigned to the node.
    /// </summary>
    public List<InboundEntity> Inbounds { get; set; } = new();

    /// <summary>
    /// Gets or sets outbound configurations assigned to the node.
    /// </summary>
    public List<OutboundEntity> Outbounds { get; set; } = new();

    /// <summary>
    /// Gets or sets routing rules assigned to the node.
    /// </summary>
    public List<RoutingRuleEntity> RoutingRules { get; set; } = new();

    /// <summary>
    /// Gets or sets TLS certificates assigned to the node.
    /// </summary>
    public List<CertificateEntity> Certificates { get; set; } = new();

    /// <summary>
    /// Gets or sets geo resources assigned to the node.
    /// </summary>
    public List<GeoResourceEntity> GeoResources { get; set; } = new();
}
