\ usage:

object class
  cell var text
  cell var len
  cell var x
  cell var y
  method init
  method draw
end-class button

:noname ( o -- ) >r
 r@ x @ r@ y @ at-xy  r@ text @ r> len @ type ;
 button defines draw
:noname ( addr u o -- ) >r
 0 r@ x ! 0 r@ y ! r@ len ! r> text ! ;
 button defines init

\ interitance

: bold   27 emit ." [1m" ;
: normal 27 emit ." [0m" ;

button class end-class bold-button
:noname bold [ button :: draw ] normal ; bold-button defines draw

\ Create and draw a button:

button new Constant foo
s" thin foo" foo init
page
foo draw
bold-button new Constant bar
s" fat bar" bar init
1 bar y !
bar draw
