\ lib.fs	shared library support package 		16aug03py

\ Copyright (C) 1995,1996,1997,1998,2000,2003,2005,2006,2007,2008 Free Software Foundation, Inc.

\ This file is part of Gforth.

\ Gforth is free software; you can redistribute it and/or
\ modify it under the terms of the GNU General Public License
\ as published by the Free Software Foundation, either version 3
\ of the License, or (at your option) any later version.

\ This program is distributed in the hope that it will be useful,
\ but WITHOUT ANY WARRANTY; without even the implied warranty of
\ MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
\ GNU General Public License for more details.

\ You should have received a copy of the GNU General Public License
\ along with this program. If not, see http://www.gnu.org/licenses/.

\ replacements for former primitives
c-library fflib
s" avcall" add-lib
s" callback" add-lib

\c #include <avcall.h>
\c #include <callback.h>
\c static av_alist alist;
\c static va_alist gforth_clist;
\c #ifndef HAS_BACKLINK
\c static void **saved_gforth_pointers;
\c #endif
\c static float frv;
\c static int irv;
\c static double drv;
\c static long long llrv;
\c static void * prv;
\c typedef void *Label;
\c typedef Label *Xt;
\c 
\c void gforth_callback_ffcall(Xt* fcall, void * alist)
\c {
\c #ifndef HAS_BACKLINK
\c   void **gforth_pointers = saved_gforth_pointers;
\c #endif
\c   {
\c     /* save global valiables */
\c     Cell *rp = gforth_RP;
\c     Cell *sp = gforth_SP;
\c     Float *fp = gforth_FP;
\c     char *lp = gforth_LP;
\c     va_alist clist = gforth_clist;
\c 
\c     gforth_clist = (va_alist)alist;
\c 
\c     gforth_engine(fcall, sp, rp, fp, lp, gforth_UP);
\c 
\c     /* restore global variables */
\c     gforth_RP = rp;
\c     gforth_SP = sp;
\c     gforth_FP = fp;
\c     gforth_LP = lp;
\c     gforth_clist = clist;
\c   }
\c }

\c #define av_start_void1(c_addr) av_start_void(alist, c_addr)
c-function av-start-void av_start_void1 a -- void
\c #define av_start_int1(c_addr) av_start_int(alist, c_addr, &irv)
c-function av-start-int av_start_int1 a -- void
\c #define av_start_float1(c_addr) av_start_float(alist, c_addr, &frv)
c-function av-start-float av_start_float1 a -- void
\c #define av_start_double1(c_addr) av_start_double(alist, c_addr, &drv)
c-function av-start-double av_start_double1 a -- void
\c #define av_start_longlong1(c_addr) av_start_longlong(alist, c_addr, &llrv)
c-function av-start-longlong av_start_longlong1 a -- void
\c #define av_start_ptr1(c_addr) av_start_ptr(alist, c_addr, void *, &prv)
c-function av-start-ptr av_start_ptr1 a -- void
\c #define av_int1(w) av_int(alist,w)
c-function av-int av_int1 n -- void
\c #define av_float1(r) av_float(alist,r)
c-function av-float av_float1 r -- void
\c #define av_double1(r) av_double(alist,r)
c-function av-double av_double1 r -- void
\c #define av_longlong1(d) av_longlong(alist,d)
c-function av-longlong av_longlong1 d -- void
\c #define av_ptr1(a) av_ptr(alist, void *, a)
c-function av-ptr av_ptr1 a -- void
\c #define av_call_void() av_call(alist)
c-function av-call-void av_call_void -- void
\c #define av_call_int() (av_call(alist), irv)
c-function av-call-int av_call_int -- n
\c #define av_call_float() (av_call(alist), frv)
c-function av-call-float av_call_float -- r
\c #define av_call_double() (av_call(alist), drv)
c-function av-call-double av_call_double -- r
\c #define av_call_longlong() (av_call(alist), llrv)
c-function av-call-longlong av_call_longlong -- d
\c #define av_call_ptr() (av_call(alist), prv)
c-function av-call-ptr av_call_ptr -- a
\c #define alloc_callback1(a_ip) alloc_callback(gforth_callback_ffcall, (Xt *)a_ip)
c-function alloc-callback alloc_callback1 a -- a
\c #define va_start_void1() va_start_void(gforth_clist)
c-function va-start-void va_start_void1 -- void
\c #define va_start_int1() va_start_int(gforth_clist)
c-function va-start-int va_start_int1 -- void
\c #define va_start_longlong1() va_start_longlong(gforth_clist)
c-function va-start-longlong va_start_longlong1 -- void
\c #define va_start_ptr1() va_start_ptr(gforth_clist, (char *))
c-function va-start-ptr va_start_ptr1 -- void
\c #define va_start_float1() va_start_float(gforth_clist)
c-function va-start-float va_start_float1 -- void
\c #define va_start_double1() va_start_double(gforth_clist)
c-function va-start-double va_start_double1 -- void
\c #define va_arg_int1() va_arg_int(gforth_clist)
c-function va-arg-int va_arg_int1 -- n
\c #define va_arg_longlong1() va_arg_longlong(gforth_clist)
c-function va-arg-longlong va_arg_longlong1 -- d
\c #define va_arg_ptr1() va_arg_ptr(gforth_clist, char *)
c-function va-arg-ptr va_arg_ptr1 -- a
\c #define va_arg_float1() va_arg_float(gforth_clist)
c-function va-arg-float va_arg_float1 -- r
\c #define va_arg_double1() va_arg_double(gforth_clist)
c-function va-arg-double va_arg_double1 -- r
\c #define va_return_void1() va_return_void(gforth_clist)
c-function va-return-void1 va_return_void1 -- void
\c #define va_return_int1(w) va_return_int(gforth_clist,w)
c-function va-return-int1 va_return_int1 n -- void
\c #define va_return_ptr1(w) va_return_ptr(gforth_clist, void *, w)
c-function va-return-ptr1 va_return_ptr1 a -- void
\c #define va_return_longlong1(d) va_return_longlong(gforth_clist,d)
c-function va-return-longlong1 va_return_longlong1 d -- void
\c #define va_return_float1(r) va_return_float(gforth_clist,r)
c-function va-return-float1 va_return_float1 r -- void
\c #define va_return_double1(r) va_return_double(gforth_clist,r)
c-function va-return-double1 va_return_double1 r -- void
end-c-library

: av-int-r      2r> >r av-int ;
: av-float-r    f@local0 lp+ av-float ;
: av-double-r   f@local0 lp+ av-double ;
: av-longlong-r r> 2r> rot >r av-longlong ;
: av-ptr-r      2r> >r av-ptr ;
: va-return-void      va-return-void1     0 (bye) ;
: va-return-int       va-return-int1      0 (bye) ;
: va-return-ptr       va-return-ptr1      0 (bye) ;
: va-return-longlong  va-return-longlong1 0 (bye) ;
: va-return-float     va-return-float1    0 (bye) ;
: va-return-double    va-return-double1   0 (bye) ;

\ start of fflib proper

Variable libs 0 libs !
\ links between libraries
Variable thisproc
Variable thislib

Variable revdec  revdec off
\ turn revdec on to compile bigFORTH libraries
Variable revarg  revarg off
\ turn revarg on to compile declarations with reverse arguments
Variable legacy  legacy off
\ turn legacy on to compile bigFORTH legacy libraries

Vocabulary c-decl
Vocabulary cb-decl

: @lib ( lib -- )
    \G obtains library handle
    cell+ dup 2 cells + count open-lib
    dup 0= abort" Library not found" swap ! ;

: @proc ( lib addr -- )
    \G obtains symbol address
    cell+ tuck cell+ @ count rot cell+ @
    lib-sym  dup 0= abort" Proc not found!" swap ! ;

: proc, ( lib -- )
\G allocates and initializes proc stub
\G stub format:
\G    linked list in library
\G    address of proc
\G    ptr to OS name of symbol as counted string
\G    threaded code for invocation
    here dup thisproc !
    swap 2 cells + dup @ A, !
    0 , 0 A, ;

Defer legacy-proc  ' noop IS legacy-proc

: proc:  ( lib "name" -- )
\G Creates a named proc stub
    Create proc, 0 also c-decl
    legacy @ IF  legacy-proc  THEN
DOES> ( x1 .. xn -- r )
    dup cell+ @ swap 3 cells + >r ;

Variable ind-call ind-call off
: fptr ( "name" -- )
    Create here thisproc ! 0 , 0 , 0 ,  0 also c-decl  ind-call on
    DOES>  3 cells + >r ;

: library ( "name" "file" -- )
\G loads library "file" and creates a proc defining word "name"
\G library format:
\G    linked list of libraries
\G    library handle
\G    linked list of library's procs
\G    OS name of library as counted string
    Create  here libs @ A, dup libs !
    0 , 0 A, parse-name string, @lib
DOES> ( -- )  dup thislib ! proc: ;

: init-shared-libs ( -- )
    defers 'cold
    0  libs  BEGIN
	@ dup WHILE
	    dup  REPEAT
    drop BEGIN
	dup  WHILE
	    >r
	    r@ @lib
	    r@ 2 cells +  BEGIN
		@ dup  WHILE
		    r@ over @proc  REPEAT
	    drop rdrop
    REPEAT
    drop ;

' init-shared-libs IS 'cold

: argtype ( revxt pushxt fwxt "name" -- )
    Create , , , ;

: arg@ ( arg -- argxt pushxt )
    revarg @ IF  2 cells + @ ['] noop swap  ELSE  2@  THEN ;

: arg, ( xt -- )
    dup ['] noop = IF  drop  EXIT  THEN  compile, ;

: decl, ( 0 arg1 .. argn call start -- )
    2@ compile, >r
    revdec @ IF  0 >r
	BEGIN  dup  WHILE  >r  REPEAT
	BEGIN  r> dup  WHILE  arg@ arg,  REPEAT  drop
	BEGIN  dup  WHILE  arg,  REPEAT drop
    ELSE  0 >r
	BEGIN  dup  WHILE  arg@ arg, >r REPEAT drop
	BEGIN  r> dup  WHILE  arg,  REPEAT  drop
    THEN
    r> compile,  postpone EXIT ;

: symbol, ( "c-symbol" -- )
    here thisproc @ 2 cells + ! parse-name s,
    thislib @ thisproc @ @proc ;

: rettype ( endxt startxt "name" -- )
    Create 2,
  DOES>  decl, ind-call @ 0= IF  symbol,  THEN
    previous revarg off ind-call off ;

also c-decl definitions

: <rev>  revarg on ;

' av-int      ' av-int-r      ' >r  argtype int
' av-float    ' av-float-r    ' f>l argtype sf
' av-double   ' av-double-r   ' f>l argtype df
' av-longlong ' av-longlong-r ' 2>r argtype dlong
' av-ptr      ' av-ptr-r      ' >r  argtype ptr

' av-call-void     ' av-start-void     rettype (void)
' av-call-int      ' av-start-int      rettype (int)
' av-call-float    ' av-start-float    rettype (sf)
' av-call-double   ' av-start-double   rettype (fp)
' av-call-longlong ' av-start-longlong rettype (dlong)
' av-call-ptr      ' av-start-ptr      rettype (ptr)

: (addr)  postpone EXIT drop symbol, previous revarg off ;

previous definitions

\ legacy support for old library interfaces
\ interface to old vararg stuff not implemented yet

also c-decl

:noname ( n 0 -- 0 int1 .. intn )
    legacy @ 0< revarg !
    swap 0 ?DO  int  LOOP  (int)
; IS legacy-proc

: (int) ( n -- )
    >r ' execute r> 0 ?DO  int  LOOP  (int) ;
: (void) ( n -- )
    >r ' execute r> 0 ?DO  int  LOOP  (void) ;
: (float) ( n -- )
    >r ' execute r> 0 ?DO  df   LOOP  (fp) ;

previous

\ callback stuff

Variable callbacks
\G link between callbacks

: callback ( -- )
    Create  0 ] postpone >r also cb-decl
  DOES>
    Create here >r 0 , callbacks @ A, r@ callbacks !
    swap postpone Literal postpone call , postpone EXIT
    r> dup cell+ cell+ alloc-callback swap !
  DOES> @ ;

: callback; ( 0 xt1 .. xtn -- )
    BEGIN  over  WHILE  compile,  REPEAT
    postpone r> postpone execute compile, drop
    postpone EXIT postpone [ previous ; immediate

: va-ret ( xt xt -- )
    Create A, A, immediate
  DOES> 2@ compile, ;

: init-callbacks ( -- )
    defers 'cold  callbacks 1 cells -
    BEGIN  cell+ @ dup  WHILE  dup cell+ cell+ alloc-callback over !
    REPEAT  drop ;

' init-callbacks IS 'cold

also cb-decl definitions

\ arguments

' va-arg-int      Alias int
' va-arg-float    Alias sf
' va-arg-double   Alias df
' va-arg-longlong Alias dlong
' va-arg-ptr      Alias ptr

' va-return-void     ' va-start-void     va-ret (void)
' va-return-int      ' va-start-int      va-ret (int)
' va-return-float    ' va-start-float    va-ret (sf)
' va-return-double   ' va-start-double   va-ret (fp)
' va-return-longlong ' va-start-longlong va-ret (dlong)
' va-return-ptr      ' va-start-ptr      va-ret (ptr)

previous definitions
