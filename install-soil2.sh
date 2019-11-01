#!/bin/bash

case `uname`
in
    Linux)
	OS=linux
	;;
    Darwin)
	OS=macosx
	;;
esac

function gen_soil2 {
    if [ -f soil2/.git/config ]
    then
	(cd soil2; git pull)
    else
	git clone https://github.com/forthy42/soil2.git
    fi
    
    (cd soil2
     case "CC" in
	 *-m32*)
	     machine=gcc32
	     premake4 --platform=$machine gmake
	     ;;
	 *)
	     premake4 gmake
     esac
     (cd make/$OS
      make config=release)
     cp lib/$OS/libsoil2.a $TOOLCHAIN/sysroot/usr/lib
     cp src/SOIL2/SOIL2.h $TOOLCHAIN/sysroot/usr/include)
}
gen_soil2
