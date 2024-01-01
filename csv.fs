\ Comma separated value reader

\ Authors: Bernd Paysan
\ Copyright (C) 2022,2023 Free Software Foundation, Inc.

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

',' Value csv-separator
'"' Value csv-quote

true Value quote-state \ if true, a separator is visible

: unquote-start ( addr u -- addr' u' )
    dup IF
	over c@ csv-quote = negate safe/string
    THEN ;
: unquote-end ( addr u -- addr u' )
    dup IF
	2dup + 1- c@ csv-separator = + dup IF
	    2dup + 1- c@ csv-quote = +
	THEN
    THEN ;

: unquote ( addr u -- addr' u' ) \ gforth-experimental
    \G remove surrounding quotes
    unquote-start unquote-end ;

: un-dquote ( addr u -- ) \ gforth-experimental
    \G replace double quotes with single quotes
    { | double-quote[ 2 ] }
    csv-quote double-quote[ c!
    csv-quote double-quote[ 1+ c!
    BEGIN
	2dup double-quote[ 2 search  WHILE
	    2dup 2 safe/string 2>r drop 1+ nip over - type
	    2r>  REPEAT  2drop type ;

: next-field ( addr u -- addr' u' ) \ gforth-experimental
    dup quote-state and IF  over c@ csv-quote <>
	IF  csv-separator scan 1 safe/string  EXIT  THEN
    THEN
    2dup bounds U+DO
	I c@ csv-separator = quote-state and
	IF  2drop I 1+ I' over -  unloop  EXIT  THEN
	I c@ csv-quote = quote-state xor to quote-state
    LOOP  + 0 ;

$Variable $csv-item

: next-csv ( addr u -- addr' u' addr1 u1 ) \ gforth-experimental
    $csv-item $free
    2dup next-field
    quote-state IF
	2tuck drop nip over - unquote ['] un-dquote $csv-item $exec
	$csv-item $@  EXIT
    THEN
    2drop unquote-start
    BEGIN
	['] un-dquote $csv-item $exec
	#lf $csv-item c$+!
	refill 0= IF  source drop 0  $csv-item $@  EXIT  THEN
	source 2dup next-field
	quote-state 0= WHILE
	    2drop
    REPEAT
    2tuck drop nip over - unquote-end
    ['] un-dquote $csv-item $exec
    $csv-item $@ ;

: csv-line ( addr u xt -- ) \ gforth-experimental
    { xt: func | cnt }
    BEGIN  next-csv cnt loadline @ func
    1 +to cnt dup 0= UNTIL  2drop ;

: csv-read-loop ( xt -- ) \ gforth-experimental
    true to quote-state
    >r  BEGIN  refill  WHILE  source r@ csv-line  REPEAT  rdrop ;

: read-csv ( addr u xt -- ) \ gforth-experimental
    \G read CVS file @var{addr u} and execute @var{xt} for every item found.
    \G @var{xt} takes @code{( addr u col line -- )}, i.e. the string, the
    \G current column (starting with 0), and the current line (starting with
    \G 1).
    [n:l csv-read-loop ;] >r
    open-fpath-file throw r> execute-parsing-named-file ;

: .quoted-csv ( addr u -- ) \ gforth-experimental
    \G print a quoted CSV entry
    csv-quote emit bounds ?DO
	I c@ dup csv-quote = IF  dup emit  THEN  emit
    LOOP  csv-quote emit ;
