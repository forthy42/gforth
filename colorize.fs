\ colorize.fs  Coloured .NAME and WORDS                20may93jaw

\ Copyright (C) 1995,1996,1997,1999,2001,2003,2007 Free Software Foundation, Inc.

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

needs ansi.fs

decimal

CREATE CT 30 cells allot
: CT! cells CT + ! ;
: CT@ cells CT + @ ;

VARIABLE Color 20 Color !

: Color: Color @ 1 Color +! constant ;

\ define colours for the different stuff that can be found in the
\ dictionary; see wordinfo.fs for the descriptions/definitions
Color: Hig#

<A White >FG A>             0 CT!
<A White >FG bold A>     Ali# CT!
<A Brown >FG A>          Con# CT!
<A Green >FG A>          Var# CT!
<A Cyan >FG A>           Def# CT!
<A Yellow >FG A>         Val# CT!
<A Brown >FG bold A>     Doe# CT!
<A Yellow >FG bold A>    Col# CT!
<A Blue >FG bold A>      Pri# CT!
<A Red >FG bold A>       Str# CT!
<A Green >FG bold A>     Com# CT!
<A Red >BG A>            Hig# CT!

: (.name) ( nfa -- )
        dup wordinfo cells ct + @ attr!
        .name blackspace ;

: .name (.name) ct @ attr! ;

\ nac31mar1999 fixed up for 0.4.0 to match std words
: words  context @ wordlist-id
         BEGIN  @ dup  WHILE  dup (.name)  REPEAT drop
         ct @ attr! ;

