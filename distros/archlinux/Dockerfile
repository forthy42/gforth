FROM archlinux:latest

# install basic packages and add user forther (in group sudo)
RUN pacman -Sy --noconfirm sudo git base-devel libffi wget libtool && \
    groupadd sudo && \
    useradd -m -G sudo forther && \
    echo "%sudo ALL=(ALL:ALL) NOPASSWD: ALL" >> /etc/sudoers

# install yay
USER forther
WORKDIR /home/forther
RUN git clone https://aur.archlinux.org/yay.git && \
    cd yay && \
    makepkg --noconfirm -si && \
    cd ..

# get gforth tarball
RUN wget https://www.complang.tuwien.ac.at/forth/gforth/gforth-0.7.3.tar.gz && \
    wget https://www.complang.tuwien.ac.at/forth/gforth/gforth-0.7.3.patch && \
    tar xzf gforth-0.7.3.tar.gz && \
    (cd gforth-0.7.3; patch -p0 <../gforth-0.7.3.patch) && \
    (cd gforth-0.7.3; CFLAGS='-std=gnu99' ./configure --prefix=/usr) && \
    (cd gforth-0.7.3; make PREFIX=/usr -j1; touch gforth.elc) && \
    (cd gforth-0.7.3; sudo make PREFIX=/usr -j1 install) && \
    rm -rf gforth-0.7.3*
