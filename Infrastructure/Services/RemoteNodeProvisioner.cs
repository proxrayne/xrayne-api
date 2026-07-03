using System.Globalization;
using System.Text;
using Microsoft.Extensions.Options;
using Renci.SshNet;
using Renci.SshNet.Common;
using Contracts.Configurations;
using Contracts.Enums;
using Infrastructure.States;
using Repositories.Entities;

namespace Infrastructure.Services;

/// <summary>
/// Provisions a remote Linux host over SSH and starts the Node container.
/// </summary>
public sealed class RemoteNodeProvisioner(
    INodeProvisionStateMachine stateMachine,
    INodeImageReleaseResolver imageReleaseResolver,
    INodeConnectionVerifier connectionVerifier,
    IOptions<NodeConnectionOptions> connectionOptions) : IRemoteNodeProvisioner
{
    public async Task<RemoteNodeProvisionResult> ProvisionAsync(
        NodeEntity node,
        string apiKey,
        string jobId,
        CancellationToken cancellationToken)
    {
        using var ssh = CreateSshClient(node);
        using var scp = CreateScpClient(node);

        await ConnectAsync(ssh, cancellationToken);
        await ConnectAsync(scp, cancellationToken);

        var installScriptPath = $"/tmp/xrayne-node-install-{jobId}.sh";
        var configPath = $"/tmp/xrayne-node-install-{jobId}.env";
        var image = await imageReleaseResolver.ResolveAsync(cancellationToken);

        stateMachine.Dispatch(jobId, NodeProvisionState.Uploading(node.Id, jobId));
        await UploadTextAsync(scp, installScriptPath, RemoteNodeInstallerScript.Content, cancellationToken);
        await UploadTextAsync(scp, configPath, RemoteNodeInstallerScript.RenderConfig(node, apiKey, image, connectionOptions.Value), cancellationToken);
        await RunAsync(ssh, $"chmod 700 {Quote(installScriptPath)} && chmod 600 {Quote(configPath)}", cancellationToken);

        stateMachine.Dispatch(jobId, NodeProvisionState.InstallingDependencies(node.Id, jobId));
        stateMachine.Dispatch(jobId, NodeProvisionState.DownloadingImage(node.Id, jobId));
        stateMachine.Dispatch(jobId, NodeProvisionState.ConfiguringCertificate(node.Id, jobId));
        stateMachine.Dispatch(jobId, NodeProvisionState.StartingContainer(node.Id, jobId));

        await RunAsync(
            ssh,
            $"sh {Quote(installScriptPath)} {Quote(configPath)}",
            cancellationToken);

        stateMachine.Dispatch(jobId, NodeProvisionState.Verifying(node.Id, jobId));

        var result = await connectionVerifier.VerifyAsync(node, apiKey, cancellationToken);

        return new RemoteNodeProvisionResult(result.XrayVersion, result.VerifiedAt);
    }

    private static SshClient CreateSshClient(NodeEntity node)
        => new(CreateConnectionInfo(node));

    private static ScpClient CreateScpClient(NodeEntity node)
        => new(CreateConnectionInfo(node));

    private static ConnectionInfo CreateConnectionInfo(NodeEntity node)
    {
        return node.AuthType switch
        {
            SSHAuthType.Password => new PasswordConnectionInfo(
                node.Address,
                node.Port,
                node.SSHUsername,
                node.Password ?? throw new InvalidOperationException("SSH password is not configured.")),
            SSHAuthType.PrivateKey => new PrivateKeyConnectionInfo(
                node.Address,
                node.Port,
                node.SSHUsername,
                CreatePrivateKeyFile(node)),
            _ => throw new InvalidOperationException($"Unsupported SSH authentication type '{node.AuthType}'.")
        };
    }

    private static PrivateKeyFile CreatePrivateKeyFile(NodeEntity node)
    {
        if (string.IsNullOrWhiteSpace(node.SSHKey))
        {
            throw new InvalidOperationException("SSH private key is not configured.");
        }

        return new PrivateKeyFile(new MemoryStream(Encoding.UTF8.GetBytes(node.SSHKey)));
    }

    private static async Task ConnectAsync(BaseClient client, CancellationToken cancellationToken)
    {
        await Task.Run(client.Connect, cancellationToken);

        if (!client.IsConnected)
        {
            throw new SshConnectionException("SSH connection was not established.");
        }
    }

    private static async Task UploadTextAsync(
        ScpClient client,
        string path,
        string content,
        CancellationToken cancellationToken)
    {
        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));

        await Task.Run(() => client.Upload(stream, path), cancellationToken);
    }

    private static async Task<string> RunAsync(
        SshClient client,
        string commandText,
        CancellationToken cancellationToken)
    {
        return await Task.Run(() =>
        {
            using var command = client.CreateCommand(commandText);
            var output = command.Execute();
            var error = command.Error;
            if (command.ExitStatus == 0)
            {
                return string.Join(Environment.NewLine, output, error).Trim();
            }

            throw new InvalidOperationException(
                $"Remote command failed with exit code {command.ExitStatus}.{Environment.NewLine}{output}{Environment.NewLine}{error}".Trim());
        }, cancellationToken);
    }

    private static string Quote(string value)
        => $"'{value.Replace("'", "'\"'\"'", StringComparison.Ordinal)}'";
}

/// <summary>
/// Describes a successful remote node provisioning verification result.
/// </summary>
public sealed record RemoteNodeProvisionResult(string? XrayVersion, DateTimeOffset VerifiedAt);

internal static class RemoteNodeInstallerScript
{
    public static string RenderConfig(
        NodeEntity node,
        string apiKey,
        NodeImageReleaseAsset image,
        NodeConnectionOptions options)
    {
        var values = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            ["CERTIFICATE_EMAIL"] = node.Admin.Email ?? string.Empty,
            ["CERTIFICATE_IDENTIFIER"] = node.Address,
            ["CERTIFICATE_MODE"] = node.CertificateMode == CertificateMode.Ip ? "ip" : "domain",
            ["NODE_API_KEY"] = apiKey,
            ["NODE_API_PORT"] = node.ApiPort.ToString(CultureInfo.InvariantCulture),
            ["NODE_IMAGE_DOWNLOAD_URL"] = image.DownloadUrl,
            ["NODE_IMAGE_TAG"] = image.ImageTag,
            ["NODE_STREAM_HEARTBEAT_SECONDS"] = Math.Max(1, options.StreamHeartbeatSeconds).ToString(CultureInfo.InvariantCulture),
            ["WORKING_DIRECTORY"] = node.WorkingDirectory,
        };

        return string.Join(
            "\n",
            values.Select(item => $"{item.Key}={ShellQuote(item.Value)}")) + "\n";
    }

    private static string ShellQuote(string value)
        => $"'{value.Replace("'", "'\"'\"'", StringComparison.Ordinal)}'";

    public const string Content = """
#!/bin/sh
set -eu

CONFIG_PATH="${1:-}"
if [ -z "$CONFIG_PATH" ] || [ ! -f "$CONFIG_PATH" ]; then
  echo "Installer config file is required." >&2
  exit 1
fi

set -a
. "$CONFIG_PATH"
set +a
rm -f "$CONFIG_PATH"

if [ "$(uname -s)" != "Linux" ]; then
  echo "Only Linux remote hosts are supported." >&2
  exit 1
fi

if [ "$(id -u)" -eq 0 ]; then
  SUDO=""
elif command -v sudo >/dev/null 2>&1 && sudo -n true >/dev/null 2>&1; then
  SUDO="sudo -n"
else
  echo "SSH user must be root or have passwordless sudo." >&2
  exit 1
fi

run() {
  echo "xrayne-node: $*"
  # shellcheck disable=SC2086
  $SUDO "$@"
}

ensure_working_directory() {
  run mkdir -p "$WORKING_DIRECTORY/downloads" "$WORKING_DIRECTORY/logs" "$WORKING_DIRECTORY/certificates"
  run chown -R "$(id -u):$(id -g)" "$WORKING_DIRECTORY"
}

ensure_downloader() {
  if command -v curl >/dev/null 2>&1 || command -v wget >/dev/null 2>&1; then
    return
  fi

  if command -v apt-get >/dev/null 2>&1; then
    run apt-get update
    run apt-get install -y curl ca-certificates
  elif command -v dnf >/dev/null 2>&1; then
    run dnf install -y curl ca-certificates
  elif command -v yum >/dev/null 2>&1; then
    run yum install -y curl ca-certificates
  else
    echo "Unsupported package manager. Expected apt, dnf, or yum." >&2
    exit 1
  fi
}

ensure_docker() {
  if command -v docker >/dev/null 2>&1 && docker compose version >/dev/null 2>&1; then
    return
  fi

  if command -v apt-get >/dev/null 2>&1; then
    run apt-get update
    run apt-get install -y ca-certificates curl gnupg docker.io docker-compose-plugin
  elif command -v dnf >/dev/null 2>&1; then
    run dnf install -y ca-certificates curl docker docker-compose-plugin
    run systemctl enable --now docker
  elif command -v yum >/dev/null 2>&1; then
    run yum install -y ca-certificates curl docker docker-compose-plugin
    run systemctl enable --now docker
  else
    echo "Unsupported package manager. Expected apt, dnf, or yum." >&2
    exit 1
  fi

  run docker version >/dev/null
}

download_file() {
  URL="$1"
  DESTINATION="$2"
  if command -v curl >/dev/null 2>&1; then
    curl -fsSL "$URL" -o "$DESTINATION"
  else
    wget -q "$URL" -O "$DESTINATION"
  fi
}

ensure_acme() {
  ACME_HOME="$WORKING_DIRECTORY/certificates/acme-sh"
  ACME_SCRIPT="$ACME_HOME/acme.sh"
  if [ ! -f "$ACME_SCRIPT" ]; then
    run mkdir -p "$ACME_HOME"
    TMP_ACME="/tmp/xrayne-acme.sh"
    download_file "https://raw.githubusercontent.com/acmesh-official/acme.sh/master/acme.sh" "$TMP_ACME"
    run mv "$TMP_ACME" "$ACME_SCRIPT"
    run chmod +x "$ACME_SCRIPT"
  fi
}

issue_certificate() {
  CERT_NAME="$CERTIFICATE_MODE-$CERTIFICATE_IDENTIFIER"
  CERT_DIR="$WORKING_DIRECTORY/certificates/letsencrypt/$CERT_NAME"
  FULLCHAIN_PATH="$CERT_DIR/fullchain.pem"
  PRIVATE_KEY_PATH="$CERT_DIR/privkey.pem"

  run mkdir -p "$CERT_DIR"
  ensure_acme

  ACME_ARGS="--home $WORKING_DIRECTORY/certificates/acme-sh --config-home $WORKING_DIRECTORY/certificates/acme-config --cert-home $WORKING_DIRECTORY/certificates/acme-certs --server letsencrypt --issue -d $CERTIFICATE_IDENTIFIER --standalone --keylength ec-256"
  if [ -n "$CERTIFICATE_EMAIL" ]; then
    ACME_ARGS="$ACME_ARGS --accountemail $CERTIFICATE_EMAIL"
  fi
  if [ "$CERTIFICATE_MODE" = "ip" ]; then
    ACME_ARGS="$ACME_ARGS --cert-profile shortlived --days 6"
  fi

  # shellcheck disable=SC2086
  run sh "$WORKING_DIRECTORY/certificates/acme-sh/acme.sh" $ACME_ARGS
  run sh "$WORKING_DIRECTORY/certificates/acme-sh/acme.sh" \
    --home "$WORKING_DIRECTORY/certificates/acme-sh" \
    --config-home "$WORKING_DIRECTORY/certificates/acme-config" \
    --cert-home "$WORKING_DIRECTORY/certificates/acme-certs" \
    --install-cert \
    -d "$CERTIFICATE_IDENTIFIER" \
    --key-file "$PRIVATE_KEY_PATH" \
    --fullchain-file "$FULLCHAIN_PATH"
}

write_runtime_files() {
  ENV_PATH="$WORKING_DIRECTORY/.env"
  COMPOSE_PATH="$WORKING_DIRECTORY/docker-compose.yml"
  CERT_NAME="$CERTIFICATE_MODE-$CERTIFICATE_IDENTIFIER"
  CERT_DIR="$WORKING_DIRECTORY/certificates/letsencrypt/$CERT_NAME"

  ensure_working_directory

  umask 077
  cat > "$ENV_PATH" <<EOF_ENV
NODE_API_KEY=$NODE_API_KEY
NODE_API_PORT=$NODE_API_PORT
NODE_IMAGE=xrayne-node:$NODE_IMAGE_TAG
NODE_STREAM_HEARTBEAT_SECONDS=$NODE_STREAM_HEARTBEAT_SECONDS
CERT_FULLCHAIN_PATH=/app/certificates/letsencrypt/$CERT_NAME/fullchain.pem
CERT_PRIVATE_KEY_PATH=/app/certificates/letsencrypt/$CERT_NAME/privkey.pem
EOF_ENV

  cat > "$COMPOSE_PATH" <<'EOF_COMPOSE'
services:
  node:
    image: ${NODE_IMAGE}
    container_name: xrayne-node
    restart: unless-stopped
    env_file:
      - .env
    environment:
      ASPNETCORE_URLS: https://+:${NODE_API_PORT}
      Node__ApiKey: ${NODE_API_KEY}
      Node__StreamHeartbeatSeconds: ${NODE_STREAM_HEARTBEAT_SECONDS}
      Kestrel__Certificates__Default__Path: ${CERT_FULLCHAIN_PATH}
      Kestrel__Certificates__Default__KeyPath: ${CERT_PRIVATE_KEY_PATH}
    ports:
      - "${NODE_API_PORT}:${NODE_API_PORT}"
    volumes:
      - ./logs:/app/logs
      - ./certificates:/app/certificates:ro
EOF_COMPOSE

  run chmod 600 "$ENV_PATH"
}

ensure_downloader
ensure_docker
ensure_working_directory

IMAGE_ARCHIVE="$WORKING_DIRECTORY/downloads/xrayne-node-image-$NODE_IMAGE_TAG.tar.gz"
download_file "$NODE_IMAGE_DOWNLOAD_URL" "$IMAGE_ARCHIVE"
gzip -dc "$IMAGE_ARCHIVE" | run docker load

issue_certificate
write_runtime_files

cd "$WORKING_DIRECTORY"
run docker compose up -d
run docker compose ps node

rm -f "$0"
""";
}
