abi-code my+  ( n1 n2 -- n3 )
\ ABI: SP passed in di, returned in ax,  address of FP passed in si
\ Caller-saved: ax,cx,dx,si,di,r8-r11,xmm0-xmm15
8 di d) ax lea        \ compute new sp in result reg
di )    dx mov        \ get old tos
dx    ax ) add        \ add to new tos
ret
end-code

: my+-compiled   ( n1 n2 -- n3 ) my+ ;

12 34 my+  46 <> throw
12 34 my+-compiled  46 <> throw


abi-code my-f+ ( r1 r2 -- r )
\ ABI: SP passed in di, returned in ax,  address of FP passed in si
si )    dx mov  \ load fp
.fl dx )   fld  \ r2
8 #     dx add  \ update fp
.fl dx )   fadd \ r1+r2
.fl dx )   fstp \ store r
dx    si ) mov  \ store new fp
di      ax mov  \ sp into return reg
ret             \ return from my-f+ 
end-code


: my-constant ( w "name" -- )
    create ,
;abi-code ( -- w )
    \ sp in di, address of fp in si, body address in dx
    -8 di d) ax lea \ compute new sp in result reg
    dx )     cx mov \ load w
    cx     ax ) mov \ put it in TOS
    ret
end-code

5 my-constant foo

: foo-compiled foo ;

foo 5 <> throw
foo-compiled 5 <> throw

: my-constant2 ( w "name" -- )
    create ,
;code ( -- w )
    \ sp=r15, tos=r14, ip=bx
    8 #        bx add
    r14     r15 ) mov
    $10 r9 d) r14 mov
    8 #       r15 sub
    -8 bx d)   jmp
end-code

7 my-constant2 bar
: bar-compiled bar ;

bar 7 <> throw
bar-compiled 7 <> throw

code my-1+ ( n1 -- n2 )
    8 #        bx add
    r14 inc
    -8 bx d)   jmp
end-code

: compiled-my-1+
    my-1+ ;

7 my-1+ 8 <> throw
8 compiled-my-1+ 9 <> throw

: funny-compiler-test
    drop my-1+ 1 ;

\ 6 9 funny-compiler-test 1 <> throw 7 <> throw
