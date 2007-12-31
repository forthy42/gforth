\ test for Gforth primitives

\ Copyright (C) 2003,2007 Free Software Foundation, Inc.

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

Create mach-file here over 1+ allot place

0 [IF]
\ debugging: produce a relocation and a symbol table
s" rel-table" r/w create-file throw
Constant fd-relocation-table

\ debuggging: produce a symbol table
s" sym-table" r/w create-file throw
Constant fd-symbol-table
[THEN]


bl word vocabulary find nip 0= [IF]
    \ if search order stuff is missing assume we are compiling on a gforth
    \ system and include it.
    \ We want the files taken from our current gforth installation
    \ so we don't include relatively to this file
    require startup.fs
[THEN]

\ include etags.fs

include ./../cross.fs              \ cross-compiler

decimal

has? kernel-start has? kernel-size makekernel
\ create image-header
has? header [IF]
here 1802 over 
    A,                  \ base address
    0 ,                 \ checksum
    0 ,                 \ image size (without tags)
has? kernel-size
    ,                   \ dict size
    has? stack-size ,   \ data stack size
    has? fstack-size ,  \ FP stack size
    has? rstack-size ,  \ return stack size
    has? lstack-size ,  \ locals stack size
    0 A,                \ code entry point
    0 A,                \ throw entry point
    has? stack-size ,   \ unused (possibly tib stack size)
    0 ,                 \ unused
    0 ,                 \ data stack base
    0 ,                 \ fp stack base
    0 ,                 \ return stack base
    0 ,                 \ locals stack base
[THEN]

doc-off
has? prims [IF]
    include ./../kernel/aliases.fs             \ primitive aliases
[ELSE]
    prims-include
    undef-words
    include prim.fs
    all-words  
[THEN]
doc-on

has? header [IF]
1802 <> [IF] .s cr .( header start address expected!) cr uffz [THEN]
AConstant image-header
: forthstart image-header @ ;
[THEN]

\ 0 AConstant forthstart

: emit ( c -- )
    stdout emit-file drop ;

: type ( addr u -- )
    stdout write-file drop ;

: cr ( -- )
    newline type ;

char j constant char-j

variable var-k
char k var-k !
defer my-emit
' emit is my-emit
cell% 2* 0 0 field >body

create cbuf 100 allot

create cellbuf 5 , 6 , 7 , 8 , 20 cells allot


4 constant w/o
0 constant r/o

variable s0
: depth s0 @ sp@ cell+ - ;

\  : myconst ( n -- )
\      create ,
\    does> ( -- n )
\      @ ;
\  char m myconst char-m
create myconst char m ,
does> @ ;

: unloop-test ( -- )
    0 >r 0 >r unloop ;

: deeper-rp@
    rp@ ;

: rp!-test2
    rp! ;

: rp!-test1
    rp@ rp!-test2 ." should not be executed" ;

: rdrop-test
    0 >r rdrop ;

: boot ( -- )
    sp@ s0 !
    [char] a stdout emit-file drop
    [char] b emit
    s" cd" type
    ." fg"
    [char] i ['] emit execute
    ['] char-j execute emit
    ['] var-k execute @ emit
    \ !!douser
    [char] l ['] my-emit execute
    [char] l ['] my-emit ['] >body execute perform
    ['] myconst execute emit
    noop
    [char] m ['] my-emit ['] execute dup execute
    [char] m ['] 1+ execute emit
    [char] o ['] my-emit >body perform
    unloop-test ." p"
    [char] q my-emit
    myconst emit
    \ !! branch-lp+!#
    ahead ." wrong" then ." r"
    0 if ." wrong" else ." s" then
    1 if ." t" else ." wrong" then
    \ !! ?dup-?branch ?dup-0=-?branch
    \ 0 ?dup-if ." wrong" drop else ." u" then
    \ [char] v ?dup-if emit else ." wrong" then
    1 for [char] x i - emit next
    [char] z 1+ [char] y do i emit loop
    [char] D [char] A do i emit 2 +loop
    [char] A [char] E do i emit -2 +loop
\    [char] A [char] D do i emit 2 -loop \ !! -loop undefined
\    [char] A [char] E do i emit -2 s+loop \ !! s+loop undefined
    [char] X [char] X ?do i emit loop
    [char] G [char] F ?do i emit loop
    \    [char] X [char] Y +do i emit loop \ !! +do undefined
    \    [char] H [char] G +do i emit loop
    \ !! (u+do) (-do) (u-do)
    [char] I >r 0 >r i' emit 2rdrop
    [char] J >r 1 0 ?do j emit loop rdrop
    [char] K >r 0 >r 0 >r 1 0 ?do k emit loop 2rdrop rdrop
    s" LMN" cbuf swap move cbuf 3 type
    cbuf cbuf 2 + 5 cmove cbuf 6 type
    cbuf 1+ cbuf 6 cmove> cbuf 2 type
    cbuf 10 [char] N fill cbuf 2 type
    cbuf 10 s" NNNN" compare [char] N + emit
    cbuf 4 s" NNNN" compare [char] P + emit
    cbuf 3 s" NNNN" compare [char] R + emit
    [char] r toupper emit
    s" abcST" 3 /string type
    [char] S 2 + emit
    [char] V ['] my-emit >body perform
    [char] V [char] W 2 under+ emit emit
    'Z 1 - emit
    'X 2 negate - emit
    '` 1+ emit
    'c 1- emit
    'a 'd max emit
    'g 'e min emit
    'e -1 abs + emit
    'a 2 3 * + emit
    'a 700 99 / + emit
    'g 8 3 mod + emit
    8 3 /mod + 'f + emit
    'a 5 2* + emit
    'n -3 2/ + emit
    7. -3 fm/mod drop 'o + emit
    7. -3 sm/rem drop 'm + emit
    -1 1 m* + 'q + emit
    -1 -1 um* + 'q + emit
    7. 3 um/mod + 'n + emit
    0 2 -1 m+ -1 1 d= 's + emit
    -1 1 1 1 d+ 0 3 d= 't + emit
    1 3 2 1 d- -1 1 d= 'u + emit
    1 0 dnegate -1 -1 d= 'v + emit
    cr
    -1 0 d2* -2 1 d= 'b + emit
    -4 3 d2/ -2 1 d= 'c + emit
    5 3 and 1 = 'd + emit
    5 3 or 7 = 'e + emit
    5 3 xor 6 = 'f + emit
    5 invert -6 = 'g + emit
    $f0f0f0f0 12 rshift $f0f0f = 'h + emit
    5 2 lshift 20 = 'i + emit
    0 0= 1 0= -1 0 d= 'j + emit
    -1 0< 0 0< -1 0 d= 'k + emit
    1 0> 0 0> -1 0 d= 'l + emit
    0 0<= 1 0<= -1 0 d= 'm + emit
    0 0<= 1 0<= -1 0 d= 'm + emit \ just to repeat the "l"
    0 0>= -1 0>= -1 0 d= 'n + emit
    5 0<> 0 0<> -1 0 d= 'o + emit
    1 1 = 2 3 = -1 0 d= 'p + emit
    -1 0 < 1 1 < -1 0 d= 'q + emit
    2 -1 > 1 1 > -1 0 d= 'r + emit
    1 1 <= 2 -1 <= -1 0 d= 's + emit
    1 1 >= -1 2 >= -1 0 d= 't + emit
    2 3 <> 1 1 <> -1 0 d= 'u + emit
    1 1 u= 2 3 u= -1 0 d= 'v + emit
    0 -2 u< 0 0 u< -1 0 d= 'w + emit
    -3 5 u> 0 0 u> -1 0 d= 'x + emit
    0 0 u<= -1 0 u<= -1 0 d= 'y + emit
    0 0 u>= 0 -1 u>= -1 0 d= 'z + emit
    2 3 u<> 0 0 u<> -1 0 d= '{ + emit
    \ dcomparisons
    0. d0= 1. d0= -1 0 d= 'j + emit
    -1. d0< 0. d0< -1 0 d= 'k + emit
    1. d0> 0. d0> -1 0 d= 'l + emit
    0. d0<= 1. d0<= -1 0 d= 'm + emit
    0. d0<= 1. d0<= -1 0 d= 'm + emit \ just to repeat the "l"
    0. d0>= -1. d0>= -1 0 d= 'n + emit
    5. d0<> 0. d0<> -1 0 d= 'o + emit
    1. 1. d= 2. 3. d= -1 0 d= 'p + emit
    -1. 0. d< 1. 1. d< -1 0 d= 'q + emit
    2. -1. d> 1. 1. d> -1 0 d= 'r + emit
    1. 1. d<= 2. -1. d<= -1 0 d= 's + emit
    1. 1. d>= -1. 2. d>= -1 0 d= 't + emit
    2. 3. d<> 1. 1. d<> -1 0 d= 'u + emit
    1. 1. du= 2. 3. du= -1 0 d= 'v + emit
    0. -2. du< 0. 0. du< -1 0 d= 'w + emit
    -3. 5. du> 0. 0. du> -1 0 d= 'x + emit
    0. 0. du<= -1. 0. du<= -1 0 d= 'y + emit
    0. 0. du>= 0. -1. du>= -1 0 d= 'z + emit
    2. 3. du<> 0. 0. du<> -1 0 d= '{ + emit
    0 0 1 within 0 0 0 within -1 0 d= 'B + emit
    \ !! useraddr
    \ !! up!
    sp@ s0 @ = 'C + emit
    sp@ -3 cells + sp! drop drop drop sp@ s0 @ = 'D + emit
    rp@ deeper-rp@ cell+ = 'E + emit
    rp!-test1 'E emit
    \ fp@ 1e fp@ float+ = 'G + emit \ !! fp@
    0 1 >r 0 = r> 1 = -1 -1 d= 'G + emit
    rdrop-test 'G emit
    0 1 2>r 'I 2r> 0 1 d= + emit
    3 4 2>r 2r@ 2r> d= 'J + emit
    5 6 2>r 7 8 2>r 2rdrop 2r> 5 6 d= 'K + emit
    1 2 over 2 1 d= 1 -1 d= 'L + emit
    1 2 3 drop 1 2 d= 'M + emit
    1 2 swap 2 1 d= 'N + emit
    1 dup 1 1 d= 'O + emit
    1 2 3 rot 3 1 d= 2 -1 d= 'P + emit
    1 2 3 -rot 1 2 d= 3 -1 d= 'Q + emit
    1 2 3 nip 1 3 d= 'R + emit
    1 2 tuck 1 2 d= 2 -1 d= 'S + emit
    4 0 ?dup 4 0 d= 'T + emit
    5 1 ?dup 1 1 d= 5 -1 d= 'U + emit
    6 0 pick 6 6 d= 'V + emit
    1 2 3 4 2drop 1 2 d= 'W + emit
    7 1 2 2dup d= 7 -1 d= 'X + emit
    8 1 2 3 4 2over 1 2 d= >r 3 4 d= >r 1 2 d= r> and r> and 8 -1 d= 'Y + emit
    1 2 3 4 2swap 1 2 d= >r 3 4 d= r> -1 -1 d= 'Z + emit
    9 1 2 3 4 5 6 2rot 1 2 d= >r 5 6 d= >r 3 4 d= r> and r> and 9 -1 d= '[ + emit
    7 1 2 3 4 2nip 3 4 d= 7 -1 d= 'b + emit
    8 1 2 3 4 2tuck 3 4 d= >r 1 2 d= >r 3 4 d= r> and r> and 8 -1 d= 'c + emit
    cr
    cellbuf @ 5 = 'b + emit
    9 cellbuf ! 5 cellbuf @ 5 9 d= 'c + emit
    -1 cellbuf +! cellbuf @ 8 = 'd + emit
    -1 cellbuf ! cellbuf c@ $ff = 'e + emit
    1 cellbuf c! cellbuf @ 1 <> 'f + emit
    3 4 cellbuf 2! cellbuf @ 4 = 'g + emit
    2 cellbuf ! cellbuf 2@ 3 2 d= 'h + emit
    9 cellbuf cell+ ! cellbuf 2@ 9 2 d= 'i + emit
    cellbuf 3 cells + @ 8 = 'j + emit
    s" ijk" drop char+ c@ emit
    s" ijk" drop 2 (chars) + c@ emit
    c" ijkl" count 3 /string type
    \ s" abc" 0 (f83find) 0= 'm + emit \ not in gforth-0.6.2
    s" abc" 0 (listlfind) 0= 'n + emit
    s" abc" 0 (hashlfind) 0= 'o + emit
    s" abc" 0 (tablelfind) 0= 'p + emit
    s" dfskdfjsdl" 5 (hashkey1) 32 u< 'n + emit
    s"    bcde   " (parse-white) s" bcde" compare 'n + emit
    1 aligned 0 cell+ = 'p + emit
    1 faligned 0 float+ = 'q + emit
    threading-method 2 u< 'r + emit
    \ stdin key-file emit
    stdin key-file emit
    stdin key?-file 't + emit
    stderr drop 't emit
    form 2drop 'u emit
    cbuf 20 flush-icache
    \ (bye)
    s" true" (system) 0 0 d= 'w + emit
    s" ENVVAR" getenv s" bla" compare 'w + emit
    s" grep -q bla" w/o open-pipe 0= 'y + emit >r
    s" blabla" i write-file 0= 'z + emit r> close-pipe d0= 'B + emit
    777 time&date 2drop 2drop 2drop 777 = 'C + emit
    1 ms 'C emit
    100 allocate 0= 'E + emit ( addr)
    200 resize 0= 'F + emit  ( addr2)
    free 0= 'G + emit
    1 strerror 2drop 'G emit
    1 strsignal 2drop 'H emit
    \ call-c
    s" prim" r/o open-file 0= 'J + emit >r
    cbuf 100 i (read-line) 0= 'K + emit drop 'L + emit drop
    i file-position 0= 'M + emit cellbuf 2!
    cbuf 10 i read-file 0= 'N + emit 10 = 'O + emit
    cellbuf 2@ i reposition-file 0= 'P + emit
    cbuf 10 + dup 10 i read-file 0= 'Q + emit cbuf 10 compare 'Q + emit
    i file-size 0= 'S + emit 2drop
    i file-eof? 'a + emit
    r> close-file 0= 'T + emit
    s" /tmp/gforth')(|&;test" w/o create-file 0= 'U + emit >r
    s" bla" i write-file 0= 'V + emit
    i flush-file 0= 'W + emit
    100. i resize-file 0= 'V + emit
    r> close-file 0= 'W + emit
    s" /tmp/gforth')(|&;test" s" /tmp/gforth'|&;test" rename-file 0= 'X + emit
    s" /tmp/gforth'|&;test" delete-file 0= 'Y + emit
    \ !! open-dir
    \ !! read-dir
    \ !! close-dir
    \ !! filename-match
    utime 2drop 'Y emit
    cputime 2drop 2drop 'Z emit
    \ !! all the FP stuff
    \ !! all the locals stuff
    \ !! syslib stuff
    \ !! ffcall stuff
    \ !! oldcall stuff
    \ compiler stuff
    ['] emit @ cellbuf !
    ['] ;s threading-method 0= if @ then cellbuf >body !
    cellbuf >body compile-prim1 'Y emit
    finish-code 'Z emit
    cellbuf execute 'a emit
    \ !! forget-dyncode
    cellbuf >body @ decompile-prim ['] ;s @ = 'c + emit
    cr
    depth (bye) ;

\ Setup                                                13feb93py

has? header [IF]
    \ set image size
    here image-header 2 cells + !         
    \ set image entry point
    ' boot >body  image-header 8 cells + A!         
[ELSE]
    >boot
[THEN]

\ include ./../kernel/pass.fs                    \ pass pointers from cross to target

.unresolved                          \ how did we do?

