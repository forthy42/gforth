\ dis-gdb.fs	disassembler using gdb
\
\ Authors: Anton Ertl, Bernd Paysan
\ Copyright (C) 2004,2007,2008,2010,2011,2014,2015,2016,2019,2021,2023,2025 Free Software Foundation, Inc.

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

: check-addr-sep-char ( -- c )
    \ gdb-7.0 and earlier do what we want with "disassemble addr1 addr2"
    \ gdb-7.1 and later only work with         "disaesemble addr1,addr2"
    \ try the old syntax to see if it works
    s" gdb -ex 'disassemble 0, 1' -ex 'quit' 2>/dev/null" r/o open-pipe throw
    dup slurp-fid rot close-pipe throw drop
    s" Dump of assembler code from" search nip nip if
        ['] #comma
    else
        ['] bl
    then
    dup is gdb-addr-sep-char
    execute ;

' check-addr-sep-char is gdb-addr-sep-char


defer gdb-set-logging-syntax ( -- )
\ prints "" or "enabled "

: .enabled ." enabled " ;

: check-set-logging-syntax ( -- )
    \ checks gdb and then prints "" or "enabled"
    \ gdb 7.11 and 8.2 do not understand "enabled", gdb-10.1 prefers it
    s" gdb -ex 'set logging enabled on' -ex 'quit' 2>&1" r/o open-pipe throw
    dup slurp-fid rot close-pipe throw drop
    s" Undefined set logging command" search nip nip if
        `noop
    else
        `.enabled
    then
    dup is gdb-set-logging-syntax
    execute ;

`check-set-logging-syntax is gdb-set-logging-syntax


set-current

: ppid ( -- )
    [ e? os-type s" cygwin" str= ] [IF]
        ." | grep -v PPID | cut -c 10-17"
    [ELSE]
        ." -o ppid="
    [THEN] ;

: 0x. ( u -- )
    ." 0x" [: 0 u.r ;] $10 base-execute ;

: disasm-gdb { addr u -- }
    u 0= if
        cr exit then
    cr addr u [: { addr u }
        \ .\" set -x\n"
        .\" type mktemp >/dev/null && "
        .\" type gdb >/dev/null && "
        .\" file=`mktemp -t gforthdis.XXXXXXXXXX` && "
        .\" file2=`mktemp -t gforthdis.XXXXXXXXXX` && \n"
        .\" trap \"rm $file $file2\" EXIT && "
        .\" echo \"set verbose off\nset logging file $file\n"
        .\" set logging " gdb-set-logging-syntax .\" on\n"
        .\" disas " addr 0x. gdb-addr-sep-char emit addr u + 0x. cr
        .\" set logging " gdb-set-logging-syntax .\" off\nquit\n\" >$file2 && "
        .\" gdb -nx -batch -p `ps -p $$ " ppid .\" ` -x $file2 2>/dev/null >/dev/null && "
        .\" ! grep -q 'Cannot access memory at address' $file && "
        .\" grep -v \"of assembler\" $file"
    ;] >string-execute
    \ cr 2dup type cr
    2dup (system) 2swap drop free throw throw if
        .\" gdb cannot not access gforth; on Linux this can be fixed with (in the shell)\n"
        .\"   sudo echo 0 >/proc/sys/kernel/yama/ptrace_scope\n"
        .\" and permanently by editing /etc/sysctl.d/10-ptrace.conf\n"
        .\" Alternatively, you can use DUMP instead of DISASM-GDB by setting\n"
        .\"     `dump is discode\n"
    endif ;

' disasm-gdb is discode

previous
