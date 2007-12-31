\ relocate 4stack binary

\ Copyright (C) 2000,2007 Free Software Foundation, Inc.

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

Create magic 8 allot
Variable image
Variable relinfo
Variable imagesize

: be@  0 swap 4 bounds DO  8 lshift I c@ +  LOOP ;

: scan-header ( fd -- skip )  >r 0
    BEGIN
	8 +
	magic 8 r@ read-file throw 8 = WHILE
	magic 8 s" Gforth2" compare 0= UNTIL
    ELSE  true abort" Magic not found!"  THEN rdrop ;

Create bits $80 c, $40 c, $20 c, $10 c, $08 c, $04 c, $02 c, $01 c,

: bit@ ( n -- flag )
    dup 3 rshift relinfo @ + c@ swap 7 and bits + c@ and 0<> ;

2Variable dovar
2Variable docol
2Variable dojmp
2Variable docon

: relocate ( -- )  hex
    image @ $80C + be@ image @ $808 + be@ docon 2!
    image @ $814 + be@ image @ $810 + be@ dovar 2!
    image @ $81C + be@ image @ $818 + be@ dojmp 2!
    image @ $824 + be@ image @ $820 + be@ docol 2!
    imagesize @ 1 cells / 0 ?DO
	image @ I cells + be@
\	dup 8 u.r I bit@ IF '+ ELSE '- THEN emit I 7 and 7 = IF cr THEN 
	dup 0< I bit@ and IF
	    CASE
		-1 OF
		    0 image @ I cells + !  1 ENDOF \ NIL
		-2 OF
		    docol 2@
		    image @ I cells + 2!  2 ENDOF \ docol
		-3 OF
		    docon 2@
		    image @ I cells + 2!  2 ENDOF \ docon
		-4 OF
		    dovar 2@
		    image @ I cells + 2!  2 ENDOF \ docon
		-8 OF
		    image @ I 1+ cells + be@ 5 -
		    docol 2@ nip
		    image @ I cells + 2!  2 ENDOF \ dodoes
		-9 OF
		    dojmp 2@
		    image @ I cells + 2!  2 ENDOF \ docol
		1 swap
	    ENDCASE
	ELSE
	    image @ I cells + ! 1
	THEN
    +LOOP
    image @ imagesize @ bounds ?DO
	I 2@ swap I 2!
	2 cells +LOOP ;

: read-gforth ( addr u -- )  r/o bin open-file throw
    >r r@ file-size throw drop
    r@ scan-header - dup allocate throw image !
    image @ swap r@ read-file throw drop
    image @ dup $804 ( 8 ) + be@ dup imagesize ! + relinfo !
    r> close-file throw
    relocate ;

Create 4magic  $10 allot
s" 4stack00" 4magic swap move

: write-gforth ( addr u -- )  w/o bin open-file throw >r
    imagesize @ 4magic $C + !
    4magic $10 r@ write-file throw
    image @ imagesize @ r@ write-file throw
    r> close-file throw ;
