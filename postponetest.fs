\ checks that postpone works correctly with words with special
\ compilation semantics

\ by M. Anton Ertl 1996

\ This file is based on John Hayes' core.fr (coretest.fs), which has
\ the following copyright notice:

\ (C) 1995 JOHNS HOPKINS UNIVERSITY / APPLIED PHYSICS LABORATORY
\ MAY BE DISTRIBUTED FREELY AS LONG AS THIS COPYRIGHT NOTICE REMAINS.

\ my contributions to this file are in the public domain

\ you have to load John Hayes' tester.fs (=tester.fr) and coretest.fs
\ (core.fr) first

\ These tests are especially useful for showing that state-smart
\ implementations of words with special compilation semantics,
\ combined with a straight-forward implementation of POSTPONE (and
\ [COMPILE]) do not conform to the ANS Forth standard. The essential
\ sentences in the standad are:

\ 6.1.2033 POSTPONE CORE
\ ...
\ Compilation: ( <spaces>name -- ) 

\ Skip leading space delimiters. Parse name delimited by a space. Find
\ name. Append the compilation semantics of name to the current
\ definition.

\ 6.2.2530 [COMPILE] bracket-compile CORE EXT 
\ ...
\ Compilation: ( <spaces>name -- ) 

\ Skip leading space delimiters. Parse name delimited by a space. Find
\ name. If name has other than default compilation semantics, append
\ them to the current definition;...


\ Note that the compilation semantics are appended, not some
\ state-dependent semantics.

\ first I test against a non-ANS solution suggested by Bernd Paysan

: state@-now ( -- f )
    state @ ; immediate

: state@ ( -- f )
    POSTPONE state@-now ;

{ state@ -> state @ }

\ here I test POSTPONE with all core words with special compilation
\ semantics.

testing postpone (

: postpone-(
    postpone ( ;

{ : pp1 [ postpone-( does nothing ) ] ; -> }
{ here pp1 -> here }

testing postpone +loop

: postpone-+loop
    postpone +loop ;

{ : PGD2 DO I -1 [ POSTPONE-+LOOP ] ; -> }
{ 1 4 PGD2 -> 4 3 2 1 }
{ -1 2 PGD2 -> 2 1 0 -1 }
{ MID-UINT MID-UINT+1 PGD2 -> MID-UINT+1 MID-UINT }

{ : PGD4 DO 1 0 DO J LOOP -1 [ POSTPONE-+LOOP ] ; -> }
{ 1 4 PGD4 -> 4 3 2 1 }
{ -1 2 PGD4 -> 2 1 0 -1 }
{ MID-UINT MID-UINT+1 PGD4 -> MID-UINT+1 MID-UINT }

testing postpone ."

: postpone-."
    postpone ." ;

: pdq2 [ postpone-." you should see this later. " ] cr ;
: pdq1 [ postpone-." you should see this first. " ] cr ;
{ pdq1 pdq2 -> }

testing postpone ;
: postpone-;
    postpone ; ;

{ : psc [ postpone-; -> }
{ psc -> }    

testing postpone abort"

: postpone-abort"
    postpone abort" ;

{ : paq1 [ postpone-abort" this should not abort" ] ; -> }

testing postpone begin
: postpone-begin
    postpone begin ;

{ : PB3 [ POSTPONE-BEGIN ] DUP 5 < WHILE DUP 1+ REPEAT ; -> }
{ 0 PB3 -> 0 1 2 3 4 5 }
{ 4 PB3 -> 4 5 }
{ 5 PB3 -> 5 }
{ 6 PB3 -> 6 }

{ : PB4 [ POSTPONE-BEGIN ] DUP 1+ DUP 5 > UNTIL ; -> }
{ 3 PB4 -> 3 4 5 6 }
{ 5 PB4 -> 5 6 }
{ 6 PB4 -> 6 7 }

{ : PB5 [ POSTPONE-BEGIN ] DUP 2 > WHILE DUP 5 < WHILE DUP 1+ REPEAT 123 ELSE 345 THEN ; -> }
{ 1 PB5 -> 1 345 }
{ 2 PB5 -> 2 345 }
{ 3 PB5 -> 3 4 5 123 }
{ 4 PB5 -> 4 5 123 }
{ 5 PB5 -> 5 123 }

testing postpone do
: postpone-do
    postpone do ;

{ : PDO1 [ POSTPONE-DO ] I LOOP ; -> }
{ 4 1 PDO1 -> 1 2 3 }
{ 2 -1 PDO1 -> -1 0 1 }
{ MID-UINT+1 MID-UINT PDO1 -> MID-UINT }

{ : PDO2 [ POSTPONE-DO ] I -1 +LOOP ; -> }
{ 1 4 PDO2 -> 4 3 2 1 }
{ -1 2 PDO2 -> 2 1 0 -1 }
{ MID-UINT MID-UINT+1 PDO2 -> MID-UINT+1 MID-UINT }

{ : PDO3 [ POSTPONE-DO ] 1 0 [ POSTPONE-DO ] J LOOP LOOP ; -> }
{ 4 1 PDO3 -> 1 2 3 }
{ 2 -1 PDO3 -> -1 0 1 }
{ MID-UINT+1 MID-UINT PDO3 -> MID-UINT }

{ : PDO4 [ POSTPONE-DO ] 1 0 [ POSTPONE-DO ] J LOOP -1 +LOOP ; -> }
{ 1 4 PDO4 -> 4 3 2 1 }
{ -1 2 PDO4 -> 2 1 0 -1 }
{ MID-UINT MID-UINT+1 PDO4 -> MID-UINT+1 MID-UINT }

{ : PDO5 123 SWAP 0 [ POSTPONE-DO ] I 4 > IF DROP 234 LEAVE THEN LOOP ; -> }
{ 1 PDO5 -> 123 }
{ 5 PDO5 -> 123 }
{ 6 PDO5 -> 234 }

{ : PDO6  ( PAT: {0 0},{0 0}{1 0}{1 1},{0 0}{1 0}{1 1}{2 0}{2 1}{2 2} )
   0 SWAP 0 [ POSTPONE-DO ]
      I 1+ 0 [ POSTPONE-DO ] I J + 3 = IF I UNLOOP I UNLOOP EXIT THEN 1+ LOOP
    LOOP ; -> }
{ 1 PDO6 -> 1 }
{ 2 PDO6 -> 3 }
{ 3 PDO6 -> 4 1 2 }

testing postpone does>
: postpone-does>
    postpone does> ;

{ : PDOES1 [ POSTPONE-DOES> ] @ 1 + ; -> }
{ : PDOES2 [ POSTPONE-DOES> ] @ 2 + ; -> }
{ CREATE PCR1 -> }
{ PCR1 -> HERE }
{ ' PCR1 >BODY -> HERE }
{ 1 , -> }
{ PCR1 @ -> 1 }
{ PDOES1 -> }
{ PCR1 -> 2 }
{ PDOES2 -> }
{ PCR1 -> 3 }

{ : pWEIRD: CREATE [ POSTPONE-DOES> ] 1 + [ POSTPONE-DOES> ] 2 + ; -> }
{ pWEIRD: PW1 -> }
{ ' PW1 >BODY -> HERE }
{ PW1 -> HERE 1 + }
{ PW1 -> HERE 2 + }

testing postpone else
: postpone-else
    postpone else ;

{ : PELSE1 IF 123 [ postpone-ELSE ] 234 THEN ; -> }
{ 0 PELSE1 -> 234 }
{ 1 PELSE1 -> 123 }

{ : PELSE2 BEGIN DUP 2 > WHILE DUP 5 < WHILE DUP 1+ REPEAT 123 [ postpone-ELSE ] 345 THEN ; -> }
{ 1 PELSE2 -> 1 345 }
{ 2 PELSE2 -> 2 345 }
{ 3 PELSE2 -> 3 4 5 123 }
{ 4 PELSE2 -> 4 5 123 }
{ 5 PELSE2 -> 5 123 }

testing postpone if
: postpone-if
    postpone if ;

{ : PIF1 [ POSTPONE-IF ] 123 THEN ; -> }
{ : PIF2 [ POSTPONE-IF ] 123 ELSE 234 THEN ; -> }
{ 0 PIF1 -> }
{ 1 PIF1 -> 123 }
{ -1 PIF1 -> 123 }
{ 0 PIF2 -> 234 }
{ 1 PIF2 -> 123 }
{ -1 PIF1 -> 123 }

{ : PIF6 ( N -- 0,1,..N ) DUP [ POSTPONE-IF ] DUP >R 1- RECURSE R> THEN ; -> }
{ 0 PIF6 -> 0 }
{ 1 PIF6 -> 0 1 }
{ 2 PIF6 -> 0 1 2 }
{ 3 PIF6 -> 0 1 2 3 }
{ 4 PIF6 -> 0 1 2 3 4 }

testing postpone literal
: postpone-literal
    postpone literal ;

{ : plit [ 42 postpone-literal ] ; -> }
{ plit -> 42 }

testing postpone loop
: postpone-loop
    postpone loop ;

{ : PLOOP1 DO I [ POSTPONE-LOOP ] ; -> }
{ 4 1 PLOOP1 -> 1 2 3 }
{ 2 -1 PLOOP1 -> -1 0 1 }
{ MID-UINT+1 MID-UINT PLOOP1 -> MID-UINT }

{ : PLOOP3 DO 1 0 DO J [ POSTPONE-LOOP ] [ POSTPONE-LOOP ] ; -> }
{ 4 1 PLOOP3 -> 1 2 3 }
{ 2 -1 PLOOP3 -> -1 0 1 }
{ MID-UINT+1 MID-UINT PLOOP3 -> MID-UINT }

{ : PLOOP4 DO 1 0 DO J [ POSTPONE-LOOP ] -1 +LOOP ; -> }
{ 1 4 PLOOP4 -> 4 3 2 1 }
{ -1 2 PLOOP4 -> 2 1 0 -1 }
{ MID-UINT MID-UINT+1 PLOOP4 -> MID-UINT+1 MID-UINT }

{ : PLOOP5 123 SWAP 0 DO I 4 > IF DROP 234 LEAVE THEN [ POSTPONE-LOOP ] ; -> }
{ 1 PLOOP5 -> 123 }
{ 5 PLOOP5 -> 123 }
{ 6 PLOOP5 -> 234 }

{ : PLOOP6  ( PAT: {0 0},{0 0}{1 0}{1 1},{0 0}{1 0}{1 1}{2 0}{2 1}{2 2} )
   0 SWAP 0 DO
      I 1+ 0 DO I J + 3 = IF I UNLOOP I UNLOOP EXIT THEN 1+ [ POSTPONE-LOOP ]
    [ POSTPONE-LOOP ] ; -> }
{ 1 PLOOP6 -> 1 }
{ 2 PLOOP6 -> 3 }
{ 3 PLOOP6 -> 4 1 2 }

testing postpone postpone
: postpone-postpone
    postpone postpone ;

{ : PPP1 123 ; -> }
{ : PPP4 [ POSTPONE-POSTPONE PPP1 ] ; IMMEDIATE -> }
{ : PPP5 PPP4 ; -> }
{ PPP5 -> 123 }
{ : PPP6 345 ; IMMEDIATE -> }
{ : PPP7 [ POSTPONE-POSTPONE PPP6 ] ; -> }
{ PPP7 -> 345 }

testing postpone recurse
: postpone-recurse
    postpone recurse ;

{ : GREC ( N -- 0,1,..N ) DUP IF DUP >R 1- [ postpone-RECURSE ] R> THEN ; -> }
{ 0 GREC -> 0 }
{ 1 GREC -> 0 1 }
{ 2 GREC -> 0 1 2 }
{ 3 GREC -> 0 1 2 3 }
{ 4 GREC -> 0 1 2 3 4 }

testing postpone repeat
: postpone-repeat
    postpone repeat ;

{ : PREP3 BEGIN DUP 5 < WHILE DUP 1+ [ POSTPONE-REPEAT ] ; -> }
{ 0 PREP3 -> 0 1 2 3 4 5 }
{ 4 PREP3 -> 4 5 }
{ 5 PREP3 -> 5 }
{ 6 PREP3 -> 6 }

{ : PREP5 BEGIN DUP 2 > WHILE DUP 5 < WHILE DUP 1+ [ POSTPONE-REPEAT ] 123 ELSE 345 THEN ; -> }
{ 1 PREP5 -> 1 345 }
{ 2 PREP5 -> 2 345 }
{ 3 PREP5 -> 3 4 5 123 }
{ 4 PREP5 -> 4 5 123 }
{ 5 PREP5 -> 5 123 }

testing postpone S"
: postpone-s"
    postpone s" ;

{ : PSQ4 [ postpone-S" XY" ] ; -> }
{ PSQ4 SWAP DROP -> 2 }
{ PSQ4 DROP DUP C@ SWAP CHAR+ C@ -> 58 59 }

testing postpone then
: postpone-then
    postpone then ;

{ : PTH1 IF 123 [ POSTPONE-THEN ] ; -> }
{ : PTH2 IF 123 ELSE 234 [ POSTPONE-THEN ] ; -> }
{ 0 PTH1 -> }
{ 1 PTH1 -> 123 }
{ -1 PTH1 -> 123 }
{ 0 PTH2 -> 234 }
{ 1 PTH2 -> 123 }
{ -1 PTH1 -> 123 }

{ : PTH5 BEGIN DUP 2 > WHILE DUP 5 < WHILE DUP 1+ REPEAT 123 ELSE 345 [ POSTPONE-THEN ] ; -> }
{ 1 PTH5 -> 1 345 }
{ 2 PTH5 -> 2 345 }
{ 3 PTH5 -> 3 4 5 123 }
{ 4 PTH5 -> 4 5 123 }
{ 5 PTH5 -> 5 123 }

{ : PTH6 ( N -- 0,1,..N ) DUP IF DUP >R 1- RECURSE R> [ POSTPONE-THEN ] ; -> }
{ 0 PTH6 -> 0 }
{ 1 PTH6 -> 0 1 }
{ 2 PTH6 -> 0 1 2 }
{ 3 PTH6 -> 0 1 2 3 }
{ 4 PTH6 -> 0 1 2 3 4 }

testing postpone until
: postpone-until
    postpone until ;

{ : PUNT4 BEGIN DUP 1+ DUP 5 > [ postpone-UNTIL ] ; -> }
{ 3 PUNT4 -> 3 4 5 6 }
{ 5 PUNT4 -> 5 6 }
{ 6 PUNT4 -> 6 7 }

testing postpone while
: postpone-while
    postpone while ;

{ : PWH3 BEGIN DUP 5 < [ POSTPONE-WHILE ] DUP 1+ REPEAT ; -> }
{ 0 PWH3 -> 0 1 2 3 4 5 }
{ 4 PWH3 -> 4 5 }
{ 5 PWH3 -> 5 }
{ 6 PWH3 -> 6 }

{ : PWH5 BEGIN DUP 2 > [ POSTPONE-WHILE ] DUP 5 < [ POSTPONE-WHILE ] DUP 1+ REPEAT 123 ELSE 345 THEN ; -> }
{ 1 PWH5 -> 1 345 }
{ 2 PWH5 -> 2 345 }
{ 3 PWH5 -> 3 4 5 123 }
{ 4 PWH5 -> 4 5 123 }
{ 5 PWH5 -> 5 123 }


testing postpone [
: postpone-[
    postpone [ ;

{ here postpone-[ -> here }

testing postpone [']
: postpone-[']
    postpone ['] ;

{ : PTICK1 123 ; -> }
{ : PTICK2 [ postpone-['] PTICK1 ] ; IMMEDIATE -> }
{ PTICK2 EXECUTE -> 123 }

testing postpone [char]
: postpone-[char]
    postpone [char] ;

{ : PCHAR1 [ postpone-[CHAR] X ] ; -> }
{ : PCHAR2 [ postpone-[CHAR] HELLO ] ; -> }
{ PCHAR1 -> 58 }
{ PCHAR2 -> 48 }

