require libcc.fs

\c #include <string.h>
\c #include <stdlib.h>

c-function strlen strlen a -- n
cr s\" fooo\0" 2dup dump drop .s strlen cr .s drop cr 
c-function labs labs n -- n

\c #define _FILE_OFFSET_BITS 64
\c #include <sys/types.h>
\c #include <unistd.h>
c-function dlseek lseek n d n -- d

cr s\" fooo\0" 2dup dump drop .s strlen cr .s drop cr 
-5 labs .s drop cr

\c #include <stdio.h>
c-function printf-nr printf a n r -- n
c-function printf-rn printf a r n -- n
s\" n=%d r=%f\n\0" drop -5 -0.5e fp@ hex. cr printf-nr . cr
s\" r=%f n=%d\n\0" drop -0.5e -5 printf-rn . cr

\c #define printfull(s,ull) printf(s,(unsigned long long)ull)
c-function printfull printfull a n -- n
s\" ull=%llu\n\0" drop -1 printfull . cr
s\" ull=%llu r=%f\n\0" drop -1 -0.5e printf-nr . cr

\c #define printfll(s,ll) printf(s,(long long)ll)
c-function printfll printfll a n -- n
s\" ll=%lld\n\0" drop -1 printfll . cr
s\" ll=%lld r=%f\n\0" drop -1 -0.5e printf-nr . cr

\ test calling a C function pointer from Forth

\ first create a C function pointer:

\c typedef long (* func1)(long);
\c #define labsptr(dummy) ((func1)labs)
c-function labsptr labsptr -- a

\ now the call
\c #define call_func1(par1,fptr) ((func1)fptr)(par1)
c-function call_func1 call_func1 n func -- n

-5 labsptr call_func1 . cr

