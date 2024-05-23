#!/usr/bin/env sh
set -eu

# Inject environment variables into NGINX configuration
# List all variables to be substituted to avoid clashing with
# NGINX own variables: https://serverfault.com/questions/577370
envsubst \
  '${BASE_HREF}' \
  </etc/nginx/nginx.conf.template \
  >/etc/nginx/nginx.conf
cat /etc/nginx/nginx.conf
# Set correct HTML base tag, so static resources are fetched
# from the right path instead of the root path.
# NOTE: Trailing and leading slashes in base href are important!
# Using `~` separator to avoid problems with forward slashes
sed --in-place \
  's~<base href="/">~<base href="'$BASE_HREF'/"><script type="text/javascript"> window.baseHref = "'$BASE_HREF'"; window.apiEndpoint = "'$API_ENDPOINT'";</script>~' \
  /usr/share/nginx/html/index.html

cat /usr/share/nginx/html/index.html
exec "$@"
