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

s" malformed UTF-8 character" exception Constant UTF-8-err

$80 Value max-single-byte

: u8len ( u8 -- n )
    dup      max-single-byte u< IF  drop 1  EXIT  THEN \ special case ASCII
    $800  2 >r
    BEGIN  2dup u>=  WHILE  5 lshift r> 1+ >r  REPEAT
    2drop r> ;

: u8@+ ( u8addr -- u8addr' u )
    count  dup max-single-byte u< ?EXIT  \ special case ASCII
    $7F and  $40 >r
    BEGIN  dup r@ and  WHILE  r@ xor
	    6 lshift r> 5 lshift >r >r count
	    dup $C0 and $80 <> IF   UTF-8-err throw  THEN
	    $3F and r> or
    REPEAT  rdrop ;

: u8!+ ( u u8addr -- u8addr' )
    over max-single-byte u< IF  tuck c! 1+  EXIT  THEN \ special case ASCII
    >r 0 swap  $3F
    BEGIN  2dup u>  WHILE
	    2/ >r  dup $3F and $80 or swap 6 rshift r>
    REPEAT  $7F xor 2* or  r>
    BEGIN   over $80 u>= WHILE  tuck c! 1+  REPEAT  nip ;

\ plug-in so that char and '<char> work for UTF-8

[ifundef] char@ \ !! bootstrapping help
    Defer char@ ( addr u -- char addr' u' )
    :noname  over c@ -rot 1 /string ; IS char@
[then]

:noname  ( addr u -- char addr' u' )
    \ !! the if here seems to work around some breakage, but not
    \ entirely; e.g., try 'ç' with LANG=C.
    dup 1 u<= IF defers char@ EXIT THEN
    over + >r u8@+ swap r> over - ; IS char@

\ scan to next/previous character

\ alternative names: u8char+ u8char-

: u8>> ( u8addr -- u8addr' )  u8@+ drop ;
: u8<< ( u8addr -- u8addr' )
    BEGIN  1- dup c@ $C0 and max-single-byte <>  UNTIL ;

\ utf key and emit

: u8key ( -- u )
    defers key dup max-single-byte u< ?EXIT  \ special case ASCII
    $7F and  $40 >r
    BEGIN  dup r@ and  WHILE  r@ xor
	    6 lshift r> 5 lshift >r >r defers key
	    dup $C0 and $80 <> IF  UTF-8-err throw  THEN
	    $3F and r> or
    REPEAT  rdrop ;

: u8emit ( u -- )
    dup max-single-byte u< IF  defers emit  EXIT  THEN \ special case ASCII
    0 swap  $3F
    BEGIN  2dup u>  WHILE
	    2/ >r  dup $3F and $80 or swap 6 rshift r>
    REPEAT  $7F xor 2* or
    BEGIN   dup $80 u>= WHILE  defers emit  REPEAT  drop ;

\ utf-8 stuff for xchars

: +u8/string ( c-addr1 u1 -- c-addr2 u2 )
    over dup u8>> swap - /string ;

: -u8/string ( c-addr1 u1 -- c-addr2 u2 )
    over dup u8<< swap - /string ;

: u8@ ( c-addr -- u )
    u8@+ nip ;

: u8!+? ( xc xc-addr1 u1 -- xc-addr2 u2 f )
    >r over u8len r@ over u< if ( xc xc-addr1 len r: u1 )
	\ not enough space
	drop nip r> false
    else
	>r u8!+ r> r> swap - true
    then ;

: u8addrlen ( u8-addr -- u )
    \ length of UTF-8 char starting at u8-addr (accesses only u8-addr)
    c@
    dup $80 u< if drop 1 exit endif
    dup $c0 u< if UTF-8-err throw endif
    dup $e0 u< if drop 2 exit endif
    dup $f0 u< if drop 3 exit endif
    dup $f8 u< if drop 4 exit endif
    dup $fc u< if drop 5 exit endif
    dup $fe u< if drop 6 exit endif
    UTF-8-err throw ;

: -u8trailing-garbage ( addr u1 -- addr u2 )
    2dup + dup u8<< ( addr u1 end1 end2 )
    2dup dup u8addrlen + = if \ last character ok
	2drop
    else
	nip nip over -
    then ;

: set-encoding-utf-8 ( -- )
    ['] u8emit is xemit
    ['] u8key is xkey
    ['] u8>> is xchar+
    ['] u8<< is xchar-
    ['] +u8/string is +x/string
    ['] -u8/string is -x/string
    ['] u8@ is xc@
    ['] u8!+? is xc!+?
    ['] u8@+ is xc@+
    ['] u8len is xc-size
    ['] -u8trailing-garbage is -trailing-garbage
;

\ input editor

[IFUNDEF] #esc  27 Constant #esc  [THEN]

: save-cursor ( -- )  #esc emit '7 emit ;
: restore-cursor ( -- )  #esc emit '8 emit ;
: .rest ( addr pos1 -- addr pos1 )
    restore-cursor 2dup type ;
: .all ( span addr pos1 -- span addr pos1 )
    restore-cursor >r 2dup swap type r> ;

: <u8ins>  ( max span addr pos1 u8char -- max span addr pos2 )
    >r  2over r@ u8len + u< IF  rdrop bell  EXIT  THEN
    >string over r@ u8len + swap move 2dup chars + r@ swap u8!+ drop
    r> u8len >r  rot r@ chars + -rot r> chars + ;
: (u8ins)  ( max span addr pos1 u8char -- max span addr pos2 )
    <u8ins> .all .rest ;
: u8back  ( max span addr pos1 -- max span addr pos2 f )
    dup  IF  over + u8<< over -  0 max .all .rest
    ELSE  bell  THEN 0 ;
: u8forw  ( max span addr pos1 -- max span addr pos2 f )
    2 pick over <> IF  over + u8@+ u8emit over -  ELSE  bell  THEN 0 ;
: (u8del)  ( max span addr pos1 -- max span addr pos2 )
    over + dup u8<< tuck - >r over -
    >string over r@ + -rot move
    rot r> - -rot ;
: ?u8del ( max span addr pos1 -- max span addr pos2 0 )
  dup  IF  (u8del) .all 2 spaces .rest  THEN  0 ;
: <u8del> ( max span addr pos1 -- max span addr pos2 0 )
  2 pick over <>
    IF  u8forw drop (u8del) .all 2 spaces .rest
    ELSE  bell  THEN  0 ;
: u8eof  2 pick over or 0=  IF  bye  ELSE  <u8del>  THEN ;

: u8first-pos  ( max span addr pos1 -- max span addr 0 0 )
  drop 0 .all .rest 0 ;
: u8end-pos  ( max span addr pos1 -- max span addr span 0 )
  drop over .all 0 ;


: u8clear-line ( max span addr pos1 -- max addr )
    drop restore-cursor swap spaces restore-cursor ;
: u8clear-tib ( max span addr pos -- max 0 addr 0 false )
    u8clear-line 0 tuck dup ;

: (u8enter)  ( max span addr pos1 -- max span addr pos2 true )
    >r end^ 2@ hist-setpos
    2dup swap history write-line drop ( throw ) \ don't worry about errors
    hist-pos 2dup backward^ 2! end^ 2!
    r> .all space true ;

: u8kill-expand ( max span addr pos1 -- max span addr pos2 )
    prefix-found cell+ @ ?dup IF  >r
	r@ - >string over r@ + -rot move
	rot r@ - -rot .all r> spaces .rest THEN ;

: insert   ( string length buffer size -- )
    rot over min >r  r@ - ( left over )
    over dup r@ +  rot move   r> move  ;

: u8tab-expand ( max span addr pos1 -- max span addr pos2 0 )
    key? IF  #tab (u8ins) 0  EXIT  THEN
    u8kill-expand 2dup extract-word dup 0= IF  nip EXIT  THEN
    search-prefix tib-full?
    IF    7 emit  2drop  prefix-off
    ELSE  dup >r
	2>r >string r@ + 2r> 2swap insert
	r@ + rot r> + -rot
    THEN
    prefix-found @ IF  bl (u8ins)  ELSE  .all .rest  THEN  0 ;

: utf-8-io ( -- )
    ['] u8forw       ctrl F bindkey
    ['] u8back       ctrl B bindkey
    ['] ?u8del       ctrl H bindkey
    ['] u8eof        ctrl D bindkey
    ['] <u8del>      ctrl X bindkey
    ['] u8clear-tib  ctrl K bindkey
    ['] u8first-pos  ctrl A bindkey
    ['] u8end-pos    ctrl E bindkey
    ['] (u8enter)    #lf    bindkey
    ['] (u8enter)    #cr    bindkey
    ['] u8tab-expand #tab   bindkey
    ['] (u8ins)      IS insert-char
    ['] kill-prefix  IS everychar
    ['] save-cursor  IS everyline
    ['] u8key        IS key
    ['] u8emit       IS emit ;

