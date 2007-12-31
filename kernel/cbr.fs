\ Structural Conditionals, based on branches               12dec92py

\ Copyright (C) 1995,1996,1997,1999,2001,2003,2006,2007 Free Software Foundation, Inc.

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

: ?struc      ( flag -- )       abort" unstructured " ;
: sys?        ( sys -- )        dup 0= ?struc ;
: >mark       ( -- sys )        here  cell allot ;
: >resolve    ( sys -- )        here swap
    [ has? flash [IF] ] flash! [ [ELSE] ] ! [  [THEN] ] ;
: <resolve    ( sys -- )        , ;

: BUT       sys? swap ;                      immediate restrict
: YET       sys? dup ;                       immediate restrict

\ Structural Conditionals                              12dec92py

: AHEAD     postpone branch >mark ;           immediate restrict
: IF        postpone ?branch >mark ;          immediate restrict
: THEN      sys? ( dup @ ?struc ) >resolve ;  immediate restrict
: ELSE      sys? postpone AHEAD swap postpone THEN ;
                                             immediate restrict

' THEN Alias ENDIF immediate restrict

: BEGIN     here ;                           immediate restrict
: WHILE     sys? postpone IF swap ;           immediate restrict
: AGAIN     sys? postpone branch  <resolve ;  immediate restrict
: UNTIL     sys? postpone ?branch <resolve ;  immediate restrict
: REPEAT    over 0= ?struc postpone AGAIN postpone THEN ;
                                             immediate restrict

0 CONSTANT case ( compilation  -- case-sys ; run-time  -- ) \ core-ext
    immediate

: of ( compilation  -- of-sys ; run-time x1 x2 -- |x1 ) \ core-ext
    \ !! the implementation does not match the stack effect
    1+ >r
    postpone over postpone = postpone if postpone drop
    r> ; immediate

: endof ( compilation case-sys1 of-sys -- case-sys2 ; run-time  -- ) \ core-ext end-of
    >r postpone else r> ; immediate

: endcase ( compilation case-sys -- ; run-time x -- ) \ core-ext end-case
    postpone drop
    0 ?do postpone then loop ; immediate
