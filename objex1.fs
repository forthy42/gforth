\ examples and tests for objects.fs
\ test for working across image generation
\ stuff to run before image generation

\ written by Anton Ertl 1996-1998, 2016
\ public domain

object class

:noname ( object -- )
    drop ." undefined" ;
overrides print
end-class nothing

nothing dict-new constant undefined

\ instance variables and this
object class
    cell% inst-var n
m: ( object -- )
   0 n ! ;m
overrides construct
m: ( object -- )
    n @ . ;m
overrides print
m: ( object -- )
    1 n +! ;m
method inc
end-class counter

counter dict-new constant counter1

\ examples of static binding

: object-print ( object -- )
    [bind] object print ;

\ interface

\ sorry, a meaningful example would be too long

interface
selector add ( n object -- )
selector val ( object -- n )
end-interface foobar

counter class
    foobar implementation

m: ( object -- )
    this [parent] inc
    n @ 10 mod 0=
    if
	." xcounter " this object-print ." made another ten" cr
    then
;m overrides inc
    
m: ( n object -- )
    0 do
	this inc
    loop
;m overrides add

m: ( object -- n )
    n @
;m overrides val

end-class xcounter


object class
    foobar implementation

    cell% inst-var n

m: ( n object -- )
    n !
;m overrides construct

m: ( object -- )
    n @ .
;m overrides print

m: ( n object -- )
    n +!
;m overrides add

protected

create protected1

protected

create protected2

cr order

public

create public1

cr order

\ we leave val undefined
end-class int

\ a perhaps more sensible class structure would be to have int as
\ superclass of counter, but that would not exercise interfaces

xcounter dict-new constant x
create y 3 int dict-new drop \ same as "3 int dict-new constant y"

cr
int push-order
order cr
words cr
int drop-order
order
cr

\ test override of inherited interface selector
xcounter class

m: ( object -- n )
    this [parent] val 2*
;m overrides val

end-class ycounter

ycounter dict-new constant z

\ test inst-value
object class
    foobar implementation

    inst-value N

    m: ( n object -- )
        this [parent] construct \ currently does nothing, but who knows
        [to-inst] N
    ;m overrides construct

    m: ( object -- )
        N .
    ;m overrides print

    m: ( object -- n )
        N
    ;m overrides val
end-class const-int

5 const-int dict-new constant five
