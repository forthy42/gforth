\ ansi.fs      Define terminal attributes              20may93jaw

\ Authors: Bernd Paysan, Anton Ertl, Gerald Wodni, Neal Crook
\ Copyright (C) 1995,1996,1997,1998,2001,2003,2007,2013,2014,2015,2016,2017,2018,2019,2020,2021,2022,2023,2024 Free Software Foundation, Inc.

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


\ If you want another terminal you can redefine the colours.

\ But a better way is it only to redefine SET-ATTR
\ to have compatible colours.

\ Attributes description:
\ <A ( -- -1 0 )           Start attributes description
\ A> ( -1 x .. x -- attr ) Terminate an attributes description and
\                          return overall attribute; currently only
\                          12 bits are used.
\
\ >BG ( colour -- x )      x is attribute with colour as Background colour
\ >FG ( colour -- x )      x is attribute with colour as Foreground colour
\
\ SET-ATTR ( attr -- )     Send attributes to terminal
\
\ BG> ( attr -- colour)    extract colour of Background from attr
\ FG> ( attr -- colour)    extract colour of Foreground from attr
\
\ See colorize.fs for an example of usage.

\ To do:        Make <A A> State smart and only compile literals!

decimal

0 $F xor Constant Black
1 $F xor Constant Red
2 $F xor Constant Green
3 $F xor Constant Yellow
4 $F xor Constant Blue
5 $F xor Constant Magenta
6 $F xor Constant Cyan
7 $F xor Constant White
9 $F xor Constant defaultcolor

1 CONSTANT Bold
2 CONSTANT Underline
4 CONSTANT Blink
8 CONSTANT Invers
16 CONSTANT Strikethrough
32 CONSTANT Italic
64 Constant Invisible
128 Constant Dim

\ For portable programs don't use invers and underline

: >BG    8 lshift ;
: >FG    12 lshift ;

: BG>    8 rshift 15 and ;
: FG>    12 rshift 15 and ;

: <A    -1 0 ;
: A>    BEGIN over -1 <> WHILE or REPEAT nip ;

User Attr   0 Attr !
UValue attr? -1 to attr?

: (Attr!) ( attr -- )
    \ set attribute
    attr? 0= IF  drop  EXIT  THEN
    dup Attr @ = IF drop EXIT THEN
    dup $6600 = Attr @ 0= and IF drop EXIT THEN
    dup Attr !
    <<# 'm' hold
    dup Bold and IF 1 #n; THEN
    dup Dim and IF 2 #n; THEN
    dup Italic and IF 3 #n; THEN
    dup Underline and IF 4 #n; THEN
    dup Blink and IF 5 #n; THEN
    dup Invers and IF 7 #n; THEN
    dup Invisible and IF 8 #n; THEN
    dup Strikethrough and IF 9 #n; THEN
    dup BG> ?dup-IF $F xor 40 + #n; THEN
    dup FG> ?dup-IF $F xor 30 + #n; THEN
    drop 0 #n #esc[ #0. #> type #>> ;

' (Attr!) IS Attr!

\ Themes

0 AValue current-theme \ points to a string containing the current theme
: theme: ( "name" -- )
    $Variable DOES> to current-theme ;

: theme-color@ ( u -- color )
    cells current-theme $@ rot safe/string IF  @  ELSE  drop 0  THEN ;

:noname >body @ current-theme $[] ! ; is theme!
:noname >body @ theme-color@ ; is theme@

Create white? 0 ,
DOES> @ theme-color@ ;
' theme-to set-to

: (theme-color!) ( u -- )
    dup theme-color ! theme-color@ attr! ;

' (theme-color!) is theme-color!

[IFDEF] debug-out
    debug-out op-vector !
    
    ' (Attr!) IS Attr!
    ' (theme-color!) is theme-color!
    
    default-out op-vector !
[THEN]

: BlackSpace ( -- )
    Attr @ dup BG> Black =
    IF drop space
    ELSE 0 attr! space attr! THEN ;

Variable mark-attr
: m>>> ( -- )
    attr @ mark-attr !
    error-hl-ul
    ." >>>" error-hl-inv ;
: <<<m ( -- )
    error-hl-ul ." <<<" mark-attr @ attr! ;
' m>>> is mark-start
' <<<m is mark-end

\ check what color our terminal has

$Variable term-rgb$

: is-terminal? ( -- f )
    stdin isatty  stdin isfg and  stdout isatty and ;

: is-color-terminal? ( -- flag )
    s" TERM" getenv
    2dup s" screen." string-prefix? IF  7 /string  THEN
    2dup s" xterm" string-prefix? >r
    2dup s" rxvt"  string-prefix? >r
    2dup s" foot"  string-prefix? >r
         s" linux" string-prefix?
    r> or r> or r> or ;

: is-xterm? ( -- f )
    s" TERM" getenv
    2dup s" screen." string-prefix? IF  7 /string  THEN
    2dup s" xterm" string-prefix? >r
    2dup s" rxvt"  string-prefix? >r
         s" foot"  string-prefix?
    r> or r> or \ rxvt and foot behave like xterm
    \ OSX' terminal claims to be a full xterm-256color, but isn't
    s" TERM_PROGRAM" getenv s" Apple_Terminal" str= 0= and
    is-terminal? and ;

: string>rgb ( addr u -- rgb )
    '/' $split '/' $split
    2 umin ['] s>number $10 base-execute drop >r
    2 umin ['] s>number $10 base-execute drop >r
    2 umin ['] s>number $10 base-execute drop
    8 lshift r> or 8 lshift r> or ;

: term-rgb@ ( -- rgb )
    \ read color value returned from terminal
    100 0 ?DO  key? ?LEAVE  1 ms  LOOP \ wait a maximum of 100 ms
    BEGIN  key?  WHILE  key #esc =  UNTIL  ELSE  0  EXIT  THEN
    BEGIN  key?  WHILE  key term-rgb$ c$+!  REPEAT
    term-rgb$ $@ ':' $split 2nip string>rgb term-rgb$ $free ;

: term-color? ( n -- rgb )
    \ query terminal's colors by number
    key? drop \ set terminal into raw mode
    s\" \e]4;" type 0 .r s\" ;?\e\\" type
    term-rgb@ ;
: term-fg? ( -- rgb )
    \ query terminal's foreground color, return value in hex RRGGBB
    key? drop \ set terminal into raw mode
    s\" \e]10;?\a" type \ avada kedavra, terminal!
    term-rgb@ ;
: term-bg? ( -- rgb )
    \ query terminal's background color, return value in hex RRGGBB
    key? drop \ set terminal into raw mode
    s\" \e]11;?\a" type \ avada kedavra, terminal!
    term-rgb@ ;

: rgb-split ( rgb -- r g b )
    dup $FF and swap 8 rshift
    dup $FF and swap 8 rshift
    ( ) $FF and swap rot ;

0 Value default-bg

theme: uncolored-mode ( -- ) \ gforth
\G This mode does not set colors, but uses the default ones.

uncolored-mode

false to white?
<a defaultcolor >fg defaultcolor >bg a> to default-color
false to error-color
false to warning-color
false to info-color
false to success-color
false to input-color
false to error-hl-ul
false to error-hl-inv
<a invers a> to status-color
<a invers a> to compile-color
<a invers a> to postpone-color

theme: light-mode ( -- ) \ gforth
\G color theme for white background

light-mode
true  to white?
<a defaultcolor >fg defaultcolor >bg a> to default-color
<a red >fg defaultcolor >bg a> to error-color
<a magenta >fg defaultcolor >bg a> to warning-color
<a cyan >fg defaultcolor >bg a> to info-color
<a green >fg defaultcolor >bg a> to success-color
<a defaultcolor >fg defaultcolor >bg bold a> to input-color
<a red >fg defaultcolor >bg underline a> to error-hl-ul
<a red >fg defaultcolor >bg invers a> to error-hl-inv
<a white >fg blue >bg bold a> to status-color
<a white >fg magenta >bg bold a> to compile-color
<a white >fg red >bg bold a> to postpone-color

theme: dark-mode ( -- ) \ gforth
\G color theme for black background

dark-mode
false to white?
<a defaultcolor >fg defaultcolor >bg a> to default-color
<a red >fg defaultcolor >bg bold a> to error-color
<a yellow >fg defaultcolor >bg bold a> to warning-color
<a cyan >fg defaultcolor >bg bold a> to info-color
<a green >fg defaultcolor >bg bold a> to success-color
<a defaultcolor >fg defaultcolor >bg bold a> to input-color
<a red >fg defaultcolor >bg underline bold a> to error-hl-ul
<a red >fg defaultcolor >bg invers bold a> to error-hl-inv
<a white >fg blue >bg bold a> to status-color
<a white >fg magenta >bg bold a> to compile-color
<a white >fg red >bg bold a> to postpone-color

uncolored-mode

: magenta-input ( -- ) \ gforth
    \G make input color easily recognizable (useful in presentations)
    [ <a magenta >fg defaultcolor >bg bold a> ]L white? + to input-color ;
: default-input ( -- ) \ gforth
    \G make input color easily recognizable (useful in presentations)
    [ <a defaultcolor >fg defaultcolor >bg bold a> ]L to input-color ;

: rgb>mode  ( rgb -- )
    rgb-split + + $17F u> IF  light-mode  ELSE  dark-mode  THEN ;

0 Value term-rgb?

slowvoc on wordlist constant gforth-init slowvoc off

: set-colors { xt: color -- }
    attr? IF
	current-theme >r
	light-mode  color
	dark-mode   color
	r> to current-theme !
    THEN ;

get-current gforth-init set-current
: light     attr? IF  light-mode     0 to term-rgb? THEN ;
: dark      attr? IF  dark-mode      0 to term-rgb? THEN ;
: uncolored attr? IF  uncolored-mode 0 to term-rgb? THEN ;
: magenta   ['] magenta-input set-colors ;
: default   ['] default-input set-colors ;
: auto ;
set-current

: ?gforth-init ( -- )
    s" GFORTH_INIT" getenv 2dup d0<> if
	action-of forth-recognize >r
	gforth-init is forth-recognize ['] evaluate catch IF 2drop THEN
	r> is forth-recognize
    else  2drop  then ;

: auto-color ( -- )
    uncolored-mode \ default mode
    is-terminal? is-color-terminal? and 0<>
    dup 2 and  to term-rgb?  to attr?
    ?gforth-init  attr? 0= term-rgb? 0= or ?EXIT
    is-xterm? if
	s" SSH_CONNECTION" getenv d0= if
	    term-bg? rgb>mode  0 to term-rgb?
	then
    else
	default-bg rgb>mode
    then ;

:is 'cold auto-color defers 'cold ;

\ scrolling etc: (thanks to Ulrich Hoffmann)

: (control-sequence) ( u char -- )
    ?dup-IF  .\" \e[" swap 0 dec.r emit  ELSE  #esc emit 0 dec.r  THEN ;

' (control-sequence) IS control-sequence

[IFDEF] debug-out
    debug-out op-vector !
    
    ' (control-sequence) IS control-sequence
    
    default-out op-vector !
[THEN]

\ special cases for safe/restore cursor position
: save-cursor-position ( -- ) 7 0 control-sequence ;
: restore-cursor-position  ( -- ) 8 0 control-sequence ;

: control-sequence: ( c -- )
    \ defines ESC [ num <c>
    Create c,
  Does> ( u -- )  c@ control-sequence ;

'A' control-sequence: cursor-up ( u -- )
'B' control-sequence: cursor-down ( u -- )
'L' control-sequence: insert-lines ( u -- )
'J' control-sequence: erase-display ( u -- )
\ 0: erase cursor and below; 1: erase above cursor; 2: erase screen

'E' control-sequence: cursor-next-line ( u -- )
'F' control-sequence: cursor-previous-line ( u -- )
'S' control-sequence: scroll-up ( u -- )
'T' control-sequence: scroll-down ( u -- )

\ : text-above ( u1 u2 -- ) \ u1 lines up  show u2 new lines
\    \ 0 ED \ from here to end of display
\    save-cursor-position
\    dup SU   swap over + CPL
\    dup IL
\    drop \ 0 DO I . cr LOOP
\    restore-cursor-position ;
