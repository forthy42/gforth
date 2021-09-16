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

0 Value recognized-method-offset
#10 cells constant recognized-method-max-offset#
"No more rec method slots free" exception constant recognized-method-overflow

: is-recognized-method ( xt rectype recmethod -- )
    >body @ >body + ! ;
to-opt: ( xt -- ) >body @ lit, ]] >body + ! [[ ;
: recognized-method-defer@ ( xt -- ) >body @ >body + @ ;
defer@-opt: ( xt -- ) >body @ lit, ]] >body + @ [[ ;

: recognized-method: ( "name" -- )
    recognized-method-offset recognized-method-max-offset# u>=
    recognized-method-overflow and throw
    Create recognized-method-offset ,  cell +to recognized-method-offset
    [: ( rec-type ) @ + >body @ execute-;s ;] set-does>
    ['] is-recognized-method set-to
    ['] recognized-method-defer@ set-defer@ ;

recognized-method: recognized-int
recognized-method: recognized-comp
recognized-method: recognized-post

: recognized-by-state ( token -- )
    state @ swap execute-;s ;
