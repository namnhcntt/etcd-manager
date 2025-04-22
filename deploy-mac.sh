#!/bin/bash

cd api
dotnet publish  -c Release -o ./bin/Release/Publish
cp -r ./bin/Release/Publish/wwwroot/rawdb/* ./bin/Release/Publish/wwwroot/data
cd ../app/etcd-manager-ui
# pnpm run build-prod
pnpm run build-prod-sourcemap
cd ../..
docker build --progress plain -f Dockerfile -t t2nh/etcd-manager:1.0.3-arm64 .
docker push t2nh/etcd-manager:1.0.3-arm64