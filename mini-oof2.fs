\ Mini-OOF2, using current object+Gforth primitives    09jan12py

\ Authors: Bernd Paysan, Anton Ertl
\ Copyright (C) 2012,2014,2015,2016,2017,2018,2019,2020,2021,2022,2023 Free Software Foundation, Inc.

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

Defer default-method ' noop IS default-method

\ optimization for object access
Create o+-table
' o0 , ' o1 , ' o2 , ' o3 , ' o4 , ' o5 , ' o6 , ' o7 ,
' o8 , ' o9 , ' o10 , ' o11 , ' o12 , ' o13 , ' o14 , ' o15 ,
' o16 , ' o17 , ' o18 , ' o19 , ' o20 , ' o21 , ' o22 , ' o23 ,
' o24 , ' o25 , ' o26 , ' o27 , ' o28 , ' o29 , ' o30 , ' o31 ,
here o+-table - >r
: oaddr, ( u -- )
    dup cell 1- and 0= over [ r> ]L u< and IF
	o+-table + @ compile,  EXIT  THEN
    postpone o lit, postpone + ;

\ template for methods and ivars
Create o# 0 ,  DOES> @ o + ;
opt: ( xt -- ) >body @ oaddr, ;
s" Invalid method for this class" exception Constant !!inv-method!!
: ?valid-method ( offset class -- offset )
    cell- @ over u<= !!inv-method!! and throw ;
: m>body ( xt class xtsel -- )
    >body @ over ?valid-method + ;
to-opt: ( xt class xtsel -- ) >body @ lit, postpone + ;
' m>body defer-table to-method: m-to
\ no validity check for compilation, normal usage is interpretative only
Create m 0 ,  DOES> @ -1 cells o + @ + perform ;
opt: ( xt -- ) >body @ cell/ postpone o#exec , ;
' m-to set-to
' o# Value var-xt
' m Value method-xt
: current-o  ['] o# to var-xt  ['] m to method-xt ;

\ ivalues

: o+field, ( addr body -- addr' )
    @ o + ;
opt: drop @ oaddr, ;

\ core system

-2 cells    field: >osize    field: >methods   drop
: method ( m v "name" -- m' v ) \ mini-oof2
    \G Define a selector @var{name}; increments the number of selectors
    \G @var{m} (in bytes).
    method-xt create-from reveal  over , swap cell+ swap ;
: var ( m v size "name" -- m v' ) \ mini-oof2
    \G define an instance variable with @var{size} bytes by the name
    \G @var{name}, and increments the amount of storage per instance @var{m}
    \G by @var{size}.
    var-xt    create-from reveal  over , dup , ( for sizeof ) + ;
: class ( class -- class methods vars ) \ mini-oof2
    \G start a class definition with superclass @var{class}, putting the size
    \G of the methods table and instance variable space on the stack.
    dup >osize 2@ ['] var IS +field ['] o+field, IS +field, ;
: end-class ( class methods vars "name" -- ) \ mini-oof2
    \G finishs a class definition and assigns a name @var{name} to the newly
    \G created class. Inherited methods are copied from the superclass.
    , dup , here >r 0 U+DO ['] default-method defer@ , cell +LOOP
    dup r@ swap >methods @ move r> Value ;
0 cells , 0 cells , here Value object

\ memory allocation

object class
    method :allocate
    method :free
end-class storage

storage class end-class static-alloc
storage class end-class dynamic-alloc

:noname  ( len -- addr )  here swap allot ; static-alloc is :allocate
:noname  ( addr -- )      drop ;            static-alloc is :free

:noname  ( len -- addr )  allocate throw ; dynamic-alloc is :allocate
:noname  ( addr -- )      free throw ;     dynamic-alloc is :free

static-alloc dup >osize @ cell+ here swap allot swap over ! cell+ Constant static-a
UValue allocater
static-a to allocater

: new ( class -- o )  dup >osize @ cell+
    allocater >o :allocate o> swap over !
    cell+ dup dup cell- @ >osize @ erase ;
: dispose ( o:o -- o:0 )  o cell- dup dup @ >osize @ cell+ erase
    allocater >o :free o>  0 >o rdrop ;
: clone ( o:o -- o' )
    o cell- @ new o cell- over cell- dup @ >osize @ cell+ move ;

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
    r@ over cell+ !@ 2 cells under+ r> swap
    U+DO  ['] default-method defer@ over I + !  cell +LOOP ;

\ dot parser .foo -> >o foo o>

: >oo> ( xt -- )  postpone >o name-compsem postpone o> ;
:noname ( object xt -- ) swap >o execute o> ;
' >oo> ' lit, >postponer translate: translate-moof2
' translate-moof2 Constant rectype-moof2

: rec-moof2 ( addr u -- xt translate-moof2 | notfound ) \ mini-oof2
    \G Very simplistic dot-parser, transforms @code{.}@var{selector/ivar} to
    \G @code{>o} @var{selector/ivar} @code{o>}.
    over c@ '.' = over 1 > and
    IF 1 /string sp@ >r forth-recognize
	translate-nt? IF rdrop ['] translate-moof2
	ELSE r> sp!  2drop ['] notfound THEN
    ELSE 2drop ['] notfound THEN ;

' rec-moof2 get-recognizers 1+ set-recognizers

standard:field

[IFDEF] cs-scope:
    : class{ ( parent "scope" -- methods vars )
	class cs-scope: ;
    : }class ( methods vars -- )
	s" class" nextname end-class }scope ;
[THEN]
