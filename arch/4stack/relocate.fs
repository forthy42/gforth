\ relocate 4stack binary

\ Copyright (C) 2000,2003,2007,2008 Free Software Foundation, Inc.

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
: x@   dup 4 + l@ swap l@ ;
: x!   tuck l! 4 + l! ;

: scan-header ( fd -- skip )  >r 0
    BEGIN
	8 +
	magic 8 r@ read-file throw 8 = WHILE
	magic 8 s" Gforth3" compare 0= UNTIL
    ELSE  true abort" Magic not found!"  THEN rdrop ;

Create bits $80 c, $40 c, $20 c, $10 c, $08 c, $04 c, $02 c, $01 c,

: bit@ ( n -- flag )
    dup 3 rshift relinfo @ + c@ swap 7 and bits + c@ and 0<> ;

2Variable dovar
2Variable docol

: relocate ( -- )  hex
    image @ $814 + be@ image @ $810 + be@ docol x!
    image @ $80C + be@ image @ $808 + be@ dovar x!
    imagesize @ 1 2* 2* / 0 ?DO
	image @ I 2* 2* + be@
\	dup 8 u.r I bit@ IF '+ ELSE '- THEN emit I 7 and 7 = IF cr THEN 
	dup $80000000 and 0<> I bit@ and IF
	    CASE
		$FFFFFFFF OF
		    0 image @ I 2* 2* + l!  1 ENDOF \ NIL
		$FFFFFFFE OF
		    docol x@
		    image @ I 2* 2* + x!  2 ENDOF \ docol
		$FFFFFFFD OF
		    dovar x@ $10. d+
		    image @ I 2* 2* + x!  2 ENDOF \ docon
		$FFFFFFFC OF
		    dovar x@
		    image @ I 2* 2* + x!  2 ENDOF \ docon
		$FFFFFFF7 OF
		    image @ I 1+ 2* 2* + be@ 5 -
		    dovar x@ nip
		    image @ I 2* 2* + x!  2 ENDOF \ dodoes
		$FFFFFFF6 OF
		    docol x@
		    image @ I 2* 2* + x!  2 ENDOF \ docol
		1 swap
	    ENDCASE
	ELSE
	    image @ I 2* 2* + l! 1
	THEN
    +LOOP
    image @ imagesize @ bounds ?DO
	I x@ swap I x!
	8 +LOOP ;

: read-gforth ( addr u -- )  r/o bin open-file throw
    >r r@ file-size throw drop
    ( r@ scan-header - ) dup allocate throw image !
    image @ swap r@ read-file throw drop
    image @ dup $804 ( 8 ) + be@ dup imagesize ! + relinfo !
    r> close-file throw
    relocate ;

Create 4magic  here $10 dup allot erase
s" 4stack00" 4magic swap move

: write-gforth ( addr u -- )  w/o bin create-file throw >r
    imagesize @ 4magic $C + !
    4magic $10 r@ write-file throw
    image @ imagesize @ r@ write-file throw
    r> close-file throw ;
