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
Copyright (C) 2016,2017,2019,2020 Free Software Foundation, Inc.
This file is free documentation; the Free Software Foundation gives
unlimited permission to copy, distribute and modify it.

## Alternative: Packets for Debian GNU/Linux

You can use the following Debian repository to make it easy to install
Gforth.  For the following commands, you need to be root (or prepend all the
`apt` commands with `sudo`):

If you don't have https transport for apt installed, do that first:

    apt install apt-transport-https

Create a debian sources.list file pointing to the net2o repository,
and add the key to the trust db so that Debian can verify the packets,
update the repository data and install net2o, so enter:

    cat >/etc/apt/sources.list.d/net2o.list <<EOF
    deb [arch=i386,amd64,armhf,armel,arm64,powerpc,mips,mipsel,all] https://net2o.de/debian testing main
    EOF
    wget -O - https://net2o.de/bernd@net2o.de-yubikey.pgp.asc | apt-key add -

Remove the architectures on the list above which you don't need; on Debian
stable, the list is not necessary anymore.  On older versions (oldstable), the
“`all`” part is not searched if you don't have that list, then Gforth fails to
install the “`gforth-common`” part (and others that are not architecture
dependent).

Now you are ready to install:

    apt update
    apt install gforth

There are actually four repositories: oldstable, stable, testing and unstable,
compiled for the respective Debian systems.
