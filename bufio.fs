\ BUFOUT.STR    Buffered output for Debug               13jun93jaw

\ Copyright (C) 1995,1996,1997,2000,2003,2007 Free Software Foundation, Inc.

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

CREATE O-Buffer 4000 chars allot align
VARIABLE O-PNT

: O-TYPE        O-PNT @ over chars O-PNT +!
                swap move ;

: O-EMIT        O-PNT @ c! 1 chars O-PNT +! ;

VARIABLE EmitXT
VARIABLE TypeXT

: O-INIT        What's type TypeXT !
                What's emit EmitXT !
                O-Buffer O-PNT !
                ['] o-type IS type
                ['] o-emit IS emit ;

: O-DEINIT      EmitXT @ IS Emit
                TypeXT @ IS Type ;

: O-PNT@        O-PNT @ O-Buffer - ;

