\ Mini-OOF2, using current object+Gforth primitives    09jan12py

: o ( -- addr )  o#+ [ 0 , ] ;
: vt ( -- addr ) o#+ [ -1 cells , ] @ ;
: method-create  Create   DOES> ( ... -- ... ) @ vt + perform ;
: method ( m v "name" -- m' v ) method-create  over , swap cell+ swap
  [: >body @ cell/ ['] o#exec compile, , ;] !compile, ;
: var-create  Create  DOES> ( -- addr ) @ o + ;
: var ( m v size "name" -- m v' ) var-create  over , +
  [: >body @ ['] o#+ compile, , ;] !compile, ;
: class ( class -- class methods vars )
  dup 2@ ['] var IS +field ;
: end-class  ( class methods vars "name" -- )
  Create  here >r , dup , 2 cells ?DO ['] noop , 1 cells +LOOP
  cell+ dup cell+ r> rot @ 2 cells /string move
  standard:field ;
: >vt ( class "name" -- addr )  ' >body @ + ;
: bind ( class "name" -- xt )    >vt @ ;
: defines ( xt class "name" -- ) >vt ! ;
: :: ( class "name" -- ) bind compile, ;
Create object  0 cells , 2 cells ,

\ memory allocation

object class
    method :allocate
    method :free
end-class storage

storage class end-class static-alloc
storage class end-class dynamic-alloc

:noname  ( len -- addr )  here swap allot ; static-alloc defines :allocate
:noname  ( addr -- )      drop ;            static-alloc defines :free

:noname  ( len -- addr )  allocate throw ; dynamic-alloc defines :allocate
:noname  ( addr -- )      free throw ;     dynamic-alloc defines :free

static-alloc dup @ cell+ here swap allot swap over ! cell+ Constant static-a
static-a Value allocater

: new ( class -- o )  dup @ cell+ allocater >o :allocate o> swap over ! cell+ ;
: dispose ( o:o -- )  o cell- allocater >o :free o> ;

dynamic-alloc new Constant dynamic-a

dynamic-a to allocater