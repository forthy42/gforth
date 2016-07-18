\ assertions

\ Copyright (C) 1995,1996,1997,1999,2002,2003,2007,2010,2012,2013,2014,2015 Free Software Foundation, Inc.

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

require source.fs

variable assert-level ( -- a-addr ) \ gforth
\G All assertions above this level are turned off.
1 assert-level !

: (end-assert) ( flag xpos -- ) \ gforth-internal
    swap if
	drop
    else
	.sourcepos1 ." : failed assertion"
	true abort" assertion failed" \ !! or use a new throw code?
    then ;

: assert) ( -- )
    compile-sourcepos POSTPONE (end-assert) ;

6 Constant assert-canary

: assertn ( n -- ) \ gforth assert-n
    \ this is internal (it is not immediate)
    assert-level @ >
    if
	POSTPONE (
    else
	['] assert) assert-canary
    then ;

: )  ( -- ) \ gforth	close-paren
    \G End an assertion. Generic end, can be used for other similar purposes
    assert-canary <> abort" unmatched assertion"
    execute ; immediate

: assert0( ( -- ) \ gforth assert-zero
    \G Important assertions that should always be turned on.
    0 assertn ; immediate compile-only
: assert1( ( -- ) \ gforth assert-one
    \G Normal assertions; turned on by default.
    1 assertn ; immediate compile-only
: assert2( ( -- ) \ gforth assert-two
    \G Debugging assertions.
    2 assertn ; immediate compile-only
: assert3( ( -- ) \ gforth assert-three
    \G Slow assertions that you may not want to turn on in normal debugging;
    \G you would turn them on mainly for thorough checking.
    3 assertn ; immediate compile-only
: assert( ( -- ) \ gforth
    \G Equivalent to @code{assert1(}
    POSTPONE assert1( ; immediate compile-only

\ conditionally executed debug code, not necessarily assertions

: debug-does>  DOES>  @
    IF ['] noop assert-canary  ELSE  postpone (  THEN ;
: debug: ( -- )
    Create false , debug-does>
comp:  >body
    ]] Literal @ IF [[ [: ]] THEN [[ ;] assert-canary ;
: )else( ( --)
    ]] ) ( [[ ; \ )
comp: drop 2>r ]] ELSE [[ 2r> ;
: else( ['] noop assert-canary ; immediate

: +db ( "word" -- ) ' >body on ;
: -db ( "word" -- ) ' >body off ;
: ~db ( "word" -- ) ' >body dup @ 0= swap ! ;

Variable debug-eval

: +-? ( addr u -- flag )
    0<> swap c@ ',' - abs 1 = and ; \ ',' is in the middle between '+' and '-'

: set-debug ( addr u -- )
    debug-eval $!
    s" db " debug-eval 1 $ins
    s" (" debug-eval $+!
    debug-eval $@ evaluate ;

: +debug ( -- )
    BEGIN  argc @ 1 > WHILE
	    1 arg +-?  WHILE
		1 arg set-debug
		shift-args
	REPEAT  THEN ;

\ timing for profiling

debug: profile(
+db profile(

2Variable timer-tick
2Variable last-tick

: 2+! ( d addr -- )  >r r@ 2@ d+ r> 2! ;
: +t ( addr -- )
    ntime 2dup last-tick dup 2@ 2>r 2! 2r> d- rot 2+! ;

Variable timer-list
: timer: Create 0. , , here timer-list !@ ,
  DOES> profile( +t )else( drop ) ;
: map-timer { xt -- }
    timer-list BEGIN  @ dup  WHILE dup >r
	    cell- cell- xt execute r> REPEAT drop ;

: init-timer ( -- )
    ntime last-tick 2! [: 0. rot 2! ;] map-timer ;

: .times ( -- )
    [: dup body> >name name>string 1 /string
	tuck type 8 swap - 0 max spaces ." : "
	2@ d>f 1n f* f. cr ;] map-timer ;

: !time ( -- ) ntime timer-tick 2! ;
: @time ( -- delta-f ) ntime timer-tick 2@ d- d>f 1n f* ;
: (.time) ( delta-f -- )
    fdup 1e f>= IF  13 9 0 f.rdp ." s "   EXIT  THEN  1000 fm*
    fdup 1e f>= IF  10 6 0 f.rdp ." ms "  EXIT  THEN  1000 fm*
    fdup 1e f>= IF   7 3 0 f.rdp ." µs "  EXIT  THEN  1000 fm*
    f>s 3 .r ." ns " ;
: .time ( -- )
    @time (.time) ;
