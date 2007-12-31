\ test search order wordset partially

\ Copyright (C) 2007 Free Software Foundation, Inc.

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

require ./tester.fs

decimal

: test-set-order0 ( c-addr u -- n )
    2>r get-order 2r> 0 set-order ['] evaluate catch dup if
        nip nip then
    >r set-order r> ;

: test-set-order1 ( c-addr u wid -- n )
    2>r get-order 2r> forth-wordlist 1 set-order ['] evaluate catch dup if
        nip nip then
    >r set-order r> ;


{ s" order"      test-set-order0 -> -13 }
{ s" 5e"         test-set-order0 -> 0 5e }
{ s" root +"     test-set-order1 -> -13 }
{ s" root forth" test-set-order1 -> 0 }

