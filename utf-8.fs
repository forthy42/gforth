\ UTF-8 handling                                       12dec04py

\ Copyright (C) 2004,2005,2006,2007,2008 Free Software Foundation, Inc.

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

Defer check-xy  ' noop IS check-xy

: u8key ( -- u )
    defers key dup max-single-byte u< ?EXIT  \ special case ASCII
    dup $FF = ?EXIT  \ special resize character
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

: +u8/string ( xc-addr1 u1 -- xc-addr2 u2 )
    over dup u8>> swap - /string ;
: u8\string- ( xcaddr u -- xcaddr u' )
    over + u8<< over - ;

: u8@ ( c-addr -- u )
    u8@+ nip ;

: u8!+? ( xc xc-addr1 u1 -- xc-addr2 u2 f )
    >r over u8len r@ over u< if ( xc xc-addr1 len r: u1 )
	\ not enough space
	drop nip r> false
    else
	>r u8!+ r> r> swap - true
    then ;

: u8addrlen ( u8-addr u -- u )  drop
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
    2dup dup over over - u8addrlen + = if \ last character ok
	2drop
    else
	nip nip over -
    then ;

[IFUNDEF] wcwidth
: wc,3 ( n low high -- )  1+ , , , ;

Create wc-table \ derived from wcwidth source code, for UCS32
0 $0300 $0357 wc,3
0 $035D $036F wc,3
0 $0483 $0486 wc,3
0 $0488 $0489 wc,3
0 $0591 $05A1 wc,3
0 $05A3 $05B9 wc,3
0 $05BB $05BD wc,3
0 $05BF $05BF wc,3
0 $05C1 $05C2 wc,3
0 $05C4 $05C4 wc,3
0 $0600 $0603 wc,3
0 $0610 $0615 wc,3
0 $064B $0658 wc,3
0 $0670 $0670 wc,3
0 $06D6 $06E4 wc,3
0 $06E7 $06E8 wc,3
0 $06EA $06ED wc,3
0 $070F $070F wc,3
0 $0711 $0711 wc,3
0 $0730 $074A wc,3
0 $07A6 $07B0 wc,3
0 $0901 $0902 wc,3
0 $093C $093C wc,3
0 $0941 $0948 wc,3
0 $094D $094D wc,3
0 $0951 $0954 wc,3
0 $0962 $0963 wc,3
0 $0981 $0981 wc,3
0 $09BC $09BC wc,3
0 $09C1 $09C4 wc,3
0 $09CD $09CD wc,3
0 $09E2 $09E3 wc,3
0 $0A01 $0A02 wc,3
0 $0A3C $0A3C wc,3
0 $0A41 $0A42 wc,3
0 $0A47 $0A48 wc,3
0 $0A4B $0A4D wc,3
0 $0A70 $0A71 wc,3
0 $0A81 $0A82 wc,3
0 $0ABC $0ABC wc,3
0 $0AC1 $0AC5 wc,3
0 $0AC7 $0AC8 wc,3
0 $0ACD $0ACD wc,3
0 $0AE2 $0AE3 wc,3
0 $0B01 $0B01 wc,3
0 $0B3C $0B3C wc,3
0 $0B3F $0B3F wc,3
0 $0B41 $0B43 wc,3
0 $0B4D $0B4D wc,3
0 $0B56 $0B56 wc,3
0 $0B82 $0B82 wc,3
0 $0BC0 $0BC0 wc,3
0 $0BCD $0BCD wc,3
0 $0C3E $0C40 wc,3
0 $0C46 $0C48 wc,3
0 $0C4A $0C4D wc,3
0 $0C55 $0C56 wc,3
0 $0CBC $0CBC wc,3
0 $0CBF $0CBF wc,3
0 $0CC6 $0CC6 wc,3
0 $0CCC $0CCD wc,3
0 $0D41 $0D43 wc,3
0 $0D4D $0D4D wc,3
0 $0DCA $0DCA wc,3
0 $0DD2 $0DD4 wc,3
0 $0DD6 $0DD6 wc,3
0 $0E31 $0E31 wc,3
0 $0E34 $0E3A wc,3
0 $0E47 $0E4E wc,3
0 $0EB1 $0EB1 wc,3
0 $0EB4 $0EB9 wc,3
0 $0EBB $0EBC wc,3
0 $0EC8 $0ECD wc,3
0 $0F18 $0F19 wc,3
0 $0F35 $0F35 wc,3
0 $0F37 $0F37 wc,3
0 $0F39 $0F39 wc,3
0 $0F71 $0F7E wc,3
0 $0F80 $0F84 wc,3
0 $0F86 $0F87 wc,3
0 $0F90 $0F97 wc,3
0 $0F99 $0FBC wc,3
0 $0FC6 $0FC6 wc,3
0 $102D $1030 wc,3
0 $1032 $1032 wc,3
0 $1036 $1037 wc,3
0 $1039 $1039 wc,3
0 $1058 $1059 wc,3
1 $0000 $1100 wc,3
2 $1100 $115f wc,3
0 $1160 $11FF wc,3
0 $1712 $1714 wc,3
0 $1732 $1734 wc,3
0 $1752 $1753 wc,3
0 $1772 $1773 wc,3
0 $17B4 $17B5 wc,3
0 $17B7 $17BD wc,3
0 $17C6 $17C6 wc,3
0 $17C9 $17D3 wc,3
0 $17DD $17DD wc,3
0 $180B $180D wc,3
0 $18A9 $18A9 wc,3
0 $1920 $1922 wc,3
0 $1927 $1928 wc,3
0 $1932 $1932 wc,3
0 $1939 $193B wc,3
0 $200B $200F wc,3
0 $202A $202E wc,3
0 $2060 $2063 wc,3
0 $206A $206F wc,3
0 $20D0 $20EA wc,3
2 $2329 $232A wc,3
0 $302A $302F wc,3
2 $2E80 $303E wc,3
0 $3099 $309A wc,3
2 $3040 $A4CF wc,3
2 $AC00 $D7A3 wc,3
2 $F900 $FAFF wc,3
0 $FB1E $FB1E wc,3
0 $FE00 $FE0F wc,3
0 $FE20 $FE23 wc,3
2 $FE30 $FE6F wc,3
0 $FEFF $FEFF wc,3
2 $FF00 $FF60 wc,3
2 $FFE0 $FFE6 wc,3
0 $FFF9 $FFFB wc,3
0 $1D167 $1D169 wc,3
0 $1D173 $1D182 wc,3
0 $1D185 $1D18B wc,3
0 $1D1AA $1D1AD wc,3
2 $20000 $2FFFD wc,3
2 $30000 $3FFFD wc,3
0 $E0001 $E0001 wc,3
0 $E0020 $E007F wc,3
0 $E0100 $E01EF wc,3
here wc-table - Constant #wc-table

\ inefficient table walk:

: wcwidth ( xc -- n )
    wc-table #wc-table over + swap ?DO
	dup I 2@ within IF  I 2 cells + @  UNLOOP EXIT  THEN
    3 cells +LOOP  1 ;
[THEN]
    
: u8width ( xcaddr u -- n )
    0 rot rot over + swap ?DO
        I xc@+ swap >r wcwidth +
    r> I - +LOOP ;

: set-encoding-utf-8 ( -- )
    ['] u8emit is xemit
    ['] u8key is xkey
    ['] u8>> is xchar+
    ['] u8<< is xchar-
[ [IFDEF] xstring+ ]
    ['] u8\string- is xstring-
    ['] +u8/string is +xstring
[ [THEN] ]
[ [IFDEF] +x/string ]
    ['] u8\string- is x\string-
    ['] +u8/string is +x/string
[ [THEN] ]
    ['] u8@ is xc@
    ['] u8!+? is xc!+?
    ['] u8@+ is xc@+
    ['] u8len is xc-size
[ [IFDEF] x-width ]
    ['] u8width is x-width
[ [THEN] ]
[ [IFDEF] x-size ]
    ['] u8addrlen is x-size
[ [THEN] ]
    ['] -u8trailing-garbage is -trailing-garbage
;

: utf-8-cold ( -- )
    s" LC_ALL" getenv 2dup d0= IF  2drop
	s" LC_CTYPE" getenv 2dup d0= IF  2drop
	    s" LANG" getenv 2dup d0= IF  2drop
		s" C"  THEN THEN THEN
    s" UTF-8" search nip nip
    IF  set-encoding-utf-8  ELSE  set-encoding-fixed-width  THEN ;

environment-wordlist set-current
: xchar-encoding ( -- addr u ) \ xchar-ext
    \G Returns a printable ASCII string that reperesents the encoding,
    \G and use the preferred MIME name (if any) or the name in
    \G @url{http://www.iana.org/assignments/character-sets} like
    \G ``ISO-LATIN-1'' or ``UTF-8'', with the exception of ``ASCII'', where
    \G we prefer the alias ``ASCII''.
    max-single-byte $80 = IF s" UTF-8" ELSE s" ISO-LATIN-1" THEN ;
forth definitions

:noname ( -- )
    defers 'cold
    utf-8-cold
; is 'cold

utf-8-cold
