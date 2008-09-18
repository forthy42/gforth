\ libffi.fs	shared library support package 		14aug05py

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
\ note that the API functions have their arguments reversed and other
\ deviations.

c-library libffi
s" ffi" add-lib

\ The ffi.h of XCode needs the following line, and it should not hurt elsewhere
\c #define MACOSX
include-ffi.h-string save-c-prefix-line \ #include <ffi.h>
\c #include <stdio.h>
\c static void **gforth_clist;
\c static void *gforth_ritem;
\c #ifndef HAS_BACKLINK
\c static void **saved_gforth_pointers;
\c #endif
\c typedef void *Label;
\c typedef Label *Xt;
\c static void gforth_callback_ffi(ffi_cif * cif, void * resp, void ** args, void * ip)
\c {
\c #ifndef HAS_BACKLINK
\c   void **gforth_pointers = saved_gforth_pointers;
\c #endif
\c   {
\c     Cell *rp1 = gforth_RP;
\c     Cell *sp = gforth_SP;
\c     Float *fp = gforth_FP;
\c     unsigned char *lp = gforth_LP;
\c     void ** clist = gforth_clist;
\c     void * ritem = gforth_ritem;
\c
\c     gforth_clist = args;
\c     gforth_ritem = resp;
\c
\c     gforth_engine((Xt *)ip, sp, rp1, fp, lp, gforth_UP);
\c 
\c     /* restore global variables */
\c     gforth_RP = rp1;
\c     gforth_SP = sp;
\c     gforth_FP = fp;
\c     gforth_LP = lp;
\c     gforth_clist = clist;
\c     gforth_ritem = ritem;
\c   }
\c }

\c static void* ffi_types[] =
\c     { &ffi_type_void,
\c       &ffi_type_uint8, &ffi_type_sint8,
\c       &ffi_type_uint16, &ffi_type_sint16,
\c       &ffi_type_uint32, &ffi_type_sint32,
\c       &ffi_type_uint64, &ffi_type_sint64,
\c       &ffi_type_float, &ffi_type_double, &ffi_type_longdouble,
\c       &ffi_type_pointer };
\c #define ffi_type(n) (ffi_types[n])
c-function ffi-type ffi_type n -- a

\c static int ffi_sizes[] = { sizeof(ffi_cif), sizeof(ffi_closure) };
\c #define ffi_size(n1) (ffi_sizes[n1])
c-function ffi-size ffi_size n -- n

\c #define ffi_prep_cif1(atypes, n, rtype, cif) \
\c           ffi_prep_cif((ffi_cif *)cif, FFI_DEFAULT_ABI, n, \
\c                        (ffi_type *)rtype, (ffi_type **)atypes)
c-function ffi-prep-cif ffi_prep_cif1 a n a a -- n

\c #ifdef HAS_BACKLINK
\c #define ffi_call1(a_avalues, a_rvalue ,a_ip ,a_cif) \
\c             ffi_call((ffi_cif *)a_cif, (void(*)())a_ip, \
\c                      (void *)a_rvalue, (void **)a_avalues)
\c #else
\c #define ffi_call1(a_avalues, a_rvalue ,a_ip ,a_cif) \
\c             (saved_gforth_pointers = gforth_pointers), \
\c             ffi_call((ffi_cif *)a_cif, (void(*)())a_ip, \
\c                      (void *)a_rvalue, (void **)a_avalues)
\c #endif
c-function ffi-call ffi_call1 a a a a -- void

\c #define ffi_prep_closure1(a_ip, a_cif, a_closure) \
\c              ffi_prep_closure((ffi_closure *)a_closure, (ffi_cif *)a_cif, gforth_callback_ffi, (void *)a_ip)
c-function ffi-prep-closure ffi_prep_closure1 a a a -- n

\ !! use ud?
\c #define ffi_2fetch(a_addr) (*(long long *)a_addr)
c-function ffi-2@ ffi_2fetch a -- d

\c #define ffi_2store(d,a_addr) ((*(long long *)a_addr) = (long long)d)
c-function ffi-2! ffi_2store d a -- void

\c #define ffi_arg_int() (*(int *)(*gforth_clist++))
c-function ffi-arg-int ffi_arg_int -- n

\c #define ffi_arg_long() (*(long *)(*gforth_clist++))
c-function ffi-arg-long ffi_arg_long -- n

\c #define ffi_arg_longlong() (*(long long *)(*gforth_clist++))
c-function ffi-arg-longlong ffi_arg_longlong -- d

\ !! correct?  The primitive is different, but looks funny
c-function ffi-arg-dlong ffi_arg_long -- d

\c #define ffi_arg_ptr() (*(char **)(*gforth_clist++))
c-function ffi-arg-ptr ffi_arg_ptr -- a

\c #define ffi_arg_float() (*(float *)(*gforth_clist++))
c-function ffi-arg-float ffi_arg_float -- r

\c #define ffi_arg_double() (*(double *)(*gforth_clist++))
c-function ffi-arg-double ffi_arg_double -- r

: ffi-ret-void ( -- )
    0 (bye) ;

\c #define ffi_ret_int1(w) (*(int*)(gforth_ritem) = w)
c-function ffi-ret-int1 ffi_ret_int1 n -- void
: ffi-ret-int ( w -- ) ffi-ret-int1 ffi-ret-void ;

\c #define ffi_ret_longlong1(d) (*(long long *)(gforth_ritem) = d)
c-function ffi-ret-longlong1 ffi_ret_longlong1 d -- void
: ffi-ret-longlong ( d -- ) ffi-ret-longlong1 ffi-ret-void ;

\c #define ffi_ret_dlong1(d) (*(long *)(gforth_ritem) = d)
c-function ffi-ret-dlong1 ffi_ret_dlong1 d -- void
: ffi-ret-dlong ( d -- ) ffi-ret-dlong1 ffi-ret-void ;

c-function ffi-ret-long1 ffi_ret_dlong1 n -- void
: ffi-ret-long ( n -- ) ffi-ret-long1 ffi-ret-void ;

\c #define ffi_ret_ptr1(w) (*(char **)(gforth_ritem) = w)
c-function ffi-ret-ptr1 ffi_ret_ptr1 a -- void
: ffi-ret-ptr ( a -- ) ffi-ret-ptr1 ffi-ret-void ;

\c #define ffi_ret_float1(r) (*(float *)(gforth_ritem) = r)
c-function ffi-ret-float1 ffi_ret_float1 r -- void
: ffi-ret-float ( r -- ) ffi-ret-float1 ffi-ret-void ;

\c #define ffi_ret_double1(r) (*(double *)(gforth_ritem) = r)
c-function ffi-ret-double1 ffi_ret_double1 r -- void
: ffi-ret-double ( r -- ) ffi-ret-double1 ffi-ret-void ;
end-c-library

\ common stuff, same as fflib.fs

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
    3 cells + >r ;

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
    defers 'cold  libs
    0  libs  BEGIN  @ dup  WHILE  dup  REPEAT  drop
    BEGIN  dup  WHILE  >r
	r@ @lib
	r@ 2 cells +  BEGIN  @ dup  WHILE  r@ over @proc  REPEAT
	drop rdrop
    REPEAT  drop ;

' init-shared-libs IS 'cold

: symbol, ( "c-symbol" -- )
    here thisproc @ 2 cells + ! parse-name s,
    thislib @ thisproc @ @proc ;

\ stuff for libffi

\ libffi uses a parameter array for the input

$20 Value maxargs

Create retbuf 2 cells allot
Create argbuf maxargs 2* cells allot
Create argptr maxargs 0 [DO]  argbuf [I] 2* cells + A, [LOOP]

\ "forward" when revarg is on

\ : >c+  ( char buf -- buf' )  tuck   c!    cell+ cell+ ;
: >i+  ( n buf -- buf' )     tuck   l!    cell+ cell+ ;
: >p+  ( addr buf -- buf' )  tuck    !    cell+ cell+ ;
: >d+  ( d buf -- buf' )     dup >r ffi-2! r> cell+ cell+ ;
: >dl+ ( d buf -- buf' )     nip dup >r  ! r> cell+ cell+ ;
: >sf+ ( r buf -- buf' )     dup   sf!    cell+ cell+ ;
: >df+ ( r buf -- buf' )     dup   df!    cell+ cell+ ;

\ "backward" when revarg is off

: >i-  ( n buf -- buf' )     2 cells - tuck   l! ;
: >p-  ( addr buf -- buf' )  2 cells - tuck    ! ;
: >d-  ( d buf -- buf' )     2 cells - dup >r ffi-2! r> ;
: >dl- ( d buf -- buf' )     2 cells - nip dup >r ! r> ;
: >sf- ( r buf -- buf' )     2 cells - dup   sf! ;
: >df- ( r buf -- buf' )     2 cells - dup   df! ;

\ return value

: i>x   ( -- n )  retbuf l@ ;
: is>x   ( -- n )  retbuf sl@ ;
: p>x   ( -- addr ) retbuf @ ;
: dl>x   ( -- d ) retbuf @ s>d ;
: d>x   ( -- d )  retbuf ffi-2@ ;
: sf>x  ( -- r )  retbuf sf@ ;
: df>x  ( -- r )  retbuf df@ ;

wordlist constant cifs

Variable cifbuf $40 allot \ maximum: 64 parameters
: cifreset  cifbuf cell+ cifbuf ! ;
cifreset
Variable args args off

: argtype ( bkxt fwxt type "name" -- )
    Create , , , DOES>  1 args +! ;

: arg@ ( arg -- type pushxt )
    dup @ swap cell+
    revarg @ IF  cell+  THEN  @    ;

: arg, ( xt -- )
    dup ['] noop = IF  drop  EXIT  THEN  compile, ;

: start, ( n -- )  cifbuf cell+ cifbuf !
    revarg @ IF  drop 0  ELSE  2* cells  THEN  argbuf +
    postpone Literal ;

Variable ind-call  ind-call off
: fptr  ind-call on  Create  here thisproc !
    0 , 0 , 0 , 0 also c-decl  DOES>  cell+ dup cell+ cell+ >r ! ;

: ffi-call, ( -- lit-cif )
    postpone drop postpone argptr postpone retbuf
    thisproc @ cell+ postpone literal postpone @
    0 postpone literal here cell -
    postpone ffi-call ;

: cif, ( n -- )
    cifbuf @ c! 1 cifbuf +! ;

: cif@ ( -- addr u )
    cifbuf cell+ cifbuf @ over - ;

: create-cif ( rtype -- addr ) cif,
    cif@ cifs search-wordlist
    IF  execute  EXIT  THEN
    get-current >r cifs set-current
    cif@ nextname Create  here >r
    cif@ 1- bounds ?DO  I c@ ffi-type ,  LOOP  r>
    r> set-current ;

: make-cif ( rtype -- addr )  create-cif
    cif@ 1- tuck + c@ ffi-type here 0 ffi-size allot
    dup >r ffi-prep-cif throw r> ;

: decl, ( 0 arg1 .. argn call rtype start -- )
    start, { retxt rtype } cifreset
    revdec @ IF  0 >r
	BEGIN  dup  WHILE  >r  REPEAT
	BEGIN  r> dup  WHILE  arg@ arg,  REPEAT
	ffi-call, retxt compile,  postpone  EXIT
	BEGIN  dup  WHILE  cif,  REPEAT drop
    ELSE  0 >r
	BEGIN  dup  WHILE  arg@ arg, >r REPEAT drop
	ffi-call, retxt compile,  postpone  EXIT
	BEGIN  r> dup  WHILE  cif,  REPEAT  drop
    THEN  rtype make-cif swap ! here thisproc @ 2 cells + ! ;

: rettype ( endxt n "name" -- )
    Create 2,
  DOES>  2@ args @ decl, ind-call @ 0= IF  symbol,  THEN
    previous revarg off args off ind-call off ;

6 1 cells 4 > 2* - Constant _long

also c-decl definitions

: <rev>  revarg on ;

' >i+  ' >i-    6 argtype int
' >p+  ' >p-    _long argtype long
' >p+  ' >p-  &12 argtype ptr
' >d+  ' >d-    8 argtype llong
' >dl+ ' >dl-   6 argtype dlong
' >sf+ ' >sf-   9 argtype sf
' >df+ ' >df- &10 argtype df
: ints 0 ?DO int LOOP ;

' noop   0 rettype (void)
' is>x   6 rettype (int)
' i>x    5 rettype (uint)
' p>x    _long rettype (long)
' p>x  &12 rettype (ptr)
' d>x    8 rettype (llong)
' dl>x   6 rettype (dlong)
' sf>x   9 rettype (sf)
' df>x &10 rettype (fp)

: (addr) thisproc @ cell+ postpone Literal postpone @ postpone EXIT
    drop symbol, previous revarg off args off ;

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

Variable rtype

: alloc-callback ( ip -- addr )
    rtype @ make-cif here 1 ffi-size allot
    dup >r ffi-prep-closure throw r> ;

: callback ( -- )
    Create  0 ] postpone >r also cb-decl cifreset
  DOES>
    0 Value  -1 cells allot
    here >r 0 , callbacks @ A, r@ callbacks !
    swap postpone Literal postpone call , postpone EXIT
    r@ cell+ cell+ alloc-callback r> ! ;

\ !! is the stack effect right?  or is it ( 0 ret arg1 .. argn -- ) ?
: callback; ( 0 arg1 .. argn -- )
    BEGIN  over  WHILE  compile,  REPEAT
    postpone r> postpone execute compile, drop
    \ !! should we put ]] 0 (bye) [[ here?
    \ !! is the EXIT ever executed?
    postpone EXIT postpone [ previous ; immediate

: rettype' ( xt n -- )
    Create , A, immediate
  DOES> 2@ rtype ! ;
: argtype' ( xt n -- )
    Create , A, immediate
  DOES> 2@ cif, ;

: init-callbacks ( -- )
    defers 'cold  callbacks cell -
    BEGIN  cell+ @ dup  WHILE  dup cell+ cell+ alloc-callback over !
    REPEAT  drop ;

' init-callbacks IS 'cold

also cb-decl definitions

\ arguments

' ffi-arg-int        6 argtype' int
' ffi-arg-float      9 argtype' sf
' ffi-arg-double   &10 argtype' df
' ffi-arg-long       _long argtype' long
' ffi-arg-longlong   8 argtype' llong
' ffi-arg-dlong      6 argtype' dlong
' ffi-arg-ptr      &12 argtype' ptr
: ints ( n -- ) 0 ?DO postpone int LOOP ; immediate

' ffi-ret-void       0 rettype' (void)
' ffi-ret-int        6 rettype' (int)
' ffi-ret-float      9 rettype' (sf)
' ffi-ret-double   &10 rettype' (fp)
' ffi-ret-longlong   8 rettype' (llong)
' ffi-ret-long       _long rettype' (long)
' ffi-ret-dlong      _long rettype' (dlong)
' ffi-ret-ptr      &12 rettype' (ptr)

previous definitions
    
