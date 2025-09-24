\ add structure values to Forth 2012 structs

\ Authors: Bernd Paysan, Anton Ertl
\ Copyright (C) 2014,2016,2017,2018,2019,2022,2023,2024 Free Software Foundation, Inc.

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
    >r r@ 2 th@ execute
    r> 2@ create-from reveal
    over , + action-of +field, , ;

noname Create latestnt >r \ field-to-class template
DOES> ( .. u xt1 xt2 -- .. )
    rot >r @ r> th@ vfield-int, ;
opt: ( u xt2 -- )
    @ swap th@ lits> swap >lits vfield-comp, ;

: wrapper-xts ( xt@ !-table "name" -- dummy-xt ) { xt@ xt! }
    :noname xt@ >lits ]] vfield-int, [[ postpone ; >r \ xt-does
    :noname xt@ >lits ]] >lits vfield-comp, [[ postpone ; >r \ xt-comp,
    xt! [ r> ]L noname-from , latestxt >r \ xt-to
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
inline: sc@  ( c-addr -- n ) ]]  c@  c>s [[ ;inline
inline: sw@  ( c-addr -- n ) ]]  w@  w>s [[ ;inline
inline: sl@  ( c-addr -- n ) ]]  l@  l>s [[ ;inline
[IFDEF] x@
inline: sx@  ( c-addr -- n ) ]]  x@  x>s [[ ;inline
[THEN]
: $[]-@ ( n addr -- x ) $[] @ ;
: $[]-! ( n addr -- x ) $[] ! ;
: $[]-+! ( n addr -- x ) $[] +! ;

to-table: w!-table  w! w+!
to-table: l!-table  l! l+!
to-table: sf!-table sf! sf+!
to-table: df!-table df! df+!
to-table: $!-table  $! $+!
to-table: $[]!-table $[]! $[]+!
to-table: $[]-!-table $[]-! $[]-+!

cell      ' aligned   ' @   !-table   wrap+value: value:   ( u1 "name" -- u2 ) \ gforth-experimental
\G @i{Name} is a value-flavoured field; in-memory-size: cell; on-stack: cell
1         ' noop      ' c@  c!-table  wrap+value: cvalue:  ( u1 "name" -- u2 ) \ gforth-experimental
\G @i{Name} is a value-flavoured field; in-memory-size: char; on-stack: unsigned cell
2         ' waligned  ' w@  w!-table  wrap+value: wvalue:  ( u1 "name" -- u2 ) \ gforth-experimental
\G @i{Name} is a value-flavoured field; in-memory-size: 16 bits; on-stack: unsigned cell
4         ' laligned  ' l@  l!-table  wrap+value: lvalue:  ( u1 "name" -- u2 ) \ gforth-experimental
\G @i{Name} is a value-flavoured field; in-memory-size: 32 bits; on-stack: unsigned cell
0 warnings !@
1         ' noop      ' sc@ c!-table  wrap+value: scvalue: ( u1 "name" -- u2 ) \ gforth-experimental
\G @i{Name} is a value-flavoured field; in-memory-size: char; on-stack: signed cell
2         ' waligned  ' sw@ w!-table  wrap+value: swvalue: ( u1 "name" -- u2 ) \ gforth-experimental
\G @i{Name} is a value-flavoured field; in-memory-size: 16 bits; on-stack: signed cell
4         ' laligned  ' sl@ l!-table  wrap+value: slvalue: ( u1 "name" -- u2 ) \ gforth-experimental
\G @i{Name} is a value-flavoured field; in-memory-size: 32 bits; on-stack: signed cell
warnings ! \ yes, these are obsolete, but they are good that way
2 cells   ' aligned   ' 2@  2!-table  wrap+value: 2value:  ( u1 "name" -- u2 ) \ gforth-experimental
\G @i{Name} is a value-flavoured field; in-memory-size: 2 cells;
\G on-stack: 2 cells; @code{+to} performs double-cell addition
\G (@code{d+}).
1 floats  ' faligned  ' f@  f!-table  wrap+value: fvalue:  ( u1 "name" -- u2 ) \ gforth-experimental
\G @i{Name} is a value-flavoured field; in-memory-size: float; on-stack: float
1 sfloats ' sfaligned ' sf@ sf!-table wrap+value: sfvalue: ( u1 "name" -- u2 ) \ gforth-experimental
\G @i{Name} is a value-flavoured field; in-memory-size: 32-bit float; on-stack: float
1 dfloats ' dfaligned ' df@ df!-table wrap+value: dfvalue: ( u1 "name" -- u2 ) \ gforth-experimental
\G @i{Name} is a value-flavoured field; in-memory-size: 64-bit float; on-stack: float
[IFDEF] z@
    1 complex' ' dfaligned ' z@ z!-table wrap+value: zvalue: ( u1 "name" -- u2 ) \ gforth-experimental
    \G @i{Name} is a value-flavoured field; in-memory-size: 2 floats;
    \G on-stack: 2 floats; @code{+to} performs componentwise addition.
[THEN]
cell      ' aligned   ' $@  $!-table       wrap+value: $value: ( u1 "name" -- u2 ) \ gforth-experimental
\G @i{Name} is a value-flavoured field; in-memory-size: cell;
\G on-stack: c-addr u (@pxref{$tring words}); @code{( c-addr u ) +to
\G @i{name}} appends c-addr u to the string in the field.
cell      ' aligned   ' perform defer-table wrap+value: defer: ( u1 "name" -- u2 ) \ gforth-experimental
\G @i{Name} is a defer-flavoured field
cell      ' aligned   ' $[]-@ $[]-!-table wrap+value: value[]: ( u1 "name" -- u2 ) \ gforth-experimental
\G @i{Name} is a value-flavoured array field; in-memory-size: cell; on-stack: cell
cell      ' aligned   ' $[]@ $[]!-table  wrap+value: $value[]: ( u1 "name" -- u2 ) \ gforth-experimental
\G @i{Name} is a value-flavoured array field; in-memory-size: cell;
\G on-stack: c-addr u (@pxref{$tring words}); @code{( c-addr u ) +to
\G @i{name}} appends c-addr u to the string in the array element.

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
    $value: o
    defer: p
    value[]: q
    $value[]: r
    end-structure
    foo buffer: test
[THEN]
