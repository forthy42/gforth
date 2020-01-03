\ ansi.fs      Define terminal attributes              20may93jaw

\ Authors: Bernd Paysan, Anton Ertl, Gerald Wodni, Neal Crook
\ Copyright (C) 1995,1996,1997,1998,2001,2003,2007,2013,2014,2015,2016,2017,2018,2019 Free Software Foundation, Inc.

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

: (Attr!) ( attr -- )
    \G set attribute
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
    dup BG> ?dup IF $F xor 40 + #n; THEN
    dup FG> ?dup IF $F xor 30 + #n; THEN
    drop 0 #n #esc[ 0. #> type #>> ;

' (Attr!) IS Attr!

[IFDEF] debug-out
    debug-out op-vector !
    
    ' (Attr!) IS Attr!
    
    default-out op-vector !
[THEN]

: BlackSpace ( -- )
    Attr @ dup BG> Black =
    IF drop space
    ELSE 0 attr! space attr! THEN ;

Variable mark-attr
: m>>> ( -- )
    attr @ dup mark-attr !
    dup error-hl-ul xor attr!
    ." >>>" error-hl-inv xor attr! ;
: <<<m ( -- )
    mark-attr @ dup error-hl-ul xor attr! ." <<<" attr! ;
' m>>> is mark-start
' <<<m is mark-end

\ check what color our terminal has

$Variable term-rgb$

: is-terminal? ( -- f )
    stdin isatty  stdin isfg and  stdout isatty and ;

: is-color-terminal? ( -- flag )
    s" TERM" getenv
    2dup s" xterm" search nip nip >r
    2dup s" linux" search nip nip >r
         s" rxvt"  search nip nip r> r> or or ;

: is-xterm? ( -- f )
    s" TERM" getenv
    2dup s" xterm" string-prefix? >r
         s" rxvt"  string-prefix? r> or \ rxvt behaves like xterm
    \ OSX' terminal claims to be a full xterm-256color, but isn't
    s" TERM_PROGRAM" getenv s" Apple_Terminal" str= 0= and
    is-terminal? and ;

: term-bg? ( -- rgb )
    \G query terminal's background color, return value in hex RRGGBB
    key? drop \ set terminal into raw mode
    s\" \e]11;?\007" type \ avada kedavra, terminal!
    100 0 ?DO  key? ?LEAVE  1 ms  LOOP \ wait a maximum of 100 ms
    BEGIN  key?  WHILE  key #esc =  UNTIL  ELSE  0  EXIT  THEN
    BEGIN  key?  WHILE  key term-rgb$ c$+!  REPEAT
    term-rgb$ $@ ':' $split 2nip
    '/' $split '/' $split
    ['] s>number $10 base-execute drop >r
    ['] s>number $10 base-execute drop >r
    ['] s>number $10 base-execute drop
    $FF00 and $8 lshift r> $FF00 and or r> $8 rshift or
    term-rgb$ $free ;

: rgb-split ( rgb -- r g b )
    dup $FF and swap 8 rshift
    dup $FF and swap 8 rshift
    ( ) $FF and swap rot ;

$0 Value default-bg

: auto-color ( -- )
    is-terminal? is-color-terminal? and 0= if
        \ TODO: no terminal - switch to other output class
	default-mode  EXIT
    then
    is-xterm? if term-bg? else default-bg then
    rgb-split + + $17F u> IF
	light-mode
    ELSE
	dark-mode
    THEN ;

:noname auto-color defers 'cold ; is 'cold
