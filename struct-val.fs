\ add structure values to Forth 2012 structs

\ Authors: Bernd Paysan, Anton Ertl
\ Copyright (C) 2014,2016,2017,2018,2019 Free Software Foundation, Inc.

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

Defer +field,

: standard+field, ( addr body -- addr' )
    @ + ;
opt: drop @ ?dup-IF
	lits# 0> IF  lits> + >lits
	ELSE  ['] lit+ peephole-compile, ,  THEN
    THEN ;

:noname ( -- )
    defers standard:field ['] standard+field, IS +field, ; is standard:field

standard:field

: vfield-int, ( addr body -- addr+offset ) dup cell+ @ execute ;
: vfield-comp, ( body -- ) dup cell+ @ opt-compile, ;

: create+value ( n1 addr "name" -- n3 )
    >r r@ cell+ cell+ 2@ r> 2@
    2>r >r Create over , + action-of +field, ,
    r> set-does> 2r> set-to set-optimizer ;

: create+defer ( n1 addr "name" -- n3 )
    create+value
    [: ( addr -- xt ) >body vfield-int, @ ;
    defer@-opt: ( xt -- ) >body vfield-comp, postpone @ ;] set-defer@ ;

: vfield-to: ( xt! -- )
    Create ,
    [: ( xt body -- ) >r >body vfield-int, r> @ to-!exec ;] set-does>
    [: ( xt -- ) >r lits# 0= IF  r> does,  EXIT  THEN
	lits> >body vfield-comp, r> >body @ to-!, ;] set-optimizer ;

: wrapper-xts ( xt@ !-table -- xt-does xt-opt xt-to ) { xt@ xt! }
    :noname ]] vfield-int, [[ xt@ compile, postpone ; \ xt-does
    :noname ]] >body vfield-comp, [[ xt@ lit, ]] compile, ; [[ \ xt-comp,
    xt! noname vfield-to: latestxt ;

: wrap+value: ( n2 xt-align xt@ !-table "name" -- ) rot { xt-align }
    wrapper-xts :noname ]] >r [[ xt-align compile, ]] r> create+value ; [[
    Create set-does> , , , , ;
: wrap+defer: ( n2 xt-align xt@ !-table "name" -- ) rot { xt-align }
    wrapper-xts :noname ]] >r [[ xt-align compile, ]] r> create+defer ; [[
    Create set-does> , , , , ;

: w+! ( w addr -- ) dup >r w@ + r> w! ;
: l+! ( w addr -- ) dup >r l@ + r> l! ;
: sf+! ( w addr -- ) dup >r sf@ f+ r> sf! ;
: df+! ( w addr -- ) dup >r df@ f+ r> df! ;
: sc@ ( addr -- c ) c@ c>s ;
opt: drop ]] c@ c>s [[ ;
: $[]-@ ( n addr -- x ) $[] @ ;
: $[]-! ( n addr -- x ) $[] ! ;
: $[]-+! ( n addr -- x ) $[] +! ;

Create w!-table  ' w!  , ' w+!  ,
Create l!-table  ' l!  , ' l+!  ,
Create sf!-table ' sf! , ' sf+! ,
Create df!-table ' df! , ' df+! ,
Create $!-table  ' $!  , ' $+!  ,
Create $[]!-table ' $[]! , ' $[]+! ,
Create $[]-!-table ' $[]-! , ' $[]-+! ,

cell      ' aligned   ' @   !-table   wrap+value: value:   ( u1 "name" -- u2 )
1         ' noop      ' c@  c!-table  wrap+value: cvalue:  ( u1 "name" -- u2 )
1         ' noop      ' sc@ c!-table  wrap+value: scvalue:  ( u1 "name" -- u2 )
2         ' waligned  ' w@  w!-table  wrap+value: wvalue:  ( u1 "name" -- u2 )
2         ' waligned  ' sw@ w!-table  wrap+value: swvalue: ( u1 "name" -- u2 )
4         ' laligned  ' l@  l!-table  wrap+value: lvalue:  ( u1 "name" -- u2 )
4         ' laligned  ' sl@ l!-table  wrap+value: slvalue: ( u1 "name" -- u2 )
2 cells   ' aligned   ' 2@  2!-table  wrap+value: 2value:  ( u1 "name" -- u2 )
1 floats  ' faligned  ' f@  f!-table  wrap+value: fvalue:  ( u1 "name" -- u2 )
1 sfloats ' sfaligned ' sf@ sf!-table wrap+value: sfvalue: ( u1 "name" -- u2 )
1 dfloats ' dfaligned ' df@ df!-table wrap+value: dfvalue: ( u1 "name" -- u2 )
cell      ' aligned   ' $@  $!-table  wrap+value: $value:  ( u1 "name" -- u2 )
cell      ' aligned   ' perform !-table wrap+defer: defer: ( u1 "name" -- u2 )
cell      ' aligned   ' $[]-@ $[]-!-table wrap+value: value[]: ( u1 "name" -- u2 )
cell      ' aligned   ' $[]@ $[]!-table wrap+value: $value[]: ( u1 "name" -- u2 )

0 [IF] \ test
    begin-structure foo
    value: a
    cvalue: b
    cvalue: c
    value: d
    fvalue: e
    wvalue: f
    swvalue: g
    lvalue: h
    slvalue: l
    sfvalue: m
    dfvalue: n
    end-structure
    foo buffer: test
[THEN]
