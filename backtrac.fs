\ backtrace handling

\ Copyright (C) 1999,2000,2003,2004,2006,2007 Free Software Foundation, Inc.

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


\ growing buffers that need not be full

struct
    cell% 0 * field buffer-descriptor \ addr u
    cell% field buffer-length
    cell% field buffer-address
    cell% field buffer-maxlength \ >=length
end-struct buffer%

: init-buffer ( addr -- )
    buffer% %size erase ;

: adjust-buffer ( u addr -- )
    \G adjust buffer% at addr to length u
    \ this may grow the allocated area, but never shrinks it
    dup >r buffer-maxlength @ over <
    if ( u )
	r@ buffer-address @ over resize throw r@ buffer-address !
	dup r@ buffer-maxlength !
    then
    r> buffer-length ! ;

\ backtrace stuff

create backtrace-rs-buffer buffer% %allot drop
\ copy of the return stack at throw

: init-backtrace ( -- )
    backtrace-rs-buffer init-buffer ;
    
init-backtrace

:noname ( -- )
    DEFERS 'cold
    init-backtrace ;
IS 'cold

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
    backtrace-return-stack
    dup backtrace-rs-buffer adjust-buffer
    backtrace-rs-buffer buffer-address @ swap move ;
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
    cr ." Backtrace:"
    swap u+do
	cr
	i @ dup hex. ( return-addr? )
	print-bt-entry
	cell +loop ;

:noname ( -- )
    backtrace-rs-buffer 2@ over + print-backtrace ;
IS dobacktrace

[ifdef] defer-default
:noname
    r@ >stderr cr ." deferred word " print-bt-entry ." is uninitialized" ;
is defer-default
[then]