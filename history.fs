\ History file support                                 16oct94py

\ Copyright (C) 1995 Free Software Foundation, Inc.

\ This file is part of Gforth.

\ Gforth is free software; you can redistribute it and/or
\ modify it under the terms of the GNU General Public License
\ as published by the Free Software Foundation; either version 2
\ of the License, or (at your option) any later version.

\ This program is distributed in the hope that it will be useful,
\ but WITHOUT ANY WARRANTY; without even the implied warranty of
\ MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
\ GNU General Public License for more details.

\ You should have received a copy of the GNU General Public License
\ along with this program; if not, write to the Free Software
\ Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.

0 Value history

2Variable forward^
2Variable backward^
2Variable end^

: force-open ( addr len -- handle )
  2dup r/w open-file 0<
  IF  drop r/w create-file throw  ELSE  nip nip  THEN ;

: get-history ( addr len -- wid )
  force-open to history
  history file-size throw
  2dup forward^ 2! 2dup backward^ 2! end^ 2! ;

s" ~/.gforth-history" get-history

: history-cold
    Defers 'cold
    s" ~/.gforth-history" get-history ;

' history-cold IS 'cold

\ moving in history file                               16oct94py

: clear-line ( max span addr pos1 -- max addr )
  backspaces over spaces swap backspaces ;

: clear-tib ( max span addr pos -- max 0 addr 0 false )
  clear-line 0 tuck dup ;

: hist-pos    ( -- ud )  history file-position throw ;
: hist-setpos ( ud -- )  history reposition-file throw ;

: get-line ( addr len -- len' flag )
  swap history read-line throw ;

: next-line  ( max span addr pos1 -- max span addr pos2 false )
  clear-line
  forward^ 2@ 2dup hist-setpos backward^ 2!
  2dup get-line drop
  hist-pos  forward^ 2!
  tuck 2dup type 0 ;

: prev-line  ( max span addr pos1 -- max span addr pos2 false )
  clear-line  backward^ 2@ forward^ 2!
  over 2 + negate s>d backward^ 2@ d+ 0. dmax 2dup hist-setpos
  BEGIN
      backward^ 2!   2dup get-line  WHILE
      hist-pos 2dup forward^ 2@ d<  WHILE
      rot drop
  REPEAT  2drop  THEN
  tuck 2dup type 0 ;

: ctrl  ( "<char>" -- ctrl-code )
  char [char] @ - postpone Literal ; immediate

Create lfpad #lf c,

: (enter)  ( max span addr pos1 -- max span addr pos2 true )
  >r end^ 2@ hist-setpos
  2dup swap history write-line throw
  hist-pos 2dup backward^ 2! end^ 2!
  r> (ret) ;

\ some other key commands                              16oct94py

: first-pos  ( max span addr pos1 -- max span addr 0 0 )
  backspaces 0 0 ;
: end-pos  ( max span addr pos1 -- max span addr span 0 )
  type-rest 2drop over 0 ;

: extract-word ( addr len -- addr' len' )  dup >r
  BEGIN  1- dup 0>=  WHILE  2dup + c@ bl =  UNTIL  THEN  1+
  tuck + r> rot - ;

Create prefix-found  0 , 0 ,

: word-lex ( nfa1 nfa2 -- -1/0/1 )
  dup 0=  IF  2drop 1  EXIT  THEN
  cell+ >r cell+ count $1F and
  dup r@ c@ $1F and =
  IF  r> char+ capscomp 0<=  EXIT  THEN
  nip r> c@ $1F and < ;

: search-voc ( addr len nfa1 nfa2 -- addr len nfa3 ) >r
    BEGIN  dup  WHILE  >r dup r@ cell+ c@ $1F and <=
	IF  2dup r@ cell+ char+ capscomp  0=
	    IF  r> dup r@ word-lex
		IF  dup prefix-found @ word-lex
		    0>= IF  rdrop dup >r  THEN
		THEN >r
	    THEN
	THEN  r> @
    REPEAT drop r> ;

: prefix-string ( addr len nfa -- addr' len' )
    dup prefix-found !  ?dup
    IF  cell+ count $1F and rot /string rot drop
	dup 1+ prefix-found cell+ !
    ELSE
	2drop s" " prefix-found cell+ off
    THEN ;

: search-prefix  ( addr1 len1 -- addr2 len2 )
    0 vp dup @ 1- cells over +
    DO  I 2@ <>
        IF  I cell+ @ @ swap  search-voc  THEN
	[ -1 cells ] Literal +LOOP
    prefix-string ;

: kill-expand ( max span addr pos1 -- max span addr pos2 )
    prefix-found cell+ @  0 ?DO  (del)  LOOP ;

: tib-full? ( max span addr pos addr' len' -- max span addr pos addr1 u flag )
    5 pick over 4 pick + prefix-found @ 0<> - < ;

: tab-expand ( max span addr pos1 -- max span addr pos2 0 )
    kill-expand  2dup extract-word search-prefix
    tib-full?
    IF    7 emit  2drop  0 0 prefix-found 2!
    ELSE  bounds ?DO  I c@ (ins)  LOOP  THEN
    prefix-found @ IF  bl (ins)  THEN  0 ;

: kill-prefix  ( key -- key )
  dup #tab <> IF  0 0 prefix-found 2!  THEN ;

' kill-prefix IS everychar

' next-line  ctrl N cells ctrlkeys + !
' prev-line  ctrl P cells ctrlkeys + !
' clear-tib  ctrl K cells ctrlkeys + !
' first-pos  ctrl A cells ctrlkeys + !
' end-pos    ctrl E cells ctrlkeys + !
' (enter)    #lf    cells ctrlkeys + !
' (enter)    #cr    cells ctrlkeys + !
' tab-expand #tab   cells ctrlkeys + !
