#!/usr/bin/env bash
set -euo pipefail

BASE_URL=${BASE_URL:-http://localhost:5000}

function assert_200() {
  local url="$1"
  echo "→ GET $url"
  status=$(curl -s -o /dev/null -w "%{http_code}" "$url")
  if [[ "$status" != "200" ]]; then
    echo "Expected 200 but got $status for $url" >&2
    exit 1
  fi
}

echo "Running API smoke tests against $BASE_URL"

# Health
assert_200 "$BASE_URL/health"

# Basic endpoints (they may return empty but must be 200)
assert_200 "$BASE_URL/api/customers"
assert_200 "$BASE_URL/api/products"

echo "✅ Smoke tests passed"
