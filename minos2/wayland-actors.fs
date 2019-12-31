\ MINOS2 actors on Wayland

\ Author: Bernd Paysan
\ Copyright (C) 2017,2019 Free Software Foundation, Inc.

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

#200 Value twoclicks  \ every edge further apart than 150ms into separate clicks
$60000. 2Value samepos      \ position difference square-summed less than is same pos

1e 256e f/ fconstant 1/256

\ handle scrolling

:noname ( time axis val -- )
    rot ev-time ! top-act .scrolled ; IS b-scroll

\ handle clicks

: samepos? ( x y -- flag )
    lastpos 2@ rot - dup m* 2swap - dup m* d+ samepos d< ;
: ?samepos ( -- )
    ev-xy 2@
    2dup samepos? 0= IF   0 to clicks  THEN  lastpos 2! ;
: send-clicks ( -- )
    lastpos 2@ swap 1/256 fm* 1/256 fm* buttonmask le-ul@
    clicks 2* flags #lastdown bit@ -
    flags #pending -bit
    grab-move? ?dup-IF  .clicked  EXIT  THEN
    top-act    ?dup-IF  .clicked  EXIT  THEN
    2drop fdrop fdrop ;

: >xy$ ( x1 y1 .. xn yn n -- $rxy )
    2* sfloats xy$ $!len
    xy$ $@ bounds 1 sfloats - swap 1 sfloats - U-DO
	1/256 fm* I sf!
    1 sfloats -LOOP
    xy$ ;

:noname ( -- )
    Xtime lasttime @ - twoclicks >= IF
	flags #pending -bit@ IF
	    send-clicks
	THEN
	flags #clearme -bit@ IF
	    0 to clicks
	THEN
    THEN ; is ?looper-timeouts

Create >button 0 c, 2 c, 1 c, 3 c, 4 c, 6 c, 5 c, 7 c,
DOES> + c@ ;

:noname { time b mask -- }
    mask IF  \ button pressend
	buttonmask b 7 and >button +bit
	top-act IF  ev-xy 2@ 1 >xy$ buttonmask le-ul@ top-act .touchdown  THEN
	time lasttime !  ?samepos
	flags #lastdown +bit  flags #pending +bit
    ELSE \ button released
	?samepos  time lasttime !
	flags #lastdown -bit@  IF
	    1 +to clicks  send-clicks  flags #clearme +bit  THEN
	buttonmask b 7 and >button -bit
	top-act IF ev-xy 2@ 1 >xy$ buttonmask le-ul@ top-act .touchup  THEN
    THEN
; is b-button

:noname ( time x y -- )
    ev-xy 2!  ev-time !
    flags #pending bit@  ev-xy 2@ samepos? 0= and IF
	send-clicks  0 to clicks
    THEN
    top-act IF  ev-xy 2@ 1 >xy$ buttonmask le-ul@ top-act .touchmove  THEN
; is b-motion

\ enter and leave

: enter-minos ( -- )
    edit-widget edit-out ! ;
: leave-minos ( -- )
    edit-terminal edit-out !
    +sync  +show ;
