#!/bin/bash

flatpak-builder --user --install --force-clean build org.freedesktop.Sdk.Extension.stb.yml
flatpak-builder --user --install --force-clean build org.gforth.gforth.yml
flatpak-builder --user --install --force-clean build org.gforth.swig.yml
