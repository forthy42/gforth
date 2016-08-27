\ examples and tests for objects.fs
\ test for working across image generation
\ stuff to run after image generation

cr object heap-new print

cr undefined print

cr
counter1 print
counter1 inc
counter1 print
counter1 inc
counter1 inc
counter1 inc
counter1 print
counter1 print

\ examples of static binding
cr undefined bind object print

cr undefined object-print

cr
y print cr
20 x add
20 y add
x val .
\ y val . \ undefined
y print

cr
z print cr
z val . cr
z inc
z val . cr
1 z add
z val . cr

five print
five val 1+ . cr
.s cr
