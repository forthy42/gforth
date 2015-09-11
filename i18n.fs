\ Internationalization and localization

\ Copyright (C) 2015 Free Software Foundation, Inc.

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

\ This implementation keeps everything in memory, LSIDs are linked
\ together in lists. Each LSID has also a number, which is used to go
\ from native to local LSID.

\ LSIDs

Variable lsids
0 Value lsid#

: native@ ( lsid -- addr u )  cell+ cell+ dup cell+ swap @ ;
: id#@ ( lsid -- n )  cell+ @ ;

: search-lsid ( addr u -- lsid )
    lsids
    BEGIN  @ dup  WHILE  >r 2dup r@ native@ str= r> swap  UNTIL  THEN
    nip nip ;

: append-list ( addr list -- )
    BEGIN  dup @  WHILE  @  REPEAT  ! ;

: sl, ( addr u -- )  dup , here swap dup allot move align ;
: l, ( addr u -- )
    here lsids append-list 0 , lsid# dup , 1+ to lsid# sl, ;
: [l,] ( addr u -- addr )
    2>r postpone AHEAD 2r> align here >r l,
    [defined] bigFORTH [IF] 0 :r T&P [THEN]
    postpone THEN r> ;

: LLiteral ( addr u -- )
    2dup search-lsid dup  IF
        nip nip
    ELSE  drop [l,]  THEN
    postpone Literal ; immediate

: L" ( "lsid<">" -- lsid )
    '"' parse 2dup search-lsid dup  IF
	nip nip
    ELSE  drop align here >r l, r>  THEN ;
comp: '"' parse  postpone LLiteral ;

\ deliberately unique string
: LU" ( "lsid<">" -- lsid )
    '"' parse align here >r l, r> ;
comp: '"' parse [l,] postpone Literal ; immediate

: .lsids ( lsids -- )  BEGIN  @ dup  WHILE dup native@ type cr  REPEAT  drop ;

\ locale@ stuff

$3 Constant locale-depth \ lang country variances
Variable locale-stack  locale-depth 1+ cells allot
here 0 , locale-stack cell+ !

: >locale ( lsids -- )
    locale-stack dup cell+ swap @ 1+ cells + !  1 locale-stack +!
    locale-stack @ locale-depth u>= abort" locale stack full" ;
: locale-drop ( -- )
    -1 locale-stack +!
    locale-stack @ locale-depth u>= abort" locale stack empty" ;
: locale' ( -- addr )  locale-stack dup cell+ swap @ cells + @ ;

: Locale ( "name" -- )
    Create 0 , DOES>  locale-stack off >locale ;
: Country ( "name" -- )
    Create 0 , , DOES>  locale-stack off dup cell+ @ >locale >locale ;

: set-language ( lang -- ior )  locale-stack off >locale 0 ;
: set-country ( country -- ior )
    dup cell+ @ set-language >locale 0 ;

: search-lsid# ( id# lsids -- lsid )
    BEGIN  @ dup  WHILE  >r dup r@ cell+ @ = r> swap  UNTIL  THEN
    nip ;

Variable last-namespace

: locale@ ( lsid -- addr u )
    last-namespace off dup >r id#@
    locale-stack dup cell+ swap @ cells bounds swap DO
	dup I @ search-lsid# dup IF
	    I last-namespace !
	    nip native@ unloop rdrop EXIT  THEN
	drop
    cell -LOOP  drop r>
    native@ ;

: lsid@ ( lsid -- addr u )
    last-namespace @  IF
	dup >r id#@
	last-namespace @ locale-stack cell+  DO
	    dup I @ search-lsid# dup IF
		nip native@ unloop rdrop EXIT  THEN
            drop
	cell -LOOP  drop r>
    THEN  native@ ;

: locale! ( addr u lsid -- )
    >r 2dup r@ locale@ str= IF  rdrop 2drop  EXIT  THEN
    r> id#@ here locale' append-list 0 , , sl, ;

: native-file ( fid -- )
    >r BEGIN  pad $1000 r@ read-line throw  WHILE
	    pad swap l,  REPEAT
    drop r> close-file throw ;

: locale-file ( fid -- )
    >r  lsids
    BEGIN  @ dup  WHILE  pad $1000 r@ read-line throw
	    IF  pad swap 2 pick locale!  ELSE  drop  THEN  REPEAT
    drop r> close-file throw ;

: included-locale ( addr u -- )  r/o open-file throw locale-file ;

: included-native ( addr u -- )  r/o open-file throw native-file ;

[defined] getpathspec 0= [IF]
    : getpathspec ( -- fd )  parse-name r/o open-file throw ;
[THEN]

: include-locale ( -- )  getpathspec locale-file ;
: include-native ( -- )  getpathspec native-file ;

\ easy use

: x" ( "string"<"> -- addr u )
    ['] l" execute locale@ ;
comp: postpone l" postpone locale@ ;

l" FORTH" constant forth-lx
[defined] gforth [IF] s" Gforth" forth-lx locale! [THEN]
\ [defined] bigforth [IF] s" bigFORTH" forth-lx locale! [THEN]
[defined] VFXforth [IF] s" VFX FORTH" forth-lx locale! [THEN]
