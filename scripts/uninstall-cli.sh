#!/usr/bin/env sh
set -eu

APP_DIR="/opt/xrayne"
INSTALL_DIR="$APP_DIR/cli"
PROJECT_DIR=""
PROJECT_DIR_PROVIDED=0
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
    --project-dir)
      PROJECT_DIR="${2:-}"
      PROJECT_DIR_PROVIDED=1
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

if [ -z "$PROJECT_DIR" ]; then
  PROJECT_DIR="$APP_DIR"
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

is_project_dir() {
  directory="$1"

  [ -d "$directory" ] || return 1

  if [ -x "$directory/cli/$EXECUTABLE" ] || \
     [ -f "$directory/.env" ] || \
     [ -f "$directory/config.json" ] || \
     [ -f "$directory/docker-compose.yml" ] || \
     [ -f "$directory/docker-compose.yaml" ]; then
    return 0
  fi

  return 1
}

assert_safe_project_dir() {
  directory="$1"

  if [ -z "$directory" ] || [ "$directory" = "/" ]; then
    echo "Refusing to remove unsafe project directory '$directory'." >&2
    exit 1
  fi

  if ! is_project_dir "$directory"; then
    echo "'$directory' does not look like an XRayne project directory." >&2
    echo "Expected at least one of: cli/$EXECUTABLE, .env, config.json, docker-compose.yml." >&2
    exit 1
  fi
}

remove_xrayne_docker_containers() {
  if ! command -v docker >/dev/null 2>&1; then
    echo "Warning: Docker not found. XRayne containers might still be running."
    return
  fi

  container_ids="$(
    {
      docker ps -aq --filter "name=xrayne" 2>/dev/null || true
      docker ps -aq --filter "label=com.docker.compose.project=xrayne" 2>/dev/null || true
    } | sort -u
  )"
  if [ -z "$container_ids" ]; then
    return
  fi

  echo "Removing XRayne Docker containers..."
  # shellcheck disable=SC2086
  run_root docker rm -f $container_ids || echo "Warning: Failed to remove some XRayne containers, proceeding anyway."
}

if [ "$PROJECT_DIR_PROVIDED" -eq 0 ] && ! is_project_dir "$PROJECT_DIR"; then
  printf "Standard project directory '%s' was not found. Enter XRayne project directory: " "$APP_DIR"
  read -r PROJECT_DIR
fi

assert_safe_project_dir "$PROJECT_DIR"

echo "Project directory selected for removal: $PROJECT_DIR"
printf "This will delete all XRayne data in this directory. Type 'yes' to continue: "
read -r confirmation
if [ "$confirmation" != "yes" ]; then
  echo "Uninstall cancelled."
  exit 0
fi

if [ -d "$PROJECT_DIR" ] && ( [ -f "$PROJECT_DIR/docker-compose.yml" ] || [ -f "$PROJECT_DIR/docker-compose.yaml" ] ); then
  echo "Stopping and removing Docker containers..."
  if command -v docker >/dev/null 2>&1 && docker compose version >/dev/null 2>&1; then
    if [ -f "$PROJECT_DIR/docker-compose.yml" ]; then
      run_root docker compose -f "$PROJECT_DIR/docker-compose.yml" down || echo "Warning: Failed to stop docker containers, proceeding anyway."
    else
      run_root docker compose -f "$PROJECT_DIR/docker-compose.yaml" down || echo "Warning: Failed to stop docker containers, proceeding anyway."
    fi
  elif command -v docker-compose >/dev/null 2>&1; then
    if [ -f "$PROJECT_DIR/docker-compose.yml" ]; then
      run_root docker-compose -f "$PROJECT_DIR/docker-compose.yml" down || echo "Warning: Failed to stop docker containers, proceeding anyway."
    else
      run_root docker-compose -f "$PROJECT_DIR/docker-compose.yaml" down || echo "Warning: Failed to stop docker containers, proceeding anyway."
    fi
  else
    echo "Warning: Docker Compose not found. Containers might still be running."
  fi
fi

remove_xrayne_docker_containers

COMMAND_PATH="$BIN_DIR/$EXECUTABLE"

if [ -e "$COMMAND_PATH" ]; then
  run_root rm -f "$COMMAND_PATH"
fi

if [ -d "$PROJECT_DIR" ]; then
  run_root rm -rf "$PROJECT_DIR"
fi

remove_path_line

echo "XRayne CLI removed."
echo "Removed project directory: $PROJECT_DIR"
echo "Removed command path: $COMMAND_PATH"
