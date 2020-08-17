\ status line, inspired by seedForth

\ Authors: Bernd Paysan
\ Copyright (C) 2020 Free Software Foundation, Inc.

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

blue >bg white >fg or bold or Value status-attr
: redraw-status ( addr u -- )
    save-cursor-position
    0 rows 1 - at-xy
    status-attr attr!
    type
    default-color attr!
    restore-cursor-position ;
: .unstatus-line ( -- )
    0 erase-display ;
: replace-char ( c1 c2 addr u -- )
    bounds U+DO
	over I c@ = IF  dup I c!  THEN
    LOOP  2drop ;

0 Value wide?

: .base ( -- )
    base @ #10 <> IF  wide? IF  ." base="  ELSE  ." b="  THEN
	base @ 0 ['] .r #10 base-execute cr  THEN ;
: .stacks ( -- )
    f.s-precision >r  wide? IF  #14  ELSE  #10  THEN  to f.s-precision
    ['] ... #10 base-execute cr
    r> to f.s-precision ;
: .order ( -- )
    wide? IF  ."  order: " ELSE  ." o:" THEN  order ;

10 stack: status-xts
\G status line prints a stack of status words
' .base ' .stacks ' .order 3 status-xts set-stack

: .status-line ( -- ) { | w^ status$ }
    cols #100 > to wide?
    [: status-xts $@ bounds DO  I perform  cell +LOOP ;] status$ $exec
    #lf '|' status$ $@ replace-char
    cols status$ $@ x-width - dup 0> IF
	['] spaces $tmp
	status$ dup $@ '|' -scan nip $ins
    ELSE  0< IF
	    0 status$ $@ bounds U+DO
		I xc@+ swap >r
		dup #tab = IF  drop 1+ dfaligned  ELSE  xc-width +  THEN
		dup cols u> IF  rdrop I status$ $@ drop - status$ $!len
		    leave  THEN
	    r> I - +LOOP  drop
	THEN
    THEN
    cr cr 0 -2 at-deltaxy
    status$ $@ redraw-status
    status$ $free ;

: +status ['] .status-line is .status ['] .unstatus-line is .unstatus ;
: -status ['] noop is .status ['] noop is .unstatus ;

+status
