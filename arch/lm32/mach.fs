\ mach.fs	mach-file for LatticeMico32 CPU
\
\ Copyright (C) 2012 Free Software Foundation, Inc.

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

\ Author: David Kühling
\ Created: Feb 2012

4     Constant cell
2     Constant cell<<
5     Constant cell>bit
8     Constant bits/char
8     Constant float
4     Constant /maxalign
true  Constant bigendian
( true=big, false=little )

: prims-include  ." Include primitives" cr s" arch/lm32/prim.fs" included ;
: asm-include    ." Include assembler" cr s" arch/lm32/asm.fs" included ;
: >boot          ." Prepare booting" cr
\    s" include ../arch/r8c/errors.fs" evaluate
   \ patch lm32boot's body address into the commands that initialize FIP
   s" &3000 loops/ms !" evaluate
   s" ' lm32boot >body $10000 / into-forth 4 + DUP @ ROT OR SWAP !" evaluate
   s" ' lm32boot >body $FFFF AND into-forth 8 + DUP @ ROT OR SWAP ! " evaluate
   \ Workaround for SAVECROSS bug with kernel-start != 0
   s"   unlock .regions lock $40000000 here OVER - save-region kernl-lm32.fi- BYE " evaluate
;

false Constant NIL

>ENVIRON

true  SetValue ec
true  SetValue crlf
false SetValue new-input     \ disables object oriented input
false SetValue peephole
true  SetValue f83headerstring
true  SetValue abranch       \ enables absolute branches
true  SetValue games         \ include sokoban, tt games into image
\ true  SetValue rom

$40000000 SetValue kernel-start
$40000    SetValue kernel-size
   
\ 256 KB Constant kernel-size

16 KB		Constant stack-size
15 KB 512 +	Constant fstack-size
15 KB		Constant rstack-size
14 KB 512 +	Constant lstack-size



\ Customize Emacs
0 [IF]
   Local Variables:
   compile-command: "cd ../.. && ./build-ec lm32"
   End:
[THEN]
