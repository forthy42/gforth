
include ../../asm/generic.fs
include ../../asm/bitmask.fs

hex

: mask0 postpone I-Latch postpone g@
	postpone maskinto 
	postpone I-Latch postpone g! ; immediate

: r5! 	mask0 	%0000001000001111 ;
: d5! 	mask0 	%0000000111110000 ;
: k6! 	mask0		%11001111 ;
: d2! 	mask0		%00110000 ;
: d4! 	mask0		%11110000 ;
: k8! 	mask0 	%0000111100001111 ;
: s3! 	mask0		%01110000 ;
: t3! 	mask0		%00000111 ;
: k7! 	mask0	%0000001111111000 ;
: p5! 	mask0		%11111000 ;
: x4! 	mask0		%11110000 ;
: p6! 	mask0	%0000011000001111 ;
: k22! 	dup $ffff and I-Latch 2 + g!
	$10 rshift 
	mask0 	%0000000111110001 ;
: q6!   mask0	%0010110000000111 ;
: k16!	I-Latch 2 + g! ;

: r,r r5! d5! ;
: rdl,k k6! &24 - d2! ;
: r,k k8! d4! ; \ ?!
: r, d5! ;
: s, s3! ; \ ?!
: rd,b t3! d5! ;
: s,k k7! t3! ;
: p,b t3! p5! ;
: x, x4! ;
: rd,p p6! d5! ;
: k, k22! ;
: rd,q q6! d5! ;
: q,rd swap rd,q ;
: rd,k16 k16! d5! ;

\ Addressing Modes

02 Mode 16Reg
03 Mode 32Reg
04 Mode SBit

10 Mode Z
11 Mode Z+
12 Mode -Z
14 Mode Y
15 Mode Y+
16 Mode -Y
18 Mode X
19 Mode X+
1A Mode -X

: SBit: ['] Sbit Reg ;
: R: ['] 16Reg Reg ;

0 R: R0
1 R: R1
2 R: R2
\ ...

\ Bits

0 SBit: /c
1 SBit: /z
2 SBit: /n
3 SBit: /v
4 SBit: /s
5 SBit: /h
6 SBit: /t
7 SBit: /i

\ Register commands

	R0 , R0 	' r,r	Alone( 	adc,  	%00011100 00
	R0 , R0		' r,r	Alone( 	add,  	%00001100 00
	R0 , 0		' rdl,k Alone( 	adiw, 	%10010110 00
	R0 , R0		' r,r	Alone( 	and,  	%00100000 00
	R0 , 0		' r,k  Alone( 	andi, 	%01110000 00
	R0		' r,	Alone( 	asr,  	%10010100 %0101
	/c		' s,	Alone( 	bclr, 	%10010100 %10001000
	R0 , 0		' rd,b	Alone( 	bld,  	%11111000 00
	/c , 0		' s,k	Alone( 	brbc, 	%11110100 00
	/c , 0		' s,k	Alone( 	brbs, 	%11110000 00
	/c		' s,	Alone( 	bset, 	%10010100 %00001000
	R0 , 0		' rd,b	Alone( 	bst,  	%11111010 00
	0		' k,	Alone( 	call,	%10010100 %00001110 00 00
	R0		' r,	Alone( 	com,  	%10010100 00
	R0 , R0		' r,r	Alone( 	cp,   	%00010100 00
	R0 , R0		' r,r	Alone( 	cpc,  	%00000100 00
	R0 , 0		' r,k	Alone( 	cpi,  	%00110000 00
	R0 , R0		' r,r	Alone( 	cpse, 	%00010000 00
	R0		' r, 	Alone( 	dec,  	%10010100 %00001010
	R0 , R0		' r,r	Alone( 	eor,  	%00100100 00
	0		' x,	Alone( 	icall, 	%10010101 %00001001
	0		' x,	Alone( 	ijmp, 	%10010100 %00001001
	R0 , 0		' rd,p	Alone( 	in,	%10110000 00
	R0		' r,	Alone( 	inc,	%10010100 %00000011
	0		' k,	Alone( 	jmp,	%10010100 %00001100 00 00
	R0 , 0 		' r,k	Alone(	ldi,	%11100000 00
	R0 , 0		' rd,k16 Alone(	lds,	%10010000 00 00 00
				Alone	lpm,	%10010101 %11001000
				\	lsl,	Macro
	R0		' r,	Alone(	lsr,	%10010100 %00000110
	R0 , R0		' r,r	Alone(	mov,	%00101100 00
	R0 , R0		' r,r	Alone( 	mul, 	%10011100 00
	R0		' r,	Alone(	neg,	%10010100 %00000001
				Alone	nop,	00 00
	R0 , R0		' r,r	Alone(	or,	%00101000 %00000000
	R0 , 0		' r,k	Alone(	ori,	%01100000 00
\ ?!
	R0 , R0		' r,r	Alone(	xor,	%00101000 %00000000

	
	
		
Table ld,
	R0 , X		' r,	Opc(	%10010000 %00001100
	R0 , X+		' r,	Opc(	%10010000 %00001101
	R0 , -X		' r,	Opc(	%10010000 %00001110
	R0 , Y		' r,	Opc(	%10010000 %00001000
	R0 , Y+		' r,	Opc(	%10010000 %00001001
	R0 , -Y		' r,	Opc(	%10010000 %00001010
	R0 , Z		' r,	Opc(	%10010000 %00000000
	R0 , Z+		' r,	Opc(	%10010000 %00000001
	R0 , -Z		' r,	Opc(	%10010000 %00000010
End-Table

Table ldd,
	R0 , Y+		' rd,q	Opc(	%10000000 %00001000
	R0 , Z+		' rd,q	Opc(	%10000000 %00000000
End-Table

Table st,
				Opc+	%00000010
Follows ld,

Table std,
	Y+ , R0		' q,rd	Opc(	%10000010 %00001000
	Z+ , R0		' q,rd	Opc(	%10000010 %00000000
End-Table

		
	

: brcc, /c , swap brbc, ;
: brcs, /c , swap brbs, ;
: breq, /z , swap brbs, ;
: brge, /s , swap brbc, ;
: brhc, /h , swap brbc, ;
: brhs, /h , swap brbs, ;
: brid, /i , swap brbc, ;
: brie, /i , swap brbs, ;
: brlo, /c , swap brbs, ;
: brlt, /s , swap brbs, ;
: brmi, /n , swap brbs, ;
: brne, /z , swap brbc, ;
: brpl, /n , swap brbc, ;
: brsh, /c , swap brbc, ;
: brtc, /t , swap brbc, ;
: brts, /t , swap brbs, ;
: brvc, /v , swap brbc, ;
: brvs, /v , swap brbs, ;

\ Macros

: cbr, $ff xor andi, ;
: clc, /c bclr, ;
: clh, /h bclr, ;
: cli, /i bclr, ;
: cln, /n bclr, ;
: clr, dup , xor, ;

: lsl, dup , R0 drop add, ;
