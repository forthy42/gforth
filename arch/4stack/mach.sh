#!/bin/sh
./gforth arch/4stack/relocate.fs \
 -e "s\" $1-\" read-gforth s\" arch/4stack/gforth.4o\" write-gforth bye"
cp $1- $1
