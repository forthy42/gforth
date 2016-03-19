\ command line edit and history support                 16oct94py

\ Copyright (C) 1995,2000,2003,2004,2005,2006,2007,2008,2010,2011,2012,2013,2014,2015 Free Software Foundation, Inc.

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

Defer edit-update ( span addr pos1 -- span addr pos1 )
\G deferred word to keep an editor informed about the command line content
' noop is edit-update

\ command line editing                                  16oct94py

: >string  ( span addr pos1 -- span addr pos1 addr2 len )
    \G get rest of the string
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

: history-file ( -- addr u )
    s" GFORTHHIST" getenv dup 0= IF
	2drop s" ~/.gforth-history"
    THEN ;

\ moving in history file                               16oct94py

defer back-restore ( u -- )
defer cur-correct ( addr u -- )
' backspaces IS back-restore
' 2drop IS cur-correct

Variable linew
Variable linew-all
Variable screenw
Variable setstring \ additional string at cursor for IME
: linew-off  linew off cols screenw ! ;

[IFDEF] x-width
: clear-line ( max span addr pos1 -- max addr )
    drop linew @ back-restore
    2dup swap x-width setstring $@ x-width +
    dup spaces back-restore nip linew off ;
[ELSE]
: clear-line ( max span addr pos1 -- max addr )
  back-restore over spaces swap back-restore ;
[THEN]
\ : clear-tib ( max span addr pos -- max 0 addr 0 false )
\   clear-line 0 tuck dup ;

: hist-pos    ( -- ud )
    history ?dup-IF  file-position drop  ELSE  backward^ 2@  THEN ;
: hist-setpos ( ud -- )
    history ?dup-IF  reposition-file drop  ELSE  2drop  THEN ;

: get-line ( addr len -- len' flag )
    swap history ?dup IF  read-line throw
    ELSE  2drop 0 false  THEN ;

: next-line  ( max span addr pos1 -- max span addr pos2 false )
  clear-line
  forward^ 2@ 2dup hist-setpos backward^ 2!
  2dup get-line drop
  hist-pos  forward^ 2!
  tuck 2dup type 2dup cur-correct edit-update 0 ;

: find-prev-line ( max addr -- max span addr pos2 )
  backward^ 2@ forward^ 2!
  over 2 + negate s>d backward^ 2@ d+ 0. dmax 2dup hist-setpos
  BEGIN
      backward^ 2!   2dup get-line  WHILE
      hist-pos 2dup forward^ 2@ d<  WHILE
      rot drop
  REPEAT  2drop  THEN  tuck ;

: prev-line  ( max span addr pos1 -- max span addr pos2 false )
    clear-line find-prev-line 2dup type 2dup cur-correct edit-update 0 ;

\ Create lfpad #lf c,

: (enter)  ( max span addr pos1 -- max span addr pos2 true )
    >r 2dup swap -trailing nip IF
	end^ 2@ hist-setpos
	2dup swap history
	?dup-IF  write-line drop \ don't worry about errors
	ELSE  2drop  THEN
	hist-pos 2dup backward^ 2! end^ 2!
    THEN  r> (ret) ;

: extract-word ( addr len -- addr' len' )
    dup >r
    BEGIN  1- dup 0>=  WHILE  2dup + c@ bl =  UNTIL  THEN  1+
    tuck + r> rot - ;

Create prefix-found  0 , 0 ,

: capscomp  ( c_addr1 u c_addr2 -- -1|0|1 )
    swap bounds ?DO
	count toupper i c@ toupper - ?dup-IF
	    nip 0< 2* 1+ unloop exit THEN
    LOOP
    drop 0 ;

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
	r> >link @
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

\ : cygwin? ( -- flag ) s" TERM" getenv s" cygwin" str= ;
\ : at-xy? ( -- x y )
\     key? drop \ make sure prep_terminal() is executed
\     #esc emit ." [6n"  0 0
\     BEGIN  key dup 'R <>  WHILE
\ 	    dup '; = IF  drop  swap  ELSE
\ 		dup '0 '9 1+ within  IF  '0 - swap 10 * +  ELSE
\ 		    drop  THEN  THEN
\     REPEAT  drop 1- swap 1- ;
\ : cursor@ ( -- n )  at-xy? screenw @ * + ;
\ : cursor! ( n -- )  screenw @ /mod at-xy ;
: xcur-correct  ( addr u -- )  x-width linew ! ;

' xcur-correct IS cur-correct

info-color Value setstring-color

: color-execute ( color xt -- )
    attr! catch default-color attr! throw ;

: xback-restore ( u -- )
    \ correction for line=screenw, no wraparound then!
    dup screenw @ mod 0= over 0> and \ flag, true=-1
    dup >r + screenw @ /mod negate swap r> - negate swap at-deltaxy ;
: .rest ( addr pos1 -- addr pos1 )
    linew @ xback-restore 2dup type 2dup cur-correct ;
: .all ( span addr pos1 -- span addr pos1 )
    linew @ xback-restore
    2dup type setstring $@
    dup IF  ['] type setstring-color color-execute  ELSE  2drop  THEN
    >r 2dup swap r@ /string type
    2dup swap cur-correct setstring $@ x-width linew +! r>
    linew @ linew-all ! edit-update ;
: .redraw ( span addr pos1 -- span addr pos1 )
    .all .rest ;

: xretype ( max span addr pos1 -- max span addr pos1 f )
    linew @ dup xback-restore  screenw @ /mod
    cols dup screenw ! * + dup spaces linew !
    .redraw false ;

: xhide ( max span addr pos1 -- max span addr pos1 f )
    linew @ xback-restore 2 pick dup spaces xback-restore
    linew off  false ;

\ In the following, addr max is the buffer, addr span is the current
\ string in the buffer, and pos1 is the cursor position in the buffer.

: <xins>  ( max span addr pos1 xc -- max span addr pos2 )
    >r  2over r@ xc-size + u< IF  ( max span addr pos1 R:xc )
	rdrop bell 0  EXIT  THEN
    >string over r@ xc-size + swap move
    2dup chars + r@ swap r@ xc-size xc!+? 2drop drop
    r> xc-size >r  rot r@ chars + -rot r> chars + ;
: (xins)  ( max span addr pos1 xc -- max span addr pos2 )
    <xins> key? 0= IF  .redraw  THEN ;
: xback  ( max span addr pos1 -- max span addr pos2 f )
    dup  IF
	vt100-modifier @ IF
	    BEGIN  2dup + 1- c@ bl = over 0> and  WHILE
		    over + xchar- over -  REPEAT
	    BEGIN  2dup + 1- c@ bl <> over 0> and  WHILE
		    over + xchar- over -  REPEAT
	ELSE
	    over + xchar- over -
	THEN
	0 max .redraw
    ELSE  bell  THEN 0 ;
: xforw  ( max span addr pos1 -- max span addr pos2 f )
    2 pick over <> IF
	vt100-modifier @ IF
	    BEGIN  2 pick over u> >r 2dup + c@ bl = r> and  WHILE
		    over + xchar+ over -  REPEAT
	    BEGIN  2 pick over u> >r 2dup + c@ bl <> r> and  WHILE
		    over + xchar+ over -  REPEAT
	ELSE
	    over + xchar+ over -
	THEN
	.redraw
    ELSE  bell  THEN 0 ;
: (xdel)  ( max span addr pos1 -- max span addr pos2 )
    over + dup xchar- tuck - >r over -
    >string over r@ + -rot move
    rot r> - -rot ;
: xdel ( max span addr pos1 -- max span addr pos2 )
    2dup + dup xchar- tuck - x-width >r
    (xdel) .all r@ spaces r> linew +! .rest ;
: ?xdel ( max span addr pos1 -- max span addr pos2 0 )
    dup  IF  xdel  THEN  0 ;
: <xdel> ( max span addr pos1 -- max span addr pos2 0 )
    2 pick over <>
    IF  xforw drop xdel  ELSE  bell  THEN  0 ;
: xeof  2 pick over or 0=  IF  -56 throw  ELSE  <xdel>  THEN ;

: xfirst-pos  ( max span addr pos1 -- max span addr 0 0 )
  drop 0 .redraw 0 ;
: xend-pos  ( max span addr pos1 -- max span addr span 0 )
  drop over .all 0 ;

Variable paste$

Defer paste!
: xpaste! ( addr u -- )
    paste$ $! ;
' xpaste! is paste!

: xclear-rest ( max span addr pos -- max pos addr pos false )
    rot >r tuck 2dup r> swap /string 2dup paste!
    x-width dup spaces linew +! .all 0 ;

: xclear-first ( max span addr pos -- max pos addr pos false )
    2dup paste! linew @ xback-restore >r
    2dup swap x-width dup spaces xback-restore  linew off
    2dup swap r@ /string 2 pick swap move
    swap r> - swap 0 .redraw 0 ;

: (xenter)  ( max span addr pos1 -- max span addr pos2 true )
    >r 2dup swap -trailing nip IF
	end^ 2@ hist-setpos
	2dup swap history
	?dup-IF  write-line drop \ don't worry about errors
	ELSE  2drop  THEN
	hist-pos 2dup backward^ 2! end^ 2!
    THEN  r> .all space true ;

: xkill-expand ( max span addr pos1 -- max span addr pos2 )
    prefix-found cell+ @ ?dup IF  >r
	r@ - >string over r@ + -rot move
	rot r@ - -rot .all r@ spaces r> back-restore .rest THEN ;

[IFUNDEF] insert
: insert   ( string length buffer size -- )
    rot over min >r  r@ - ( left over )
    over dup r@ +  rot move   r> move  ;
[THEN]

: xtab-expand ( max span addr pos1 -- max span addr pos2 0 )
    key? IF  #tab (xins) 0  EXIT  THEN
    xkill-expand 2dup extract-word dup 0= IF  nip EXIT  THEN
    search-prefix tib-full?
    IF    bell  2drop  prefix-off
    ELSE  dup >r
	2>r >string r@ + 2r> 2swap insert
	r@ + rot r> + -rot
    THEN
    prefix-found @ IF  bl (xins)  ELSE  .redraw  THEN  0 ;

: xpaste ( max span addr pos -- max span' addr pos' false )
    2over paste$ $@len + u< IF
	rdrop bell  0 EXIT  THEN
    >string paste$ $@ 2swap paste$ $@len + insert
    paste$ $@len + 2>r paste$ $@len + 2r> .redraw  0 ;

: xtranspose ( max span addr pos -- max span' addr pos' false )
    dup IF
	2 pick over = IF  over + xchar- over -  THEN
	2dup + xchar- xc@ >r (xdel)
	over + xchar+ over - r> (xins)
    THEN 0 ;

: setcur ( max span addr pos1 -- max span addr pos2 0 )
    drop 0 .redraw 0 ;
: setsel ( max span addr pos1 -- max span addr pos2 0 )
    >r 2dup swap r@ /string 2dup setstring $!
    dup >r r@ - over r@ + -rot move
    swap r> - swap r> .redraw 0 ;

: xchar-history ( -- )
    ['] setcur       ctrl A bindkey
    ['] xback        ctrl B bindkey
    ['] xeof         ctrl D bindkey
    ['] xend-pos     ctrl E bindkey
    ['] xforw        ctrl F bindkey
    ['] ?xdel        ctrl H bindkey
    ['] xtab-expand  #tab   bindkey \ ctrl I
    ['] (xenter)     #lf    bindkey \ ctrl J
    ['] xclear-rest  ctrl K bindkey
    ['] xretype      ctrl L bindkey
    ['] (xenter)     #cr    bindkey \ ctrl M
    ['] next-line    ctrl N bindkey
    ['] prev-line    ctrl P bindkey
    ['] setsel       ctrl S bindkey
    ['] xtranspose   ctrl T bindkey
    ['] xclear-first ctrl U bindkey
    ['] <xdel>       ctrl X bindkey
    ['] xpaste       ctrl Y bindkey
    ['] xhide        ctrl Z bindkey \ press ctrl-L to reshow
    ['] (xins)       IS insert-char
    ['] kill-prefix  IS everychar
[ifdef] everyline
    ['] linew-off     IS everyline
[endif]
    ['] xback-restore IS back-restore
    ['] xcur-correct  IS cur-correct
;

xchar-history

\ initializing history

: get-history ( addr len -- )
    ['] force-open catch
    ?dup-if
	\ !! >stderr
        \ history-file type ." : " .error cr
	drop 2drop 0 to history
    else
	to history
	history file-size throw
	2dup forward^ 2! 2dup backward^ 2! end^ 2!
    endif
;

: history-cold ( -- )
    history-file get-history xchar-history ;

:noname ( -- )
    defers 'cold
    history-cold
; is 'cold

history-cold

