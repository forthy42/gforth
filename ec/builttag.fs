\ builttag.fs

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

variable builtnr
create linebuf 200 chars allot
create filename 200 chars allot
0 value fd

: s'
  [char] ' parse postpone sliteral ; immediate

: builttag
  sourcefilename filename place
  'n filename count + 1 chars - c!
  filename count r/o bin open-file
  IF   drop 0 builtnr !
  ELSE 	>r linebuf 100 r@ read-line drop drop
	linebuf swap 0 -rot 0 -rot >number 2drop drop 1+
	builtnr ! r> close-file throw
  THEN
  filename count r/w bin create-file throw to fd
  base @ >r decimal
  builtnr @ s>d <# #S #> fd write-file throw
  s"  constant built#" fd write-line throw
  s' const create builtdate ," ' fd write-file throw
  time&date >r >r >r
  s>d <# ': hold # # #> fd write-file throw
  s>d <# bl hold # # #> fd write-file throw
  drop
  r> s>d <# '. hold # # #> fd write-file throw
  r> s>d <# '. hold # # #> fd write-file throw
  r> s>d <# # # # # #> fd write-file throw
  s' "' fd write-line throw
  s' : .built cr ." Built #" built# . ." Date " builtdate count type cr ;'
  fd write-line throw
  fd close-file throw
  filename count included 
  r> base ! ;
