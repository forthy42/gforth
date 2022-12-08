\ Internationalization and localization

\ Authors: Bernd Paysan, Anton Ertl
\ Copyright (C) 2015,2018,2019,2020,2021 Free Software Foundation, Inc.

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

require set-compsem.fs

\ LSIDs

$[]Variable lsids 0 ,
: lsid# ( -- n )
    lsids $[]# 1- ;

: native@ ( lsid -- addr u ) \ gforth-experimental native-fetch
    \G fetch native string from an @var{lsid}
    lsids $[]@ ;

: search-lsid ( addr u -- lsid )
    lsid# 1+ 0 ?DO
	2dup I lsids $[]@ str= IF  2drop I unloop  EXIT  THEN
    LOOP  2drop -1 ;

: $l, ( addr u -- )  dup , here swap dup allot move align ;
: new-lsid ( addr u -- lsid )
    lsids $+[]! lsid# ;
: ?new-lsid ( addr u -- lsid )
        2dup search-lsid dup 0>= IF
        nip nip
    ELSE
	drop new-lsid
    THEN ;

: LLiteral ( addr u -- )
    ?new-lsid postpone Literal ; immediate

: L" ( "lsid<">" -- lsid ) \ gforth-experimental l-quote
    \G Parse a string and define a new lsid, if the string is uniquely new.
    \G Identical strings result in identical lsids, which allows to refer
    \G to the same lsid from multiple locations using the same string.
    '"' parse ?new-lsid ;
compsem: '"' parse  postpone LLiteral ;

\ deliberately unique string
: LU" ( "lsid<">" -- lsid ) \ gforth-experimental l-unique-quote
    \G Parse a string and always define a new lsid, even if the string is not
    \G unique.
    '"' parse new-lsid ;
compsem: '"' parse new-lsid postpone Literal ; immediate

: .lsids ( locale -- ) \ gforth-experimental dot-lsids
    \G print the string for all lsids
    $[]. ;

\ locale@ stuff

$[]Variable default-locale lsids ,
default-locale Value locale

: Language ( "name" -- ) \ gforth-experimental
    \G define a locale.  Executing that locale makes it the current locale.
    $[]Variable default-locale ,
  DOES> to locale ;
: Country ( <lang> "name" -- ) \ gforth-experimental
    \G define a variant (typical: country) for the current locale.  Executing
    \G that locale makes it the current locale.  You can create variants of
    \G variants (a country may have variants within, e.g. think of how many
    \G words for rolls/buns there are in many languages).
    $[]Variable locale ,
  DOES> to locale ;

: locale@ ( lsid -- addr u ) \ gforth-experimental locale-fetch
    \G fetch the localized string in the current language and country
    locale
    BEGIN  2dup $[]@ 2dup d0= WHILE
	2drop cell+ @ dup 0= UNTIL  0 0  THEN
    2nip ;

: locale! ( addr u lsid -- ) \ gforth-experimental locale-store
    \G Store localized string @var{addr u} for the current locale and country
    \G in @var{lsid}.
    locale $[]! ;

: native-file ( fid -- )
    dup >r lsids $[]slurp
    r> close-file throw ;

: locale-file ( fid -- )
    dup >r locale $[]slurp
    r> close-file throw
    locale $[]# 0 DO
	I locale $[]@ nip 0= IF  I locale $[] $free  THEN
    LOOP ;

: included-locale ( addr u -- )  open-fpath-file throw 2drop locale-file ;
: included-native ( addr u -- )  open-fpath-file throw 2drop native-file ;

[defined] getpathspec 0= [IF]
    : getpathspec ( "name" -- fd )  parse-name open-fpath-file throw 2drop ;
[THEN]

: include-locale ( "name" -- )  getpathspec locale-file ;
: include-native ( "name" -- )  getpathspec native-file ;

\ easy use

: x" ( "string"<"> -- addr u )
    ['] l" execute locale@ ;
compsem: postpone l" postpone locale@ ;

l" FORTH" constant forth-lx
[defined] gforth   [IF] s" Gforth"    forth-lx locale! [THEN]
[defined] bigforth [IF] s" bigFORTH"  forth-lx locale! [THEN]
[defined] VFXforth [IF] s" VFX FORTH" forth-lx locale! [THEN]
