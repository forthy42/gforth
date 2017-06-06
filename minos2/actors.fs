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
    event( ." keyed: " type cr ) ; simple-actor is ukeyed
:noname ( ekey -- )
    event( ." ekeyed: " hex. cr ) ; simple-actor is ekeyed
: .touch ( $xy b -- )
    event( hex. $@ bounds ?DO  I sf@ f.  1 sfloats +LOOP cr ) ;
:noname ( $xy b -- )
    event( ." down: " .touch )
; simple-actor is touchdown
:noname ( $xy b -- )
    event( ." up: " .touch )
; simple-actor is touchup
:noname ( $xy b -- )
    event( ." move: " .touch )
; simple-actor is touchmove

: simple[] ( o -- o )
    >o simple-actor new to act o act >o to caller-w o> o o> ;

\ actor for a box with one active element

actor class
    value: active-w
end-class box-actor

:noname ( rx ry b n -- )
    fover fover simple-inside? IF
	o caller-w >o
	[: { c-act } act IF  fover fover act .inside?
		IF
		    o c-act >o to active-w o>
		    fover fover 2dup act .clicked   THEN  THEN
	c-act ;] do-childs o> drop
    THEN  2drop fdrop fdrop ;
box-actor is clicked
:noname ( addr u -- )
    active-w ?dup-IF  .act .ukeyed  THEN ; box-actor is ukeyed
:noname ( ekey -- )
    active-w ?dup-IF  .act .ekeyed  THEN ; box-actor is ekeyed
' simple-inside? box-actor is inside?
: xy@ ( addr -- rx ry )  $@ drop dup sf@ sfloat+ sf@ ;
:noname ( $xy b -- )
    over xy@ simple-inside? IF
	o caller-w >o
	[: { c-act } act IF  over xy@ act .inside?
		IF
		    o c-act >o to active-w o>
		    2dup act .touchdown   THEN  THEN
	c-act ;] do-childs o> drop
    THEN  2drop ; box-actor is touchdown
:noname ( $xy b -- )
    over xy@ simple-inside? IF
	o caller-w >o
	[: { c-act } act IF  over xy@ act .inside?
		IF
		    o c-act >o to active-w o>
		    2dup act .touchup   THEN  THEN
	c-act ;] do-childs o> drop
    THEN  2drop ; box-actor is touchup
:noname ( $xy b -- )
    over xy@ simple-inside? IF
	o caller-w >o
	[: { c-act } act IF  over xy@ act .inside?
		IF
		    o c-act >o to active-w o>
		    2dup act .touchmove  THEN  THEN
	c-act ;] do-childs o> drop
    THEN  2drop ; box-actor is touchmove

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

' grow-edit$ is grow-tib
' noop is edit-update \ no need to do that here

edit-terminal edit-out !

simple-actor class
    value: edit-curpos
    value: edit-w
end-class edit-actor

\ edit things

: edit-xt ( xt o:actor -- )
    edit-out @ >r  history >r  edit-widget edit-out !  >r  0 to history
    edit-w >o addr text$ o> dup edit$ ! $@ swap over swap edit-curpos
    r> catch >r to edit-curpos drop edit$ @ $!len drop
    r> r> edit-out !  r> to history throw
    need-sync on ;

keycode-limit keycode-start - 1+ buffer: keycode-tab
: bind-ekey ( ctrl ekey -- )  [ keycode-tab keycode-start - ]L + c! ;
ctrl F k-right bind-ekey
ctrl B k-left  bind-ekey
ctrl P k-up    bind-ekey
ctrl N k-down  bind-ekey

:noname ( key o:actor -- )
    [: 4 roll ekey>ckey dup k-shift-mask u>= IF
	    dup mask-shift# rshift 7 and vt100-modifier !
	    [ 1 mask-shift# lshift 1- ]L and  THEN
	>control edit-control drop ;] edit-xt ; edit-actor is ekeyed
:noname ( addr u o:actor -- )
    [: 2rot insert-string ;] edit-xt ; edit-actor is ukeyed

: edit[] ( o widget -- o )
    swap >o edit-actor new to act
    o act >o to caller-w to edit-w o> o o> ;

