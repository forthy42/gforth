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

0 Value token-method-offset
#10 cells constant token-method-max-offset#
"No more rec method slots free" exception constant token-method-overflow

: is-token-method ( xt rectype recmethod -- )
    >body @ >body + ! ;
to-opt: ( xt -- ) >body @ lit, ]] >body + ! [[ ;
: token-method-defer@ ( xt -- ) >body @ >body + @ ;
defer@-opt: ( xt -- ) >body @ lit, ]] >body + @ [[ ;

: token-method: ( "name" -- )
    token-method-offset token-method-max-offset# u>=
    token-method-overflow and throw
    Create token-method-offset ,  cell +to token-method-offset
    [: ( rec-type ) @ + >body @ execute-;s ;] set-does>
    ['] is-token-method set-to
    ['] token-method-defer@ set-defer@ ;

token-method: token-int
token-method: token-comp
token-method: token-post

: token-by-state ( token -- )
    state @ swap execute-;s ;
