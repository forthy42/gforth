\  NCEX simple postfix assembler
\
\  Copyright (C) 1998 Lars Krueger 
\
\  This file is part of FLK.
\
\  FLK is free software; you can redistribute it and/or
\  modify it under the terms of the GNU General Public License
\  as published by the Free Software Foundation, either version 3
\  of the License, or (at your option) any later version.
\
\  This program is distributed in the hope that it will be useful,
\  but WITHOUT ANY WARRANTY; without even the implied warranty of
\  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
\  GNU General Public License for more details.
\
\  You should have received a copy of the GNU General Public License
\  along with this program; if not, write to the Free Software
\  Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.

\ This file implements a classical postfix assembler for Intel 386+ and based
\ on that a register allocator for the optimizer. All operations are  
\ "src dest op,"  or  "dest op," . A postfix assembler is controlled by a few
\ variables that contain information about the type of operand, its size,
\ register number and offset. One set of variables is required per possible
\ operand i.e. a CPU supports commands like "add r0,r1,r17" meaning "add the
\ contents of r1 and r17 and store the result in r0", three sets of variables
\ are used. Apart from some stranger multiplication operations the Intel 386
\ never uses more than 2 operands, further referred to as source and
\ destination. 

\ At the begin of the operation and after each stored operation a pointer has
\ to be re-set that tells the system whether the source or the destination set
\ of variables is meant with the given operand.

\ For a detailed description of the 386 and above see Intels manuals.

\ Portability stuff.
: asm-r, , ;
: asm-, , ;
: asm-c, c, ;
: asm-here here ;
: asm-c! c! ;
: asm-! ! ;

\ Changes to the hexadecimal system are quite often, especially in the
\ assembler. The following word eases the pain of switching a lot.
\ Warning!!! $$ is state-smart.
: $$					( -<number>- )
  0.
  BL PARSE				\ wrd nwrd
  BASE @ >R
  HEX
  >NUMBER 2DROP
  R> BASE !
  DROP
  STATE @ IF POSTPONE LITERAL THEN
  ; IMMEDIATE

\ Number of local labels
10 CONSTANT MAXLOCALLABEL

\ The next few constants are here for readability.

\ The size of the operand. 
0 CONSTANT SZ-8 			\ al through dh
1 CONSTANT SZ-32 			\ eax through esi
2 CONSTANT SZ-UNKNOWN 			\ for memory references

\ The type of the operand.
0 CONSTANT RT-REG 			\ register
1 CONSTANT RT-INDEX 			\ [register]
2 CONSTANT RT-IMMED 			\ literal
3 CONSTANT RT-ABS 			\ [literal]

\ The scale of a sib-byte
0 CONSTANT SC-1
1 CONSTANT SC-2
2 CONSTANT SC-4
3 CONSTANT SC-8

\ Some error messages are nessesary too.
\ When this messages occurs something really went wrong.
: (internal-error) 			( -- ) 
  CR ." This is an internal assembler error." CR BYE ;
\ This message is printed whenever an invalid combination of operands is
\ found, i.e. after 0 [ebp] 0 [ecx] mov, .
: (unknown-combination) 	( -- )
  CR ." Illegal or unimplemented combination of operands." CR ABORT ;
\ If called with a true flag in TOS (top of stack) the register cache is full.
: TooManyRegs IF ." Too many registers requested." ABORT THEN ;
\ Called with TRUE when the sizes of the operands are different.
: (sz-mismatch) IF ." Mismatching sizes of operands." ABORT THEN ;

\ Called to complain about unknown size.
: (unknown-size) 		( size -- )
  SZ-UNKNOWN = IF ." Unknown operand size." ABORT THEN ;

\ Called to complain about wrong size.
: (wrong-size) 			( wrong? -- )
  IF ." Wrong operand size." ABORT THEN ;


\ These words check a type value for the given property.
: isreg? 				( type -- flag )
  RT-REG = ;
: ismem? 				( type -- flag )
  DUP RT-INDEX =  			\ type flag2
  SWAP RT-ABS = OR ;
: isr/m? 				( type -- flag )
  DUP isreg?  				\ type flag1
  SWAP ismem? OR ;
: isimm?  				( type -- flag )
  RT-IMMED = ;

\ The sets of variables containing the details of the operands.
VARIABLE (ts) VARIABLE (td) 		\ type
VARIABLE (rs) VARIABLE (rd) 		\ register number
VARIABLE (ss) VARIABLE (sd) 		\ size
VARIABLE (os) VARIABLE (od) 		\ offset/immediate ...

VARIABLE (sb) VARIABLE (db) 		\ sib base
VARIABLE (sc) VARIABLE (dc) 		\ sib scale
VARIABLE (si) VARIABLE (di) 		\ sib index

\ Word operations in the 386+ are distinguished from byte operations by
\ setting Bit 0 in the opcode. The following VALUE contains this Bit 0
0 VALUE (wrd)

\ To distinguish between the "normal" (non-SIB) addressing and the
\ SIB-addressing mode the following word is used. Setting if to TRUE makes the
\ mod/rm byte compiler switch to SIB-mode. It is reset by (asm-reset).
FALSE VALUE (sib)

\ To generate byte-offsets on demand the next VALUE is used. Does it contain
\ TRUE, byte offsets are generated.
FALSE VALUE (byte-offs)

\ Prefix to switch to byte offsets.
: BOFFS TRUE TO (byte-offs) ;
 
\ This word simply sets the word/byte bit in the opcode depending on the state
\ of (wrd).
: (w+) 					( opcode -- opcode )
  (wrd) + ;

\ This VALUE is the pointer telling whether source or destination VARIABLEs
\ are meant.
0 VALUE (#operands)

\ Since no syntax checking is performed (How? Why?) this check gives a minimum
\ security that the VARIABLEs contain valid numbers.
: (#operands?) 				( n -- )
  2 > IF ." Too many operands." ABORT THEN ;
  
\ Before giving the first operand to an operation the source/destination
\ pointer must be set to its initial state. This can be done by the user who
\ will forget that now and then or by the system that never forgets this.
\ Therefore this word must be executed before assembling the first instruction
\ and after the assembly of each instruction.
: (0operands) 				( -- )
  0 TO (#operands) ;
  
\ To advance the pointer from source to destination call this word. A check on
\ valid number of operands is placed here because this word is called by all
\ words that advance the pointer.
: (+operand) 				( -- )
  (#operands) DUP (#operands?)
  1+ TO (#operands) ;
  
\ The next word delivers the address of a source or destination VARIABLE
\ depending on the flag given. TRUE means the destination set, FALSE the
\ source.
: {s/d} 				( flag a-src a-dst -- addr )
  ROT IF
    NIP
  ELSE
    DROP
  THEN ;

\ For safety another check of valid operand number is performed before the
\ word decideds between source and destination depending on the state of the
\ source/destination pointer.
: (s/d) 				( addr-source addr-dest -- addr )
  (#operands) DUP (#operands?)
  0<> -ROT {s/d} ;
  
\ These words return the address of the variable depending on the state of the
\ source/destination pointer.
: (type) 	(ts) (td) (s/d) ;
: (size) 	(ss) (sd) (s/d) ;
: (regnum) 	(rs) (rd) (s/d) ;
: (offs) 	(os) (od) (s/d) ;

: (sib-scale) 	(sc) (dc) (s/d) ;
: (sib-index) 	(si) (di) (s/d) ;
: (sib-base) 	(sb) (db) (s/d) ;

\ These words return the address of the variable depending on the flag given.
\ FALSE means source, TRUE destination. All words have the stack effect 
\ ( flag -- addr ) .
: {type} 	(ts) (td) {s/d} ;
: {size} 	(ss) (sd) {s/d} ;
: {regnum} 	(rs) (rd) {s/d} ;
: {offs} 	(os) (od) {s/d} ;

: {sib-scale} 	(sc) (dc) {s/d} ;
: {sib-index} 	(si) (di) {s/d} ;
: {sib-base} 	(sb) (db) {s/d} ;

\ Since every instructions requires a different number of operands these
\ words perform the check.
: need0op 			( addr u -- )
  (#operands) 0 <> IF 
    ." Operation "  TYPE ."  needs no operands." CR BYE
  ELSE 2DROP THEN ;

: need1op 			( addr u -- )
  (#operands) 1 <> IF 
    ." Operation "  TYPE ."  needs one operand." CR BYE
  ELSE 2DROP THEN ;

: need2op 			( addr u -- )
  (#operands) 2 <> IF 
    ." Operation "  TYPE ."  needs two operands." CR BYE
  ELSE 2DROP THEN ;

\ Access to the source and destination operand variables is quite often used,
\ so make the job easier and define a few shortcuts.
: ts@ (ts) @ ;
: td@ (td) @ ;
: ss@ (ss) @ ;
: sd@ (sd) @ ;
: rs@ (rs) @ ;
: rd@ (rd) @ ;
: os@ (os) @ ;
: od@ (od) @ ;

\ Whenever the current operand should be a register, this word is called. It
\ is used both by the words for using a specific register and the meta
\ registers. Remember to give the size, because the number of the register
\ alone doesn't tell which register is meant.
: (#reg) 			( num size -- )
  RT-REG (type) !
  (size) !
  (regnum) !
  (+operand) ;

\ Refer to an indexed address. The base (or offset) is given by the user, the
\ number comes either from the words below or from the allocator.
: (#[reg]) 			( offset num -- )
  RT-INDEX (type) !
  (regnum) !
  (offs) ! (+operand) ;

\ Refer to an indexed address. Base, index, scale and offset are given by the
\ user.
: (#[sib]) 			( offs scale index base -- )
  RT-INDEX (type) !
  TRUE TO (sib)
  (sib-base) !
  (sib-index) !
  (sib-scale) ! 
  (offs) ! (+operand) ;

: [esp] 			( offs -- )
  SC-1 4 4 (#[sib]) ; 

\ Refer to an absolute address.
: #[] 				( addr -- )
  RT-ABS (type) !
  (offs) ! (+operand) ;
  
\ The value in TOS is an immediate value. Even though it is allowed only as a
\ source, this is not checked here. 
: ## 				( val -- )
  RT-IMMED (type) !
  (offs) ! (+operand) ;

\ Mark the current operand as a double-word (32 bit) or byte (8 bit).
\ Nessesary before xx ## [eax] mov,
: DWORD SZ-32 (size) ! ;
: BYTE SZ-8 (size) ! ;

\ Symbolic names for the registers are easier to remember, so provide words
\ for both 32 bit and 8 bit registers. Because of the register bl and the
\ constant BL collision, the 8 bit registers begin with "reg-" .
: eax 0 SZ-32 (#reg) ;
: ecx 1 SZ-32 (#reg) ;
: edx 2 SZ-32 (#reg) ;
: ebx 3 SZ-32 (#reg) ;
: esp 4 SZ-32 (#reg) ;
: ebp 5 SZ-32 (#reg) ;
: esi 6 SZ-32 (#reg) ;
: edi 7 SZ-32 (#reg) ;

: reg-al 0 SZ-8 (#reg) ;
: reg-cl 1 SZ-8 (#reg) ;
: reg-dl 2 SZ-8 (#reg) ;
: reg-bl 3 SZ-8 (#reg) ;
: reg-ah 4 SZ-8 (#reg) ;
: reg-ch 5 SZ-8 (#reg) ;
: reg-dh 6 SZ-8 (#reg) ;
: reg-bh 7 SZ-8 (#reg) ;

: [eax] 0 (#[reg]) ;
: [ecx] 1 (#[reg]) ;
: [edx] 2 (#[reg]) ;
: [ebx] 3 (#[reg]) ;
: [ebp] 5 (#[reg]) ;
: [esi] 6 (#[reg]) ;
: [edi] 7 (#[reg]) ;

\ This word performs three task. (i) it adjustes the sizes of operands, if one
\ size is unknown, (ii) it stops the execution if the sizes of both operands
\ and (iii) sets the word operation flag.
: (check-sizes) 			( -- )
  ss@ SZ-UNKNOWN = IF sd@ (ss) ! ELSE
  sd@ SZ-UNKNOWN = IF ss@ (sd) ! THEN THEN
  ss@ SZ-UNKNOWN = sd@ SZ-UNKNOWN = AND (sz-mismatch) 
  ss@ sd@ <> (sz-mismatch)
  ss@ SZ-32 = IF 1 ELSE 0 THEN 
  TO (wrd) ;
  
\ Writing an offset or not depends on the type of the memory operand. mop is
\ TRUE when the memory operand is the destination operand, rop when the
\ register operand is.
: (offs/rm), 			( mop rop -- )
  OVER DUP {offs} @ 		\ mop rop mop offs
  SWAP {regnum} @ 5 <> SWAP 0=
  AND IF ( [reg] ) 		\ mop rop
    {regnum} @ 8 * SWAP 
    {regnum} @ + asm-c,
  ELSE ( n [reg] ) 		\ mop rop
    OVER SWAP 			\ mop mop rop
    {regnum} @ 8 * SWAP 
    {regnum} @ + SWAP 		\ hmod/rm mop
    {offs} @ 			\ hmod/rm offs
    SWAP 
    (byte-offs) IF
      64 + asm-c, asm-c,
    ELSE
      128 + asm-c, asm-,
    THEN
  THEN ;

\ This word compiles a mod/rm byte from the settings of operand types,
\ register names etc. if no SIB-byte is required.
: (mod/rm-no-sib) 		( mop rop -- )
  OVER 				\ mop rop mop
  {type} @ 			\ mop rop mtype
  RT-ABS OVER = IF DROP 	\ mop rop 
    {regnum} @ 8 * 5 + asm-c, 	\ mop
    {offs} @ asm-, 		\ 
  ELSE 				\ mop rop mop
    RT-REG = IF 		\ mop rop 
      192 SWAP {regnum} @ 8 * + \ mop mod/rm
      SWAP {regnum} @ + asm-c, 	\ 
    ELSE ( index ) 		\ mop rop 
      (offs/rm), 		\ 
    THEN 
  THEN ;

\ This word compiles a mod/rm byte from the settings of operand types,
\ register names etc. if a SIB-byte is required.
: (mod/rm-sib) 			( mop rop -- )
  {regnum} @ 8 * OVER {offs} @ 
  IF 				\ mop opcocde
    128 + 
  THEN
  4 + asm-C, ( mod/rm ) 		\ mop
  DUP 2DUP 			\ mop mop mop mop
  {sib-scale} @ 6 LSHIFT 	\ mop mop mop sib
  SWAP {sib-index} @ 3 LSHIFT + \ mop mop sib
  SWAP {sib-base} @ + 		\ mop sib
  asm-C, 
  {offs} @ ?DUP IF asm-, THEN ;

\ This word is the main work horse of the assembler. It decides whether or not
\ to use sib addressing and calls the special compiler words for these cases.
: ((mod/rm)), 			( mop rop -- )
  (sib) IF
    (mod/rm-sib)
  ELSE
    (mod/rm-no-sib)
  THEN ;

\ The 386 can handle combinations with at least one memory operand. The
\ decision whether this memory operand is the source or the destination of an
\ operation is done by the opcode. This word is called with the value TRUE
\ when the memory operand is the source operand after the decision
\ produced this flag and compiled the opcode.
: (mod/rm), 			( source-is-rm -- )
  DUP INVERT SWAP ((mod/rm)), ;

\ In the reset-state, the sizes of both operands are unknown and no operands
\ have been accepted yet.
: (asm-reset) 			( -- )
  (0operands) 
  FALSE TO (sib)
  SZ-UNKNOWN (ss) !
  SZ-UNKNOWN (sd) ! 
  FALSE TO (byte-offs) ;

\ All tools for creating assembler operation words are ready so we can start
\ with the actual work.

\ The mov operation is a good example how such operation words are written. At
\ first the checks for valid number of operands and valid sizes are performed.
\ Then all supported register/memory combinations are compared with the
\ parameters and the right version is assembled then. If no combination can be
\ found, complain about it and leave. After successful assembly reset the
\ assembler.
: mov, 				( -- )
  S" mov," need2op
  (check-sizes)
  ts@ isreg? td@ isr/m? AND IF 	( mov r/m, r )
    $$ 88 (w+) asm-c, FALSE (mod/rm),
  ELSE 
  ts@ isr/m? td@ isreg? AND IF 	( mov r, r/m )
    $$ 8A (w+) asm-c, TRUE (mod/rm),
  ELSE
  td@ isreg? ts@ isimm? AND IF
    (wrd) 0<> IF $$ B8 ELSE $$ B0 THEN
    rd@ + asm-c, os@ 
    (wrd) 0<> IF  asm-, ELSE asm-c, THEN
  ELSE
  ts@ isimm? td@ isr/m? AND IF
    $$ C6 (w+) asm-c, 
    td@ CASE
      RT-REG   OF FALSE $$ C0 rd@ + ENDOF
      RT-ABS   OF TRUE 5 ENDOF
      RT-INDEX OF TRUE $$ 80 rd@ + ENDOF
      ." Can't address by ##. Use #[]. " ABORT
    ENDCASE
    asm-c, IF od@ asm-, THEN
    os@ asm-,
  ELSE
    (unknown-combination)
  THEN THEN THEN THEN
  (asm-reset) ;

\ Compile a jmp operation
: jmp, 				( -- )
  S" jmp," need1op
  ts@ RT-IMMED = IF 			( jmp 42 )
    os@ asm-here 5 + -
    $$ E9 asm-c, asm-,
  ELSE
  ts@ isr/m? ss@ SZ-8 <> AND IF
    ss@ SZ-32 <> (wrong-size)
    $$ FF asm-c, 4 ss@ (#reg) TRUE (mod/rm),
  ELSE
    (unknown-combination)
  THEN THEN
  (asm-reset) ;

\ Compile a conditional near (32 bit relative) jump
: (n-jcc,) 				( opcode addr len -- )
  need1op 				\ opcode
  ts@ RT-IMMED = IF
    $$ 0F asm-c, asm-c, os@ asm-here 4 + -
    asm-,
  ELSE
    (unknown-combination)
  THEN (asm-reset) ;

: n-ja,   $$ 87 S" n-ja,"   (n-jcc,) ;
: n-jae,  $$ 83 S" n-jae,"  (n-jcc,) ;
: n-jb,   $$ 82 S" n-jb,"   (n-jcc,) ;
: n-jbe,  $$ 86 S" n-jbe,"  (n-jcc,) ;
: n-jc,   $$ 82 S" n-jc,"   (n-jcc,) ;
: n-je,   $$ 84 S" n-je,"   (n-jcc,) ;
: n-jg,   $$ 8F S" n-jg,"   (n-jcc,) ;
: n-jge,  $$ 8D S" n-jge,"  (n-jcc,) ;
: n-jl,   $$ 8C S" n-jl,"   (n-jcc,) ;
: n-jle,  $$ 8E S" n-jle,"  (n-jcc,) ;
: n-jna,  $$ 86 S" n-jna,"  (n-jcc,) ;
: n-jnae, $$ 82 S" n-jnae," (n-jcc,) ;
: n-jnb,  $$ 83 S" n-jnb,"  (n-jcc,) ;
: n-jnbe, $$ 87 S" n-jnbe," (n-jcc,) ;
: n-jnc,  $$ 83 S" n-jnc,"  (n-jcc,) ;
: n-jne,  $$ 85 S" n-jne,"  (n-jcc,) ;
: n-jng,  $$ 8E S" n-jng,"  (n-jcc,) ;
: n-jnge, $$ 8C S" n-jnge," (n-jcc,) ;
: n-jnl,  $$ 8D S" n-jnl,"  (n-jcc,) ;
: n-jnle, $$ 8F S" n-jnle," (n-jcc,) ;
: n-jno,  $$ 81 S" n-jno,"  (n-jcc,) ;
: n-jnp,  $$ 8B S" n-jnp,"  (n-jcc,) ;
: n-jns,  $$ 89 S" n-jns,"  (n-jcc,) ;
: n-jnz,  $$ 85 S" n-jnz,"  (n-jcc,) ;
: n-jo,   $$ 80 S" n-jo,"   (n-jcc,) ;
: n-jp,   $$ 8A S" n-jp,"   (n-jcc,) ;
: n-jpe,  $$ 8A S" n-jpe,"  (n-jcc,) ;
: n-jpo,  $$ 8B S" n-jpo,"  (n-jcc,) ;
: n-js,   $$ 88 S" n-js,"   (n-jcc,) ;
: n-jz,   $$ 84 S" n-jz,"   (n-jcc,) ;

\ Compile a call
: call, 				( -- )
    S" call," need1op
    ss@ SZ-UNKNOWN = IF
      SZ-32 (ss) !
    THEN
    ss@ SZ-32 <> 
    IF ." Call address must be 32 bit" ABORT THEN
    ts@ RT-IMMED = IF 			( call 42 )
      os@ asm-here 5 + -
      $$ E8 asm-c, asm-,
    ELSE
    ts@ isr/m? IF
      $$ FF asm-c, 2 SZ-32 (#reg) TRUE (mod/rm),
    ELSE
      (unknown-combination)
    THEN THEN
    (asm-reset) ;
  
\ Compile a single byte instruction
: <single-byte> 			( byte -- )
  need0op asm-c, ;

: aaa, 		$$ 37 S" aaa,"   <single-byte> ;
: aas, 		$$ 3F S" aas,"   <single-byte> ;
: clc, 		$$ F8 S" clc,"   <single-byte> ;
: cld, 		$$ FC S" cld,"   <single-byte> ;
: cmc, 		$$ F5 S" cmc,"   <single-byte> ;
: cdq, 		$$ 99 S" cdq,"   <single-byte> ;
: cmpsb, 	$$ A6 S" cmpsb," <single-byte> ;
: cmpsd, 	$$ A7 S" cmpsd," <single-byte> ;
: daa, 		$$ 27 S" daa,"   <single-byte> ;
: das, 		$$ 2F S" das,"   <single-byte> ;
: movsb, 	$$ A4 S" movsb," <single-byte> ;
: movsd, 	$$ A5 S" movsd," <single-byte> ;
: movs, 	$$ A5 S" movs,"  <single-byte> ;
: lodsb, 	$$ AC S" lodsb," <single-byte> ;
: lodsd, 	$$ AD S" lodsd," <single-byte> ;
: nop, 		$$ 90 S" nop,"   <single-byte> ;
: repne, 	$$ F2 S" repne," <single-byte> ;
: repnz, 	$$ F2 S" repnz," <single-byte> ;
: repz, 	$$ F3 S" repz,"  <single-byte> ;
: popf, 	$$ 9D S" popf,"  <single-byte> ;
: ret, 		$$ C3 S" ret,"   <single-byte> ;
: pushf, 	$$ 9C S" pushf," <single-byte> ;
: rep, 		$$ F3 S" rep,"   <single-byte> ;
: repe, 	$$ F3 S" repe,"  <single-byte> ;
: scasb, 	$$ AE S" scasb," <single-byte> ;
: scasd, 	$$ AF S" scasd," <single-byte> ;
: stosd, 	$$ AB S" stosd," <single-byte> ;
: stc, 		$$ F9 S" stc,"   <single-byte> ;
: std, 		$$ FD S" std,"   <single-byte> ;
: xlat, 	$$ D7 S" xlat,"  <single-byte> ;
: stosb, 	$$ AA S" stosb," <single-byte> ;
: sahf, 	$$ E9 S" sahf,"  <single-byte> ;
: WORD: 	$$ 66 S" WORD:"  <single-byte> ;
: wait,         $$ 9B S" wait,"  <single-byte> ;

\ compile an alu-operation
: <alu> 					( eax,i32 r32,i32 col r,rm -- ) 
( OK )
  need2op
  (check-sizes)
  ts@ isimm? IF 				\ eax,i32 r32,i32 col r,rm
    DROP 					\ eax,i32 r32,i32 col
    td@ isreg? rd@ 0= AND IF
      2DROP (w+) asm-c, os@ asm-,
    ELSE
    td@ isr/m? IF 				\ eax,i32 r32,i32 col
      ROT DROP 					\ r32,i32 col
      SWAP (w+) asm-c, 				\ col
      (rs) ! 
      RT-REG (ts) ! 
      FALSE (mod/rm),
      os@ asm-,
    ELSE
     (unknown-combination)
   THEN THEN
  ELSE  					\ eax,i32 r32,i32 col r,rm
  NIP NIP NIP 					\ r,m
  td@ isr/m? ts@ isreg? AND IF
    (w+) asm-c, 
    FALSE (mod/rm),
  ELSE
  td@ isreg? ts@ isr/m? AND IF
    2 + (w+) asm-c, 
    TRUE (mod/rm),
  ELSE
    (unknown-combination)
  THEN THEN THEN
  (asm-reset) ;

: adc,  $$ 14 $$ 80 $$ 02 $$ 10 S" adc,"  <alu> ;
: add,  $$ 04 $$ 80 $$ 00 $$ 00 S" add,"  <alu> ;
: and,  $$ 24 $$ 80 $$ 04 $$ 20 S" and,"  <alu> ;
: cmp,  $$ 3C $$ 80 $$ 07 $$ 38 S" cmp,"  <alu> ;
: or,   $$ 0C $$ 80 $$ 01 $$ 08 S" or,"   <alu> ;
: sbb,  $$ 1C $$ 80 $$ 03 $$ 18 S" sbb,"  <alu> ;
: sub,  $$ 2C $$ 80 $$ 05 $$ 28 S" sub,"  <alu> ;
: test, $$ A8 $$ F6 $$ 00 $$ 84 S" test," <alu> ;
: xor,  $$ 34 $$ 80 $$ 06 $$ 30 S" xor,"  <alu> ;

\ produce a inc/dec
: <inc>, 		( column -- )
  S" inc/dec" need1op
  ts@ isreg? ss@ SZ-32 = AND IF
    8 * $$ 40 + rs@ + asm-c,
  ELSE
  ts@ isr/m? ts@ RT-ABS = OR IF
    $$ FE (w+) asm-c, SZ-32 (#reg) 
    TRUE (mod/rm),
  ELSE
    (unknown-combination)
  THEN THEN
  (asm-reset) ;

: inc, 0 <inc>, ;
: dec, 1 <inc>, ;

: sreg=ecx/cl? 				( -- flag )
  ts@ isreg? rs@ 1 = AND ;

\ produce a shift
: <shift>, 		( column -- )
( OK )
  S" shift" need2op
  sd@ SZ-32 = IF 1 ELSE 0 THEN TO (wrd)
  ts@ isimm? td@ isr/m? AND os@ 1 = AND IF
    $$ D0 (w+) asm-c, 
    RT-REG (ts) !
    (rs) !
    FALSE (mod/rm),
  ELSE
  ts@ isimm? td@ isr/m? AND IF
    $$ C0 (w+) asm-c,
    RT-REG (ts) !
    (rs) !
    FALSE (mod/rm),
    os@ asm-c,
  ELSE
  sreg=ecx/cl? td@ isr/m? AND IF
    $$ D2 (w+) asm-c,
    RT-REG (ts) !
    (rs) !
    FALSE (mod/rm),
  ELSE
    (unknown-combination)
  THEN THEN THEN
  (asm-reset) ;

: rol, 0 <shift>, ;
: ror, 1 <shift>, ;
: rcl, 2 <shift>, ;
: rcr, 3 <shift>, ;
: sal, 4 <shift>, ;
: shl, 4 <shift>, ;
: shr, 5 <shift>, ;
: sar, 7 <shift>, ;
 
\ compile a mul, div, neg or not
: <mul/neg>, 		( column -- )
  S" mul/neg/div" need1op
  ts@ isr/m? ss@ SZ-UNKNOWN <> AND IF
    $$ F6 (w+) asm-c, SZ-32 (#reg) TRUE (mod/rm),
  ELSE
    (unknown-combination)
  THEN
  (asm-reset) ;

: not,  2 <mul/neg>, ;
: neg,  3 <mul/neg>, ;
: mul,  4 <mul/neg>, ;
: imul, 5 <mul/neg>, ;
: div,  6 <mul/neg>, ;
: idiv, 7 <mul/neg>, ;

\ produce an exchange operation
: xchg, 					( -- )
  S" xchg," need2op (check-sizes)
  ts@ isreg? rs@ 0= AND ss@ SZ-32 = AND 
  td@ isreg? AND IF ( xchg eax, reg)
    rd@ $$ 90 + asm-c, 
  ELSE
    $$ 86 (w+) asm-c, ts@ ismem? (mod/rm),
  THEN 
  (asm-reset) ;

\ produce a push/pop
: <push/pop> 					( m32 col rd -- ) 
  S" push/pop" need1op
  ss@ SZ-32 <> 
  IF ." push, and pop, only work on DWORDs." ABORT THEN
  ts@ ismem? IF 				\ m32 col rd
    DROP 					\ m32 col
    SZ-32 (#reg) asm-c, TRUE (mod/rm), 
  ELSE
    ts@ isreg? IF 				\ m32 col
      rs@ + asm-c,
      2DROP
    ELSE
      (unknown-combination)
  THEN THEN (asm-reset) ;

: push, $$ FF 6 $$ 50 <push/pop> ;
: pop,  			( -- )
  S" pop," need1op
  ts@ isimm? IF
    $$ 68 asm-c,
    os@ asm-,
    (asm-reset)
  ELSE
    $$ 8F 0 $$ 58 <push/pop> 
  THEN ;

\ produce a setcc
: <setcc> 					( cc -- )
  S" setcc," need1op
  ss@ SZ-8 <> 
  IF ." setcc, requires a byte operand." ABORT THEN
  ts@ isr/m? IF
    $$ 0f asm-c, asm-c, 0 SZ-32 (#reg) TRUE (mod/rm),
  ELSE
    (unknown-combination)
  THEN
  (asm-reset) ;

: seta,   $$ 97 <setcc> ;
: setae,  $$ 93 <setcc> ;
: setb,   $$ 92 <setcc> ;
: setbe,  $$ 96 <setcc> ;
: setc,   $$ 92 <setcc> ;
: sete,   $$ 94 <setcc> ;
: setg,   $$ 9F <setcc> ;
: setge,  $$ 9D <setcc> ;
: setl,   $$ 9C <setcc> ;
: setle,  $$ 9E <setcc> ;
: setna,  $$ 96 <setcc> ;
: setnae, $$ 92 <setcc> ;
: setnb,  $$ 93 <setcc> ;
: setnbe, $$ 97 <setcc> ;
: setnc,  $$ 93 <setcc> ;
: setne,  $$ 95 <setcc> ;
: setng,  $$ 9E <setcc> ;
: setnge, $$ 9C <setcc> ;
: setnl,  $$ 9D <setcc> ;
: setnle, $$ 9F <setcc> ;
: setno,  $$ 91 <setcc> ;
: setnp,  $$ 9B <setcc> ;
: setns,  $$ 99 <setcc> ;
: setnz,  $$ 95 <setcc> ;
: seto,   $$ 90 <setcc> ;
: setp,   $$ 9A <setcc> ;
: setpe,  $$ 9A <setcc> ;
: setpo,  $$ 9B <setcc> ;
: sets,   $$ 98 <setcc> ;
: setz,   $$ 94 <setcc> ;

\ The local label mechanism is quite simple but useful.  The number passed to
\ jcond, is the number of the local label which can be used either as a
\ forward or backward jump label. Due to space constraints only one forward
\ jump can be used for one label, but an unlimited number of backward jumps to
\ this label. It is possible to use a label for a fwd jump first and then for
\ backward jumps. If you need more than one forward jump to the same place,
\ use different labels.
CREATE loclabel-tab MAXLOCALLABEL CELLS ALLOT

\ The given label number is checked and complained about if wrong.
: (chk-label-ind) 			( ind -- )
  MAXLOCALLABEL < INVERT 
  IF ." Label number too high." ABORT THEN ;

\ Provide simple access to the labels.
: >label 				( label -- addr )
  DUP (chk-label-ind)
  CELLS loclabel-tab + ;

\ An address of 0 for a label means that the label is not used yet. At the
\ start of each local label scope this state has to be set.
: reset-labels 			( -- )
  MAXLOCALLABEL 0 DO
    0 I >label !
  LOOP ;

: save-labels 			( -- labels n )
  MAXLOCALLABEL 0 DO
    I >label @
  LOOP MAXLOCALLABEL ;

: restore-labels 		( labels n -- )
  DROP
  MAXLOCALLABEL 0 DO
    MAXLOCALLABEL 1- I - >label !
  LOOP ;

\ Declare a local label. If the label is a forward jump calculate the offset,
\ check it and store it in the appropiate place.
: $: 					( label -- )
  DUP >label @ 0<> IF ( fwd jmp )
    DUP >label @ 			\ label dst-addr
    asm-here OVER 1+ - 			\ label dst-addr abs-dist
    DUP 127 < INVERT
    IF ." Jump out of bounds." ABORT THEN \ label dst-addr abs-dist
    SWAP asm-c!
  THEN
  asm-here SWAP >label ! ;

\ Change the short-branch-target to the give address.
: change-$: 				( addr label -- )
  >label ! ;

\ Compile a conditional jump.
: <jcc>, 					( label opcode -- )
  asm-c, 					\ label  
  DUP 
  >label @ 
  0= IF ( fwd jmp ) 				\ label 
    asm-here SWAP 				\ here label
    >label !
    0 asm-c,
  ELSE 	( bwd jmp) 				\ label
    asm-here 1+ SWAP >label @ 			\ dst orig
    - DUP 127 < INVERT
    IF ." Jump out of bounds." ABORT THEN  	\ abs-dist
    NEGATE asm-c,
  THEN ;

: jae,    $$ 73 <jcc>, ;
: jb,     $$ 72 <jcc>, ;
: jbe,    $$ 76 <jcc>, ;
: jc,     $$ 72 <jcc>, ;
: jcxz,   $$ E3 <jcc>, ;
: je,     $$ 74 <jcc>, ;
: jg,     $$ 7F <jcc>, ;
: jge,    $$ 7D <jcc>, ;
: jl,     $$ 7C <jcc>, ;
: ja,     $$ 77 <jcc>, ;
: jle,    $$ 7E <jcc>, ;
: jnle,   $$ 7F <jcc>, ;
: jna,    $$ 76 <jcc>, ;
: jno,    $$ 71 <jcc>, ;
: jnae,   $$ 72 <jcc>, ;
: jnp,    $$ 7B <jcc>, ;
: jnb,    $$ 73 <jcc>, ;
: jns,    $$ 79 <jcc>, ;
: jnbe,   $$ 77 <jcc>, ;
: jnz,    $$ 75 <jcc>, ;
: jnc,    $$ 73 <jcc>, ;
: jo,     $$ 70 <jcc>, ;
: jne,    $$ 75 <jcc>, ;
: jp,     $$ 7A <jcc>, ;
: jng,    $$ 7E <jcc>, ;
: jpe,    $$ 7A <jcc>, ;
: jnge,   $$ 7C <jcc>, ;
: jpo,    $$ 7B <jcc>, ;
: jnl,    $$ 7D <jcc>, ;
: js,     $$ 78 <jcc>, ;
: jz,     $$ 74 <jcc>, ;
: loopne, $$ E0 <jcc>, ;
: loopnz, $$ E0 <jcc>, ;
: loopz,  $$ E1 <jcc>, ;
: loop,   $$ E2 <jcc>, ;
: loope,  $$ E1 <jcc>, ;
\ Uncondition short jump.
: jmpn,   $$ EB <jcc>, ; 

\ ------------------------------------------------------------------------------
\ ---------------------------- floating point words ----------------------------
\ ------------------------------------------------------------------------------

\ FPU stack items
0 CONSTANT st0
1 CONSTANT st1
2 CONSTANT st2
3 CONSTANT st3
4 CONSTANT st4
5 CONSTANT st5
6 CONSTANT st6
7 CONSTANT st7

\ Check the given FPU register for valid index.
: st-range 				( st -- )
  0 8 WITHIN INVERT IF ." Invalid FP-Stack register." ABORT THEN ;

\ Compile an FPU operation requiring a mod/rm parameter.
: <fop-mod/rm>, 			( col op addr len -- )
  need1op asm-c,
  SZ-32 (#reg) TRUE (mod/rm),
  (asm-reset) ;

\ Compile an FPU operation without or with implicit parameters.
: <fop>, 			( oc1 oc2 addr len -- )
  need0op
  SWAP asm-c, asm-c,
  (asm-reset) ;

\ Compile an FPU operation with one FPU register as parameter.
: <fopst>, 			( st oc1 oc2 addr len -- )
  need0op PLUCK st-range
  SWAP asm-c, + asm-c,
  (asm-reset) ;

\ Store operations in different formats.
: fst32,   2 $$ D9 S" fst32,"   <fop-mod/rm>, ;
: fst64,   2 $$ DD S" fst64,"   <fop-mod/rm>, ;
: fstp32,  3 $$ D9 S" fstp32,"  <fop-mod/rm>, ;
: fstp64,  3 $$ DD S" fstp64,"  <fop-mod/rm>, ;
: fstp80,  7 $$ DB S" fstp80,"  <fop-mod/rm>, ;
: fist16,  2 $$ DF S" fist16,"  <fop-mod/rm>, ;
: fist32,  2 $$ DB S" fist32,"  <fop-mod/rm>, ;
: fistp16, 3 $$ DF S" fistp16," <fop-mod/rm>, ;
: fistp32, 3 $$ DB S" fistp32," <fop-mod/rm>, ;
: fistp64, 7 $$ DF S" fistp64," <fop-mod/rm>, ;
: fbstp,   6 $$ DF S" fbstp,"   <fop-mod/rm>, ;

\ Loads in different formats.
: fld32,   0 $$ D9 S" fld32,"   <fop-mod/rm>, ;
: fld64,   0 $$ DD S" fld64,"   <fop-mod/rm>, ;
: fld80,   5 $$ DB S" fld80,"   <fop-mod/rm>, ;
: fild16,  0 $$ DF S" fild16,"  <fop-mod/rm>, ;
: fild32,  0 $$ DB S" fild32,"  <fop-mod/rm>, ;
: fild64,  5 $$ DF S" fild64,"  <fop-mod/rm>, ;
: fbld,    4 $$ DF S" fbld,"    <fop-mod/rm>, ;

\ Other operations to memory.
: frstor,  4 $$ DD S" frstor,"  <fop-mod/rm>, ;
: fnsave,  6 $$ DD S" fnsave,"  <fop-mod/rm>, ;
: fnstcw,  7 $$ D9 S" fnstcw,"  <fop-mod/rm>, ;
: fldcw,   5 $$ D9 S" fldcw,"   <fop-mod/rm>, ;

\ Calculations, comparing ops, etc.
: fchs,     $$ D9 $$ E0 S" fchs,"     <fop>, ;
: fabs,     $$ D9 $$ E1 S" fabs,"     <fop>, ;
: f2xm1,    $$ D9 $$ F0 S" f2xm1,"    <fop>, ;
: fcos,     $$ D9 $$ FF S" fcos,"     <fop>, ;
: fscale,   $$ D9 $$ FD S" fscale,"   <fop>, ;
: fsin,     $$ D9 $$ FE S" fsin,"     <fop>, ;
: fsincos,  $$ D9 $$ FB S" fsincos,"  <fop>, ;
: fsqrt,    $$ D9 $$ FA S" fsqrt,"    <fop>, ;
: ftst,     $$ D9 $$ E4 S" ftst,"     <fop>, ;
: fxtract,  $$ D9 $$ F4 S" fxtract,"  <fop>, ;
: fyl2x,    $$ D9 $$ F1 S" fyl2x,"    <fop>, ;
: fyl2xp1,  $$ D9 $$ F9 S" fyl2xp1,"  <fop>, ;
: fnstswax, $$ DF $$ E0 S" fnstswax," <fop>, ;
: fcompp,   $$ DE $$ D9 S" fcompp,"   <fop>, ;
: fld1,     $$ D9 $$ E8 S" fld1,"     <fop>, ;
: fldl2t,   $$ D9 $$ E9 S" fldl2t,"   <fop>, ;
: fldl2e,   $$ D9 $$ EA S" fldl2e,"   <fop>, ;
: fldpi,    $$ D9 $$ EB S" fldpi,"    <fop>, ;
: fldlg2,   $$ D9 $$ EC S" fldlg2,"   <fop>, ;
: fldln2,   $$ D9 $$ ED S" fldln2,"   <fop>, ;
: fldz,     $$ D9 $$ EE S" fldz,"     <fop>, ;
: fincstp,  $$ D9 $$ F7 S" fincstp,"  <fop>, ;
: frndint,  $$ D9 $$ FC S" frndint,"  <fop>, ;
: fxam,     $$ D9 $$ E5 S" fxam,"     <fop>, ;
: fninit,   $$ DB $$ E3 S" fninit,"   <fop>, ;
: fpatan,   $$ D9 $$ F3 S" fpatan,"   <fop>, ;
: fprem,    $$ D9 $$ F8 S" fprem,"    <fop>, ;
: fprem1,   $$ D9 $$ F5 S" fprem1,"   <fop>, ;
: fptan,    $$ D9 $$ F2 S" fptan,"    <fop>, ;
: ftst,     $$ D9 $$ E4 S" ftst,"     <fop>, ;

: fld,      $$ D9 $$ C0 S" fld,"    <fopst>, ;
: fmulp,    $$ DE $$ C8 S" fmulp,"  <fopst>, ;
: faddp,    $$ DE $$ C0 S" faddp,"  <fopst>, ;
: fsubp,    $$ DE $$ E8 S" fsubp,"  <fopst>, ;
: fdivp,    $$ DE $$ F8 S" fdivp,"  <fopst>, ;
: ffree,    $$ DD $$ C0 S" ffree,"  <fopst>, ;
: fxch,     $$ D9 $$ C8 S" fxch,"   <fopst>, ;
: fstp,     $$ DD $$ D8 S" fstp,"   <fopst>, ;
: fmul,     $$ D8 $$ C8 S" fmul,"   <fopst>, ;
: fmulr,    $$ DC $$ C8 S" fmulr,"  <fopst>, ;
: fcomp,    $$ D8 $$ D8 S" fcomp,"  <fopst>, ;
: fcom,     $$ D8 $$ D0 S" fcom,"   <fopst>, ;
\ Since I am much too lazy to write a special kind of assembler word for fsub
\ I invented a mnemonic: fssub. It means: float-swap-subtract and is written
\ as: fsub st(i), st.
: fssub,    $$ DC $$ E8 S" fssub,"  <fopst>, ;
\ This is the normal subtraction operator: fsub st, st(i)
: fsub,     $$ D8 $$ E0 S" fsub,"   <fopst>, ;
\ The reverse subtraction with pop.
: fsubrp,   $$ DE $$ E0 S" fsubrp," <fopst>, ;

\ Compile a software interrupt.
: int, 				( nr -- )
  S" int," need0op
  $$ CD asm-c, asm-c, (asm-reset) ;

\ The load-effective-address operation. It is most useful with sib-addressing.
: lea, 				( -- )
  S" lea," need2op
  $$ 8D asm-c, TRUE (mod/rm), (asm-reset) ;

