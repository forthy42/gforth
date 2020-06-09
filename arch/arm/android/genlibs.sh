#!/bin/bash
#Authors: Bernd Paysan, Anton Ertl
#Copyright (C) 2015,2016,2017,2018,2019 Free Software Foundation, Inc.

#This file is part of Gforth.

#Gforth is free software; you can redistribute it and/or
#modify it under the terms of the GNU General Public License
#as published by the Free Software Foundation, either version 3
#of the License, or (at your option) any later version.

#This program is distributed in the hope that it will be useful,
#but WITHOUT ANY WARRANTY; without even the implied warranty of
#MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
#GNU General Public License for more details.

#You should have received a copy of the GNU General Public License
#along with this program. If not, see http://www.gnu.org/licenses/.

# Generate stuff needed for android Gforth

nprocs=`nproc || echo 1`

. build.local
export PKG_CONFIG_PATH
TOOLCHAIN=$(which $TARGET-gcc | sed -e s,/bin/.*-gcc,,g)

case "$TARGET" in
    arm64*|aarch64*|mips64*)
	export CC="$TARGET-gcc -D__ANDROID_API__=21"
	;;
    *)
	export CC="$TARGET-gcc -D__ANDROID_API__=21"
	;;
esac

FREETYPE=freetype-2.10.1
HARFBUZZ=harfbuzz-2.6.2
LIBPNG=libpng-1.6.37
BZIP2=bzip2-1.0.8
OPUS=opus-1.3.1

fine=yes
for i in git wget
do
    if ! which $i >/dev/null 2>/dev/null
    then
	fine=no
	echo install $i please
    fi
done
if [ $fine = no ]
then
    Missing stuff, exiting
    exit 1
fi

#get dependent files

# support stuff

function gen_png {
    (cd ~/Downloads
     test -f $LIBPNG.tar.xz || wget https://downloads.sourceforge.net/project/libpng/libpng16/${LIBPNG#libpng-}/$LIBPNG.tar.xz)
    tar Jxvf ~/Downloads/$LIBPNG.tar.xz
    (cd $LIBPNG
     ./autogen.sh # get fresh libtool&co
     ./configure --host=$TARGET --prefix=$TOOLCHAIN/sysroot/usr/
     make -j$nprocs
     make install)
}

function gen_bzip2 {
    (cd ~/Downloads
     test -f $BZIP2.tar.gz || wget https://sourceware.org/pub/bzip2/$BZIP2.tar.gz)
    tar zxvf ~/Downloads/$BZIP2.tar.gz
    (cd $BZIP2
     PREFIX=$TOOLCHAIN/sysroot/usr
     make -j$nprocs CC="$CC -fPIC" libbz2.a
     cp -f libbz2.a $PREFIX/lib
     cp -f bzlib.h $PREFIX/include)
}

#make and install freetype, part 1 (no harfbuzz)

function gen_freetype {
    (cd ~/Downloads
     test -f $FREETYPE.tar.xz || wget https://sourceforge.net/projects/freetype/files/freetype2/${FREETYPE#*-}/$FREETYPE.tar.xz)
    tar Jxvf ~/Downloads/$FREETYPE.tar.xz
    (cd $FREETYPE
     ./autogen.sh # get fresh libtool&co
     ./configure --host=$TARGET --prefix=$TOOLCHAIN/sysroot/usr/ --with-png=yes --with-zlib=no --with-harfbuzz=no 
     make -j$nprocs
     make install)
}

#make and install harfbuzz

function gen_harfbuzz {
    (cd ~/Downloads
     test -f $HARFBUZZ.tar.xz || wget http://www.freedesktop.org/software/harfbuzz/release/$HARFBUZZ.tar.xz)
    tar Jxvf ~/Downloads/$HARFBUZZ.tar.xz
    (cd $HARFBUZZ
     ./autogen.sh --host=$TARGET --prefix=$TOOLCHAIN/sysroot/usr/ --with-glib=no --with-icu=no --with-uniscribe=no --with-cairo=no
     make -j$nprocs
     make install)
}

function gen_opus {
    (cd ~/Downloads
     test -f $OPUS.tar.gz || wget https://archive.mozilla.org/pub/opus/$OPUS.tar.gz)
    tar zxvf ~/Downloads/$OPUS.tar.gz
    (cd $OPUS
     ./configure --host=$TARGET --prefix=$TOOLCHAIN/sysroot/usr/ --libdir=$TOOLCHAIN/sysroot/usr/lib
     make -j$nprocs
     make install)
}

#now freetype with harfbuzz support

function gen_fthb {
    (cd $FREETYPE
     ./configure --host=$TARGET --prefix=$TOOLCHAIN/sysroot/usr/ --with-png=yes --with-bzip2=no --with-zlib=no --with-harfbuzz=yes
     make clean
     make -j$nprocs
     make install)
}

#freetype GL

function gen_ftgl {
    if [ -f freetype-gl/.git/config ]
    then
	(cd freetype-gl; git pull)
    else
	git clone -b android https://github.com/forthy42/freetype-gl.git
    fi
    
    (cd freetype-gl
     ./autogen.sh --host=$TARGET --prefix=$TOOLCHAIN/sysroot/usr/
     make -j$nprocs
     make install
    )
}

# SOIL2

function gen_soil2 {
    if [ -f soil2/.git/config ]
    then
	(cd soil2; git pull)
    else
	git clone https://github.com/forthy42/soil2.git
    fi
    
    (cd soil2
     case "$machine" in
	 386)
	     machine=x86
	     ;;
	 amd64)
	     machine=x86_64
	     ;;
     esac
     premake4 --platform=$machine-android gmake
     (cd make/linux
      make config=release)
     cp lib/linux/libsoil2.a $TOOLCHAIN/sysroot/usr/lib
     cp src/SOIL2/SOIL2.h $TOOLCHAIN/sysroot/usr/include)
}

function gen_typeset {
    $TARGET-libtool  --tag=CC   --mode=link $TARGET-gcc  -O2   -o libtypeset.la -rpath $TOOLCHAIN/sysroot/usr/lib $(find $HARFBUZZ -name libharfbuzz_la*.lo) $(find $FREETYPE $LIBPNG freetype-gl -name '*.lo') -lm -lGLESv2 -lz -lbz2 -llog
    cp .libs/libtypeset.{a,so} $TOOLCHAIN/sysroot/usr/lib
}

if [ "$1" = "" ]
then
    gen_png
    gen_bzip2
    gen_freetype
    gen_harfbuzz
    gen_opus
    gen_fthb
    gen_ftgl
    gen_soil2
    gen_typeset
else
    while [ "$1" != "" ]
    do
	eval $1
	shift
    done
fi


