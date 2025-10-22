#!/bin/sh
which sudo || alias sudo=eval
install_debian() {
  sudo apt-get -y update
  sudo apt-get -y install libffi-dev libltdl7 libsoil-dev libtool make gcc automake m4 texinfo texi2html texlive-base install-info dpkg-dev debhelper yodl bison libboost-dev git g++ # yodl, bison, ... git: are for swig
  case "`lsb_release -sc`" in
      trixie|forky)
	  git clone https://github.com/nektro/pcre-8.45.git
	  (cd pcre-8.45; ./configure && sed -e 's/1[.]16/1.17/g' <Makefile >Makefile.new; mv Makefile.new Makefile; make && sudo make install)
	  ;;
      *) sudo apt-get install -y libpcre3-dev
	  ;;
  esac  
  test `lsb_release -sc` = "forky" && sudo apt-get -y install texlive-base texlive-latex-base
  sudo apt-get -y install libtool-bin
  sudo apt-get -y install libltdl-dev
  sudo apt-get -y install libffi-dev
  sudo apt-get -y install autoconf-archive
  sudo apt-get -y install libx11-dev
  sudo apt-get -y install libx11-xcb-dev
  sudo apt-get -y install libxrandr-dev
  sudo apt-get -y install libxkbcommon-dev 
  sudo apt-get -y install libgles2-mesa-dev libglew-dev
  sudo apt-get -y install libgl1-mesa-dev
  sudo apt-get -y install libwayland-dev wayland-protocols
  sudo apt-get -y install libharfbuzz-dev
  sudo apt-get -y install libvulkan-dev
  sudo apt-get -y install libpng-dev
  sudo apt-get -y install libwebp-dev
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
    sudo apk add freetype-dev build-base autoconf automake m4 libtool git \
        coreutils gcc libffi-dev mesa-dev glew-dev libx11-dev \
        libxrandr-dev glfw-dev harfbuzz-dev gstreamer-dev gst-plugins-base-dev \
	opus-dev pulseaudio-dev pipewire-dev wayland-dev unzip texinfo wayland-protocols libxkbcommon-dev libwebp-dev
    (cd /tmp && git clone https://github.com/nothings/stb.git && \
    sudo mkdir /usr/include/stb && sudo cp stb/*.h /usr/include/stb && rm -rf stb)
}

install_fedora() {
    sudo dnf -y install wget file xz tar
    sudo dnf -y install freetype-devel
	@development-tools autoconf automake m4 \
	libtool libtool-ltdl libtool-ltdl-devel git \
        coreutils gcc libffi-devel mesa-devel glew-devel libx11-devel \
        libXrandr-devel glfw-devel harfbuzz-devel gstreamer-devel gst-plugins-base-devel wayland-protocols-devel libxkbcommon-devel \
	opus-devel libwebp-devel pulseaudio-devel unzip texinfo
    (cd /tmp && git clone https://github.com/nothings/stb.git && \
    sudo mkdir /usr/include/stb && sudo cp stb/*.h /usr/include/stb && rm -rf stb)
}

install_opensuse() {
    sudo zypper install -y libtool libltdl7 Mesa-libGL-devel \
    Mesa-libglapi-devel glew-devel vulkan-devel gpsd-devel \
    Mesa-libGLESv2-devel Mesa-libGLESv3-devel libpng16-devel stb-devel \
    freetype2-devel harfbuzz-devel libpulse-devel libopus-devel \
    libva-devel libva-gl-devel linux-glibc-devel libxkbcommon-devel \
    makeinfo texinfo info wayland-devel wayland-protocols-devel m4 \
    emacs-nox libffi-devel libX11-devel libwebp-devel
}

install_linux() {
    install_debian
}

install_ubuntu() {
    install_debian
}

install_linuxmint() {
    install_debian
}

install_osx() {
  which brew || /bin/bash -c "$(curl -fsSL https://raw.githubusercontent.com/Homebrew/install/HEAD/install.sh)"
  brew tap forthy42/homebrew-zsh
  brew update > /dev/null
  brew upgrade > /dev/null
  brew install gcc harfbuzz texinfo xz mesa premake automake yodl libtool
  export PATH="/usr/local/opt/texinfo/bin:$PATH"
  brew install --cask xquartz mactex
  export PATH="/Library/TeX/texbin:$PATH"
  brew link --overwrite gcc
  INSTALLED_GCC=$(basename $(\ls /usr/local/bin/gcc-* | /usr/bin/grep -E 'gcc-[0-9]+$' | /usr/bin/sort --version-sort | /usr/bin/tail -1))
  CC=$INSTALLED_GCC
#  (cd /usr/local/Cellar/gcc/8.2.0/lib/gcc/8/gcc/x86_64-apple-darwin17.7.0/8.2.0/include-fixed && mv stdio.h stdio.h.botched)
}

install_gforth_osx() {
    brew install gforth
}

install_gforth_debian() {
    case "`lsb_release -sc`" in
	trixie|forky)
            wcurl https://www.complang.tuwien.ac.at/forth/gforth/gforth-0.7.3.tar.gz
       	    tar zxf gforth-0.7.3.tar.gz
	    BARCH=$(bash --version | grep -w bash | sed -e 's/.*(\([^ ]*\))$/\1/g')
	    (cd gforth-0.7.3; ./configure CC=gcc-14 --prefix=/usr --host=$BARCH --build=$BARCH; make; sudo make install)
	    ;;
	*) sudo apt-get -y install gforth gforth-lib gforth-common
	    ;;
    esac
}

install_gforth_ubuntu() {
    install_gforth_debian
}

install_gforth_linuxmint() {
    install_gforth_debian
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
	./install-swig.sh --nosudo "--prefix=$PWD/local"
	export PATH="$PATH:$PWD/local/bin"
	;;
esac

which sudo || unalias sudo
