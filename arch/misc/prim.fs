\ pie primitives

$20 allot

Label start	ahere $21 + , jmp ,

Label #0	0 ,
Label #1	1 ,
Label RP	0 ,
Label SP	0 ,
Label UP	0 ,
Label IP	0 ,
Label W		0 ,
Label t0	0 ,
Label t1	0 ,
Label t2	0 ,
Label t3	0 ,
Label srcx	0 ,
Label dstx	0 ,

\ Up to here it's self modified
\ If there's a gap here, add a jump to Next after dstx

Label Next	#0 , add ,
		IP , shr ,
		sym Next
		*accu , W ,
		#1 , add ,
		accu , add ,
		accu , IP ,
Label Next1	W , shr ,
		*accu , shr ,
		accu , jmp ,
Label "Next"	Next ,
Label "Next1"	Next1 ,
Label "xmov"	srcx ,

Label #2	2 ,
Label #4	4 ,
Label #FF	$FF ,
Label #$8000	$8000 ,
Label #-1	-1 ,

Label "0"	'< ,
Label "1"	'1 ,
Label "2"	'2 ,
Label "3"	'3 ,
Label "4"	'4 ,
Label "5"	'5 ,
Label "6"	'6 ,
Label "A"	'A ,
Label "B"	'B ,
Label "C"	'C ,
Label "D"	'D ,
Label "E"	'E ,
Label "F"	'> ,
Label "?"	'? ,
Label "+"	'+ ,
Label "/"	'/ ,
Label "H"	'H ,
Label "I"	'I ,
Label "J"	'J ,
Label "K"	'K ,
Label "L"	'L ,
Label "M"	'M ,
Label "N"	'N ,
Label "O"	'O ,
Label "P"	'P ,
Label "Q"	'Q ,
Label "R"	'R ,
Label "S"	'S ,
Label "T"	'T ,
Label "#"	'# ,

end-label

Code: :docol	sym docol
\		"0" , tx ,
		RP , accu ,
		#1 , sub ,
		accu , RP ,
		IP , *accu ,
		W , accu ,
		#4 , add ,
		accu , IP ,
		"Next" , jmp ,
end-code

Code: :docon	sym docon
\		"1" , tx ,
		#0 , add ,
		W , shr ,
		#2 , add ,
		*accu , t0 ,
		SP , accu ,
		#1 , sub ,
		accu , SP ,
		t0 , *accu ,
		"Next" , jmp ,
end-code

Code: :dovar	sym dovar
\		"2" , tx ,
		W , accu ,
		#4 , add ,
		accu , t0 ,
		SP , accu ,
		#1 , sub ,
		accu , SP ,
		t0 , *accu ,
		"Next" , jmp ,
end-code

Code: :douser	sym douser
\		"3" , tx ,
		#0 , add ,
		W , shr ,
		#2 , add ,
		*accu , accu ,
		UP , add ,
		accu , t0 ,
		SP , accu ,
		#1 , sub ,
		accu , SP ,
		t0 , *accu ,
		"Next" , jmp ,
end-code

Code: :dodefer	sym dodefer
\		"4" , tx ,
		#0 , add ,
		W , shr ,
		#2 , add ,
		*accu , W ,
		"Next1" , jmp ,
end-code

Code: :dofield	sym dofield
\		"5" , tx ,
		#0 , add ,
		W , shr ,
		#2 , add ,
		*accu , accu ,
		accu , t0 ,
		SP , accu ,
		*accu , accu ,
		t0 , add ,
		accu , t0 ,
		SP , accu ,
		t0 , *accu ,
		"Next" , jmp ,
end-code

Code: :dodoes	sym dodoes
\		"6" , tx ,
		RP , accu ,
		#1 , sub ,
		accu , RP ,
		IP , *accu ,
		W , accu ,
		#4 , add ,
		accu , t0 ,
		SP , accu ,
		#1 , sub ,
		accu , SP ,
		t0 , *accu ,
		t0 , accu ,
		#2 , sub ,
		#0 , add ,
		accu , shr ,
		*accu , IP ,
		"Next" , jmp ,
end-code

Code !		sym !
\		"A" , tx ,
		SP , accu ,
		*accu , t0 ,
		#1 , add ,
		*accu , t1 ,
		#1 , add ,
		accu , SP ,
		t0 , shr ,
		t1 , *accu ,
		"Next" , jmp ,
end-code

Code @		sym @
\		"B" , tx ,
		#0 , add ,
		SP , accu ,
		*accu , shr ,
		*accu , t0 ,
		SP , accu ,
		t0 , *accu ,
		"Next" , jmp ,
end-code

Code x!		sym x!
\		"C" , tx ,
		SP , accu ,
		*accu , dstx ,
		#1 , add ,
		accu , srcx ,
		#1 , add ,
		accu , SP ,
		"xmov" , jmp ,
end-code
		
Code x@		sym x@
\		"D" , tx ,
		SP , accu ,
		*accu , srcx ,
		accu , dstx ,
		"xmov" , jmp ,
end-code

Code execute	sym execute
\		"E" , tx ,
		SP , accu ,
		*accu , W ,
		#1 , add ,
		accu , SP ,
		"Next1" , jmp ,
end-code

Code ;s		sym ;s
\		"F" , tx ,
		RP , accu ,
		#1 , add ,
		accu , RP ,
		#1 , sub ,
		*accu , IP ,
		"Next" , jmp ,
end-code

Code ?branch	sym ?branch
\		"?" , tx ,
		#0 , add ,
		IP , shr ,
		accu , t0 ,
		#1 , add ,
		accu , add ,
		accu , IP ,
		SP , accu ,
		accu , 1 m ,
		#1 , add ,
		accu , SP ,
		1 r , accu ,
		pc+4 , jz ,
		sym no-branch
		"Next" , jmp ,
\		"+" , tx ,
		t0 , accu ,
		*accu , accu ,
Label >branch	sym branch-o
		IP , add ,
		#2 , sub ,
		sym branch-to
		accu , IP ,
		"Next" , jmp ,
Label "branch"	>branch ,
end-code

Code branch	sym branch
\		"/" , tx ,
		#0 , add ,
		IP , shr ,
		*accu , accu ,
		IP , add ,
		accu , IP ,
		"Next" , jmp ,
end-code

Code (loop)	sym (loop)
		#0 , add ,
		IP , shr ,
		*accu , t0 ,
		#1 , add ,
		accu , add ,
		accu , IP ,
		RP , accu ,
		*accu , t2 ,
		#1 , add ,
		*accu , t3 ,
		t2 , accu ,
		#1 , add ,
		accu , t1 ,
		RP , accu ,
		t1 , *accu ,
		t1 , accu ,
		t3 , sub ,
		"Next" , jz ,
		t0 , accu ,
		"branch" , jmp ,
end-code
		
Code xor	sym xor
\		"H" , tx ,
		SP , accu ,
		*accu , t0 ,
		#1 , add ,
		accu , SP ,
		*accu , accu ,
		t0 , xor ,
		accu , t0 ,
		SP , accu ,
		t0 , *accu ,
		"Next" , jmp ,
end-code

Code or		sym or
\		"I" , tx ,
		SP , accu ,
		*accu , t0 ,
		#1 , add ,
		accu , SP ,
		*accu , accu ,
		t0 , or ,
		accu , t0 ,
		SP , accu ,
		t0 , *accu ,
		"Next" , jmp ,
end-code

Code and	sym and
\		"J" , tx ,
		SP , accu ,
		*accu , t0 ,
		#1 , add ,
		accu , SP ,
		*accu , accu ,
		t0 , and ,
		accu , t0 ,
		SP , accu ,
		t0 , *accu ,
		"Next" , jmp ,
end-code

Code +		sym +
\		"K" , tx ,
		SP , accu ,
		*accu , t0 ,
		#1 , add ,
		accu , SP ,
		*accu , accu ,
		t0 , add ,
		accu , t0 ,
		SP , accu ,
		t0 , *accu ,
		"Next" , jmp ,
end-code

Code -		sym -
\		"L" , tx ,
		SP , accu ,
		*accu , t0 ,
		#1 , add ,
		accu , SP ,
		*accu , accu ,
		t0 , sub ,
		accu , t0 ,
		SP , accu ,
		t0 , *accu ,
		"Next" , jmp ,
end-code

Code 2/		sym 2\/
\		"M" , tx ,
		#0 , add ,
		SP , accu ,
		*accu , accu ,
		PC+6 , js ,
		accu , shr ,
		PC+6 , jmp ,
		accu , shr ,
		#$8000 , or ,
		accu , t0 ,
		SP , accu ,
		t0 , *accu ,
		"Next" , jmp ,
end-code

Code 0=		sym 0=
		SP , accu ,
		*accu , accu ,
		ZF , accu ,
		#1 , xor ,
		#1 , sub ,
		accu , t0 ,
		SP , accu ,
		t0 , *accu ,
		"Next" , jmp ,
end-code

Code 0<>	sym 0<>
		SP , accu ,
		*accu , accu ,
		ZF , accu ,
		#1 , sub ,
		accu , t0 ,
		SP , accu ,
		t0 , *accu ,
		"Next" , jmp ,
end-code

Code =		sym =
		SP , accu ,
		*accu , t0 ,
		#1 , add ,
		accu , SP ,
		*accu , accu ,
		t0 , sub ,
		ZF , accu ,
		#1 , xor ,
		#1 , sub ,
		accu , t0 ,
		SP , accu ,
		t0 , *accu ,
		"Next" , jmp ,
end-code

Code u<		sym u<
		SP , accu ,
		*accu , t0 ,
		#1 , add ,
		accu , SP ,
		*accu , accu ,
		t0 , sub ,
		CF , accu ,
		#1 , xor ,
		#1 , sub ,
		accu , t0 ,
		SP , accu ,
		t0 , *accu ,
		"Next" , jmp ,
end-code

Code 1+		sym 1+
		SP , accu ,
		*accu , accu ,
		#1 , add ,
		accu , t0 ,
		SP , accu ,
		t0 , *accu ,
		"Next" , jmp ,
end-code

Code cell+	sym cell+
		SP , accu ,
		*accu , accu ,
		#2 , add ,
		accu , t0 ,
		SP , accu ,
		t0 , *accu ,
		"Next" , jmp ,
end-code

Code 8<<	sym 8<<
\		"T" , tx ,
		#0 , add ,
		SP , accu ,
		*accu , accu ,
		accu , add ,
		accu , add ,
		accu , add ,
		accu , add ,
		accu , add ,
		accu , add ,
		accu , add ,
		accu , add ,
		accu , t0 ,
		SP , accu ,
		t0 , *accu ,
		"Next" , jmp ,
end-code

Code 8>>	sym 8>>
\		"T" , tx ,
		#0 , add ,
		SP , accu ,
Label c-even@	*accu , shr ,
		accu , shr ,
		accu , shr ,
		accu , shr ,
		accu , shr ,
		accu , shr ,
		accu , shr ,
		accu , shr ,
		#FF , and ,
		accu , t0 ,
		SP , accu ,
		t0 , *accu ,
		"Next" , jmp ,
Label "c-even@"	c-even@ ,
end-code

Code c@		sym c@
		#0 , add ,
		SP , accu ,
		*accu , shr ,
		PC+4 , jc ,
		"c-even@" , jmp ,
		*accu , accu ,
		#FF , and ,
		accu , t0 ,
		SP , accu ,
		t0 , *accu ,
		"Next" , jmp ,

Code 2*		sym 2*
\		"N" , tx ,
		SP , accu ,
		*accu , accu ,
		accu , add ,
		accu , t0 ,
		SP , accu ,
		t0 , *accu ,
		"Next" , jmp ,
end-code

Code >r		sym >r
\		"O" , tx ,
		SP , accu ,
		*accu , t0 ,
		#1 , add ,
		accu , SP ,
		RP , accu ,
		#1 , sub ,
		accu , RP ,
		t0 , *accu ,
		"Next" , jmp ,
end-code

Code r>		sym r>
\		"P" , tx ,
		RP , accu ,
		*accu , t0 ,
		#1 , add ,
		accu , RP ,
		SP , accu ,
		#1 , sub ,
		accu , SP ,
		t0 , *accu ,
		"Next" , jmp ,
end-code

Code sp@	sym sp@
\		"Q" , tx ,
		SP , accu ,
		accu , add ,
		accu , t0 ,
		SP , accu ,
		#1 , sub ,
		accu , SP ,
		t0 , *accu ,
		"Next" , jmp ,
end-code

Code sp!	sym sp!
		#0 , add ,
		SP , accu ,
		*accu , shr ,
		accu , SP ,
		"Next" , jmp ,
end-code

Code rp@	sym rp@
\		"R" , tx ,
		RP , accu ,
		accu , add ,
		accu , t0 ,
		SP , accu ,
		#1 , sub ,
		accu , SP ,
		t0 , *accu ,
		"Next" , jmp ,
end-code

Code rp!	sym rp!
		SP , accu ,
		*accu , t0 ,
		#1 , add ,
		accu , SP ,
		#0 , add ,
		t0 , shr ,
		accu , RP ,
		"Next" , jmp ,
end-code

Code drop	sym drop
\		"S" , tx ,
		SP , accu ,
		#1 , add ,
		accu , SP ,
		"Next" , jmp ,
end-code

Code lit	sym lit
\		"#" , tx ,
		IP , shr ,
		*accu , t0 ,
		#1 , add ,
		accu , add ,
		accu , IP ,
		SP , accu ,
		#1 , sub ,
		accu , SP ,
		t0 , *accu ,
		"Next" , jmp ,
end-code

Code dup	sym dup
		SP , accu ,
		*accu , t0 ,
		#1 , sub ,
		accu , SP ,
		t0 , *accu ,
		"Next" , jmp ,
end-code

Code I		sym r@
		RP , accu ,
		*accu , t0 ,
		SP , accu ,
		#1 , sub ,
		accu , SP ,
		t0 , *accu ,
		"Next" , jmp ,
end-code

Code over	sym over
		SP , accu ,
		#1 , add ,
		*accu , t0 ,
		#2 , sub ,
		accu , SP ,
		t0 , *accu ,
		"Next" , jmp ,
end-code

Code swap	sym swap
		SP , accu ,
		*accu , t0 ,
		#1 , add ,
		*accu , t1 ,
		t0 , *accu ,
		#1 , sub ,
		t1 , *accu ,
		"Next" , jmp ,
end-code

Code d+		sym d+
		SP , accu ,
		*accu , t0 ,
		#1 , add ,
		*accu , t1 ,
		#1 , add ,
		*accu , t2 ,
		accu , SP ,
		#1 , add ,
		*accu , accu ,
		t1 , add ,
		accu , t1 ,
		CF , accu ,
		t2 , add ,
		t0 , add ,
		accu , t0 ,
		SP , accu ,
		t0 , *accu ,
		#1 , add ,
		t1 , *accu ,
		"Next" , jmp ,
end-code

Label cf1	0 ,
Code d2*+	sym d2*+
		SP , accu ,
Label >d2*+	*accu , t0 ,
		#1 , add ,
		*accu , t1 ,
		#1 , add ,
		*accu , t2 ,
		accu , t3 ,
		t0 , accu ,
		t2 , add ,
		t2 , add ,
		accu , t2 ,
		CF , accu ,
		t1 , add ,
		t1 , add ,
		accu , t0 ,
		t1 , accu ,
		#$8000 , and ,
		accu , t1 ,
		t3 , accu ,
		t2 , *accu ,
		#1 , sub ,
		t0 , *accu ,
		#1 , sub ,
		t1 , *accu ,
		"Next" , jmp ,
end-code

Label res1	0 ,
Label "d2*+"	>d2*+ ,
Code /modstep ( ud c R: u -- ud-?u 0/1 )
		sym /modstep
		SP , accu ,
		*accu , t0 ,
		#1 , add ,
		*accu , t1 ,
		#1 , add ,
		*accu , t2 ,
		t2 , accu ,
		t0 , sub ,
		accu , t0 ,
		CF , accu ,
		t1 , or ,
		PC+6 , JZ ,
		#0 , accu ,
		PC+6 , jmp ,
		t0 , t2 ,
		#1 , accu ,
		accu , t0 ,
		SP , accu ,
		#1 , add ,
		t0 , *accu ,
		#1 , add ,
		t2 , *accu ,
		#1 , sub ,
		"d2*+" , jmp ,
end-code
		
RP 2* Constant RP
SP 2* Constant SP
UP 2* Constant UP
IP 2* Constant IP

c: sp! 2/ sp ! ;
c: sp@ sp @ 1+ 2* ;
c: rp@ rp @ 2* ;
c: rp! r> swap 2/ rp ! >r ;
c: up@ up @ ;
c: up! up ! ;

include ~+/key.fs
