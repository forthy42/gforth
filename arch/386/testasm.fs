\ this code works with the following register assignments:
\ epb=sp, edi=tos

code my+ ( n1 n2 -- n )
    4 [ebp] edi add
    4 # ebp add
    ' noop >code-address jmp \ next
end-code

\ see my+
\ 3 5 my+ .
