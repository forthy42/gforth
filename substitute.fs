\ substitute stuff

\ Authors: Bernd Paysan, Anton Ertl
\ Copyright (C) 2012,2015,2016,2022,2024,2025 Free Software Foundation, Inc.

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

require string.fs

cs-wordlist AConstant macros-wordlist ( -- wid ) \ gforth-experimental
\G wordlist for string replacement macros

[IFUNDEF] $Value
    [IFUNDEF] $!-table
	to-table: $!-table  $! $+! [noop]
    [THEN]
    ' >body $!-table to-class: $value-to
    
    : $Value ( addr u -- )
	Create here dup $saved 0 , $!
	['] $@ set-does> ['] $value-to set-to ;
[THEN]

synonym macro: $value

: replaces ( addr1 len1 addr2 len2 -- ) \ string-ext
    \G create a macro with name @var{addr2 len2} and content @var{addr1 len1}.
    \G If the macro already exists, just change the content.
    2dup macros-wordlist find-name-in
    ?dup-IF
	nip nip value!
    ELSE
	[: macros-wordlist set-current nextname macro: ;] current-execute
    THEN ;

: warn-hardcoded ( addr u xt1 xt2 -- )
    true [: id. ." is a hard-coded macro" cr ;] ?warning  2drop drop ;

: replacer: ( "name" -- ) \ gforth-experimental
    \G Start a colon definition @i{name} in @code{macros-wordlist},
    \G i.e. this colon definition is a macro.  It must have the stack
    \G effect @var{( -- addr u )}.
    get-current >r macros-wordlist set-current
    : ['] warn-hardcoded set-to
    r> set-current ;

replacer: rd ( -- addr u ) sourcefilename extractpath ;

: .% ( -- ) '%' emit ;
: .substitute ( addr1 len1 -- n / ior ) \ gforth-experimental dot-substitute
    \G substitute all macros in text @var{addr1 len1} and print the
    \G result.  @var{n} is the number of substitutions or, if
    \G negative, a throwable @var{ior}.
    0 >r
    BEGIN  dup  WHILE  '%' $split
	    2swap type dup IF
		over c@ '%' = IF
		    .% 1 /string
		ELSE
		    '%' $split 2swap dup 0= IF
			2drop .%
		    ELSE
			2over drop 1- c@ '%' = IF
			    2dup macros-wordlist search-wordlist IF
				nip nip -rot
				2>r execute type 2r> r> 1+ >r
			    ELSE
				.% type .%
			    THEN
			ELSE
			    .% type
			THEN
		    THEN
		THEN
	    ELSE
		over 1- c@ '%' = IF  .%  THEN
	    THEN
    REPEAT 2drop r> ;

: $substitute ( addr1 len1 -- addr2 len2 n/ior ) \ gforth-experimental string-substitute
    \G substitute all macros in text @var{addr1 len1}.  @var{n} is the
    \G number of substitutions, if negative, it's a throwable @var{ior},
    \G @var{addr2 len2} the result.
    ['] .substitute $tmp rot ;

: substitute ( addr1 len1 addr2 len2 -- addr2 len3 n/ior ) \ string-ext
    \G substitute all macros in text @var{addr1 len1}, and copy the
    \G result to @var{addr2 len2}.  @var{n} is the number of
    \G substitutions or, if negative, a throwable @var{ior},
    \G @var{addr2 len3} the result.
    2>r $substitute over r@ u<= -78 swap select -rot
    2r> rot umin 2dup 2>r move 2r> rot -1 tmp$# +!  ;

: unescape ( addr1 u1 dest -- dest u2 ) \ string-ext
    \G double all delimiters in @var{addr1 u1}, so that substitute
    \G will result in the original text.  Note that the buffer
    \G @var{dest} does not have a size, as in worst case, it will need
    \G just twice as many characters as @var{u1}. @var{dest u2} is the
    \G resulting string.
    dp @ >r dup >r dp !
    bounds ?DO
	I c@ dup '%' = IF  dup c,  THEN  c,
    LOOP  r> here over -  r> dp ! ;

: $unescape ( addr1 u1 -- addr2 u2 ) \ gforth-experimental string-unescape
    \G same as @code{unescape}, but creates a temporary destination string with
    \G @code{$tmp}.
    [: bounds ?DO  I c@ dup emit '%' = IF '%' emit  THEN  LOOP ;] $tmp ;

\ file name replacements in include and require

: subst>filename ['] .substitute $tmp rot 0 min throw ;
' subst>filename is >include
