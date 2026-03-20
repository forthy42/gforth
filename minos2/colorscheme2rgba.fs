\ Read KDE colorscheme files into memory to write out as PNG

\ Authors: Bernd Paysan
\ Copyright (C) 2026 Free Software Foundation, Inc.

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

$Variable imagebuf
$20 sfloats buffer: Colorbuf

: color! ( abgr index -- )
    sfloats colorbuf + >r lle r> l! ;

: ##, ( -- n )
    ',' parse snumber?
    case
	0 of 0 endof
	-1 of endof
	nip
    endcase ;

: comb ( u1 u2 -- u3 )  8 lshift or ;

: set-color ( index -- )
    >r '=' parse 2drop ##, ##, ##, $FF comb comb comb r> color! ;

: Color: ( index -- )
    Create ,
    DOES> refill drop @ set-color ;

: colors ( n -- )
    0 ?DO  refill 0= ?LEAVE  I Color:  LOOP ;

Vocabulary colorscheme
get-current >r
also colorscheme definitions

32 colors
[Foreground]
[ForegroundFaint]
[ForegroundIntense]
[ForegroundReserved]
[Background]
[BackgroundFaint]
[BackgroundIntense]
[BackgroundReserved]
[Color0]
[Color1]
[Color2]
[Color3]
[Color4]
[Color5]
[Color6]
[Color7]
[Color0Intense]
[Color1Intense]
[Color2Intense]
[Color3Intense]
[Color4Intense]
[Color5Intense]
[Color6Intense]
[Color7Intense]
[Color0Faint]
[Color1Faint]
[Color2Faint]
[Color3Faint]
[Color4Faint]
[Color5Faint]
[Color6Faint]
[Color7Faint]

: [General] ( -- )
    \G scan description
    BEGIN  refill  WHILE  source nip 0= UNTIL  THEN ;

previous r> set-current

: read-color-file ( addr u -- )
    Colorbuf $20 sfloats erase
    [ ' colorscheme >wordlist ]L ['] rec-forth ['] included wrap-xt
    Colorbuf $20 sfloats imagebuf $+! ;

: read-color-files ( -- )
    BEGIN  argc @ 2 >  WHILE
	    next-arg read-color-file
    REPEAT ;

require unix/stb-image-write.fs

: write-png-map ( addr u -- )
    1 stbi_flip_vertically_on_write
    $20 imagebuf $@len $20 sfloats / 1 sfloats
    imagebuf $@ drop $20 sfloats stbi_write_png 
    0 stbi_flip_vertically_on_write ;

script? [IF] read-color-files next-arg write-png-map bye [THEN]
