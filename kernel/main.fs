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

Create mach-file here over 1+ allot place

require errors.fs
require extend.fs
require search.fs

\ include etags.fs

include cross.fs               \ include cross-compiler

decimal

cell 2 = [IF] 32 [ELSE] 256 [THEN] KB makekernel ( size )
\ create image-header
has-header [IF]
0 A,	\ base address
0 ,	\ checksum
0 ,	\ image size (without tags)
,	\ dict size
16 KB ,	\ data stack size
15 KB 512 + ,	\ FP stack size
15 KB ,	\ return stack size
14 KB 512 + ,	\ locals stack size
0 A,	\ code entry point
0 A,	\ throw entry point
16 KB ,	\ unused (possibly tib stack size)
0 ,	\ unused
0 ,	\ data stack base
0 ,	\ fp stack base
0 ,	\ return stack base
0 ,	\ locals stack base
[THEN]

UNLOCK ghost - drop \ ghost must exist because - would be treated as number
LOCK

doc-off
has-prims [IF]
    include kernel/aliases.fs             \ include primitive aliases
[ELSE]
    prims-include
    undef-words
    include kernel/prim.fs
    all-words  UNLOCK LOCK
[THEN]
doc-on

0 AConstant forthstart

include kernel/vars.fs                \ variables and other stuff
include kernel/errore.fs
include kernel/version.fs
include kernel/kernel.fs              \ load kernel
has-files [IF]
include kernel/args.fs
include kernel/files.fs               \ load file words
include kernel/paths.fs
include kernel/require.fs
[THEN]
has-locals [IF]
include kernel/cond.fs                \ load IF and co
[ELSE]
include kernel/cond-old.fs            \ load IF and co w/o locals
[THEN]
include kernel/tools.fs               \ load tools ( .s dump )
include kernel/toolsext.fs
\ include arch/misc/tt.fs
\ include arch/misc/sokoban.fs
include kernel/special.fs             \ special must be last!

\ Setup                                                13feb93py

here normal-dp !
tudp H @ minimal udp !
decimal

has-header [IF]
  here         2 cells !  \ image size
  ' boot >body 8 cells !  \ Entry point
[ELSE]
  >boot
[THEN]

UNLOCK Tlast @
LOCK
1 cells - dup forth-wordlist ! Last !
.unresolved
