#!/bin/bash
mkdir -p libs/armeabi
cp ../../../engine/.libs/libgforth-fast.so libs/armeabi/
LIBCC=../../../lib/gforth/*/libcc-named/.libs/
for i in $(cd $LIBCC; echo *.so)
do
  cp $LIBCC/$i libs/armeabi/lib$i
done
#ant debug
ant release
cp bin/Gforth-release-unsigned.apk bin/Gforth.apk
jarsigner -verbose -sigalg MD5withRSA -digestalg SHA1 -keystore ~/.gnupg/bernd-release-key.keystore bin/Gforth.apk bernd