#!/bin/bash
#Copyright (C) 2011,2012,2013,2014 Free Software Foundation, Inc.

#This file is part of Gforth.

#Gforth is free software; you can redistribute it and/or
#modify it under the terms of the GNU General Public License
#as published by the Free Software Foundation, either version 3
#of the License, or (at your option) any later version.

#This program is distributed in the hope that it will be useful,
#but WITHOUT ANY WARRANTY; without even the implied warranty of
#MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.#See the
#GNU General Public License for more details.

#You should have received a copy of the GNU General Public License
#along with this program. If not, see http://www.gnu.org/licenses/.

while [ "${1%%[^\+]*}" == '+' ]
do
    arch+=" ${1#+}"
    shift
done

echo "Extra build in $arch"

for i in $arch
do
    newdir=../../../../android-$i/arch/$i/android
    if [ -d $newdir ]
    then
	(cd $newdir && git pull && ./build.sh "$@")
    else
	echo "Can't cd to $newdir"
    fi
done

# takes as extra argument a directory where to look for .so-s

ENGINES="gforth-fast gforth-itc"

GFORTH_VERSION=$(gforth --version 2>&1 | cut -f2 -d' ')
APP_VERSION=$[$(cat ~/.app-version)+1]
echo $APP_VERSION >~/.app-version

sed -e "s/@VERSION@/$GFORTH_VERSION/g" -e "s/@APP@/$APP_VERSION/g" <AndroidManifest.xml.in >AndroidManifest.xml

if [ ! -f build.xml ]
then
    android update project -p . -s --target android-14
fi

SRC=../../..
LIBS=libs/x86_64
LIBCCNAMED=lib/$(gforth --version 2>&1 | tr ' ' '/')/libcc-named/.libs
TOOLCHAIN=${TOOLCHAIN-~/proj/android-toolchain-x86_64}

rm -rf $LIBS
mkdir -p $LIBS

if [ ! -f $TOOLCHAIN/sysroot/usr/lib/libsoil2.a ]
then
    cp $TOOLCHAIN/sysroot/usr/lib/libsoil.so $LIBS
fi
cp .libs/libtypeset.so $LIBS
strip $LIBS/lib{soil,typeset}.so

EXTRAS=""
for i in $@
do
    EXTRAS+=" -with-extras=$i"
done

if [ "$1" != "--no-gforthgz" ]
then
    (cd $SRC
	if [ "$1" != "--no-config" ]; then ./configure --host=x86_64-linux-android --with-cross=android --with-ditc=gforth-ditc --prefix= --datarootdir=/sdcard --libdir=/sdcard --libexecdir=/lib --enable-lib $EXTRAS || exit 1; fi
	make || exit 1
	if [ "$1" != "--no-config" ]; then make extras || exit 1; fi
	make setup-debdist || exit 1) || exit 1
    if [ "$1" == "--no-config" ]; then CONFIG=no; shift; fi

    for i in . $*
    do
	cp $i/*.{fs,fi,png,jpg} $SRC/debian/sdcard/gforth/site-forth
    done
    (cd $SRC/debian/sdcard
	rm -rf gforth/$GFORTH_VERSION/libcc-named
	mkdir -p gforth/home
	gforth ../../archive.fs gforth/home/ $(find gforth -type f)) | gzip -9 >$LIBS/libgforthgz.so
else
    shift
fi

SHA256=$(sha256sum $LIBS/libgforthgz.so | cut -f1 -d' ')

for i in $ENGINES
do
    sed -e "s/sha256sum-sha256sum-sha256sum-sha256sum-sha256sum-sha256sum-sha2/$SHA256/" $SRC/engine/.libs/lib$i.so >$LIBS/lib$i.so
done

FULLLIBS=$PWD/$LIBS
LIBCC=$SRC
for i in $LIBCC $*
do
    (cd $i; test -d shlibs && cp shlibs/*/.libs/*.so $FULLLIBS)
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
cp bin/Gforth-release.apk bin/Gforth.apk
#jarsigner -verbose -sigalg SHA1withRSA -digestalg SHA1 -keystore ~/.gnupg/bernd-release-key.keystore bin/Gforth$EXT.apk bernd
