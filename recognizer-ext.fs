\ Recognizer extensions

\ Authors: Bernd Paysan
\ Copyright (C) 2020,2021 Free Software Foundation, Inc.

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

0 Value translate-method-offset
#10 cells constant translate-method-max-offset#
"No more rec method slots free" exception constant translate-method-overflow

: is-translate-method ( xt rectype recmethod -- )
    >body @ >body + ! ;
to-opt: ( xt -- ) >body @ lit, ]] >body + ! [[ ;
: translate-method-defer@ ( xt -- ) >body @ >body + @ ;
defer@-opt: ( xt -- ) >body @ lit, ]] >body + @ [[ ;

: translate-method: ( "name" -- )
    translate-method-offset translate-method-max-offset# u>=
    translate-method-overflow and throw
    Create translate-method-offset ,  cell +to translate-method-offset
    [: ( rec-type ) @ + >body @ execute-;s ;] set-does>
    ['] is-translate-method set-to
    ['] translate-method-defer@ set-defer@ ;

translate-method: translate-int
translate-method: translate-comp
translate-method: translate-post
