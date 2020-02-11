# Install
## Build from git:

    git clone https://git.savannah.gnu.org/git/gforth.git
    cd gforth
    source ./install-deps.sh # install all known dependencies for a full build
    ./BUILD-FROM-SCRATCH
    sudo make install

## Additional info for MacOS ##

You'll get brew and XCode command line tools installed (git will trigger the
latter) if it is not already there.  The `install-deps.sh` exports a few
variables, and you probably should put those into your shell setup if you want
to build Gforth latest without sourcing `install-deps.sh` again.

## Alternative: Build from Tarball
If you are building from the tarball, please consult [INSTALL](INSTALL).

Authors: Gerald Wodni, Anton Ertl, Bernd Paysan
Copyright (C) 2016,2017,2019 Free Software Foundation, Inc.
This file is free documentation; the Free Software Foundation gives
unlimited permission to copy, distribute and modify it.
