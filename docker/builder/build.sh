#!/bin/bash

VERSIONS=${VERSIONS-"stable oldstable unstable"}
case `uname -m`
in
    x86_64)
	ARCHS=${ARCHS-"linux/amd64 linux/i386"}
	;;
    aarch64)
	ARCHS=${ARCHS-"linux/arm64/v8 linux/arm/v7"}
	;;
esac

#docker login -u forthy42 <token

for arch in $ARCHS
do
    arch1=$(echo $arch | tr '/' '-')
    for i in $VERSIONS
    do
	docker build $* --build-arg VERSION=$i --build-arg ARCH=${arch#*/} --platform $arch -t forthy42/gforth-builder-$arch1:$i .
	docker push forthy42/gforth-builder-$arch1:$i
    done
done
