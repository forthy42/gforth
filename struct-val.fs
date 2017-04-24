\ add structure values to Forth 2012 structs

\ Copyright (C) 2014,2016 Free Software Foundation, Inc.

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
comp: drop @ ?dup-IF ['] lit+ peephole-compile, , THEN ;

warnings @ warnings off
: standard:field ( -- )
    standard:field ['] standard+field, IS +field, ;
warnings !

standard:field

: vfield-int, ( addr body -- offset ) dup cell+ @ to-!exec ;
: vfield-comp, ( body -- ) dup cell+ @ to-!, ;

: create+value ( n1 addr "name" -- n3 )
    >r r@ cell+ cell+ 2@ r> 2@
    2>r >r Create over , + action-of +field, ,
    r> set-does> 2r> set-to set-optimizer ;

: wrap+value: ( n2 xt-align xt@ xt!-table "name" -- ) { xt-align xt@ xt! }
    :noname ]] vfield-int, [[ xt@ compile, postpone ; \ xt-does
    :noname ]] >body vfield-comp, [[ xt@ ]]L compile, ; [[ \ xt-comp,
    :noname ]] drop >body vfield-comp, [[ xt! ]]L compile, ; [[ \ xt-to-comp,
    :noname ]] >body vfield-int, [[ xt! compile, postpone ; swap set-optimizer \ xt-to
    :noname ]] >r [[ xt-align compile, ]] r> create+value ; [[
    Create set-does> , , , , ;

: w+! ( w addr -- ) dup >r w@ + r> w! ;
: l+! ( w addr -- ) dup >r l@ + r> l! ;
: sf+! ( w addr -- ) dup >r sf@ f+ r> sf! ;
: df+! ( w addr -- ) dup >r df@ f+ r> df! ;

Create w!-table ' w! , ' w+! ,
Create l!-table ' l! , ' l+! ,
Create sf!-table ' sf! , ' sf+! ,
Create df!-table ' df! , ' df+! ,
Create $!-table ' $! , ' $+! ,

cell      ' aligned   ' @   ' !-table   wrap+value: value:   ( u1 "name" -- u2 )
1         ' noop      ' c@  ' c!-table  wrap+value: cvalue:  ( u1 "name" -- u2 )
2         ' waligned  ' w@  ' w!-table  wrap+value: wvalue:  ( u1 "name" -- u2 )
2         ' waligned  ' sw@ ' w!-table  wrap+value: swvalue: ( u1 "name" -- u2 )
4         ' laligned  ' l@  ' l!-table  wrap+value: lvalue:  ( u1 "name" -- u2 )
4         ' laligned  ' sl@ ' l!-table  wrap+value: slvalue: ( u1 "name" -- u2 )
2 cells   ' aligned   ' 2@  ' 2!-table  wrap+value: 2value:  ( u1 "name" -- u2 )
1 floats  ' faligned  ' f@  ' f!-table  wrap+value: fvalue:  ( u1 "name" -- u2 )
1 sfloats ' sfaligned ' sf@ ' sf!-table wrap+value: sfvalue: ( u1 "name" -- u2 )
1 dfloats ' dfaligned ' df@ ' df!-table wrap+value: dfvalue: ( u1 "name" -- u2 )
cell      ' aligned   ' $@  ' $!-table  wrap+value: $value:  ( u1 "name" -- u2 )

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
