#!/bin/sh
gforth arch/4stack/relocate.fs \
 -e "s\" $1-\" read-gforth s\" ../4stack/${1%%fi}4o\" write-gforth bye"
cp $1- $1
