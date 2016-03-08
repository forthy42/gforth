#!/bin/bash

NDK=${NDK-10e}
CPU=$(uname -p)

if [ ! -d ~/proj/android-ndk-r$NDK ]
then
    (cd ~/Downloads
     wget http://dl.google.com/android/ndk/android-ndk-r$NDK-linux-$CPU.bin
     chmod +x android-ndk-r$NDK-linux-$CPU.bin
     mkdir -p ~/proj
     cd ~/proj
     ~/Downloads/android-ndk-r$NDK-linux-$CPU.bin)
fi

for i in arm aarch64 i686 x86_64 mipsel
do
    mkdir -p ~/proj/android-toolchain-$i
    (cd ~/proj/android-toolchain-$i
     ~/proj/android-ndk-r$NDK/build/tools/make-standalone-toolchain.sh --arch=arm --platform=android-21 --ndk-dir=/home/bernd/proj/android-ndk-r$NDK --install-dir=$PWD --toolchain=$i-linux-androideabi-4.9)
done
