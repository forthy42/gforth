\ process options in an extensible way

\ Copyright (C) 2016 Free Software Foundation, Inc.

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

Defer image-options
:noname ( -- )
    ."   FILE				    load FILE (with `require')" cr
    ."   -e STRING, --evaluate STRING	    interpret STRING (with `EVALUATE')" cr
    ."   -Werror|pedantic|all|on|           set warning levels" cr
; is image-options

Vocabulary options

options also definitions

: --evaluate ( -- ) next-arg args-evaluate ;
' --evaluate alias -e

: --help ( -- )
    ." Image Options:" cr image-options
    ." Report bugs on <https://savannah.gnu.org/bugs/?func=addbug&group=gforth>" cr
    bye ;
' --help alias -h

: -Werror ( -- )    -4 warnings ! ;
: -Wpedantic ( -- ) -3 warnings ! ;
: -Wall ( -- )      -2 warnings ! ;
: -Won ( -- )       -1 warnings ! ;
: -W ( -- )          warnings off ;

previous forth definitions

: process-voc-option ( addr u -- true / addr u false )
    2dup [ ' options >body ] Literal search-wordlist
    IF    nip nip execute true
    ELSE  false  THEN ;

' process-voc-option is process-option