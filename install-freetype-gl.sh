#!/bin/sh
which sudo || alias sudo=eval

git clone https://github.com/forthy42/soil2.git
(cd soil2 && premake4 gmake && (cd make/linux; make config=release) && (cd lib/linux; cp libsoil2.a /usr/lib/x86_64-linux-gnu) && (cd src/SOIL2; cp SOIL2.h /usr/include))
git clone -b android https://github.com/forthy42/freetype-gl.git
(cd freetype-gl && ./autogen.sh && ./configure && make && sudo make install)
