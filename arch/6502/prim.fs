\ Copyright (C) 1999,2000,2003,2007 Free Software Foundation, Inc.

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

hex

start-macros
$10
dup EQU ip 3 +
dup EQU W 2 +
dup EQU N 8 +
drop

\ Debugging stuff

\ uncomment this if you want to see want primitives are executed

: dout	drop
\	#. 	LDA,
\	a-out 	JSR, 
;

\ : .depth
\ 		TXA,
\	FF #.	EOR,
\		CLC,
\	'0 #.	ADC,
\	a-out	JSR, 
\ ;

4 EQU XT>BODY
end-macros

UNLOCK also assembler definitions
: back2 -2 tdp +! ;
LOCK

0 CONSTANT BOT-

LABEL IP-R 0 ,
LABEL IntoForth
	FF #. 	LDX,
	IP-R	LDA,
	IP	STA,
	IP-R 1+ LDA,
	IP 1+	STA,
	6C #.	LDA,
	W 1-	STA,
LABEL "Next"
LABEL Next
	'. dout
\	.depth
	1 #.	LDY,
	IP ).	LDA,
	W	STA,
	IP )Y	LDA,
	W 1+ 	STA,	\ n points to cfa now
	IP	INC,	\ increment IP, code is aligned...
	IP	INC,
	1 $	BNE,	\ so wrap around could only be here!
	IP 1+ 	INC,
1 $:
LABEL Next1
	W 1-	JMP,
END-LABEL

start-macros
: next, "Next" JMP, ;
end-macros

Code: :docol	\
	': dout
	IP 1+		LDA,	\ save IP
			PHA,
	IP  		LDA,
			PHA,
			CLC,
	W     		LDA,	\ and load it with body address
	XT>BODY #. 	ADC,
	IP		STA,
	W 1+ 		LDA,
	0 #.		ADC,
	IP 1+ 		STA,
			Next,
end-code

Code: :docon	\
	'1 dout
	XT>BODY #. 	LDY,	\ put body content on stack
	W )Y		LDA,
			DEX, DEX,
	BOT- ,X		STA,
			INY,
	W )Y		LDA,
	BOT- 1+ ,X 	STA,			
			Next,
end-code

Code: :dovar	\
	'2 dout
			DEX, DEX, \ put body address on stack
			CLC,
	W		LDA,
	XT>BODY #.	ADC,
	BOT- ,X		STA,
	W 1+		LDA,
	0 #.		ADC,
	BOT- 1+ ,X	STA,
			Next,
end-code

Code: :dodoes
	'6 dout
	        	DEX, DEX, \ put body address on stack
                        CLC,
        W               LDA,
        XT>BODY #.     ADC,
        BOT- ,X         STA,
        W 1+            LDA,
        0 #.            ADC,
        BOT- 1+ ,X      STA,
	IP 1+		LDA,	\ save IP
			PHA,
	IP  		LDA,
			PHA,
			CLC,
	W     		LDA,	\ set W to W+2
	2 #. 		ADC,
	W		STA,
	0 $		BCC,
	W 1+		INC,
0 $:
	1 #.		LDY,	\ fetch IP from does field
	W ).		LDA,
	IP		STA,
	W )Y		LDA,
	IP 1+		STA,		
			Next,
end-code
 	
Code: :dodefer
	'4 dout
	XT>BODY #. 	LDY,	\ 
	W )Y		LDA,
	N		STA,
			INY,
	W )Y		LDA,
	W 1+ 	 	STA,
	N		LDA,
	W		STA,
	Next1		JMP,
end-code

require ./zero.fs

code: :douser
	'3 dout
			DEX, DEX,
	XT>BODY #.	LDY,
			CLC,
	W )Y		LDA,
	UP		ADC,
	BOT- ,X		STA,
			INY,
	W )Y		LDA,
	UP 1+		ADC,
	BOT- 1+ ,X	STA,
			Next,
end-code

code: :doesjump
end-code

require ./softuart.fs

: up! up ! ;

code ;s		\
	'; dout
			PLA,
	IP		STA,
			PLA,
	IP 1+		STA,
			Next,
end-code


code execute               \ ( Addr1 ---  )
	'E dout
	BOT- ,X 	LDA,   \ Addr1 is a CFA that will be executed
      	W           	STA,
      	BOT- 1+  ,X 	LDA,
       	W 1+        	STA,
                	INX, INX,
			Next1 JMP,
end-code

code branch
label dobranch
	1 #.		LDY,
	IP ).		LDA,
			CLC,
	IP		ADC,
			PHA,
	IP )Y		LDA,
	IP 1+		ADC,
	IP 1+		STA,
			PLA,
	IP		STA,
			Next,
end-code

code ?branch
	'? dout
	BOT- ,X		LDA,
	BOT- 1+ ,X	ORA,
			INX, INX,
	FF #.		AND,
	dobranch	BEQ,
label doskip
			CLC,
	IP		LDA,
	2 #.		ADC, 
	IP		STA,
	2 $		BCC,
	IP 1+		INC,
2 $:			Next,
end-code

code (loop)
			PLA,
			TAY,
			PLA,
	N 1+		STA,
			INY,
	1 $		BNE,
	N 1+		INC,
1 $:			PLA,
	N 2+		STA,
	N 2+		CPY,
	2 $		BNE,
			PLA,
			PHA,
	N 1+		CMP,
	2 $		BNE,
			SEC,
	3 $		BCS,		
2 $:			CLC,
3 $:	N 2+		LDA,
			PHA,
	N 1+		LDA,
			PHA,
			TYA,
			PHA,
	doskip		BCS,
	dobranch	BCC,
end-code

CODE @ 		\
	'@ dout
          BOT-     X) LDA,         \ Push Content of Addr1 to Stack
                      TAY,
          BOT-     ,X INC,         \ Content of Addr1 is 16Bit
1 $                   BNE,
          BOT- 1+  ,X INC,
1 $:
          BOT-     X) LDA,
          BOT- 1+  ,X STA,
          BOT-     ,X STY,
	Next,
end-code

CODE !           \
	'! dout
          BOT- 2+  ,X LDA,         \ Store N1 in Addr1
          BOT-     X) STA,         \ Content of Addr1 is 16Bit
          BOT-     ,X INC,
		1 $   BNE,
          BOT- 1+  ,X INC,
1 $:
          BOT- 3 + ,X LDA,
          BOT-     X) STA,
  	INX, INX, INX, INX,
	Next,
end-code

CODE XOR   	
	'x dout
         BOT-     ,X LDA,       \ 16Bit - EXOR
         BOT- 2+  ,X EOR,
         BOT- 2+  ,X STA,
         BOT- 1+  ,X LDA,
         BOT- 3 + ,X EOR,
         BOT- 3 + ,X STA,
           INX, INX,
	Next,
end-code

CODE OR         
	'o dout
         BOT-     ,X LDA,       \ 16Bit - EXOR
         BOT- 2+  ,X ORA,
         BOT- 2+  ,X STA,
         BOT- 1+  ,X LDA,
         BOT- 3 + ,X ORA,
         BOT- 3 + ,X STA,
           INX, INX,
	Next,
end-code

CODE AND                      \ ( UN1 UN2 --- UN3 )
	'a dout
         BOT-     ,X LDA,       \ 16Bit - EXOR
         BOT- 2+  ,X AND,
         BOT- 2+  ,X STA,
         BOT- 1+  ,X LDA,
         BOT- 3 + ,X AND,
         BOT- 3 + ,X STA,
           INX, INX,
	Next,
end-code

CODE +	\
	'+ dout
                     CLC,        \ N1 + N2 = N3
         BOT-     ,X LDA,
         BOT- 2+  ,X ADC,
         BOT- 2+  ,X STA,
         BOT- 1+  ,X LDA,
         BOT- 3 + ,X ADC,
         BOT- 3 + ,X STA,
           INX, INX,
	Next,
end-code

CODE -                         \ ( N1 N2 --- N3 )
	'- dout
                     SEC,       \ N1 - N2 = N3
         BOT- 2+  ,X LDA,
         BOT-     ,X SBC,
         BOT- 2+  ,X STA,
         BOT- 3 + ,X LDA,
         BOT- 1+  ,X SBC,
         BOT- 3 + ,X STA,
           INX, INX,
	Next,
end-code

Code >r		\
	'R dout
	BOT- 1+ ,X 	LDA,
			PHA,
	BOT- ,X		LDA,
			PHA,
			INX, INX,
			Next,
end-code

Code r>		\	
	'r dout
			DEX, DEX,
			PLA,
	BOT- ,X		STA,
			PLA,
	BOT- 1+ ,X	STA,
			Next,
end-code

Code RP@
	'p dout
			DEX, DEX,
	N		STX,
			TSX,
			TXA,
	N		LDX,
	BOT- ,X		STA,
	01 #.		LDA,
	BOT- 1+ ,X	STA,
			Next,
end-code

Code RP!
	'P dout
	BOT- ,X		LDA,
	N		STX,
			TAX,
			TXS,
	N		LDX,
			INX, INX,
			Next,		
end-code	

Code SP@	\
	's dout
			TXA,
			DEX, DEX,
	BOT- ,X		STA,
	0 #.		AND,
	BOT- 1+ ,X	STA,
			Next,
end-code

Code SP!
	'S dout
	BOT- ,X		LDA,	
			TAX,
			Next,
end-code

Code 2/
	'/ dout
	BOT- 1+ ,X	LDA,
		A.	ROL,	
	BOT- 1+	,X	ROR,
	BOT- ,X		ROR,
			Next,
end-code
	
code C@                           \ ( ADDR1 --- C1 )
	'c dout
          BOT-     X) LDA,         \ Push Content of Addr1 to Stack
          BOT-     ,X STA,         \ Content of Addr1 is a Byte
                00 #. LDA,
          BOT- 1+  ,X STA,
                      Next,
end-code

code C!                            \ ( C1 ADDR1 --- )
	'C dout
          BOT- 2+  ,X LDA,          \ Store C1 in Addr1
          BOT-     X) STA,          \ Content of Addr1 is a byte
  INX, INX, INX, INX,
			Next,
end-code

code CMOVE               \ ( Addr1 Addr2 N1  ---     )
                          \ Addr1 = FROM
                          \ Addr2 = TO
                          \ N1    = Number of Bytes
                          \ First Byte copied first
	'M dout
         0       #. LDY,
3 $:     BOT-    ,X LDA,
         N       ,Y STA,
                    INX,
                    INY,
         06      #. CPY,
3 $                 BNE,
         0       #. LDY,
1 $:     N          CPY, 
2 $                 BNE, 
         N 1 +      DEC,
2 $                 BPL, 
		Next,
2 $:     N 4 +   )Y LDA, 
         N 2 +   )Y STA, 
                    INY,
1 $                 BNE,
         N 5 +      INC,
         N 3 +      INC,
1 $                 JMP,
end-code

CODE U<                         \ ( N1 N2 --- F1 )
               00 #. LDY,        \ N1 > N2   F1 = 0001
                     CLC,        \ N1 < N2   F1 = 0000
         BOT-     ,X LDA,        \ N1 = N2   F1 = 0000
         BOT- 2+  ,X SBC,
         BOT- 1+  ,X LDA,
         BOT- 3 + ,X SBC,
1 $                  BCC,
                     DEY,
1 $:     BOT- 2+  ,X STY,
         BOT- 3 + ,X STY,
                INX, INX,
		Next,
end-code

CODE 1-                        \ ( N1 --- N2 )
         BOT-     ,X LDA,       \ N1 - 1 = N2
1 $                  BEQ,
         BOT-     ,X DEC,
                     Next,
1 $:     BOT- 1+  ,X DEC,
         BOT-     ,X DEC,
                     Next,
end-code

code 1+                        \ ( N1 --- N2 )
         BOT-     ,X INC,       \ N1 + 1 = N2
1 $                  BNE,
         BOT- 1+  ,X INC,
1 $:                 Next,
end-code

code =                         \ ( N1 N2 --- F1 )
                0 #. LDY,
         BOT-     ,X LDA,       \ N1 = N2    F1 = 0001
         BOT- 2+  ,X CMP,       \ N1 >< N2   F1 = 0000
1 $                  BNE,
         BOT- 1+  ,X LDA,
         BOT- 3 + ,X CMP,
1 $                  BNE,
                     INY,
1 $:     BOT- 2+  ,X STY,
         BOT- 3 + ,X STY,
                INX, INX,
                     Next,
end-code

code OVER                       \ ( N1 N2 --- N1 N2 N1 )
         BOT- 2+  ,X LDA,
         BOT- 3 + ,X LDY,
                DEX, DEX,
         BOT-     ,X STA,
         BOT- 1+  ,X STY,
                     Next,
end-code

code DROP                        \ ( N1 --- )
           INX, INX, Next,
end-code

code 2DROP                       \ ( D1 --- )
 INX, INX, INX, INX, Next,
end-code

code SWAP                        \ ( N1 N2 --- N2 N1 )
          BOT-     ,X LDA,
          BOT- 2+  ,X LDY,
          BOT- 2+  ,X STA,
          BOT-     ,X STY,
          BOT- 1+  ,X LDA,
          BOT- 3 + ,X LDY,
          BOT- 3 + ,X STA,
          BOT- 1+  ,X STY,
                      Next,
end-code

code DUP                         \ ( N1 --- N1 N1 )
          BOT-     ,X LDY,
          BOT- 1+  ,X LDA,
                 DEX, DEX,
          BOT-     ,X STY,
          BOT- 1+  ,X STA,
                      Next,
end-code

code ROT                     \ ( N1 N2 N3 --- N2 N3 N1 )
          BOT-     ,X LDY,
          BOT- 4 + ,X LDA,
          BOT-     ,X STA,
          BOT- 2 + ,X LDA,
          BOT- 4 + ,X STA,
          BOT- 2 + ,X STY,
          BOT- 5 + ,X LDY,
          BOT- 3 + ,X LDA,
          BOT- 5 + ,X STA,
          BOT- 1 + ,X LDA,
          BOT- 3 + ,X STA,
          BOT- 1 + ,X STY,
                      Next,
end-code

code 2SWAP                       \ ( D1 D2 --- D2 D1 )
                4 #. LDA,
         N           STA,
1 $:     BOT- 4 + ,X LDY,
         BOT-     ,X LDA,
         BOT-     ,X STY,
         BOT- 4 + ,X STA,
                     INX,
         N           DEC,
1 $                  BNE,
      DEX, DEX, DEX, DEX,
                     Next,
end-code

code 2DUP                       \ ( N1 N2 --- N1 N2 N1 )
         BOT- 2+  ,X LDA,
         BOT- 3 + ,X LDY,
                DEX, DEX,
         BOT-     ,X STA,
         BOT- 1+  ,X STY,
         BOT- 2+  ,X LDA,
         BOT- 3 + ,X LDY,
                DEX, DEX,
         BOT-     ,X STA,
         BOT- 1+  ,X STY,
                     Next,
end-code

code +!                           \ ( N1 ADDR1 --- )
                      CLC,         \ Add N1 to the Content of Addr1
          BOT-     X) LDA,         \ Content of Addr1 is 16Bit
          BOT- 2+  ,X ADC,
          BOT-     X) STA,
          BOT-     ,X INC,
1 $                   BNE,
          BOT- 1+  ,X INC,
1 $:      BOT-     X) LDA,
          BOT- 3 + ,X ADC,
          BOT-     X) STA,
  INX, INX, INX, INX, Next,
end-code

code lit
			DEX, DEX,
	1 #.		LDY,
	IP ).		LDA,
	BOT- ,X		STA,
	IP )Y		LDA,
	BOT- 1+ ,X	STA,
			CLC,
	IP		LDA,
	02 #.		ADC,
	IP		STA,
	1 $		BCS,
			Next,
1 $:	IP 1+		INC,
			Next,
end-code

code (find-samelen) ( u f83name1 -- u f83name2/0 )
	BOT- ,X		LDA,
	N		STA,
	BOT- 1+ ,X	LDA,
	N 1+		STA,
	1 #.		LDY,
2 $:			INY,
	N )Y		LDA,
	1F #.		AND,
	BOT- 2+ ,X	CMP,
	1 $		BEQ,
			DEY,
	N ).		LDA,
			PHA,
	N )Y		LDA,
	N 1+		STA,
			PLA,
	N		STA,
	N 1+		ORA,
	2 $		BNE,
1 $:	N		LDA,
	BOT- ,X		STA,
	N 1+		LDA,
	BOT- 1+ ,X	STA,		
			Next,
end-code

code (f83find) ( addr len f83name -- f83name|0 )
	BOT- ,X		LDA,	\ setup search pointer
	N		STA,
	BOT- 1+ ,X	LDA,
	N 1+		STA,
	1 #.		LDY,
	BOT- 4 + ,X	LDA,	\ pointer to string
			SEC,
	3 #.		SBC,
	N 2+ 		STA,
	BOT- 5 + ,X	LDA,
	0 #.		SBC,
	N 3 +		STA,
2 $:			INY,	\ loop - findsamelen
	N )Y		LDA,
	1F #.		AND,
	BOT- 2+ ,X	CMP,
	1 $		BEQ,
			DEY,
3 $:	N ).		LDA,	\ next word
			PHA,
	N )Y		LDA,
	N 1+		STA,
			PLA,
	N		STA,
	N 1+		ORA,
	2 $		BNE,
10 $:			INX, INX, INX, INX, \ found / notfound
	N		LDA,	\ pointer to stack
	BOT- ,X		STA,
	N 1+		LDA,
	BOT- 1+ ,X	STA,		
			Next,
1 $:			CLC,	\ found same len
	2 #.		ADC,
	N 4 +		STA,	\ end marker
6 $:			INY,
	N )Y		LDA,	\ loop string compare
	N 2+ )Y		CMP,
	4 $		BNE,	\ not the same ->
5 $:	N 4 +		CPY,	\ end?
	10 $		BEQ,	\ found something ->
	6 $		BNE,	\ always ->
4 $:	\ start of case insensitive compare
			SEC,	\ check whether chars differ 'a-'A
	N )Y		LDA,
	N 2+ )Y		SBC,
	'a 'A - #.	CMP,
	12 $		BEQ,
	'A 'a - FF and #. CMP,
	13 $		BNE,
	N 2+ )Y		LDA,	\ second must between a and z
	FFFF		BIT,
	back2			\ trick excape next opcode!
12 $:	N )Y		LDA,	\ first must between a and z
	'a #.		CMP,
	13 $		BCC,
	'z 1+ #.	CMP,
	5 $		BCC,
13 $:	1 #.		LDY,
	3 $		JMP,	\ don't make case insensitive ->
end-code

has? hash [IF]
code (hashfind) ( addr len a_addr -- f83name|0 )
	BOT- ,X		LDA,	\ setup search pointer
	W		STA,
	BOT- 1+ ,X	LDA,
	W 1+		STA,
	BOT- 4 + ,X	LDA,	\ pointer to string
			SEC,
	3 #.		SBC,
	N 2+ 		STA,
	BOT- 5 + ,X	LDA,
	0 #.		SBC,
	N 3 +		STA,
	W		LDA,
	11 $		BNE,
	11 $		BEQ,	\ check first ->
2 $:	2 #.		LDY,	\ copy word-pointer to N
	W )Y		LDA,
	N		STA,
			INY,
	W )Y		LDA,
	N 1+		STA,
			DEY,	\ loop - findsamelen
	N )Y		LDA,
	1F #.		AND,
	BOT- 2+ ,X	CMP,
	1 $		BEQ,
			DEY,
3 $:	W ).		LDA,	\ next word
			PHA,
	W )Y		LDA,
	W 1+		STA,
			PLA,
	W		STA,
11 $:	W 1+		ORA,
	2 $		BNE,
			INX, INX, INX, INX, \ not found
	BOT- ,X		STA,
	BOT- 1+ ,X 	STA,
			Next,

10 $:			INX, INX, INX, INX, \ found / notfound
	N		LDA,	\ pointer to stack
	BOT- ,X		STA,
	N 1+		LDA,
	BOT- 1+ ,X	STA,		
			Next,
1 $:			CLC,	\ found same len
	2 #.		ADC,
	N 4 +		STA,	\ end marker
6 $:			INY,
	N )Y		LDA,	\ loop string compare
	N 2+ )Y		CMP,
	4 $		BNE,	\ not the same ->
5 $:	N 4 +		CPY,	\ end?
	10 $		BEQ,	\ found something ->
	6 $		BNE,	\ always ->
4 $:	\ start of case insensitive compare
			SEC,	\ check whether chars differ 'a-'A
	N )Y		LDA,
	N 2+ )Y		SBC,
	'a 'A - #.	CMP,
	12 $		BEQ,
	'A 'a - FF and #. CMP,
	13 $		BNE,
	N 2+ )Y		LDA,	\ second must between a and z
	FFFF		BIT,
	back2			\ trick excape next opcode!
12 $:	N )Y		LDA,	\ first must between a and z
	'a 1- #.	CMP,
	13 $		BCC,
	'z #.		CMP,
	5 $		BCC,
13 $:	1 #.		LDY,
	3 $		JMP,	\ don't make case insensitive ->
end-code
[THEN]

code toupper
	BOT- ,X		LDA,
			SEC,
	'a #.		SBC,
	1 $		BCC,	\ overflow, was smaller ->
	'z 'a -	#.	SBC,
	1 $		BCS,	\ no overflow, is bigger ->
	'z 'a - 'A + #.	ADC,
	BOT- ,X		STA,
1 $:			Next,
end-code

code i
			DEX, DEX,
			PLA,
			TAY,
			PLA,
	BOT- 1+ ,X	STA,
			PHA,
			TYA,
	BOT- ,X		STA,
			PHA,
			Next,
end-code

code i'
			DEX, DEX,
			PLA,
	N		STA,
			PLA,
	N 1+		STA,
			PLA,
			TAY,
			PLA,
	BOT- 1+ ,X	STA,
			PHA,
			TYA,
	BOT- ,X		STA,
			PHA,
	N 1+		LDA,
			PHA,
	N		LDA,
			PHA,
			Next,
end-code

code 0=
	BOT- ,X		LDA,
	BOT- 1+ ,X	ORA,
	1 $		BEQ,
 	FF #.		LDA,
1 $:	FF #.		EOR,
	BOT- ,X		STA,
	BOT- 1+ ,X	STA,
			Next,
end-code

code 0<>
	BOT- ,X		LDA,
	BOT- 1+ ,X	ORA,
	1 $		BEQ,
 	FF #.		LDA,
	BOT- ,X		STA,
	BOT- 1+ ,X	STA,
1 $:			Next,
end-code

code 0<
	BOT- 1+ ,X	LDA,
	80 #.		AND,
	1 $		BEQ,
 	FF #.		LDA,
1 $:	BOT- ,X		STA,
	BOT- 1+ ,X	STA,
			Next,
end-code

code <>
	BOT- ,X		LDA,
	BOT- 2+ ,X	CMP,
	1 $		BNE,
	BOT- 1+ ,X	LDA,
	BOT- 3 + ,X	CMP,
	1 $		BNE,
	0 #.		AND,
	2 $		BEQ,
1 $:	FF #.		LDA,
2 $:			INX, INX,
	BOT- ,X		STA,
	BOT- 1+ ,X	STA,
			Next,
end-code
	
code cell+
			CLC,
	BOT- ,X 	LDA,
	2 #.		ADC,
	BOT- ,X		STA,
	1 $		BCC,
	BOT- 1+ ,X	INC,
1 $:			Next,
end-code

code char+
         BOT-     ,X INC,       \ N1 + 1 = N2
1 $                  BNE,
         BOT- 1+  ,X INC,
1 $:                 Next,
end-code

code d+
			CLC,
	BOT- 2 + ,X	LDA,
	BOT- 6 + ,X	ADC,
	BOT- 6 + ,X	STA,
	BOT- 3 + ,X	LDA,
	BOT- 7 + ,X	ADC,
	BOT- 7 + ,X	STA,
	BOT-     ,X	LDA,
	BOT- 4 + ,X	ADC,
	BOT- 4 + ,X	STA,
	BOT- 1 + ,X	LDA,
	BOT- 5 + ,X	ADC,
	BOT- 5 + ,X	STA,
			INX, INX, INX, INX,
			Next,
end-code

code d2*+ ( ud n -- ud+n c )
	BOT- 4 + ,X	ASL,
	BOT- 5 + ,X	ROL,
	BOT- 2 + ,X	ROL,
	BOT- 3 + ,X 	ROL,
	0 #.		LDA,
	A.		ROR,
			PHA,
	BOT- ,X		LDA,
	BOT- 4 + ,X	ADC,
	BOT- 4 + ,X	STA,
	BOT- 1+ ,X	LDA,
	BOT- 5 + ,X	ADC,
	BOT- 5 + ,X	STA,
	1 $		BCC,
	BOT- 2 + ,X	INC,
1 $:			PLA,
	BOT- 1+ ,X	STA,
	A.		ASL,
	BOT- ,X		STA,
			Next,
end-code

code um/mod
       	BOT- 4 + ,X 	LDA,       \ UD1 / UN1 = UN3
      	BOT- 2+  ,X 	LDY,       \       UN2 = Remainder
       	BOT- 4 + ,X 	STY,
                A. 	ASL,
       	BOT- 2+  ,X 	STA,
       	BOT- 5 + ,X 	LDA,
       	BOT- 3 + ,X 	LDY,
       	BOT- 5 + ,X 	STY,
                A. 	ROL,
       	BOT- 3 + ,X 	STA,
       	10 #.	   	LDY,
       	N	   	STY,
2 $:   	0 #.	   	LDY,
       	N 1+	   	STY,
       	BOT- 4 + ,X 	ROL,
       	BOT- 5 + ,X 	ROL,
       	N 1+        	ROL,
                   	SEC,
       	BOT- 4 + ,X 	LDA,
       	BOT-     ,X 	SBC,
                   	TAY,
       	BOT- 5 + ,X 	LDA,
       	BOT- 1+  ,X 	SBC,
       	N 2+        	STA,
       	N 1+        	LDA,
       	00       #. 	SBC,
	1 $        	BCC,
       	BOT- 4 + ,X 	STY,
       	N 2+        	LDA,
       	BOT- 5 + ,X 	STA,
1 $:   	BOT- 2+  ,X 	ROL,
       	BOT- 3 + ,X 	ROL,
                 N 	DEC,
	2 $        	BNE,
         INX, INX, Next,

end-code

code UM*                      \ ( UN1 UN2  --- UD1 )
             00 #. LDY,       \ UN1 * UN2 = UD1
        BOT- 2+ ,X LDA,
                 N STA,
        BOT- 2+ ,X STY,
       BOT- 3 + ,X LDA,
              N 1+ STA,
       BOT- 3 + ,X STY,
             10 #. LDY,
2 $:    BOT- 2+ ,X ASL,
       BOT- 3 + ,X ROL,
           BOT- ,X ROL,
        BOT- 1+ ,X ROL,
1 $                BCC,
                   CLC,
                 N LDA,
        BOT- 2+ ,X ADC,
        BOT- 2+ ,X STA,
              N 1+ LDA,
       BOT- 3 + ,X ADC,
       BOT- 3 + ,X STA,
1 $                BCC,
           BOT- ,X INC,
1 $                BNE,
        BOT- 1+ ,X INC,
1 $:               DEY,
2 $                BNE,
                   Next,
end-code


code (name) ( -- adr len )
	\ skip white spaces first
	>IN		LDY,
			DEY,
2 $:			INY,
	#TIB		CPY,
	1 $		BEQ,
	>TIB )Y		LDA,
	21 #.		CMP,
	2 $		BCC, 	\ smaller than $21 ->
	\ now look for a word
1 $:	>IN		STY,
			DEY,
3 $:			INY,
	#TIB		CPY,
	4 $		BEQ,
	>TIB )Y		LDA,
	21 #.		CMP,
	3 $		BCS,
4 $: 	DEX, DEX, DEX, DEX,
	>IN		LDA,	\ >TIB+>IN = adr
			CLC,
	>TIB		ADC,
	BOT- 2 + ,X	STA,
	0 #.		LDA,	\ len highbyte=0
	BOT- 1+ ,X	STA,
	>TIB 1+		ADC,
	BOT- 3 + ,X	STA,
			TYA,    \ Y->IN = len
			SEC,
	>IN		SBC,
	BOT- ,X		STA,
	#TIB		CPY,	
	5 $		BEQ,	\ if not the end vvv
			INY,	\ return char after space
5 $:	>IN		STY,
			Next,
end-code
	
\ load high level primitives
UNDEF-WORDS	include kernel/prim.fs	ALL-WORDS
\ fill doers
include kernel/doers.fs
