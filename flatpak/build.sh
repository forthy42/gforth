#!/bin/bash

GPG_ID=${GPG_ID-67007C30} # Default is my id
OPTS="${OPTS---gpg-sign=$GPG_ID --repo=repo --user --install --force-clean build}"

flatpak-builder $OPTS org.freedesktop.Sdk.Extension.stb.yml
flatpak-builder $OPTS org.gforth.gforth.yml
flatpak-builder $OPTS org.gforth.swig.yml

flatpak build-update-repo --generate-static-deltas --gpg-sign=$GPG_ID repo

rsync -avz repo root@net2o.de:/var/www/flathub.net2o.net/html
