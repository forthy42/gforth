\ Simple debugging aids

\ Authors: Bernd Paysan, Anton Ertl, Gerald Wodni, Neal Crook
\ Copyright (C) 1995,1997,1999,2002,2003,2004,2005,2006,2007,2009,2011,2012,2013,2014,2016,2017,2018,2019 Free Software Foundation, Inc.

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
    info-color attr!
    cr .sourceview ." :"
    \ it would be nice to print the name of the following word,
    \ but that's not easily possible for primitives
    printdebugdata
    cr default-color attr! ;

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

:noname ( -- )  stderr to debug-fid  defers 'cold ; IS 'cold

\ code coverage helpers that are always present

0 Value coverage? ( -- f )
\G Value: Coverage check on/off
$10 stack: cov-stack

: nocov[ ( -- )
    \G turn coverage off temporarily
    coverage? cov-stack >stack  false to coverage? ; immediate
: ]nocov ( -- )
    \G end of temporary turned off coverage
    cov-stack stack> to coverage? ; immediate

\ print a no-overhead backtrace

: once ( -- )
    \G do the following up to THEN only once
    here cell+ >r ]] true if [[ r> ]] Literal off [[ ;
    immediate compile-only
    
: ~~bt ( -- )
    \G print stackdump and backtrace
    ]] ~~ store-backtrace dobacktrace nothrow [[ ;
    immediate compile-only

: ~~1bt ( -- )
    \G print stackdump and backtrace once
    ]] once ~~bt then [[ ; immediate compile-only

\ launch a debug shell, quit with emtpy line

[ifundef] bt-rp0-catch
: bt-rp0-catch ( ... xt -- ... ball )
    backtrace-rp0 @ >r	
    catch
    r> backtrace-rp0 ! ;

: bt-rp0-wrapper ( ... xt -- ... )
    bt-rp0-catch throw ;
[then]

: ???-loop ( ... -- ... )
    BEGIN
        ." dbg> " refill  WHILE
            source nip WHILE
                interpret ."  ok" cr
        REPEAT  THEN ;

: ??? ( -- )
    \G Open a debuging shell
    create-input cr
    ['] ???-loop bt-rp0-catch throw
    0 pop-file drop ;
' ??? alias dbg-shell

: WTF?? ( -- )
    \G Open a debugging shell with backtrace and stack dump
    ]] ~~bt ??? [[ ; immediate compile-only

\ special exception for places that should never be reached

s" You've reached a !!FIXME!! marker" exception constant FIXME#

: !!FIXME!! ( -- )  FIXME# throw ;
\G word that should never be reached

\ warn beginners that double numbers clash with floating points

[IFUNDEF] ?warning \ fix compilation problem
    Defer ?warning
[THEN]

:noname ( f xt -- )
    \ if f, output a warning by EXECUTEing xt
    swap warnings @ and if
	[: cr current-view .sourceview ." : warning: " execute
	;] warning-color ['] color-execute do-debug
	warnings @ abs 4 >= warning-error and throw
	exit then
    drop ;
is ?warning

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
    \G prints a warning if the string is already present in the wordlist
    warnings @ 0= IF  drop 2drop  EXIT  THEN
    >r 2dup r> find-name-in dup
    ['] shadow-warning ?warning IF  2drop  EXIT  THEN
    warnings @ >r warnings off
    sp@ fp@ 2>r 2dup warning-recs recognize 2r> rot >r
    fp! sp! r> rectype-null <>  r> dup warnings ! 0<> and
    ['] shadow-num-warning ?warning  2drop
; is check-shadow
[then]

: ?warn-dp ( -- )
    >num-state @ >num-state off 1 and 0= dpl @ 0>= and warnings @ abs 1 > and
    [: '' emit input-lexeme 2@ type
	." ' is a double-cell integer; type `help' for more info" ;] ?warning
    warnings @ abs 2 > input-lexeme 2@ '. scan nip 1 > and dpl @ 0>= and
    [: '' emit input-lexeme 2@ type
	." ' is a non-standard double: only trailing '.' standard" ;] ?warning ;

' ?warn-dp is ?warn#

\ replacing one word with another

: >colon-body ( xt -- addr )
    dup @ docol: <> -12 and throw >body ;

: >prim-code ( xt -- x )
    \ converts xt of a primitive into a form usable in the code of
    \ colon definitions on the current engine
    threading-method 0= IF @ THEN ;

: replace-word ( xt1 xt2 -- ) \ gforth
  \G make xt2 do xt1, both need to be colon definitions
    swap >colon-body ['] branch >prim-code rot >colon-body 2! ;

\ watching variables and values

: watch-does> ( -- ) DOES> dup @ ~~ drop ;
: watch-comp: ( xt -- ) comp: >body ]] Literal dup @ ~~ drop [[ ; 
: ~~Variable ( "name" -- )
    \G Variable that will be watched on every access
  Create 0 , watch-does> watch-comp: ;

: ~~Value ( n "name" -- )
    \G Value that will be watched on every access
    Value [: >body ~~ ! ; to-opt: >body ]] Literal ~~ ! [[ ;] set-to ;

\ trace lines

: line-tracer ( -- )  ['] ~~ execute ;
\G print source position and stack on every source line start
: +ltrace ( -- ) ['] line-tracer is before-line ;
\G turn on line tracing
: -ltrace ['] noop is before-line ;
\G turn off line tracing

\ view/locate

require string.fs

: esc'type ( addr u -- )
    bounds ?DO
	I c@ ''' = IF  ''' emit '"' emit ''' emit '"' emit ''' emit
	ELSE  I c@ emit  THEN  LOOP ;

: esc'"type ( addr u -- )
    bounds ?DO
	I c@ ''' = IF  ''' emit '"' emit ''' emit '"' emit ''' emit
	ELSE  I c@ '"' = IF  '\' emit '"' emit
	    ELSE  I c@ emit  THEN  THEN  LOOP ;

: view-emacs ( "name" -- ) \ gforth
    [: ." emacsclient -e '(forth-find-tag " '"' emit
      parse-name esc'"type
      '"' emit ." )'" ;] $tmp system ;

: view-vi ( "name" -- ) \ gforth
    [: ." vi -t '" parse-name esc'type ." '" ;] $tmp system ;

: type-prefix ( c-addr1 u1 u -- c-addr2 u2 )
    \ type the u-len prefix of c-addr1 u1, c-addr2 u2 is the rest
    >r 2dup r> umin tuck type /string ;

\ locate/view of recognized tokens show the recognizer, if not a word
\ Idea: Jenny Brian

Variable rec'

: view' ( "name" -- xt )
    \G @var{xt} is either the word to view if it is a word
    \G or the recognizer that successfully parsed @var{"name"}
    what's trace-recognizer >r
    sp@ fp@ 2>r parse-name  name-too-short?
    [: rec' ! ;] is trace-recognizer
    forth-recognizer recognize 2r> rot >r fp! sp! r>  r> is trace-recognizer
    dup rectype-null = -#13 and throw
    rectype-nt <> IF  drop rec' @  THEN ;

: kate-l:c ( line pos -- )
    swap ." -l " . ." -c " . ;
: emacs-l:c ( line pos -- )
    ." +" swap 0 .r ." :" . ;
: vi-l:c ( line pos -- )  ." +" drop . ;
: editor-cmd ( souceview -- )
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

: edit-file-cmd ( c-addr u -- )
    \ prints the editor command for editing the file with the name c-addr u
    s" EDITOR" getenv dup 0= IF
	2drop s" vi" \ if you don't set EDITOR, use vi as default
    THEN
    2dup 2>r type space
    2r@ s" emacsclient" string-prefix? IF  ." -n "  THEN
    ''' emit esc'type ''' emit  2rdrop ;

\ insert a different location

: #loc ( nline nchar "file" -- )
    \G set next word's location to @var{nline nchar} in @var{"file"}
    parse-name 2dup str>loadfilename# dup 0< IF
	drop add-included-file included-files $[]# 1-
    ELSE  nip nip  THEN
    -rot encode-view to replace-sourceview ;

: prompt-ok ( -- )
    ."  ok"
    depth ?dup-if
        space 0 dec.r then
    fdepth ?dup-if
        ."  f:" 0 dec.r then
    rp0 @ rp@ - cell/ 30 - ?dup-if
        ."  r:" 0 dec.r then ;

: prompt-text    state @ IF ."  compiled" EXIT THEN  prompt-ok ;

: color-prompt ( -- )
    ['] prompt-text success-color color-execute ;

' color-prompt is prompt

\ print name vtable

: .name? ( xt -- )
    \ prints name of xt if named, otherwise prints xt as hex number
    dup >name if
	.name
    else
	hex.
    then ;

: .vt ( nt -- )
    >namevt @ cr
    ." opt:    " dup >vtcompile, @ .name? cr
    ." to:     " dup >vtto       @ .name? cr
    ." >int:   " dup >vt>int     @ .name? cr
    ." >comp:  " dup >vt>comp    @ .name? cr
    ." defer@: " dup >vtdefer@   @ .name? cr
    ." extra:  " dup >vtextra    @ .name? cr
    ." >string " dup >vt>string  @ .name? cr
    ." >link   "     >vt>link    @ .name? ;
