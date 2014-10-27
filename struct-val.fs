\ add structure values to Forth 2012 structs

\ Copyright (C) 2014 Free Software Foundation, Inc.

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

: create+value ( n1 addr "name" -- n3 )
    >r r@ cell+ cell+ 2@ r> 2@
    2>r >r Create over , +
    r> 2r> set-to set-compiler set-does> ;

: wrap+value: ( n2 xt-align xt@ xt! "name" -- ) { xt-align xt@ xt! }
    :noname ]] @ + [[ xt@ compile, postpone ; \ xt-does
    :noname postpone field+, xt@ ]] literal compile, ; [[ \ xt-comp,
    :noname ]] drop field+, [[ xt! ]] literal compile, ; [[ \ xt-to-comp,
    :noname ]] >body @ + [[ xt! compile, postpone ; swap set-compiler \ xt-to
    :noname ]] >r [[ xt-align compile, ]] r> create+value ; [[
    Create set-does> , , , , ;

: waligned ( addr -- waddr ) 1+ -2 and ;
: laligned ( addr -- waddr ) 3 + -4 and ;

cell ' aligned ' @ ' ! wrap+value: value:
1 ' noop ' c@ ' c! wrap+value: cvalue:
2 ' waligned ' w@ ' w! wrap+value: wvalue:
2 ' waligned ' sw@ ' w! wrap+value: swvalue:
4 ' laligned ' l@ ' l! wrap+value: lvalue:
4 ' laligned ' sl@ ' l! wrap+value: slvalue:
2 cells ' aligned ' 2@ ' 2! wrap+value: 2value:
1 floats ' faligned ' f@ ' f! wrap+value: fvalue:
1 sfloats ' sfaligned ' sf@ ' sf! wrap+value: sfvalue:
1 dfloats ' dfaligned ' df@ ' df! wrap+value: dfvalue:

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