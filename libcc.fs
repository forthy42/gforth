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


\ data structures

\ c-function word body:
\  cell function pointer
\  char return type index
\  char parameter count n
\  char*n parameters (type indices)
\  counted string: c-name

: .nb ( n -- )
    0 .r ;

: const+ ( n1 "name" -- n2 )
    dup constant 1+ ;

\ linked list stuff (should go elsewhere)

hex

require struct.fs

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

: save-c-prefix-line ( c-addr u -- )
    align here 0 , c-prefix-lines-end list-append ( c-addr u )
    longstring, ;

: \c ( "rest-of-line" -- )
    -1 parse save-c-prefix-line ;

: print-c-prefix-line ( node -- )
    dup c-prefix-chars swap c-prefix-count @ type cr ;

: print-c-prefix-lines ( -- )
    c-prefix-lines @ ['] print-c-prefix-line list-map ;

\c #include "engine/libcc.h"

print-c-prefix-lines

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

: gen-wrapper-function ( addr -- )
    \ addr points to the return type index of a c-function descriptor
    c@+ { ret } count 2dup { d: pars } chars + count { d: c-name }
    print-c-prefix-lines
    ." void gforth_c_" c-name type ." _"
    pars bounds u+do
	i c@ type-letter emit
    loop
    ." _" ret type-letter emit .\" (void)\n"
    .\" {\n  Cell MAYBE_UNUSED *sp = gforth_SP;\n  Float MAYBE_UNUSED *fp = gforth_FP;\n  "
    pars c-name 2over count-stacks ret gen-wrapped-stmt .\" ;\n"
    ?dup-if
	."   gforth_SP = sp+" .nb .\" ;\n"
    endif
    ?dup-if
	."   gforth_FP = fp+" .nb .\" ;\n"
    endif
    .\" }\n" ;

: compile-wrapper-function ( -- )
    s" gcc -fPIC -shared -Wl,-soname,xxx.so.1 -Wl,-export_dynamic -o xxx.so.1 -O xxx.c" system
    $? abort" compiler generated error" ;
\    s" ar rcs xxx.a xxx.o" system
\    $? abort" ar generated error" ;

: link-wrapper-function ( -- sym )
    s" /home/anton/gforth/xxx.so.1" open-lib ( lib-handle )
    s" gforth_c_strlen_a_n" rot lib-sym dup 0= -&32 and throw ;


: c-function-ft ( xt-defer xt-cfr "c-name" "{libcc-type}" "--" "libcc-type" -- )
    \ build time/first time action for c-function
    noname create 2,
    parse-name { d: c-name }
    here parse-function-types c-name string,
    s" xxx.c" w/o create-file throw >r ( R:file-id )
    ['] gen-wrapper-function r@ outfile-execute
    r> close-file throw
  does> ( ... -- ... )
    2@ { xt-defer xt-cfr }
    compile-wrapper-function
    link-wrapper-function xt-cfr >body !
    xt-cfr xt-defer defer!
    xt-cfr execute ;

: c-function-rt ( -- )
    \ run-time definition for c function; addr is the address where
    \ the sym should be stored
    noname create 0 ,
  does> ( ... -- ... )
    @ call-c ;

: c-function ( "forth-name" "c-name" "{libcc-type}" "--" "libcc-type" -- )
    defer lastxt dup c-function-rt lastxt c-function-ft
    lastxt swap defer! ;

s" Library not found" exception constant err-nolib

: library ( "name" "file" -- ) \ gforth
\G Dynamically links the library specified by @i{file}.  Defines a
\G word @i{name} ( -- lib ) that starts the declaration of a
\G function from that library.
    create parse-name open-lib dup 0= err-nolib and throw ,
  does> ( -- lib )
    @ ;

\ test

\ test all parameter and return types

\ cr .( #include "engine/libcc.h")
\ cr .( #include <unistd.h>)
\ cr ." typedef void (* func)(int);
\ cr ." int test1(int,char*,long,double,void (*)(int));"
\ cr ." Cell *test2(void);"
\ cr ." int test3(void);"
\ cr ." float test4(void);"
\ cr ." func test5(void);"
\ cr ." void test6(void);"
\ cr

\ c-function dlseek lseek n d n -- d
\ c-function n test1 n a d r func -- n
\ c-function a test2 -- a
\ c-function d test3 -- d
\ c-function r test4 -- r
\ c-function func test5 -- func
\ c-function void test6 -- void

\c #include <string.h>

c-function strlen strlen a -- n

cr s\" fooo\0" 2dup dump drop .s strlen cr .s cr
