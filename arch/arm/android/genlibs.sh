#!/bin/bash
#Copyright (C) 2015 Free Software Foundation, Inc.

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

. build.local

FREETYPE=freetype-2.5.3
HARFBUZZ=harfbuzz-0.9.36

#get dependent files

wget http://downloads.sourceforge.net/project/freetype/freetype2/${FREETYPE#*-}/$FREETYPE.tar.bz2
wget http://www.freedesktop.org/software/harfbuzz/release/$HARFBUZZ.tar.bz2

# support stuff

function patchlibtool {
    sed -e s/version_type=linux/version_type=none/g <$1 >$1+
    mv $1 $1~
    mv $1+ $1
    chmod +x $1
}

#make and install freetype, part 1 (no harfbuzz)
tar jxvf $FREETYPE.tar.bz2

(cd $FREETYPE
./autogen.sh # get fresh libtool&co
./configure --host=$TARGET --prefix=$TOOLCHAIN/sysroot/usr/ --with-png=no --with-bzip2=no --with-zlib=no --with-harfbuzz=no 
#for android, we don't need a library version
patchlibtool builds/unix/libtool
make -j4
make install)

#make and install harfbuzz
tar jxvf $HARFBUZZ.tar.bz2

(cd $HARFBUZZ
./autogen.sh --host=$TARGET --prefix=$TOOLCHAIN/sysroot/usr/ --with-glib=no --with-icu=no --with-uniscribe=no --with-cairo=no
patchlibtool libtool
make -j4
make install)

#now freetype with harfbuzz support

(cd $FREETYPE
./configure --host=$TARGET --prefix=$TOOLCHAIN/sysroot/usr/ --with-png=no --with-bzip2=no --with-zlib=no --with-harfbuzz=yes
patchlibtool builds/unix/libtool
make clean
make -j4
make install)

#freetype GL

if [ -f freetype-gl/.git/config ]
then
    (cd freetype-gl; git pull)
else
    git clone https://github.com/forthy42/freetype-gl.git
fi

(cd freetype-gl
./autogen.sh --host=$TARGET --prefix=$TOOLCHAIN/sysroot/usr/
patchlibtool libtool
make
make install
)

# SOIL2

if [ -f soil2/.hg/hgrc ]
then
    (cd soil2; hg pull; hg up)
else
    hg clone https://bitbucket.org/forthy42/soil2
fi

(cd soil2
premake4 --platform=arm-android gmake
(cd make/linux
make config=release)
(cd lib/linux
cp libsoil2.a $TOOLCHAIN/sysroot/usr/lib)
(cd src/SOIL2
cp SOIL2.h $TOOLCHAIN/sysroot/usr/include))

$TARGET-libtool  --tag=CC   --mode=link $TARGET-gcc  -O2   -o libtypeset.la -rpath /home/bernd/proj/android-toolchain/sysroot/usr/lib $(find $FREETYPE $HARFBUZZ freetype-gl -name '*.lo') -lm -lGLESv2 -lz -llog

cp .libs/libtypeset.{a,so} $TOOLCHAIN/sysroot/usr/lib
