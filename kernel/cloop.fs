\ Structural Conditionals, loops part                  12dec92py

\ Copyright (C) 1995,1996,1997 Free Software Foundation, Inc.

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

\ Structural Conditionals                              12dec92py

Variable leavings

: (leave)   here  leavings @ ,  leavings ! ;
: LEAVE     compile branch  (leave) ;  immediate restrict
: ?LEAVE    compile 0= compile ?branch  (leave) ;
                                             immediate restrict

: DONE   ( addr -- )  leavings @
  BEGIN  2dup u<=  WHILE  dup @ swap >resolve  REPEAT
  leavings ! drop ;                          immediate restrict

\ Structural Conditionals                              12dec92py

: DO        compile (do)   here ;            immediate restrict

: ?DO       compile (?do)  (leave) here ;
                                             immediate restrict
: FOR       compile (for)  here ;            immediate restrict

: loop]     dup <resolve 2 cells - compile done compile unloop ;

: LOOP      sys? compile (loop)  loop] ;     immediate restrict
: +LOOP     sys? compile (+loop) loop] ;     immediate restrict
: NEXT      sys? compile (next)  loop] ;     immediate restrict

: EXIT compile ;s ; immediate restrict
: ?EXIT postpone IF postpone EXIT postpone THEN ; immediate restrict

