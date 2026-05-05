#!/usr/bin/env sh
set -eu

APP_DIR="/opt/xrayne"
INSTALL_DIR="$APP_DIR/cli"
RESERVE_DIR="/usr/shared/xrayne"
BIN_DIR="/usr/local/bin"
EXECUTABLE="xrayne"

while [ "$#" -gt 0 ]; do
  case "$1" in
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

if [ -z "$INSTALL_DIR" ]; then
  echo "--install-dir value cannot be empty." >&2
  exit 1
fi

if [ -z "$BIN_DIR" ]; then
  echo "--bin-dir value cannot be empty." >&2
  exit 1
fi

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

remove_path_line() {
  profile="$HOME/.profile"
  line="export PATH=\"$BIN_DIR:\$PATH\""

  if [ ! -f "$profile" ]; then
    return
  fi

  tmp_file="$(mktemp)"
  grep -Fvx "$line" "$profile" | grep -Fvx "# XRayne CLI" > "$tmp_file" || true
  mv "$tmp_file" "$profile"
}

if [ -d "$APP_DIR" ] && ( [ -f "$APP_DIR/docker-compose.yml" ] || [ -f "$APP_DIR/docker-compose.yaml" ] ); then
  echo "Stopping and removing Docker containers..."
  if command -v docker >/dev/null 2>&1 && docker compose version >/dev/null 2>&1; then
    run_root docker compose -f "$APP_DIR/docker-compose.yml" down || echo "Warning: Failed to stop docker containers, proceeding anyway."
  elif command -v docker-compose >/dev/null 2>&1; then
    run_root docker-compose -f "$APP_DIR/docker-compose.yml" down || echo "Warning: Failed to stop docker containers, proceeding anyway."
  else
    echo "Warning: Docker Compose not found. Containers might still be running."
  fi
fi
# -------------------------------

COMMAND_PATH="$BIN_DIR/$EXECUTABLE"

if [ -e "$COMMAND_PATH" ]; then
  run_root rm -f "$COMMAND_PATH"
fi

if [ -d "$APP_DIR" ]; then
  run_root rm -rf "$APP_DIR"
fi

if [ -d "$RESERVE_DIR" ]; then
  run_root rm -rf "$RESERVE_DIR"
fi

remove_path_line

echo "XRayne CLI removed."
echo "Removed application directory: $APP_DIR"
echo "Removed reserve directory: $RESERVE_DIR"
echo "Removed command path: $COMMAND_PATH"