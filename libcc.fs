\ libcc.fs	foreign function interface implemented using a C compiler

\ Copyright (C) 2006,2007,2008,2009,2010,2011,2012,2013 Free Software Foundation, Inc.

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

\ The files are built in .../lib/gforth$ARCH/$VERSION/libcc/ or
\ ~/.gforth$ARCH/libcc/$HOST/.

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
[ifundef] defer!
: defer! ( xt xt-deferred -- ) \ gforth  defer-store
\G Changes the @code{defer}red word @var{xt-deferred} to execute @var{xt}.
    >body [ has? rom [IF] ] @ [ [THEN] ] ! ;
[then]

\ : delete-file 2drop 0 ;

require struct.fs
require mkdir.fs
require string.fs

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
struct
    cell% field cff-cfr \ xt of c-function-rt word
    cell% field cff-deferred \ xt of c-function deferred word
    cell% field cff-lha \ address of the lib-handle for the lib that
                        \ contains the wrapper function of the word
    char% field cff-ctype  \ call type (function=1, value=0)
    char% field cff-rtype  \ return type
    char% field cff-np     \ number of parameters
    1 0   field cff-ptypes \ #npar parameter types
    \  counted string: c-name
end-struct cff%

struct
    cell% field ccb-num
    cell% field ccb-lha
    cell% field ccb-ips
    cell% field ccb-cfuns
end-struct ccb%

variable c-source-file-id \ contains the source file id of the current batch
0 c-source-file-id !
variable lib-handle-addr \ points to the library handle of the current batch.
                         \ the library handle is 0 if the current
                         \ batch is not yet compiled.
Variable lib-filename   \ filename without extension
variable lib-modulename \ basename of the file without extension
variable libcc-named-dir$ \ directory for named libcc wrapper libraries
Variable libcc-path      \ pointer to path of library directories
Variable ptr-declare

defer replace-rpath ( c-addr1 u1 -- c-addr2 u2 )
' noop is replace-rpath

: .nb ( n -- )
    0 .r ;

: const+ ( n1 "name" -- n2 )
    dup constant 1+ ;

: scan-back { c-addr u1 c -- c-addr u2 }
    \ the last occurence of c in c-addr u1 is at u2-1; if it does not
    \ occur, u2=0.
    c-addr 1- c-addr u1 + 1- u-do
	i c@ c = if
	    c-addr i over - 1+ unloop exit endif
    1 -loop
    c-addr 0 ;

Variable c-libs \ library names in a string (without "lib")

: lib-prefix ( -- addr u )  s" libgf" ;

: add-lib ( c-addr u -- ) \ gforth
\G Add library lib@i{string} to the list of libraries, where
    \G @i{string} is represented by @i{c-addr u}.
    [: ."  -l" type ;] c-libs $exec ;

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
    [: ." #include <gforth"
    arch-modifier type ." /"
    version-string type ." /libcc.h>" cr ;] c-source-file-execute ;

\ Types (for parsing)

wordlist constant libcc-types

get-current libcc-types set-current

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
const+ void
drop

set-current

\ call types
0
const+ c-func
const+ c-val
const+ c-var
drop

: libcc-type ( c-addr u -- u2 )
    libcc-types search-wordlist 0= -13 and throw execute ;

: parse-libcc-type ( "libcc-type" -- u )
    parse-name 2dup '{' scan-back
    dup IF  2nip 1- 2dup + source drop - >in !  ELSE  2drop  THEN
    libcc-type ;

: parse-libcc-cast ( "<{>cast<}>" -- addr u )
    source >in @ /string IF  c@ '{' =  IF
	    '{' parse 2drop '}' parse
	ELSE  s" "  THEN
    ELSE  drop  s" "  THEN ;

: libcc-cast, ( "<{>cast<}>" -- )
    parse-libcc-cast string, ;

: parse-return-type ( "libcc-type" -- u )
    parse-libcc-type dup 0< -32 and throw ;

: parse-function-types ( "{libcc-type}" "--" "libcc-type" -- addr )
    c-func c, here
    dup 2 chars allot here begin
	parse-libcc-type dup 0>= while
	    c, libcc-cast, \ cast string
    repeat
    drop here swap - over char+ c!
    parse-return-type swap c! ;

: parse-value-type ( "{--}" "libcc-type" -- addr )
    c-val c, here
    parse-libcc-type  dup 0< if drop parse-return-type then
    c,  0 c, ( terminator ) ;

: parse-variable-type ( -- addr )
    c-var c, here
    s" a" libcc-type c,  0 c, ;

0 Value is-funptr?

: type-letter ( n -- c )
    chars s" nuadUrfv" drop + c@ ;

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
    swap 1+ swap ;

: count-stacks-func ( fp-change1 sp-change1 -- fp-change2 sp-change2 )
    1+ ;

: count-stacks-void ( fp-change1 sp-change1 -- fp-change2 sp-change2 )
;

create count-stacks-types
' count-stacks-n ,
' count-stacks-u ,
' count-stacks-a ,
' count-stacks-d ,
' count-stacks-ud ,
' count-stacks-r ,
' count-stacks-func ,
' count-stacks-void ,

: count-stacks ( pars -- fp-change sp-change )
    \ pars is an addr u pair
    0 0 2swap over + swap u+do
	i c@ cells count-stacks-types + @ execute
    i 1+ c@ 2 + +loop ;

\ gen-pars

: .gen ( n -- n' )  1- dup .nb ;

: gen-par-sp ( fp-depth1 sp-depth1 -- fp-depth2 sp-depth2 )
    ." sp[" .gen ." ]" ;

: gen-par-sp+ ( fp-depth1 sp-depth1 -- fp-depth2 sp-depth2 )
    ." sp+" .gen ;

: gen-par-fp ( fp-depth1 sp-depth1 -- fp-depth2 sp-depth2 )
    swap ." fp[" .gen ." ]" swap ;

: gen-par-n ( fp-depth1 sp-depth1 cast-addr u -- fp-depth2 sp-depth2 )
    type gen-par-sp ;

: gen-par-u ( fp-depth1 sp-depth1 cast-addr u -- fp-depth2 sp-depth2 )
    type gen-par-sp ;

: gen-par-a ( fp-depth1 sp-depth1 cast-addr u -- fp-depth2 sp-depth2 )
    dup 0= IF  2drop ." (void *)"  ELSE type  THEN s" (" gen-par-n ." )" ;

: gen-par-d ( fp-depth1 sp-depth1 cast-addr u -- fp-depth2 sp-depth2 )
    2drop s" gforth_d2ll(" gen-par-n ." ," gen-par-sp ." )" ;

: gen-par-ud ( fp-depth1 sp-depth1 cast-addr u -- fp-depth2 sp-depth2 )
    2drop s" gforth_d2ll(" gen-par-n ." ," gen-par-sp ." )" ;

: gen-par-r ( fp-depth1 sp-depth1 cast-addr u -- fp-depth2 sp-depth2 )
    2drop gen-par-fp ;

: gen-par-func ( fp-depth1 sp-depth1 cast-addr u -- fp-depth2 sp-depth2 )
    gen-par-a ;

: gen-par-void ( fp-depth1 sp-depth1 cast-addr u -- fp-depth2 sp-depth2 )
    2drop ;

create gen-par-types
' gen-par-n ,
' gen-par-u ,
' gen-par-a ,
' gen-par-d ,
' gen-par-ud ,
' gen-par-r ,
' gen-par-func ,
' gen-par-void ,

: gen-par ( fp-depth1 sp-depth1 cast-addr u partype -- fp-depth2 sp-depth2 )
    cells gen-par-types + @ execute ;

\ the call itself

: gen-wrapped-func { d: pars d: c-name fp-change1 sp-change1 -- }
    c-name type ." ("
    fp-change1 sp-change1 pars over + swap u+do 
	i 1+ count i c@ gen-par
	i 1+ c@ 2 + dup i + i' u< if
	    ." ,"
	endif
    +loop
    2drop ." )" ;

: gen-wrapped-const { d: pars d: c-name fp-change1 sp-change1 -- }
	." (" c-name type ." )" ;

: gen-wrapped-var { d: pars d: c-name fp-change1 sp-change1 -- }
	." &(" c-name type ." )" ;

create gen-call-types
' gen-wrapped-func ,
' gen-wrapped-const ,
' gen-wrapped-var ,

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

create gen-wrapped-types
' gen-wrapped-n ,
' gen-wrapped-u ,
' gen-wrapped-a ,
' gen-wrapped-d ,
' gen-wrapped-ud ,
' gen-wrapped-r ,
' gen-wrapped-func ,
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
    pars + count type '_' emit
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
    s" *sp++" 2swap \ default is fetch ptr from stack
    ptr-declare [: ( decl u1 c-name u2 ptr-name u3 -- decl' u1' c-name u2 )
	2>r 2dup 2r> ':' $split 2>r string-prefix?
	IF  2nip 2r> 2swap  ELSE  2rdrop THEN ;] $[]map 2drop ;

: gen-wrapper-function ( addr -- )
    \ addr points to the return type index of a c-function descriptor
    dup { descriptor }
    count { ret } count 2dup { d: pars } chars + count { d: c-name }
    ." void " .prefix
    descriptor wrapper-function-name type
    .\" (GFORTH_ARGS)\n"
    .\" {\n  Cell MAYBE_UNUSED *sp = gforth_SP;\n  Float MAYBE_UNUSED *fp = gforth_FP;\n  "
    pars c-name 2over count-stacks
    .\" int MAYBE_UNUSED arg0=" dup 1- .nb .\" , farg0=" over 1- .nb .\" ;\n  "
    is-funptr? IF  ." Cell ptr = " c-name >ptr-declare type .\" ;\n  "  THEN
    ret gen-wrapped-stmt .\" ;\n"
    dup is-funptr? or if
	."   gforth_SP = sp+" dup .nb .\" ;\n"
    endif drop
    ?dup-if
	."   gforth_FP = fp+"     .nb .\" ;\n"
    endif
    .\" }\n" ;

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

: print-type ( n -- ) cells gen-types + perform ;

: callback-header ( descriptor -- )
    count { ret } count 2dup { d: pars } chars + count { d: c-name }
    ." #define CALLBACK_" c-name type ." (I) \" cr
    ret print-type space .prefix ." gforth_cb_" c-name type ." _##I ("
    0 pars bounds u+do
	i 1+ count dup IF  type  ELSE  2drop i c@ print-type  THEN
	."  x" dup 0 .r 1+
	i 1+ c@ 2 + dup i + i' u< if
	    ." , "
	endif
    +loop  drop .\" ) \\\n{ \\" cr ;

Create callback-style c-val c,

: callback-pushs ( descriptor -- )
    1+ count 0 { d: pars vari }
    ."   Cell*  sp=gforth_SP; \" cr
    ."   Float* fp=gforth_FP; \" cr
    0 0 pars bounds u+do
	callback-style 3 + 1 2swap
	vari 0 <# #s 'x' hold #> 2swap
	i c@ 2 spaces gen-wrapped-stmt ." ; \" cr
	i 1+ c@ 2 +  vari 1+ to vari
    +loop
    ?dup-if  ."   sp+=" .nb ." ; \" cr  then
    ?dup-if  ."   fp+=" .nb ." ; \" cr  then ;

: callback-call ( descriptor -- )
    1+ count + count \ callback C name
    ."   gforth_SP=sp; gforth_FP=fp; gforth_engine(" .prefix ." gforth_cbips_" type
    ." [I]); \" cr ;

: gen-par-callback ( sp-change1 sp-change1 addr u type -- fp-change sp-change )
    dup [ libcc-types >order ] void [ previous ] =
    IF  drop 2drop  ELSE  gen-par  THEN ;

: callback-wrapup ( -- )
    ."   gforth_SP=oldsp; gforth_RP=oldrp; gforth_LP=oldlp; gforth_FP=oldfp; gforth_UP=oldup; \" cr ;

: callback-adjust ( descriptor -- )
    ."   sp=gforth_SP; fp=gforth_FP; \" cr
    1 count-stacks
    ?dup-if  ."   sp=gforth_SP+=" .nb ." ; \" cr  then
    ?dup-if  ."   fp=gforth_FP+=" .nb ." ; \" cr  then ;

: callback-return ( descriptor -- )
    >r 0 0 s"   return " r> c@ gen-par-callback 2drop .\" ; \\\n}" cr ;

: callback-define ( descriptor -- )
    dup callback-header dup callback-pushs dup callback-call
    dup callback-adjust callback-return ;

: callback-wrapper ( -- )
    ."   Cell *oldsp=gforth_SP; Cell *oldrp=gforth_RP; char *oldlp=gforth_LP; Float *oldfp=gforth_FP; user_area *oldup=gforth_UP; \" cr
    ."   Cell stack[GFSS], rstack[GFSS], lstack[GFSS]; Float fstack[GFSS]; \" cr
    ."   gforth_SP=stack+GFSS-1; gforth_RP=rstack+GFSS; gforth_LP=(char*)(lstack+GFSS); gforth_FP=fstack+GFSS-1; gforth_UP=gforth_main_UP; \" cr ;

: callback-thread-define ( descriptor -- )
    dup callback-header callback-wrapper
    dup callback-pushs dup callback-call
    dup callback-adjust callback-wrapup callback-return ;

8 Value callback# \ how many callbacks should be created?

: callback-instantiate ( addr u -- )
    callback# 0 ?DO
	." CALLBACK_" 2dup type ." (" I .nb ." )" cr
    LOOP 2drop ;

: callback-ip-array ( addr u -- )
    ." Address " .prefix ." gforth_cbips_" 2dup type ." [" callback# .nb ." ] = {" cr
    space callback# 0 ?DO ."  0," LOOP ." };" cr 2drop ;

: callback-c-array ( addr u -- )
    ." const Address " .prefix ." gforth_callbacks_" 2dup type ." [" callback# .nb ." ] = {" cr
    callback# 0 ?DO
	."   (Address)" .prefix ." gforth_cb_" 2dup type ." _" I .nb ." ," cr
    LOOP
    ." };" cr 2drop ;

: callback-gen ( descriptor -- )
    dup callback-define  1+ count + count \ c-name u
    2dup callback-ip-array 2dup callback-instantiate callback-c-array ;

: callback-thread-gen ( descriptor -- )
    dup callback-thread-define  1+ count + count \ c-name u
    2dup callback-ip-array 2dup callback-instantiate callback-c-array ;

: lookup-ip-array ( addr u lib -- addr ) >r
    [: ." gforth_cbips_" type ;] $tmp r> lib-sym ;

: lookup-c-array ( addr u lib -- addr ) >r
    [: ." gforth_callbacks_" type ;] $tmp r> lib-sym ;

\ file stuff

: dirname ( c-addr1 u1 -- c-addr2 u2 )
    \ directory name of the file name c-addr1 u1, including the final "/".
    '/ scan-back ;

: basename ( c-addr1 u1 -- c-addr2 u2 )
    \ file name without directory component
    2dup dirname nip /string ;

: libcc-named-dir ( -- c-addr u )
    libcc-named-dir$ $@ ;

: >libcc-named-dir ( addr u -- )
    libcc-named-dir$ $! ;

: libcc-tmp-dir ( -- c-addr u )
    [: ." ~/.gforth" arch-modifier type ." /" machine type
    ." /libcc-tmp/" ;] $tmp ;

: prepend-dirname ( c-addr1 u1 c-addr2 u2 -- c-addr3 u3 )
    [: type type ;] $tmp ;

: open-wrappers ( -- addr|0 )
    [: lib-filename $@ dirname type lib-prefix type
       lib-filename $@ basename type lib-suffix type ;] $tmp
    2dup libcc-named-dir string-prefix? if ( c-addr u )
	\ see if we can open it in the path
	libcc-named-dir nip /string
	libcc-path open-path-file if
\	    ." Failed to find library '" lib-filename $. ." ' in '"
\	    libcc-path .path ." ', need compiling" cr
	    0 exit endif
	( wfile-id c-addr2 u2 ) rot close-file throw save-mem ( c-addr2 u2 )
    endif
    \ 2dup cr type
    open-lib ;

: c-library-name-setup ( c-addr u -- )
    assert( c-source-file-id @ 0= )
    { d: filename }
    filename lib-filename $!
    filename basename lib-modulename $! lib-modulename $@ sanitize ;
   
: c-library-name-create ( -- )
    [: lib-filename $. ." .c" ;] $tmp r/w create-file throw
    c-source-file-id ! ;

: c-named-library-name ( c-addr u -- )
    \ set up filenames for a (possibly new) library; c-addr u is the
    \ basename of the library
    libcc-named-dir 2dup  $1ff mkdir-parents drop
    prepend-dirname c-library-name-setup
    open-wrappers lib-handle-addr @ ! ;

: c-tmp-library-name ( c-addr u -- )
    \ set up filenames for a new library; c-addr u is the basename of
    \ the library
    libcc-tmp-dir 2dup $1ff mkdir-parents drop
    prepend-dirname c-library-name-setup
    open-wrappers lib-handle-addr @ ! ;

: lib-handle ( -- addr )
    lib-handle-addr @ @ ;

: c-source-file ( -- file-id )
    c-source-file-id @ assert( dup ) ;

: .lib-error ( -- )
    [ifdef] lib-error
        ['] cr stderr outfile-execute
        lib-error ['] type stderr outfile-execute
    [then] ;

\ hashing

[IFUNDEF] hashkey2
    : hash-c-source ( -- ) ;
    : check-c-hash ( -- ) ;
[ELSE]
    : replace-modulename ( addr u -- ) { d: replace }
	libcc$ $@  BEGIN  s" _replace_this_with_the_hash_code" search  WHILE
		over replace rot swap move $20 /string  REPEAT
	2drop ;
    
    Create c-source-hash 16 allot

    : .xx ( n -- ) 0 [: <<# # # #> type #>> ;] $10 base-execute ;
    : .bytes ( addr u -- ) bounds ?DO
	    ." \x" I c@ .xx
	LOOP ;
    : .c-hash ( -- )
	lib-filename @ 0= IF
	    [: c-source-hash 16 bounds DO  I c@ .xx  LOOP ;] $tmp
	    save-mem c-tmp-library-name
	    lib-modulename $@ replace-modulename
	THEN
	." hash_128 gflibcc_hash_" lib-modulename $.
	.\"  = \"" c-source-hash 16 .bytes .\" \";" cr ;
    
    : hash-c-source ( -- )
	c-source-hash 16 erase
	libcc$ $@ false c-source-hash hashkey2
	['] .c-hash c-source-file-execute ;

    : check-c-hash ( -- )
	[: ." gflibcc_hash_" lib-modulename $. ;] $tmp
	lib-handle lib-sym
	?dup-IF  c-source-hash 16 tuck compare  ELSE  true  THEN
	IF  lib-handle close-lib  lib-handle-addr @ off  THEN ;
[THEN]

\ clear library

DEFER compile-wrapper-function ( -- )

: clear-libs ( -- ) \ gforth
\G Clear the list of libs
    c-source-file-id @ if
	compile-wrapper-function
    endif
    align here 0 , lib-handle-addr !
    c-libs $init
    lib-modulename $init
    libcc$ $init libcc-include
    ptr-declare $[]off ;
clear-libs

\ compilation wrapper

: compile-cmd ( -- )
    [ libtool-command tmp$ $! s"  --silent --tag=CC --mode=compile " $type
      s" CROSS_PREFIX" getenv $type
      libtool-cc $type s"  -I '" $type
      s" includedir" getenv tuck $type
      0= [IF]  pad $100 get-dir $type s" /include" $type  [THEN]
      s" '" $type tmp$ $@ ] sliteral type
    ."  -O -c " lib-filename $. ." .c -o "
    lib-filename $. ." .lo" ;

: link-cmd ( -- )
    s" CROSS_PREFIX" getenv type
    [ libtool-command tmp$ $! s"  --silent --tag=CC --mode=link " $type
      libtool-cc $type libtool-flags $type s"  -module -rpath " $type tmp$ $@ ] sliteral type
    lib-filename $@ dirname replace-rpath type space
    lib-filename $. ." .lo -o "
    lib-filename $@ dirname type lib-prefix type
    lib-filename $@ basename type ." .la"
    c-libs $.  c-libs $off ;

: compile-wrapper-function1 ( -- )
    hash-c-source check-c-hash
    lib-handle 0= if
	c-library-name-create
	libcc$ $@ c-source-file write-file throw  libcc$ $off
	c-source-file close-file throw
	['] compile-cmd $tmp system $? 0<> !!libcompile!! and throw
	['] link-cmd    $tmp system $? 0<> !!liblink!! and throw
	open-wrappers dup 0= if
	    .lib-error !!openlib!! throw
	endif
	( lib-handle ) lib-handle-addr @ !
    endif
    s" gforth_libcc_init" lib-handle lib-sym  ?dup-if  call-c  endif
    0 c-source-file-id !
    lib-filename $off clear-libs ;
' compile-wrapper-function1 IS compile-wrapper-function

: link-wrapper-function { cff -- sym }
    cff cff-rtype wrapper-function-name
    cff cff-lha @ @ assert( dup ) lib-sym dup 0= if
        .lib-error -&32 throw
    endif ;

: parse-c-name ( -- addr u )
    is-funptr? IF
	'{' parse 2drop '}' parse
    ELSE
	parse-name
    THEN ;

: ?compile-wrapper ( addr -- addr )
    dup cff-lha @ @ 0= if
	compile-wrapper-function
    endif ;

: ?link-wrapper ( addr -- xf-cfr )
    dup 2@ { xt-defer xt-cfr }
    xt-defer defer@ xt-cfr <> IF
	link-wrapper-function
	xt-cfr >body !
	xt-cfr xt-defer defer!  THEN
    xt-cfr ;

: c-function-ft ( xt-defr xt-cfr xt-parse "c-name" "type signature" -- )
    \ build time/first time action for c-function
    { xt-parse-types }
    noname create 2, lib-handle-addr @ ,
    parse-c-name { d: c-name }
    xt-parse-types execute c-name string,
    ['] gen-wrapper-function c-source-file-execute
  does> ( ... -- ... )
    ?compile-wrapper ?link-wrapper execute ;

: c-function-rt ( -- )
    \ run-time definition for c function; addr is the address where
    \ the sym should be stored
    noname create 0 ,
  does> ( ... -- ... )
    @ call-c ;

c-function-rt  lastxt Constant dummy-rt

: (c-function) ( xt-parse "forth-name" "c-name" "{stack effect}" -- )
    { xt-parse-types } defer
    [: defer@ dup >does-code dummy-rt >does-code <>
    IF  >body ?compile-wrapper ?link-wrapper  THEN
    postpone call-c# >body , ;] set-compiler
    lastxt dup c-function-rt
    lastxt xt-parse-types c-function-ft
    lastxt swap defer! ;

: c-function ( "forth-name" "c-name" "@{type@}" "---" "type" -- ) \ gforth
    \G Define a Forth word @i{forth-name}.  @i{Forth-name} has the
    \G specified stack effect and calls the C function @code{c-name}.
    ['] parse-function-types (c-function) ;

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

: (c-callback) ( xt "forth-name" "@{type@}" "---" "type" -- ) \ gforth
    \G Define a callback instantiator with the given signature.  The
    \G callback instantiator @i{forth-name} @code{( xt -- addr )} takes
    \G an @var{xt}, and returns the @var{addr}ess of the C function
    \G handling that callback.
    >r Create here dup ccb% %size dup allot erase
    lib-handle-addr @ swap ccb-lha !
    parse-function-types
    here lastxt name>string string, count sanitize
    r> c-source-file-execute
  DOES> ( xt -- addr ) \ create a callback instance
    >r r@ ccb-lha @ @ 0= IF
	compile-wrapper-function
    THEN
    r@ ccb-cfuns @ 0= IF
	r@ cff% %size + 2 + count + count 2dup
	r@ ccb-lha @ @ lookup-ip-array r@ ccb-ips !
	r@ ccb-lha @ @ lookup-c-array r@ ccb-cfuns !
    THEN
    r@ @ callback# u>= !!callbacks!! and throw
    >r :noname r> compile, ]] 0 (bye) ; [[
    >body r@ ccb-ips @ r@ ccb-num @ cells + !
    r@ ccb-cfuns @ r@ ccb-num @ cells + @
    1 r> ccb-num +! ;

: c-callback ( "forth-name" "@{type@}" "---" "type" -- ) \ gforth
    \G Define a callback instantiator with the given signature.  The
    \G callback instantiator @i{forth-name} @code{( xt -- addr )} takes
    \G an @var{xt}, and returns the @var{addr}ess of the C function
    \G handling that callback.
    [: callback-gen ;] (c-callback) ;

: c-callback-thread ( "forth-name" "@{type@}" "---" "type" -- ) \ gforth
    \G Define a callback instantiator with the given signature.  The
    \G callback instantiator @i{forth-name} @code{( xt -- addr )} takes
    \G an @var{xt}, and returns the @var{addr}ess of the C function
    \G handling that callback.  This callback is save when called from
    \G another thread
    [: callback-thread-gen ;] (c-callback) ;

: c-library-incomplete ( -- )
    !!unfinished!! throw ;

: c-library-name ( c-addr u -- ) \ gforth
\G Start a C library interface with name @i{c-addr u}.
    clear-libs
    ['] c-library-incomplete is compile-wrapper-function
    c-named-library-name ;

: init-libcc ( -- )
    libcc-named-dir$ $init
    [: ." ~/.gforth" arch-modifier type ." /" machine type ." /libcc-named/"
    ;] libcc-named-dir$ $exec
    libcc-path $init  ptr-declare $init
    clear-libs
    libcc-named-dir libcc-path also-path
    [ s" libccdir" getenv ] sliteral libcc-path also-path ;

init-libcc

:noname ( -- )
    defers 'cold
    init-libcc ;
is 'cold

set-current

: c-library ( "name" -- ) \ gforth
\G Parsing version of @code{c-library-name}
    parse-name save-mem c-library-name also c-lib ;

: end-c-library ( -- ) \ gforth
    \G Finish and (if necessary) build the latest C library interface.
    previous
    ['] compile-wrapper-function1 is compile-wrapper-function
    compile-wrapper-function1 ;

previous