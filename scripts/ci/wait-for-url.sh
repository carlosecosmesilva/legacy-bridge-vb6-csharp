#!/usr/bin/env bash
set -euo pipefail

URL="${1:-}"
TIMEOUT="${2:-60}"

if [[ -z "$URL" ]]; then
  echo "Usage: $0 <url> [timeout_seconds]"
  exit 1
fi

echo "Waiting for $URL (timeout: ${TIMEOUT}s)"
start=$(date +%s)

until curl -fsS -o /dev/null "$URL"; do
  sleep 2
  now=$(date +%s)
  elapsed=$(( now - start ))
  if (( elapsed > TIMEOUT )); then
    echo "Timeout waiting for $URL after ${TIMEOUT}s"
    exit 1
  fi
done

echo "URL is up: $URL"
