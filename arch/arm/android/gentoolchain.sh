#!/bin/bash

NDK=${NDK-10e}
CPU=$(uname -p)

if [ ! -d ~/proj/android-ndk-r$NDK ]
then
    http://dl.google.com/android/ndk/android-ndk-r$NDK-linux-$CPU.bin
fi

for i in arm arm64 386 amd64 mips
do
    mkdir -p ~/proj/android-toolchain-$i
    (cd ~/proj/android-toolchain-$i
     ~/proj/android-ndk-r$NDK/build/tools/make-standalone-toolchain.sh --arch=arm --platform=android-14 --ndk-dir=/home/bernd/proj/android-ndk-r$NDK --install-dir=$PWD --toolchain=arm-linux-androideabi-4.9)
done
