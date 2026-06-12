# Digest pinned (mcr.microsoft.com/dotnet/aspnet:8.0 multi-arch index).
# To re-pin: docker buildx imagetools inspect mcr.microsoft.com/dotnet/aspnet:8.0
FROM mcr.microsoft.com/dotnet/aspnet:8.0@sha256:8e7aefa6e35d12dec6301c26bd3ceba514aee4e699562b7b10cc242b2bd48c12
RUN apt-get update -y && apt-get install --no-install-recommends gettext-base nginx sqlite3 libsqlite3-dev etcd-client -y \
    && rm -rf /var/lib/apt/lists/*
COPY ./api/bin/Release/Publish /api
COPY nginx.conf.template /etc/nginx/
COPY ./app/etcd-manager-ui/dist/etcd-manager-ui/browser /usr/share/nginx/html
COPY docker-entrypoint.sh /
ENV ETCDCTL_API=3
RUN etcdctl && etcdctl version
# Create non-root user for both the .NET API and nginx, and make every path
# they write at runtime owned by it:
# - /api/wwwroot/data: SQLite DB + Data Protection keys (volume)
# - /etc/nginx/nginx.conf: rendered by docker-entrypoint.sh (envsubst)
# - env.js + index.html ONLY in the web root: the entrypoint writes runtime
#   config through these files; the directory and the JS/CSS bundle stay
#   root-owned so a compromised runtime process cannot rewrite served assets
#   (which would undermine the CSP's script-src 'self')
# - /var/log/nginx: nginx opens its compiled-in default error log at startup
#   (runtime logs go to stdout/stderr per nginx.conf)
RUN adduser --disabled-password --gecos '' appuser \
    && mkdir -p /api/wwwroot/data /var/log/nginx \
    && touch /etc/nginx/nginx.conf /usr/share/nginx/html/env.js \
    && chown -R appuser:appuser /api/wwwroot/data /var/log/nginx \
    && chown appuser:appuser /etc/nginx/nginx.conf /usr/share/nginx/html/env.js /usr/share/nginx/html/index.html
USER appuser
# 80: api, 81: # app
EXPOSE 80 81
WORKDIR /
ENTRYPOINT ["/docker-entrypoint.sh"]
CMD [ "/bin/bash", "-c", "nginx -g 'daemon on;'; cd api && dotnet EtcdManager.API.dll" ]
