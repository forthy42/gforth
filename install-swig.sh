#!/bin/sh
which sudo || alias sudo=eval
which nproc || alias nproc="echo 1"

git clone https://github.com/forthy42/swig.git
(cd swig && ./autogen.sh && ./configure "$@" && make -j`nproc` && sudo make install)
