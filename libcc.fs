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

\ \ something that tells it to include <unistd.h>
\ \ something that tells it that the current library is libc

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

: .n ( n -- )
    0 .r ;

: const+ ( n1 "name" -- n2 )
    dup constant 1+ ;

wordlist constant libcc-types

get-current libcc-types set-current

\ index values
-1
const+ -- \ end of arguments
const+ n \ integer cell
const+ p \ pointer cell
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
    drop swap - over char+ c!
    parse-libcc-type 0< -32 and throw swap c! ;

: type-letter ( n -- c )
    chars s" npdrfv" drop + c@ ;

\ count-stacks

: count-stacks-n ( fp-change1 sp-change1 -- fp-change2 sp-change2 )
    1+ ;

: count-stacks-p ( fp-change1 sp-change1 -- fp-change2 sp-change2 )
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
' count-stacks-p ,
' count-stacks-d ,
' count-stacks-r ,
' count-stacks-func ,
' count-stacks-void ,

: count-stacks ( pars -- fp-change sp-change )
    \ pars is an addr u pair
    0 0 2swap over + swap u+do
	i c@ cells count-stacks-type + @ execute
    loop ;

\ gen-pars

: gen-par-n ( fp-depth1 sp-depth1 -- fp-depth2 sp-depth2 )
    1- dup ." sp[" .n ." ]" ;

: gen-par-p ( fp-depth1 sp-depth1 -- fp-depth2 sp-depth2 )
    ." (void *)(" gen-par-n ." )" ;

: gen-par-d ( fp-depth1 sp-depth1 -- fp-depth2 sp-depth2 )
    ." gforthd2ll(" gen-par-n ." ," gen-par-n ." )" ;

: gen-par-r ( fp-depth1 sp-depth1 -- fp-depth2 sp-depth2 )
    swap 1- tuck ." fp[" .n ." ]" ;

: gen-par-func ( fp-depth1 sp-depth1 -- fp-depth2 sp-depth2 )
    gen-par-p ;

: gen-par-void ( fp-depth1 sp-depth1 -- fp-depth2 sp-depth2 )
    -32 throw ;

create gen-par-types
' gen-par-n ,
' gen-par-p ,
' gen-par-d ,
' gen-par-r ,
' gen-par-func ,
' gen-par-void ,

: gen-par ( fp-depth1 sp-depth1 partype -- fp-depth2 sp-depth2 )
    cells gen-par-types @ execute ;

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

create gen-wrapped-types
' gen-wrapped-n ,
' gen-wrapped-p ,
' gen-wrapped-d ,
' gen-wrapped-r ,
' gen-wrapped-func ,
' gen-wrapped-void ,

: gen-wrapped-stmt ( pars c-name fp-change1 sp-change1 ret -- fp-change sp-change )
    cells gen-wrapped-types @ execute ;

: gen-wrapper-function ( addr -- )
    \ addr points to the return type index of a c-function descriptor
    c@+ { ret } count 2dup { d: pars } chars + count { d: c-name }
    ." void gforth_c_" c-name type ." _"
    pars 0 +do
	i chars over + c@ type-letter emit
    loop
    ." _" ret type-letter emit .\" (void)\n"
    .\" {\n  Cell *sp = gforth_SP;\n  Float *fp = gforth_FP;"
    pars c-name 2over count-stacks ret gen-wrapped-stmt .\" ;\n"
    ?dup-if
	."   gforth_SP = sp+" .n .\" ;\n"
    endif
    ?dup-if
	."   gforth_FP = fp+" .n .\" ;\n"
    endif
    ." }\n" ;

: c-function ( "forth-name" "c-name" "{libcc-type}" "--" "libcc-type" -- )
    create here >r 0 , \ place for the wrapper function pointer
    parse-name { d: c-name }
    parse-function-types c-name string,
    r> cell+ gen-wrapper-function
    compile-wrapper-function
    link-wrapper-function
    r> !
  does> ( ... -- ... )
    @ call-c ;





s" Library not found" exception constant err-nolib

: library ( "name" "file" -- ) \ gforth
\G Dynamically links the library specified by @i{file}.  Defines a
\G word @i{name} ( -- lib ) that starts the declaration of a
\G function from that library.
    create parse-name open-lib dup 0= err-nolib and throw ,
  does> ( -- lib )
    @ ;


