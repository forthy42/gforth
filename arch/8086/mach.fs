\ **************************************************************
\ File:         MACH.FS
\                 MACH-file for GFORTH on PC
\ Autor:        Klaus Kohl (adaptet from volksFORTH_PC)
\ Log:          30.07.97 KK: file generated
\

    2 Constant cell
    1 Constant cell<<
    4 Constant cell>bit
    8 Constant bits/char
    8 Constant float
    2 Constant /maxalign
false Constant bigendian
( true=big, false=little )

: prims-include  ." Include primitives" cr s" arch/8086/prim.fs" included ;
: asm-include    ." Include assembler" cr s" arch/8086/asm.fs" included ;
: >boot          ." Prepare booting" cr
    s" ' boot >body into-forth 1+ !" evaluate ;

false Constant NIL

>ENVIRON

true  SetValue ec
true  SetValue crlf
false SetValue new-input     \ disables object oriented input
false SetValue peephole
true  SetValue f83headerstring
true  SetValue abranch       \ enables absolute branches
\ true Constant has-rom

cell 2 = [IF] 32 [ELSE] 256 [THEN] KB Constant kernel-size

16 KB		Constant stack-size
15 KB 512 +	Constant fstack-size
15 KB		Constant rstack-size
14 KB 512 +	Constant lstack-size
