\ Mini-OOF2, using current object+Gforth primitives    09jan12py

\ Copyright (C) 2012,2014,2015 Free Software Foundation, Inc.

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

require struct-val.fs
require rec-scope.fs

Defer default-method ' noop IS default-method

\ template for methods and ivars

Create o 0 ,  DOES> @ o#+ [ 0 , ] + ;
comp: >body @ postpone o#+ , ;
: to-m >body @ + ! ;
Create m 0 ,  DOES> @ o#+ [ -1 cells , ] @ + perform ;
comp: >body @ cell/ postpone o#exec , ;
' to-m set-to
' o Value var-xt
' m Value method-xt
: current-o  ['] o to var-xt  ['] m to method-xt ;

\ ivalues

: o+field, ( addr body -- addr' )
    @ o + ;
comp: drop @ postpone o#+ , ;

\ core system

-2 cells    field: >osize    field: >methods   drop
: method ( m v size "name" -- m' v )
  Header reveal method-xt vtcopy,  over , swap cell+ swap ;
: var ( m v size "name" -- m v' )
  Header reveal    var-xt vtcopy,  over , dup , ( for sizeof ) + ;
: class ( class -- class methods vars )
  dup >osize 2@ ['] var IS +field  ['] o+field, IS +field, ;
: end-class  ( class methods vars "name" -- )
  , dup , here >r 0 U+DO ['] default-method defer@ , cell +LOOP
  dup r@ swap >methods @ move  standard:field
  r> Value ;
: >vt ( class "name" -- addr )  ' >body @ + ;
: :: ( class "name" -- ) >vt @ compile, ;
0 cells , 0 cells ,  here Value object

\ memory allocation

object class
    method :allocate
    method :free
end-class storage

storage class end-class static-alloc
storage class end-class dynamic-alloc

:noname  ( len -- addr )  here swap allot ; static-alloc to :allocate
:noname  ( addr -- )      drop ;            static-alloc to :free

:noname  ( len -- addr )  allocate throw ; dynamic-alloc to :allocate
:noname  ( addr -- )      free throw ;     dynamic-alloc to :free

static-alloc dup >osize @ cell+ here swap allot swap over ! cell+ Constant static-a
static-a Value allocater

: new ( class -- o )  dup >osize @ cell+
    allocater >o :allocate o> swap over !
    cell+ dup dup cell- @ >osize @ erase ;
: dispose ( o:o -- o:0 )  o cell- dup dup @ >osize @ cell+ erase
    allocater >o :free o>  0 >o rdrop ;

dynamic-alloc new Constant dynamic-a
dynamic-a to allocater

: with-allocater ( xt allocater -- )
    allocater >r  to allocater  catch  r> to allocater  throw ;

\ building blocks for dynamic methods

: class>count ( addr -- addr' u ) >osize dup cell+ @ 2 cells + ;
: >dynamic ( class -- class' ) class>count save-mem drop 2 cells + ;
: >static ( class -- class' ) here >r class>count
    over swap dup allot r@ swap move
    free throw r> 2 cells + ;
: >inherit ( class1 class2 -- class' ) >dynamic swap >osize @ over >osize ! ;
: class-resize ( class u -- class' ) over >methods @ umax >r
    class>count r@ 2 cells + umax resize throw
    r@ over cell+ !@ >r 2 cells + r> r> swap
    U+DO  ['] default-method defer@ over I + !  cell +LOOP ;

\ dot parser .foo -> >o foo o>

: >oo> ( xt table -- )  postpone >o compile, postpone o> ;
:noname ( object xt -- ) swap >o execute o> ; ' >oo> ' lit, recognizer: r:moof2

: rec:moof2 ( addr u -- xt r:moof2 | r:fail )
    2dup s" ." string-prefix?
    IF  1 /string rec:scope dup r:fail <> IF  r:moof2  THEN
    ELSE  2drop r:fail  THEN ;

' rec:moof2 get-recognizers 1+ set-recognizers