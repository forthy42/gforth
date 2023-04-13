#!/bin/bash

VERSIONS=${VERSIONS-"stable oldstable unstable"}
ARCHS=${ARCHS-"amd64 i386"}

#docker login -u forthy42 <token

for arch in $ARCHS
do
    for i in $VERSIONS
    do
	docker build $* --build-arg VERSION=$i --build-arg ARCH=$arch -t forthy42/gforth-builder-$arch:$i .
	docker push forthy42/gforth-builder-$arch:$i
    done
done
