#!/bin/sh
which sudo || alias sudo=eval
install_debian() {
  sudo apt-get -y update
  sudo apt-get -y install libffi-dev libltdl7 libsoil-dev libtool make gcc automake texinfo texi2html texlive-base install-info dpkg-dev debhelper yodl bison libpcre3-dev libboost-dev git g++ # yodl, bison, ... git: are for swig
  test `lsb_release -sc` = "buster" && sudo apt-get -y install texlive-latex-base
  sudo apt-get -y install libtool-bin
  sudo apt-get -y install libltdl-dev
  sudo apt-get -y install autoconf-archive
  sudo apt-get -y install libx11-dev
  sudo apt-get -y install libx11-xcb-dev
  sudo apt-get -y install libxrandr-dev
  sudo apt-get -y install libgles2-mesa-dev libglew-dev
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
  sudo apt-get -y install libstb-dev
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

install_alpine() {
    sudo apk add libltdl libffi
    sudo apk add wget file xz tar
    sudo apk add freetype-dev
        build-base autoconf automake m4 libtool git \
        coreutils gcc libffi-dev mesa-dev glew-dev libx11-dev \
        libxrandr-dev glfw-dev harfbuzz-dev gstreamer-dev gst-plugins-base-dev \
	opus-dev pulseaudio-dev unzip texinfo
    (cd /tmp && git clone https://github.com/nothings/stb.git \
    sudo mkdir /usr/include/stb && sudo cp stb/*.h /usr/include/stb && rm -rf stb)
}

install_fedora() {
    sudo dnf -y install wget file xz tar
    sudo dnf -y install freetype-devel
	@development-tools autoconf automake m4 \
	libtool libtool-ltdl libtool-ltdl-devel git \
        coreutils gcc libffi-devel mesa-devel glew-devel libx11-devel \
        libXrandr-devel glfw-devel harfbuzz-devel gstreamer-devel gst-plugins-base-devel \
	opus-devel pulseaudio-devel unzip texinfo
    (cd /tmp && git clone https://github.com/nothings/stb.git \
    sudo mkdir /usr/include/stb && sudo cp stb/*.h /usr/include/stb && rm -rf stb)
}

install_linux() {
    install_debian
}

install_osx() {
  which brew || /bin/bash -c "$(curl -fsSL https://raw.githubusercontent.com/Homebrew/install/HEAD/install.sh)"
  brew tap forthy42/homebrew-zsh
  brew update > /dev/null
  brew upgrade > /dev/null
  brew install gcc harfbuzz texinfo xz mesa premake automake yodl
  export PATH="/usr/local/opt/texinfo/bin:$PATH"
  brew install --cask xquartz mactex
  export PATH="/Library/TeX/texbin:$PATH"
  brew link --overwrite gcc
  export CC=gcc-11
#  (cd /usr/local/Cellar/gcc/8.2.0/lib/gcc/8/gcc/x86_64-apple-darwin17.7.0/8.2.0/include-fixed && mv stdio.h stdio.h.botched)
}

install_gforth_osx() {
    brew install gforth
}

install_gforth_debian() {
    sudo apt-get -y install gforth gforth-lib gforth-common
}

install_gforth_alpine() {
    sudo apk add gforth
}

install_gforth_fedora() {
    sudo dnf -y install gforth
}

case `uname` in
    Linux)
	OS=`. /etc/os-release; echo ${ID%-*}`
	;;
    Darwin)
	OS=osx
	;;
esac

install_gforth() {
    install_gforth_${TRAVIS_OS_NAME:-$OS}
}

install_${TRAVIS_OS_NAME:-$OS}

case $BUILD_FROM in
    tarball)
	;;
    *)
	install_gforth
	./install-swig.sh
	;;
esac

which sudo || unalias sudo
