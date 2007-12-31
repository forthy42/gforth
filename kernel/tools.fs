\ TOOLS.FS     Toolkit extentions                      2may93jaw

\ Copyright (C) 1995,1998,1999,2001,2003,2006,2007 Free Software Foundation, Inc.

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

\ May be cross-compiled

require ./io.fs		\ type ...
require ./nio.fs	\ . <# ...
require ./int.fs	\ wordlist-id ..

hex

\ .S            CORE / CORE EXT                         9may93jaw

variable maxdepth-.s ( -- addr ) \ gforth maxdepth-dot-s
\G A variable containing 9 by default.  @code{.s} and @code{f.s}
\G display at most that many stack items.
9 maxdepth-.s !

: .s ( -- ) \ tools dot-s
\G Display the number of items on the data stack, followed by a list
\G of the items (but not more than specified by @code{maxdepth-.s};
\G TOS is the right-most item.
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
        IF  ."    "  ELSE  dup c@ 0 <<# # # #> type #>> space  THEN
    char+ NEXT ;
: .chars ( addr -- )
    /dump @ bounds
    ?DO I c@ dup 7f bl within
	IF  drop [char] .  THEN  emit
    LOOP ;

: .line ( addr -- )
  dup .4 space .4 ." - " .4 space .4 drop  10 /dump +!  space .chars ;

: dump  ( addr u -- ) \ tools dump
    \G Display @var{u} lines of memory starting at address @var{addr}. Each line
    \G displays the contents of 16 bytes. When Gforth is running under
    \G an operating system you may get @file{Invalid memory address} errors
    \G if you attempt to access arbitrary locations.
    cr base @ >r hex        \ save base on return stack
    0 ?DO  I' I - 10 min /dump !
	dup 8 u.r ." : " dup .line cr  10 +
	10 +LOOP
    drop r> base ! ;

\ ?                                                     17may93jaw

: ? ( a-addr -- ) \ tools question
    \G Display the contents of address @var{a-addr} in the current number base.
    @ . ;

\ words visible in roots                               14may93py

include  ./../termsize.fs

: wordlist-words ( wid -- ) \ gforth
    \G Display the contents of the wordlist wid.
    [ has? ec 0= [IF] ] wordlist-id [ [THEN] ]
    0 swap cr
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

: words
    \G ** this will not get annotated. See other defn in search.fs .. **
    \G It does not work to use "wordset-" prefix since this file is glossed
    \G by cross.fs which doesn't have the same functionalty as makedoc.fs
    [ has? ec 0= [IF] ] context @ [ [ELSE] ] forth-wordlist [ [THEN] ]
    wordlist-words ;

' words alias vlist ( -- ) \ gforth
\g Old (pre-Forth-83) name for @code{WORDS}.
