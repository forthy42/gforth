\ MAIN.FS      Kernal main load file                   20may93jaw

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

\ : include bl word count included ;
\ we want write include...

\ : : ( -- colon-sys )  Header [ ' : @ ] ALiteral cfa, 0 ] ;
\ : ; ( colon-sys -- )  ?struc postpone exit reveal postpone [ ; immediate
\ : :noname ( -- xt colon-sys )  here [ ' : @ ] ALiteral cfa, 0 ] ;

[IFUNDEF] vocabulary include search-order.fs [THEN]
\ include etags.fs

include cross.fs               \ include cross-compiler

decimal

cell 2 = [IF] 32 [ELSE] 256 [THEN] KB
makekernal , 0 , 0 , 0 A, 0 A, 0 A,

UNLOCK ghost - drop \ ghost must exist because - would be treated as number
LOCK

0 AConstant forthstart

include aliases.fs             \ include primitive aliases
\ include cond.fs                \ conditional compile
\ include patches.fs             \ include primitive patches

include vars.fs                \ variables and other stuff
include add.fs                 \ additional things
include errore.fs
include kernal.fs              \ load kernal
include version.fs
include extend.fs              \ load core-extended
include tools.fs               \ load tools ( .s dump )
\ include words.fs
\ include wordinfo.fs
\ include see.fs                 \ load see
include toolsext.fs
\ include search-order.fs

\ Setup                                                13feb93py

here normal-dp !
tudp H @ minimal udp !
decimal

\ 64 KB        0 cells !  \ total Space... defined above!
  here         1 cells !  \ Size of the system
  16 KB        2 cells !  \ Return and fp stack size
  ' boot >body 3 cells !  \ Entry point

UNLOCK Tlast @
LOCK
1 cells - dup forth-wordlist ! Last !
.unresolved

cr cr
cell bigendian
[IF]
   dup 2 = [IF]   save-cross kernl16b.fi-  [THEN]
   dup 4 = [IF]   save-cross kernl32b.fi-  [THEN]
   dup 8 = [IF]   save-cross kernl64b.fi-  [THEN]
[ELSE]
   dup 2 = [IF]   save-cross kernl16l.fi-  [THEN]
   dup 4 = [IF]   save-cross kernl32l.fi-  [THEN]
   dup 8 = [IF]   save-cross kernl64l.fi-  [THEN]
[THEN] drop cr

bye
