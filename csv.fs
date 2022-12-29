\ Comma separated value reader

\ Authors: Bernd Paysan
\ Copyright (C) 2022 Free Software Foundation, Inc.

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

: unquote ( addr u -- addr' u' ) \ gforth-experimental
    \G remove surrounding quotes
    dup 0= ?EXIT
    2dup + 1- c@ ',' = + dup 0= ?EXIT
    over c@ '"' = negate safe/string ?dup-IF  2dup + 1- c@ '"' = +  THEN ;

: un-dquote ( addr u -- ) \ gforth-experimental
    \G replace double quotes with single quotes
    BEGIN
	2dup "\"\"" search  WHILE
	    2dup 2 safe/string 2>r drop 1+ nip over - type
	    2r>  REPEAT  2drop type ;

: next-field ( addr u -- addr' u' ) \ gforth-experimental
    2dup "\"" string-prefix? 0= IF  ',' scan 1 safe/string  EXIT  THEN
    2dup "\"\"," string-prefix? IF  3 safe/string  EXIT  THEN
    BEGIN  1 safe/string "\","  search  WHILE
	over 1- c@ '"' <>  UNTIL  2 safe/string  THEN ;

: next-csv ( addr u -- addr' u' addr1 u1 ) \ gforth-experimental
    2dup next-field 2tuck drop nip over - unquote ['] un-dquote $tmp ;

: csv-line ( addr u xt -- ) \ gforth-experimental
    { xt: func | cnt }
    BEGIN  next-csv cnt func
    1 +to cnt dup 0= UNTIL  2drop ;

: csv-read-loop ( xt1 xt2 -- ) \ gforth-experimental
    >r >r
    BEGIN  refill  WHILE
	    source r> csv-line
	    r@ >r
    REPEAT  2rdrop ;

: read-csv ( addr u xt1 xt2 -- ) \ gforth-experimental
    \G read CVS file @var{addr u} and execute @var{xt1} for every item in the
    \G title line and @var{xt2} for all other lines.
    [{: xt1 xt2 :}l xt1 xt2 csv-read-loop ;] >r
    r/o open-file throw r> execute-parsing-file ;
