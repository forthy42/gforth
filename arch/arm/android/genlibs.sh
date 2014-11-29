#!/bin/bash
# Generate stuff needed for android Gforth

TOOLCHAIN=${TOOLCHAIN-~/proj/android-toolchain}
ARCH=armeabi
TARGET=arm-linux-androideabi

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

git clone https://github.com/forthy42/freetype-gl.git

(cd freetype-gl
./autogen.sh --host=$TARGET --prefix=$TOOLCHAIN/sysroot/usr/
patchlibtool libtool
make
make install
)

$TARGET-libtool  --tag=CC   --mode=link $TARGET-gcc  -O2   -o libtypeset.la -rpath /home/bernd/proj/android-toolchain/sysroot/usr/lib $(find $FREETYPE $HARFBUZZ freetype-gl -name '*.lo') -lm -lGLESv2 -lz -llog

cp .libs/libtypeset.* $TOOLCHAIN/sysroot/usr/lib
