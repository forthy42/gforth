\ libcc.fs	foreign function interface implemented using a C compiler

\ Copyright (C) 2006 Free Software Foundation, Inc.

\ This file is part of Gforth.

\ Gforth is free software; you can redistribute it and/or
\ modify it under the terms of the GNU General Public License
\ as published by the Free Software Foundation; either version 2
\ of the License, or (at your option) any later version.

\ This program is distributed in the hope that it will be useful,
\ but WITHOUT ANY WARRANTY; without even the implied warranty of
\ MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
\ GNU General Public License for more details.

\ You should have received a copy of the GNU General Public License
\ along with this program; if not, write to the Free Software
\ Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111, USA.


\ What this implementation does is this: if it sees a declaration like

\ libc dlseek int dlong int (dlong) lseek ( fd doffset whence -- doffset2 )

\ it genererates C code similar to the following:

\ #include <gforth.h>
\ 
\ void gforth_call_dl_i_dl_i(void)
\ {
\   Cell *sp = gforth_SP;
\   Float *fp = gforth_FP;
\   long (*func)(int, long, int);
\   int arg1;
\   long arg2;
\   int arg3;
\   long result;
\   func = (char *)((Cell *)sp)[0];
\   arg3 = ((Cell *)sp)[1];
\   arg2 = gforth_d2ll(sp[3],sp[2]);
\   arg1 = ((Cell *)sp)[4];
\   result = func(arg1, arg2, arg3);
\   gforth_ll2d(result, sp[4], sp[3]);
\   gforth_SP += 3;
\ }

\ Then it compiles this code and dynamically links it into the Gforth
\ system (batching and caching are future work).  It also dynamically
\ links lseek.  Performing DLSEEK then puts the function pointer of
\ lseek() on the stack, the function pointer of
\ gforth_call_del_i_dl_i, and calls CALL-C.


s" Library not found" exception constant err-nolib

: library ( "name" "file" -- ) \ gforth
\G Dynamically links the library specified by @i{file}.  Defines a
\G word @i{name} ( -- lib ) that starts the declaration of a
\G function from that library.
    create parse-name open-lib dup 0= err-nolib and throw ,
  does> ( -- lib )
    @ ;


