# TODO: pin to digest for production (mcr.microsoft.com/dotnet/aspnet:8.0@sha256:...)
FROM mcr.microsoft.com/dotnet/aspnet:8.0
RUN apt-get update -y && apt-get install --no-install-recommends gettext-base nginx sqlite3 libsqlite3-dev etcd-client -y \
    && rm -rf /var/lib/apt/lists/*
RUN groupadd -r nginx && useradd -r -s /sbin/nologin -d /dev/null -g nginx nginx
COPY ./api/bin/Release/Publish /api
COPY nginx.conf.template /etc/nginx/
COPY ./app/etcd-manager-ui/dist/etcd-manager-ui/browser /usr/share/nginx/html
COPY docker-entrypoint.sh /
ENV ETCDCTL_API=3
RUN etcdctl && etcdctl version
# Create non-root user for .NET API and grant write access to SQLite data directory
RUN adduser --disabled-password --gecos '' appuser \
    && mkdir -p /api/wwwroot/data \
    && chown -R appuser:appuser /api/wwwroot/data
USER appuser
# 80: api, 81: # app
EXPOSE 80 81
WORKDIR /
ENTRYPOINT ["/docker-entrypoint.sh"]
CMD [ "/bin/bash", "-c", "nginx -g 'daemon on;'; cd api && dotnet EtcdManager.API.dll" ]