\ MINOS2 actors on Wayland

\ Author: Bernd Paysan
\ Copyright (C) 2017,2019,2023,2024,2025 Free Software Foundation, Inc.

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

require bits.fs

Variable buttonmask
Variable flags
0 Value clicks

0 Constant #pending
1 Constant #lastdown
2 Constant #clearme

Variable ev-time
2Variable ev-xy
Variable ev-button
Variable ev-up/down
2Variable lastpos
Variable lasttime

\ handle scrolling

:is b-scroll ( time axis val -- )
    rot dup lasttime !@ - twoclicks u<
    IF  1 +to clicks  clicks *  ELSE  0 to clicks  THEN  #-60 /
    ev-xy 2@ swap coord>f coord>f top-act .scrolled ;

\ handle clicks

: samepos? ( x y -- flag )
    lastpos 2@ rot - dup m* 2swap - dup m* d+ samepos $10000 m* d< ;
: ?samepos ( -- )
    ev-xy 2@
    2dup samepos? 0= IF   0 to clicks  THEN  lastpos 2! ;
: send-clicks ( -- )
    lastpos 2@ swap coord>f coord>f buttonmask l@ lle
    clicks 2* flags #lastdown bit@ -
    flags #pending -bit
    grab-move? ?dup-IF  gxy-sum z+ [: .clicked ;] vp-needed<>| EXIT  THEN
    top-act    ?dup-IF  .clicked  EXIT  THEN
    2drop fdrop fdrop ;

Variable xy$
: >xy$ ( x1 y1 .. xn yn n -- $rxy )
    2* sfloats xy$ $!len
    xy$ $@ bounds 1 sfloats - swap 1 sfloats - U-DO
	coord>f I sf!
    1 sfloats -LOOP
    xy$ ;

:is ?looper-timeouts ( -- )
    XTime lasttime @ - twoclicks >= IF
	flags #pending -bit@ IF
	    send-clicks
	THEN
	flags #clearme -bit@ IF
	    0 to clicks
	THEN
    THEN ;

Create >button 0 c, 2 c, 1 c, 7 c, 8 c, 3 c, 4 c, 5 c,
DOES> + c@ ;

:is b-button { time b mask -- }
    event( ." button event: mask=" mask h. ." button=" b h. cr )
    mask IF  \ button pressend
	buttonmask b 7 and >button +bit
	top-act IF  ev-xy 2@ 1 >xy$ buttonmask l@ lle top-act .touchdown  THEN
	time lasttime !  ?samepos
	flags #lastdown +bit  flags #pending +bit
    ELSE \ button released
	?samepos  time lasttime !
	flags #lastdown -bit@  IF
	    event( ." send downclick" cr )
	    1 +to clicks  send-clicks  flags #clearme +bit  THEN
	buttonmask b 7 and >button -bit
	top-act IF  ev-xy 2@ 1 >xy$ buttonmask l@ lle top-act .touchup  THEN
    THEN
;

:is b-motion ( time x y -- )
    ev-xy 2!  ev-time !
    flags #pending bit@  ev-xy 2@ samepos? 0= and IF
	send-clicks  0 to clicks
    THEN
    grab-move? IF  ev-xy 2@ 1 >xy$ >dxy buttonmask l@ lle
	[: grab-move? .touchmove ;] vp-needed<>|  EXIT
    THEN
    top-act IF  ev-xy 2@ 1 >xy$ buttonmask l@ lle top-act .touchmove  THEN
;

:is dnd-move ( x y -- )
    swap coord>f coord>f
    top-act ?dup-IF  .dndmove  ELSE  fdrop fdrop  THEN
;

:is dnd-drop ( x y addr u -- )
    2swap swap coord>f coord>f
    top-act ?dup-IF  .dnddrop  ELSE  2drop fdrop fdrop  THEN
;

\ key handling

4 buffer: xstring

: >xstring ( xchar -- addr u )
    xstring xc!+ xstring tuck - ;
: ctrls? ( addr u -- flag )
    false -rot bounds ?DO
	I c@ bl < or \ all UTF-8 codepoints are >= bl
	I c@ #del = or \ and <> del
    LOOP ;
: u/ekeyed ( ekey -- )
    wayland( [: cr ." ekey: " dup h. ;] do-debug )
    dup 0= IF  drop  EXIT  THEN
    case
	#del of  k-delete     wl-meta mask-shift# lshift or  endof
	#bs  of  k-backspace  wl-meta mask-shift# lshift or  endof
	#lf  of  k-enter      wl-meta mask-shift# lshift or  endof
	#cr  of  k-enter      wl-meta mask-shift# lshift or  endof
    dup endcase
    dup bl keycode-start within over #del <> and
    IF    $1000000 invert and >xstring top-act .ukeyed
    ELSE  top-act .ekeyed  THEN ;
: ctrl-keyed ( addr u -- )
    bounds ?DO  I xc@+ swap >r u/ekeyed  r> I -  +LOOP ;
: u/keyed ( addr u -- )
    wayland( [: cr ." u/keys: " 2dup dump ;] do-debug )
    2dup ctrls? IF
	ctrl-keyed
    ELSE
	top-act .ukeyed
    THEN ;
: keys-commit ( addr u -- )
    wayland( [: cr ." keys: " 2dup dump ;] do-debug )
    2dup ctrls? IF
	ctrl-keyed
    ELSE
	top-act .ukeyed
    THEN ;

\ enter and leave

: enter-minos ( -- )
    ['] keys-commit is wayland-keys
    ['] u/ekeyed is wl-ekeyed
    ['] u/keyed is wl-ukeyed
    edit-widget edit-out ! ;
: leave-minos ( -- )
    preserve wayland-keys
    preserve wl-ekeyed
    preserve wl-ukeyed
    edit-terminal edit-out !
    +sync  +show ;
