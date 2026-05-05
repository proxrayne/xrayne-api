#!/usr/bin/env sh
set -eu

REPOSITORY="VanyaKrotov/xrayne"
VERSION="latest"
INSTALL_DIR="/opt/xrayne/cli"
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

ensure_stable_version() {
  case "$VERSION" in
    *-*)
      echo "Pre-release versions are not supported by this installer. Use a stable release tag." >&2
      exit 1
      ;;
  esac
}

resolve_download_url() {
  if [ "$VERSION" = "latest" ]; then
    printf '%s\n' "https://github.com/$REPOSITORY/releases/latest/download/$ASSET"
  else
    ensure_stable_version
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
export XRAYNE_CLI_CONFIG_DIR="$INSTALL_DIR"
cd "$INSTALL_DIR"
exec "$INSTALL_DIR/$EXECUTABLE" "\$@"
EOF

run_root mkdir -p "$INSTALL_DIR"
run_root mkdir -p "$BIN_DIR"
run_root cp -R "$EXTRACT_DIR/." "$INSTALL_DIR/"
run_root chmod +x "$INSTALL_DIR/$EXECUTABLE"
run_root install -m 755 "$WRAPPER_PATH" "$BIN_DIR/$EXECUTABLE"
add_to_path_if_needed "$BIN_DIR"

echo ""
echo "XRayne CLI installed successfully."
echo "Application directory: $INSTALL_DIR"
echo "Command path: $BIN_DIR/$EXECUTABLE"
echo ""
echo "Try: xrayne version"
