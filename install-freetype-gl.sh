#!/bin/sh
which sudo || alias sudo=eval

case `uname`
in
    Linux)
	OS=linux
	TARGET=/usr/lib/`uname -p`-linux-gnu
	if [ ! -d $TARGET ]
	then
	    TARGET=/usr/lib
	fi
	;;
    Darwin)
	OS=macosx
	TARGET=/usr/local/lib
	;;
esac
case "$CC" in
    *-m32*)
	platform="--platform=gcc32"
	;;
esac

git clone -b android https://github.com/forthy42/freetype-gl.git
(cd freetype-gl && ./autogen.sh && ./configure "$@" && make && sudo make install)
