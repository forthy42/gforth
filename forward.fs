\ forward definitions

\ Copyright (C) 2016 Free Software Foundation, Inc.

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

\ caveat: forwards don't work with words that use locals!

: forward, ( xt -- )
    >body ['] call peephole-compile, here swap !@ , ;

: forward ( "name" -- )
    \G create a forward reference
    Create 0 , compile-only ['] forward, set-optimizer ;

: resolve-fwds ( addr -- ) \ resolve forward refereneces
    BEGIN  dup  WHILE  latestxt >body swap !@  REPEAT  drop ;

: auto-resolve ( addr u wid -- )
    \G auto-resolve the forward reference in check-shadow
    dup 2over rot find-name-in  dup IF
	dup >namevt @ >vtcompile, @ ['] forward, = IF
	    0 swap >body !@ resolve-fwds  drop 2drop  EXIT
	THEN
    THEN  drop
    defers check-shadow ;

' auto-resolve is check-shadow

: .unresolved ( -- )
    \G print all unresolved forward references
    [: [: dup >namevt @ >vtcompile, @ ['] forward, = IF
		dup >body @ [: dup .name ." is unresolved" cr ;] ?warning
	    THEN  drop true ;] swap traverse-wordlist ;] map-vocs ;
