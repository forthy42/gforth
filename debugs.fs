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

: ?warn-dp ( -- )
    warnings @ abs 1 > IF
	>num-state @ 1 and 0= dpl @ 0>= and  >num-state off
	warning" number with embedded '.'s converted to double integers, not float. Use base prefix for doubles and exponent for floats to disambiguate"
	dpl @ 0> warning" Non-standard double; '.' not in the last position"
    THEN ;
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

\ lines to show before and after locate
3 value before-locate
12 value after-locate

Variable locate-file[]
Variable locate-pos

: show-pos1 ( pos1 nt -- ) {: nt :}
    decode-pos1  nt name>string nip {: lineno charno offset :}
    loadfilename#>str locate-file[] $[]slurp-file
    lineno after-locate + 1+ locate-file[] $[]# umin
    lineno before-locate 1+ - 0 max +DO  cr
	I 1+ lineno = IF
	    err-color attr!
	    '*' emit  I 3 .r ." : "
	    I locate-file[] $[]@
	    over charno type charno /string
	    info-color attr!
	    over nt name>string nip dup >r type r> /string
	    err-color attr!
	    type
	    default-color attr!
	ELSE
	    I 4 .r ." : "
	    I locate-file[] $[]@ type
	THEN
    LOOP ;
: scroll-pos1 ( pos1 -- )
    decode-pos1 drop nip {: lineno :}
    lineno after-locate + 1+ locate-file[] $[]# umin
    lineno before-locate 1+ - 0 max +DO  cr
	I 4 .r ." : "
	I locate-file[] $[]@ type
    LOOP ;

: view-name {: nt -- :}
    locate-file[] $[]off
    warn-color attr!  nt name>view @ dup cr .sourcepos1  default-color attr!
    dup locate-pos ! nt show-pos1 ;

: +locate-lines ( n -- pos )
    >r locate-pos @ decode-pos1 swap r> + swap encode-pos1 ;

: n ( -- )
    before-locate after-locate + 2 +
    +locate-lines dup locate-pos ! scroll-pos1 ;
: b ( -- )
    before-locate after-locate + 2 + negate
    +locate-lines dup locate-pos ! scroll-pos1 ;

: view-native ( "name" -- )
    (') view-name ;

: kate-l:c ( line pos -- )
    swap ." -l " . ." -c " . ;
: emacs-l:c ( line pos -- )
    ." +" swap 0 .r ." :" . ;
: vi-l:c ( line pos -- )  ." +" drop . ;
: editor-cmd ( soucepos1 -- )
    s" EDITOR" getenv dup 0= IF  2drop s" vi"  THEN
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
