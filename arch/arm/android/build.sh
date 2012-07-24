#!/bin/bash

# takes as extra argument a directory where to look for .so-s

rm -rf libs/armeabi
mkdir -p libs/armeabi
cp ../../../engine/.libs/libgforth-fast.so libs/armeabi/
LIBCC=../../..
for i in $LIBCC $*
do
    for j in $(cd $i/lib/gforth/*/libcc-named/.libs; echo *.so)
    do
	cp $i/lib/gforth/*/libcc-named/.libs/$j libs/armeabi/$j
    done
    shift
done
#ant debug
ant release
cp bin/Gforth-release-unsigned.apk bin/Gforth.apk
jarsigner -verbose -sigalg SHA1withRSA -digestalg SHA1 -keystore ~/.gnupg/bernd-release-key.keystore bin/Gforth.apk bernd