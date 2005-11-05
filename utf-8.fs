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
    BEGIN  2dup u>=  WHILE  5 lshift r> 1+ >r  dup 0= UNTIL  THEN
    2drop r> ;

: u8@+ ( u8addr -- u8addr' u )
    count  dup max-single-byte u< ?EXIT  \ special case ASCII
    dup $C2 u< IF  UTF-8-err throw  THEN  \ malformed character
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
    dup $C2 u< IF  UTF-8-err throw  THEN  \ malformed character
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

: utf-8-cold ( -- )
    s" LC_ALL" getenv 2dup d0= IF  2drop
	s" LC_CTYPE" getenv 2dup d0= IF  2drop
	    s" LANG" getenv 2dup d0= IF  2drop
		s" C"  THEN THEN THEN
    s" UTF-8" search nip nip
    IF  set-encoding-utf-8  ELSE  set-encoding-fixed-width  THEN ;

' utf-8-cold INIT8 chained

utf-8-cold
