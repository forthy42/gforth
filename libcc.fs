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

\ \ something that tells it that the current library is libc
\ \c #include <unistd.h>
\ c-function dlseek lseek n d n -- d

\ it genererates C code similar to the following:

\ #include <gforth.h>
\ #include <unistd.h>
\ 
\ void gforth_c_lseek_ndn_d(void)
\ {
\   Cell *sp = gforth_SP;
\   Float *fp = gforth_FP;
\   long long result;  /* longest type in C */
\   gforth_ll2d(lseek(sp[3],gforth_d2ll(sp[2],sp[1]),sp[0]),sp[3],sp[2]);
\   gforth_SP = sp+2;
\ }

\ Then it compiles this code and dynamically links it into the Gforth
\ system (batching and caching are future work).  It also dynamically
\ links lseek.  Performing DLSEEK then puts the function pointer of
\ the function pointer of gforth_c_lseek_ndn_d on the stack and
\ calls CALL-C.

\ ToDo:

\ Batching, caching and lazy evaluation:

\ Batching:

\ New words are deferred, and the corresponding C functions are
\ collected in one file, until the first word is EXECUTEd; then the
\ file is compiled and linked into the system, and the word is
\ resolved.

\ Caching:

\ Instead of compiling all this stuff anew for every execution, we
\ keep the files around and have an index file containing the function
\ names and their corresponding .so files.  If the needed wrapper name
\ is already present, it is just linked instead of generating the
\ wrapper again.  This is all done by loading the index file(s?),
\ which define words for the wrappers in a separate wordlist.

\ The files are built in .../lib/gforth/$VERSION/libcc/ or
\ ~/.gforth/libcc/$HOST/.

\ other things to do:

\ c-variable forth-name c-name
\ c-constant forth-name c-name

\ Todo: conversion between function pointers and xts (both directions)

\ taking an xt and turning it into a function pointer:

\ e.g., assume we have the xt of + and want to create a C function int
\ gforth_callback_plus(int, int), and then pass the pointer to that
\ function:

\ There should be Forth code like this:
\   ] + 0 (bye)
\ Assume that the start of this code is START
        
\ Now, there should be a C function:

\ int gforth_callback_plus(int p1, int p2)
\ {
\   Cell   *sp = gforth_SP;
\   Float  *fp = gforth_FP;
\   Float  *fp = gforth_FP;
\   Address lp = gforth_LP;
\   sp -= 2;
\   sp[0] = p1;
\   sp[1] = p2;
\   gforth_engine(START, sp, rp, fp, lp);
\   sp += 1;
\   gforth_RP = rp;
\   gforth_SP = sp;
\   gforth_FP = fp;
\   gforth_LP = lp;
\   return sp[0];
\ }

\ and the pointer to that function is the C function pointer for the XT of +.

\ Future problems:
\   how to combine the Forth code generation with inlining
\   START is not a constant across executions (when caching the C files)
\      Solution: make START a variable, and store into it on startup with dlsym

\ Syntax:
\  callback <rettype> <params> <paramtypes> -- <rettype>


\ data structures

\ For every c-function, we have three words: two anonymous words
\ created by c-function-ft (first time) and c-function-rt (run-time),
\ and a named deferred word.  The deferred word first points to the
\ first-time word, then to the run-time word; the run-time word calls
\ the c function.


require struct.fs

    \ counted-string
    
\ c-function-ft word body:
struct
    cell% field cff-cfr \ xt of c-function-rt word
    cell% field cff-deferred \ xt of c-function deferred word
    cell% field cff-lha \ address of the lib-handle for the lib that
                        \ contains the wrapper function of the word
    char% field cff-rtype  \ return type
    char% field cff-np     \ number of parameters
    1 0   field cff-ptypes \ #npar parameter types
    \  counted string: c-name
end-struct cff%

variable c-source-file-id \ contains the source file id of the current batch
0 c-source-file-id !
variable lib-handle-addr \ points to the library handle of the current batch.
                         \ the library handle is 0 if the current
                         \ batch is not yet compiled.
2variable lib-filename \ filename without extension

: .nb ( n -- )
    0 .r ;

: const+ ( n1 "name" -- n2 )
    dup constant 1+ ;

: front-string { c-addr1 u1 c-addr2 u2 -- c-addr3 u3 }
    \ insert string c-addr2 u2 in buffer c-addr1 u1; c-addr3 u3 is the
    \ remainder of the buffer.
    assert( u1 u2 u>= )
    c-addr2 c-addr1 u2 move
    c-addr1 u1 u2 /string ;

: front-char { c-addr1 u1 c -- c-addr3 u2 }
    \ insert c in buffer c-addr1 u1; c-addr3 u3 is the remainder of
    \ the buffer.
    assert( u1 0 u> )
    c c-addr1 c!
    c-addr1 u1 1 /string ;

: s+ { addr1 u1 addr2 u2 -- addr u }
    u1 u2 + allocate throw { addr }
    addr1 addr u1 move
    addr2 addr u1 + u2 move
    addr u1 u2 +
;

: append { addr1 u1 addr2 u2 -- addr u }
    addr1 u1 u2 + dup { u } resize throw { addr }
    addr2 addr u1 + u2 move
    addr u ;

\ linked list stuff (should go elsewhere)

struct
    cell% field list-next
    1 0   field list-payload
end-struct list%

: list-insert { node list -- }
    list list-next @ node list-next !
    node list list-next ! ;

: list-append { node endlistp -- }
    \ insert node at place pointed to by endlistp
    node endlistp @ list-insert
    node list-next endlistp ! ;

: list-map ( ... list xt -- ... )
    \ xt ( ... node -- ... )
    { xt } begin { node }
	node while
	    node xt execute
	    node list-next @
    repeat ;

\ C prefix lines

\ linked list of longcstrings: [ link | count-cell | characters ]

list%
    cell% field c-prefix-count
    1 0   field c-prefix-chars
end-struct c-prefix%

variable c-prefix-lines 0 c-prefix-lines !
variable c-prefix-lines-end c-prefix-lines c-prefix-lines-end !

: print-c-prefix-line ( node -- )
    dup c-prefix-chars swap c-prefix-count @ type cr ;

: print-c-prefix-lines ( -- )
    c-prefix-lines @ ['] print-c-prefix-line list-map ;

: save-c-prefix-line ( c-addr u -- )
    c-source-file-id @ ?dup-if
	>r 2dup r> write-line throw
    then
    align here 0 , c-prefix-lines-end list-append ( c-addr u )
    longstring, ;

: \c ( "rest-of-line" -- ) \ gforth backslash-c
    \G One line of C declarations for the C interface
    -1 parse save-c-prefix-line ;

s" #include <gforth/" version-string s+ s" /libcc.h>" append ( c-addr u )
  2dup save-c-prefix-line drop free throw

\ Types (for parsing)

wordlist constant libcc-types

get-current libcc-types set-current

\ index values
-1
const+ -- \ end of arguments
const+ n \ integer cell
const+ a \ address cell
const+ d \ double
const+ r \ float
const+ func \ C function pointer
const+ void
drop

set-current

: parse-libcc-type ( "libcc-type" -- u )
    parse-name libcc-types search-wordlist 0= -13 and throw execute ;

: parse-function-types ( "{libcc-type}" "--" "libcc-type" -- )
    here 2 chars allot here begin
	parse-libcc-type dup 0>= while
	    c,
    repeat
    drop here swap - over char+ c!
    parse-libcc-type dup 0< -32 and throw swap c! ;

: type-letter ( n -- c )
    chars s" nadrfv" drop + c@ ;

\ count-stacks

: count-stacks-n ( fp-change1 sp-change1 -- fp-change2 sp-change2 )
    1+ ;

: count-stacks-a ( fp-change1 sp-change1 -- fp-change2 sp-change2 )
    1+ ;

: count-stacks-d ( fp-change1 sp-change1 -- fp-change2 sp-change2 )
    2 + ;

: count-stacks-r ( fp-change1 sp-change1 -- fp-change2 sp-change2 )
    swap 1+ swap ;

: count-stacks-func ( fp-change1 sp-change1 -- fp-change2 sp-change2 )
    1+ ;

: count-stacks-void ( fp-change1 sp-change1 -- fp-change2 sp-change2 )
;

create count-stacks-types
' count-stacks-n ,
' count-stacks-a ,
' count-stacks-d ,
' count-stacks-r ,
' count-stacks-func ,
' count-stacks-void ,

: count-stacks ( pars -- fp-change sp-change )
    \ pars is an addr u pair
    0 0 2swap over + swap u+do
	i c@ cells count-stacks-types + @ execute
    loop ;

\ gen-pars

: gen-par-n ( fp-depth1 sp-depth1 -- fp-depth2 sp-depth2 )
    ." sp[" 1- dup .nb ." ]" ;

: gen-par-a ( fp-depth1 sp-depth1 -- fp-depth2 sp-depth2 )
    ." (void *)(" gen-par-n ." )" ;

: gen-par-d ( fp-depth1 sp-depth1 -- fp-depth2 sp-depth2 )
    ." gforth_d2ll(" gen-par-n ." ," gen-par-n ." )" ;

: gen-par-r ( fp-depth1 sp-depth1 -- fp-depth2 sp-depth2 )
    swap 1- tuck ." fp[" .nb ." ]" ;

: gen-par-func ( fp-depth1 sp-depth1 -- fp-depth2 sp-depth2 )
    gen-par-a ;

: gen-par-void ( fp-depth1 sp-depth1 -- fp-depth2 sp-depth2 )
    -32 throw ;

create gen-par-types
' gen-par-n ,
' gen-par-a ,
' gen-par-d ,
' gen-par-r ,
' gen-par-func ,
' gen-par-void ,

: gen-par ( fp-depth1 sp-depth1 partype -- fp-depth2 sp-depth2 )
    cells gen-par-types + @ execute ;

\ the call itself

: gen-wrapped-call { d: pars d: c-name fp-change1 sp-change1 -- }
    c-name type ." ("
    fp-change1 sp-change1 pars over + swap u+do 
	i c@ gen-par
	i 1+ i' < if
	    ." ,"
	endif
    loop
    2drop ." )" ;

\ calls for various kinds of return values

: gen-wrapped-void ( pars c-name fp-change1 sp-change1 -- fp-change sp-change )
    2dup 2>r gen-wrapped-call 2r> ;

: gen-wrapped-n ( pars c-name fp-change1 sp-change1 -- fp-change sp-change )
    2dup gen-par-n 2>r ." =" gen-wrapped-call 2r> ;

: gen-wrapped-a ( pars c-name fp-change1 sp-change1 -- fp-change sp-change )
    2dup gen-par-n 2>r ." =(Cell)" gen-wrapped-call 2r> ;

: gen-wrapped-d ( pars c-name fp-change1 sp-change1 -- fp-change sp-change )
    ." gforth_ll2d(" gen-wrapped-void
    ." ," gen-par-n ." ," gen-par-n ." )" ;

: gen-wrapped-r ( pars c-name fp-change1 sp-change1 -- fp-change sp-change )
    2dup gen-par-r 2>r ." =" gen-wrapped-void 2r> ;

: gen-wrapped-func ( pars c-name fp-change1 sp-change1 -- fp-change sp-change )
    gen-wrapped-a ;

create gen-wrapped-types
' gen-wrapped-n ,
' gen-wrapped-a ,
' gen-wrapped-d ,
' gen-wrapped-r ,
' gen-wrapped-func ,
' gen-wrapped-void ,

: gen-wrapped-stmt ( pars c-name fp-change1 sp-change1 ret -- fp-change sp-change )
    cells gen-wrapped-types + @ execute ;

: wrapper-function-name ( addr -- c-addr u )
    \ addr points to the return type index of a c-function descriptor
    count { r-type } count { d: pars }
    pars + count { d: c-name }
    s" gforth_c_" { d: prefix }
    prefix nip c-name nip + pars nip + 3 + { u }
    u allocate throw { c-addr }
    c-addr u
    prefix front-string c-name front-string '_ front-char
    pars bounds u+do
	i c@ type-letter front-char
    loop
    '_ front-char r-type type-letter front-char assert( dup 0= )
    2drop c-addr u ;

: gen-wrapper-function ( addr -- )
    \ addr points to the return type index of a c-function descriptor
    dup { descriptor }
    count { ret } count 2dup { d: pars } chars + count { d: c-name }
    ." void " descriptor wrapper-function-name 2dup type drop free throw
    .\" (void)\n"
    .\" {\n  Cell MAYBE_UNUSED *sp = gforth_SP;\n  Float MAYBE_UNUSED *fp = gforth_FP;\n  "
    pars c-name 2over count-stacks ret gen-wrapped-stmt .\" ;\n"
    ?dup-if
	."   gforth_SP = sp+" .nb .\" ;\n"
    endif
    ?dup-if
	."   gforth_FP = fp+" .nb .\" ;\n"
    endif
    .\" }\n" ;

: tempdir ( -- c-addr u )
    s" TMPDIR" getenv dup 0= if
        2drop s" /tmp"
    then ;

: gen-filename ( x -- c-addr u )
    \ generates a filename without extension for lib-handle-addr X
    0 <<# ['] #s $10 base-execute #> 
    tempdir s" /gforth-c-" s+ 2swap append #>> ;

: init-c-source-file ( -- )
    c-source-file-id @ 0= if
	here 0 , dup lib-handle-addr ! gen-filename 2dup lib-filename 2!
	s" .c" s+ 2dup w/o create-file throw dup c-source-file-id !
        ['] print-c-prefix-lines swap outfile-execute
        drop free throw
    endif ;

: c-source-file ( -- file-id )
    c-source-file-id @ assert( dup ) ;

: compile-wrapper-function ( -- )
    c-source-file close-file throw
    0 c-source-file-id !
    s" gcc -fPIC -shared -Wl,-soname," lib-filename 2@ s+
    s" .so.1 -Wl,-export_dynamic -o " append lib-filename 2@ append
    [ s" .so.1 -O -I " s" includedir" getenv append s"  " append ] sliteral
    append lib-filename 2@ append s" .c" append ( c-addr u )
    2dup system drop free throw
    $? abort" compiler generated error" \ !! call dlerror
    lib-filename 2@ s" .so.1" s+
    2dup open-lib dup 0= abort" open-lib failed" \ !! call dlerror
    ( lib-handle ) lib-handle-addr @ !
    2dup delete-file throw drop free throw
    lib-filename 2@ s" .c" s+ 2dup delete-file throw drop free throw
    lib-filename 2@ drop free throw 0 0 lib-filename 2! ;
\    s" ar rcs xxx.a xxx.o" system
\    $? abort" ar generated error" ;

: link-wrapper-function { cff -- sym }
    cff cff-rtype wrapper-function-name { d: wrapper-name }
    wrapper-name cff cff-lha @ @ assert( dup ) lib-sym dup 0= -&32 and throw
    wrapper-name drop free throw ;

: c-function-ft ( xt-defr xt-cfr "c-name" "{libcc-type}" "--" "libcc-type" -- )
    \ build time/first time action for c-function
    init-c-source-file
    noname create 2, lib-handle-addr @ ,
    parse-name { d: c-name }
    here parse-function-types c-name string,
    ['] gen-wrapper-function c-source-file outfile-execute
  does> ( ... -- ... )
    dup 2@ { xt-defer xt-cfr }
    dup cff-lha @ @ 0= if
	compile-wrapper-function
    endif
    link-wrapper-function xt-cfr >body !
    xt-cfr xt-defer defer!
    xt-cfr execute ;

: c-function-rt ( -- )
    \ run-time definition for c function; addr is the address where
    \ the sym should be stored
    noname create 0 ,
  does> ( ... -- ... )
    @ call-c ;

: c-function ( "forth-name" "c-name" "@{type@}" "--" "type" -- ) \ gforth
    \G Define a Forth word @i{forth-name}.  @i{Forth-name} has the
    \G specified stack effect and calls the C function @code{c-name}.
    defer lastxt dup c-function-rt lastxt c-function-ft
    lastxt swap defer! ;
