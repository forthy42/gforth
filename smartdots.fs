\ smart .s                                             09mar2012py

\ Authors: Bernd Paysan, Anton Ertl, Gerald Wodni
\ Copyright (C) 2012,2018,2019,2020,2021,2022,2023,2024 Free Software Foundation, Inc.

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

\ idea: Gerald Wodni

User smart.s-skip

: addr? ( addr -- flag )
    ['] c@ catch-nobt  IF  drop  false  ELSE  drop  true  THEN ;
: .var? ( addr -- flag )
    TRY  body> >code-address dovar: <> throw  IFERROR  2drop false
	ELSE  true  THEN   ENDTRY ;

: string? ( addr u -- flag )
    \ does it look like a string that we want to print with smart. ?
    TRY  dup #80 #1 within throw  bounds ?DO
	    I xc@+ dup invalid-char = swap bl < or IF  -1 throw  THEN
	I - +LOOP
	IFERROR  2drop drop false  ELSE  true  THEN  ENDTRY ;
Create cs? ( addr -- flag )
defstart , live-orig , dead-orig , dest , do-dest , scopestart ,
does> 6 cell array>mem MEM+DO
      dup I @ = if  drop true unloop  exit  then
  LOOP  drop false ;

: .addr. ( addr -- ) dup >r
    [:  dup xt? if
	    dup name>string 2dup string? if
		third >namehm @ >hm>int @ ['] noop <> if '`' emit then
		." `" type space drop exit
	    else
		2drop
	    then
	then
	dup which-section? ?dup-if
	    @ >body over [ 1 maxaligned negate ]L and U-DO
		I body> xt? if
		    I body> name>string 2dup string? if
			'<' emit type I - ?dup-if
			    ." +$" 0 ['] u.r $10 base-execute  then
			'>' emit space unloop  EXIT
		    else  2drop  then
		then
	    [ 1 maxaligned ]L -LOOP
	then
	h. ;] catch-nobt IF  drop r> h.  ELSE  rdrop  THEN ;

: .var. ( addr -- )
    dup body> >name dup IF  id. drop  ELSE  drop h.  THEN ;

: .cs. ( x1 addr -- )
    '<' emit
    dup defstart = IF  drop ['] colon-sys >body  THEN
    body> name>string type '>' emit space  drop ;

: smart. ( n -- )
    dup addr? IF
	dup .var? IF
	    .var.
	ELSE
	    .addr.  THEN
    ELSE
	'#' emit dec.  THEN ;

debug: .string.( ( -- ) \ gforth-internal dot-string-dot-paren
\G this debug switch adds a printout of addr len in parents
\G to a smart string printout

: .string. ( addr u -- )
    .string.( ." ( " over smart. dup dec. ." ) " )
    \ print address and length of string?
    '"' emit type '"' emit space ;

\ a .s.<matcher> either consumes its data and returns true
\ or it doesn't, and returns false
\ The last item in the stack must consume and return true

: .s.skip ( n depth -- t / n f ) \ gforth-experimental
    drop smart.s-skip @ dup 1- 0 max smart.s-skip !
    0<> dup IF  nip  THEN ;
: .s.cs ( n depth -- t / n f ) \ gforth-experimental
    dup cs-item-size 1- < IF  drop false EXIT  THEN
    cs-item-size 2 - - pick dup cs?
    IF  .cs.  cs-item-size 1- smart.s-skip !  true  EXIT  THEN
    drop false ;
: .s.string ( addr depth -- t / addr f ) \ gforth-experimental
    dup 2 < IF  drop false  EXIT  THEN
    pick  2dup string?
    IF  .string. 1 smart.s-skip ! true EXIT THEN
    drop false ;
: .s.smart ( n depth -- t ) \ gforth-experimental
    drop smart. true ;

\ This is actually a sequence, so the top of stack is executed last
10 stack: smart<> \ gforth-experimental

' .s.skip ' .s.cs ' .s.string ' .s.smart 4 smart<> set-stack

: smart.s. ( total n -- total ) \ gforth-experimental
    over r> i swap >r - { dpth } \ i is the loop index of the calling .s
    smart<> $@ cell MEM+DO
	dpth I perform ?LEAVE
    LOOP ;

: wrap-xt ( ... xt1 xt2 xt3 -- ... ) \ gforth
    {: xt1 xt2 xt: xt3 :}
    \G Set deferred word xt2 to xt1 and execute xt3.
    \G Restore afterwards.
    xt2 defer@ {: old-xt2 :} try
	xt1 xt2 defer! xt3 0
    restore
	old-xt2 xt2 defer!
    endtry
    throw ;

: ... ( x1 .. xn -- x1 .. xn ) \ gforth
    \G smart version of @code{.s}
    smart.s-skip off
    ['] smart.s. ['] .s. ['] .s
    ['] wrap-xt catch-nobt drop
    fdepth IF  cr ." F:" f.s  THEN ;

' ... IS printdebugdata
