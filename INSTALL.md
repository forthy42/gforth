# Install
## Build from git:

    git clone http://savannah.gnu.org/git/?group=gforth
    sudo apt-get install gforth libffi-dev libltdl-dev libsoil-dev libtool-bin yodl
    ./install-swig.sh # optional for C-bindings (e.g. OpenGL, posix )
    ./BUILD-FROM-SCRATCH
    sudo make install

## Alternative: Build from Tarball
If you are building from the tarball, please consult [INSTALL](INSTALL).


Authors: Gerald Wodni, Anton Ertl, Bernd Paysan
Copyright (C) 2016,2017 Free Software Foundation, Inc.
This file is free documentation; the Free Software Foundation gives
unlimited permission to copy, distribute and modify it.
