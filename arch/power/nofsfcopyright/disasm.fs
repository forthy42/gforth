get-current
wordlist >order definitions

: unknownop ( n -- ) ." ( --Unknown Op-- ) " .  cr ;

: filltab ( a n --  ) cells over + swap  ?DO
 ['] unknownop i ! cell +LOOP ; 

: opfwtab ( xt a --  ) { xt start } 32 0 ?DO xt start i 6 lshift cells + !  LOOP ;


create disasmtab 64 cells allot
create oprrrtab 2048 cells allot
create opcrcrcrtab 2048 cells allot
create opfrfrfrtab 2048 cells allot
create opsfrfrfrtab 2048 cells allot
create brtab 4 cells allot
create bctab 4 cells allot

disasmtab 64 filltab
oprrrtab 2048 filltab
opcrcrcrtab 2048 filltab
opfrfrfrtab 2048 filltab
opsfrfrfrtab 2048 filltab
brtab 4 filltab
bctab 4 filltab

: oprrrdisp ( n -- ) dup 2047 and cells oprrrtab + @ execute ;
: opcrcrcrdisp ( n -- ) dup 2047 and cells opcrcrcrtab + @ execute ;
: opfrfrfrdisp ( n -- ) dup 2047 and cells opfrfrfrtab + @ execute ;
: opsfrfrfrdisp ( n -- ) dup 2047 and cells opsfrfrfrtab + @ execute ;
: brdisp ( n -- ) dup 3 and cells brtab + @ execute ;
: bcdisp ( n -- ) dup 3 and cells bctab + @ execute ;

' oprrrdisp disasmtab 31 cells + ! 
' opcrcrcrdisp disasmtab 19 cells + ! 
' opfrfrfrdisp disasmtab 63 cells + ! 
' opsfrfrfrdisp disasmtab 59 cells + ! 
' brdisp disasmtab 18 cells + ! 
' bcdisp disasmtab 16 cells + ! 

: sprint ( -- ) ."  " ;
: bprint ( -- ) ." , " ;
: eprint ( -- ) ." ;" cr ;
: imm16print ( n -- ) ." " $FFFF and . ;
: imm5print ( n -- ) ." " 31 and dec. ;
: imm14print  ( n -- ) ." " $3FFF and . ;
: rprint ( n -- ) 31 and ." r" dec. ;
: crfprint ( n -- ) 7 and ." crf" dec. ;
: frprint ( n -- ) 31 and ." fr" dec. ;
: crbprint ( n -- ) 31 and ." crb" dec. ;
: r1print ( n -- ) 11 rshift rprint ;
: r2print ( n -- ) 16 rshift rprint ;
: r3print ( n -- ) 21 rshift rprint ;
: fr0print ( n -- ) 6 rshift frprint ;
: fr1print ( n -- ) 11 rshift frprint ;
: fr2print ( n -- ) 16 rshift frprint ;
: fr3print ( n -- ) 21 rshift frprint ;
: crb1print ( n -- ) 11 rshift crbprint ;
: crb2print ( n -- ) 16 rshift crbprint ;
: crb3print ( n -- ) 21 rshift crbprint ;
: cr1print ( n -- ) 13 rshift crfprint ;
: cr2print ( n -- ) 18 rshift crfprint ;
: cr3print ( n -- ) 23 rshift crfprint ;

: crfidprint ( n -- ) dup 2 rshift crfprint bprint 1 and . ;

: defineop ( name xt -- xt ) >r :noname name POSTPONE SLITERAL POSTPONE type r> compile, POSTPONE ; ;

: op000print  ( n -- ) sprint drop eprint ;
: op000 ( name n n -- ) ['] op000print defineop swap cells oprrrtab + ! drop ;

: opcrfrrprint  ( n -- ) sprint dup 21 rshift crfidprint bprint dup r2print bprint r1print eprint ;
: opcrfrr ( name n n -- ) ['] opcrfrrprint defineop swap cells oprrrtab + ! drop ;

: opcrfriprint ( n -- ) sprint dup 21 rshift crfidprint bprint dup r2print bprint imm16print eprint ;
: opcrfri ( name n -- ) ['] opcrfriprint defineop swap cells disasmtab + ! ;

: opbrprint ( n -- ) sprint 2 rshift $FFFFFF and . eprint ;
: opbr ( name n n -- ) ['] opbrprint defineop  swap cells brtab + ! drop ;

: opbcprint ( n -- ) sprint dup 21 rshift imm5print bprint dup 16 rshift imm5print bprint 2 rshift imm14print eprint ;
: opbc ( name n n -- ) ['] opbcprint defineop swap cells bctab + ! drop ; 

: oprrrprint ( n -- ) sprint  dup r3print bprint dup r2print bprint  r1print eprint ;
: oprrr ( name n n -- ) ['] oprrrprint defineop  swap cells oprrrtab +  ! drop ;

: opnrrprint ( n -- ) sprint  dup 21 rshift 31 and . bprint dup r2print bprint  r1print eprint ;
: opnrr ( name n n -- ) ['] opnrrprint defineop  swap cells oprrrtab +  ! drop ;


: oprrnprint ( n -- ) sprint  dup r3print bprint dup r2print bprint 11 rshift 31 and . eprint ;
: oprrn ( name n n -- ) ['] oprrnprint defineop  swap cells oprrrtab +  ! drop ;

: opswrrnprint ( n -- ) sprint  dup r2print bprint dup r3print bprint 11 rshift 31 and . eprint ;
: opswrrn ( name n n -- ) ['] opswrrnprint defineop  swap cells oprrrtab +  ! drop ;


: oprrrnnprint ( n -- ) dup 1 and if ." . " else ."  " then dup r2print bprint dup r3print bprint dup r1print bprint dup 6 rshift 31 and . bprint 1 rshift 31 and . eprint ; 
: oprrnnnprint ( n -- ) dup 1 and if ." . " else ."  " then dup r2print bprint dup r3print bprint dup 11 rshift 31 and . bprint dup 6 rshift 31 and . bprint 1 rshift 31 and . eprint ; 

: oprrrnn ( name n -- ) drop ['] oprrrnnprint defineop swap cells disasmtab + ! ;
: oprrnnn ( name n -- ) drop ['] oprrnnnprint defineop swap cells disasmtab + ! ;

: opfr0frprint ( n -- ) sprint  dup fr3print bprint   fr1print eprint ;
: opfr0fr ( name n n -- ) ['] opfr0frprint defineop  swap cells opfrfrfrtab +  ! drop ;

: opsfr0frprint ( n -- ) sprint  dup fr3print bprint   fr1print eprint ;
: opsfr0fr ( name n n -- ) ['] opsfr0frprint defineop  swap cells opsfrfrfrtab +  ! drop ;

: opfrfrfrprint ( n -- ) sprint  dup fr3print bprint dup fr2print bprint fr1print eprint ;
: opfrfrfr ( name n n -- ) ['] opfrfrfrprint defineop  swap cells opfrfrfrtab +  ! drop ;
: opsfrfrfrprint ( n -- ) sprint  dup fr3print bprint dup fr2print bprint fr1print eprint ;
: opsfrfrfr ( name n n -- ) ['] opsfrfrfrprint defineop  swap cells opsfrfrfrtab +  ! drop ;

: opcrffrfrprint ( n -- ) sprint  dup 23 rshift crfprint bprint dup fr2print bprint fr1print eprint ;
: opcrffrfr ( name n n -- ) ['] opcrffrfrprint defineop  swap cells opfrfrfrtab +  ! drop ;

: opfrrrprint ( n -- ) sprint  dup fr3print bprint dup r2print bprint r1print eprint ;
: opfrrr ( name n n -- ) ['] opfrrrprint defineop swap cells oprrrtab +  ! drop ;


: opswrrrprint ( n -- ) sprint  dup r2print bprint dup r1print bprint  r1print eprint ;
: opswrrr ( name n n -- ) [']  opswrrrprint defineop  swap cells oprrrtab +  ! drop ;


: opcrcrcrprint ( n -- ) sprint  dup crb3print bprint dup crb2print bprint  crb1print eprint ;
: opcrcrcr ( name n n -- ) ['] opcrcrcrprint defineop swap cells opcrcrcrtab +  ! drop ;

: opcrcr0print ( n -- ) sprint dup cr3print bprint cr2print eprint ;
: opcrcr0 ( name n n -- ) ['] opcrcr0print defineop swap cells opcrcrcrtab + ! drop ;

: opfcrcr0print ( n -- ) sprint dup cr3print bprint cr2print eprint ;
: opfcrcr0 ( name n n -- ) ['] opfcrcr0print defineop swap cells opfrfrfrtab + ! drop ;

: opcr00print ( n -- ) sprint  cr3print  eprint ;
: opcr00 ( name n n -- ) ['] opcr00print defineop swap cells oprrrtab + ! drop ;

: opcrb00print ( n -- ) sprint  crb3print  eprint ;
: opcrb00 ( name n n -- ) ['] opcrb00print defineop swap cells opfrfrfrtab + ! drop ;

: opc000print ( n -- ) sprint eprint drop ;
: opc000 ( name n n -- ) ['] opc000print defineop swap cells opcrcrcrtab + ! drop ;

: oprr0print ( n -- ) sprint  dup r3print bprint  r2print  eprint ;
: oprr0 ( name n n -- ) ['] oprr0print defineop swap cells oprrrtab +  ! drop ;

: opr0rprint ( n -- ) sprint  dup r3print bprint  r1print  eprint ;
: opr0r ( name n n -- ) ['] opr0rprint defineop swap cells oprrrtab +  ! drop ;

: oprn0print ( n -- ) sprint  dup r3print bprint  16 rshift 31 and .  eprint ;
: oprn0 ( name n n -- ) ['] oprn0print defineop swap cells oprrrtab +  ! drop ;

: opswrn0print ( n -- ) sprint  dup   16 rshift 31 and . bprint r3print  eprint ;
: opswrn0 ( name n n -- ) ['] opswrn0print defineop swap cells oprrrtab +  ! drop ;

: opswrr0print ( n -- ) sprint  dup r2print bprint  r3print  eprint ;
: opswrr0 ( name n n -- ) ['] opswrr0print defineop swap cells oprrrtab +  ! drop ;


: op0rrprint ( n -- ) sprint  dup r2print bprint  r1print  eprint ;
: op0rr ( name n n -- ) ['] op0rrprint defineop swap cells oprrrtab +  ! drop ;

: op00rprint ( n -- ) sprint  r1print  eprint ;
: op00r ( name n n -- ) ['] op00rprint defineop swap cells oprrrtab +  ! drop ;
: opr00print ( n -- ) sprint  r3print  eprint ;
: opr00 ( name n n -- ) ['] opr00print defineop swap cells oprrrtab +  ! drop ;
: opfr00print ( n -- ) sprint  fr3print  eprint ;
: opfr00 ( name n n -- ) ['] opfr00print defineop swap cells opfrfrfrtab +  ! drop ;

: opri0print ( n -- ) sprint  dup r3print bprint  11 rshift 2047 and .  eprint ;
: opri0 ( name n n -- ) ['] opri0print defineop swap cells oprrrtab +  ! drop ;

: opswri0print ( n -- ) sprint  dup   11 rshift 2047 and . bprint r3print  eprint ;
: opswri0 ( name n n -- ) ['] opswri0print defineop swap cells oprrrtab +  ! drop ;

: opcrmprint ( n -- ) sprint  dup r3print bprint  12 rshift 255 and .  eprint ;
: opcrm ( name n n -- ) ['] opcrmprint defineop swap cells oprrrtab +  ! drop ;

: opifrprint ( n -- ) sprint  dup 17 rshift 255 and . bprint  r1print  eprint ;
: opifr ( name n n -- ) ['] opifrprint defineop swap cells opfrfrfrtab +  ! drop ;

: opcrf0iprint ( n -- ) sprint  dup cr3print bprint  12 rshift 15 and . eprint ;
: opcrf0i ( name n n -- ) ['] opcrf0iprint defineop swap cells opfrfrfrtab +  ! drop ;


: oprriprint ( n -- ) sprint  dup r3print  bprint dup r2print   bprint imm16print eprint ;
: oprri ( name n -- ) [']  oprriprint defineop swap cells disasmtab + ! ;

: opnriprint ( n -- ) sprint  dup 26 rshift 31 and .  bprint dup r2print   bprint imm16print eprint ;
: opnri ( name n n -- ) [']  opnriprint defineop swap cells disasmtab + ! ;

: oparriprint ( n -- ) sprint dup r3print bprint dup imm16print ." ( " r2print ."  )" eprint ;
: oparri ( name n n -- ) ['] oparriprint defineop swap cells disasmtab + ! ;

: opafrriprint ( n -- ) sprint  dup fr3print  bprint dup imm16print ." ( " r2print ."  )" eprint ;
: opafrri ( name n -- ) [']  opafrriprint defineop swap cells disasmtab + ! ;


: opswrriprint ( n -- ) sprint dup r2print bprint dup r3print bprint imm16print  eprint ;
: opswrri ( name n -- ) [']  opswrriprint defineop swap cells disasmtab + ! ;

: opbiiprint ( n -- ) sprint dup 21 rshift imm5print bprint 16 rshift imm5print eprint ;
: opbii ( name n n -- ) ['] opbiiprint defineop swap cells opcrcrcrtab + ! drop ;

: opfrfrfrfrprint ( n -- ) sprint  dup fr3print bprint dup fr2print bprint dup fr0print  bprint  fr1print eprint ;
: opfrfrfrfr ( name n n -- ) ['] opfrfrfrfrprint defineop  swap cells opfrfrfrtab +  opfwtab drop ;
: opsfrfrfrfrprint ( n -- ) sprint  dup fr3print bprint dup fr2print bprint dup fr0print  bprint  fr1print eprint ;
: opsfrfrfrfr ( name n n -- ) ['] opsfrfrfrfrprint defineop  swap cells opsfrfrfrtab +  opfwtab drop ;

: opfrfr0frprint ( n -- ) sprint  dup fr3print bprint dup fr2print bprint  fr0print eprint ;
: opfrfr0fr ( name n n -- ) ['] opfrfr0frprint defineop  swap cells opfrfrfrtab +  opfwtab drop ;
: opsfrfr0frprint  ( n -- ) sprint  dup fr3print bprint dup fr2print bprint fr0print eprint ;
: opsfrfr0fr ( name n n -- ) ['] opsfrfr0frprint defineop  swap cells opsfrfrfrtab +  opfwtab drop ;

: opscprint  ( n -- ) sprint drop eprint ;
: opsc ['] opscprint defineop swap cells disasmtab + ! ;


1 9 lshift constant OE
0 constant ~OE

: RC 1 lshift 1 + ;
: ~RC 1 lshift ;


include ops.fs


set-current
: disasm-inst  ( n -- ) dup 26 rshift 63 and cells disasmtab + @ execute ; 
 previous


( Test code )
include asm.fs


ppc-asm-start
: disasm disasm-inst ;
