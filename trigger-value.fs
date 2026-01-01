\ Authors: Bernd Paysan
\ Copyright (C) 2024,2025 Free Software Foundation, Inc.

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
fold1: postpone ! >lits postpone perform ;
: +!trigger ( n value-addr xt-addr -- ) >r +! r> perform ;
fold1: postpone +! >lits postpone perform ;
: trigger@ ( value-addr xt-addr -- xt ) nip @ ;
fold1: postpone drop >lits postpone @ ;
: is-trigger ( xt value-addr xt-addr -- ) nip ! ;
fold1: postpone drop >lits postpone ! ;

to-table: trigger-table !trigger +!trigger trigger@ is-trigger n/a
:noname ( xt -- value-addr xt-addr ) >body dup cell+ ;
fold1: >body dup >lits cell+ >lits ;
trigger-table to-class: to-trigger

0 Value dummy-trigger ' noop ,
' to-trigger set-to

: trigger-value ( x xt "name" -- )
    ['] dummy-trigger create-from reveal swap , , ;

\ high level triggers for things that are 0 when unset

10 stack: trigger-stack

: >trigger-stack ( "value1" .. "valuen" "<rparen>" -- )
    trigger-stack $free
    BEGIN  parse-name ")" 2over string-prefix? 0= WHILE
	    rec-forth '-error trigger-stack >stack
    REPEAT  2drop ;
: +trigger ( xt1 xt2 -- )
    dup >r defer@ dup ['] noop <> IF
	2>r :noname 2r> compile, compile, postpone ;
    ELSE  drop  THEN  r> defer! ;
: :trigger-on( ( "value1" .. "valuen" "<rparen>" -- )
    >trigger-stack
    :noname
    trigger-stack get-stack ?dup-IF
	swap compile, ]] 0= [[
	1 U+DO
	    compile, ]] 0= or [[
	LOOP
	postpone ?EXIT
    THEN
    [: {: xt :}
      trigger-stack get-stack 0 ?DO
	  xt swap +trigger
      LOOP ;] colon-sys-xt-offset stick ;
