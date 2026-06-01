#!/usr/bin/env bash

# Ensure strict mode for safer script execution.
set -euo pipefail

# Align the container user with the host Docker socket group.
# This avoids "permission denied" errors when Testcontainers or docker CLI
# access /var/run/docker.sock from inside the dev container.

SOCKET_PATH="/var/run/docker.sock"
FALLBACK_GROUP_NAME="docker-host"

# Exit quietly when Docker socket is not available.
if [[ ! -S "$SOCKET_PATH" ]]; then
  exit 0
fi

# Read socket GID from host-mounted docker.sock.
socket_gid="$(stat -c '%g' "$SOCKET_PATH")"

# If a group with this GID already exists, reuse it.
if group_entry="$(getent group "$socket_gid" 2>/dev/null)"; then
  existing_group_name="${group_entry%%:*}"
else
  existing_group_name=""
fi

if [[ -n "$existing_group_name" ]]; then
  target_group_name="$existing_group_name"
else
  # Create a deterministic fallback group when no matching GID is found.
  target_group_name="$FALLBACK_GROUP_NAME"
  sudo groupadd -f -g "$socket_gid" "$target_group_name"
fi

# Add current user to the target Docker socket group.
# Group membership takes effect on next shell/container session.
sudo usermod -aG "$target_group_name" "$USER"
