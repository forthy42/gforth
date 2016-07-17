\ SwiftForth-like locate

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

\ lines to show before and after locate
3 value before-locate
12 value after-locate

: locate-next-line ( c-addr1 u1 lineno -- c-addr2 u2 lineno+1 )
    ... ;

: locate-highlight-line ( c-addr1 u1 lineno charno nt --  c-addr2 u2 lineno+1 )
    ... ;

: locate-print-line ( c-addr1 u1 lineno -- c-addr2 u2 lineno+1 )
    ... ;
    
: locate-name {: nt -- :}
    nt name>view dup cr .sourcepos1
    decode-pos1 {: lineno charno :}
    loadfilename#>str slurp-file over {: c-addr :}
    1 case ( c-addr u lineno1 )
        dup lineno after-locate + >= ?of endof
        over 0= ?of endof
        dup lineno = ?of charno nt locate-highlight-line contof
        dup lineno before-locate - >= ?of locate-print-line contof
        locate-next-line
    next-case
    2drop drop c-addr free throw ;

: locate ( "name" -- )
    (') locate-name ;