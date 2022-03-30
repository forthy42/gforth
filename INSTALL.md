# Install from source

Gforth's build process is partially self-hosted, i.e. it needs a working
Gforth.  For the tarball, everything needed to build a working Gforth is
included, but from git, you need to install at least an outdated version of
Gforth to build successfully.

For building the C interface files, a Swig fork is needed.  The tarball also
contains all files to create these interface files when the Swig fork is not
available.

The script `install-deps.sh` tries to install all necessary build
dependencies, depending on the variable `BUILD_FROM` (if set to `tarball`,
it's a tarball build, otherwise, everything for a git build will be installed).

## Build from git

    git clone https://git.savannah.gnu.org/git/gforth.git
    cd gforth
    source ./install-deps.sh # install all known dependencies for a full build
    ./BUILD-FROM-SCRATCH
    sudo make install

## Build from tarball

    BUILD_FROM=tarball
    source ./install-deps.sh # install only the dependencies for a tarball build
    ./configure
    make
    sudo make install

# Configuration Options

## Build options

If you use GNU make, you can build in a directory different from the
source directory by changing to the build directory and invoking
configure thus:

    $srcdir/configure

where `$srcdir` is the source directory.

configure has the following useful parameters:

    --prefix=PREFIX         install architecture-independent files in PREFIX
                            [default: /usr/local]
    --exec-prefix=PREFIX    install architecture-dependent files in PREFIX
                            [default: same as prefix]
    --help: tells you about other parameters.

The file `Benchres` shows the best gforth-fast performance that we
achieved.

If you don't like the defaults for the installation directories, you
should override them already during configure.  E.g., if you want to
install in the /gnu hierarchy instead of in the default /usr/local
hierarchy, say

    ./configure --prefix=/gnu

Moreover, if your GCC is not called gcc (but, e.g., gcc-2.7.1), you
should say so during configuration. E.g.:

    ./configure CC=gcc-2.7.1

You can also pass additional options to gcc in this way, e.g., if you
want to generate an a.out executable under Linux with gcc-2.7.0:

    ./configure CC="gcc -b i486-linuxaout -V 2.7.0"

You can change the sizes of the various areas used in the default
image `gforth.fi' by passing the appropriate Gforth command line
options in the FORTHSIZES environment variable:

    ./configure "FORTHSIZES=--dictionary-size=1048576 --data-stack-size=16k --fp-stack-size=16K --return-stack-size=15k --locals-stack-size=14848b"

The line above reaffirms the default sizes. Note that the locals
stack area is also used as input buffer stack.

If C's "long long" do not work properly on your machine (i.e., if the
tests involving double-cell numbers fail), you can build Gforth such
that it does not use "long long":

    ./configure ac_cv_sizeof_long_long=0

For MacOS X on Core 2 processors, you might want to use the 64-bit
version for increased speed (more registers available); you have to
ask for that on configuration, as follows:

    ./configure CC='gcc-4.2 -arch x86_64' --build=x86_64-apple-darwin9.4.0

## Cross installation

For systems like Android, where you don't build on the actual system, but do
cross building, there is a

    --with-cross=<subdir>

switch for `configure`.  This will cause configure to disable things that
don't work in cross building, an source a file `config.sh` in the subdir of
the `arch/`_<cpu>_ directory (e.g. `arch/arm64/android`).  This file needs to
contain all the check results like `ac_cv_sizeof_void_p` that can be done on a
hosted system.

This approach has not been prepared for other systems, where you usually can
get a sufficiently similar environment to build, so if you need a cross
installation there, you should have a look at how it is done for Android.

## Preloading installation-specific code

If you want to have some installation-specific files loaded when
Gforth starts (e.g., an assembler for your processor), put commands
for loading them into `/usr/local/share/gforth/site-forth/siteinit.fs`
(if the commands work for all architectures) or
`/usr/local/lib/gforth/site-forth/siteinit.fs` (for
architecture-specific commands);
`/usr/local/lib/gforth/site-forth/siteinit.fs` takes precedence if both
files are present (unless you change the search path). The file names
given above are the defaults; if you have changed the prefix, you have
to replace "`/usr/local`" in these names with your prefix.

By default, the installation procedure creates an empty
`/usr/local/share/gforth/site-forth/siteinit.fs` if there is no such
file.

If you change the siteinit.fs file, you should run "`make install`"
again for the changes to take effect (Actually, the part of "`make
install`" starting with "`rm gforth.fi`" is sufficient).

## Multiple Versions and Deinstallation

Several versions of Gforth can be installed and used at the same
time. Version `foo` can be invoked with `gforth-foo`. We recommend to
keep the old version for some time after a new one has been installed.

You can deinstall this version of Gforth with `make uninstall` and
version foo with `make uninstall VERSION=foo`. `make uninstall` also
tells you how to uninstall Gforth completely.

## Installing Info Files

Info is the GNU project on-line documentation format. You can read
info files either from within Emacs (Ctrl-h i) or using the
stand-alone Info reader, `info`.

If you use the default install root of `/usr/local` then the info
files will be installed in `/usr/local/info`.

Many GNU/Linux distributions are set up to put all of their
documentation in `/usr/info`, in which case you might have to do a
couple of things to get your environment set up to accommodate files
in both areas:

1. Add an `INFOPATH` environment variable. The easiest place to do
this is `/etc/profile`, right next to `PATH` and `MANPATH`:

    INFOPATH=/usr/local/info:/usr/info

2. Create a file called `dir` in `usr/local/info`. Use the file
`/usr/info/dir` as a template. You can add the line for gforth
manually, or use `/sbin/install-info` (`man install-info` for details).

## Additional info for MacOS ##

You'll get brew and XCode command line tools installed (git will trigger the
latter) if it is not already there.  The `install-deps.sh` exports a few
variables, and you probably should put those into your shell setup if you want
to build Gforth latest without sourcing `install-deps.sh` again.

# Install binaries
## Packets for Debian GNU/Linux

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

## Flatpak

    flatpak remote-add --if-not-exists net2o https://flathub.net2o.net/repo/net2o.flatpakrepo
    flatpak install org.gforth.gforth

Since Flatpaks manage access to external resources, you want the following two
aliases, one for terminal applications, the other if you use MINOS2 GUI
applications:

    alias gforth="flatpak run --filesystem=$PWD org.gforth.gforth"
    alias gforth-gui="flatpak run --socket=x11 --device=dri --socket=pulseaudio --filesystem=$PWD org.gforth.gforth"

## Docker

There are different builts for terminal only Gforth, GUI (with MINOS2) and GUI
with all the necessary fonts:

    docker pull forthy42/gforth
    docker pull forthy42/gforth-gui
    docker pull forthy42/gforth-gui-fonts

Like Flatpak, you want aliases to run it with the necessary permissions

    alias gforthdk="docker run -ti --rm forthy42/gforth"
    alias gforth-guidk="docker run -ti -e USER=$USER -e DISPLAY=$DISPLAY -v /tmp/.X11-unix/:/tmp/.X11-unix/ -v /dev/dri:/dev/dri -v /usr/share/fonts:/usr/share/fonts -v $XAUTHORITY:/home/gforth/.Xauthority -v ${XDG_RUNTIME_DIR}/pulse:/run/user/1000/pulse --rm forthy42/gforth-gui"
    alias gforth-gui-fontsdk="docker run -ti -e USER=$USER -e DISPLAY=$DISPLAY -v /tmp/.X11-unix/:/tmp/.X11-unix/ -v /dev/dri:/dev/dri -v $XAUTHORITY:/home/gforth/.Xauthority -v ${XDG_RUNTIME_DIR}/pulse:/run/user/1000/pulse --rm forthy42/gforth-gui-fonts"

## Snap

    sudo snap install gforth

# About

Authors: Gerald Wodni, Anton Ertl, Bernd Paysan
Copyright (C) 2016,2017,2019,2020,2021 Free Software Foundation, Inc.
This file is free documentation; the Free Software Foundation gives
unlimited permission to copy, distribute and modify it.
