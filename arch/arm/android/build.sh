#!/bin/bash

if [ ! -f build.xml ]
then
    android update project -p . -s --target android-10
fi

# takes as extra argument a directory where to look for .so-s

ENGINE=gforth

case "$1" in
    -ditc|-fast)
	EXT=$1
	shift
	ENGINE=gforth$EXT
	;;
    --ext)
	shift
	EXT=$1
	shift
	ENGINE=$EXT
	;;
esac

GFORTH_VERSION=$(gforth --version 2>&1 | cut -f2 -d' ')
APP_VERSION=$[$(cat ~/.app-version)+1]
echo $APP_VERSION >~/.app-version

sed -e "s/@ENGINE@/$ENGINE/g" -e "s/@VERSION@/$GFORTH_VERSION/g" -e "s/@APP@/$APP_VERSION/g" <AndroidManifest.xml.in >AndroidManifest.xml

SRC=../../..
LIBS=libs/armeabi
LIBCCNAMED=lib/$(gforth --version 2>&1 | tr ' ' '/')/libcc-named/.libs

rm -rf $LIBS
mkdir -p $LIBS

if [ "$1" != "--no-gforthgz" ]
then
    (cd $SRC
	if [ "$1" != "--no-config" ]; then ./configure --host=arm-unknown-linux-android --with-cross=android --with-ditc=gforth-ditc-x32 --prefix= --datarootdir=/sdcard --libdir=/sdcard --libexecdir=/lib --enable-lib || exit 1; fi
	make # || exit 1
	make setup-debdist || exit 1) || exit 1
    if [ "$1" == "--no-config" ]; then CONFIG=no; shift; fi

    for i in . $*
    do
	cp $i/*.{fs,fi,png,jpg} $SRC/debian/sdcard/gforth/site-forth
    done
    (cd $SRC/debian/sdcard
	mkdir -p gforth/home
	gforth ../../archive.fs gforth/home/ $(find gforth -type f)) | gzip -9 >$LIBS/libgforthgz.so
else
    shift
fi

SHA256=$(sha256sum libs/armeabi/libgforthgz.so | cut -f1 -d' ')

sed -e "s/sha256sum-sha256sum-sha256sum-sha256sum-sha256sum-sha256sum-sha2/$SHA256/" $SRC/engine/.libs/lib$ENGINE.so >$LIBS/lib$ENGINE.so

ANDROID=${PWD%/*/*/*}
CFLAGS="-O3 -march=armv5 -mfloat-abi=softfp -mfpu=vfp"
LIBCC=$SRC
for i in $LIBCC $*
do
    (cd $i; test -d shlibs && \
	(cd shlibs
	    for j in *; do
		(cd $j
		    if [ "$CONFIG" == no ]
		    then
			make
		    else
			./configure CFLAGS=$CFLAGS --host=arm-linux-androideabi && make clean && make
		    fi
		)
	    done
	)
    )
    (cd $i; test -x ./libcc.android && ANDROID=$ANDROID ./libcc.android)
    for j in $LIBCCNAMED .libs
    do
	for k in $(cd $i/$j; echo *.so)
	do
	    cp $i/$j/$k $LIBS
	done
    done
    shift
done
strip $LIBS/*.so
#ant debug
ant release
cp bin/Gforth-release.apk bin/$ENGINE.apk
#jarsigner -verbose -sigalg SHA1withRSA -digestalg SHA1 -keystore ~/.gnupg/bernd-release-key.keystore bin/Gforth$EXT.apk bernd
