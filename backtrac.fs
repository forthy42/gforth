\ backtrace handling

\ Copyright (C) 1999,2000,2003,2004,2006,2007,2012,2013 Free Software Foundation, Inc.

\ This file is part of Gforth.

\ Gforth is free software; you can redistribute it and/or
\ modify it under the terms of the GNU General Public License
\ as published by the Free Software Foundation, either version 3
\ of the License, or (at your option) any later version.

\ This program is distributed in the hope that it will be useful,
\ but WITHOUT ANY WARRANTY; without even the implied warranty of
\ MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
\ GNU General Public License for more details.

\ You should have received a copy of the GNU General Public License
\ along with this program. If not, see http://www.gnu.org/licenses/.


\ backtrace stuff

: backtrace-return-stack ( -- addr u )
    \ addr is the address of top element of return stack (the stack
    \ grows downwards), u is the number of aus that interest us on the
    \ stack.
    rp@ in-return-stack?
    if
	rp@ [ 2 cells ]L +
    else \ throw by signal handler with insufficient information
	handler @ cell - \ beyond that we know nothing
    then
    backtrace-rp0 @ [ 1 cells ]L - over - 0 max ;

:noname ( -- )
    backtrace-return-stack first-throw $! ;
IS store-backtrace

: print-bt-entry ( return-stack-item -- )
    cell - dup in-dictionary? over dup aligned = and
    if
	@ dup threaded>name dup if
	    .name drop
	else
	    drop dup look if
		.name drop
	    else
		drop body> look \ !! check for "call" in cell before?
		if
		    .name
		else
		    drop
		then
	    then
	then
    else
	drop
    then ;

: print-backtrace ( addr1 addr2 -- )
    \G print a backtrace for the return stack addr1..addr2
    2dup u< IF  cr ." Backtrace:"  THEN
    swap u+do
	cr
	i @ dup hex. ( return-addr? )
	print-bt-entry
	cell +loop ;

: bt ( -- )
    \G backtrace for interactive use
    backtrace-rp0 @ #10 cells + dup 3 cells - @ cell- print-backtrace ;
comp: drop ]] store-backtrace dobacktrace nothrow [[ ;

:noname ( -- )
    first-throw $@ over + print-backtrace ;
IS dobacktrace

[ifdef] defer-default
:noname
    r@ >stderr cr ." deferred word " print-bt-entry ." is uninitialized" ;
is defer-default
[then]