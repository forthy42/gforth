\ extended characters (either 8bit or UTF-8, possibly other encodings)
\ and their fixed-size variant

\ Copyright (C) 2005 Free Software Foundation, Inc.

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
\ Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111, USA.

\ We can do some of these (and possibly faster) by just using the
\ utf-8 words with an appropriate setting of max-single-byte, but I
\ like to see how an 8bit setting without UTF-8 stuff looks like.

DEFER XEMIT ( xc -- )
DEFER XKEY ( -- xc )
DEFER XCHAR+ ( xc-addr1 -- xc-addr2 )
DEFER XCHAR- ( xc-addr1 -- xc-addr2 )
DEFER +X/STRING ( xc-addr1 u1 -- xc-addr2 u2 )
DEFER -X/STRING ( xc-addr1 u1 -- xc-addr2 u2 )
DEFER XC@ ( xc-addr -- xc )
DEFER XC!+? ( xc xc-addr1 u1 -- xc-addr2 u2 f ) \ f if operation succeeded
DEFER XC@+ ( xc-addr1 -- xc-addr2 xc )
DEFER XC-SIZE ( xc -- u ) \ size in cs
DEFER -TRAILING-GARBAGE ( addr u1 -- addr u2 ) \ remove trailing incomplete xc

\ derived words, faster implementations are probably possible

: X@+/string ( xc-addr1 u1 -- xc-addr2 u2 xc )
    \ !! check for errors?
    over >r +x/string
    r> xc@ ;

\ fixed-size versions of these words

: char- ( c-addr1 -- c-addr2 )
    [ 1 chars ] literal - ;

: 1/string ( c-addr1 u1 -- c-addr2 u2 )
    1 /string ;

: -1/string ( c-addr1 u1 -- c-addr2 u2 )
    -1 /string ;

: c!+? ( c c-addr1 u1 -- c-addr2 u2 f )
    1 chars u< if \ or use < ?
	>r dup >r c!
	1 r> r> /string true
    else
	rot drop false
    then ;

: c-size ( c -- 1 )
    drop 1 ;

: set-encoding-fixed-width ( -- )
    ['] emit is xemit
    ['] key is xkey
    ['] char+ is xchar+
    ['] char- is xchar-
    ['] 1/string is +x/string
    ['] -1/string is -x/string
    ['] c@ is xc@
    ['] c!+? is xc!+?
    ['] count is xc@+
    ['] c-size is xc-size
    ['] noop is -trailing-garbage
;
