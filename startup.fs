#! /usr/stud/paysan/bin/forth
\ startup file

warnings off

include float.fs
include search-order.fs
include environ.fs
\ include toolsext.fs
include wordinfo.fs
include vt100.fs
\ include colorize.fs
include see.fs
include bufio.fs
include debug.fs

0 Value $?
: sh  '# parse cr system  to $? ;

warnings on
