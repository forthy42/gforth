\ Recognizer extensions

\ Authors: Bernd Paysan
\ Copyright (C) 2020 Free Software Foundation, Inc.

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

0 Value rec-method-offset

: is-rec-method ( xt rectype recmethod -- )
    >body @ + ! ;
to-opt: ( xt -- ) >body @ postpone lit+ , postpone ! ;
: rec-method-defer@ ( xt -- ) >body @ + @ ;
defer@-opt: ( xt -- ) >body @ postpone lit+ , postpone @ ;

: rec-method ( "name" -- )
    Create rec-method-offset ,  cell +to rec-method-offset
    [: ( rec-type ) @ + @ execute-;s ;] set-does>
    ['] is-rec-method set-to
    ['] rec-method-defer@ set-defer@ ;

rec-method rectype-int
rec-method rectype-comp
rec-method rectype-post

: rectype-by-state ( rectype -- )
    state @ abs cells + @ execute-;s ;

