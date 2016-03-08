#!/bin/bash

NDK=${NDK-10e}
LIBT=${LIBT-2.4.6}
CPU=$(uname -p)
CCVER=${CCVER-4.9}

if [ ! -d ~/proj/android-ndk-r$NDK ]
then
    (cd ~/Downloads
     wget http://dl.google.com/android/ndk/android-ndk-r$NDK-linux-$CPU.bin
     chmod +x android-ndk-r$NDK-linux-$CPU.bin
     mkdir -p ~/proj
     cd ~/proj
     ~/Downloads/android-ndk-r$NDK-linux-$CPU.bin)
fi

if [ ! -d ~/proj/libtool-$LIBT ]
then
    (cd ~/Downloads
     wget http://ftpmirror.gnu.org/libtool/libtool-$LIBT.tar.gz
     mkdir -p ~/proj
     cd ~/proj
     tar zxvf ~/Downloads/libtool-$LIBT.tar.gz)
fi

for i in arm aarch64 x86 x86_64 mipsel mips64el
do
    ARCH=$i
    case $i in
	arm)
	    ABI=-linux-androideabi
	    ;;
	aarch64)
	    ABI=-linux-android
	    ARCH=arm64
	    ;;
	mipsel|mips64el)
	    ARCH=${i%el}
	    ABI=-linux-android
	    ;;
	*)
	    ABI=""
	    ;;
    esac
    mkdir -p ~/proj/android-toolchain-$ARCH
    (cd ~/proj/android-toolchain-$ARCH
     ~/proj/android-ndk-r$NDK/build/tools/make-standalone-toolchain.sh --arch=$ARCH --platform=android-21 --ndk-dir=/home/bernd/proj/android-ndk-r$NDK --install-dir=$PWD --toolchain=$i$ABI-$CCVER)
    (cd ~/proj/libtool-$LIBT
     ./configure --host=$i$ABI --program-prefix=$i$ABI- --prefix=$HOME/proj/android-toolchain-$ARCH host_alias=$CPU-linux-gnu
     make && make install && make clean
    )
done

cat <<EOF
Add the following line to your .bashrc:
PATH=\$PATH:\$(echo ~/proj/android-toolchain-{arm,arm64,x86,x86_64,mips,mips64}/bin | tr ' ' ':')
EOF
