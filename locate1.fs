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

Variable locate-file[]

: locate-name {: nt -- :}
    nt name>view @ dup cr .sourcepos1
    decode-pos1  nt name>string nip {: lineno charno offset :}
    loadfilename#>str locate-file[] $[]slurp-file
    lineno after-locate + 1+ locate-file[] $[]# umin
    lineno before-locate 1+ - 0 max ?DO  cr
	I 4 .r ." : "
	I 1+ lineno = IF
	    warn-color attr!
	    I locate-file[] $[]@
	    2dup charno <> charno + offset - dup >r type r> /string
	    info-color attr!
	    over nt name>string nip dup >r type r> /string
	    warn-color attr!
	    type
	    default-color attr!
	ELSE
	    I locate-file[] $[]@ type
	THEN
    LOOP
    locate-file[] $[]off ;

: locate ( "name" -- )
    (') locate-name ;