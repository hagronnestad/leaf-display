#!/bin/bash

dotnet build
dotnet publish -c Release
docker build -t image-leaf-client .
docker run -it --rm image-leaf-client

docker tag image-leaf-client ghcr.io/hagronnestad/image-leaf-client
docker push ghcr.io/hagronnestad/image-leaf-client:latest
