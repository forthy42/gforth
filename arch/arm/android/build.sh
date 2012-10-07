#!/bin/bash

# takes as extra argument a directory where to look for .so-s

if [ -z "$ENGINE" ]
then
   ENGINE=-fast
   EXT=""
else
   EXT=$ENGINE
fi

<<<<<<< build.sh
sed -e 's/android:value="gforth-[a-z]*"/android:value="gforth'$ENGINE'"/g' <AndroidManifest.xml >AndroidManifest.xml+
=======
sed -e 's/android:value="gforth[a-z-]*"/android:value="gforth'$ENGINE'"/g' <AndroidManifest.xml >AndroidManifest.xml+
>>>>>>> 1.12
mv AndroidManifest.xml+ AndroidManifest.xml

SRC=../../..
LIBS=libs/armeabi
LIBCCNAMED=lib/$(gforth --version 2>&1 | tr ' ' '/')/libcc-named/.libs

rm -rf $LIBS
mkdir -p $LIBS

if [ "$1" != "--no-gforthgz" ]
then
    (cd $SRC
	./configure --host=arm-unknown-linux-android --with-cross=android --prefix= --datarootdir=/sdcard --libdir=/sdcard --libexecdir=/lib --enable-lib
	make
	make setup-debdist)
else
    shift
fi

for i in . $*
do
    cp $i/*.{fs,fi,png,jpg} $SRC/debian/sdcard/gforth/site-forth
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
cp bin/Gforth-release-unsigned.apk bin/Gforth$EXT.apk
jarsigner -verbose -sigalg SHA1withRSA -digestalg SHA1 -keystore ~/.gnupg/bernd-release-key.keystore bin/Gforth$EXT.apk bernd