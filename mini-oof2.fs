\ Mini-OOF2, using current object+Gforth primitives    09jan12py

: o ( -- addr )  o#+ [ 0 , ] ;
: vt ( -- addr ) o#+ [ -1 cells , ] @ ;
: method-compile  compile>  >body @ ['] o#exec compile, , ;
: method ( m v "name" -- m' v ) Create  over , swap cell+ swap
    method-compile
  DOES> ( ... -- ... ) @ vt + perform ;
: var-compile   compile>  >body @ ['] o#+ compile, , ;
: var ( m v size "name" -- m v' ) Create  over , +
    var-compile
  DOES> ( -- addr ) @ o + ;
: class ( class -- class methods vars ) dup 2@ ;
: end-class  ( class methods vars "name" -- )
  Create  here >r , dup , 2 cells ?DO ['] noop , 1 cells +LOOP
  cell+ dup cell+ r> rot @ 2 cells /string move ;
: >vt ( class "name" -- addr )  ' >body @ + ;
: bind ( class "name" -- xt )    >vt @ ;
: defines ( xt class "name" -- ) >vt ! ;
: new ( class -- o )  here over @ cell+ allot swap over ! cell+ ;
: :: ( class "name" -- ) bind compile, ;
Create object  0 cells , 2 cells ,
