\ SwiftForth-like locate etc.

\ Copyright (C) 2016,2017 Free Software Foundation, Inc.

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

variable included-file-buffers
\ Bernd-array of c-addr u descriptors for read-only buffers that
\ contain the contents of the included files (same index as
\ included-files); filled on demand and cleared on session end.
:noname ( -- )
    included-file-buffers off defers 'image ; is 'image

: included-buffer ( u -- c-addr u2 )
    \ u is the index into included-files, c-addr u2 describes a buffer
    \ containing the content of the file, or 0 0, if the file cannot
    \ be read.
    dup *terminal*# = IF  drop s" "  EXIT  THEN \ special files
    >r r@ included-file-buffers $[] >r
    r@ $@ dup IF  rdrop rdrop  EXIT  THEN  2drop
    i' included-files $[]@ r@ ['] $slurp-file catch IF
	drop 2drop 0 0  r> $free rdrop  EXIT  THEN
    r> $@ rdrop ;

    \ >r included-file-buffers $@len r@ 2* cells u< if
    \ 	r@ 1+ 2* cells included-file-buffers $!len then
    \ included-file-buffers $@ drop r@ 2* cells + ( addr r:u )
    \ dup 2@ over 0= if ( addr c-addr3 u3 r:u )
    \ 	2drop r@ included-files $[]@ ['] slurp-file catch if
    \ 	    2drop 0 0 then
    \ 	2dup 4 pick 2! then
    \ rot r> 2drop ;

: view>buffer ( view -- c-addr u )
    view>filename# included-buffer ;

: set-bn-view ( -- )
    bn-view @ view>filename# located-top @ 0 encode-view bn-view ! ;

: locate-line {: c-addr1 u1 lineno -- c-addr2 u2 lineno+1 c-addr1 u3 :}
    \ c-addr1 u1 is the rest of the file, c-addr1 u3 the line, and
    \ c-addr2 u2 the rest of the file without the line
    u1 0 u+do
	c-addr1 u1 i /string s\" \r\l" string-prefix? if
	    c-addr1 u1 i 2 + /string lineno 1+ c-addr1 i unloop exit then
	c-addr1 i + c@ dup #lf = swap #cr = or if
	    c-addr1 u1 i 1 + /string lineno 1+ c-addr1 i unloop exit then
    loop
    c-addr1 u1 + 0 lineno 1+ c-addr1 u1 ;

: locate-next-line ( c-addr1 u1 lineno -- c-addr2 u2 lineno+1 )
    locate-line 2drop ;

: type-prefix ( c-addr1 u1 u -- c-addr2 u2 )
    \ type the u-len prefix of c-addr1 u1, c-addr2 u2 is the rest
    >r 2dup r> umin tuck type safe/string ;

: locate-type ( c-addr u lineno -- )
    cr located-view @ view>line = if
	warn-color attr! located-view @ view>char type-prefix
	err-color  attr! located-len @            type-prefix
	warn-color attr! type
	default-color attr! exit
    then
    type ;

: locate-print-line ( c-addr1 u1 lineno -- c-addr2 u2 lineno+1 )
    dup >r locate-line r> locate-type ;


: located-buffer ( -- c-addr u )
    located-view @ view>buffer ;

: l1 ( -- )
    located-buffer 1 case ( c-addr u lineno1 )
	over 0= ?of endof
	dup located-bottom @ >= ?of endof
	dup located-top @ >= ?of locate-print-line contof
	locate-next-line
    next-case
    2drop drop ;

: view>filename ( view -- c-addr u )
    \G filename of view (obtained by @code{name>view})
    view>filename# loadfilename#>str ;

: l ( -- )
    \g Display line of source after compiler error or locate
    cr located-view @ view>filename type  ': emit
    located-top @ dec.
    l1 ;

: name-set-located-view ( nt -- )
    dup name>view @ swap name>string nip set-located-view ;

: locate-name ( nt -- )
     name-set-located-view l ;

: locate ( "name" -- )
    (') locate-name ;

: n ( -- )
    \g Display next lines after locate or error
    located-bottom @ dup located-top ! form drop 2/ + located-bottom !
    set-bn-view l1 ;

: b ( -- )
    \g Display previous lines after locate.
    located-top @ dup located-bottom ! form drop 2/ - 0 max located-top !
    set-bn-view l ;

: extern-g ( -- )
    \g Enter the external editor at the place of the latest error,
    \g @code{locate}, @code{n} or @code{b}.
    bn-view @ ['] editor-cmd >string-execute 2dup system drop free
    throw ;

Defer g ' extern-g is g
    \g Enter the editor at the place of the latest error, @code{locate},
    \g @code{n} or @code{b}.

: edit ( "name" -- )
    \g Enter the editor at the place of "name"
    (') name-set-located-view g ;


\ backtrace locate stuff:

\ an alternative implementation of much of this stuff is elsewhere.
\ The following implementation works for code in sections, too, but
\ currently does not survive SAVESYSTEM.
0 [if]
256 1024 * constant bl-data-size

0
2field:  bl-bounds
field:   bl-next
bl-data-size cell+ +field bl-data
constant bl-size

variable code-locations 0 code-locations !

: .bl {: bl -- :}
    cr bl bl-bounds 2@ swap 16 hex.r 17 hex.r
    bl bl-data 17 hex.r
    bl bl-next @ 17 hex.r ;

: .bls ( -- )
    cr ."       code-start         code-end          bl-data          bl-next"
    code-locations @ begin
	dup while
	    dup .bl
	    bl-next @ repeat
    drop ;

: addr>view ( addr -- view|0 )
    code-locations @ begin ( addr bl )
	dup while
	    2dup bl-bounds 2@ within if
		tuck bl-bounds 2@ drop  - + bl-data @ exit then
	    bl-next @ repeat
    2drop 0 ;

: xt-location2 ( addr bl -- addr )
    \ knowing that addr is within bl, record the current source
    \ position for addr
    2dup bl-bounds 2@ drop - + bl-data ( addr addr' )
    current-sourceview swap 2dup ! cell+ ! ;

: new-bl ( addr blp -- )
    bl-size allocate throw >r
    swap dup bl-data-size + r@ bl-bounds 2!
    dup @ r@ bl-next !
    r@ bl-data bl-data-size cell+ erase
    r> swap ! ;
    
: xt-location1 ( addr -- addr )
    code-locations begin ( addr blp )
	dup @ 0= if
	    2dup new-bl then
	@ 2dup bl-bounds 2@ within 0= while ( addr bl )
	    bl-next repeat
    xt-location2 ;

' xt-location1 is xt-location
[then]

: bt-location ( u -- f )
    \ locate-setup backtrace entry with index u; returns true iff successful
    cells >r stored-backtrace $@ r@ u> if ( addr1 r: offset )
	r> + @ cell- addr>view dup if ( view )
	    1 set-located-view true exit then
    else
	rdrop then
    drop ." no location for this backtrace index" false ;

: lb ( u -- )
    bt-location if
	l then ;

\ where

: unbounds ( c-start c-end -- c-start u )
    over - ;
    
: .wheretype ( c-addr u view -- )
    view>char >r -trailing over r> + {: c-pos :} 2dup + {: c-lineend :}
    (parse-white) drop ( c-addr1 )
    warn-color attr! c-pos unbounds type
    err-color  attr! c-pos c-lineend unbounds (parse-white) tuck type
    warn-color attr! c-pos + c-lineend unbounds type
    default-color attr! ;
    
: .whereline {: view u -- :}
    \ print the part of the source line around view that fits in the
    \ current line, of which u characters have already been used
    view view>buffer
    1 case ( c-addr u lineno1 )
	over 0= ?of endof
	dup view view>line = ?of locate-line view .wheretype endof
	locate-next-line
    next-case
    drop 2drop ;

: .whereview ( view -- )
    dup .sourceview-width ": " type 2 + .whereline ;

: forwheres ( ... xt -- ... )
    { xt } wheres $@ bounds u+do
	i where-nt @ xt execute if
	    i where-loc @ cr .whereview
	then
    where-struct +loop ;

: where ( "name" -- )
    parse-name find-name [: over = ;] forwheres drop ;

\ count word usage

: usage# ( nt -- n )
    \G count usage of the word @var{nt}
    0 wheres $@ bounds U+DO
	over i where-nt @ = -
    where-struct +LOOP  nip ;

\ display unused words

lcount-mask 1+ Constant unused-mask

: .wids ( nt1 .. ntn n ) cr 0 swap 0 ?DO swap .word LOOP drop ;
: +unused ( nt -- )
    >f+c unused-mask over @ or swap ! ;
: -unused ( nt -- )
    >f+c unused-mask invert over @ and swap ! ;
: unused-all ( wid -- )
    [: +unused true ;] swap traverse-wordlist ;
: unmark-used ( -- )
    wheres $@ bounds U+DO
	i where-nt @ dup forthstart here within
	IF  -unused  ELSE  drop  THEN
    where-struct +LOOP ;
: unused@ ( wid -- nt1 .. ntn n )
    0 [: dup >f+c @ unused-mask and IF
	    dup -unused swap 1+
	ELSE  drop  THEN  true ;]
    rot traverse-wordlist ;
: unused-wordlist ( wid -- )
    dup unused-all unmark-used unused@ .wids ;
: unused-words ( -- )
    \G list all words without usage
    context @ unused-wordlist ;

