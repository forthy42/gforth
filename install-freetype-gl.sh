#!/bin/sh
which sudo || alias sudo=eval

case `uname`
in
    Linux)
	OS=linux
	TARGET=/usr/lib/`uname -p`-linux-gnu
	;;
    Darwin)
	OS=macosx
	TARGET=/usr/local/lib
	;;
esac

git clone https://github.com/forthy42/soil2.git
(cd soil2 && premake4 gmake && (cd make/$OS; make config=release) && (cd lib/$OS; cp libsoil2.a $TARGET) && (cd src/SOIL2; cp SOIL2.h /usr/include))
git clone -b android https://github.com/forthy42/freetype-gl.git
(cd freetype-gl && ./autogen.sh && ./configure && make && sudo make install)
