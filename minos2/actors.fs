\ MINOS2 actors basis

\ Copyright (C) 2017 Free Software Foundation, Inc.

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

\ actors are responding to any events that need to be handled

\ actor handler class

\ platform specific action handler

[IFDEF] x11      include x11-actors.fs      [THEN]
[IFDEF] android  include android-actors.fs  [THEN]

\ generic actor stuff

actor class
end-class simple-actor

: simple-inside? ( rx ry -- flag )
    caller-w >o
    y f- fdup d f< h fnegate f> and
    x f- fdup w f< 0e f> and
    and o> ;
' simple-inside? simple-actor is inside?

debug: event( \ +db event(
:noname { f: rx f: ry b n -- }
    event( ." simple click: " rx f. ry f. b . n . cr ) ; simple-actor is clicked
:noname ( addr u -- )
    event( ." keyed: " type cr )else( 2drop ) ; simple-actor is ukeyed
:noname ( ekey -- )
    event( ." ekeyed: " hex. cr )else( drop ) ; simple-actor is ekeyed
: .touch ( $xy b -- )
    event( hex. $@ bounds ?DO  I sf@ f.  1 sfloats +LOOP cr )else( 2drop ) ;
:noname ( $xy b -- )
    event( ." down: " .touch )else( 2drop )
; simple-actor is touchdown
:noname ( $xy b -- )
    event( ." up: " .touch )else( 2drop )
; simple-actor is touchup
:noname ( $xy b -- )
    event( ." move: " .touch )else( 2drop )
; simple-actor is touchmove

: simple[] ( o -- o )
    >o simple-actor new to act o act >o to caller-w o> o o> ;

\ actor for a box with one active element

actor class
    value: active-w
end-class box-actor

: re-focus { c-act -- }
    c-act .active-w ?dup-IF  .act .defocus  THEN
    o c-act >o to active-w o>
    c-act .active-w ?dup-IF  .act .focus  THEN ;

:noname ( rx ry b n -- )
    fover fover simple-inside? IF
	o caller-w >o
	[: { c-act } act IF  fover fover act .inside?
		IF
		    c-act re-focus
		    fover fover 2dup act .clicked   THEN  THEN
	c-act ;] do-childs o> drop
    THEN  2drop fdrop fdrop ;
box-actor is clicked
:noname ( addr u -- )
    active-w ?dup-IF  .act .ukeyed  ELSE  2drop  THEN ; box-actor is ukeyed
:noname ( ekey -- )
    active-w ?dup-IF  .act .ekeyed  ELSE  drop   THEN ; box-actor is ekeyed
' simple-inside? box-actor is inside?
: xy@ ( addr -- rx ry )  $@ drop dup sf@ sfloat+ sf@ ;
:noname ( $xy b -- )
    over xy@ simple-inside? IF
	o caller-w >o
	[: { c-act } act IF  over xy@ act .inside?
		IF
		    \ c-act re-focus
		    2dup act .touchdown   THEN  THEN
	c-act ;] do-childs o> drop
    THEN  2drop ; box-actor is touchdown
:noname ( $xy b -- )
    over xy@ simple-inside? IF
	o caller-w >o
	[: { c-act } act IF  over xy@ act .inside?
		IF
		    \ c-act re-focus
		    2dup act .touchup   THEN  THEN
	c-act ;] do-childs o> drop
    THEN  2drop ; box-actor is touchup
:noname ( $xy b -- )
    over xy@ simple-inside? IF
	o caller-w >o
	[: { c-act } act IF  over xy@ act .inside?
		IF
		    \ c-act re-focus
		    2dup act .touchmove  THEN  THEN
	    c-act ;] do-childs o> drop
    THEN  2drop ; box-actor is touchmove
:noname ( -- ) caller-w >o [: act ?dup-IF  .defocus  THEN ;] do-childs o> ; box-actor is defocus

: box[] ( o -- o )
    >o box-actor new to act o act >o to caller-w o> o o> ;

\ edit actor

edit-terminal-c class
    cell uvar edit$ \ pointer to the edited string
end-class edit-widget-c

edit-widget-c ' new static-a with-allocater Constant edit-widget

: grow-edit$ { max span addr pos1 more -- max span addr pos1 true }
    max span more + u> IF  max span addr pos1 true  EXIT  THEN
    span more + edit$ @ $!len
    edit$ @ $@ swap span swap pos1 true ;

edit-widget edit-out !

bl cells buffer: edit-ctrlkeys
xchar-ctrlkeys edit-ctrlkeys bl cells move

' edit-ctrlkeys is ctrlkeys
' grow-edit$ is grow-tib
' noop is edit-update \ no need to do that here
' noop is edit-error  \ no need to make annoying bells
[IFDEF] clipboard!     ' clipboard!     is paste!  [THEN]
[IFDEF] android-paste! ' android-paste! is paste!  [THEN]

\ extra key bindings for editors

simple-actor class
    method edit-next-line
    method edit-prev-line
    defer: edit-enter
    value: edit-w
end-class edit-actor

' false edit-actor is edit-next-line
' false edit-actor is edit-prev-line

: edit-paste ( max span addr pos1 - max span addr pos2 false )
    clipboard@ xins-string edit-update 0 ;

0 value xselw

: edit-copy ( max span addr pos1 -- max span addr pos1 false )
    >r 2dup swap r@ safe/string xselw min clipboard!
    r> 0 ;
: edit-cut ( max span addr pos1 -- max span addr pos1 false )
    edit-copy drop >r
    2dup swap r@ safe/string xselw delete
    swap xselw - swap
    r> edit-update 0 ;

' edit-next-line ctrl N bindkey
' edit-prev-line ctrl P bindkey
' edit-paste     ctrl V bindkey
' edit-paste     ctrl Y bindkey
' edit-copy      ctrl C bindkey
' edit-cut       ctrl X bindkey
' edit-cut       ctrl W bindkey
' edit-enter     #lf    bindkey
' edit-enter     #cr    bindkey

edit-terminal edit-out !

\ edit things

: edit-xt ( xt o:actor -- )
    edit-out @ >r  history >r  edit-widget edit-out !  >r  0 to history
    edit-w >o addr text$ curpos cursize 0 max o> to xselw
    >r dup edit$ ! $@ swap over swap r>
    r> catch >r edit-w >o to curpos 0 to cursize o> drop edit$ @ $!len drop
    r> r> edit-out !  r> to history throw
    need-sync on need-glyphs on ;

: edit>curpos ( x o:actor -- )
    edit-w >o  text-font to font
    x f- border f- w border f2* f- text-w f/ f/
    text$ pos-string to curpos
    o>  need-sync on ;

:noname ( key o:actor -- )
    [: 4 roll ekey>ckey dup k-shift-mask u>= IF
	    dup mask-shift# rshift 7 and vt100-modifier !
	    [ 1 mask-shift# lshift 1- ]L and  THEN
	>control edit-control drop ;] edit-xt ; edit-actor is ekeyed
:noname ( addr u o:actor -- )
    [: 2rot insert-string ;] edit-xt ; edit-actor is ukeyed
:noname ( o:actor -- )
    edit-w >o -1 to cursize o> need-sync on ; edit-actor is defocus
:noname ( o:actor -- )
    edit-w >o 0 to cursize o> need-sync on ; edit-actor is focus
:noname ( $rxy*n bmask -- )
    case 1 of
	    edit-w .start-curpos 0>= IF
		xy@ fdrop edit>curpos
		edit-w >o
		curpos start-curpos 2dup - abs to cursize
		umin to curpos
		o>
	    ELSE  drop
	    THEN
	endof
	nip
    endcase
; edit-actor is touchmove
:noname ( o:actor rx ry b n -- )
    dup 1 and IF  edit-w .start-curpos 0< IF
	    2drop fdrop edit>curpos  edit-w >o
	    curpos to start-curpos  -1 to cursize o>
	ELSE
	    2drop fdrop fdrop
	THEN
    ELSE
	swap
	case
	    1 of
		drop fdrop edit>curpos
		edit-w >o
		start-curpos 0>= IF
		    curpos start-curpos 2dup - abs to cursize
		    umin to curpos
		    text$ curpos safe/string cursize min primary!
		    -1 to start-curpos
		THEN  o>
	    endof
	    2 of  drop fdrop edit>curpos
		[: primary@ insert-string ;] edit-xt  endof
	    4 of  ( menu   )  drop fdrop fdrop  endof
	    nip fdrop fdrop
	endcase
    THEN
; edit-actor is clicked

: edit[] ( o widget -- o )
    swap >o edit-actor new to act
    o act >o to caller-w to edit-w ['] true is edit-enter o> o o> ;

