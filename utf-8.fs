\ UTF-8 handling                                       12dec04py

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

\ short: u8 means utf-8 encoded address

: u8len ( u -- n )
    dup      $80 u< IF  drop 1  EXIT  THEN \ special case ASCII
    $800  2 >r
    BEGIN  2dup u>=  WHILE  5 lshift r> 1+ >r  REPEAT
    2drop r> ;

: u8@+ ( u8addr -- u8addr' u )
    count  dup $80 and 0= ?EXIT  \ special case ASCII
    $7F and  $40 >r
    BEGIN  dup r@ and  WHILE  r@ xor
	    6 lshift r> 5 lshift >r >r count
\	    dup $C0 and $80 <> abort" malformed character"
	    $3F and r> or
    REPEAT  rdrop ;

: u8!+ ( u u8addr -- u8addr' )
    over $80 < IF  tuck c! 1+  EXIT  THEN \ special case ASCII
    >r 0 swap  $3F
    BEGIN  2dup u>  WHILE
	    2/ >r  dup $3F and $80 or swap 6 rshift r>
    REPEAT  $7F xor 2* or  r>
    BEGIN   over $80 u>= WHILE  tuck c! 1+  REPEAT  nip ;

\ scan to next/previous character

: u8>> ( u8addr -- u8addr' )
    BEGIN  count $C0 and $80 <>  UNTIL ;
: u8<< ( u8addr -- u8addr' )
    BEGIN  1- dup c@ $C0 and $80 <>  UNTIL ;

\ utf key and emit

: u8key ( -- u )
    defers key dup $80 and 0= ?EXIT  \ special case ASCII
    $7F and  $40 >r
    BEGIN  dup r@ and  WHILE  r@ xor
	    6 lshift r> 5 lshift >r >r defers key
\	    dup $C0 and $80 <> abort" malformed character"
	    $3F and r> or
    REPEAT  rdrop ;

: u8emit ( u -- )
    dup $80 < IF  defers emit  EXIT  THEN \ special case ASCII
    0 swap  $3F
    BEGIN  2dup u>  WHILE
	    2/ >r  dup $3F and $80 or swap 6 rshift r>
    REPEAT  $7F xor 2* or
    BEGIN   dup $80 u>= WHILE  defers emit  REPEAT  drop ;

\ input editor

: save-cursor ( -- )  27 emit '7 emit ;
: restore-cursor ( -- )  27 emit '8 emit ;
: .rest ( addr pos1 -- addr pos1 )
    restore-cursor 2dup type ;
: .all ( span addr pos1 -- span addr pos1 )
    restore-cursor >r 2dup swap type r> ;

: (u8ins)  ( max span addr pos1 u8char -- max span addr pos2 )
    >r >string over r@ u8len + swap move 2dup chars + r@ swap u8!+ drop
    r> u8len >r  rot r@ chars + -rot r> chars + .all .rest ;
: u8back  ( max span addr pos1 -- max span addr pos2 f )
    dup  IF  over + u8<< over -  0 max .all .rest
    ELSE  #bell emit  THEN 0 ;
: u8forw  ( max span addr pos1 -- max span addr pos2 f )
    2 pick over <> IF  over + u8@+ u8emit over -  ELSE  #bell emit  THEN 0 ;
: (u8del)  ( max span addr pos1 -- max span addr pos2 )
    over + dup u8<< tuck - >r over -
    >string over r@ + -rot move
    rot r> - -rot .all 2 spaces .rest ;
: ?u8del ( max span addr pos1 -- max span addr pos2 0 )
  dup  IF  (u8del)  THEN  0 ;
: <u8del> ( max span addr pos1 -- max span addr pos2 0 )
  2 pick over <>
	IF  u8forw drop (u8del)  ELSE  #bell emit  THEN  0 ;
: u8eof  2 pick over or 0=  IF  bye  ELSE  <u8del>  THEN ;

' u8forw  ctrl F bindkey
' u8back  ctrl B bindkey
' ?u8del  ctrl H bindkey
' u8eof   ctrl D bindkey
' <u8del> ctrl X bindkey
' (u8ins) IS insert-char
' noop IS everychar
' save-cursor IS everyline
' u8key IS key
' u8emit IS emit
