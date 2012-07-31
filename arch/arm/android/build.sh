#!/bin/bash

# takes as extra argument a directory where to look for .so-s

rm -rf libs/armeabi
mkdir -p libs/armeabi

(cd ../../..
./configure --host=arm --with-cross=android --prefix= --datarootdir=/sdcard --libdir=/sdcard --libexecdir=/lib --enable-lib
make
make setup-debdist)
for i in . $*
do
    cp $i/*.fs ../../../debian/sdcard/gforth/site-forth
done
(cd ../../../debian/sdcard
    mkdir -p gforth/home
    touch gforth/home/.gforth-history
    gforth ../../archive.fs $(find gforth -type f)) | gzip -9 >libs/armeabi/libgforthgz.so

cp ../../../engine/.libs/libgforth-fast.so libs/armeabi/
LIBCC=../../..
for i in $LIBCC $*
do
    for j in $(cd $i/lib/gforth/*/libcc-named/.libs; echo *.so)
    do
	cp $i/lib/gforth/*/libcc-named/.libs/$j libs/armeabi/
    done
    shift
done
#ant debug
ant release
cp bin/Gforth-release-unsigned.apk bin/Gforth.apk
jarsigner -verbose -sigalg SHA1withRSA -digestalg SHA1 -keystore ~/.gnupg/bernd-release-key.keystore bin/Gforth.apk bernd