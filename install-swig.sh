#!/bin/sh
case "$1"
in
    --nosudo)
	alias sudo=eval
	shift
	;;
    *)
	which sudo || alias sudo=eval
	;;
esac
case "$1"
in
    --nonproc)
	alias sudo="echo 1"
	shift
	;;
    *)
	which nproc || alias nproc="echo 1"
	;;
esac


git clone https://github.com/GeraldWodni/swig.git
(cd swig && ./autogen.sh && ./configure --program-suffix=-forth "$@" && make -j`nproc` && sudo make install)
