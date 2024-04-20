\ add structure values to Forth 2012 structs

\ Authors: Bernd Paysan, Anton Ertl
\ Copyright (C) 2014,2016,2017,2018,2019,2022,2023 Free Software Foundation, Inc.

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

:noname ( -- )
    defers standard:field ['] + IS +field, ; is standard:field

standard:field

\ The xt to create the actual code with is at the second cell
\ The actual offset in the first cell, which will be used by that code
\ in the second cell...
: vfield-int, ( addr body xt -- addr+offset ) >r 2@ swap execute r> execute ;
: vfield-comp, ( body -- ) lits> >r 2@ >lits r> 2compile, ;

: create+value ( n1 addr "name" -- n3 )
    >r r@ 2 cells + perform
    r> 2@ create-from reveal over , + action-of +field, , ;

: field-to:exec ( .. u xt1 xt2 -- .. )
    rot >r @ r> cells + @ vfield-int, ;
: field-to:,  ( u xt2 -- )
    @ swap cells + @ lits> swap >lits vfield-comp, ;

: field-to-method: ( !-table -- )
    Create , ['] field-to:exec set-does> ['] field-to:, set-optimizer ;

: wrapper-xts ( xt@ !-table "name" -- dummy-xt ) { xt@ xt! }
    :noname xt@ >lits ]] vfield-int, [[ postpone ; >r \ xt-does
    :noname xt@ >lits ]] >lits vfield-comp, [[ postpone ; >r \ xt-comp,
    xt! noname field-to-method: latestxt >r \ xt-to
    \ create a dummy word with these methods
    >in @ >r parse-name r> >in ! 2dup + 1- c@ ':' = +
    [: type ." -dummy" ;] $tmp
    nextname Create r> r> r> set-does> set-optimizer set-to latestxt
;

: wrap+value: ( n2 xt-align xt@ !-table "name" -- )
    wrapper-xts
    Create , swap , ,
    DOES> create+value ;

: w+! ( w addr -- ) dup >r w@ + r> w! ;
: l+! ( w addr -- ) dup >r l@ + r> l! ;
: sf+! ( w addr -- ) dup sf@ f+ sf! ;
: df+! ( w addr -- ) dup df@ f+ df! ;
: sc@ ( addr -- c ) c@ c>s ;
opt: drop ]] c@ c>s [[ ;
: $[]-@ ( n addr -- x ) $[] @ ;
: $[]-! ( n addr -- x ) $[] ! ;
: $[]-+! ( n addr -- x ) $[] +! ;

to-table: w!a-table  w! w+! [noop]
to-table: l!a-table  l! l+! [noop]
to-table: sf!a-table sf! sf+! [noop]
to-table: df!a-table df! df+! [noop]
to-table: $!a-table  $! $+! [noop]
to-table: $[]!a-table $[]! $[]+! [noop]
to-table: $[]-!a-table $[]-! $[]-+! [noop]

[IFUNDEF] !a-table
    !-table >to+addr-table: !a-table
    defer-table >to+addr-table: defera-table
    2!-table >to+addr-table: 2!a-table
    c!-table >to+addr-table: c!a-table
    f!-table >to+addr-table: f!a-table
[THEN]

cell      ' aligned   ' @   !a-table   wrap+value: value:   ( u1 "name" -- u2 )
1         ' noop      ' c@  c!a-table  wrap+value: cvalue:  ( u1 "name" -- u2 )
2         ' waligned  ' w@  w!a-table  wrap+value: wvalue:  ( u1 "name" -- u2 )
4         ' laligned  ' l@  l!a-table  wrap+value: lvalue:  ( u1 "name" -- u2 )
0 warnings !@
1         ' noop      ' sc@ c!a-table  wrap+value: scvalue:  ( u1 "name" -- u2 )
2         ' waligned  ' sw@ w!a-table  wrap+value: swvalue: ( u1 "name" -- u2 )
4         ' laligned  ' sl@ l!a-table  wrap+value: slvalue: ( u1 "name" -- u2 )
warnings ! \ yes, these are obsolete, but they are good that way
2 cells   ' aligned   ' 2@  2!a-table  wrap+value: 2value:  ( u1 "name" -- u2 )
1 floats  ' faligned  ' f@  f!a-table  wrap+value: fvalue:  ( u1 "name" -- u2 )
1 sfloats ' sfaligned ' sf@ sf!a-table wrap+value: sfvalue: ( u1 "name" -- u2 )
1 dfloats ' dfaligned ' df@ df!a-table wrap+value: dfvalue: ( u1 "name" -- u2 )
[IFDEF] z@
    1 complex' ' dfaligned ' z@ z!a-table wrap+value: zvalue: ( u1 "name" -- u2 )
[THEN]
cell      ' aligned   ' $@  $!a-table  wrap+value: $value:  ( u1 "name" -- u2 )
cell      ' aligned   ' perform defera-table wrap+value: defer: ( u1 "name" -- u2 )
cell      ' aligned   ' $[]-@ $[]-!a-table wrap+value: value[]: ( u1 "name" -- u2 )
cell      ' aligned   ' $[]@ $[]!a-table wrap+value: $value[]: ( u1 "name" -- u2 )

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
