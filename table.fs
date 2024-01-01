\ table fomerly in search.fs

\ Authors: Anton Ertl, Bernd Paysan, Jens Wilke, Neal Crook
\ Copyright (C) 1996,1997,1999,2001,2003,2007,2015,2017,2019,2020,2021,2023 Free Software Foundation, Inc.

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

require hash.fs

\ table (case-sensitive wordlist)

: table-find ( addr len wordlist -- nfa / false )
    >r 2dup r> bucket @ (tablelfind) ;
: table-rec ( addr len wordlist-id -- nfa rectype-nt / rectype-null )
    0 wordlist-id - table-find nt>rec ;

' tablevoc-to  ' table-rec wordlist-class
>namehm @ Constant tablesearch-map
' hashvoc-to   ' table-rec wordlist-class
>namehm @ Constant cs-wordlist-search-map

voclink @ @ @ voclink !

: table ( -- wid ) \ gforth
    \g Create a lookup table (case-sensitive, no warnings).
    tablesearch-map mappedwordlist ;

: cs-wordlist ( -- wid ) \ gforth
    \g Create a case-sensitive wordlist.
    cs-wordlist-search-map mappedwordlist ;

: cs-vocabulary ( "name" -- ) \ gforth
    \g Create a case-sensitive vocabulary
    Vocabulary cs-wordlist-search-map latestnt >wordlist >namehm ! ;

' cs-vocabulary alias voctable
