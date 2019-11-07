#!/bin/sh
which sudo || alias sudo=eval

git clone https://github.com/GeraldWodni/swig.git
(cd swig && ./autogen.sh && ./configure "$@" && make && sudo make install)
