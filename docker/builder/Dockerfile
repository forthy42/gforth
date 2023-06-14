ARG VERSION=unstable
ARG ARCH=amd64
FROM $ARCH/debian:$VERSION
LABEL maintainer="Bernd Paysan <bernd@net2o.de>"
ENV LANG C.UTF-8

RUN apt-get -y update; apt-get -y install gnupg \
  && apt-key adv --keyserver keyserver.ubuntu.com --recv-keys 0E98404D386FA1D9 6ED0E7B82643E131 \
  && apt-get -y update \
  && apt-get upgrade -y \
  && apt-get -y install libffi-dev libltdl7 libsoil-dev libtool make gcc automake texinfo texi2html texlive-base texlive-latex-base install-info dpkg-dev debhelper gforth gforth-lib gforth-common libtool-bin libltdl-dev autoconf-archive libx11-dev libx11-xcb-dev libxrandr-dev libgles2-mesa-dev libglew-dev libgl1-mesa-dev libwayland-dev libvulkan-dev libpng-dev libfreetype6-dev libgstreamer1.0-dev libgstreamer-plugins-base1.0-dev libopus-dev libva-dev libavcodec-dev libavutil-dev libstb-dev \
  && if [ `lsb_release -sc` = "buster" ]; then sudo apt-get -y install texlive-latex-base; fi \
  && apt-get clean
RUN cd /tmp/ \
    && apt-get install -y yodl bison libpcre3-dev libboost-dev git g++ \
    && git clone https://github.com/forthy42/swig.git \
    && cd swig \
    && ./autogen.sh \
    && ./configure --prefix=/usr --exec-prefix=/usr \
    && make \
    && make install \
    && cd .. \
    && rm -rf swig \
    && apt-get remove -y yodl bison libpcre3-dev libboost-dev git g++ \
    && apt-get clean
RUN apt-get install -y libpulse-dev libharfbuzz-dev \
  && apt-get clean
