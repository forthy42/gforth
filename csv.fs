\ Comma separated value reader

\ Authors: Bernd Paysan
\ Copyright (C) 2022,2023,2024 Free Software Foundation, Inc.

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

',' Value csv-separator ( -- c ) \ gforth-experimental
\G CSV field separator (default is `,', hence the name
\G "comma-separated"); this is a value and can be
\G changed with @code{to csv-separator}.

'"' Value csv-quote ( -- c ) \ gforth-experimental
\G CSV quote character (default is @code{"}); this is a value and can be
\G changed with @code{to csv-quote}.

true Value quote-state \ if true, a separator is visible

variable csv-line-number

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

: unquote ( c-addr u -- c-addr' u' ) \ gforth-internal
    \G @i{C-addr' u'} is the substring of @i{c-addr u} without the
    \G leading and trailing @code{csv-quote}.
    unquote-start unquote-end ;

: un-dquote ( c-addr u -- ) \ gforth-internal
    \G Print @code{c-addr u}, but for each consecutive pair of
    \G @code{csv-quote}s, print only one @code{csv-quote}
    { | double-quote[ 2 ] }
    csv-quote double-quote[ c!
    csv-quote double-quote[ 1+ c!
    BEGIN
	2dup double-quote[ 2 search  WHILE
	    2dup 2 safe/string 2>r drop 1+ nip over - type
	    2r>  REPEAT  2drop type ;

: next-field ( c-addr u -- c-addr' u' ) \ gforth-internal
    \g Given a sequence of @code{csv-separator}ed fields @i{c-addr u},
    \g @i{C-addr' u'} is the rest of the string starting with the
    \g second field; if there is only one field in @i{c-addr u},
    \g @i{u'}=0.
    dup quote-state and IF  over c@ csv-quote <>
	IF  csv-separator scan 1 safe/string  EXIT  THEN
    THEN
    2dup bounds U+DO
	I c@ csv-separator = quote-state and
	IF  2drop I 1+ I' over -  unloop  EXIT  THEN
	I c@ csv-quote = quote-state xor to quote-state
    LOOP  + 0 ;

$Variable $csv-item

: next-csv ( addr u -- addr' u' addr1 u1 ) \ gforth-internal
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

: csv-line ( addr u xt -- ) \ gforth-internal
    { xt: func | cnt }
    BEGIN  next-csv cnt csv-line-number @ func
    1 +to cnt dup 0= UNTIL  2drop ;

: csv-read-loop ( xt -- ) \ gforth-internal
    true to quote-state
    >r  BEGIN
        refill  WHILE
            source r@ csv-line
            1 csv-line-number +!
    REPEAT
    rdrop ;

: read-csv ( c-addr u xt -- ) \ gforth-experimental
    \G Read the CSV file with the name given by @i{c-addr u} and
    \G execute @i{xt} for every field found.@* @i{Xt} @code{(
    \G @i{c-addr2 u2 field line} -- )} is called once for each field;
    \G @i{c-addr2 u2} is the decoded field content, @i{field} is the
    \G field number (starting with 0), and @i{line} is the line number
    \G (starting with 1).
    1 csv-line-number !
    [n:l csv-read-loop ;] >r
    open-fpath-file throw r> execute-parsing-named-file ;

: .quoted-csv ( c-addr u -- ) \ gforth-experimental dot-quoted-csv
    \G print a field in CSV format, i.e., with enough quotes that
    \G @code{read-csv} will produce @i{c-addr u} when encountering the
    \G output of @code{.quoted-csv}.
    csv-quote emit bounds ?DO
	I c@ dup csv-quote = IF  dup emit  THEN  emit
    LOOP  csv-quote emit ;
