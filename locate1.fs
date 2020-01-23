\ SwiftForth-like locate etc.

\ Authors: Anton Ertl, Bernd Paysan, Gerald Wodni
\ Copyright (C) 2016,2017,2018,2019 Free Software Foundation, Inc.

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

$variable where-results
\ addresses in WHERES that contain the results of the last WHERE
variable where-index -1 where-index !
variable backtrace-index -1 backtrace-index !

-1 0 set-located-view

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
    dup *terminal*# = IF  drop 0 0  EXIT  THEN \ special files
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
	info-color  attr! located-view @ view>char type-prefix
	error-color attr! located-len @            type-prefix
	info-color  attr! type
	default-color attr! exit
    then
    type ;

: locate-print-line ( c-addr1 u1 lineno -- c-addr2 u2 lineno+1 )
    dup >r locate-line r> locate-type ;

: located-buffer ( -- c-addr u )
    located-view @ view>buffer ;

: current-location?1 ( -- f )
    located-view @ -1 = if
        true [: ." no current location" ;] ?warning true exit then
    false ;

: current-location? ( -- )
    ]] current-location?1 ?exit [[ ; immediate

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
    \g Display source code lines at the current location.
    current-location?
    cr located-view @ view>filename type ': emit
    located-top @ dec.
    l1 ;

: name-set-located-view ( nt -- )
    dup name>view swap name>string nip set-located-view ;

: locate-name ( nt -- )
    name-set-located-view l ;

: locate ( "name" -- ) \ gforth
    \g Show the source code of the word @i{name} and set the current
    \g location there.
    (') locate-name ;

' locate alias view ( "name" -- ) \ gforth

: n ( -- ) \ gforth
    \g Display lines behind the current location, or behind the last
    \g @code{n} or @code{b} output (whichever was later).
    current-location?
    located-bottom @ dup located-top ! form drop 2/ + located-bottom !
    set-bn-view l1 ;

: b ( -- ) \ gforth
    \g Display lines before the current location, or before the last
    \g @code{n} or @code{b} output (whichever was later).
    current-location?
    located-top @ dup located-bottom ! form drop 2/ - 0 max located-top !
    set-bn-view l1 ;

: extern-g ( -- )
    \g Enter the external editor at the place of the latest error,
    \g @code{locate}, @code{n} or @code{b}.
    current-location?
    bn-view @ ['] editor-cmd >string-execute 2dup system drop free
    throw ;

Defer g ( -- ) \ gforth
    \g Enter the editor at the current location, or at the start of
    \g the last @code{n} or @code{b} output (whichever was later).
' extern-g is g

: edit ( "name" -- ) \ gforth
    \g Enter the editor at the location of "name"
    (') name-set-located-view g ;


\ avoid needing separate edit and locate words for the rest

defer l|g ( -- )
\ either do l or g, and then possibly change l|g

variable next-l|g ( -- addr )

: l-once ( -- )
    l next-l|g @ is l|g ;

: ll ( -- ) \ gforth
    \g The next @code{ww}, @code{nw}, @code{bw}, @code{bb}, @code{nb},
    \g @code{lb} (but not @code{locate}, @code{edit}, @code{l} or
    \g @code{g}) displays in the Forth system (like @code{l}).  Use
    \g @code{ll ll} to make this permanent rather than one-shot.
    `l|g defer@ next-l|g !
    `l-once is l|g ;

: g-once ( -- )
    g next-l|g @ is l|g ;

: gg ( -- ) \ gforth
    \g The next @code{ww}, @code{nw}, @code{bw}, @code{bb}, @code{nb},
    \g @code{lb} (but not @code{locate}, @code{edit}, @code{l} or
    \g @code{g}) puts it result in the editor (like @code{g}).  Use
    \g @code{gg gg} to make this permanent rather than one-shot.
    `l|g defer@ next-l|g !
    `g-once is l|g ;

ll ll \ set default to use L


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

: tt ( u -- ) \ gforth
    dup backtrace-index !
    bt-location if
        l|g
    else
        -1 backtrace-index ! then ;

: nt (  -- ) \ gforth
    backtrace-index @ 1+ tt ;

: bt ( -- ) \ gforth
    backtrace-index @ dup 0< if
        drop stored-backtrace $@ nip cell/ then
    1- tt ; 

\ where

: unbounds ( c-start c-end -- c-start u )
    over - 0 max ;

: type-notabs ( c-addr u -- )
    \G like type, but type a space for each tab
    bounds ?do
        i c@ dup #tab = if drop bl then emit loop ;

: width-type ( c-addr u uwidth -- uwidth1 )
    \g type the part of the string that fits in uwidth; uwidth1 is the
    \g remaining width; replaces tabs with spaces
    >r over + swap case ( end c-addr1 r: uwidth2 )
	2dup u<= ?of endof
	xc@+ dup #tab = if drop bl endif ( end c-addr1 xc r: uwidth2 )
	dup xc-width dup r@ u> ?of 2drop endof ( end c-addr1 xc u r: uwidth2 )
	r> swap - >r xemit next-case 
    2drop r> ;

: .wheretype1 ( c-addr u view urest -- )
    { urest } view>char >r -trailing over r> + { c-pos } 2dup + { c-lineend }
    (parse-white) drop ( c-addr1 )
    info-color  attr! c-pos unbounds urest width-type ->urest
    error-color attr! c-pos c-lineend unbounds (parse-white) tuck
    urest width-type ->urest
    info-color  attr! c-pos + c-lineend unbounds urest width-type ->urest
    default-color attr! urest spaces ;
    
: .whereline {: view u -- :}
    \ print the part of the source line around view that fits in the
    \ current line, of which u characters have already been used
    view view>buffer
    1 case ( c-addr u lineno1 )
	over 0= ?of endof
	dup view view>line = ?of locate-line view u .wheretype1 endof
	locate-next-line
    next-case
    drop 2drop ;

: .whereview1 ( view wno -- )
    0 <<# `#s #10 base-execute #> rot ( c-addr u view )
    dup .sourceview-width ." : " 3 + 2 pick + cols swap - .whereline type #>> ;

: forwheres ( ... xt -- ... )
    where-results $free
    0 { xt wno } wheres $@ bounds u+do
	i where-nt @ xt execute if
            i where-loc @ cr wno .whereview1
            i { w^ ip } ip cell where-results $+!
            wno 1+ ->wno
	then
    where-struct +loop ;

: where ( "name" -- ) \ gforth
    \g Show all places where @i{name} is used (text-interpreted).  You
    \g can then use @code{ww}, @code{nw} or @code{bw} to inspect
    \g specific occurences more closely.
    parse-name find-name dup 0= #-13 and throw [: over = ;] forwheres
    drop -1 where-index ! ;

: ww ( u -- ) \ gforth
    \G The next @code{l} or @code{g} shows the @code{where} result
    \G with index @i{u}
    dup where-index !
    where-results $@ rot cells tuck u<= if
        2drop -1 0 -1 where-index !
    else
        + @ 2@ name>string nip then
    set-located-view l|g ;

: nw ( -- ) \ gforth
    \G The next @code{l} or @code{g} shows the next @code{where}
    \G result; if the current one is the last one, after @code{nw}
    \G there is no current one.  If there is no current one, after
    \G @code{nw} the first one is the current one.
    where-index @ 1+ ww ;

: bw ( -- ) \ gforth
    \G The next @code{l} or @code{g} shows the previous @code{where}
    \G result; if the current one is the first one, after @code{bw}
    \G there is no current one.    If there is no current one, after
    \G @code{bw} the last one is the current one.
    where-index @ dup 0< if
        drop where-results $@ nip cell/ then
    1- ww ;

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

\ help

s" doc/gforth.txt" add-included-file

included-files $[]# 1- constant doc-file#

: count-lfs ( c-addr u -- u1 )
    0 -rot bounds ?do
        i c@ #lf = - loop ;

: help-word {: c-addr u -- :}
    doc-file# included-buffer {: c-addr1 u1 :} u1 if
        c-addr1 u1 c-addr u [: "\l'" type type "'    " type ;] $tmp
        capssearch if
            {: c-addr3 u3 :} c-addr1 u1 u3 - count-lfs 2 +
            doc-file# swap 1 encode-view u set-located-view l exit
        else
	    2drop c-addr u cr
	    [: ." No documentation for " type ;] error-color color-execute
	then
    else
        cr [: ." Documentation file not found" ;] error-color color-execute
    then
    [: ." , LOCATEing source" ;] info-color color-execute
    c-addr u find-name dup 0= -13 and throw locate-name ;

: help-section {: c-addr u -- :}
    ." help for section" c-addr u type ;

[ifdef] string-suffix?
: help ( "rest-of-line" -- ) \ gforth
    \G If no name is given, show basic help.  If a documentation node
    \G name is given followed by "::", show the start of the node.  If
    \G the name of a word is given, show the documentation of the word
    \G if it exists, or its source code if not.  Use @code{g} to enter
    \G the editor at the point shown by @code{help}.
    >in @ >r parse-name dup 0= if
        rdrop 2drop basic-help exit then
    drop 0 parse + over - -trailing 2dup s" ::" string-suffix? if
        rdrop help-section exit then
    r@ >in ! parse-name 2dup find-name if
        rdrop help-word 2drop exit then
    2drop r> >in ! 0 parse 2drop 2drop
    [: ." Not a section or word" ;] error-color color-execute ;
[then]

\ whereg

#24 #80 2Constant plain-form

' (type) ' (emit) ' (cr) ' plain-form output: plain-out
: plain-output ( xt -- )
    op-vector @ >r  plain-out  catch  r> op-vector !  throw ;

s" os-type" environment? [IF]
    s" linux-android" string-prefix? 0= [IF]

User sh$  cell uallot drop
: sh-get ( addr u -- addr' u' )
    \G open command addr u, and read in the result
    sh$ free-mem-var
    r/o open-pipe throw dup >r slurp-fid
    r> close-pipe throw to $? 2dup sh$ 2! ;

:noname '`' parse sh-get ;
:noname '`' parse postpone SLiteral postpone sh-get ;
interpret/compile: s` ( "eval-string" -- addr u )

2variable whereg-filename 0 0 whereg-filename 2!

: delete-whereg ( -- )
    \ delete whereg file
    whereg-filename 2@ dup if
	2dup delete-file throw drop free throw
    else  2drop  then ;

: whereg ( "name" -- ) \ gforth
    \g Like @code{where}, but puts the output in the editor.  In
    \g Emacs, you can then use the compilation-mode commands
    \g (@pxref{Compilation Mode,,,emacs,GNU Emacs Manual}) to inspect
    \g specific occurences more closely.
    delete-whereg
    s` mktemp /tmp/gforth-whereg-XXXXXX` 1- save-mem 2dup whereg-filename 2!
    2dup r/w open-file throw
    [:  "-*- mode: compilation; default-directory: \"" type
	s` pwd` 1- type
	"\" -*-" type
	['] where plain-output
    ;] over outfile-execute close-file throw
    `edit-file-cmd >string-execute 2dup system drop free throw ;

: bye delete-whereg bye ;

    [THEN]
[THEN]
