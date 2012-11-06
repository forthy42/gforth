#!/bin/bash

# takes as extra argument a directory where to look for .so-s

case "$1" in
    -ditc|-fast)
	ENGINE=$1
	shift
	;;
esac

EXT=$ENGINE

sed -e 's/android:value="gforth[a-z-]*"/android:value="gforth'$ENGINE'"/g' <AndroidManifest.xml >AndroidManifest.xml+
mv AndroidManifest.xml+ AndroidManifest.xml

SRC=../../..
LIBS=libs/armeabi
LIBCCNAMED=lib/$(gforth --version 2>&1 | tr ' ' '/')/libcc-named/.libs

rm -rf $LIBS
mkdir -p $LIBS

if [ "$1" != "--no-gforthgz" ]
then
    (rm androidmain.o zexpand.o
	cd $SRC
	if [ "$1" != "--no-config" ]; then ./configure --host=arm-unknown-linux-android --with-cross=android --prefix= --datarootdir=/sdcard --libdir=/sdcard --libexecdir=/lib --enable-lib; fi
	make
	make setup-debdist)
    if [ "$1" == "--no-config" ]; then shift; fi

    for i in . $*
    do
	cp $i/*.{fs,fi,png,jpg} $SRC/debian/sdcard/gforth/site-forth
    done
    (cd $SRC/debian/sdcard
	mkdir -p gforth/home
	touch gforth/home/.gforth-history
	gforth ../../archive.fs $(find gforth -type f)) | gzip -9 >$LIBS/libgforthgz.so
else
    shift
fi

SHA256=$(sha256sum libs/armeabi/libgforthgz.so | cut -f1 -d' ')

sed -e "s/sha256sum-sha256sum-sha256sum-sha256sum-sha256sum-sha256sum-sha2/$SHA256/" $SRC/engine/.libs/libgforth$ENGINE.so >$LIBS/libgforth$ENGINE.so

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
cp bin/Gforth-release-unsigned.apk bin/Gforth$EXT.apk
jarsigner -verbose -sigalg SHA1withRSA -digestalg SHA1 -keystore ~/.gnupg/bernd-release-key.keystore bin/Gforth$EXT.apk bernd
