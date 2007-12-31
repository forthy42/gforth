\ Structural Conditionals, loops no extra (?do)		10May99jaw

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

[IFDEF] (?do)
	cr ." Warning: (?do) is defined, use cloop.fs" 
[THEN]

Variable tleavings 0 tleavings !

: DONE   ( addr -- )
      tleavings @
      BEGIN  dup
      WHILE  >r dup r@ cell+ @ \ address of branch
             u> 0=         \ lower than DO?
      WHILE  r@ 2 cells + @ \ branch token
             branchtoresolve,
             r@ @ r> free throw
      REPEAT drop r>
      THEN
      tleavings ! drop ; immediate restrict

: (leave ( branchtoken -- )
    3 cells allocate throw >r
    here r@ cell+ !
    r@ 2 cells + !
    tleavings @ r@ !
    r> tleavings ! ;

: LEAVE     branchmark, (leave ;  		immediate restrict
: ?LEAVE    compile 0= ?branchmark, (leave ;	immediate restrict

\ Structural Conditionals                              12dec92py

\ !!JW ToDo : Move to general tools section

: to1 ( x1 x2 xn n -- addr )
\G packs n stack elements in a allocated memory region
   dup dup 1+ cells allocate throw dup >r swap 1+ 0 DO tuck ! cell+ LOOP drop r> ;
: 1to ( addr -- x1 x2 xn )
\G unpacks the elements saved by to1
   dup @ swap over cells + swap 0 DO dup @ swap 1 cells - LOOP free throw ;

: loop]     branchto, dup <resolve 1 cells - compile DONE ;

: skiploop] ?dup IF compile THEN THEN ;

: DO	0 compile (do) branchtomark, 2 to1  ;		immediate restrict

: ?DO 	compile 2dup compile = compile IF
	compile 2drop compile ELSE
	compile (do) branchtomark, 2 to1 ;		immediate restrict

: FOR   compile (for) branchtomark, ;			immediate restrict

: LOOP      sys?  1to compile (loop)  loop] compile unloop skiploop] ;     
							immediate restrict
: +LOOP     sys? 1to compile (+loop)  loop] compile unloop skiploop] ;
							immediate restrict
: NEXT      sys? compile (next)  loop] compile unloop ;
							immediate restrict
: EXIT compile ;s ; immediate restrict
: ?EXIT postpone IF postpone EXIT postpone THEN ; immediate restrict
