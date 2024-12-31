\ test for Gforth primitives

\ Author: Anton Ertl, Bernd Paysan
\ Copyright (C) 2023,2024 Free Software Foundation, Inc.

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

\ this is a minimal viable image

[IFDEF] save-mem save-mem [THEN] 2Constant machine-file

include ./../cross.fs              \ cross-compiler

decimal

has? kernel-start has? kernel-size makekernel
\ create image-header
has? header [IF]
here 1802 over 
    A,                  \ base address
    has? kernel-size ,  \ dict size
    0 A,                \ image dp (without tags)
    0 A,                \ section name
    0 A,                \ locs[]
    NIL A,              \ primbits
    NIL A,              \ targets
    NIL A,              \ codestart
\    NIL A,              \ last-header
    has? stack-size ,   \ data stack size
    has? fstack-size ,  \ FP stack size
    has? rstack-size ,  \ return stack size
    has? lstack-size ,  \ locals stack size
    0 A,                \ boot entry point
    0 A,                \ quit entry point
    0 A,                \ execute entry point
    0 A,                \ find entry point
    0 ,                 \ checksum
    0 ,                 \ base of DOUBLE_INDIRECT xts[], for comp-i.fs
    0 ,                 \ base of DOUBLE_INDIRECT labels[], for comp-i.fs
[THEN]

doc-off
include kernel/aliases.fs             \ primitive aliases
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

\ helper words the cross compiler needs to resolve (but not use)

: named>string ( nt -- addr count ) ;
: named>link ( nt1 -- nt2 / 0 ) ;
: noname>string ( nt -- cfa 0 ) ;
: noname>link ( nt -- 0 ) ;
: value-to ( n value-xt -- ) ;
: default-name>comp ( nt -- w xt ) ;
: field+, ;
: defer, ;
: variable, ;
: constant, ;
: peephole-compile, ;
: :, ;
: does, ;
: compile, ;
: no-to ;

\ end helper words

\ Minimal Gforth boot program

: boot s" Hello World!" type cr 0 (bye) ;

\ Setup                                                13feb93py

\ set image size
here image-header + image-header #02 cells + !
.( set image entry point) cr
\ the minimal viable system only boots, and terminates from there
' boot >body  image-header #12 cells + A!

.unresolved                          \ how did we do?

