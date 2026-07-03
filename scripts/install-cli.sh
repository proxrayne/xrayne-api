#!/usr/bin/env sh
set -eu

REPOSITORY="proxrayne/xrayne-cli"
VERSION="latest"
PROJECT_DIR="/opt/xrayne"
INSTALL_DIR="$PROJECT_DIR/cli"
BIN_DIR="/usr/local/bin"
EXECUTABLE="xrayne"

while [ "$#" -gt 0 ]; do
  case "$1" in
    --version)
      VERSION="${2:-}"
      shift 2
      ;;
    --install-dir)
      INSTALL_DIR="${2:-}"
      shift 2
      ;;
    --bin-dir)
      BIN_DIR="${2:-}"
      shift 2
      ;;
    *)
      echo "Unknown argument: $1" >&2
      exit 1
      ;;
  esac
done

if [ -z "$VERSION" ]; then
  echo "--version value cannot be empty." >&2
  exit 1
fi

if [ -z "$INSTALL_DIR" ]; then
  echo "--install-dir value cannot be empty." >&2
  exit 1
fi

if [ -z "$BIN_DIR" ]; then
  echo "--bin-dir value cannot be empty." >&2
  exit 1
fi

OS="$(uname -s)"
ARCH="$(uname -m)"

case "$OS:$ARCH" in
  Linux:x86_64|Linux:amd64)
    ASSET="xrayne-cli-linux-x64.tar.gz"
    ;;
  Darwin:arm64|Darwin:aarch64)
    ASSET="xrayne-cli-osx-arm64.tar.gz"
    ;;
  *)
    echo "Unsupported OS/architecture: $OS $ARCH" >&2
    echo "Published CLI builds: linux-x64, osx-arm64, win-x64." >&2
    exit 1
    ;;
esac

require_command() {
  if ! command -v "$1" >/dev/null 2>&1; then
    echo "'$1' is required." >&2
    exit 1
  fi
}

run_root() {
  if [ "$(id -u)" -eq 0 ]; then
    "$@"
  else
    require_command sudo
    sudo "$@"
  fi
}

install_compose_plugin_binary() {
  compose_arch="$(uname -m)"
  case "$compose_arch" in
    x86_64|amd64)
      compose_arch="x86_64"
      ;;
    aarch64|arm64)
      compose_arch="aarch64"
      ;;
    *)
      echo "Unsupported Docker Compose architecture: $compose_arch" >&2
      exit 1
      ;;
  esac

  plugin_dir="/usr/local/lib/docker/cli-plugins"
  plugin_path="$plugin_dir/docker-compose"
  plugin_url="https://github.com/docker/compose/releases/latest/download/docker-compose-linux-$compose_arch"

  echo "Installing Docker Compose plugin from $plugin_url"
  run_root mkdir -p "$plugin_dir"
  tmp_compose="$(mktemp)"
  download "$plugin_url" "$tmp_compose"
  run_root install -m 755 "$tmp_compose" "$plugin_path"
  rm -f "$tmp_compose"
}

install_system_dependencies() {
  case "$(uname -s)" in
    Linux)
      ;;
    *)
      return
      ;;
  esac

  missing_common=""
  if ! command -v curl >/dev/null 2>&1 && ! command -v wget >/dev/null 2>&1; then
    missing_common="$missing_common curl"
  fi

  if ! command -v gzip >/dev/null 2>&1; then
    missing_common="$missing_common gzip"
  fi

  if [ ! -f "/etc/ssl/certs/ca-certificates.crt" ] && [ ! -f "/etc/ssl/cert.pem" ]; then
    missing_common="$missing_common ca-certificates"
  fi

  docker_missing=0
  if ! command -v docker >/dev/null 2>&1; then
    docker_missing=1
  fi

  compose_missing=0
  if ! docker compose version >/dev/null 2>&1; then
    compose_missing=1
  fi

  if [ -z "$missing_common" ] && [ "$docker_missing" -eq 0 ] && [ "$compose_missing" -eq 0 ]; then
    echo "Required system modules are already installed."
  else
    echo "Installing missing system modules..."
  fi

  if command -v apt-get >/dev/null 2>&1; then
    packages="$missing_common"
    if [ "$docker_missing" -eq 1 ]; then
      packages="$packages docker.io"
    fi
    if [ -n "$packages" ]; then
      run_root apt-get update
      run_root env DEBIAN_FRONTEND=noninteractive apt-get install -y $packages
    fi
    if [ "$compose_missing" -eq 1 ] && ! docker compose version >/dev/null 2>&1; then
      run_root apt-get update
      run_root env DEBIAN_FRONTEND=noninteractive apt-get install -y docker-compose-plugin || \
        run_root env DEBIAN_FRONTEND=noninteractive apt-get install -y docker-compose-v2 || true
    fi
  elif command -v dnf >/dev/null 2>&1; then
    packages="$missing_common"
    if [ "$docker_missing" -eq 1 ]; then
      packages="$packages docker"
    fi
    if [ -n "$packages" ]; then
      run_root dnf makecache -y
      run_root dnf install -y $packages
    fi
    if [ "$compose_missing" -eq 1 ] && ! docker compose version >/dev/null 2>&1; then
      run_root dnf install -y docker-compose-plugin || true
    fi
  elif command -v yum >/dev/null 2>&1; then
    packages="$missing_common"
    if [ "$docker_missing" -eq 1 ]; then
      packages="$packages docker"
    fi
    if [ -n "$packages" ]; then
      run_root yum makecache -y
      run_root yum install -y $packages
    fi
    if [ "$compose_missing" -eq 1 ] && ! docker compose version >/dev/null 2>&1; then
      run_root yum install -y docker-compose-plugin || true
    fi
  elif command -v apk >/dev/null 2>&1; then
    packages="$missing_common"
    if [ "$docker_missing" -eq 1 ]; then
      packages="$packages docker"
    fi
    if [ -n "$packages" ]; then
      run_root apk update
      run_root apk add --no-cache $packages
    fi
    if [ "$compose_missing" -eq 1 ] && ! docker compose version >/dev/null 2>&1; then
      run_root apk add --no-cache docker-cli-compose || true
    fi
  else
    echo "Unsupported Linux package manager. Install Docker and Docker Compose plugin manually." >&2
    exit 1
  fi

  if ! docker compose version >/dev/null 2>&1; then
    install_compose_plugin_binary
  fi

  if command -v systemctl >/dev/null 2>&1; then
    run_root systemctl enable --now docker
  elif command -v service >/dev/null 2>&1; then
    run_root service docker start
  fi

  docker --version
  docker compose version
}

download() {
  url="$1"
  destination="$2"

  if command -v curl >/dev/null 2>&1; then
    curl -fsSL "$url" -o "$destination"
  elif command -v wget >/dev/null 2>&1; then
    wget -q "$url" -O "$destination"
  else
    echo "curl or wget is required." >&2
    exit 1
  fi
}

resolve_download_url() {
  if [ "$VERSION" = "latest" ]; then
    printf '%s\n' "https://github.com/$REPOSITORY/releases/latest/download/$ASSET"
  else
    printf '%s\n' "https://github.com/$REPOSITORY/releases/download/$VERSION/$ASSET"
  fi
}

add_to_path_if_needed() {
  directory="$1"

  case ":$PATH:" in
    *":$directory:"*) return 0 ;;
  esac

  profile="$HOME/.profile"
  line="export PATH=\"$directory:\$PATH\""

  touch "$profile"
  if ! grep -Fqx "$line" "$profile"; then
    {
      echo ""
      echo "# XRayne CLI"
      echo "$line"
    } >> "$profile"
  fi

  export PATH="$directory:$PATH"
  echo "Added '$directory' to '$profile'. Restart the shell or run: . '$profile'"
}

TMP_DIR="$(mktemp -d)"
ARCHIVE_PATH="$TMP_DIR/$ASSET"
EXTRACT_DIR="$TMP_DIR/extract"
WRAPPER_PATH="$TMP_DIR/xrayne-wrapper"

cleanup() {
  rm -rf "$TMP_DIR"
}
trap cleanup EXIT

DOWNLOAD_URL="$(resolve_download_url)"

install_system_dependencies

echo "Downloading XRayne CLI from $DOWNLOAD_URL"
download "$DOWNLOAD_URL" "$ARCHIVE_PATH"

mkdir -p "$EXTRACT_DIR"
tar -xzf "$ARCHIVE_PATH" -C "$EXTRACT_DIR"

SOURCE_PATH="$(find "$EXTRACT_DIR" -type f -name "$EXECUTABLE" | head -n 1)"
if [ -z "$SOURCE_PATH" ]; then
  echo "Executable '$EXECUTABLE' was not found inside '$ASSET'." >&2
  exit 1
fi

cat > "$WRAPPER_PATH" <<EOF
#!/usr/bin/env sh
cd "$INSTALL_DIR"
exec "$INSTALL_DIR/$EXECUTABLE" "\$@"
EOF

run_root mkdir -p "$PROJECT_DIR" "$INSTALL_DIR"
run_root mkdir -p "$BIN_DIR"
run_root cp -R "$EXTRACT_DIR/." "$INSTALL_DIR/"
run_root chmod +x "$INSTALL_DIR/$EXECUTABLE"
run_root install -m 755 "$WRAPPER_PATH" "$BIN_DIR/$EXECUTABLE"
if [ -n "${SUDO_UID:-}" ] && [ -n "${SUDO_GID:-}" ]; then
  run_root chown "$SUDO_UID:$SUDO_GID" "$PROJECT_DIR"
fi
add_to_path_if_needed "$BIN_DIR"

echo ""
echo "XRayne CLI installed successfully."
echo "Application directory: $INSTALL_DIR"
echo "Command path: $BIN_DIR/$EXECUTABLE"
echo ""
echo "Try: xrayne version"
