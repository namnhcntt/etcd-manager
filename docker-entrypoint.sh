#!/usr/bin/env sh
set -eu

# Validate required environment variables — reject empty values and
# characters that could allow shell injection (quotes, backticks, $, etc.)
_validate_var() {
  local name="$1"
  local value="$2"
  if [ -z "$value" ]; then
    echo "ERROR: $name must not be empty" >&2
    exit 1
  fi
  case "$value" in
    *[\'\"\`\$\;\|\&\<\>]*)
      echo "ERROR: $name contains disallowed characters" >&2
      exit 1
      ;;
  esac
}

_validate_var BASE_HREF    "$BASE_HREF"
_validate_var API_ENDPOINT "$API_ENDPOINT"

# Inject environment variables into NGINX configuration
# List all variables to be substituted to avoid clashing with
# NGINX own variables: https://serverfault.com/questions/577370
envsubst \
  '${BASE_HREF}' \
  </etc/nginx/nginx.conf.template \
  >/etc/nginx/nginx.conf
# Removed: cat /etc/nginx/nginx.conf  — avoid leaking config to logs

# Set correct HTML base tag, so static resources are fetched
# from the right path instead of the root path.
# NOTE: Trailing and leading slashes in base href are important!
# Using `~` separator to avoid problems with forward slashes
sed --in-place \
  's~<base href="/">~<base href="'$BASE_HREF'/"><script type="text/javascript"> window.baseHref = "'$BASE_HREF'"; window.apiEndpoint = "'$API_ENDPOINT'";</script>~' \
  /usr/share/nginx/html/index.html
# Removed: cat /usr/share/nginx/html/index.html  — avoid leaking injected HTML to logs

exec "$@"
