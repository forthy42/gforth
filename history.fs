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

\ : clear-tib ( max span addr pos -- max 0 addr 0 false )
\   clear-line 0 tuck dup ;

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

\ Create lfpad #lf c,

: (enter)  ( max span addr pos1 -- max span addr pos2 true )
  >r end^ 2@ hist-setpos
  2dup swap history write-line drop ( throw ) \ don't worry about errors
  hist-pos 2dup backward^ 2! end^ 2!
  r> (ret) ;

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

: tib-full? ( max span addr pos addr' len' -- max span addr pos addr1 u flag )
    5 pick over 4 pick + prefix-found @ 0<> - < ;

: kill-prefix  ( key -- key )
  dup #tab <> IF  prefix-off  THEN ;

\ UTF-8 support

require utf-8.fs

[IFUNDEF] #esc  27 Constant #esc  [THEN]

: save-cursor ( -- )  #esc emit '7 emit ;
: restore-cursor ( -- )  #esc emit '8 emit ;
: .rest ( addr pos1 -- addr pos1 )
    restore-cursor 2dup type ;
: .all ( span addr pos1 -- span addr pos1 )
    restore-cursor >r 2dup swap type r> ;

: <xins>  ( max span addr pos1 xc -- max span addr pos2 )
    >r  2over r@ xc-size + u< IF  ( max span addr pos1 R:xc )
	rdrop bell  EXIT  THEN
    >string over r@ xc-size + swap move
    2dup chars + r@ swap r@ xc-size xc!+? 2drop drop
    r> xc-size >r  rot r@ chars + -rot r> chars + ;
: (xins)  ( max span addr pos1 xc -- max span addr pos2 )
    <xins> .all .rest ;
: xback  ( max span addr pos1 -- max span addr pos2 f )
    dup  IF  over + xchar- over -  0 max .all .rest
    ELSE  bell  THEN 0 ;
: xforw  ( max span addr pos1 -- max span addr pos2 f )
    2 pick over <> IF  over + xc@+ xemit over -  ELSE  bell  THEN 0 ;
: (xdel)  ( max span addr pos1 -- max span addr pos2 )
    over + dup xchar- tuck - >r over -
    >string over r@ + -rot move
    rot r> - -rot ;
: ?xdel ( max span addr pos1 -- max span addr pos2 0 )
  dup  IF  (xdel) .all 2 spaces .rest  THEN  0 ;
: <xdel> ( max span addr pos1 -- max span addr pos2 0 )
  2 pick over <>
    IF  xforw drop (xdel) .all 2 spaces .rest
    ELSE  bell  THEN  0 ;
: xeof  2 pick over or 0=  IF  bye  ELSE  <xdel>  THEN ;

: xfirst-pos  ( max span addr pos1 -- max span addr 0 0 )
  drop 0 .all .rest 0 ;
: xend-pos  ( max span addr pos1 -- max span addr span 0 )
  drop over .all 0 ;


: xclear-line ( max span addr pos1 -- max addr )
    drop restore-cursor swap spaces restore-cursor ;
: xclear-tib ( max span addr pos -- max 0 addr 0 false )
    xclear-line 0 tuck dup ;

: (xenter)  ( max span addr pos1 -- max span addr pos2 true )
    >r end^ 2@ hist-setpos
    2dup swap history write-line drop ( throw ) \ don't worry about errors
    hist-pos 2dup backward^ 2! end^ 2!
    r> .all space true ;

: xkill-expand ( max span addr pos1 -- max span addr pos2 )
    prefix-found cell+ @ ?dup IF  >r
	r@ - >string over r@ + -rot move
	rot r@ - -rot .all r> spaces .rest THEN ;

: insert   ( string length buffer size -- )
    rot over min >r  r@ - ( left over )
    over dup r@ +  rot move   r> move  ;

: xtab-expand ( max span addr pos1 -- max span addr pos2 0 )
    key? IF  #tab (xins) 0  EXIT  THEN
    xkill-expand 2dup extract-word dup 0= IF  nip EXIT  THEN
    search-prefix tib-full?
    IF    bell  2drop  prefix-off
    ELSE  dup >r
	2>r >string r@ + 2r> 2swap insert
	r@ + rot r> + -rot
    THEN
    prefix-found @ IF  bl (xins)  ELSE  .all .rest  THEN  0 ;

: xchar-history ( -- )
    ['] xforw        ctrl F bindkey
    ['] xback        ctrl B bindkey
    ['] ?xdel        ctrl H bindkey
    ['] xeof         ctrl D bindkey
    ['] <xdel>       ctrl X bindkey
    ['] xclear-tib   ctrl K bindkey
    ['] xfirst-pos   ctrl A bindkey
    ['] xend-pos     ctrl E bindkey
    ['] (xenter)     #lf    bindkey
    ['] (xenter)     #cr    bindkey
    ['] xtab-expand  #tab   bindkey
    ['] (xins)       IS insert-char
    ['] kill-prefix  IS everychar
    ['] save-cursor  IS everyline ;

xchar-history

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

