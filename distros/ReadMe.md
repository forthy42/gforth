# Distro folder

this folder is intended to house distribution or build-system specific processes.

## HowTo:
Each folder needs to habe a `build.sh` file which is to be run locally and cannot require any additional parameters.
For now it is fine if building only works on devs' laptops. In the future this should be in a CI/CD.

## Contents

### archlinux

Archlinux uses aur-packages, we maintain (currently: fiz/Gerald Wodni) [gforth-git](https://aur.archlinux.org/packages/gforth-git)

#### devel-testing
In order to test the compilation in arch linux, please use the following commands:
```bash
cd archlinux
docker build --tag archlinux-gforth:latest .
docker run -it --rm archlinux-gforth:latest
```

in the container try to get stuff running:
```bash
cd gforth-0.7.3
CFLAGS='-std=gnu99' ./configure --prefix=/usr # works
make PREFIX=/usr -j1 # I cannot get this to complete successfully, but we end up with a gforth binary
```

in gforth c-libraries seem broken:
```
c-library cstr
c-function cstr       cstr       a n n -- a ( c-addr u fclear -- c-addr2 )
end-c-library
```

it fails like so:
```
file not found
:3: open-lib failed
>>>end-c-library<<<
Backtrace:
$7F95DCC04988 throw 
$7F95DCC40AD8 c(abort") 
$7F95DCC41388 compile-wrapper-function1 
```

