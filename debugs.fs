\ Simple debugging aids

\ Copyright (C) 1995,1997,1999,2002,2003,2004,2005,2006,2007,2009,2011,2012,2013,2014 Free Software Foundation, Inc.

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
' .s IS printdebugdata
defer .debugline ( nfile nline -- ) \ gforth print-debug-line
\G Print the source code location indicated by @var{nfile nline}, and
\G additional debugging information; the default @code{.debugline}
\G prints the additional information with @code{printdebugdata}.

: (.debugline) ( xpos -- )
    info-color attr!
    cr .sourcepos1 ." :"
    \ it would be nice to print the name of the following word,
    \ but that's not easily possible for primitives
    printdebugdata
    cr default-color attr! ;

[IFUNDEF] debug-fid
stderr value debug-fid ( -- fid )
\G (value) Debugging output prints to this file
[THEN]

' (.debugline) IS .debugline

: .debugline-directed ( xpos -- )
    op-vector @ { oldout }
    debug-vector @ op-vector !
    ['] .debugline catch
    oldout op-vector !
    throw ;

:noname ( -- )
    current-sourcepos1 .debugline-directed ;
:noname ( compilation  -- ; run-time  -- )
    compile-sourcepos POSTPONE .debugline-directed ;
interpret/compile: ~~ ( -- ) \ gforth tilde-tilde
\G Prints the source code location of the @code{~~} and the stack
\G contents with @code{.debugline}.

:noname ( -- )  stderr to debug-fid  defers 'cold ; IS 'cold

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

: ??? ( -- )
    \G Open a debuging shell
    create-input cr
    BEGIN  ." dbg> " refill  WHILE  source nip WHILE
		interpret ."  ok" cr  REPEAT  THEN
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
	[: warn-color attr!
	    cr current-sourcepos3 .sourcepos3 ." : " execute
	    default-color attr! ;] stderr outfile-execute
	warnings @ abs 4 >= warning-error and throw
	exit then
    drop ;
is ?warning

: shadow-warning ( c-addr u nt -- c-addr u nt )
    dup >r name>string ." redefined " 2dup type ( c-addr u c-addr2 u2 )
    2over str= 0= if
	."  with " 2dup type then
    r> ;
: shadow-num-warning ( c-addr u -- c-addr u )
    ." defined literal " 2dup type ."  as word" ;

10 deque: warning-recs
' rec:float ' rec:num 2 warning-recs deque!

' check-shadow >code-address dodefer: = [if]
:noname  ( addr count wid -- )
    \G prints a warning if the string is already present in the wordlist
    >r 2dup r> find-name-in warnings @ 0<> and dup
    ['] shadow-warning ?warning IF  2drop  EXIT  THEN
    warnings @ >r warnings off
    sp@ fp@ 2>r 2dup warning-recs map-recognizer 2r> rot >r
    fp! sp! r> r:fail <>  r> dup warnings ! 0<> and
    ['] shadow-num-warning ?warning  2drop
; is check-shadow
[then]

:noname defers 'cold  warning-recs $boot ; is 'cold
:noname defers 'image warning-recs $save ; is 'image

: ?warn-dp ( -- )
    >num-state @ >num-state off 1 and 0= dpl @ 0>= and warnings @ abs 1 > and
    [: '' emit input-lexeme 2@ type
	." ' is a double-cell integer; type `help' for more info" ;] ?warning
    warnings @ abs 2 > input-lexeme 2@ '. scan nip 1 > and dpl @ 0>= and
    [: '' emit input-lexeme 2@ type
	." ' is a non-standard double: only trailing '.' standard" ;] ?warning ;

' ?warn-dp is ?warn#

\ replacing one word with another

: replace-word ( xt2 xt1 -- )
  \G make xt1 do xt2, both need to be colon definitions
  >body  here >r dp !  >r postpone AHEAD  r> >body dp !  postpone THEN
  r> dp ! ;

\ watching variables and values

: watch-does> ( -- ) DOES> dup @ ~~ drop ;
: watch-comp: ( xt -- ) comp: >body ]] Literal dup @ ~~ drop [[ ; 
: ~~Variable ( "name" -- )
    \G Variable that will be watched on every access
  Create 0 , watch-does> watch-comp: ;

: ~~Value ( n "name" -- )
    \G Value that will be watched on every access
    Value [: ~~ >body ! ; comp: drop ]] Literal ~~ >body ! [[ ;] set-to ;

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

Variable locate-file[]
Variable locate-pos

: type-prefix ( c-addr1 u1 u -- c-addr2 u2 )
    \ type the u-len prefix of c-addr1 u1, c-addr2 u2 is the rest
    >r 2dup r> umin tuck type /string ;

: show-pos1 ( pos1 u -- ) {: u :}
    decode-pos1  {: lineno charno :}
    loadfilename#>str locate-file[] $[]slurp-file
    lineno after-locate + 1+ locate-file[] $[]# umin
    lineno before-locate 1+ - 0 max +DO  cr
	I locate-file[] $[]@
	I 1+ lineno = IF
	    warn-color attr! '*' emit  I 1+ 5 .r ." : "  charno type-prefix
	    err-color attr!                                   u type-prefix
	    warn-color attr!                                    type
	    default-color attr!
	ELSE
	    I 1+ 6 .r ." : "  type
	THEN
    LOOP ;
: scroll-pos1 ( pos1 -- )
    decode-pos1 drop nip {: lineno :}
    lineno after-locate + 1+ locate-file[] $[]# umin
    lineno before-locate 1+ - 0 max +DO  cr
	I 1+ 6 .r ." : "  I locate-file[] $[]@ type
    LOOP ;

: view-name {: nt -- :}
    locate-file[] $[]off
    warn-color attr!  nt name>view @ dup cr .sourcepos1  default-color attr!
    dup locate-pos ! nt name>string nip dup located-len ! show-pos1 ;

: +locate-lines ( n -- pos )
    >r locate-pos @ decode-pos1 swap r> + 0 max
    locate-file[] $[]# 1- min swap encode-pos1 ;

: n ( -- )
    before-locate after-locate + 2 +
    +locate-lines dup locate-pos ! scroll-pos1 ;
: b ( -- )
    before-locate after-locate + 2 + negate
    +locate-lines dup locate-pos ! scroll-pos1 ;
: l ( -- )
    warn-color attr!  located-xpos @ dup cr .sourcepos1  default-color attr!
    located-len @ show-pos1 ;

: view-native ( "name" -- )
    (') view-name ;

: kate-l:c ( line pos -- )
    swap ." -l " . ." -c " . ;
: emacs-l:c ( line pos -- )
    ." +" swap 0 .r ." :" . ;
: vi-l:c ( line pos -- )  ." +" drop . ;
: editor-cmd ( soucepos1 -- )
    s" EDITOR" getenv dup 0= IF
	2drop s" vi" \ if you don't set EDITOR, use vi as default
    THEN
    2dup 2>r type space
    decode-pos1 1+
    2r@ s" emacs" search nip nip  2r@ s" gedit" str= or  IF  emacs-l:c  ELSE
	2r@ s" kate" string-prefix? IF  kate-l:c  ELSE
	    vi-l:c  \ also works for joe, mcedit, nano, and is de facto standard
	THEN
    THEN
    ''' emit loadfilename#>str esc'type ''' emit  2rdrop ;

: g ( -- )
    locate-pos @ ['] editor-cmd $tmp system ;

: external-edit ( "name" )
    (') name>view @ locate-pos ! g ;

Defer edit ( "name" -- ) \ gforth
' external-edit IS edit
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

Defer view ( "name" -- ) \ gforth
\G directly view the source in the curent terminal
' view-native IS view

' view alias locate ( "name" -- ) \ forth inc
\G directly view the source in the curent terminal
