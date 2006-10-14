get-current
wordlist >order definitions
wordlist constant ppc-asm-int
wordlist constant ppc-asm

: startasm ( -- ) ppc-asm-int >order ;
: endasm ( -- ) previous ;


: regand ( n -- n ) 31 and ;
: reg0 ( n -- n )  regand 6 lshift ;
: reg1 ( n -- n )  regand 11 lshift ;
: reg2 ( n -- n )  regand 16 lshift ;
: reg3 ( n -- n )  regand 21 lshift ;

: crand ( n -- n )  7 and 2 lshift ;
: cr1 ( n -- n )  crand 11 lshift ;
: cr2 ( n -- n )  crand 16 lshift ;
: cr3 ( n -- n )  crand 21 lshift ;

: imm16 ( n -- n )  $FFFF and ;
: imm24 ( n -- n )  $FFFFFF and ;

: crfid ( n -- n )  1 and swap 7 and 2 lshift or ; 

: defineop ( n xt name -- )  >r >r    : POSTPONE startasm r>  POSTPONE literal r>  POSTPONE literal POSTPONE ;  ;

: oprrnE ( n n n n -- n ) reg1 swap reg2 or swap reg3 or or ;
: oprrn ( n n name -- ) swap 26 lshift or ['] oprrnE  defineop  ;

: opswrrnE  ( n n n n -- n )  reg1 swap reg3 or swap reg2 or or ;
: opswrrn ( n n name -- ) swap 26 lshift or ['] opswrrnE  defineop  ;


: oprrrE  ( n n n n -- n )  reg1 swap reg2 or swap reg3 or or ;
: oprrr ( n n name -- ) swap 26 lshift or ['] oprrrE  defineop  ;

: opfrrr ( n n name -- )  oprrr ;

: opcrcrcr ( n n name -- ) swap 26 lshift or ['] oprrrE  defineop  ;

: opswrrrE  ( n n n n -- n )  reg1 swap reg3 or swap reg2 or or ;
: opswrrr ( n n name -- ) swap 26 lshift or  ['] opswrrrE  defineop  ;



: oprr0E  ( n n n -- n )  reg2 swap reg3 or or ;

: oprr0 ( n n name -- ) swap 26 lshift or  ['] oprr0E  defineop  ;
: opr0rE  ( n n n -- n )  reg1 swap reg3 or or ;

: opr0r ( n n name -- ) swap 26 lshift or  ['] opr0rE  defineop  ;

: oprn0 ( n n name -- ) oprr0 ;

: opri0E  ( n n n -- n )  2047 and 11 lshift swap reg3 or or ;
: opri0 ( n n name -- ) swap 26 lshift or  ['] opri0E  defineop  ;

: opswri0E  ( n n n -- n )  swap 2047 and 11 lshift swap reg3 or or ;
: opswri0 ( n n name -- ) swap 26 lshift or  ['] opswri0E  defineop  ;


: opifrE  ( n n n -- n )  reg1 swap 255 and 17 lshift or or ;
: opifr ( n n name -- ) swap 26 lshift or  ['] opifrE  defineop  ;

: opcrf0iE  ( n n n -- n )  15 and 12 lshift swap 7 and 23 lshift  or or ;
: opcrf0i  ( n n name -- ) swap 26 lshift or ['] opcrf0iE  defineop  ;


: oprrrnnE  ( n n n n n n -- n ) 31 and 1 lshift swap 31 and 6 lshift or swap reg1 or swap reg3 or swap reg2 or or ;
: oprrrnn ( n n name -- ) swap 26 lshift or ['] oprrrnnE defineop ;
: oprrnnn ( n n name -- ) swap 26 lshift or ['] oprrrnnE defineop ;

: opcrmE  ( n n n -- n )  255 and 12 lshift swap reg3 or or ;

: opcrm ( n n name -- ) swap 26 lshift or  ['] opcrmE  defineop  ;

: opswrr0E  ( n n n -- n )  reg3 swap reg2 or or ;

: opswrr0 ( n n name -- ) swap 26 lshift or  ['] opswrr0E  defineop  ;

: opswrn0 ( n n name -- ) opswrr0 ;

: op0rrE  ( n n n -- n )  reg1 swap reg2 or or ;

: op0rr ( n n name -- ) swap 26 lshift or ['] op0rrE  defineop  ;

: op00rE  ( n n -- n )  reg1  or ;
: op00r ( n n name -- ) swap 26 lshift or ['] op00rE  defineop  ;

: opr00E  ( n n -- n )  reg3  or ;
: opr00 ( n n name -- ) swap 26 lshift or ['] opr00E  defineop  ;

: opcrb00 ( n n name -- ) opr00 ;

: opfr00 ( n n name -- ) opr00 ;

: opcr00E  ( n n -- n )  cr3  or ;
: opcr00 ( n n name -- ) swap 26 lshift or ['] opcr00E  defineop  ;

: oprriE ( n n n n -- n )  imm16 swap reg2 or swap reg3 or or ;
: oprri ( n name -- ) 26 lshift  ['] oprriE defineop ;

: opnri ( n name -- ) oprri ;
: opnrr ( n n name -- ) oprrr ;

: oparriE  ( n n n n -- n ) swap imm16 swap reg2 or swap reg3 or or ;
: oparri ( n name -- ) 26 lshift  ['] oparriE defineop ;

: opafrri ( n name -- ) oparri ;

: opswrriE  ( n n n n -- n ) imm16 swap reg3 or swap reg2 or or ;
: opswrri ( n name -- ) 26 lshift ['] opswrriE defineop ;


: opbrE  ( n n n n -- n ) imm24 2 lshift  or ;
: opbr ( n n name -- ) swap 26 lshift or  ['] opbrE defineop ; 

: opbcE  ( n n n n -- n ) $3FFF and 2 lshift swap reg2 or swap reg3 or or ;
: opbc ( n n name -- ) swap 26 lshift or ['] opbcE defineop ;

: opbiiE  ( n n n -- n ) reg2 swap reg3 or or ;
: opbii ( n n name -- ) swap 26 lshift or ['] opbiiE defineop ;

: opcrfrrE  ( n n n n n -- n ) reg1 swap reg2 or rot rot crfid 21 lshift or or ;
: opcrfrr ( n n name -- ) swap 26 lshift or ['] opcrfrrE defineop ;

: opcrfriE  ( n n n n n -- n ) imm16 swap reg2 or rot rot crfid 21 lshift or or ;
: opcrfri ( n name -- ) 26 lshift  ['] opcrfriE defineop ;


: op000 ( n n name -- ) swap 26 lshift or ['] noop swap ( <-because no parameter ) defineop ;

: opc000 ( n n name -- ) op000 ;

: opsc ( n name -- ) 26 lshift 2 or ['] noop swap ( <-because no parameter ) defineop ;

: opcrcr0E  ( n n n -- n ) cr2 swap cr3 or or ;
: opcrcr0 ( n n name -- ) swap 26 lshift or ['] opcrcr0E  defineop ;

: opfcrcr0 ( n n name -- ) opcrcr0 ;


: opsfrfrfrE  ( n n n n -- n )  reg1 swap reg2 or swap reg3 or or ;
: opsfrfrfr ( n n name -- )  swap 26 lshift or ['] opsfrfrfrE  defineop  ;
: opfrfrfrE  ( n n n n -- n )  reg1 swap reg2 or swap reg3 or or ;
: opfrfrfr ( n n name -- ) swap 26 lshift or ['] opfrfrfrE  defineop  ;
: opfr0frE  ( n n n -- n )  reg1  swap reg3 or or ;
: opfr0fr ( n n name -- ) swap 26 lshift or ['] opfr0frE  defineop  ;

: opsfr0frE  ( n n n -- n )  reg1  swap reg3 or or ;
: opsfr0fr ( n n name -- ) swap 26 lshift or ['] opsfr0frE  defineop  ;

: opcrffrfrE  ( n n n n -- n )  reg1 swap reg2 or swap 7 and 23 lshift or or ;
: opcrffrfr ( n n name -- ) swap 26 lshift or ['] opcrffrfrE  defineop  ;

: opfrfrfrfrE  ( n n n n n -- n )  reg1 swap reg0 or swap reg2 or swap reg3 or or ;
: opfrfrfrfr ( n n name -- ) swap 26 lshift or ['] opfrfrfrfrE  defineop  ;
: opsfrfrfrfrE  ( n n n n n -- n )  reg1 swap reg0 or swap reg2 or swap reg3 or or ;
: opsfrfrfrfr ( n n name -- ) swap 26 lshift or ['] opsfrfrfrfrE  defineop  ;

: opfrfr0frE  ( n n n n -- n )   reg0  swap reg2 or swap reg3 or or ;
: opfrfr0fr ( n n name -- ) swap 26 lshift or ['] opfrfr0frE  defineop  ;
: opsfrfr0frE  ( n n n n -- n )  reg0 swap reg2 or swap reg3 or or ;
: opsfrfr0fr ( n n name -- ) swap 26 lshift or ['] opsfrfr0frE  defineop  ;


1 9 lshift constant OE
0 constant ~OE

: RC 1 lshift 1 + ;
: ~RC 1 lshift ;

ppc-asm-int >order definitions

0 constant r0
1 constant r1
2 constant r2
3 constant r3
4 constant r4
5 constant r5
6 constant r6
7 constant r7
8 constant r8
9 constant r9
10 constant r10
11 constant r11
12 constant r12
13 constant r13
14 constant r14
15 constant r15
16 constant r16
17 constant r17
18 constant r18
19 constant r19
20 constant r20
21 constant r21
22 constant r22
23 constant r23
24 constant r24
25 constant r25
26 constant r26
27 constant r27
28 constant r28
29 constant r29
30 constant r30
31 constant r31

0 constant fr0
1 constant fr1
2 constant fr2
3 constant fr3
4 constant fr4
5 constant fr5
6 constant fr6
7 constant fr7
8 constant fr8
9 constant fr9
10 constant fr10
11 constant fr11
12 constant fr12
13 constant fr13
14 constant fr14
15 constant fr15
16 constant fr16
17 constant fr17
18 constant fr18
19 constant fr19
20 constant fr20
21 constant fr21
22 constant fr22
23 constant fr23
24 constant fr24
25 constant fr25
26 constant fr26
27 constant fr27
28 constant fr28
29 constant fr29
30 constant fr30
31 constant fr31

0 constant crb0
1 constant crb1
2 constant crb2
3 constant crb3
4 constant crb4
5 constant crb5
6 constant crb6
7 constant crb7
8 constant crb8
9 constant crb9
10 constant crb10
11 constant crb11
12 constant crb12
13 constant crb13
14 constant crb14
15 constant crb15
16 constant crb16
17 constant crb17
18 constant crb18
19 constant crb19
20 constant crb20
21 constant crb21
22 constant crb22
23 constant crb23
24 constant crb24
25 constant crb25
26 constant crb26
27 constant crb27
28 constant crb28
29 constant crb29
30 constant crb30
31 constant crb31

0 constant crf0
1 constant crf1
2 constant crf2
3 constant crf3
4 constant crf4
5 constant crf5
6 constant crf6
7 constant crf7


: ;, ( n* xt n -- n ) swap execute endasm , ; 
: , ( xt n -- n xt ) swap ;

: ( ( xt n -- n xt ) swap ;
: ) \ ( -- )
	;

: ; \ ( n* xt n -- n )
	swap execute endasm ; 



previous 
ppc-asm >order definitions

include ops.fs

set-current
: ppc-asm-start ( -- ) ppc-asm >order ;
previous
previous
