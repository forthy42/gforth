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
: new ( class -- o )  here over @ cell+ allot swap over ! cell+ ;
: :: ( class "name" -- ) bind compile, ;
Create object  0 cells , 2 cells ,
