\ dynamic string handling                              10aug99py

\ Copyright (C) 2000 Free Software Foundation, Inc.

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
\ Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.

: delete   ( buffer size count -- )
  over min >r  r@ - ( left over )  dup 0>
  IF  2dup swap dup  r@ +  -rot swap move  THEN  + r> bl fill ;

: insert   ( string length buffer size -- )
  rot over min >r  r@ - ( left over )
  over dup r@ +  rot move   r> move  ;

: $padding ( n -- n' )
  [ 6 cells ] Literal + [ -4 cells ] Literal and ;
: $! ( addr1 u addr2 -- )
  dup @ IF  dup @ free throw  THEN
  over $padding allocate throw over ! @
  over >r  rot over cell+  r> move 2dup ! + cell+ bl swap c! ;
: $@len ( addr -- u )  @ @ ;
: $@ ( addr1 -- addr2 u )  @ dup cell+ swap @ ;
: $!len ( u addr -- )
  over $padding over @ swap resize throw over ! @ ! ;
: $del ( addr off u -- )   >r >r dup $@ r> /string r@ delete
  dup $@len r> - swap $!len ;
: $ins ( addr1 u addr2 off -- ) >r
  2dup dup $@len rot + swap $!len  $@ 1+ r> /string insert ;
: $+! ( addr1 u addr2 -- ) dup $@len $ins ;
: $off ( addr -- )  dup @ free throw off ;

\ dynamic string handling                              12dec99py

: $split ( addr u char -- addr1 u1 addr2 u2 )
  >r 2dup r> scan dup >r dup IF  1 /string  THEN
  2swap r> - 2swap ;

: $iter ( .. $addr char xt -- .. ) { char xt }
  $@ BEGIN  dup  WHILE  char $split >r >r xt execute r> r>
     REPEAT  2drop ;
