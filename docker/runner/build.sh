#!/bin/bash

PLATFORMS=${PLATFORMS:-linux/amd64,linux/arm64}

docker pull alpine:latest
docker buildx build --platform $PLATFORMS --network host --progress=plain -f Dockerfile.swig -t forthy42/swig:latest . 2>swig-build.log
docker buildx build --platform $PLATFORMS --network host --progress=plain -t forthy42/gforth:latest . 2>gforth-build.log
docker buildx build --platform $PLATFORMS --network host --progress=plain -f Dockerfile.gui -t forthy42/gforth-gui:latest . 2>gforth-gui-build.log
docker buildx build --platform $PLATFORMS --network host --progress=plain -f Dockerfile.gui+fonts -t forthy42/gforth-gui-fonts:latest . 2>gforth-fonts-build.log
if [ "$1" != "nopush" ]
then
    docker push forthy42/swig:latest
    docker push forthy42/gforth:latest
    docker push forthy42/gforth-gui:latest
    docker push forthy42/gforth-gui-fonts:latest
fi
