\ TOOLS.FS     Toolkit extentions                      2may93jaw

\ Copyright (C) 1995 Free Software Foundation, Inc.

\ This file is part of Gforth.

\ Gforth is free software; you can redistribute it and/or
\ modify it under the terms of the GNU General Public License
\ as published by the Free Software Foundation; either version 2
\ of the License, or (at your option) any later version.

\ This program is distributed in the hope that it will be useful,
\ but WITHOUT ANY WARRANTY; without even the implied warranty of
\ MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
\ GNU General Public License for more details.

\ You should have received a copy of the GNU General Public License
\ along with this program; if not, write to the Free Software
\ Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.

\ May be cross-compiled

hex

\ .S            CORE / CORE EXT                         9may93jaw

variable maxdepth-.s
9 maxdepth-.s !

: .s ( -- )
    ." <" depth 0 .r ." > "
    depth 0 max maxdepth-.s @ min
    dup 0
    ?do
	dup i - pick .
    loop
    drop ;

\ DUMP                       2may93jaw - 9may93jaw    06jul93py
\ looks very nice, I know

Variable /dump

: .4 ( addr -- addr' )
    3 FOR  -1 /dump +!  /dump @ 0<
        IF  ."    "  ELSE  dup c@ 0 <# # # #> type space  THEN
    char+ NEXT ;
: .chars ( addr -- )
    /dump @ bounds
    ?DO I c@ dup 7f bl within
	IF  drop [char] .  THEN  emit
    LOOP ;

: .line ( addr -- )
  dup .4 space .4 ." - " .4 space .4 drop  10 /dump +!  space .chars ;

: dump  ( addr u -- )
    cr base @ >r hex        \ save base on return stack
    0 ?DO  I' I - 10 min /dump !
	dup 8 u.r ." : " dup .line cr  10 +
	10 +LOOP
    drop r> base ! ;

\ ?                                                     17may93jaw

: ? @ . ;

\ words visible in roots                               14may93py

include  termsize.fs

: words ( -- ) \ tools
    cr 0 context @
    BEGIN
	@ dup
    WHILE
	2dup name>string nip 2 + dup >r +
	cols >=
	IF
	    cr nip 0 swap
	THEN
	dup name>string type space r> rot + swap
    REPEAT
    2drop ;

' words alias vlist ( -- ) \ gforth
\g Old (pre-Forth-83) name for @code{WORDS}.

