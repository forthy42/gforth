\ this code works with the following register assignments:
\ esi=sp, ebx=tos

code my+ ( n1 n2 -- n )
    4 si D) bx add
    4 # si add
    Next
end-code

\ see my+
\ 3 5 my+ .
