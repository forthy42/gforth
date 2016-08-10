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

: print-bt-entry ( return-stack-item -- )
    >bt-entry ?dup-IF  .name  THEN ;

defer .backtrace-pos ( addr -- )
' drop is .backtrace-pos

: print-backtrace ( addr1 addr2 -- )
    \G print a backtrace for the return stack addr1..addr2
    2dup u< IF  cr ." Backtrace:"  THEN
    0 swap rot u+do
	cr i @ dup .backtrace-pos over 2 .r space dup hex. print-bt-entry 1+
    cell +loop
    drop ;

: bt ( -- )
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

\ locate position in backtrace

[ifdef] .backtrace-pos
    40 value bt-pos-width
    0 Value locs-start
    Variable locs[]
    : xt-location1 ( addr -- addr )
	dup locs-start - cell/ >r
	current-sourcepos1 dup r> 1+ locs[] $[] cell- 2! ;
    : record-locs ( -- )
	\G record locations to annotate backtraces with source locations
	here to locs-start  locs[] $free
	['] xt-location1 is xt-location ;
    : addr>pos1 ( addr -- pos1 / 0 )
	dup cell- locs-start here within locs-start and ?dup-IF
	    over cell- swap - cell/ locs[] $[] @
	    ?dup-IF  nip  EXIT  THEN
	THEN  drop 0 ;

    : .backtrace-pos1 ( addr -- )
	addr>pos1 dup IF
	    ['] .sourcepos1 $tmp 2dup type x-width  THEN
	bt-pos-width swap - 1 max spaces ;
    ' .backtrace-pos1 is .backtrace-pos
[then]
