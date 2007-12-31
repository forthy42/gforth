\ chains.fs execution chains for gforth			21jun97jaw

\ Copyright (C) 1998,2000,2003,2007 Free Software Foundation, Inc.

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

0 [IF]
This defines execution chains.
The first application for this is building initialization chains:
Think of many modules or program parts, each of it with some specific
initialization code. If we hardcode the initialization routines into a
"master-init" we will get unflexible and are not able only to load some
specific modules...

The chain is basicaly a linked-list. Define a Variable for the head of
linked-list. Name it "foo8" or "foo-chain" to indicate it is a execution
chain.

You can add a word to the list with "' my-init foo8 chained". You can
execute all the code with "foo8 chainperform".
[THEN]

has? cross 
[IF]   e? compiler
[ELSE] true
[THEN]

[IF] \ only needed with compiler

[IFUNDEF] linked
: linked        here over @ a, swap ! ;
[THEN]

\ generic chains

: chained 	( xt list -- ) \ gforth
  linked , ;

[THEN]

: chainperform	( list -- ) \ gforth
  BEGIN @ dup WHILE dup cell+ perform REPEAT drop ;

