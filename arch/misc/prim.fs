\ pie primitives

$20 allot

Label start	ahere $28 + , jmp ,

Label #0	0 ,
Label #1	1 ,
Label #2	2 ,
Label #4	4 ,
Label #FF	$FF ,
Label #$8000	$8000 ,
Label #-1	-1 ,
Label RP	0 ,
Label SP	0 ,
Label UP	0 ,
Label IP	0 ,
Label W		0 ,
Label Next	#0 , add ,
		IP , shr ,
		sym Next
		accu , 0 m ,
		0 r , W ,
		#1 , add ,
		accu , add ,
		accu , IP ,
Label Next1	#0 , add ,
		W , shr ,
		accu , 0 m ,
		#0 , add ,
		0 r , shr ,
		accu , jmp ,
Label "Next"	Next ,
Label "Next1"	Next1 ,

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
		accu , 0 m ,
		IP , 0 r ,
		W , accu ,
		#4 , add ,
		accu , IP ,
		"Next" , jmp ,
end-code

Code: :docon	sym docon
\		"1" , tx ,
		SP , accu ,
		#1 , sub ,
		accu , SP ,
		accu , 0 m ,
		#0 , add ,
		W , shr ,
		#2 , add ,
		accu , 1 m ,
		1 r , 0 r ,
		"Next" , jmp ,
end-code

Code: :dovar	sym dovar
\		"2" , tx ,
		SP , accu ,
		#1 , sub ,
		accu , SP ,
		accu , 0 m ,
		W , accu ,
		#4 , add ,
		accu , 0 r ,
		"Next" , jmp ,
end-code

Code: :douser	sym douser
\		"3" , tx ,
		SP , accu ,
		#1 , sub ,
		accu , SP ,
		accu , 0 m ,
		#0 , add ,
		W , shr ,
		#2 , add ,
		accu , 1 m ,
		1 r , accu ,
		UP , add ,
		accu , 0 r ,
		"Next" , jmp ,
end-code

Code: :dodefer	sym dodefer
\		"4" , tx ,
		#0 , add ,
		W , shr ,
		#2 , add ,
		accu , 0 m ,
		0 r , W ,
		"Next1" , jmp ,
end-code

Code: :dofield	sym dofield
\		"5" , tx ,
		#0 , add ,
		W , shr ,
		#2 , add ,
		accu , 0 m ,
		0 r , accu ,
		SP , 0 m ,
		0 r , add ,
		SP , 0 m ,
		accu , 0 r ,
		"Next" , jmp ,
end-code

Code: :dodoes	sym dodoes
\		"6" , tx ,
		RP , accu ,
		#1 , sub ,
		accu , RP ,
		accu , 0 m ,
		ip , 0 r ,
		SP , accu ,
		#1 , sub ,
		accu , SP ,
		accu , 0 m ,
		W , accu ,
		#4 , add ,
		accu , 0 r ,
		#2 , sub ,
		#0 , add ,
		accu , shr ,
		accu , 0 m ,
		0 r , IP ,
		"Next" , jmp ,
end-code

Code !		sym !
\		"A" , tx ,
		SP , accu ,
		accu , 0 m ,
		#1 , add ,
		accu , 1 m ,
		#1 , add ,
		accu , SP ,
		#0 , add ,
		0 r , shr ,
		accu , 2 m ,
		1 r , 2 r ,
		"Next" , jmp ,
end-code

Code @		sym @
\		"B" , tx ,
		#0 , add ,
		SP , 0 m ,
		SP , 1 m ,
		1 r , shr ,
		accu , 2 m ,
		2 r , 0 r ,
		"Next" , jmp ,
end-code

Code x!		sym x!
\		"C" , tx ,
		SP , accu ,
		accu , 0 m ,
		#1 , add ,
		accu , 1 m ,
		#1 , add ,
		accu , SP ,
		0 r , 2 m ,
		1 r , 2 r ,
		"Next" , jmp ,
end-code
		
Code x@		sym x@
\		"D" , tx ,
		SP , 0 m ,
		SP , 1 m ,
		1 r , 2 m ,
		2 r , 0 r ,
		"Next" , jmp ,
end-code

Code execute	sym execute
\		"E" , tx ,
		SP , accu ,
		accu , 0 m ,
		0 r , W ,
		#1 , add ,
		accu , SP ,
		"Next1" , jmp ,
end-code

Code ;s		sym ;s
\		"F" , tx ,
		RP , accu ,
		accu , 0 m ,
		#1 , add ,
		accu , RP ,
		0 r , IP ,
		"Next" , jmp ,
end-code

Code ?branch	sym ?branch
\		"?" , tx ,
		#0 , add ,
		IP , shr ,
		accu , 0 m ,
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
		0 r , accu ,
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
		accu , 0 m ,
		0 r , accu ,
		IP , add ,
		accu , IP ,
		"Next" , jmp ,
end-code

Code (loop)	sym (loop)
		#0 , add ,
		IP , shr ,
		accu , 0 m ,
		#1 , add ,
		accu , add ,
		accu , IP ,
		RP , accu ,
		accu , 1 m ,
		accu , 2 m ,
		#1 , add ,
		accu , 3 m ,
		2 r , accu ,
		#1 , add ,
		accu , 1 r ,
		3 r , sub ,
		"Next" , jz ,
		0 r , accu ,
		"branch" , jmp ,
end-code
		
Code xor	sym xor
\		"H" , tx ,
		SP , accu ,
		accu , 0 m ,
		#1 , add ,
		accu , SP ,
		accu , 1 m ,
		accu , 2 m ,
		0 r , accu ,
		1 r , xor ,
		accu , 2 r ,
		"Next" , jmp ,
end-code

Code or		sym or
\		"I" , tx ,
		SP , accu ,
		accu , 0 m ,
		#1 , add ,
		accu , SP ,
		accu , 1 m ,
		accu , 2 m ,
		0 r , accu ,
		1 r , or ,
		accu , 2 r ,
		"Next" , jmp ,
end-code

Code and	sym and
\		"J" , tx ,
		SP , accu ,
		accu , 0 m ,
		#1 , add ,
		accu , SP ,
		accu , 1 m ,
		accu , 2 m ,
		0 r , accu ,
		1 r , and ,
		accu , 2 r ,
		"Next" , jmp ,
end-code

Code +		sym +
\		"K" , tx ,
		SP , accu ,
		accu , 0 m ,
		#1 , add ,
		accu , SP ,
		accu , 1 m ,
		accu , 2 m ,
		0 r , accu ,
		1 r , add ,
		accu , 2 r ,
		"Next" , jmp ,
end-code

Code -		sym -
\		"L" , tx ,
		SP , accu ,
		accu , 0 m ,
		#1 , add ,
		accu , SP ,
		accu , 1 m ,
		accu , 2 m ,
		0 r , accu ,
		1 r , subr ,
		accu , 2 r ,
		"Next" , jmp ,
end-code

Code 2/		sym 2\/
\		"M" , tx ,
		#0 , add ,
		SP , 0 m ,
		SP , 1 m ,
		0 r , accu ,
		PC+6 , js ,
		accu , shr ,
		PC+6 , jmp ,
		accu , shr ,
		#$8000 , or ,
		accu , 1 r ,
		"Next" , jmp ,
end-code

Code 0=		sym 0=
		SP , 0 m ,
		SP , 1 m ,
		0 r , accu ,
		ZF , accu ,
		#0 , subr ,
		accu , 1 r ,
		"Next" , jmp ,
end-code

Code 0<>	sym 0<>
		SP , 0 m ,
		SP , 1 m ,
		0 r , accu ,
		ZF , accu ,
		#0 , subr ,
		#-1 , xor ,
		accu , 1 r ,
		"Next" , jmp ,
end-code

Code =		sym =
		SP , accu ,
		accu , 0 m ,
		#1 , add ,
		accu , SP ,
		accu , 1 m ,
		accu , 2 m ,
		0 r , accu ,
		1 r , sub ,
		ZF , accu ,
		#0 , subr ,
		accu , 2 r ,
		"Next" , jmp ,
end-code

Code u<		sym u<
		SP , accu ,
		accu , 0 m ,
		#1 , add ,
		accu , SP ,
		accu , 1 m ,
		accu , 2 m ,
		1 r , accu ,
		0 r , sub ,
		CF , accu ,
		#0 , subr ,
		accu , 2 r ,
		"Next" , jmp ,
end-code

Code 1+		sym 1+
		SP , 0 m ,
		SP , 1 m ,
		0 r , accu ,
		#1 , add ,
		accu , 1 r ,
		"Next" , jmp ,
end-code

Code cell+	sym cell+
		SP , 0 m ,
		SP , 1 m ,
		0 r , accu ,
		#2 , add ,
		accu , 1 r ,
		"Next" , jmp ,
end-code

Code 8<<	sym 8<<
\		"T" , tx ,
		#0 , add ,
		SP , accu ,
		accu , 0 m ,
		0 r , accu ,
		accu , add ,
		accu , add ,
		accu , add ,
		accu , add ,
		accu , add ,
		accu , add ,
		accu , add ,
		accu , add ,
		SP , 0 m ,
		accu , 0 r ,
		"Next" , jmp ,
end-code

Code 8>>	sym 8>>
\		"T" , tx ,
		#0 , add ,
		SP , accu ,
Label c-even@	accu , 0 m ,
		0 r , shr ,
		accu , shr ,
		accu , shr ,
		accu , shr ,
		accu , shr ,
		accu , shr ,
		accu , shr ,
		accu , shr ,
		#FF , and ,
		SP , 0 m ,
		accu , 0 r ,
		"Next" , jmp ,
Label "c-even@"	c-even@ ,
end-code

Code c@		sym c@
		#0 , add ,
		SP , 0 m ,
		0 r , shr ,
		PC+4 , jc ,
		"c-even@" , jmp ,
		accu , 0 m ,
		0 r , accu ,
		#FF , and ,
		SP , 0 m ,
		accu , 0 r ,
		"Next" , jmp ,

Code 2*		sym 2*
\		"N" , tx ,
		SP , 0 m ,
		SP , 1 m ,
		0 r , accu ,
		accu , add ,
		accu , 1 r ,
		"Next" , jmp ,
end-code

Code >r		sym >r
\		"O" , tx ,
		RP , accu ,
		#1 , sub ,
		accu , RP ,
		accu , 0 m ,
		SP , accu ,
		accu , 1 m ,
		#1 , add ,
		accu , SP ,
		1 r , 0 r ,
		"Next" , jmp ,
end-code

Code r>		sym r>
\		"P" , tx ,
		RP , accu ,
		accu , 0 m ,
		#1 , add ,
		accu , RP ,
		SP , accu ,
		#1 , sub ,
		accu , SP ,
		accu , 1 m ,
		0 r , 1 r ,
		"Next" , jmp ,
end-code

Code sp@	sym sp@
\		"Q" , tx ,
		SP , accu ,
		#1 , sub ,
		accu , 0 m ,
		accu , SP ,
		#1 , add ,
		accu , add ,
		accu , 0 r ,
		"Next" , jmp ,
end-code

Code sp!	sym sp!
		#0 , add ,
		SP , 0 m ,
		0 r , shr ,
		accu , SP ,
		"Next" , jmp ,
end-code

Code rp@	sym rp@
\		"R" , tx ,
		SP , accu ,
		#1 , sub ,
		accu , 0 m ,
		accu , SP ,
		RP , accu ,
		accu , add ,
		accu , 0 r ,
		"Next" , jmp ,
end-code

Code rp!	sym rp!
		SP , accu ,
		accu , 0 m ,
		#1 , add ,
		accu , SP ,
		#0 , add ,
		0 r , shr ,
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
		SP , accu ,
		#1 , sub ,
		accu , SP ,
		#0 , add ,
		accu , 0 m ,
		IP , shr ,
		accu , 1 m ,
		#1 , add ,
		accu , add ,
		accu , IP ,
		1 r , 0 r ,
		"Next" , jmp ,
end-code

Code dup	sym dup
		SP , accu ,
		accu , 0 m ,
		#1 , sub ,
		accu , SP ,
		accu , 1 m ,
		0 r , 1 r ,
		"Next" , jmp ,
end-code

Code I		sym r@
		SP , accu ,
		RP , 0 m ,
		#1 , sub ,
		accu , SP ,
		accu , 1 m ,
		0 r , 1 r ,
		"Next" , jmp ,
end-code

Code over	sym over
		SP , accu ,
		#1 , add ,
		accu , 0 m ,
		#2 , sub ,
		accu , 1 m ,
		accu , SP ,
		0 r , 1 r ,
		"Next" , jmp ,
end-code

Code swap	sym swap
		SP , accu ,
		accu , 0 m ,
		accu , 1 m ,
		#1 , add ,
		accu , 2 m ,
		accu , 3 m ,
		0 r , accu ,	\ TOS -> accu
		2 r , 1 r ,	\ NOS -> TOS
		accu , 3 r ,	\ accu -> NOS
		"Next" , jmp ,
end-code

Code d+		sym d+
		SP , accu ,
		accu , 0 m ,
		#1 , add ,
		accu , 1 m ,
		#1 , add ,
		accu , SP ,
		accu , 2 m ,
		accu , 3 m ,
		#1 , add ,
		accu , 4 m ,
		accu , 5 m ,
		1 r , accu ,
		4 r , add ,
		accu , 5 r ,
		CF , accu ,
		0 r , add ,
		2 r , add ,
		accu , 3 r ,
		"Next" , jmp ,
end-code

Label cf1	0 ,
Code d2*+	sym d2*+
Label >d2*+	SP , accu ,
		accu , 0 m ,
		accu , 1 m ,
		#1 , add ,
		accu , 2 m ,
		accu , 3 m ,
		accu , 4 m ,
		accu , 5 m ,
		#1 , add ,
		accu , 6 m ,
		accu , 7 m ,
		6 r , accu ,
		accu , add ,
		CF , cf1 ,
		0 r , add ,
		accu , 7 r ,
		5 r , accu ,
		#$8000 , and ,
		accu , 1 r ,
		CF , accu ,
		cf1 , add ,
		2 r , add ,
		3 r , add ,
		accu , 4 r ,
		"Next" , jmp ,
end-code

Label res1	0 ,
Label "d2*+"	>d2*+ ,
Code /modstep ( ud c R: u -- ud-?u 0/1 )
		sym /modstep
		SP , accu ,
		accu , 0 m ,
		accu , 1 m ,
		#1 , add ,
		accu , 2 m ,
		accu , 3 m ,
		RP , 4 m ,
		2 r , accu ,
		4 r , sub ,
		accu , res1 ,
		CF , accu ,
		0 r , or ,
		PC+6 , JZ ,
		#0 , accu ,
		PC+6 , jmp ,
		res1 , 3 r ,
		#1 , accu ,
		accu , 1 r ,
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
