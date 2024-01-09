\ Authors: Bernd Paysan
\ Copyright (C) 2024 Free Software Foundation, Inc.

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

: !trigger ( w value-addr xt-addr -- ) >r ! r> perform ;
opt: ?fold-to postpone ! >lits postpone perform ;
: +!trigger ( n value-addr xt-addr -- ) >r +! r> perform ;
opt: ?fold-to postpone +! >lits postpone perform ;
: trigger@ ( value-addr xt-addr -- xt ) nip @ ;
opt: ?fold-to postpone drop >lits postpone @ ;
: is-trigger ( xt value-addr xt-addr -- ) nip ! ;
opt: ?fold-to postpone drop >lits postpone ! ;

to-table: trigger-table !trigger +!trigger n/a trigger@ is-trigger
:noname ( xt -- value-addr xt-addr ) >body dup cell+ ;
opt: ?fold-to >body dup >lits cell+ >lits ;
trigger-table to-method: to-trigger

0 Value dummy-trigger ' noop ,
' to-trigger set-to

: trigger-value ( x xt "name" -- )
    ['] dummy-trigger create-from reveal swap , , ;

\\\ potential optimization: early bound trigger:

: !early-trigger >r ! r> perform ;
opt: ?fold-to postpone ! @ compile, ;
: +!early-trigger >r +! r> perform ;
opt: ?fold-to postpone +! @ compile, ;

to-table: early-trigger-table !early-trigger +!early-trigger n/a trigger@ is-trigger
' >body early-trigger-table to-method: to-early-trigger

0 Value dummy-early-trigger ' noop ,
' to-early-trigger set-to

: early-trigger-value ( x xt "name" - )
    ['] dummy-early-trigger create-from reveal swap , , ;
