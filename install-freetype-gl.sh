#!/bin/sh
which sudo || alias sudo=eval

git clone -b android https://github.com/forthy42/freetype-gl.git
(cd freetype-gl && ./autogen.sh && ./configure && make && sudo make install)
