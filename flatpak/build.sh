#!/bin/bash

flatpak-builder --user --install --force-clean build org.freedesktop.Sdk.Extension.stb.yml
flatpak-builder --user --install --force-clean build org.gnu.gforth.yml
