\ fix path in gforth*.exe

\ Copyright (C) 2000,2003 Free Software Foundation, Inc.

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
\ Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111, USA.

." Fixing " 3 arg type ."  with " 2 arg type cr

$CCE0 Constant gforth.exe
$C4F0 Constant gforth-fast.exe
$89F0 Constant gforth-ditc.exe
$6BF0 Constant gforth-itc.exe
$6880 Constant gforth-prof.exe

include string.fs

Variable path$  2 arg path$ $!
Variable pathes$  2 arg pathes$ $!
Variable exe$

\ pathes$ 1 1 $del s" /cygdrive/" pathes$ 0 $ins
\ : fixpathes ( addr u -- )
\   bounds ?DO  I c@ '\ = IF  '/ I c!  THEN  LOOP ;
\ pathes$ $@ fixpathes
s" .;" pathes$ 0 $ins

: fix-exe ( offset addr u -- )
  path$ $@ exe$ $! s" \" exe$ $+! exe$ $+!
  exe$ $@ r/w bin open-file throw >r
  0 r@ reposition-file throw
  pathes$ $@ 2dup + 0 swap c! 1+ r@ write-file throw
  r> close-file throw ;

3 arg evaluate 3 arg fix-exe

bye
