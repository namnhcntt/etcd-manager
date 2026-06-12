#!/usr/bin/env sh
set -eu

# Validate required environment variables with per-variable ALLOWLISTS —
# reject empty values and any character outside the set the variable
# legitimately needs. Note: `~` is excluded everywhere because it is used
# as the sed delimiter below.

# URL-ish values (scheme://host:port/path), e.g. API_ENDPOINT
_validate_url_var() {
  local name="$1"
  local value="$2"
  if [ -z "$value" ]; then
    echo "ERROR: $name must not be empty" >&2
    exit 1
  fi
  case "$value" in
    *[!a-zA-Z0-9._:/-]*)
      echo "ERROR: $name contains disallowed characters (allowed: a-z A-Z 0-9 . _ : / -)" >&2
      exit 1
      ;;
  esac
}

# Path values, e.g. BASE_HREF
_validate_path_var() {
  local name="$1"
  local value="$2"
  if [ -z "$value" ]; then
    echo "ERROR: $name must not be empty" >&2
    exit 1
  fi
  case "$value" in
    *[!a-zA-Z0-9._/-]*)
      echo "ERROR: $name contains disallowed characters (allowed: a-z A-Z 0-9 . _ / -)" >&2
      exit 1
      ;;
  esac
}

_validate_path_var BASE_HREF    "$BASE_HREF"
_validate_url_var  API_ENDPOINT "$API_ENDPOINT"

# Inject environment variables into NGINX configuration
# List all variables to be substituted to avoid clashing with
# NGINX own variables: https://serverfault.com/questions/577370
envsubst \
  '${BASE_HREF} ${API_ENDPOINT}' \
  </etc/nginx/nginx.conf.template \
  >/etc/nginx/nginx.conf
# Removed: cat /etc/nginx/nginx.conf  — avoid leaking config to logs

# Write runtime configuration to an EXTERNAL script (referenced by
# <script src="env.js"> in index.html) instead of an inline <script>,
# so the Content-Security-Policy can stay script-src 'self' without
# 'unsafe-inline'.
cat >/usr/share/nginx/html/env.js <<EOF
window.baseHref = "${BASE_HREF}";
window.apiEndpoint = "${API_ENDPOINT}";
EOF

# Set correct HTML base tag, so static resources are fetched
# from the right path instead of the root path.
# NOTE: Trailing and leading slashes in base href are important!
# Using `~` separator to avoid problems with forward slashes
# (the allowlists above reject `~`, so the value cannot break out)
sed --in-place \
  's~<base href="/">~<base href="'$BASE_HREF'/">~' \
  /usr/share/nginx/html/index.html
# Removed: cat /usr/share/nginx/html/index.html  — avoid leaking injected HTML to logs

exec "$@"
