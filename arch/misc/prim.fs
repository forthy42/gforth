\ MISC primitives

\ Copyright (C) 1998,2000,2003,2004,2006,2007,2008 Free Software Foundation, Inc.

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

0 [IF]
Ideas/Todo


[THEN]

UNLOCK
>ENVIRON
\ true SetValue PrimTrace

LOCK

UNLOCK
also assembler definitions

X has? PrimTrace [IF]
: dout PC+6 X , accu X ,
       *accu X , txd X ,
       PC+4 X , jmp X ,
       X , 0 X , ;
[ELSE]
: dout drop ;
[THEN]

LOCK

\ pie primitives

$20 allot

Label start	ahere 2 + , jmp ,
Label "IntoForth" 4711 ,

Label RP'       0 ,
Label SP'       0 ,
Label UP'       0 ,
Label IP'       0 ,

Label #0	0 ,
Label #1	1 ,
Label #2	2 ,
Label #4	4 ,
Label #FF	$FF ,
Label #$8000	$8000 ,
Label #-1	-1 ,
Label "Next"	1802 ,
Label "Next1"	1802 ,
Label ""Next""  "Next" ,
End-Label

\ The virtual machine registers an data (stacks) go
\ to a seperate memory region (hopefully ram)

\ UNLOCK
\ current-region vm-memory activate ( saved-region )
\ LOCK

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
                "Next" , jmp ,
Label data-stack     50 cells allot
Label data-stack-top 2 cells allot
Label return-stack     50 cells allot
Label return-stack-top 2 cells allot

End-Label

\ UNLOCK
\ ( saved-region ) activate
\ LOCK

\ Up to here it's self modified
Label IntoForth 
                \ Transfer VM registers initial values
                RP' , RP ,
                SP' , SP ,
                IP' , IP ,
                UP' , UP , \ useless since UP is initialized by gforth boot
                ""Next"" , dstx 1 + ,
                #0 , dstx 2 + ,

Label Next	#0 , add ,  \ clear carry
		IP , shr ,
		sym Next
		*accu , W ,
		#1 , add ,
		accu , add ,
		accu , IP ,
Label Next1	W , shr ,
		*accu , shr ,
		accu , jmp ,

Label "xmov"	srcx ,
End-Label

IntoForth "IntoForth" 2* !
Next "Next" 2* !
Next1 "Next1" 2* !

has? PrimTrace [IF]
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
End-Label
[THEN]


Code: :docol	
                ': dout
		RP , accu ,
		#1 , sub ,
		accu , RP ,
		IP , *accu ,
		W , accu ,
		#4 , add ,
		accu , IP ,
		"Next" , jmp ,
end-code

Code: :docon	
                '1 dout
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

Code: :dovar
                '2 dout
		W , accu ,
		#4 , add ,
		accu , t0 ,
		SP , accu ,
		#1 , sub ,
		accu , SP ,
		t0 , *accu ,
		"Next" , jmp ,
end-code

Code: :douser	
                '3 dout
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

Code: :dodefer	
                '4 dout
		#0 , add ,
		W , shr ,
		#2 , add ,
		*accu , W ,
		"Next1" , jmp ,
end-code

Code: :dofield
                '5 dout
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

Code: :dodoes
                '6 dout
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

Code: :doesjump
end-code

Code execute
                'E dout
		SP , accu ,
		*accu , W ,
		#1 , add ,
		accu , SP ,
		"Next1" , jmp ,
end-code

Code ;s
                '; dout
		RP , accu ,
		#1 , add ,
		accu , RP ,
		#1 , sub ,
		*accu , IP ,
		"Next" , jmp ,
end-code

Code !
                '! dout
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

Code @
                '@ dout
		#0 , add ,
		SP , accu ,
		*accu , shr ,
		*accu , t0 ,
		SP , accu ,
		t0 , *accu ,
		"Next" , jmp ,
end-code

Code ?branch
                '? dout
		#0 , add ,
		IP , shr ,
		accu , t0 ,
		#1 , add ,
		accu , add ,
		accu , IP ,
                SP , accu ,
                *accu , t1 ,
		#1 , add ,
		accu , SP ,
                t1 , accu ,
		pc+4 , jz ,
		"Next" , jmp ,
                '~ dout
		t0 , accu ,
		*accu , IP ,
		"Next" , jmp ,
end-code

Code branch
                'b dout
		#0 , add ,
		IP , shr ,
		*accu , IP ,
		"Next" , jmp ,
end-code

Code (loop)	
                'l dout
		#0 , add ,
		IP , shr ,
		accu , t0 ,
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
		*accu , IP ,
		"Next" , jmp ,
end-code
		
Code xor
                'x dout
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

Code or	
                'o dout	
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

Code and
                'a dout
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

Code +		
                '+ dout
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

Code -		
                '- dout
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

Code 2/		
                '/ dout
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

Code 0=		
                '0 dout
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

Code 0<>	
                '% dout
		SP , accu ,
		*accu , accu ,
		ZF , accu ,
		#1 , sub ,
		accu , t0 ,
		SP , accu ,
		t0 , *accu ,
		"Next" , jmp ,
end-code

Code =		
                '= dout
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

Code u<		
                '< dout
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

Code 1+		
                'p dout
		SP , accu ,
		*accu , accu ,
		#1 , add ,
		accu , t0 ,
		SP , accu ,
		t0 , *accu ,
		"Next" , jmp ,
end-code

Code cell+	
                'P dout
		SP , accu ,
		*accu , accu ,
		#2 , add ,
		accu , t0 ,
		SP , accu ,
		t0 , *accu ,
		"Next" , jmp ,
end-code

Code 8<<	
                '{ dout
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

Code 8>>	
                '{ dout
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

Code c@		
                'c dout
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
end-code

Code 2*		
                '* dout
		SP , accu ,
		*accu , accu ,
		accu , add ,
		accu , t0 ,
		SP , accu ,
		t0 , *accu ,
		"Next" , jmp ,
end-code

Code >r		
                'R dout
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

Code r>		
                'r dout
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

Code sp@	
                's dout
		SP , accu ,
		accu , add ,
		accu , t0 ,
		SP , accu ,
		#1 , sub ,
		accu , SP ,
		t0 , *accu ,
		"Next" , jmp ,
end-code

Code sp!	
                'S dout
		#0 , add ,
		SP , accu ,
		*accu , shr ,
		accu , SP ,
		"Next" , jmp ,
end-code

Code rp@	
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

Code drop
                'd dout	
		SP , accu ,
		#1 , add ,
		accu , SP ,
		"Next" , jmp ,
end-code

Code lit	
                '# dout
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

Code dup	
                'u dout
		SP , accu ,
		*accu , t0 ,
		#1 , sub ,
		accu , SP ,
		t0 , *accu ,
		"Next" , jmp ,
end-code

Code r@		
                'I dout
		RP , accu ,
		*accu , t0 ,
		SP , accu ,
		#1 , sub ,
		accu , SP ,
		t0 , *accu ,
		"Next" , jmp ,
end-code

Code over	
                'v dout
		SP , accu ,
		#1 , add ,
		*accu , t0 ,
		#2 , sub ,
		accu , SP ,
		t0 , *accu ,
		"Next" , jmp ,
end-code

Code swap	
                'w dout
		SP , accu ,
		*accu , t0 ,
		#1 , add ,
		*accu , t1 ,
		t0 , *accu ,
		#1 , sub ,
		t1 , *accu ,
		"Next" , jmp ,
end-code

Code d+		
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
End-Label
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

Label "d2*+"	>d2*+ ,
End-Label
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

Code (key)      
		SP , accu ,
		#1 , sub ,
		accu , SP ,
                rxd , *accu ,
		"Next" , jmp ,
end-code

Code (key?)      
                rx? , accu ,
		ZF , accu ,
		#1 , sub ,
		accu , t0 ,
		SP , accu ,
		#1 , sub ,
		accu , SP ,
		t0 , *accu ,
		"Next" , jmp ,
end-code

Code (emit)      
		SP , accu ,
                *accu , txd ,
		#1 , add ,
		accu , SP ,
		"Next" , jmp ,
end-code
		
UP 2* Constant UP

: up@ up @ ;
: up! up ! ;

\ include ./key.fs
include ./optcmove.fs

: finish-code ;
: compile-prim1 ;
: (bye) ;
: bye ;
: float+ 8 + ;
