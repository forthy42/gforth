\ arch/h8/asm.fs assebmler for the Hitachi H8 CPU		14aug97jaw

\ The syntax is not according to the Hitatchi reference manual.
\ Basicaly this is a forth assembler that makes heavy use of the stack.
\ The instruction must be written at last. The abstract syntax looks like:
\
\ operand:= register | address | value [ addressing-mode ]
\ instruction:= [ size-mode ] [ operand ] [ , operand ] mnemonic
\
\ All mnemonics end up with and comma, to avoid conflicts with normal
\ forth words.
\
\ E.g.:
\
\ .L $01020304 # , ER4 add,
\ $1234	jmp,
\ rts,
\
\ Addressing Modes:
\
\ The Hitachi syntax for addressing modes is very strange. Hitachis'
\ syntax says that everything with "@" gets something from memory.
\ So register indirect and memory absolute is all with "@". This is
\ easy to remember for forth-speaking people but poor in two ways:
\ "@" conflics with the forth-word, on normal CPUs there is a clear
\ syntax for "indirect" and "absolute" where it doesn't matter whether
\ we are working with a register or a memory address. This is good
\ because memory and registers could be almost the same on some CPUs,
\ or sometimes the registers are the only memory we have.
\ On some CPUs we could simply replace the register-label by a
\ memory-label when we are out of registers (works not for the H8 and
\ RISCs because absolute addressing only works in MOV)

\ Addressing Mode H8		H-Symbol	F-Symbol
\ Register direct		R0		R0
\ Register indirect		@ER0		ER0 ]
\ Register indirect with displ  @(0,ER0)	ER0 0 #]
\ Register ind. post increment	@ER0+		ER0 ]+
\ Register ind. pre decrement	@-ER0		ER0 -]
\ Absolute address		@01234		01234
\ PC relative			@(0,PC)		PC 0 #]
\ Memory indirect		@@12		12 ]
 
include asm/generic.fs
include asm/bitmask.fs

hex

-1 ByteDirection !

\ Bytes

: 22! ( c -- ) I-Latch 1 chars + c! ;
: 34! ( word -- ) I-Latch 2 chars + 2 g! ;
: 36! ( quad -- ) I-Latch 2 chars + 4 g! ;

\ Nibles r= right l= left

: 1nr! I-Latch c@ maskinto $0F I-Latch c! ;
: 2nr! I-Latch char+ c@ maskinto $0F I-Latch char+ c! ;
: 2nl! I-Latch char+ c@ maskinto $F0 I-Latch char+ c! ;
: 4nr! I-Latch 4 chars + c@ maskinto %00001111 I-Latch 4 chars + c! ;
: 4nl! I-Latch 4 chars + c@ maskinto %11110000 I-Latch 4 chars + c! ;

\ Small Nibles

: 2sr! I-Latch char+ c@ maskinto %00000111 I-Latch char+ c! ;
: 2sl! I-Latch char+ c@ maskinto %01110000 I-Latch char+ c! ;
: 4sr! I-Latch 4 chars + c@ maskinto %00000111 I-Latch 4 chars + c! ;
: 4sl! I-Latch 4 chars + c@ maskinto %01110000 I-Latch 4 chars + c! ;

\ ----- Addressing Modes

1 0Mode .B
2 0Mode .W
3 0Mode .L

01 Mode 8Reg
02 Mode 16Reg
03 Mode 32Reg

10 Mode #	\ Immediate
20 Mode ]	\ Register/Memory indirect
30 Mode #]	\ Register indirect with displacement
		\ room for long displacement
50 Mode ]+	\ Register indirect with post increment
60 Mode -]	\ Register indirect with pre decrement

100 Mode PC	\ Program counter ( used together with #] )

a0 Mode #1
b0 Mode #2
c0 Mode #4

\ Registers

: R: ['] 16Reg Reg ;
: ER: ['] 32Reg Reg ;

0 R: R0
1 R: R1
2 R: R2
3 R: R3
4 R: R4
5 R: R5
6 R: R6
7 R: R7

\ ... ?

0 ER: ER0
1 ER: ER1
2 ER: ER2
3 ER: ER3
4 ER: ER4

\ ... ?

: i8,r	 22! 1nr! ;
: r,r    2nr! 2nl! ;
: i16,r	 2nr! 34! ;
: er,er  2sr! 2sl! ;
: er,er4 4sr! 4sl! ;
: i32,er BREAK: 2sr! 36! ;
: ,er	 2sr! ;
: abs24  I-Latch char+ 3 g! ;
: abs8   I-Latch char+ c! ;
: ,r	 2nr! ;

Table add,
	.B # , R0 	' i8,r 		opc( 80 00
	.B R0 , R0	' r,r		opc( 08 00
	.W # , R0	' i16,r		opc( 79 10 00 00
	.W R0 , R0	' r,r		opc( 09 00
	.L # , ER0	' i32,er	opc( 7A 10 00 00 00 00
	.L ER0 , ER0 	' er,er		opc( 0A 80
End-Table

Table adds,
	#1 , ER0	' ,er		opc( 0B 00
	#2 , ER0	' ,er		opc( 0B 80
	#4 , ER0	' ,er		opc( 0B 90
End-Table

Table subs,	opc+ 10
Follows adds,

Table jmp,
	ER0 ]		' ,er		opc( 59 00
	0 		' abs24		opc( 5A 00 00
	0 ]		' abs8		opc( 5B 00
End-Table

Table jsr,	opc+ 04
Follows jmp,	

Table inc,
	.B R0		' ,r		opc( 0A 00
	.W #1 , R0	' ,r		opc( 0B 50
	.W #2 , R0	' ,r		opc( 0B D0
	.L #1 , ER0	' ,r		opc( 0B 70
	.L #2 , ER0	' ,r		opc( 0B F0
End-Table

Table dec,	opc+ 10
Follows inc,

Alone sleep, 01 80
Alone rts,   54 70
