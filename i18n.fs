\ Internationalization and localization

\ Authors: Bernd Paysan, Anton Ertl
\ Copyright (C) 2015,2018,2019,2020,2021,2022,2023,2025 Free Software Foundation, Inc.

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
require csv.fs

\ LSIDs

cs-vocabulary locales \ locales go in here

0 Value locale
in locales $[]Variable program 0 , ( -- ) \ gforth-experimental
\G @word{locales:program} activates the locale for which @word{locale@}
\G produces the string used for identifying the lsid (i.e., the string
\G parsed by @word{l"}).  This locale is useful for seeing which lsid
\G is used in which context.
DOES> to locale ;
`locales:program constant lsids
: lsid# ( -- n )
    lsids $[]# 1- ;

: native@ ( lsid -- c-addr u ) \ gforth-experimental native-fetch
    \G @i{c-addr u} is the @word{l"} string for @i{lsid} (i.e., the
    \G text-interpretation argument of @word{l"}).
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

: L" ( Interpretation "string<">" -- lsid; Compilation "string<">" -- ) \ gforth-experimental l-quote
    \G At text interpretation time, parse @i{string}.  At run-time,
    \G push the @i{lsid} associated with @i{string}.  Each string has
    \G a unique lsid.  If no lsid for the string exists yet, a new one
    \G is created.  If an lsid for the string exists already, that
    \G lsid is returned.  This means that one can refer to and use the
    \G same lsid with @word{l"} in different locations in the source
    \G code.  If you need to make your string unique, append " [specifier]"
    \G to it, e.g. @code{L" bank [finance]"} or @code{L" bank [geography]"}.
    '"' parse ?new-lsid ;
compsem: '"' parse postpone LLiteral ;

: .lsids ( locale -- ) \ gforth-experimental dot-lsids
    \G print the string for all lsids
    $[]. ;

\ locale@ stuff

`locales:program in locales create-from default ( -- ) \ gforth-experimental
reveal here $[]saved 0 , lsids ,
\G @word{locales:default} is the default locale if the user has not set
\G one.  Most lsids don't have a specific default string, so fallback
\G to the @word{program} locale happens.  But if you have a program
\G string that is inappropriate for end-user usage (in particular, if
\G the program string contains an extra specifier), you will prefer to
\G define an appropriate string in the default locale.
locales:default

: Locale: ( "name" -- ) \ gforth-experimental
    \G Defines a new locale @i{l} with name @i{name}.@* @i{name}
    \G execution: ( -- ) @i{l} becomes the current locale.
    [: ['] locales >wordlist set-current `locales:program create-from reveal ;]
    current-execute here $[]saved 0 ,
    latest name>string '_' -scan dup IF
	['] locales >wordlist find-name-in
	?dup-IF name>interpret >body ELSE `locales:default THEN
    ELSE 2drop `locales:default THEN ,
  DOES> to locale ;

: locale@ ( lsid -- c-addr u ) \ gforth-experimental locale-fetch
    \G @i{c-addr u} is the localized string for @i{lsid} in the
    \G current locale.  If no localized string is found in the current
    \G locale with a name of the form @code{@i{X}_@i{Y}}, @i{lsid} is
    \G looked up in locale @code{@i{X}}.  If no localized string is
    \G found in the locale @code{@i{X}}, @i{lsid} is looked up in the
    \G locale @word{default}.  If no localized string is found in the
    \G locale @word{default}, @i{lsid} is looked up in the locale
    \G @word{default} (i.e., @i{c-addr u} is the text-interpretation
    \G argument of @word{l"}).
    locale
    BEGIN 2dup $[]@ 2dup d0= WHILE
	2drop cell+ @ dup 0= UNTIL 0 0 THEN
    2nip ;

: locale! ( addr u lsid -- ) \ gforth-experimental locale-store
    \G After executing @word{locale!}, the localized string for
    \G @i{lsid} in the current locale is @i{c-addr u}.
    locale $[]! ;

: set-locale ( addr u -- ) \ gforth-experimental
    \G sets the locale, by searching it in the @code{LOCALES} vocabulary.
    \G If the variant is not available, falls back to the language.
    locales:default
    BEGIN  2dup ['] locales >wordlist find-name-in
	?dup-IF  execute 2drop EXIT  THEN
    '_' -scan dup 0= UNTIL  2drop ;

\ CSV reader part

Variable lang[] \ array 

: define-locale ( c-addr u -- xt ) \ gforth-experimental
    \G Define a locale named @var{addr u} and return its @var{xt}.
    \G A locale is defined by @code{language} if it doesn't contain a '_',
    \G if it does, it is defined by @code{country} referring to the
    \G language before the '_', if that exists.
    2dup `locales >wordlist find-name-in dup if
        nip nip name>interpret
    else
        drop nextname locale: latestxt
    then ;

0 Value csv-lsid

: insert-locale ( addr u col line -- ) \ gforth-experimental
    \G insert a locale entry @var{addr u} from a table in column
    \G @var{col} and line @var{line}.  Line 1 is special, it contains
    \G the name of the corresponding locale.
    1- over 0= IF  dup to csv-lsid  THEN  drop
    csv-lsid dup >r -1 = IF  rdrop 2drop  EXIT  THEN
    r@ 0= IF
        rdrop >r define-locale
	r> lang[] $[] ! EXIT  THEN
    lang[] $[] @ 2dup swap 0<> and IF
	dup lsids = IF
	    rdrop drop lsid# >r 2dup ?new-lsid to csv-lsid lsid# r> <>
	    [: '"' emit 2dup type '"' emit ."  native string not found" ;]
	    ?warning  2drop
	ELSE
	    r> swap $[]!
	THEN
    ELSE  rdrop drop 2drop  THEN ;

: locale-csv ( "name" -- ) \ gforth-experimental locale-csv
    \G import comma-separated value table into locales.  first line contains
    \G locale names, “program” and “default” are special entries; generic
    \G languages must preceed translations for specific countries.  Entries
    \G under “program” (must be leftmost) are used to search for the lsid; if
    \G empty, the line number-1 is the lsid index.
    lang[] $free ?parse-name ['] insert-locale read-csv ;

: .locale-csv ( -- ) \ gforth-experimental dot-locale-csv
    \G write the locale database in CSV format to the terminal output.
    words[] $free [ ' locales >wordlist ]L wid>words[]    
    false words[] $@ bounds cell- swap cell- U-DO
	IF  csv-separator  emit  THEN
	I @ name>string .quoted-csv
	true
    cell -LOOP  drop cr
    lsid# 1+ 1 U+DO
	false words[] $@ bounds cell- swap cell- U-DO
	    IF  csv-separator  emit  THEN
	    J I @ name>interpret $[]@ .quoted-csv
	    true
	cell -LOOP  drop cr
    LOOP ;

: locale-csv-out ( "name" -- ) \ gforth-experimental locale-csv-out
    \G Create file @var{"name"} and write the locale database out to the file
    \G @var{"name"} in CSV format.
    ?parse-name r/w create-file throw >r
    ['] .locale-csv r@ ['] outfile-execute catch
    r> close-file swap throw throw ;

\ easy use

: x" ( "string"<"> -- addr u )
    ['] l" execute locale@ ;
compsem: postpone l" postpone locale@ ;

l" FORTH" constant forth-lx
[defined] gforth   [IF] s" Gforth"    forth-lx locale! [THEN]
[defined] bigforth [IF] s" bigFORTH"  forth-lx locale! [THEN]
[defined] VFXforth [IF] s" VFX FORTH" forth-lx locale! [THEN]
