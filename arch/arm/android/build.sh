#!/bin/bash

# takes as extra argument a directory where to look for .so-s

ENGINE=-fast
SRC=../../..
LIBS=libs/armeabi
LIBCCNAMED=lib/$(gforth --version 2>&1 | tr ' ' '/')/libcc-named/.libs

rm -rf $LIBS
mkdir -p $LIBS

if [ "$1" != "--no-gforthgz" ]
then
    (cd $SRC
	./configure --host=arm --with-cross=android --prefix= --datarootdir=/sdcard --libdir=/sdcard --libexecdir=/lib --enable-lib
	make
	make setup-debdist)
else
    shift
fi

for i in . $*
do
    cp $i/*.{fs,png,jpg} $SRC/debian/sdcard/gforth/site-forth
done
(cd $SRC/debian/sdcard
    mkdir -p gforth/home
    touch gforth/home/.gforth-history
    gforth ../../archive.fs $(find gforth -type f)) | gzip -9 >$LIBS/libgforthgz.so

cp $SRC/engine/.libs/libgforth$ENGINE.so $LIBS/
LIBCC=$SRC
for i in $LIBCC $*
do
    for j in $LIBCCNAMED .libs
    do
	for k in $(cd $i/$j; echo *.so)
	do
	    cp $i/$j/$k $LIBS
	done
    done
    shift
done
#ant debug
ant release
cp bin/Gforth-release-unsigned.apk bin/Gforth.apk
jarsigner -verbose -sigalg SHA1withRSA -digestalg SHA1 -keystore ~/.gnupg/bernd-release-key.keystore bin/Gforth.apk bernd