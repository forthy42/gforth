\ MAIN.FS      Kernal main load file                   20may93jaw

\ Copyright (C) 1995,1996,1997,1998 Free Software Foundation, Inc.

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

require ./../errors.fs
require ./../search.fs
require ./../extend.fs

\ include etags.fs

include ./../cross.fs               \ include cross-compiler

decimal

has? kernel-size makekernel ( size )
\ create image-header
has? header [IF]
0 A,	\ base address
0 ,	\ checksum
0 ,	\ image size (without tags)
>address ,	\ dict size
has? stack-size ,	\ data stack size
has? fstack-size ,	\ FP stack size
has? rstack-size ,	\ return stack size
has? lstack-size ,	\ locals stack size
0 A,	\ code entry point
0 A,	\ throw entry point
has? stack-size ,	\ unused (possibly tib stack size)
0 ,	\ unused
0 ,	\ data stack base
0 ,	\ fp stack base
0 ,	\ return stack base
0 ,	\ locals stack base
[THEN]

UNLOCK ghost - drop \ ghost must exist because - would be treated as number
LOCK

doc-off
has? prims [IF]
    include ./aliases.fs             \ include primitive aliases
[ELSE]
    prims-include
    undef-words
    include ./prim.fs
    all-words  UNLOCK LOCK
[THEN]
doc-on

0 AConstant forthstart

\ include ./vars.fs                \ variables and other stuff
\ include kernel/version.fs \ is in $(build)/kernel
include ./kernel.fs              \ load kernel
\ include ./special.fs             \ special must be last!
\ include ./errore.fs
include ./doers.fs
has? file [IF]
include ./args.fs
include ./files.fs               \ load file words
include ./paths.fs
include ./require.fs
[THEN]

has? compiler [IF]
has? glocals [IF]
include ./cond.fs                \ load IF and co
[ELSE]
include ./cond-old.fs            \ load IF and co w/o locals
[THEN]
\ include arch/misc/tt.fs
\ include arch/misc/sokoban.fs
[THEN]
include ./quotes.fs
include ./toolsext.fs
include ./tools.fs               \ load tools ( .s dump )
include ./getdoers.fs

\ Setup                                                13feb93py

has? header [IF]
\    UNLOCK
    here >address 2 cells  !  \ image size
    ' boot >body  8 cells A!  \ Entry point
\    LOCK
[ELSE]
  >boot
[THEN]

include ./pass.fs

.unresolved
