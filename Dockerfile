FROM mcr.microsoft.com/dotnet/aspnet:8.0
RUN apt-get update -y && apt-get install gettext-base nginx sqlite3 libsqlite3-dev etcd-client -y
RUN groupadd -r nginx && useradd -r -s /sbin/nologin -d /dev/null -g nginx nginx
COPY ./api/bin/Release/Publish /api
COPY nginx.conf.template /etc/nginx/
COPY ./app/etcd-manager-ui/dist/etcd-manager-ui/browser /usr/share/nginx/html
COPY docker-entrypoint.sh /
ENV ETCDCTL_API=3
RUN etcdctl && etcdctl version
# 80: api, 81: # app
EXPOSE 80 81
WORKDIR /
ENTRYPOINT ["/docker-entrypoint.sh"]
CMD [ "/bin/bash", "-c", "nginx -g 'daemon on;'; cd api && dotnet EtcdManager.API.dll" ]