\ Number IO

\ Copyright (C) 1995,1996,1997,1998 Free Software Foundation, Inc.

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

: pad    ( -- addr ) \ core-ext
    here word-pno-size + aligned ;

\ hold <# #> sign # #s                                 25jan92py

: hold    ( char -- ) \ core
    pad cell - -1 chars over +! @ c! ;

: <# ( -- ) \ core	less-number-sign
    pad cell - dup ! ;

: #>      ( xd -- addr u ) \ core	number-sign-greater
    2drop pad cell - dup @ tuck - ;

: sign    ( n -- ) \ core
    0< IF  [char] - hold  THEN ;

: #       ( ud1 -- ud2 ) \ core		number-sign
    base @ 2 max ud/mod rot 9 over <
    IF
	[ char A char 9 - 1- ] Literal +
    THEN
    [char] 0 + hold ;

: #s      ( +d -- 0 0 ) \ core	number-sign-s
    BEGIN
	# 2dup or 0=
    UNTIL ;

\ print numbers                                        07jun92py

: d.r ( d n -- ) \ double	d-dot-r
    >r tuck  dabs  <# #s  rot sign #>
    r> over - spaces  type ;

: ud.r ( ud n -- ) \ gforth	u-d-dot-r
    >r <# #s #> r> over - spaces type ;

: .r ( n1 n2 -- ) \ core-ext	dot-r
    >r s>d r> d.r ;
: u.r ( u n -- )  \ core-ext	u-dot-r
    0 swap ud.r ;

: d. ( d -- ) \ double	d-dot
    0 d.r space ;
: ud. ( ud -- ) \ gforth	u-d-dot
    0 ud.r space ;

: . ( n -- ) \ core	dot
    s>d d. ;
: u. ( u -- ) \ core	u-dot
    0 ud. ;

