#!/bin/sh
which sudo || alias sudo=eval
install_linux() {
  sudo apt-get -y update
  sudo apt-get -y install libffi-dev libltdl7 libsoil-dev libtool make gcc automake texinfo texi2html texlive install-info dpkg-dev debhelper yodl bison libpcre3-dev libboost-dev git g++ premake4 # yodl, bison, ... git: are for swig
  sudo apt-get -y install gforth gforth-lib gforth-common
  sudo apt-get -y install libtool-bin
  sudo apt-get -y install libltdl-dev
  sudo apt-get -y install autoconf-archive
  sudo apt-get -y install libx11-dev
  sudo apt-get -y install libx11-xcb-dev
  sudo apt-get -y install libxrandr-dev
  sudo apt-get -y install libgles2-mesa-dev
  sudo apt-get -y install libgl1-mesa-dev
  sudo apt-get -y install libwayland-dev
  sudo apt-get -y install libharfbuzz-dev
  sudo apt-get -y install libvulkan-dev
  sudo apt-get -y install libpng-dev
  sudo apt-get -y install libfreetype6-dev
  sudo apt-get -y install libgstreamer1.0-dev
  sudo apt-get -y install libgstreamer-plugins-base1.0-dev
  sudo apt-get -y install libpulse-dev
  sudo apt-get -y install libopus-dev
  sudo apt-get -y install libva-dev
  sudo apt-get -y install libavcodec-dev libavutil-dev
  if [ `uname -m`$M32 = x86_64-m32 ]; then
    sudo apt-get -y --fix-missing install gcc-multilib
    sudo apt-get -y install libx11-dev:i386
    sudo apt-get -y install libgles2-mesa-dev:i386
    sudo apt-get -y install libgl1-mesa-dev:i386
    sudo apt-get -y install libwayland-dev:i386
    sudo apt-get -y install libharfbuzz-dev:i386
    sudo apt-get -y install libvulkan-dev:i386
    sudo apt-get -y install libpng-dev:i386
    sudo apt-get -y install libfreetype6-dev:i386
    sudo apt-get -y install libgstreamer1.0-dev:i386
    sudo apt-get -y install libgstreamer-plugins-base1.0-dev:i386
  fi
}

install_osx() {
  which brew || /usr/bin/ruby -e "$(curl -fsSL https://raw.githubusercontent.com/Homebrew/install/master/install)"
  brew tap forthy42/homebrew-zsh
  brew update > /dev/null
  brew upgrade > /dev/null
  brew install gforth gcc harfbuzz texinfo xz mesa premake automake yodl
  export PATH="/usr/local/opt/texinfo/bin:$PATH"
  brew cask install xquartz mactex
  export PATH="/Library/TeX/texbin:$PATH"
  brew link --overwrite gcc
  export CC=gcc-9
#  (cd /usr/local/Cellar/gcc/8.2.0/lib/gcc/8/gcc/x86_64-apple-darwin17.7.0/8.2.0/include-fixed && mv stdio.h stdio.h.botched)
}

case `uname` in
    Linux)
	OS=linux
	;;
    Darwin)
	OS=osx
	;;
esac

install_${TRAVIS_OS_NAME:-$OS}
./install-swig.sh
./install-freetype-gl.sh
#./install-soil2.sh
