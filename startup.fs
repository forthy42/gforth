#! /usr/stud/paysan/bin/forth
\ startup file

warnings off

include search-order.fs
\ include etags.fs
include float.fs
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
