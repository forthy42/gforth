\ Mini-OOF2 example

require mini-oof2.fs

object class
    field: x
    field: y
    method p@
    method p!
    method p.
end-class point

:noname x @ y @ ; point to p@
:noname x ? y ? ; point to p.
:noname y ! x ! ; point to p!

point new Constant p1
p1 >o 1 2 p! o>
p1 >o p. o> cr
: test p1 >o 1 2 p! p. x @ y @ + . o> ;
test cr