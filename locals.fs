\ Local primitives                                      17jan92py

\ Copyright (C) 1995,2000,2003,2007 Free Software Foundation, Inc.

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

Variable loffset   0 loffset !
Variable locals  here locals !  100 ( some) cells allot
: local, ( offset -- )  postpone rp@ loffset @ swap -
  postpone Literal postpone + ;
: delocal, ( offset -- ) local, postpone rp! ;
: (local  DOES>  @ local, postpone @ ;
: f>r  r> rp@ 1 floats - dup rp! f! >r ;
: (flocal DOES>  @ local, postpone f@ ;

: do-nothing ;
: ralign  r>
  BEGIN  rp@ [ 1 floats 1- ] Literal and
         WHILE  [ ' do-nothing >body ] ALiteral >r
  REPEAT  >r ;

: <local ( -- sys1 )  current @ @ loffset @ locals @
  over 0= IF  postpone  ralign  THEN  ;                 immediate
: local: ( -- )  postpone >r  latest latestxt here locals @ dp !
  cell loffset +! Create  loffset @ , immediate (local
  here locals !  dp !  lastcfa ! last ! ;               immediate
: flocal: ( -- )  latest latestxt here locals @ dp !
  BEGIN  loffset @ 0 1 floats fm/mod drop  WHILE
         0 postpone Literal postpone >r  1 cells  loffset +!  REPEAT
  postpone f>r  Create  loffset @ , immediate (flocal
  here locals !  dp !  lastcfa ! last ! ;               immediate
: local> ( sys1 -- sys2 ) ;                             immediate
: local; ( sys2 -- ) locals ! dup delocal,
  loffset ! current @ ! ;                               immediate
: TO  >in @ ' dup @ [ ' (local >body cell+ ] ALiteral =
  IF    >body @ local, postpone ! drop
  ELSE  dup @ [ ' (flocal >body cell+ ] ALiteral =
        IF    >body @ local, postpone f!  drop
        ELSE  drop >in ! postpone to  THEN THEN ;       immediate

: DO      2 cells loffset +!  postpone DO     ; immediate restrict
: ?DO     2 cells loffset +!  postpone ?DO    ; immediate restrict
: FOR     2 cells loffset +!  postpone FOR    ; immediate restrict
: LOOP   -2 cells loffset +!  postpone LOOP   ; immediate restrict
: +LOOP  -2 cells loffset +!  postpone +LOOP  ; immediate restrict
: NEXT   -2 cells loffset +!  postpone NEXT   ; immediate restrict
: >R      1 cells loffset +!  postpone >R     ; immediate restrict
: R>     -1 cells loffset +!  postpone R>     ; immediate restrict

\ High level locals                                    19aug93py

: { postpone <local  -1
  BEGIN  >in @ name dup c@ 1 = swap 1+ c@ '| = and  UNTIL
  drop >in @ >r
  BEGIN  dup 0< 0= WHILE  >in ! postpone local:  REPEAT  drop
  r> >in ! postpone local> ;                  immediate restrict

: F{ postpone <local  -1
  BEGIN  >in @ name dup c@ 1 = swap 1+ c@ '| = and  UNTIL
  drop >in @ >r
  BEGIN  dup 0< 0= WHILE  >in ! postpone Flocal:  REPEAT  drop
  r> >in ! postpone local> ;                  immediate restrict

' local; alias } immediate restrict

\ ANS Locals                                           19aug93py

Create inlocal  5 cells allot  inlocal off
: (local)  ( addr u -- )  inlocal @ 0=
  IF  postpone <local inlocal on
      inlocal 3 cells + 2!  inlocal cell+ 2! THEN
  dup IF    linestart @ >r sourceline# >r loadfile @ >r
            blk @ >r >tib @ >r  #tib @ dup >r  >in @ >r

            >tib +! dup #tib ! >tib @ swap move
            >in off blk off loadfile off -1 linestart !

            postpone local:

            r> >in !  r> #tib !  r> >tib ! r> blk !
            r> loadfile ! r> loadline ! r> linestart !
      ELSE  2drop  inlocal cell+ 2@  inlocal 3 cells + 2@
            postpone local>
            inlocal 2 cells + 2! inlocal cell+ ! THEN ;

: ?local;  inlocal @
  IF  inlocal cell+ @ inlocal 2 cells + 2@
      postpone local; inlocal off  THEN ;

: ;      ?local; postpone ; ;                 immediate restrict
: DOES>  ?local; postpone DOES> ;             immediate
: EXIT  inlocal @ IF  0 delocal,  THEN  postpone EXIT ; immediate

: locals|
  BEGIN  name dup c@ 1 = over 1+ c@ '| = and 0=  WHILE
         count (local)  REPEAT  0 (local) ;   immediate restrict
