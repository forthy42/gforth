\ test for Gforth primitives

\ Copyright (C) 2003 Free Software Foundation, Inc.

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

: cr ( -- )
    10 emit ;

: type ( addr u -- )
    stdout write-file drop ;

char j constant char-j

variable var-k
char k var-k !
defer my-emit
' emit is my-emit
cell% 2* 0 0 field >body

variable s0
: depth s0 @ sp@ cell+ - ;

\  : myconst ( n -- )
\      create ,
\    does> ( -- n )
\      @ ;
\  char m myconst char-m

: unloop-test ( -- )
    0 >r 0 >r unloop ;

: boot ( -- )
    sp@ s0 !
    [char] a stdout emit-file drop
    [char] b emit
    s" cde" type
    ." fgh"
    [char] i ['] emit execute
    ['] char-j execute emit
    ['] var-k execute @ emit
    \ !!douser
    [char] l ['] my-emit execute
    [char] l ['] my-emit ['] >body execute perform
    \ !!dodoes ['] char-m execute emit
    noop
    [char] m ['] my-emit ['] execute dup execute
    [char] m ['] 1+ execute emit
    [char] o ['] my-emit >body perform
    unloop-test ." p"
    [char] q my-emit
    \ !!does-exec
    \ !! branch-lp+!#
    ahead ." wrong" then ." r"
    0 if ." wrong" else ." s" then
    1 if ." t" else ." wrong" then
    \ !! ?dup-?branch ?dup-0=-?branch
    \ 0 ?dup-if ." wrong" drop else ." u" then
    \ [char] v ?dup-if emit else ." wrong" then
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

