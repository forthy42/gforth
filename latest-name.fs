\ last-name in the dictionary

\ Author: Bernd Paysan
\ Copyright (C) 2025 Free Software Foundation, Inc.

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

: latest-name-in ( wid -- nt ) \ gforth-experimental
    \G return the latest name defined in a vocabulary or 0 if none
    { | w^ result }
    result [{: r :}l r ! false ;] swap traverse-wordlist
    result @ ;
: latest-name ( -- nt ) \ gforth-experimental
    \G return the @code{LATEST-NAME-IN} the first vocabulary in the
    \G search order.
    context @ latest-name-in ;
