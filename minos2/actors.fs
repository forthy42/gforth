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

:noname { f: rx f: ry b n -- }
    ." simple click: " rx f. ry f. b . n . cr ; simple-actor is clicked
:noname ( addr u -- )
    ." keyed: " type cr ; simple-actor is ukeyed
:noname ( ekey -- )
    ." ekeyed: " hex. cr ; simple-actor is ekeyed

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

: box[] ( o -- o )
    >o box-actor new to act o act >o to caller-w o> o o> ;
