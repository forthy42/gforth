\ mirror.fs mirrors ram in rom and copies back at startup

\ Copyright (C) 1998 Free Software Foundation, Inc.

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

0 [IF]

For the romable feature:

We need to save the ram area (there might be initialized variables,
and code-fields...) into rom and copy it back at system startup.

[THEN]

\ save ram area

unlock >CROSS

: saveram
  mirror-link 
  BEGIN @ dup WHILE
	>r r@ >rstart @ r@ >rdp @ over - tuck
	2dup X , X , X here swap tcmove
	X allot X align

>rom
unlock sramdp @ lock		constant ram-start
unlock ramdp @ sramdp @ - lock	constant ram-len
variable ram-origin
ram-start ram-origin ram-len unlock tcmove lock 
ram-len allot align
>auto

: mirrorram
  ram-origin ram-start ram-len cmove ;

