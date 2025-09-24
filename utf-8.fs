\ UTF-8 handling                                       12dec04py

\ Authors: Bernd Paysan, Anton Ertl
\ Copyright (C) 2004,2005,2006,2007,2008,2009,2010,2011,2013,2015,2016,2018,2019,2020,2021,2022,2023,2024 Free Software Foundation, Inc.

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

-77 Constant UTF-8-err

$80 Constant max-single-byte
[IFUNDEF] invalid-char
$FFFD Constant invalid-char
[THEN]

: u8len ( u8 -- n )
    dup      max-single-byte u< IF  drop 1  EXIT  THEN \ special case ASCII
    $800  2 >r
    BEGIN  2dup u>=  WHILE  5 lshift r> 1+ >r  dup 0= UNTIL  THEN
    2drop r> ;

: u8@+ ( u8addr -- u8addr' u )
    [IFDEF] u8@+?
	4 u8@+? nip
    [ELSE]
	count  dup max-single-byte u< ?EXIT  \ special case ASCII
	dup $C2 u< IF  drop invalid-char EXIT  THEN  \ malformed character
	$7F and  $40 >r
	BEGIN  dup r@ and r@ $100000 u< and  WHILE  r@ xor
		6 lshift r> 5 lshift >r >r count
		dup $C0 and $80 <> IF  drop rdrop rdrop 1- invalid-char EXIT  THEN
		$3F and r> or
	REPEAT  rdrop dup $10FFFF u> IF  drop invalid-char  THEN
    [THEN] ;

: u8!+ ( u u8addr -- u8addr' )
    over max-single-byte u< IF  tuck c! 1+  EXIT  THEN \ special case ASCII
    >r 0 swap  $3F
    BEGIN  2dup u>  WHILE
	    2/ >r  dup $3F and $80 or swap 6 rshift r>
    REPEAT  $7F xor 2* or  r>
    BEGIN   over $80 u>= WHILE  tuck c! 1+  REPEAT  nip ;

\ scan to next/previous character

\ alternative names: u8char+ u8char-

: u8>> ( u8addr -- u8addr' )  u8@+ drop ;
: u8<< ( u8addr -- u8addr' )
    BEGIN  1- dup c@ $C0 and max-single-byte <>  UNTIL ;

\ utf key and emit

Defer check-xy  ' noop IS check-xy

: u8key ( -- u )
    key dup max-single-byte u< ?EXIT  \ special case ASCII
    dup $FF = ?EXIT  \ special resize character
    dup $C2 u< IF  drop invalid-char EXIT  THEN  \ malformed character
    $7F and  $40 >r
    BEGIN  dup r@ and  WHILE  r@ xor
	    6 lshift r> 5 lshift >r >r key
	    dup $C0 and $80 <> IF  drop rdrop invalid-char EXIT  THEN
	    $3F and r> or
    REPEAT  rdrop ;

: u8emit ( u -- )
    dup max-single-byte u< IF  emit  EXIT  THEN \ special case ASCII
    0 swap  $3F
    BEGIN  2dup u>  WHILE
	    2/ >r  dup $3F and $80 or swap 6 rshift r>
    REPEAT  $7F xor 2* or
    BEGIN   dup $80 u>= WHILE  emit  REPEAT  drop ;

\ utf-8 stuff for xchars

: +u8/string ( xc-addr1 u1 -- xc-addr2 u2 )
    u8@+? drop ;
: u8\string- ( xcaddr u -- xcaddr u' )
    BEGIN  dup  WHILE  1- 2dup + c@ $C0 and $80 <>  UNTIL  THEN ;

: u8@ ( c-addr -- u )
    u8@+ nip ;

: u8!+? ( xc xc-addr1 u1 -- xc-addr2 u2 f )
    >r over u8len r@ over u< if ( xc xc-addr1 len r: u1 )
	\ not enough space
	drop nip r> false
    else
	>r u8!+ r> r> swap - true
    then ;

[IFDEF] u8@+?
    : u8addrlen ( u8-addr u -- u1 )
	tuck u8@+? drop nip - ;
[ELSE]
    : u8?valid ( addr u -- u )
	tuck 1- bounds U+DO
	    i c@ $C0 and $80 <> IF  I' I - - unloop  EXIT  THEN
	LOOP ;
    
    Create (u8addrlen)
    1 c, 1 c, 1 c, 1 c,
    1 c, 1 c, 1 c, 1 c,
    1 c, 1 c, 1 c, 1 c,
    2 c, 2 c, 3 c, 4 c,
      DOES> ( pc1 -- len ) swap 4 rshift + c@ ;
    Create (u8mask)
    $0 ,
    $80000000 lbe ,
    $E0C00000 lbe ,
    $F0C0C000 lbe ,
    $F8C0C0C0 lbe ,
      DOES> ( len -- mask ) swap th@ ;
    Create (u8fit)
    $0 ,
    $00000000 lbe ,
    $C0800000 lbe ,
    $E0808000 lbe ,
    $F0808080 lbe ,
      DOES> ( len -- mask ) swap th@ ;

    : u8?invalid ( u8-addr u -- flag len )
	over c@ (u8addrlen) >r ( u8-addr u r:len )
	r@ u< IF  r> min true swap  EXIT  THEN
	l@ r@ (u8mask) and r@ (u8fit) <> r> ;
    
    : u8addrlen ( u8-addr u -- u1 )
	\ length of UTF-8 char starting at u8-addr (accesses only u8-addr)
	dup 0= IF  nip  EXIT  THEN
	2dup u8?invalid >r IF  rdrop over + >r dup u8@+ drop r> umin swap -
	ELSE  2drop r>  THEN ;
[THEN]

: -u8trailing-garbage ( addr u1 -- addr u2 )
    dup 0= ?EXIT
    2dup u8\string- 2over 2over nip safe/string
    u8@+? invalid-char = IF
	2drop 2nip
    ELSE
	2nip + nip over -
    THEN ;

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

: xc-width ( xc -- n ) \ xchar-ext x-c-width
    wc-table #wc-table bounds ?DO
	dup I 2@ within IF  I 2 th@  UNLOOP EXIT  THEN
    3 cells +LOOP  1 ;
[ELSE]
    ' wcwidth Alias xc-width ( xc -- n ) \ xchar-ext x-c-width
    \g @var{xc} has a width of @var{n} times the width of a normal
    \g fixed-width glyph.
[THEN]

: xc-width+ ( n xc -- n' )
    dup #tab = IF  drop 1+ dfaligned  ELSE  xc-width 0 max +  THEN ;
: u8width ( xcaddr u -- n )
    0 -rot bounds ?DO
	I xc@+ swap >r xc-width+
    r> I - +LOOP ;

: xc-hw+ ( h w xc cols -- h' w' ) {: cols :}
    dup #lf = over #cr = or IF  2drop 1+ 0  ELSE
	dup >r xc-width+ dup cols u> IF
	    drop 1+ 0 r> xc-width+
	ELSE  rdrop  THEN
    THEN ;
: +x-lines+rest ( lines chars c-addr u cols -- lines' chars' ) \ gforth-internal
    \G Incremental simulated output of string @var{c-addr u}, and how many
    \G more lines and characters it will use on a terminal with width
    \G @var{cols}
    {: cols :} bounds U+DO
	I xc@+ swap >r cols xc-hw+
    r> I delta-I [ #cr pad c!  #lf pad 1+ c!  pad 2 ] SLiteral string-prefix? -
    I - +LOOP ;
: x-lines+rest ( c-addr u cols -- lines chars ) \ gforth-internal
    \G calculate how many lines an xchar string @var{c-addr u} needs with
    \G @var{cols} characters per line, plus how many chars the last line needs
    >r 0 0 2swap r> +x-lines+rest ;
: x-lines ( c-addr u cols -- lines )
    x-lines+rest drop ;

: x-maxlines+rest ( c-addr u lines cols -- c-addr u' rest ) \ gforth-internal
    \G limit an xchar string @var{c-addr u} to take up at most @var{lines}
    \G with @var{cols} characters per line
    2over {: cols start len :}
    negate 0 2swap bounds U+DO
	dup {: oldlen :} I xc@+ swap >r cols xc-hw+
	over 0>= IF
	    rdrop 2drop  start I over - cols oldlen -  unloop  EXIT
	THEN
    r> I delta-I [ #cr pad c!  #lf pad 1+ c!  pad 2 ] SLiteral string-prefix? -
    I - +LOOP
    >r drop start len cols r> - ;
: x-maxlines ( c-addr u lines cols -- c-addr u' ) \ gforth-internal
    \G limit an xchar string @var{c-addr u} to take up at most @var{lines}
    \G with @var{cols} characters per line
    x-maxlines+rest drop ;

here
' u8emit ,
' u8key ,
' u8>> ,
' u8<< ,
' +u8/string ,
' u8\string- ,
' u8@ ,
' u8!+ ,
' u8!+? ,
' u8@+ ,
' u8len ,
' u8addrlen ,
' u8width ,
' -u8trailing-garbage ,
' u8@+? ,
, here Constant utf-8

: set-encoding-utf-8 ( -- )
    utf-8 set-encoding ;

: utf-8-cold ( -- )
    s" LC_ALL" getenv 2dup d0= IF  2drop
	s" LC_CTYPE" getenv 2dup d0= IF  2drop
	    s" LANG" getenv 2dup d0= IF  2drop
		[ e? os-type s" cygwin" str= ] [IF]
		    s" UTF-8" \ assume UTF-8 in Cygwin
		[ELSE]
		    s" C"
		[THEN] THEN THEN THEN
    2dup s" UTF-8" search >r 2drop s" utf8" search nip nip r> or
    IF  utf-8  ELSE  fixed-width  THEN  set-encoding ;

environment-wordlist set-current

: XCHAR-ENCODING ( -- addr u ) \ environment
    \G Returns a printable ASCII string that reperesents the encoding,
    \G and use the preferred MIME name (if any) or the name in
    \G @url{http://www.iana.org/assignments/character-sets} like
    \G ``ISO-LATIN-1'' or ``UTF-8'', with the exception of ``ASCII'', where
    \G we prefer the alias ``ASCII''.
    xc-vector @ utf-8 = IF s" UTF-8" ELSE s" ISO-LATIN-1" THEN ;

: MAX-XCHAR ( -- xchar ) \ environment
    \G Maximal value for xchar.  This depends on the encoding.
    xc-vector @ utf-8 = IF $10FFFF  ELSE  $FF  THEN ;

: XCHAR-MAXMEM ( -- u ) \ environment
    \G Maximal memory consumed by an xchar in address units
    xc-vector @ utf-8 = IF  4  ELSE  1  THEN ;

\ ' noop Alias X:xchar

forth definitions

:is 'cold ( -- )
    defers 'cold utf-8-cold ;

utf-8-cold
