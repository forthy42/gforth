\ dis-gdb.fs	disassembler using gdb
\
\ Copyright (C) 2004,2007,2008,2010,2011,2014,2015 Free Software Foundation, Inc.

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

: append-extend-string ( addr1 u1 addr2 u2 -- addr3 u1+u2 )
    \ append string2 to string1 with dynamic memory reallocation.
    swap >r dup >r extend-mem ( to addr3 u1+u2 r: addr2 u2 )
    rot r> r> rot rot chars move ;

get-current also see-voc definitions

defer gdb-addr-sep-char ( -- c )

',' constant #comma

: check-gdb-syntax ( -- c )
    \ gdb-7.0 and earlier do what we want with "disassemble addr1 addr2"
    \ gdb-7.1 and later only work with         "disaesemble addr1,addr2"
    \ try the old syntax to see if it works
    s" gdb -ex 'disassemble 0 1' -ex 'quit' 2>/dev/null" r/o open-pipe throw
    dup slurp-fid rot close-pipe throw drop
    s" Dump of assembler code from" search nip nip if
        ['] bl
    else
        ['] #comma
    then
    dup is gdb-addr-sep-char
    execute ;

' check-gdb-syntax is gdb-addr-sep-char

set-current

: disasm-gdb { addr u -- }
    cr addr u
    [: [: { addr u }
            s\" type mktemp >/dev/null && type gdb >/dev/null && file=`mktemp -t gforthdis.XXXXXXXXXX` && file2=`mktemp -t gforthdis.XXXXXXXXXX` && echo \"set verbose off\nset logging file $file\nset logging on\ndisas " save-mem ( addr u addr1 u1 )
            addr 0 <<# gdb-addr-sep-char hold # #s 'x hold # #> append-extend-string #>>
            addr u + 0 <<# # #s 'x hold # #> append-extend-string #>>
        ;] $10 base-execute
    ;] catch if
        ." Gdb does not work, fall back to DUMP"
        2drop ['] dump is discode
        addr u dump exit then
    [ e? os-type s" cygwin" str= ] [IF]
	s\" \nset logging off\nquit\n\" >$file2 && gdb -nx -q -p `ps -p $$ | grep -v PPID | cut -c 10-17` -x $file2 2>/dev/null >/dev/null && rm $file2 && grep -v \"of assembler\" $file && rm $file"
    [ELSE]
	s\" \nset logging off\nquit\n\" >$file2 && gdb -nx -q -p `ps -p $$ -o ppid=` -x $file2 2>/dev/null >/dev/null && rm $file2 && grep -v \"of assembler\" $file && rm $file"
    [THEN]  append-extend-string
    2dup (system) 2swap drop free throw throw if
	addr u dump
    endif ;

' disasm-gdb is discode

previous