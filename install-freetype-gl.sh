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

# Sorry, SOIL2 on Darwin is meh
if [ `uname` != "Darwin" ]
then
    git clone https://github.com/forthy42/soil2.git
    (cd soil2 && premake4 $platform gmake && (cd make/$OS; make config=release) && (cp $(find . -name '*.a') $(find . -name '*.so') $TARGET) && (cp src/SOIL2/SOIL2.h /usr/include))
fi

git clone -b android https://github.com/forthy42/freetype-gl.git
(cd freetype-gl && ./autogen.sh && ./configure "$@" && make && sudo make install)
