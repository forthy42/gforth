\ libcc.fs	foreign function interface implemented using a C compiler

\ Authors: Bernd Paysan, Anton Ertl, David Kühling
\ Copyright (C) 2006,2007,2008,2009,2010,2011,2012,2013,2014,2015,2016,2017,2018,2019,2020,2021,2022,2023,2024 Free Software Foundation, Inc.

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


\ What this implementation does is this: if it sees a declaration like

\ \ something that tells it that the current library is libc
\ \c #include <unistd.h>
\ c-function dlseek lseek n d n -- d

\ it genererates C code similar to the following:

\ #include <gforth.h>
\ #include <unistd.h>
\ 
\ gforth_stackpointers gforth_c_lseek_ndn_d(gforth_stackpointers x, void* addr)
\ {
\   long long result;  /* longest type in C */
\   gforth_ll2d(lseek(x.spx[3],gforth_d2ll(x.spx[2],x.spx[1]),x.spx[0]),x.spx[3],x.spx[2]);
\   x.spx += 2;
\   return x;
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

\ The files are built in .../lib/gforth/$VERSION/$machine/libcc/ or
\ ~/.cache/gforth/libcc/$machine/.

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

[ifundef] parse-name
    ' parse-word alias parse-name
[then]
?: defer! ( xt xt-deferred -- ) \ gforth  defer-store
\G Changes the @code{defer}red word @var{xt-deferred} to execute @var{xt}.
    >body [ has? rom [IF] ] @ [ [THEN] ] ! ;

\ : delete-file 2drop 0 ;

require struct.fs
require mkdir.fs
require string.fs

\ these words are generally useful and used by at least one user

: scan-back { c-addr u1 c -- c-addr u2 } \ gforth
    \G The last occurrence of @i{c} in @i{c-addr u1} is at
    \G @i{c-addr}+@i{u2}@minus{}1; if it does not occur, @i{u2}=0.
    c-addr u1 1 mem-do
	i c@ c = if
	    c-addr i over - 1+ unloop exit endif
    loop
    c-addr 0 ;

: dirname ( c-addr1 u1 -- c-addr1 u2 ) \ gforth
    \G @i{C-addr1 u2} is the directory name of the file name
    \G @i{c-addr1 u1}, including the final @code{/}.  If @i{caddr1 u1}
    \G does not contain a @code{/}, @i{u2}=0.
    '/ scan-back ;

: basename ( c-addr1 u1 -- c-addr2 u2 ) \ gforth
    \G Given a file name @i{c-addr1 u1}, @i{c-addr2 u2} is the part of
    \G it with any leading directory components removed.
    2dup dirname nip /string ;

\ stubs for 0.7-style usage without C-LIBRARY

s" Must now be used inside C-LIBRARY, see C interface doc" exception
constant !!0.7-style!!

: \c !!0.7-style!! throw ;
synonym c-function \c
synonym add-lib \c
synonym clear-libs \c

: get-host? ( -- flag )  s" HOSTPREFIX" getenv nip 0=
    s" GFORTH_IGNLIB" getenv s" true" str= 0= and ;
get-host? Value host?

Vocabulary c-lib

get-current also c-lib definitions

s" libtool compile failed" exception Constant !!libcompile!!
s" libtool link failed"    exception Constant !!liblink!!
s" open-lib failed"        exception Constant !!openlib!!
s" Too many callbacks!"    exception Constant !!callbacks!!
s" Called function of unfinished named C library"
                           exception Constant !!unfinished!!

Variable libcc$ \ source string for libcc generated source

\ c-function-ft word body:
begin-structure cff%
    field: cff-c-call \ c function pointer
    field: cff-lha    \ address of the lib-handle for the lib that
                           \ contains the wrapper function of the word
    cfield: cff-ctype  \ call type (function=1, value=0)
    cfield: cff-rtype  \ return type
    cfield: cff-np     \ number of parameters
    0 +field cff-ptypes \ #npar parameter types
    \  counted string: c-name
end-structure

begin-structure ccb%
    field: ccb-num
    field: ccb-lha
    field: ccb-ips
    field: ccb-cfuns
end-structure

begin-structure lha%
    field: lha-id    \ open-lib returned library id
    field: lha-next  \ link to next library in chain
    field: lha-name  \ library name string
    $10 +field lha-hash
end-structure

variable c-source-file-id \ contains the source file id of the current batch
0 c-source-file-id !
variable lib-handle-addr \ points to the library handle of the current batch.
                         \ the library handle is 0 if the current
                         \ batch is not yet compiled.
Variable lib-filename   \ filename without extension
: lib-modulename ( -- addr ) lib-handle-addr @ lha-name ;
: lib-handle ( -- addr )     lib-handle-addr @ lha-id @ ;
: lib-handle! ( addr -- )    lib-handle-addr @ lha-id ! ;
: c-source-hash ( -- addr )  lib-handle-addr @ lha-hash ;
\ basename of the file without extension
variable libcc-named-dir$ \ directory for named libcc wrapper libraries
Variable libcc-path      \ pointer to path of library directories
Variable ptr-declare

defer replace-rpath ( c-addr1 u1 -- c-addr2 u2 )
' noop is replace-rpath

: .nb ( n -- )
    0 .r ;

: const+ ( n1 "name" -- n2 )
    dup constant 1+ ;

Variable c-flags \ include flags
Variable c-libs \ library names in a string (without "lib")
0 Value c++-mode

: lib-prefix ( -- addr u )  s" libgf" ;

: add-cflags ( c-addr u -- ) \ gforth
    \G add any kind of cflags to compilation
    [: space type ;] c-flags $exec ;

: add-incdir ( c-addr u -- ) \ gforth
    \G Add path @i{c-addr u} to the list of include search pathes
    [: ."  -I" type ;] c-flags $exec ;

: add-ldflags ( c-addr u -- ) \ gforth
    \G add flag to linker
    [: space type ;] c-libs $exec ;

: add-lib ( c-addr u -- ) \ gforth
    \G Add library lib@i{string} to the list of libraries, where
    \G @i{string} is represented by @i{c-addr u}.
    [: ."  -l" type ;] c-libs $exec ;

: add-framework ( c-addr u -- ) \ gforth
    \G Add framework lib@i{string} to the list of frameworks, where
    \G @i{string} is represented by @i{c-addr u}.
    [: ."  -framework " type ;] c-libs $exec ;

: add-libpath ( c-addr u -- ) \ gforth
\G Add path @i{string} to the list of library search pathes, where
    \G @i{string} is represented by @i{c-addr u}.
    [: ."  -L" type ;] c-libs $exec ;

\ C prefix lines

: c-source-file-execute ( ... xt -- ... )
    libcc$ $exec ;

: write-c-prefix-line ( c-addr u -- )
    [: type cr ;] c-source-file-execute ;

: save-c-prefix-line ( addr u -- )  write-c-prefix-line ;

: \c ( "rest-of-line" -- ) \ gforth backslash-c
    \G One line of C declarations for the C interface
    -1 parse write-c-prefix-line ;

: libcc-include ( -- )
    [: ." #include <libcc.h>" cr
\      ." #include <stdio.h>" cr
    ;] c-source-file-execute ;

\ Types (for parsing)

wordlist constant libcc-types

Variable vararg$

get-current libcc-types set-current

0 warnings !@ \ no warnings for 0 as null pointer
\ index values
-1
const+ -- \ end of arguments
const+ n \ integer cell
const+ u \ integer cell
const+ a \ address cell
const+ d \ double
const+ ud \ double
const+ r \ float
const+ func \ C function pointer
const+ void \ no return value
const+ s \ string
const+ ws \ wide string
const+ t \ tuple
const+ 0 \ NULL pointer (sentinel)
const+ ... \ varargs (programmable)
drop
warnings !

set-current

\ call types
0
const+ c-func
const+ c-val
const+ c-var
drop

: libcc-type ( c-addr u -- u2 )
    libcc-types find-name-in ?found execute ;

: >libcc-type ( c-addr u -- u2 )
    2dup '{' scan-back
    dup IF  2nip 1- 2dup + source drop - >in !  ELSE  2drop  THEN
    libcc-type ;

: parse-libcc-type ( "libcc-type" -- u )
    ?parse-name >libcc-type ;

: parse-libcc-cast ( "<{>cast<}>" -- addr u )
    source >in @ /string IF  c@ '{' =  IF
	    '{' parse 2drop '}' parse
	ELSE  s" "  THEN
    ELSE  drop  s" "  THEN ;

: libcc-cast, ( "<{>cast<}>" -- )
    parse-libcc-cast string, ;

: parse-return-type ( "libcc-type" -- u )
    parse-libcc-type dup 0< -32 and throw ;

: ...-types, ( -- )
    vararg$ $@ [:
	BEGIN  parse-name dup WHILE
		>libcc-type c, libcc-cast,  REPEAT
	2drop ;] execute-parsing ;

: function-types, ( "{libcc-type}" "--" -- )
    begin
	parse-libcc-type dup 0>= while
	    dup [ libcc-types >order ... previous ]L =
	    IF
		drop ...-types,
	    ELSE
		c, libcc-cast, \ cast string
	    THEN
    repeat drop ;

: parse-function-types ( "{libcc-type}" "--" "libcc-type" -- addr )
    c-func c, here
    dup 2 chars allot here function-types,
    here swap - over char+ c!
    parse-return-type swap c! libcc-cast, ;

: parse-value-type ( "{--}" "libcc-type" -- addr )
    c-val c, here
    parse-libcc-type  dup 0< if drop parse-return-type then
    c, libcc-cast, 0 c, ( terminator ) ;

: parse-variable-type ( -- addr )
    c-var c, here
    s" a" libcc-type c, 0 c, 0 c, ;

0 Value is-funptr?
0 Value is-weak?

: type-letter ( n -- c )
    chars s" nuadUrfvsSt" drop + c@ ;

\ count-stacks

: count-stacks-n ( fp-change1 sp-change1 -- fp-change2 sp-change2 )
    1+ ;

: count-stacks-u ( fp-change1 sp-change1 -- fp-change2 sp-change2 )
    1+ ;

: count-stacks-a ( fp-change1 sp-change1 -- fp-change2 sp-change2 )
    1+ ;

: count-stacks-d ( fp-change1 sp-change1 -- fp-change2 sp-change2 )
    2 + ;

: count-stacks-ud ( fp-change1 sp-change1 -- fp-change2 sp-change2 )
    2 + ;

: count-stacks-r ( fp-change1 sp-change1 -- fp-change2 sp-change2 )
    1 under+ ;

: count-stacks-func ( fp-change1 sp-change1 -- fp-change2 sp-change2 )
    1+ ;

: count-stacks-void ( fp-change1 sp-change1 -- fp-change2 sp-change2 )
;

: count-stacks-s ( fp-change1 sp-change1 -- fp-change2 sp-change2 )
    2 + ;

: count-stacks-ws ( fp-change1 sp-change1 -- fp-change2 sp-change2 )
    2 + ;

: count-stacks-t ( fp-change1 sp-change1 -- fp-change2 sp-change2 )
    1+ ;

create count-stacks-types
' count-stacks-n ,
' count-stacks-u ,
' count-stacks-a ,
' count-stacks-d ,
' count-stacks-ud ,
' count-stacks-r ,
' count-stacks-func ,
' count-stacks-void ,
' count-stacks-s ,
' count-stacks-ws ,
' count-stacks-t ,
' noop ,

: count-stacks ( pars -- fp-change sp-change )
    \ pars is an addr u pair
    0 0 2swap over + swap u+do
	i c@ cells count-stacks-types + @ execute
    i 1+ c@ 2 + +loop ;

\ gen-pars

: .gen ( n -- n' )  1- dup .nb ;

: gen-par-sp ( fp-depth1 sp-depth1 -- fp-depth2 sp-depth2 )
    ." x.spx[" .gen ." ]" ;

#0. 2Value r-cast

: *gen-par-sp++ ( fp-depth1 sp-depth1 -- fp-depth2 sp-depth2 )
    r-cast type ." (x.spx[" 1+ .gen ." ])" 1+ ;

: gen-par-sp+ ( fp-depth1 sp-depth1 -- fp-depth2 sp-depth2 )
    ." x.spx+" .gen ;

: gen-par-fp ( fp-depth1 sp-depth1 -- fp-depth2 sp-depth2 )
    swap ." x.fpx[" .gen ." ]" swap ;

: gen-par-n ( fp-depth1 sp-depth1 cast-addr u -- fp-depth2 sp-depth2 )
    type gen-par-sp ;

: gen-par-u ( fp-depth1 sp-depth1 cast-addr u -- fp-depth2 sp-depth2 )
    type gen-par-sp ;

: gen-par-a ( fp-depth1 sp-depth1 cast-addr u -- fp-depth2 sp-depth2 )
    dup 0= IF  2drop ." (void *)"  ELSE
	2dup type s"   return " str= IF  ." (void *)"  THEN
    THEN s" (" gen-par-n ." )" ;

: ?return ( cast-addr u -- )
    2dup  s"   return " str= IF  type  ELSE  2drop  THEN ;

: gen-par-d ( fp-depth1 sp-depth1 cast-addr u -- fp-depth2 sp-depth2 )
    ?return s" gforth_d2ll(" gen-par-n s" ," gen-par-n ." )" ;

: gen-par-ud ( fp-depth1 sp-depth1 cast-addr u -- fp-depth2 sp-depth2 )
    ?return s" gforth_d2ll(" gen-par-n s" ," gen-par-n ." )" ;

: gen-par-r ( fp-depth1 sp-depth1 cast-addr u -- fp-depth2 sp-depth2 )
    ?return gen-par-fp ;

: gen-par-func ( fp-depth1 sp-depth1 cast-addr u -- fp-depth2 sp-depth2 )
    gen-par-a ;

: gen-par-void ( fp-depth1 sp-depth1 cast-addr u -- fp-depth2 sp-depth2 )
    2drop ;

: gen-par-s ( fp-depth1 sp-depth1 cast-addr u -- fp-depth2 sp-depth2 )
    ?return s" gforth_str2c((Char*)" gen-par-n s" ," gen-par-n ." )" ;

: gen-par-ws ( fp-depth1 sp-depth1 cast-addr u -- fp-depth2 sp-depth2 )
    ?return s" gforth_str2wc((Char*)" gen-par-n s" ," gen-par-n ." )" ;

: gen-par-0 ( fp-depth1 sp-depth1 cast-addr u -- fp-depth2 sp-depth2 )
    ?return ." NULL" ;

: gen-par-t ( fp-depth1 sp-depth1 cast-addr u -- fp-depth2 sp-depth2 )
    dup 0= IF  2drop ." (void *)"  ELSE
	." *(" type ." *)"
    THEN s" (" gen-par-n ." )" ;

create gen-par-types
' gen-par-n ,
' gen-par-u ,
' gen-par-a ,
' gen-par-d ,
' gen-par-ud ,
' gen-par-r ,
' gen-par-func ,
' gen-par-void ,
' gen-par-s ,
' gen-par-ws ,
' gen-par-t ,
' gen-par-0 ,

: gen-par ( fp-depth1 sp-depth1 cast-addr u partype -- fp-depth2 sp-depth2 )
    cells gen-par-types + @ execute ;

\ the call itself

: gen-call-func { d: pars d: c-name fp-change1 sp-change1 -- }
    c-name type ." ("
    fp-change1 sp-change1 pars over + swap u+do 
	i 1+ count i c@ gen-par
	i 1+ c@ 2 + dup delta-I u< if
	    ." ,"
	endif
    +loop
    2drop ." )" ;

: gen-call-const { d: pars d: c-name fp-change1 sp-change1 -- }
    ." (" c-name type ." )" ;

: gen-call-var { d: pars d: c-name fp-change1 sp-change1 -- }
    ." &(" c-name type ." )" ;

create gen-call-types
' gen-call-func ,
' gen-call-const ,
' gen-call-var ,

: gen-wrapped-call ( pars c-name fp-change1 sp-change1 -- )
    5 pick 3 chars - c@ cells gen-call-types + @ execute ;

\ calls for various kinds of return values

: gen-wrapped-void ( pars c-name fp-change1 sp-change1 -- fp-change sp-change )
    2dup 2>r gen-wrapped-call 2r> ;

: gen-wrapped-n ( pars c-name fp-change1 sp-change1 -- fp-change sp-change )
    2dup gen-par-sp 2>r ." =" gen-wrapped-call 2r> ;

: gen-wrapped-u ( pars c-name fp-change1 sp-change1 -- fp-change sp-change )
    2dup gen-par-sp 2>r ." =" gen-wrapped-call 2r> ;

: gen-wrapped-a ( pars c-name fp-change1 sp-change1 -- fp-change sp-change )
    2dup gen-par-sp 2>r ." =(Cell)" gen-wrapped-call 2r> ;

: gen-wrapped-s ( pars c-name fp-change1 sp-change1 -- fp-change sp-change )
    ." c_str2gforth_str(" gen-wrapped-void
    ." ," gen-par-sp ." ," gen-par-sp ." )" ;

: gen-wrapped-ws ( pars c-name fp-change1 sp-change1 -- fp-change sp-change )
    ." wc_str2gforth_str(" gen-wrapped-void
    ." , (Char**)&(" gen-par-sp ." ), (UCell*)&(" gen-par-sp ." ))" ;

: gen-wrapped-d ( pars c-name fp-change1 sp-change1 -- fp-change sp-change )
    ." gforth_ll2d(" gen-wrapped-void
    ." ," gen-par-sp ." ," gen-par-sp ." )" ;

: gen-wrapped-ud ( pars c-name fp-change1 sp-change1 -- fp-change sp-change )
    ." gforth_ll2ud(" gen-wrapped-void
    ." ," gen-par-sp ." ," gen-par-sp ." )" ;

: gen-wrapped-r ( pars c-name fp-change1 sp-change1 -- fp-change sp-change )
    2dup gen-par-fp 2>r ." =" gen-wrapped-call 2r> ;

: gen-wrapped-func ( pars c-name fp-change1 sp-change1 -- fp-change sp-change )
    gen-wrapped-a ;

: gen-wrapped-t ( pars c-name fp-change1 sp-change1 -- fp-change sp-change )
    2dup *gen-par-sp++ 2>r ." =" gen-wrapped-call 2r> ;

create gen-wrapped-types
' gen-wrapped-n ,
' gen-wrapped-u ,
' gen-wrapped-a ,
' gen-wrapped-d ,
' gen-wrapped-ud ,
' gen-wrapped-r ,
' gen-wrapped-func ,
' gen-wrapped-void ,
' gen-wrapped-s ,
' gen-wrapped-ws ,
' gen-wrapped-t ,
' gen-wrapped-void ,

: gen-wrapped-stmt ( pars c-name fp-change1 sp-change1 ret -- fp-change sp-change )
    cells gen-wrapped-types + @ execute ;

: sanitize ( addr u -- )
    bounds ?DO
	I c@
	dup 'a' 'z' 1+ within
	over 'A' 'Z' 1+ within or
	swap '0' '9' 1+ within or
	0= IF  '_' I c!  THEN
    LOOP ;

: wrapper-function-name ( addr -- c-addr u )
    \ addr points to the return type index of a c-function descriptor
    [: ." gforth_c_"
    count { r-type } count { d: pars }
    pars + count + count type '_' emit
    pars bounds u+do
	i c@ type-letter emit
    i 1+ c@ 2 + +loop
    '_' emit r-type type-letter emit
    ;] $tmp 2dup sanitize ;

: .prefix ( -- )
    [ lib-suffix s" .la" str= [IF] ] lib-prefix type
	lib-modulename $@ dup 0= IF 2drop s" _replace_this_with_the_hash_code" THEN type
	." _LTX_" [ [THEN] ] ;

: >ptr-declare ( c-name u1 -- addr u2 )
    s" *x.spx++" 2swap \ default is fetch ptr from stack
    ptr-declare [: ( decl u1 c-name u2 ptr-name u3 -- decl' u1' c-name u2 )
	2>r 2dup 2r> ':' $split 2>r string-prefix?
	IF  2nip 2r> 2swap  ELSE  2rdrop THEN ;] $[]map 2drop ;

: .externc ( -- )
    c++-mode IF .\" extern \"C\" " THEN ;

: gen-wrapper-function ( addr -- )
    \ addr points to the return type index of a c-function descriptor
    dup { descriptor }
    count { ret }
    count 2dup { d: pars }
    + count 2dup to r-cast
    + count { d: c-name }
    is-weak? IF  ." #pragma weak " c-name type cr  THEN
    .externc ." gforth_stackpointers " .prefix
    descriptor wrapper-function-name type
    .\" (GFORTH_ARGS)\n{\n"
    pars c-name 2over count-stacks
    .\"   ARGN(" dup 1- .nb .\" ," over 1- .nb .\" );\n  "
    is-funptr? IF  ." Cell ptr = " c-name >ptr-declare type .\" ;\n  "  THEN
    is-weak? IF  ." if(" c-name type .\" ) {\n    "  THEN
    ret gen-wrapped-stmt .\" ;\n"
    is-weak? IF  .\" } else { gforth_fail(); }\n"  THEN
    dup is-funptr? or if
	."   x.spx += " dup .nb .\" ;\n"
    endif drop
    ?dup-if
	."   x.fpx += "     .nb .\" ;\n"
    endif
    .\"   return x;\n}\n"
    0 to is-weak? ;

\ callbacks

: gen-n ( -- ) ." Cell" ;
: gen-u ( -- ) ." UCell" ;
: gen-a ( -- ) ." void*" ;
: gen-d ( -- ) ." Clongest" ;
: gen-ud ( -- ) ." UClongest" ;
: gen-r ( -- ) ." Float" ;
: gen-func ( -- ) ." void(*)()" ;
: gen-void ( -- ) ." void" ;

create gen-types
' gen-n ,
' gen-u ,
' gen-a ,
' gen-d ,
' gen-ud ,
' gen-r ,
' gen-func ,
' gen-void ,
' gen-a ,
' gen-a ,
' gen-a ,

: print-type ( n -- ) cells gen-types + perform ;

: callback-header ( descriptor -- )
    count { ret } count 2dup { d: pars } chars + count + count { d: c-name }
    ." #define CALLBACK_" c-name type ." (I) \" cr
    .externc ret print-type space .prefix ." gforth_cb_" c-name type ." _##I ("
    0 pars bounds u+do
	i 1+ count dup IF
	    2dup s" *(" string-prefix? IF
		2 /string  2 - 0 max
	    THEN  type
	ELSE  2drop i c@ print-type  THEN
	."  x" dup 0 .r 1+
	i 1+ c@ 2 + dup delta-I u< if
	    ." , "
	endif
    +loop  drop .\" ) \\\n{ \\" cr ;

Create callback-style c-val c,
Create callback-&style c-var c,

: callback-pushs ( descriptor -- )
    1+ count 0 { d: pars vari }
    0 0 pars bounds u+do
	I 1+ c@  IF  callback-&style  ELSE  callback-style  THEN
	3 + 1 2swap
	vari 0 <# #s 'x' hold #> 2swap
	i c@ 2 spaces gen-wrapped-stmt ." ; \" cr
	i 1+ c@ 2 +  vari 1+ to vari
    +loop
    ?dup-if  ."   x.spx+=" .nb ." ; \" cr  then
    ?dup-if  ."   x.fpx+=" .nb ." ; \" cr  then ;

: callback-call ( descriptor -- )
    1+ count + count + count \ callback C name
\    .\"   fprintf(stderr, \"Calling IP=%p\\n\", " .prefix ." gforth_cbips_" 2dup type ." [I]); \" cr
    ."   gforth_engine(" .prefix ." gforth_cbips_" type
    ." [I], &x); \" cr ;

: gen-par-callback ( sp-change1 sp-change1 addr u type -- fp-change sp-change )
    dup [ libcc-types >order ] void [ previous ] =
    IF  drop 2drop  ELSE  gen-par  THEN ;

: callback-wrapup ( -- ) ;

: callback-return ( descriptor -- )
    >r 0 0 r@ c@ cells count-stacks-types + perform
    s"   return " r> c@ gen-par-callback 2drop .\" ; \\\n}" cr ;

: callback-wrapper ( -- )
    ."   stackpointers x; \" cr
    ."   Cell stack[GFSS+8], rstack[GFSS], lstack[GFSS]; Float fstack[GFSS+2]; \" cr
    ."   x.spx=stack+GFSS; x.rpx=rstack+GFSS; x.lpx=(Address)(lstack+GFSS); x.fpx=fstack+GFSS; x.upx=gforth_main_UP; x.magic=GFORTH_MAGIC; \" cr
    ."   x.handler=0; x.first_throw = ~0; x.wraphandler=0; \" cr ;

: callback-thread-define ( descriptor -- )
    dup callback-header callback-wrapper
    dup callback-pushs dup callback-call
    callback-wrapup callback-return ;

' callback-thread-define alias callback-define

2 Value callback# \ how many callbacks should be created?

: callback-instantiate ( addr u -- )
    callback# 0 ?DO
	." CALLBACK_" 2dup type ." (" I .nb ." )" cr
    LOOP 2drop ;

: callback-ip-array ( addr u -- )
    .externc ." Xt* " .prefix ." gforth_cbips_" 2dup type ." [" callback# .nb ." ] = {" cr
    space callback# 0 ?DO ."  0," LOOP ." };" cr 2drop ;

: callback-c-array ( addr u -- )
    .externc ." const Address " .prefix ." gforth_callbacks_" 2dup type ." [" callback# .nb ." ] = {" cr
    callback# 0 ?DO
	."   (Address)" .prefix ." gforth_cb_" 2dup type ." _" I .nb ." ," cr
    LOOP
    ." };" cr 2drop ;

: callback-gen ( descriptor -- )
    dup callback-define  1+ count + count + count \ c-name u
    2dup callback-ip-array 2dup callback-instantiate callback-c-array ;

: callback-thread-gen ( descriptor -- )
    dup callback-thread-define  1+ count + count + count \ c-name u
    2dup callback-ip-array 2dup callback-instantiate callback-c-array ;

: lookup-ip-array ( addr u lib -- addr )
    >r [: ." gforth_cbips_" type ;] $tmp r> lib-sym ;

: lookup-c-array ( addr u lib -- addr )
    >r [: ." gforth_callbacks_" type ;] $tmp r> lib-sym ;

\ file stuff

: libcc-named-dir ( -- c-addr u )
    libcc-named-dir$ $@ ;

: >libcc-named-dir ( addr u -- )
    libcc-named-dir$ $! ;

: libcc-tmp-dir ( -- c-addr u )
    [:  s" XDG_CACHE_HOME" getenv dup IF  type  ELSE  2drop ." ~/.cache"  THEN
	." /gforth/" machine type ." /libcc-tmp/" ;] $tmp ;

: prepend-dirname ( c-addr1 u1 c-addr2 u2 -- c-addr3 u3 )
    [: type type ;] $tmp ;

: c-hash-ok? ( -- addr1 addr2 flag )
    [: ." gflibcc_hash_" lib-modulename $. ;] $tmp
    lib-handle lib-sym
    ?dup-IF  c-source-hash 2dup $10 tuck str=  ELSE  0 0 false  THEN ;

: .xx ( n -- ) 0 [: <<# # # #> type #>> ;] $10 base-execute ;
: .hashxx ( addr u -- ) bounds DO  I c@ .xx  LOOP ;
: .bytes ( addr u -- )
    false -rot bounds ?DO  IF ',' emit  THEN  ." 0x" I c@ .xx true  LOOP drop ;

: .hash-check ( addr1 addr2 -- )
    2dup d0= IF  2drop
	[: ." libcc module " lib-modulename $. ."  doesn't have a hash value" cr ;]
    ELSE  [: ." libcc hash mismatch in module '"
	    lib-modulename $. ." ': expected " 16 .hashxx
	    ."  got " 16 .hashxx cr ;]
    THEN  do-debug ;

: check-c-hash ( -- flag )
    c-hash-ok?
    IF  2drop true
    ELSE  .hash-check
	lib-handle close-lib  0 lib-handle!  false
  THEN ;

: open-olib ( addr u -- file-id ior )
    ofile $@ open-lib
    warnings @ abs 3 >= IF
	[: cr ." try open lib: " ofile $. ."  result: " dup hex. ;] do-debug
    THEN
    dup IF
	lib-handle!
	c-hash-ok? IF  2drop lib-handle 0  EXIT  THEN
	warnings @ abs 2 >= IF  .hash-check  ELSE  2drop  THEN
	lib-handle close-lib  0 lib-handle!  0
    THEN  #-514 ;

: open-path-lib ( addr u -- addr/0 )
    \ This assumes that there's a current valid lib-handle-addr and the
    \ c source hash is computed.  Only libraries with the correct hash
    \ will be opened, other libraries will be skipped and the next in path
    \ is searched.
    ['] open-olib libcc-path execute-path-file
    IF  0  ELSE  2drop  THEN ;

: preopen-path-lib ( addr u -- addr/0 )
    \ This opens a library in the path and doesn't check for the hash
    [: ofile $@ open-lib dup IF  0  EXIT  THEN  #-514 ;]
    libcc-path execute-path-file
    IF  0  ELSE  2drop  THEN ;

: lib-name ( -- addr u )
    [: lib-filename $@ dirname type lib-prefix type
	lib-filename $@ basename type lib-suffix type ;] $tmp ;
: open-wrappers ( -- addr|0 )
    lib-name 2dup libcc-named-dir string-prefix? if ( c-addr u )
	\ see if we can open it in the path
	libcc-named-dir nip /string open-path-lib EXIT
    endif
    open-lib ;

: c-library-name-setup ( c-addr u -- )
    assert( c-source-file-id @ 0= )
    { d: filename }
    filename lib-filename $!
    filename basename lib-modulename $! lib-modulename $@ sanitize ;

: c-library-name-create ( -- )
    libcc-named-dir $1ff mkdir-parents drop
    [: lib-filename $. ." .c" c++-mode IF ." pp" THEN ;] $tmp
    r/w create-file throw
    c-source-file-id ! ;

: c-named-library-name ( c-addr u -- )
    \ set up filenames for a (possibly new) library; c-addr u is the
    \ basename of the library
    libcc-named-dir prepend-dirname c-library-name-setup ;

: c-tmp-library-name ( c-addr u -- )
    \ set up filenames for a new library; c-addr u is the basename of
    \ the library
    libcc-tmp-dir 2dup $1ff mkdir-parents drop
    prepend-dirname c-library-name-setup
    open-wrappers lib-handle! ;

: c-source-file ( -- file-id )
    c-source-file-id @ assert( dup ) ;

: .lib-error ( -- )
    [: cr lib-name type ." :"
    [ifdef] lib-error
         cr lib-error type
    [then] ;] do-debug ;

\ hashing

: replace-hash { addr u -- }
    libcc$ $@  BEGIN  s" _replace_this_with_the_hash_code" search  WHILE
	    addr third u move $20 /string  REPEAT
    2drop ;

: .c-hash ( -- )
    lib-filename @ 0= IF
	true warning" Generate anonymous C binding"
	[: c-source-hash 16 .hashxx ;] $tmp
	c-tmp-library-name
	lib-modulename $@ replace-hash
    THEN
    ." hash_128 gflibcc_hash_" lib-modulename $.
    .\"  = { " c-source-hash 16 .bytes .\"  };" cr ;

: hash-c-source ( -- )
    c-source-hash 16 erase
    libcc$ $@ false c-source-hash hashkey2
    ['] .c-hash c-source-file-execute ;

DEFER compile-wrapper-function ( -- )

: lha, ( -- )
    \ create an empty library handle
    align here 0 , lib-handle-addr @ , here $saved 0 , $10 allot  lib-handle-addr ! ;

: free-libs ( -- ) \ gforth-internal
    ptr-declare off  c-libs off  c-flags off
    libcc$ $free  libcc-include ;

: clear-libs ( -- ) \ gforth
\G Clear the list of libs
    c-source-file-id @ if
	compile-wrapper-function
    endif
    lib-handle-addr @ dup if
	lha-id @ 0=
    endif
    0= if
	lha,
    endif
    free-libs
    0 to c++-mode ;
: end-libs ( -- )
    ptr-declare $[]free
    vararg$ $free  c-flags $free  c-libs $free ;
clear-libs
end-libs

\ compilation wrapper

tmp$ $execstr-ptr !

: compile-cmd ( -- )
    c++-mode IF
	[ libtool-command tmp$ $! s"  --silent --tag=CXX --mode=compile " $type
	s" CROSS_PREFIX" getenv $type
	libtool-cxx $type s"  '-I" $type
	s" includedir" getenv tuck $type 0= [IF]
	    pad $100 get-dir $type s" /" $type version-string $type
	    s" /include" $type  [THEN]
	s" '" $type s" extrastuff" getenv $type
	tmp$ $@
	\ cr ." Libcc command: " 2dup type cr
	] sliteral
    ELSE
	[ libtool-command tmp$ $! s"  --silent --tag=CC --mode=compile " $type
	s" CROSS_PREFIX" getenv $type
	libtool-cc $type s"  '-I" $type
	s" includedir" getenv tuck $type 0= [IF]
	    pad $100 get-dir $type s" /" $type version-string $type
	    s" /include" $type  [THEN]
	s" '" $type s" extrastuff" getenv $type
	tmp$ $@
	\ cr ." Libcc command: " 2dup type cr
	] sliteral
    THEN
    type c-flags $. c-flags $free
    ."  -O -c " lib-filename $.
    c++-mode IF  ." .cpp -o "  ELSE  ." .c -o "  THEN
    lib-filename $. ." .lo" ;

: link-cmd ( -- )
    s" CROSS_PREFIX" getenv type
    [ libtool-command tmp$ $! s"  --silent --tag=CC --mode=link " $type
      libtool-cc $type libtool-flags $type s"  -module -rpath " $type tmp$ $@ ] sliteral type
    lib-filename $@ dirname replace-rpath type space
    lib-filename $. ." .lo -o "
    lib-filename $@ dirname type lib-prefix type
    lib-filename $@ basename type ." .la"
    c-libs $.  c-libs $free ;

: init-lib ( handle -- )
    s" gforth_libcc_init" rot lib-sym  ?dup-if
	gforth-pointers swap call-c  endif ;
: compile-wrapper-function1 ( -- )
    hash-c-source open-wrappers dup lib-handle!
    0= if
	c-library-name-create
	libcc$ $@ c-source-file write-file throw  libcc$ $free
	c-source-file close-file throw
	c-source-file-id off
	s" GFORTH_COMPILELIB" getenv s" no" str= 0= IF
	    ['] compile-cmd $tmp system $? 0<> !!libcompile!! and throw
	    ['] link-cmd    $tmp system $? 0<> !!liblink!! and throw
	THEN
	open-wrappers dup 0= if
	    .lib-error
	    host?  IF  !!openlib!! throw  ELSE
                -1 lib-handle! \ fake lha ID
		drop lib-filename $free
		free-libs EXIT
	    THEN
	endif
	( lib-handle ) lib-handle!
    endif
    host? IF  lib-handle init-lib  THEN
    lib-filename $free clear-libs ;
' compile-wrapper-function1 IS compile-wrapper-function

: link-wrapper-function { cff -- sym }
    cff cff-rtype wrapper-function-name
    host? 0= IF  2drop 0  EXIT  THEN
    cff cff-lha @ @ assert( dup ) lib-sym dup 0= if
        .lib-error -&32 throw
    endif ;

: parse-c-name ( -- addr u )
    is-funptr? IF
	'{' parse 2drop '}' parse
    ELSE
	?parse-name
    THEN ;

: ?compile-wrapper ( addr -- addr )
    dup cff-lha @ @ 0= if
	compile-wrapper-function
    endif ;

0 Value rt-vtable

: make-rt ( addr -- )
    rt-vtable >namehm @ swap body> >namehm ! ;

: ?link-wrapper ( addr -- xf-cfr )
    dup body> >does-code ['] call-c@ <> IF
	dup make-rt
	dup link-wrapper-function over !  THEN ;

: ft-does> ?compile-wrapper ?link-wrapper call-c@ ;

: cfun, ( xt -- )
    dup >does-code ['] call-c@ <>
    IF  host? IF
	    ?compile-wrapper ?link-wrapper
	ELSE
	    dup body> make-rt
	    dup cff-lha @ lha-id on \ fake that the library is in use
	THEN
    THEN
    postpone call-c# , ;

hm, cfalign 0 , 0 , noname Create
\ can not be named due to rebind-libcc
named-hm \ but has actually a named hm
' call-c@ set-does>
' cfun, set-optimizer

latestnt to rt-vtable

hm, cfalign 0 , 0 , noname Create
named-hm
' ft-does> set-does>
' cfun, set-optimizer

latestnt Constant ft-vtable

: (c-function) ( xt-parse "forth-name" "c-name" "{stack effect}" -- )
    { xt-parse-types }
    ft-vtable create-from reveal 0 , lib-handle-addr @ ,
    parse-c-name { d: c-name }
    xt-parse-types execute c-name string,
    ['] gen-wrapper-function c-source-file-execute ;

: c-function ( "forth-name" "c-name" "@{type@}" "---" "type" -- ) \ gforth
    \G Define a Forth word @i{forth-name}.  @i{Forth-name} has the
    \G specified stack effect and calls the C function @code{c-name}.
    ['] parse-function-types (c-function) ;

: c-weak-function ( "forth-name" "c-name" "@{type@}" "---" "type" -- ) \ gforth
    \G Same as c-function, but defines a weak function that compiles anyways
    true to is-weak? c-function ;

: c-value ( "forth-name" "c-name" "---" "type" -- ) \ gforth
    \G Define a Forth word @i{forth-name}.  @i{Forth-name} has the
    \G specified stack effect and gives the C value of @code{c-name}.
    ['] parse-value-type (c-function) ;

: c-variable ( "forth-name" "c-name" -- ) \ gforth
    \G Define a Forth word @i{forth-name}.  @i{Forth-name} returns the
    \G address of @code{c-name}.
    ['] parse-variable-type (c-function) ;

: c-funptr ( "forth-name" <@{>"c-typecast"<@}> "@{type@}" "---" "type" -- ) \ gforth
    \G Define a Forth word @i{forth-name}.  @i{Forth-name} has the
    \G specified stack effect plus the called pointer on top of stack,
    \G i.e. @code{( @{type@} ptr -- type )} and calls the C function
    \G pointer @code{ptr} using the typecast or struct access
    \G @code{c-typecast}.
    true to is-funptr? ['] parse-function-types (c-function)
    false to is-funptr? ;

: setup-callback ( addr -- ) dup
    >r ccb% + 2 + count + count + count 2dup
    r@ ccb-lha @ @ lookup-ip-array r@ ccb-ips !
    r@ ccb-lha @ @ lookup-c-array r> ccb-cfuns ! ;

: callback-does> ( xt -- addr )
    \ create a callback instance
    >r
    r@ ccb-num @ 0< !!callbacks!! and throw
    r@ ccb-lha @ @ 0= IF
	compile-wrapper-function
    THEN
    r@ ccb-cfuns @ 0= IF
	r@ setup-callback
    THEN
    >r :noname r> compile, ]] 0 (bye) ; [[
    >body r@ ccb-ips @ r@ ccb-num @ th!
    r@ ccb-cfuns @ r@ ccb-num @ th@
    -1 r> ccb-num +! ;

: (c-callback) ( xt "forth-name" "@{type@}" "---" "type" -- ) \ gforth-internal
    \G Define a callback instantiator with the given signature.  The
    \G callback instantiator @i{forth-name} @code{( xt -- addr )} takes
    \G an @var{xt}, and returns the @var{addr}ess of the C function
    \G handling that callback.
    >r Create here dup ccb% dup allot erase
    lib-handle-addr @ swap dup >r ccb-lha !
    parse-function-types
    here latestnt name>string string, count sanitize
    callback# 1- r> ccb-num !
    r> c-source-file-execute
    ['] callback-does> set-does> ;

: c-callback ( "forth-name" "@{type@}" "---" "type" -- ) \ gforth
    \G Define a callback instantiator with the given signature.  The
    \G callback instantiator @i{forth-name} @code{( xt -- addr )} takes
    \G an @var{xt}, and returns the @var{addr}ess of the C function
    \G handling that callback.
    ['] callback-gen (c-callback) ;

: c-callback-thread ( "forth-name" "@{type@}" "---" "type" -- ) \ gforth
    \G Define a callback instantiator with the given signature.  The
    \G callback instantiator @i{forth-name} @code{( xt -- addr )} takes
    \G an @var{xt}, and returns the @var{addr}ess of the C function
    \G handling that callback.  This callback is safe when called from
    \G another thread
    ['] callback-thread-gen (c-callback) ;

: c-library-incomplete ( -- )
    !!unfinished!! throw ;

: c-library-name ( c-addr u -- ) \ gforth
\G Start a C library interface with name @i{c-addr u}.
    clear-libs
    ['] c-library-incomplete is compile-wrapper-function
    c-named-library-name
    also c-lib ; \ setup of a named c library also extends vocabulary stack

: c++-library-name ( c-addr u -- ) \ gforth
\G Start a C++ library interface with name @i{c-addr u}.
    c-library-name true to c++-mode ;

: libcc>named-path ( -- )
    libcc-path clear-path  libcc-named-dir
    [ lib-suffix s" .so" str= ] [IF]
	[: type ." .libs/" ;] $tmp
    [THEN]
    libcc-path also-path ;

: init-libcc ( -- )
    libcc-named-dir$ $init
    s" libccnameddir" getenv 2dup d0= IF
	2drop libcc-tmp-dir
    THEN
    libcc-named-dir$ $!
    libcc-named-dir $1ff mkdir-parents drop
    clear-libs libcc>named-path
    s" libccdir" getenv 2dup d0= IF
	2drop [ s" libccdir" getenv ':' 0 substc ] SLiteral
    ELSE  ':' 0 substc  THEN  libcc-path also-path
    s" GFORTHCCPATH" getenv 2dup d0<> IF
	':' 0 substc libcc-path also-path
    ELSE  2drop  THEN ;

init-libcc

: rebind-libcc ( -- )
    [: [: ( lib -- )
	    case dup >does-code
		['] call-c@ of
		    >body dup link-wrapper-function
		    \ ." relink: " over body> .name dup h. cr
		    swap !  endof
		['] callback-does> of
		    >body setup-callback
		endof
	    drop endcase
	    true ;] swap traverse-wordlist ;] map-vocs ;
: unbind-libcc ( -- )
    [: [: ( lib -- )
	    case dup >does-code
		['] call-c@        of  off  endof
		['] callback-does> of  #0. rot 2 th 2!  endof
		drop endcase
	    true ;] swap traverse-wordlist ;] map-vocs ;

set-current

Defer prefetch-lib ( addr u -- )
\ load lib if the OS needs it
' 2drop is prefetch-lib

: map-libs { xt -- }
    lib-handle-addr @
    BEGIN  dup @ IF  dup xt execute  THEN
    lha-next @ dup 0= UNTIL  drop ;

: .libs ( -- ) [: lha-name $. space ;] map-libs ;

: reopen-libs ( -- )
    [:  lib-handle-addr !@ >r
	lib-modulename $@
	libcc-named-dir prepend-dirname lib-filename $!
	open-wrappers dup IF
	    \ ." link " r@ lha-name $. ."  to " dup h. cr
	    dup lib-handle!  init-lib
	    r> lib-handle-addr !
	    EXIT
	THEN
	r> lib-handle-addr !
	.lib-error !!openlib!! throw
    ;] map-libs ;

:is 'cold ( -- )
    defers 'cold  get-host? to host?
    init-libcc reopen-libs rebind-libcc lib-filename $free ;

:noname ( -- )
    defers 'image  unbind-libcc  ['] on map-libs
    libcc$ off  libcc-named-dir$ off  libcc-path off  lib-filename off ;
is 'image

: c-library ( "name" -- ) \ gforth
\G Parsing version of @code{c-library-name}
    ?parse-name save-mem c-library-name ;

: c++-library ( "name" -- ) \ gforth
\G Parsing version of @code{c++-library-name}
    ?parse-name save-mem c++-library-name ;

: end-c-library ( -- ) \ gforth
    \G Finish and (if necessary) build the latest C library interface.
    previous
    ['] compile-wrapper-function1 is compile-wrapper-function
    compile-wrapper-function1 end-libs ;

previous
