\ Parameter for target systems                         06oct92py

\ Copyright (C) 1995,2003,2006,2007 Free Software Foundation, Inc.

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

    2 Constant cell
    1 Constant cell<<
    4 Constant cell>bit
    8 Constant bits/char
    8 Constant bits/byte
    8 Constant float
    2 Constant /maxalign
false Constant bigendian
( true=big, false=little )

\ feature list

false Constant NIL  \ relocating

: prims-include  ." Include primitives" cr s" arch/r8c/prim.fs" included ;
: asm-include    ." Include assembler" cr s" arch/r8c/asm.fs" included ;
: >boot  ." Setup boot parameters" cr
    s" include arch/r8c/errors.fs" evaluate
    s" >rom $2000 flash-dp !" evaluate
    s" unlock" evaluate
    s" >rom there >ram ram-start there over -" evaluate
    s" >rom there swap dup X allot tcmove" evaluate
    s" lock" evaluate
    s" cto ram-mirror" evaluate
    s" >ram here ram-start - >rom cto ram-size" evaluate
    s" $FF $10000 here - tcallot $C000 $FFFC ! $FF00 $FFFE !" evaluate
    s" ec/shex.fs" included
    s" 0 cpu-start" evaluate
    $C000 $4000 s" save-region-shex rom-r8c.mot" evaluate
    s" $FFFF $2FFC ! $FFFF $2FFE ! $2FFC 4 save-region-shex data-r8c.mot" evaluate
    s" >ram" evaluate
    $400 s" here over - save-region-shex ram-r8c.mot" evaluate ;

>ENVIRON

false SetValue file		\ controls the presence of the
				\ file access wordset
false SetValue OS		\ flag to indicate a operating system

false SetValue prims		\ true: primitives are c-code

false SetValue floating		\ floating point wordset is present

false SetValue glocals		\ gforth locals are present
				\ will be loaded
false SetValue dcomps		\ double number comparisons

false SetValue hash		\ hashing primitives are loaded/present

false SetValue xconds		\ used together with glocals,
				\ special conditionals supporting gforths'
				\ local variables
false SetValue header		\ save a header information

true SetValue ec
true SetValue crlf
true SetValue ITC
false SetValue new-input
false SetValue peephole
true SetValue abranch       \ enables absolute branches

true SetValue rom
true SetValue flash

true SetValue compiler
false SetValue primtrace
true SetValue no-userspace

\ cell 2 = [IF] 32 [ELSE] 256 [THEN] KB Constant kernel-size

16 KB		SetValue stack-size
15 KB 512 +	SetValue fstack-size
15 KB		SetValue rstack-size
14 KB 512 +	SetValue lstack-size

$C000 SetValue kernel-start
16 KB SetValue kernel-size

