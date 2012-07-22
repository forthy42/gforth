#!/bin/bash
mkdir -p libs/armeabi
cp ../../../engine/.libs/libgforth-fast.so libs/armeabi/
ant debug
ant release
cp bin/Gforth-release-unsigned.apk bin/Gforth.apk
jarsigner -verbose -sigalg MD5withRSA -digestalg SHA1 -keystore ~/.gnupg/bernd-release-key.keystore bin/Gforth.apk bernd