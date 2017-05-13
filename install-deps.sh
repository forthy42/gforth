#!/bin/sh

install_linux() {
  sudo apt-get update
  sudo apt-get install gforth gforth-lib gforth-common
  if [ `uname -m`$M32 = x86_64-m32 ]; then
      sudo apt-get install libtool-bin:i386
      sudo apt-get --fix-missing install gcc-multilib libltdl7-dev:i386 libsoil-dev:i386 libffi-dev:i386
  else
      sudo apt-get install libsoil-dev libltdl7-dev libffi-dev
  fi
}

install_osx() {
  brew tap zchee/homebrew-zsh
  brew update > /dev/null
  brew install gforth
  brew install gcc
}

install_${TRAVIS_OS_NAME:-linux}

