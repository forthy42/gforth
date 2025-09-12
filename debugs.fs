\ Simple debugging aids

\ Authors: Bernd Paysan, Anton Ertl, Gerald Wodni, Neal Crook
\ Copyright (C) 1995,1997,1999,2002,2003,2004,2005,2006,2007,2009,2011,2012,2013,2014,2016,2017,2018,2019,2020,2021,2022,2023,2024 Free Software Foundation, Inc.

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


\ They are meant to support a different style of debugging than the
\ tracing/stepping debuggers used in languages with long turn-around
\ times.

\ IMO, a much better (faster) way in fast-compilig languages is to add
\ printing code at well-selected places, let the program run, look at
\ the output, see where things went wrong, add more printing code, etc.,
\ until the bug is found.

\ We support fast insertion and removal of the printing code.

\ !!Warning: the default debugging actions will destroy the contents
\ of the pictured numeric output string (i.e., don't use ~~ between <#
\ and #>).

require source.fs

defer printdebugdata ( -- ) \ gforth print-debug-data
' .s is printdebugdata

defer .debugline ( nfile nline -- ) \ gforth print-debug-line
\G Print the source code location indicated by @var{nfile nline}, and
\G additional debugging information; the default @code{.debugline}
\G prints the additional information with @code{printdebugdata}.

: (.debugline) ( view -- )
    info-color
    cr .sourceview ." :"
    \ it would be nice to print the name of the following word,
    \ but that's not easily possible for primitives
    printdebugdata
    cr default-color ;

[IFUNDEF] debug-fid
stderr value debug-fid ( -- fid )
\G (value) Debugging output prints to this file
[THEN]

' (.debugline) IS .debugline

: .debugline-directed ( view -- )
    ['] .debugline do-debug ;

:noname ( -- )
    current-sourceview .debugline-directed ;
:noname ( compilation  -- ; run-time  -- )
    compile-sourcepos POSTPONE .debugline-directed ;
interpret/compile: ~~ ( -- ) \ gforth tilde-tilde
\G Prints the source code location of the @code{~~} and the stack
\G contents with @code{.debugline}.

:is 'cold ( -- )  stderr to debug-fid  defers 'cold ;

\ code coverage helpers that are always present

0 Value coverage? ( -- f ) \ gforth-internal
\G Value: Coverage check on/off
$10 stack: cov-stack

: nocov[ ( -- ) \ gforth nocov-bracket
    \G (Immediate) Turn coverage off temporarily.
    coverage? cov-stack >stack  false to coverage? ; immediate
: ]nocov ( -- ) \ gforth bracket-nocov
    \G (Immediate) End of temporary turned off coverage.
    cov-stack stack> to coverage? ; immediate

\ print a no-overhead backtrace

: once ( -- ) \ gforth
    \G do the following up to THEN only once
    here cell+ >r ]] true if [[ r> ]] Literal off [[ ;
    immediate compile-only
    
: ~~bt ( -- ) \ gforth
    \G print stackdump and backtrace
    ]] ~~ store-backtrace dobacktrace nothrow [[ ;
    immediate compile-only

: ~~1bt ( -- ) \ gforth
    \G print stackdump and backtrace once
    ]] once ~~bt then [[ ; immediate compile-only

\ launch a debug shell, quit with emtpy line

?: bt-rp0-catch ( ... xt -- ... ball )
    backtrace-rp0 @ >r	
    catch
    r> backtrace-rp0 ! ;

?: bt-rp0-wrapper ( ... xt -- ... )
    bt-rp0-catch throw ;

: ???-loop ( ... -- ... )
    BEGIN
        ." dbg> " refill  WHILE
            source nip WHILE
                interpret ."  ok" cr
        REPEAT  THEN ;

: ??? ( -- ) \ gforth
    \G Open a debuging shell
    create-input cr
    ['] ???-loop bt-rp0-catch throw
    0 pop-file drop ;
' ??? alias dbg-shell

: WTF?? ( -- ) \ gforth
    \G Open a debugging shell with backtrace and stack dump
    ]] ~~bt ??? [[ ; immediate compile-only

\ special exception for places that should never be reached

s" You've reached a !!FIXME!! marker" exception constant FIXME#

: !!FIXME!! ( -- ) \ gforth
    \G word that should never be reached
    FIXME# throw ;

\ warn beginners that double numbers clash with floating points

:is ?warning ( f xt -- )
    \ if f, output a warning by EXECUTEing xt
    swap warnings @ 0<> and if
	[: cr warning-color current-view .sourceview ." : warning: " execute
	default-color ;] do-debug
	warnings @ abs 4 >= warning-error and throw
	exit then
    drop ;

: shadow-warning ( c-addr u nt -- c-addr u nt )
    dup >r name>string ." redefined " 2dup type ( c-addr u c-addr2 u2 )
    2over str= 0= if
	."  with " 2dup type then
    cr r@ name>view .sourceview ." : warning: original location"
    r> ;
: shadow-num-warning ( c-addr u -- c-addr u )
    ." defined literal " 2dup type ."  as word" ;

10 stack: warning-recs
' rec-float ' rec-num 2 warning-recs set-stack

' check-shadow >code-address dodefer: = [if]
:noname  ( addr count wid -- )
    \ prints a warning if the string is already present in the wordlist
    \ don't check 0-length names (as in noname-w:)
    over 0= warnings @ 0= or IF  drop 2drop  EXIT  THEN
    >r 2dup r> find-name-in dup 0<>
    ['] shadow-warning ?warning IF  2drop  EXIT  THEN
    warnings @ >r warnings off
    sp@ fp@ 2>r 2dup warning-recs recognize 2r> rot >r
    fp! sp! r> 0<>  r> dup warnings ! 0<> and
    ['] shadow-num-warning ?warning
    s" xlerb" str= warning" 'xlerb' shall remain undefined"
; is check-shadow
[then]

\ in pedantic mode, warn if locals overshade existing words
:is locals-warning  warnings @ abs 3 >= IF
	sp@ >r latestnt name>string 2dup search-order
	['] shadow-warning ?warning r> sp!
    THEN ;

: ?warn-dp ( -- )
    >num-state @ >num-state off 1 and 0= dpl @ 0>= and warnings @ abs 1 > and
    [: '' emit input-lexeme 2@ type
	." ' is a double-cell integer; type `help' for more info" ;] ?warning
    warnings @ abs 2 > input-lexeme 2@ '. scan nip 1 > and dpl @ 0>= and
    [: '' emit input-lexeme 2@ type
	." ' is a non-standard double: only trailing '.' standard" ;] ?warning ;

' ?warn-dp is ?warn#

\ eof warning

:is eof-warning ( -- )
    state @ [: ." EOF reached while " get-state id. ;] ?warning ;

\ replacing one word with another

: >colon-body ( xt -- addr )
    dup >code-address docol: <> -12 and throw >body ;

: >prim-code ( xt -- x )
    \ converts xt of a primitive into a form usable in the code of
    \ colon definitions on the current engine
    threading-method 0= IF >code-address THEN ;

: replace-word ( xt1 xt2 -- ) \ gforth
  \G make xt2 do xt1, both need to be colon definitions
    swap >colon-body ['] branch >prim-code rot >colon-body 2! ;

\ watching variables and values

: watch-does> ( -- ) DOES> dup @ ~~ drop ;
: watch-opt: ( xt -- ) opt: >body ]] Literal dup @ ~~ drop [[ ; 
: ~~Variable ( "name" -- ) \ gforth
    \G Variable that will be watched on every access
  Create 0 , watch-does> watch-opt: ;

: ~~>body ( addr -- body ) ~~ ;
fold1: lit, ]] ~~ [[ ;
' ~~>body !-table to-class: ~~value-to

: ~~Value ( n "name" -- ) \ gforth
    \G Value that will be watched on every access
    Value ['] ~~value-to set-to ;

\ trace lines

: line-tracer ( -- )
    \ print source position and stack on every source line start
    ['] ~~ execute ;
: +ltrace ( -- ) \ gforth
    \G turn on line tracing
    ['] line-tracer is before-line ;
: -ltrace ( -- ) \ gforth
    \G turn off line tracing
    ['] noop is before-line ;

\ locate factors

: esc'type ( addr u -- )
    bounds ?DO
	I c@ ''' = IF  ''' emit '"' emit ''' emit '"' emit ''' emit
	ELSE  I c@ emit  THEN  LOOP ;

: type-prefix ( c-addr1 u1 u -- c-addr2 u2 )
    \ type the u-len prefix of c-addr1 u1, c-addr2 u2 is the rest
    >r 2dup r> umin tuck type /string ;

\ locate/view of translate tokens show the recognizer, if not a word
\ Idea: Jenny Brian

Variable rec'[]

: rec'@ ( -- xt )
    0 rec'[] $[] @
    rec'[] $[]# 1 U+DO
	dup >does-code ['] recognize =
	IF  drop  I rec'[] $[] @  THEN
    LOOP ;

: (view') ( addr u -- xt )
    rec'[] $free  action-of trace-recognizer >r
    sp@ 2 cells + fp@ 2>r  name-too-short?
    [: rec-level @ rec'[] $[] ! ;] is trace-recognizer
    rec-forth ?found
    dup translate-name? IF  drop rec'[] $free
    ELSE
	dup translate-cell = IF
	    drop dup xt? 0= IF  drop rec'@  THEN
	ELSE  drop  rec'@  THEN
    THEN
    2r> rot >r fp! sp! r>  r> is trace-recognizer ;

: view' ( "name" -- xt ) \ gforth-internal
    \G @var{xt} is either the word to view if it is a word
    \G or the recognizer that successfully parsed @var{"name"}
    parse-name (view') ;

:is 'image  defers 'image rec'[] $free ;

: kate-l:c ( line pos -- )
    swap ." -l " . ." -c " . ;
: emacs-l:c ( line pos -- )
    ." +" swap 0 .r ." :" . ;
: vi-l:c ( line pos -- )  ." +" drop . ;

: editor-cmd ( sourceview -- ) \ gforth-internal
    \G tell the editor to go to the source of a word
    \G uses $EDITOR, and adjusts goto line command depending
    \G on vi- (default), kate-, or emacs-style
    \G @example
    \G EDITOR=emacsclient      #if you like emacs, M-x server-start in emacs
    \G EDITOR=vi|vim|gvim      #if you like vi variants
    \G EDITOR=kate             #if you like kate
    \G EDITOR=gedit            #if you like gedit
    \G EDITOR=joe|mcedit|nano  #if you like other simple editors
    \G @end example
    s" EDITOR" getenv dup 0= IF
	2drop s" vi" \ if you don't set EDITOR, use vi as default
    THEN
    2dup 2>r type space
    2r@ s" emacsclient" string-prefix? IF  ." -n "  THEN
    decode-view 1+
    2r@ s" emacs" search nip nip  2r@ s" gedit" str= or  IF  emacs-l:c  ELSE
	2r@ s" kate" string-prefix? IF  kate-l:c  ELSE
	    vi-l:c  \ also works for joe, mcedit, nano, and is de facto standard
	THEN
    THEN
    ''' emit loadfilename#>str esc'type ''' emit  2rdrop ;


: edit-file-cmd ( c-addr u -- )
    \ prints the editor command for editing the file with the name c-addr u
    s" EDITOR" getenv dup 0= IF
	2drop s" vi" \ if you don't set EDITOR, use vi as default
    THEN
    2dup 2>r type space
    2r@ s" emacsclient" string-prefix? IF  ." -n "  THEN
    ''' emit esc'type ''' emit  2rdrop ;

\ insert a different location

: #loc ( nline nchar "file" -- ) \ gforth
    \G set next word's location to @var{nline nchar} in @var{"file"}
    parse-name 2dup str>loadfilename# dup 0< IF
	drop add-included-file included-files $[]# 1-
    ELSE  nip nip  THEN
    -rot encode-view to replace-sourceview ;

#22 Value rstack-offset \ different prompt words can have different offsets

: prompt-ok ( -- )
    ."  ok"
    depth ?dup-if
        space 0 dec.r then
    fdepth ?dup-if
        ."  f:" 0 dec.r then
    rp0 @ rp@ - cell/ rstack-offset - ?dup-if
        ."  r:" 0 dec.r then ;

: prompt-text ( -- )
    case  get-state ['] interpreting of  prompt-ok  endof  id. 0 endcase ;

: color-prompt ( -- )
    success-color prompt-text default-color ;

' color-prompt is prompt

\ print name vtable

: .name? ( xt -- )
    \ prints name of xt if named, otherwise prints xt as hex number
    dup >name if
	id.
    else
	h.
    then ;

: .hm ( nt -- ) \ gforth dot-h-m
    \G print the header methods of @i{nt}
    >namehm @ cr
    ." opt:     " dup >hmcompile, @ .name? cr
    ." to:      " dup >hmto       @ .name? cr
    ." extra:   " dup >hmextra    @ .name? cr
    ." >int:    " dup >hm>int     @ .name? cr
    ." >comp:   " dup >hm>comp    @ .name? cr
    ." >string: " dup >hm>string  @ .name? cr
    ." >link:   "     >hm>link    @ .name? ;

\ warn on compiling into space outside colon definitions

[IFUNDEF] in-colon-def?
    0 Value in-colon-def? ( -- flag ) \ gforth-experimental
    \G allows to check if there currently is an active colon
    \G definition where you can append code to.
[THEN]

:is wrap! defers wrap!   true  to in-colon-def? ;
:is :-hook defers :-hook  true  to in-colon-def? ;
:is ;-hook2 defers ;-hook2 false to in-colon-def? ;
:is reset-dpp defers reset-dpp false to in-colon-def? ;
: level-check defers prim-check
    in-colon-def? 0= warning" Compiling outside a definition" ;
' level-check is prim-check
