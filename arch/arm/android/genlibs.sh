#!/bin/bash
# Generate stuff needed for android Gforth

TOOLCHAIN=${TOOLCHAIN-~/proj/android-toolchain}
ARCH=armeabi
TARGET=arm-linux-androideabi

FREETYPE=freetype-2.5.3
HARFBUZZ=harfbuzz-0.9.36

#get dependent files

wget $FREETYPE.tar.bz2
wget $HARFBUZZ.tar.bz2

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

git clone https://github.com/rougier/freetype-gl.git

(cd freetype-gl
./autogen.sh --host=$TARGET --prefix=$TOOLCHAIN/sysroot/usr/
make
make install
)

cp $TOOLCHAIN/sysroot/usr/lib/lib{harfbuzz,freetype,freetype-gl}.so libs/$ARCH
strip libs/$ARCH/lib{harfbuzz,freetype,freetype-gl}.so
