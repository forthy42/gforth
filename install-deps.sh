#!/bin/sh

install_linux() {
  sudo apt-get update
  sudo apt-get install gforth gforth-lib gforth-common libffi-dev libltdl7 libsoil-dev libtool install-info yodl bison libpcre3-dev libboost-dev # yodl, bison... are for swig
  sudo apt-get install libtool-bin
  sudo apt-get install libx11-dev
  sudo apt-get install libgles2-mesa-dev
  sudo apt-get install libgl1-mesa-dev
  if [ `uname -m`$M32 = x86_64-m32 ]; then
    sudo apt-get --fix-missing install gcc-multilib libltdl7:i386
  fi
}

install_osx() {
  brew tap zchee/homebrew-zsh
  brew update > /dev/null
  brew install yodl
  brew install gforth
}

install_${TRAVIS_OS_NAME:-linux}
./install-swig.sh
