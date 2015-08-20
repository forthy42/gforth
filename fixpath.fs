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

Variable pathes$  3 arg pathes$ $!

: fixslash ( addr u -- )
    bounds ?DO  I c@ '\' = IF  '/' I c!  THEN  LOOP ;
: fixsemi ( addr u -- )
    bounds ?DO  I c@ ';' = IF  ':' I c!  THEN  LOOP ;
: cygpath ( path -- ) >r
    BEGIN  r@ $@ ':' scan  WHILE
	    r@ $@ drop - { index }
	    r@ index 1 $del
	    s" /cygdrive/" r@ index 1- $ins
    REPEAT  drop r@ $@ fixslash  r@ $@ fixsemi
    0 r> c$+! ;
pathes$ cygpath

." In " 1 arg type ." replace " 2 arg type ."  with " pathes$ $. cr 200 ms

Variable $file

: fix-exe ( addr u -- )
    r/w bin open-file throw >r
    $file r@ $slurp
    $file $@ 2 arg search 2drop $file $@ drop -
    0 r@ reposition-file throw
    pathes$ $@ r@ write-file throw
    r> close-file throw ;

1 arg fix-exe

bye
