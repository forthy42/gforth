\ COLORIZE.STR  Coloured .NAME and WORDS                20may93jaw

\ Copyright (C) 1995 Free Software Foundation, Inc.

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

include ansi.fs

decimal

CREATE CT 30 cells allot
: CT! cells CT + ! ;
: CT@ cells CT + @ ;

VARIABLE Color 20 Color !

: Color: Color @ 1 Color +! constant ;

Color: Hig#

<A White >f A>          0 CT!
<A White >f bold A>     Ali# CT!
<A Brown >f A>          Con# CT!
<A Green >f A>          Var# CT!
<A Cyan >f A>           Def# CT!
<A Yellow >f A>         Val# CT!
<A Brown >f bold A>     Doe# CT!
<A Yellow >f bold A>    Col# CT!
<A Blue >f bold A>      Pri# CT!
<A Red >f bold A>       Str# CT!
<A Green >f bold A>     Com# CT!
<A Red >b A>            Hig# CT!

: (.name) ( lfa -- )
        cell+ dup
        wordinfo cells ct + @ attr!
        count $1f and type blackspace ;

: .name (.name) ct @ attr! ;

: words  context @
         BEGIN  @ dup  WHILE  dup (.name)  REPEAT drop
         ct @ attr! ;

