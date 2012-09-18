\ Structural Conditionals, branches with plugins		10May99jaw

\ Copyright (C) 1995-1997,1999,2000,2003,2007 Free Software Foundation, Inc.

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
: >mark       ( -- sys )        here  0 , ;
: >resolve    ( sys -- )        here over - swap ! ;
: <resolve    ( sys -- )        here - , ;

: BUT       sys? swap ;                      	immediate restrict
: YET       sys? dup ;                       	immediate restrict

\ Structural Conditionals                              12dec92py

: AHEAD     branchmark, ;           		immediate restrict
: IF        ?branchmark, ;          		immediate restrict
: THEN      branchto, branchtoresolve, ;     	immediate restrict
: ELSE      sys? compile AHEAD swap compile THEN ;
                                   		immediate restrict

' THEN Alias ENDIF immediate restrict

: BEGIN     branchtomark, ;			immediate restrict
: WHILE     sys? compile IF swap ;		immediate restrict
: AGAIN     sys? branch, ;  			immediate restrict
: UNTIL     sys? ?branch, ;  			immediate restrict
: REPEAT    over 0= ?struc compile AGAIN compile THEN ;
                                             	immediate restrict

