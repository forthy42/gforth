\ SwiftForth-like locate etc.

\ Copyright (C) 2016 Free Software Foundation, Inc.

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

variable located-xpos \ contains xpos of LOCATEd/EDITed word
variable located-len  \ contains the length of the word
variable bn-xpos      \ first contains located-xpos, but is updated by B and N
variable located-top  \ first line to display with l
variable located-bottom \ last line to display with l
2variable located-slurped \ the contents of the file in located-xpos, or 0 0

: set-located-xpos ( xpos len -- )
    over xpos>file# located-xpos @ xpos>file# <> if
	located-slurped 2@ drop ?dup-if
	    free throw then
	0 0 located-slurped 2! then
    located-len ! dup located-xpos ! dup bn-xpos !
    xpos>line
    dup before-locate - 0 max located-top !
    after-locate + located-bottom ! ;

:noname {: uline c-addr1 u1 -- uline c-addr1 u1 :}
    c-addr1 u1 str>loadfilename# uline
    input-lexeme 2@ >r source drop - encode-pos1 r> set-located-xpos
    uline c-addr1 u1
; is set-current-xpos

: set-bn-xpos ( -- )
    bn-xpos @ xpos>file# located-top @ 0 encode-pos1 bn-xpos ! ;

: slurp-located ( -- )
    located-slurped 2@ drop 0= if
	located-xpos @ xpos>file# loadfilename#>str slurp-file
	located-slurped 2!
    then ;

: locate-line {: c-addr1 u1 lineno -- c-addr2 u2 lineno+1 c-addr1 u3 :}
     u1 0 u+do
	c-addr1 u1 i /string s\" \r\l" string-prefix? if
	    c-addr1 u1 i 2 + /string lineno 1+ c-addr1 i unloop exit then
	c-addr1 i + c@ dup #lf = swap #cr = or if
	    c-addr1 u1 i 1 + /string lineno 1+ c-addr1 i unloop exit then
    loop
    c-addr1 u1 + 0 lineno 1+ c-addr1 u1 ;

: locate-next-line ( c-addr1 u1 lineno -- c-addr2 u2 lineno+1 )
    locate-line 2drop ;

: type-prefix ( c-addr1 u1 u -- c-addr2 u2 )
    \ type the u-len prefix of c-addr1 u1, c-addr2 u2 is the rest
    >r 2dup r> umin tuck type /string ;

: locate-type ( c-addr u lineno -- )
    cr located-xpos @ xpos>line = if
	warn-color attr! located-xpos @ xpos>char type-prefix
	err-color  attr! located-len @            type-prefix
	warn-color attr! type
	default-color attr! exit
    then
    type ;

: locate-print-line ( c-addr1 u1 lineno -- c-addr2 u2 lineno+1 )
    dup >r locate-line r> locate-type ;

: l1 ( -- )
    slurp-located
    located-slurped 2@ 1 case ( c-addr u lineno1 )
	over 0= ?of endof
	dup located-bottom @ >= ?of endof
	dup located-top @ >= ?of locate-print-line contof
	locate-next-line
    next-case
    2drop drop ;

: l ( -- )
    \g Display line of source after compiler error or locate
    cr located-xpos @ xpos>file# loadfilename#>str type  ': emit
    located-top @ dec.
    l1 ;

: name-set-located-xpos ( nt -- )
    dup name>view @ swap name>string nip set-located-xpos ;

: locate-name ( nt -- )
     name-set-located-xpos l ;

: locate ( "name" -- )
    (') locate-name ;

: n ( -- )
    \g Display next lines after locate or error
    located-bottom @ dup located-top ! form drop 2/ + located-bottom !
    set-bn-xpos l1 ;

: b ( -- )
    \g Display previous lines after locate.
    located-top @ dup located-bottom ! form drop 2/ - 0 max located-top !
    set-bn-xpos l ;

: g ( -- )
    \g Enter the editor at the place of the latest error, @code{locate},
    \g @code{n} or @code{b}.
    bn-xpos @ ['] editor-cmd >string-execute 2dup system drop free throw ;

: edit ( "name" -- )
    \g Enter the editor at the place of "name"
    (') name-set-located-xpos g ;
