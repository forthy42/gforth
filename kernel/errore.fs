\ ERRORE.FS English error strings                      9may93jaw

\ Authors: Bernd Paysan, Anton Ertl, Gerald Wodni, Jens Wilke
\ Copyright (C) 1995,1996,1997,1998,1999,2000,2003,2006,2007,2012,2014,2015,2016,2017,2018,2019 Free Software Foundation, Inc.

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


\ The errors are defined by a linked list, for easy adding
\ and deleting. Speed is not neccassary at this point.

require ./io.fs
require ./nio.fs

AVariable ErrLink              \ Linked list entry point
NIL ErrLink !
AVariable ErrRanges            \ List of entry ranges
NIL ErrRanges !

decimal

\ error numbers between -256 and -511 represent signals
\ signals are handled with strsignal
\ but some signals produce throw-codes > -256, e.g., -28

\ error numbers between -512 and -2047 are for OS errors and are
\ handled with strerror

Create .warning
DOES> ( addr -- ) @ count type ;
\ closure technique, implemented by hand:
\ : c(warning") [{: s :}l s count type ;] true swap ?warning ;
: c(warning") ( c-addr -- )
    [ cell 4 = [IF] ] false >l [ [THEN] ]
    >l dodoes: >l [ ' .warning cell - @ ] ALiteral >l
    true laddr# [ cell , ] ?warning lp+!# [ 3 cell 4 = - cells , ] ;

has? OS [IF]
: >exec  >r ;
: >stderr ( -- )
    r> op-vector @ >r debug-vector @ op-vector !
    >exec  r> op-vector ! ;

: do-debug ( xt -- )
    op-vector @ >r debug-vector @ op-vector !
    catch  r> op-vector !  throw ;
[THEN]

: errlink>string ( n -- addr u )
    ErrLink
    BEGIN @ dup
    WHILE
	2dup cell+ @ =
	IF
	    2 cells + count rot drop EXIT THEN
    REPEAT
    drop
    base @ >r decimal
    s>d tuck dabs <# #s rot sign s" error #" holds #> r> base ! ;

: error$ ( n -- addr u )
    ErrRanges
    BEGIN  @ dup  WHILE
	    2dup cell+ 2@ within
	    IF  >r r@ cell+ @ 1- - negate
		r> 3 cells + perform  EXIT  THEN
    REPEAT
    drop errlink>string ;

has? OS [IF]
    ErrRanges @ here ErrRanges A! A, -255 , -511 , ' strsignal A,
    ErrRanges @ here ErrRanges A! A, -511 , -2047 , ' strerror A,

    $6600 Value default-color ( -- x ) \ gforth
    \G use system-default color
    $E600 Value error-color   ( -- x ) \ gforth
    \G error color: red
    $B600 Value warning-color ( -- x ) \ gforth
    \G color for warnings: blue/yellow on black terminals
    $D600 Value info-color    ( -- x ) \ gforth
    \G color for info: green/cyan on black terminals
    $D600 Value success-color ( -- x ) \ gforth
    \G color for success: green
    $6601 Value input-color   ( -- x ) \ gforth
    \G color for user-input: black/white (both bold)
    2 Value error-hl-ul ( -- ) \ gforth
    \G color mod for error highlight underline
    8 Value error-hl-inv ( -- ) \ gforth
    \G color mod for error highlight inverse
    true Value white?
    \G reset to current colors
    : light-mode ( -- ) \ gforth
	\G color theme for white background
	true to white?
	$6600 to default-color
	$E600 to error-color
	$B600 to warning-color
	$D600 to info-color
	$D600 to success-color
	$6601 to input-color
	2 to error-hl-ul
	8 to error-hl-inv ;
    : dark-mode ( -- ) \ gforth
	\G color theme for black background
	false to white?
	$6600 to default-color
	$E601 to error-color
	$C601 to warning-color
	$9601 to info-color
	$D601 to success-color
	$6601 to input-color
	2 to error-hl-ul
	8 to error-hl-inv ;
    : default-mode ( -- ) \ gforth
	\G use the default color
	false to white?
	$6600 to default-color \ reset colors even in no-color mode
	false to error-color
	false to warning-color
	false to info-color
	false to success-color
	false to input-color
	false to error-hl-ul
	false to error-hl-inv ;
    : magenta-input ( -- ) \ gforth
	\G make input color easily recognizable (useful in presentations)
        $A601 white? + to input-color ;
[THEN]

: .error ( n -- )
[ has? OS [IF] ]
    >stderr
[ [THEN] ]
    error$ type ;
