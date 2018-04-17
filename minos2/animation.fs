\ MINOS2 animations

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

\ animations are changing variables over time

Variable anims[]

object class
    fvalue: ani-start
    fvalue: ani-delta
    value: ani-addr
    defer: animate ( addr rstate -- )
end-class animation

: new-anim-time ( rdelta rtime addr xt -- o )    
    animation new >o to ani-start to ani-delta is animate to ani-addr o o> ;
: new-anim ( rdelta addr xt -- o )
    ftime new-anim-time ;
: >animate ( rdelta addr xt -- )
    new-anim anims[] >stack ;

: anim-t ( time -- r0..1 flag ) ani-start f- ani-delta fabs f/
    fdup 1e f< dup 0= IF  fdrop 1e  THEN
    ani-delta f0< IF  1e fswap f-  THEN ;
: anims@ ( -- anim1 .. animn n )
    anims[] get-stack anims[] $free ;
: animations ( -- ) time( ." anim:  " .!time cr ) 
    ftime { f: now }
    anims@ 0 ?DO
	>o now anim-t IF
	    o anims[] >stack ani-addr animate
	ELSE  ani-addr animate dispose  THEN  o>
    LOOP ;
: anim-start ( -- )
    anims@ 0 ?DO
	>o ani-delta f0< IF  1e  ELSE  0e  THEN
	ani-addr animate o>
    LOOP ;
: anim-end ( -- )
    anims@ 0 ?DO
	>o ani-delta f0< IF  0e  ELSE  1e  THEN
	ani-addr animate dispose o>
    LOOP ;

\ edit animation

0.5e fvalue *instime

: *anim-ins ( addr -- )
    1e f= IF  *insflag off +sync  THEN  drop ;
: *anim-end ( -- )
    anims@ 0 ?DO
	>o action-of animate ['] *anim-ins = IF  dispose  THEN  o>
    LOOP ;
:noname ( -- )
    *anim-end *insflag on
    *instime *insflag ['] *anim-ins >animate ;
is anim-ins

\ helper for animation

: sin-t ( r0..1 -- r0..1 ) \ sinusoidal movement
    pi f* fcos f2/ 0.5e fswap f- ;
: sin-at ( r0..r1 -- r0..1 ) \ accellerating
    pi f2/ f* fcos 1e fswap f- ;
: sin-dt ( r0..r1 -- r0..1 ) \ decellerating
    pi f2/ f* fsin ;

\ often used animation

: fade ( r max -- )
    fm* f>s text-color $FF mux to text-color ;
