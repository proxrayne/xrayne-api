using Xray.Config.Enums;

namespace Data.Entities;

public sealed class ClientOption
{
    public Guid? Uuid { get; set; }

    public XtlsFlow? Flow { get; set; }

    public string? Password { get; set; }

    public EncryptionMethod? Method { get; set; }
}
