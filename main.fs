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

cell 2 = [IF] 32 [ELSE] 256 [THEN] KB makekernal ( size )
\ create image-header
0 A,	\ base address
0 ,	\ checksum
0 ,	\ image size (without tags)
,	\ dict size
16 KB ,	\ data stack size
16 KB ,	\ FP stack size
16 KB ,	\ return stack size
16 KB ,	\ locals stack size
0 A,	\ code entry point
0 A,	\ throw entry point
16 KB ,	\ unused (possibly tib stack size)
0 ,	\ unused

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

  here         2 cells !  \ image size
  ' boot >body 8 cells !  \ Entry point

UNLOCK Tlast @
LOCK
1 cells - dup forth-wordlist ! Last !
.unresolved
