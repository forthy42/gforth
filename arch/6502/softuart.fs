
require ./zero.fs

hex

$8000 CONSTANT RP
$8400 CONSTANT WP
0 CONSTANT TXD
0 CONSTANT RXD
1 CONSTANT LED

start-macros

: bit0 1 swap lshift $ff xor ;
: bit1 1 swap lshift ;

end-macros

label a-out
                      \ 9600 Baud / 2 MHz  8N1
                      \ Output one byte to serial line
                      \ Avoid IRQs and NMIs
                      \ Avoid crossing page-boundaries
                      \ destroys N , N 1+, Y
 	N   STA,
TXD  BIT0 #. LDA,
    WPSHD AND,
       WP STA,        \ 1
    24 #. LDY,        \ DELAY1  208 - 25 = 182
1 $:      DEY,
1 $       BNE,
    08 #. LDA,        \ 2
     N 1+ STA,        \ 3
2 $:    N LSR,        \ 5
3 $       BCS,        \ 2/3
TXD BIT0 #. LDA,        \ 2
    WPSHD AND,        \ 3
    1000  LDY,        \ 4
4 $:   WP STA,        \ 3/1
    23 #. LDY,        \ DELAY2  208 - 28 = 180
5 $:      DEY,
5 $       BNE,
    00 #. LDY,        \ 3
     N 1+ DEC,        \ 5
2 $       BNE,        \ 2/3
    03 #. LDY,        \ DELAY3  208 - 189 = 18
7 $:      DEY,
7 $       BNE,
TXD BIT1 #. LDA,        \ 2
   WPSHD  ORA,        \ 3
       WP STA,        \ 3/1
    24 #. LDY,        \ DELAY4  208 - 25 = 182
6 $:      DEY,
6 $       BNE,
     N    LDA,
          RTS,        \ 6
3 $:
TXD BIT1 #. LDA,       \ 2
     WPSHD ORA,       \ 3
4 $        BNE,       \ 3
end-label

label 	test-uart
0 $:	$65 #. LDA, 
	a-out JSR,
	$66 #. LDA,
	a-out JSR,
	$67 #. LDA,
	a-out JSR,
	CLC,
0 $	BCC,
end-label

label  a-in                   \ (   --- C )
                              \ 9600 Baud / 2 MHz   8N1
                              \ Wait and Input one Byte
                              \ destroys N, N 1+ ,Y
       08        #. LDA,
       N 1+         STA,
1 $:   RP           LDA,      \ 7
       RXD BIT1  #. AND,
1 $                 BNE,
       18 #.        LDY,      \ DELAY 312 -7-181-3 = 121
2 $:                DEY,
2 $                 BNE,
3 $:   24 #.        LDY,      \ DELAY 208 -23-3 = 182
4 $:                DEY,
4 $                 BNE,
       RP           LDA,      \ 3/1    23
       RXD BIT1  #. AND,      \ 2
5 $                 BEQ,      \ 2/3
       1000         LDY,      \ 4
                    SEC,      \ 2
6 $:   N            ROR,      \ 5
       N 1+         DEC,      \ 5
3 $                 BNE,      \ 2/3
       04        #. LDY,      \ DELAY 104-23-9 = 71
7 $:                DEY,
7 $                 BNE,
       0         #. LDA,
       N            LDA,
                    RTS,

5 $:                CLC,      \ 3
6 $                 BCC,      \ 6
end-label

Code (emit)	\
	'# dout
	BOT- ,X 	LDA,
			INX, INX,
	a-out		JSR,
			Next,
end-code

Code (key)
	'K dout
	a-in		JSR,
			DEX, DEX,
	BOT- ,X		STA,
	0 #.		LDA,
	BOT- 1+ ,X	STA,
			Next,
end-code

: (key?) -1 ;
	
