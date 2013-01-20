\ Mini-OOF2, using current object+Gforth primitives    09jan12py

\ template for methods and ivars

Create o 0 ,  DOES> @ o#+ [ 0 , ] + ;
compile> >body @ postpone o#+ , ;
Create m 0 ,  DOES> @ o#+ [ -1 cells , ] @ + perform ;
compile> >body @ cell/ postpone o#exec , ;
' o Value var-xt
' m Value method-xt
: current-o  ['] o to var-xt  ['] m to method-xt ;

\ core system

: method ( m v size "name" -- m' v )
  Header reveal method-xt vtcopy,  over , swap cell+ swap ;
: var ( m v size "name" -- m v' )
  Header reveal    var-xt vtcopy,  over , + ;
: class ( class -- class methods vars )
  dup 2@ ['] var IS +field ;
: end-class  ( class methods vars "name" -- )
  Create  here >r , dup , 2 cells ?DO ['] noop , cell +LOOP
  cell+ dup cell+ r> rot @ 2 cells /string move  standard:field ;
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
