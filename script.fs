\ Copyright (C) 2001,2003,2007 Free Software Foundation, Inc.

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

\ scripting extensions

: sh-eval ( addr u -- xt )
    \G evaluate string + rest of command line
    2dup 2>r >in @ >r
    drop source drop - >in ! source >in @ /string dup >in +!
    system
    $? IF  r> >in ! 2r> defers interpreter-notfound1
    ELSE  rdrop 2rdrop ['] noop THEN ;
' sh-eval IS interpreter-notfound1

2Variable sh$  0. sh$ 2!
: sh-get ( addr u -- addr' u' )
    \G open command addr u, and read in the result
    sh$ free-mem-var
    r/o open-pipe throw dup >r slurp-fid
    r> close-pipe throw to $? 2dup sh$ 2! ;

:noname '` parse sh-get ;
:noname '` parse postpone SLiteral postpone sh-get ;
interpret/compile: s`