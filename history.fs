\ command line edit and history support                 16oct94py

\ Authors: Bernd Paysan, Anton Ertl, Jens Wilke
\ Copyright (C) 1995,2000,2003,2004,2005,2006,2007,2008,2010,2011,2012,2013,2014,2015,2016,2017,2018,2019,2020,2021,2022,2023,2024 Free Software Foundation, Inc.

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

require user-object.fs
require mkdir.fs

edit-out next-task - class-o !

kernel-editor cell- @ 2 cells - 2@ \ extend edit-out class
umethod paste! ( addr u -- )
umethod paste@ ( -- addr u )
umethod grow-tib ( max span addr pos1 more -- max span addr pos1 flag )
umethod edit-error
umethod ekeys
cell uvar edit-curpos
cell uvar screenw
cell uvar setstring$ \ additional string at cursor for IME

Variable paste$ \ global paste buffer

align , , here
' (ins) , ' (ins-string) , ' (edit-control) ,
' noop ,  ' noop , ' noop , ' std-ctrlkeys , ' false , \ kernel stuff
' noop ,  ' noop ,  ' 0> , ' bell , ' noop , \ extended stuff
, here  0 , 0 , 0 , 0 , 0 , 0 ,
Constant edit-terminal
edit-terminal cell- @ Constant edit-terminal-c
edit-terminal edit-out !

\ command line editing                                  16oct94py

: >edit-rest  ( span addr pos1 -- span addr pos1 addr2 len )
    \ get rest of the string
    over fourth third chars /string ;

: bindkey ( xt key -- )
    dup bl u>= abort" Ctrl codes only!"
    cells ctrlkeys + ! ;
: ebindkey ( xt key -- )
    dup keycode-limit keycode-start within abort" Ekeys only!"
    keycode-start - cells ekeys + ! ;

: ctrl-i ( "<char>" -- c )
    char toupper $40 xor ;

' ctrl-i
:noname
    ctrl-i postpone Literal ;
interpret/compile: ctrl  ( "<char>" -- ctrl-code )

\ history support                                       16oct94py

0 Value history \ history file fid

:noname ( -- ) defers 'image 0 to history ; is 'image

2Variable forward^
2Variable backward^
2Variable end^
Variable vt100-modifier \ shift, ctrl, alt

?: -scan ( addr u char -- addr' u' )
    >r  BEGIN  dup  WHILE  1- 2dup + c@ r@ =  UNTIL  THEN
    rdrop ;

: force-open ( addr len -- fid )
    2dup r/w open-file
    IF
	drop
	2dup '/' -scan $1FF mkdir-parents drop
	r/w create-file throw
    ELSE
	nip nip
    THEN ;

: history-file ( -- addr u )
    s" GFORTHHIST" getenv dup 0= IF
	2drop s" XDG_DATA_HOME" getenv dup 0= IF
	    2drop s" ~/.local/share"  THEN
	[: type ." /gforth/history" ;] $tmp
    THEN ;

\ moving in history file                               16oct94py

: edit-curpos-off  edit-curpos off  edit-linew off  cols screenw ! ;

: clear-line ( max span addr pos1 -- max addr )
    drop nip ;

: xretype ( max span addr pos1 -- max span addr pos1 f )
    edit-update false ;

: hist-pos    ( -- ud )
    history ?dup-IF  file-position drop  ELSE  backward^ 2@  THEN ;
: hist-setpos ( ud -- )
    history ?dup-IF  reposition-file drop  ELSE  2drop  THEN ;

: get-line ( addr len -- len' flag )
    swap history ?dup-IF  read-line throw
    ELSE  2drop 0 false  THEN ;

: next-line  ( max span addr pos1 -- max span addr pos2 false )
  clear-line
  forward^ 2@ 2dup hist-setpos backward^ 2!
  2dup get-line drop
  hist-pos  forward^ 2!
  tuck xretype ;

: find-prev-line ( max addr -- max span addr pos2 )
  backward^ 2@ forward^ 2!
  over 2 + negate s>d backward^ 2@ d+ #0. dmax 2dup hist-setpos
  BEGIN
      backward^ 2!   2dup get-line  WHILE
      hist-pos 2dup forward^ 2@ d<  WHILE
      rot drop
  REPEAT  2drop  THEN  tuck ;

: prev-line  ( max span addr pos1 -- max span addr pos2 false )
    clear-line find-prev-line xretype ;

\ Create lfpad #lf c,

$10 buffer: lastline#
$10 buffer: thisline#

: lastline<> ( addr u -- flag )
    false thisline# dup $10 erase hashkey2
    thisline# $10 lastline# over str= 0= dup IF
	thisline# lastline# $10 move
    THEN ;

: write-history ( addr u -- )
    2dup -trailing nip IF
	2dup lastline<>  IF
	    end^ 2@ hist-setpos
	    history
	    ?dup-IF  write-line drop
		history flush-file drop \ don't worry about errors
	    ELSE  2drop  THEN
	    hist-pos 2dup backward^ 2! end^ 2!
	    EXIT
	ELSE
	    hist-pos 2dup backward^ 2! end^ 2!
	THEN
    THEN
    2drop ;

: (enter)  ( max span addr pos1 -- max span addr pos2 true )
    >r 2dup swap write-history r> (ret) ;

: extract-word ( addr len -- addr' len' )
    dup >r
    BEGIN  1- dup 0>=  WHILE  2dup + c@ bl =  UNTIL  THEN  1+
    tuck + r> rot - ;

Create prefix-found  0 , 0 ,

0 value alphabetic-tab

: word-lex ( nfa1 nfa2 -- -1/0/1 )
    dup 0=
    IF
	2drop 1  EXIT
    THEN
    name>string 2>r name>string
    vt100-modifier @ IF  2r> 2swap 2>r  THEN
    dup r@ = alphabetic-tab or
    IF
	rdrop r> over capscompare 0<=  EXIT
    THEN
    r> < nip rdrop ;

: search-voc ( addr len nfa1 nfa2 -- addr len nfa3 )
    >r
    BEGIN
	dup
    WHILE
	>r dup r@ name>string nip <=
	IF
	    2dup r@ name>string drop over capscompare  0=
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
	r> name>link
    REPEAT
    drop r> ;

: prefix-off ( -- )  0 0 prefix-found 2! ;

: prefix-string ( addr len nfa -- addr' len' )
    dup prefix-found !  ?dup-IF
	name>string rot /string rot drop
	dup 1+ prefix-found cell+ !
    ELSE
	2drop s" " prefix-off
    THEN ;

Defer search-prefix
: simple-search-prefix  ( addr1 len1 -- addr2 len2 )
    0 ['] search-order >body $@ cell MEM-DO
        I cell- 2@ <>
        IF  I @ wordlist-id @ swap  search-voc  THEN
    LOOP
    prefix-string ;
' simple-search-prefix is search-prefix

: tib-full? ( max span addr pos addr' len' -- max span addr pos addr' len' flag )
    5 pick over 4 pick + u< ;

: kill-prefix  ( key -- key )
  dup #tab <> over [ k-tab k-shift-mask or ]L <> and IF  prefix-off  THEN ;

\ UTF-8 support

require utf-8.fs

synonym setstring-color info-color

\ retype an edited line: this is generic, every word should use edit-update
\ and nothing else to redraw the edited string

: xedit-startpos ( -- )
    \ correction for line=screenw, no wraparound then!
    edit-curpos @ dup screenw @ mod 0= over 0> and \ flag, true=-1
    dup >r + screenw @ /mod negate swap r> - negate swap at-deltaxy ;
: get-hw ( addr u -- lines rest )
    screenw @ x-lines+rest setstring$ $@ screenw @ +x-lines+rest ;
: hw>width ( lines rest -- width ) swap screenw @ * + ;
: get-width+ ( addr u -- width )
    get-hw hw>width ;
: get-width+all ( span addr pos -- width )
    2dup get-hw 2>r  >r swap r> safe/string
    2r> 2swap screenw @ +x-lines+rest
    hw>width ;
: .resizeline ( span addr pos -- span addr pos )
    >r 2dup r@ get-width+all
    dup >r edit-linew @ u< IF
	xedit-startpos
	edit-linew @ spaces  edit-linew @ edit-curpos !
    ELSE
	r@ 1+ screenw @ / 0> IF
	    xedit-startpos  0 edit-curpos !  .unstatus  THEN
    THEN
    r> edit-linew !  r> ;
: .all ( span addr pos -- span addr pos )
    xedit-startpos  2dup type  setstring$ $@
    dup IF  setstring-color type input-color  ELSE  2drop  THEN
    >edit-rest type  edit-linew @ edit-curpos !  ;
: .all-rest ( span addr pos -- span addr pos )
    xedit-startpos  2dup get-width+ edit-curpos !
    2dup type ;
: .rest ( span addr pos -- span addr pos )
    dup fourth = IF
	2dup get-width+ edit-curpos !  EXIT  THEN
    .all-rest ;
: xedit-update ( span addr pos1 -- span addr pos1 )
    \ word to update the editor display
    .resizeline .all
    edit-linew @ 1+ screenw @ / 0> IF
	2>r 2>r .status 2r> 2r> .all-rest
    ELSE  .rest  THEN ;

: xhide ( max span addr pos1 -- max span addr pos1 f )
    over 0 tuck default-color edit-update .unstatus 2drop drop  false ;

\ In the following, addr max is the buffer, addr span is the current
\ string in the buffer, and pos1 is the cursor position in the buffer.

: xgrow-tib { max span addr pos1 more -- max span addr pos1 flag }
    max span more + u>= IF  max span addr pos1 true  EXIT  THEN
    addr tib = IF
	span #tib !
	span more + max#tib @ 2* umax expand-tib
	max#tib @ span tib pos1 true EXIT  THEN
    max span addr pos1 false ;

: (xins)  ( max span addr pos1 xc -- max span addr pos2 )
    dup  >r xc-size grow-tib 0= IF  rdrop edit-error  EXIT  THEN
    >edit-rest over r@ xc-size + swap move
    2dup chars + r@ swap r@ xc-size xc!+? 2drop drop
    r> xc-size >r  rot r@ chars + -rot r> chars + ;
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
	0 max edit-update
    ELSE  edit-error  THEN 0 ;
: xforw  ( max span addr pos1 -- max span addr pos2 f )
    third over <> IF
	vt100-modifier @ IF
	    BEGIN  third over u> >r 2dup + c@ bl = r> and  WHILE
		    over + xchar+ over -  REPEAT
	    BEGIN  third over u> >r 2dup + c@ bl <> r> and  WHILE
		    over + xchar+ over -  REPEAT
	ELSE
	    over + xchar+ over -
	THEN
	edit-update
    ELSE  edit-error  THEN  0 ;
: (xdel)  ( max span addr pos1 -- max span addr pos2 )
    over + dup xchar- tuck - >r over -
    >edit-rest over r@ + -rot move
    rot r> - -rot ;
: xdel ( max span addr pos1 -- max span addr pos2 )
    (xdel) edit-update ;
: ?xdel ( max span addr pos1 -- max span addr pos2 0 )
    vt100-modifier @ IF
	BEGIN  dup  WHILE
		2dup 1- + c@ bl u<= WHILE  (xdel)  REPEAT  THEN
	BEGIN  dup  WHILE
		2dup 1- + c@ bl u> WHILE  (xdel)  REPEAT  THEN
	edit-update
    ELSE  dup IF   xdel  THEN  THEN  0 ;
: <xdel> ( max span addr pos1 -- max span addr pos2 0 )
    vt100-modifier @ IF  ?xdel  EXIT  THEN  \ emacs binds Alt-Del to Alt-Backspace
    third over <>
    IF  xforw drop xdel  ELSE  edit-error  THEN  0 ;
: xeof  third over or 0=  IF  -56 throw  ELSE  <xdel>  THEN ;

: xfirst-pos  ( max span addr pos1 -- max span addr 0 0 )
  drop 0 xretype ;
: xend-pos  ( max span addr pos1 -- max span addr span 0 )
  drop over xretype ;

: xpaste! ( addr u -- )
    paste$ $! ;

: xclear-rest ( max span addr pos -- max pos addr pos false )
    >edit-rest paste! rot drop tuck xretype ;

: xclear-first ( max span addr pos -- max pos addr pos false )
    2dup paste!  >r
    2dup swap r@ /string third swap move
    swap r> - swap 0 xretype ;

: xins-string ( max span addr pos addr1 u1 -- max span' addr pos' )
    2>r r@ grow-tib 0= IF  edit-error 2rdrop  EXIT  THEN
    >edit-rest 2r@ 2swap r@ + insert
    r@ + rot r> + -rot  rdrop ;

: (xenter)  ( max span addr pos1 -- max span addr span true )
    setstring$ $@ xins-string  setstring$ $free
    drop 2dup swap write-history
    over edit-update edit-curpos-off true ;
: xeof' ( max span addr pos1 -- max span addr pos2 flag )
    third over or 0=  IF  -56 throw  ELSE  (xenter)  THEN ;

: xkill-expand ( max span addr pos1 -- max span addr pos2 )
    prefix-found cell+ @ ?dup-IF  dup
	>r - >edit-rest over r@ + -rot move
	rot r> - -rot  THEN ;

?: insert   ( string length buffer size -- )
    rot over min dup  >r - ( left over )
    over dup r@ +  rot move   r> move  ;

: (xtab-expand) ( max span addr pos1 -- max span addr pos2 0 )
    xkill-expand 2dup extract-word dup 0= IF  nip EXIT  THEN
    search-prefix tuck 2>r  prefix-found @ 0<> - grow-tib
    0= IF  edit-error  2rdrop  prefix-off 0  EXIT  THEN
    >edit-rest r@ + 2r> dup >r 2swap insert
    r@ + rot r> + -rot
    prefix-found @ IF  bl (xins)  THEN  edit-update  0 ;

: xtab-expand ( max span addr pos1 -- max span addr pos2 0 )
    key? IF  #tab (xins) 0  EXIT  THEN
    (xtab-expand) ;

: xpaste@ ( -- addr u )
    paste$ $@ ;

: xpaste ( max span addr pos -- max span' addr pos' false )
    paste@ xins-string  edit-update  0 ;

: xtranspose ( max span addr pos -- max span' addr pos' false )
    dup IF
	third over = IF  over + xchar- over -  THEN
	2dup + xchar- xc@ >r (xdel)
	over + xchar+ over - r> (xins)
    THEN 0 ;

Variable setcur# \ relative to the end, in utf8 charactes
Variable setsel# \ size of selection relative to the end

: xback-chars ( addr len +n -- addr len' )
    0 +DO x\string- dup 0<= ?LEAVE LOOP ;
: xchars>chars ( addr len +n -- len' )
    >r tuck r>  0 +DO  +x/string  dup 0<= ?LEAVE  LOOP  nip - ;
: setcur ( max span addr pos1 -- max span addr pos2 )
    drop over setcur# @ 0<= IF
	setsel# @ setcur# @ - xback-chars
    ELSE  2dup setcur# @ xchars>chars nip  THEN ;
: setsel ( max span addr pos1 -- max span addr pos2 0 )
    setstring$ $@ xins-string
    setcur >r 2dup swap r@ safe/string
    2dup 2dup setsel# @ xchars>chars nip tuck setstring$ $!
    delete
    swap setstring$ $@len - swap r> xretype ;
: xreformat ( max span addr pos1 -- max span addr pos1 0 )
    .unstatus xedit-startpos input-color
    edit-linew @ screenw @ /mod cols dup screenw ! * +
    dup spaces dup edit-curpos ! edit-linew !
    .resizeline .all 2>r 2>r .status 2r> 2r> .rest false ;

Create xchar-altkeys ( -- )
$80 0 [DO] ' false , [LOOP]

: altbindkey ( xt key -- )
    dup $80 u>= abort" Alt+ASCII only!"
    cells altkeys + ! ;

: xchar-altkey ( max span addr pos1 -- max span addr pos2 flag )
    8 vt100-modifier !  false ;

: xdelw ( max span addr pos1 -- max span addr pos2 flag )
    dup >r xforw drop >edit-rest r> -rot 2>r
    tuck - >r 2dup + r@ paste! \ set paste$
    rot r> - -rot \ update span
    2r> 2over + swap move
    edit-update false ;

: xfgcolor ( max span addr pos1 -- max span addr pos2 flag )
    xreformat ;
: xbgcolor ( max span addr pos1 -- max span addr pos2 flag )
    ekey-rgb rgb>mode xreformat -1 attr ! ;

Create xchar-ctrlkeys ( -- )
    ' false        , ' xfirst-pos   , ' xback        , ' false        ,
    ' xeof         , ' xend-pos     , ' xforw        , ' false        ,
    ' ?xdel        , ' xtab-expand  , ' (xenter)     , ' xclear-rest  ,
    ' xreformat    , ' (xenter)     , ' next-line    , ' false        ,

    ' prev-line    , ' false        , ' false        , ' setsel       ,
    ' xtranspose   , ' xclear-first , ' xpaste       , ' false        ,
    ' <xdel>       , ' xpaste       , ' xhide        , ' xchar-altkey ,
    ' false        , ' false        , ' false        , ' false        ,

Create std-ekeys
    ' xback ,        ' xforw ,        ' prev-line ,    ' next-line ,
    ' xfirst-pos ,   ' xend-pos ,     ' prev-line ,    ' next-line ,
    ' false ,        ' <xdel> ,       ' (xenter) ,     ' false ,
    ' false ,        ' false ,        ' false ,        ' false ,
    ' false ,        ' false ,        ' false ,        ' false ,
    ' false ,        ' false ,        ' false ,        ' xreformat ,
    ' xhide ,        ' false ,        ' prev-line ,    ' next-line ,
    ' ?xdel ,        ' xtab-expand ,  ' setsel ,       ' xfgcolor ,
    ' xbgcolor ,     ' xeof' ,

: xchar-edit-ctrl ( max span addr pos1 ekey -- max span addr pos2 flag )
    dup mask-shift# rshift 7 and vt100-modifier !
    dup 1 mask-shift# lshift 1- and swap keycode-start u>= IF
	cells ekeys + perform  EXIT  THEN
    dup $80 $20 vt100-modifier @ 2 and select
    u>= IF  drop false  EXIT  THEN
    cells altkeys ctrlkeys vt100-modifier @ 2 and select
    + perform ;

: xins ( max span addr pos1 char -- max span addr pos2 )
    vt100-modifier @ 8 and IF
	2 mask-shift# lshift or xchar-edit-ctrl drop
    ELSE  (xins)  THEN ;

: xchar-history ( -- )
    edit-terminal edit-out ! ;

: xeveryline ( -- )
    edit-curpos-off  key? drop \ set terminal into raw mode
    term-rgb? 1 and IF  s\" \e]10;?\a" type  THEN
    term-rgb? 2 and IF  s\" \e]11;?\a" type  THEN
    0 to term-rgb? ;

xchar-history

' xins            IS insert-char
' xins-string     IS insert-string
' kill-prefix     IS everychar
' xeveryline      IS everyline
' xedit-update    IS edit-update
' xpaste!         IS paste!
' xpaste@         IS paste@
' xgrow-tib       IS grow-tib
' xchar-ctrlkeys  IS ctrlkeys
' xchar-altkeys   IS altkeys
' bell            IS edit-error
' std-ekeys       IS ekeys
' xchar-edit-ctrl IS edit-control

' xback 'b' altbindkey
' xdelw 'd' altbindkey
' xforw 'f' altbindkey

\ initializing history

: get-history ( addr len -- )
    ['] force-open catch-nobt
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
    history-file get-history xchar-history edit-curpos-off ;

:noname ( -- )
    defers bootmessage
    history-cold
; is bootmessage

history-cold

