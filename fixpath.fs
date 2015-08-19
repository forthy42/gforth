\ fix path in gforth*.exe

\ Copyright (C) 2000,2003,2006,2007,2008,2012 Free Software Foundation, Inc.

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

: "search s" .:/usr/local" ;

Variable pathes$  2 arg pathes$ $!

pathes$ 1 1 $del s" /cygdrive/" pathes$ 0 $ins
: fixpathes ( addr u -- )
  bounds ?DO  I c@ '\ = IF  '/ I c!  THEN  LOOP ;
pathes$ $@ fixpathes
s" .:" pathes$ 0 $ins

." Fixing " 1 arg type ."  with " pathes$ $. cr 200 ms

0 Value #size
0 Value #file
: fix-exe ( addr u -- )
  r/w bin open-file throw >r
  r@ file-size throw drop to #size
  #size allocate throw to #file
  #file #size r@ read-file throw drop
  #file #size "search search 2drop #file -
  0 r@ reposition-file throw
  pathes$ $@ 2dup + 0 swap c! 1+ r@ write-file throw
  r> close-file throw ;

1 arg fix-exe

bye
