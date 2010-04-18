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
