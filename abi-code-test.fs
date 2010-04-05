
abi-code my+  ( n1 n2 -- n3 )
   di ax mov		\ ABI: sp passed in di, returned in ax
   si dx mov		\ ABI: fp passed in si, returned in dx
   ax ) r8  mov		\ load sp[0]
   8 ax d) r8 add	\ add sp[1]
   8 # ax  add		\ store result to *++sp
   r8  ax ) mov
   ret			\ return to caller
end-code

: my+-compiled   ( n1 n2 -- n3 ) my+ ;

assert0( 12 34 my+  46 = )
assert0( 12 34 my+-compiled  46 = )
