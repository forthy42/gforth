\ command line edit and history support                 16oct94py

\ Copyright (C) 1995,2000,2003,2004 Free Software Foundation, Inc.

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
\ Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111, USA.

:noname
    char [char] @ - ;
:noname
    char [char] @ - postpone Literal ;
interpret/compile: ctrl  ( "<char>" -- ctrl-code )

\ command line editing                                  16oct94py

: >string  ( span addr pos1 -- span addr pos1 addr2 len )
  over 3 pick 2 pick chars /string ;
: type-rest ( span addr pos1 -- span addr pos1 back )
  >string tuck type ;
: (del)  ( max span addr pos1 -- max span addr pos2 )
  1- >string over 1+ -rot move
  rot 1- -rot  #bs emit  type-rest bl emit 1+ backspaces ;
: (ins)  ( max span addr pos1 char -- max span addr pos2 )
  >r >string over 1+ swap move 2dup chars + r> swap c!
  rot 1+ -rot type-rest 1- backspaces 1+ ;
: ?del ( max span addr pos1 -- max span addr pos2 0 )
  dup  IF  (del)  THEN  0 ;
: (ret)  type-rest drop true space ;
: back  dup  IF  1- #bs emit  ELSE  #bell emit  THEN 0 ;
: forw 2 pick over <> IF  2dup + c@ emit 1+  ELSE  #bell emit  THEN 0 ;
: <del> ( max span addr pos1 -- max span addr pos2 0 )
  2 pick over <>
	IF  forw drop (del)  ELSE  #bell emit  THEN  0 ;
: eof  2 pick over or 0=  IF  bye  ELSE  <del>  THEN ;

: bindkey ( xt key -- )  cells ctrlkeys + ! ;

\ history support                                       16oct94py

0 Value history \ history file fid

2Variable forward^
2Variable backward^
2Variable end^

: force-open ( addr len -- fid )
    2dup r/w open-file
    IF
	drop r/w create-file throw
    ELSE
	nip nip
    THEN ;

s" os-class" environment? [IF] s" unix" str= [ELSE] true [THEN] 
[IF]
: history-file ( -- addr u )
    s" GFORTHHIST" getenv dup 0= IF
	2drop s" ~/.gforth-history"
    THEN ;
[ELSE]

: history-dir ( -- addr u )
  s" TMP" getenv ?dup ?EXIT drop
  s" TEMP" getenv ?dup ?EXIT drop
  s" c:/" ;

: history-file ( -- addr u )
  s" GFORTHHIST" getenv ?dup ?EXIT
  drop
  history-dir pad place
  s" /ghist.fs" pad +place pad count ;
[THEN]

\ moving in history file                               16oct94py

: clear-line ( max span addr pos1 -- max addr )
  backspaces over spaces swap backspaces ;

: clear-tib ( max span addr pos -- max 0 addr 0 false )
  clear-line 0 tuck dup ;

: hist-pos    ( -- ud )  history file-position drop ( throw ) ;
: hist-setpos ( ud -- )  history reposition-file drop ( throw ) ;

: get-line ( addr len -- len' flag )
  swap history read-line throw ;

: next-line  ( max span addr pos1 -- max span addr pos2 false )
  clear-line
  forward^ 2@ 2dup hist-setpos backward^ 2!
  2dup get-line drop
  hist-pos  forward^ 2!
  tuck 2dup type 0 ;

: find-prev-line ( max addr -- max span addr pos2 )
  backward^ 2@ forward^ 2!
  over 2 + negate s>d backward^ 2@ d+ 0. dmax 2dup hist-setpos
  BEGIN
      backward^ 2!   2dup get-line  WHILE
      hist-pos 2dup forward^ 2@ d<  WHILE
      rot drop
  REPEAT  2drop  THEN  tuck ;

: prev-line  ( max span addr pos1 -- max span addr pos2 false )
    clear-line find-prev-line 2dup type 0 ;

Create lfpad #lf c,

: (enter)  ( max span addr pos1 -- max span addr pos2 true )
  >r end^ 2@ hist-setpos
  2dup swap history write-line drop ( throw ) \ don't worry about errors
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

: sgn ( n -- -1/0/1 )
 dup 0= IF EXIT THEN  0< 2* 1+ ;

: capscomp  ( c_addr1 u c_addr2 -- n )
 swap bounds
 ?DO  dup c@ I c@ <>
     IF  dup c@ toupper I c@ toupper =
     ELSE  true  THEN  WHILE  1+  LOOP  drop 0
 ELSE  c@ toupper I c@ toupper - unloop  THEN  sgn ;

: word-lex ( nfa1 nfa2 -- -1/0/1 )
    dup 0=
    IF
	2drop 1  EXIT
    THEN
    name>string 2>r name>string
    dup r@ =
    IF
	rdrop r> capscomp 0<= EXIT
    THEN
    r> <
    nip rdrop ;

: search-voc ( addr len nfa1 nfa2 -- addr len nfa3 )
    >r
    BEGIN
	dup
    WHILE
	>r dup r@ name>string nip <=
	IF
	    2dup r@ name>string drop capscomp  0=
	    IF
		r> dup r@ word-lex
		IF
		    dup prefix-found @ word-lex
		    0>=
		    IF
			rdrop dup >r
		    THEN
		THEN
		>r
	    THEN
	THEN
	r> @
    REPEAT
    drop r> ;

: prefix-off ( -- )  0 0 prefix-found 2! ;

: prefix-string ( addr len nfa -- addr' len' )
    dup prefix-found !  ?dup
    IF
	name>string rot /string rot drop
	dup 1+ prefix-found cell+ !
    ELSE
	2drop s" " prefix-off
    THEN ;

: search-prefix  ( addr1 len1 -- addr2 len2 )
    0 vp dup @ 1- cells over +
    DO  I 2@ <>
        IF  I cell+ @ wordlist-id @ swap  search-voc  THEN
	[ -1 cells ] Literal +LOOP
    prefix-string ;

: kill-expand ( max span addr pos1 -- max span addr pos2 )
    prefix-found cell+ @  0 ?DO  (del)  LOOP ;

: tib-full? ( max span addr pos addr' len' -- max span addr pos addr1 u flag )
    5 pick over 4 pick + prefix-found @ 0<> - < ;

: tab-expand ( max span addr pos1 -- max span addr pos2 0 )
    key? IF  #tab (ins) 0  EXIT  THEN
    kill-expand  2dup extract-word dup 0= IF  nip EXIT  THEN
    search-prefix  tib-full?
    IF    7 emit  2drop  0 0 prefix-found 2!
    ELSE  bounds ?DO  I c@ (ins)  LOOP  THEN
    prefix-found @ IF  bl (ins)  THEN  0 ;

: kill-prefix  ( key -- key )
  dup #tab <> IF  prefix-off  THEN ;

[IFUNDEF] everyline defer everyline [THEN]

: 8-bit-io ( -- )
    ['] forw        ctrl F bindkey
    ['] back        ctrl B bindkey
    ['] ?del        ctrl H bindkey
    ['] eof         ctrl D bindkey
    ['] <del>       ctrl X bindkey
    ['] next-line   ctrl N bindkey
    ['] prev-line   ctrl P bindkey
    ['] clear-tib   ctrl K bindkey
    ['] first-pos   ctrl A bindkey
    ['] end-pos     ctrl E bindkey
    ['] (enter)     #lf    bindkey
    ['] (enter)     #cr    bindkey
    ['] tab-expand  #tab   bindkey
    ['] (ins)       IS insert-char
    ['] kill-prefix IS everychar
    ['] noop        IS everyline
    [ action-of key ] Literal IS key
    [ action-of emit ] Literal IS emit ;

\ UTF-8 support

require utf-8.fs

: utf-8-cold ( -- )
    s" LANG" getenv s" .UTF-8" search nip nip
    IF  utf-8-io  ELSE  8-bit-io  THEN ;

' utf-8-cold INIT8 chained

utf-8-cold

\ initializing history

: get-history ( addr len -- )
    ['] force-open catch
    ?dup-if
	\ !! >stderr
        \ history-file type ." : " .error cr
	drop 2drop
	['] false ['] false ['] (ret)
    else
	to history
	history file-size throw
	2dup forward^ 2! 2dup backward^ 2! end^ 2!
	['] next-line ['] prev-line ['] (enter)
    endif
    dup #lf bindkey
        #cr bindkey
     ctrl P bindkey
     ctrl N bindkey
;

: history-cold ( -- )
    history-file get-history ;

' history-cold INIT8 chained
history-cold

