\ Mini-OOF2 example

require mini-oof2.fs

object class
    cell var x
    cell var y
    method p@
    method p!
    method p.
end-class point

:noname x @ y @ ; point defines p@
:noname x ? y ? ; point defines p.
:noname y ! x ! ; point defines p!

point new Constant p1
p1 >o 1 2 p! o>
p1 >o p. o>
