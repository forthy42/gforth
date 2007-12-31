\ Copyright (C) 2000,2003,2007 Free Software Foundation, Inc.

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

\ run this with

\ gforth arch/mips/asm.fs arch/mips/disasm.fs arch/mips/testasmcontrol.fs -e "' foo >body 16 disasm"

\ and it will produce something like

\ ( $400EBA98 ) 1 11 10 sltu,
\ ( $400EBA9C ) 1 0 4 bne,
\ ( $400EBAA0 ) 0 0 4 beq,
\ ( $400EBAA4 ) 0 0 -8 beq,

code foo
    10 11 leu if,
    begin,
    ahead,
    2 cs-roll
    then,
    1 cs-roll
    again,
    then,
end-code

