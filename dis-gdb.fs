\ dis-gdb.fs	disassembler using gdb
\
\ Copyright (C) 2004 Free Software Foundation, Inc.

\ This file is part of Gforth.

\ Gforth is free software; you can redistribute it and/or
\ modify it under the terms of the GNU General Public License
\ as published by the Free Software Foundation; either version 2
\ of the License, or (at your option) any later version.

\ This program is distributed in the hope that it will be useful,
\ but WITHOUT ANY WARRANTY; without even the implied warranty of
\ MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
\ GNU General Public License for more details.

\ You should have received a copy of the GNU General Public License
\ along with this program; if not, write to the Free Software
\ Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111, USA.

: append-extend-string ( addr1 u1 addr2 u2 -- addr2 u1+u2 )
    \ concatenate string1 and string2 with dynamic memory allocation
    swap >r dup >r extend-mem ( to addr2 u1+u2 r: addr2 u2 )
    rot r> r> rot rot chars move ;

: disasm-gdb { addr u -- }
    base @ >r hex
    s\" file=`mktemp -t gforthdis.XXXXXXXXXX` && file2=`mktemp -t gforthdis.XXXXXXXXXX` && echo \"set verbose off\nset logging file $file\nset logging on\ndisas " save-mem ( addr u addr1 u1 )
    addr 0 <<# bl hold # #s 'x hold # #> append-extend-string #>>
    addr u + 0 <<# # #s 'x hold # #> append-extend-string #>>
    r> base ! cr
    s\" \nset logging off\nquit\n\" >$file2 && gdb -nx -q -p $PPID -x $file2 2>/dev/null >/dev/null && rm $file2 && grep -v \"of assembler\" $file && rm $file" append-extend-string
    2dup (system) 2swap drop free throw throw if
	addr u dump
    endif ;

' disasm-gdb is discode
