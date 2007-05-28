require libcc.fs

\c #include <string.h>
\c #include <stdlib.h>

c-function strlen strlen a -- n
c-function labs labs n -- n

\c #include <sys/types.h>
\c #include <unistd.h>
c-function dlseek lseek n d n -- d

cr s\" fooo\0" 2dup dump drop .s strlen cr .s drop cr 
-5 labs .s drop cr
