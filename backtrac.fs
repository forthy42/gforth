\ backtrace handling

\ Authors: Anton Ertl, Bernd Paysan, Gerald Wodni
\ Copyright (C) 1999,2000,2003,2004,2006,2007,2012,2013,2016,2017,2018,2019 Free Software Foundation, Inc.

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

0 Value extra-backtrace#
\G add further cells to backtrace stack for non-debugging engine exceptions

: backtrace-return-stack ( -- addr u )
    \ addr is the address of top element of return stack (the stack
    \ grows downwards), u is the number of aus that interest us on the
    \ stack.
    rp@ in-return-stack?
    if
	rp@ [ 2 cells ]L +
    else \ throw by signal handler with insufficient information
	handler @ cell- \ beyond that we know nothing
	extra-backtrace# ?dup-IF  cells -
	    rp0 @ [ forthstart 7 cells + ]L @ - $FFF + -$1000 and umax
	    BEGIN  dup @ 0=  WHILE  cell+  REPEAT
	THEN
    then
    backtrace-rp0 @ cell- over - 0 max ;

:noname ( -- )
    backtrace-return-stack stored-backtrace $! first-throw off ;
IS store-backtrace

: >bt-entry ( return-stack-item -- nt )
    cell- dup in-dictionary? over dup aligned = and
    if
	@ dup threaded>name dup if
	    nip EXIT
	else
	    drop dup look if
		nip EXIT
	    else
		drop body> look \ !! check for "call" in cell before?
		if
		    EXIT
		else
		    drop
		then
	    then
	then
    else
	drop
    then  0 ;

defer .backtrace-pos ( addr -- )
' drop is .backtrace-pos

: print-bt-entry ( return-stack-item -- )
    >bt-entry ?dup-IF  .name  THEN ;

: print-backtrace ( addr1 addr2 -- )
    \G print a backtrace for the return stack addr1..addr2
    2dup u< IF  cr ." Backtrace:"  THEN
    0 swap rot u+do
	cr i @ dup .backtrace-pos over 2 .r space
	dup hex. dup print-bt-entry
	catch-frame = IF  ."  [catch frame]" 1+  7 cells  ELSE  1+ cell  THEN
    +loop
    drop ;

: .bt ( -- )
    \G backtrace for interactive use
    backtrace-rp0 @ #10 cells + dup 3 cells - @ cell- print-backtrace ;
comp: drop ]] store-backtrace dobacktrace [[ ;

:noname ( -- )
    stored-backtrace $@ over + print-backtrace  nothrow ;
IS dobacktrace

[ifdef] defer-default
:noname
    r@ >stderr cr ." deferred word " print-bt-entry ." is uninitialized" ;
is defer-default
[then]

\ print backtrace location

: .sourceview-width ( view -- u )
    \ prints sourceview, returns width of printed string
    ['] .sourceview $tmp 2dup type x-width ;
    
: .backtrace-view ( addr -- )
    addr>view dup IF
	.sourceview-width THEN
    bt-pos-width swap - 1 max spaces ;
' .backtrace-view is .backtrace-pos
