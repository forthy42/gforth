#!/bin/sh
which sudo || alias sudo=eval
install_linux() {
  sudo apt-get -y update
  sudo apt-get -y install gforth gforth-lib gforth-common libffi-dev libltdl7 libsoil-dev libtool make gcc automake install-info dpkg-buildpackage yodl bison libpcre3-dev libboost-dev git g++ # yodl, bison, ... git: are for swig
  sudo apt-get -y install libtool-bin
  sudo apt-get -y install autoconf-archive
  sudo apt-get -y install libx11-dev
  sudo apt-get -y install libgles2-mesa-dev
  sudo apt-get -y install libgl1-mesa-dev
  sudo apt-get -y install libwayland-dev
  sudo apt-get -y install libharfbuzz-dev
  sudo apt-get -y install libvulkan-dev
  if [ `uname -m`$M32 = x86_64-m32 ]; then
    sudo apt-get -y --fix-missing install gcc-multilib libltdl7:i386
  fi
}

install_osx() {
  brew tap forthy42/homebrew-zsh
  brew update > /dev/null
  brew install yodl
  brew install gforth
  brew install gcc
  brew link --overwrite gcc
}

install_${TRAVIS_OS_NAME:-linux}
./install-swig.sh
