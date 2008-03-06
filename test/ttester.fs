\ for the original tester
\ From: John Hayes S1I
\ Subject: tester.fr
\ Date: Mon, 27 Nov 95 13:10:09 PST  
\ (C) 1995 JOHNS HOPKINS UNIVERSITY / APPLIED PHYSICS LABORATORY
\ MAY BE DISTRIBUTED FREELY AS LONG AS THIS COPYRIGHT NOTICE REMAINS.
\ VERSION 1.1

\ for the FNEARLY= stuff:
\ from ftester.fs written by David N. Williams, based on the idea of
\ approximate equality in Dirk Zoller's float.4th
\ public domain

\ for the rest:
\ revised by Anton Ertl 2007-08-12, 2007-08-19, 2007-08-28
\ public domain

\ The original has the following shortcomings:

\ - It does not work as expected if the stack is non-empty before the {.

\ - It does not check FP results if the system has a separate FP stack.

\ - There is a conflict with the use of } for FSL arrays and { for locals.

\ I have revised it to address these shortcomings.  You can find the
\ result at

\ http://www.forth200x.org/tests/tester.fs
\ http://www.forth200x.org/tests/ttester.fs

\ tester.fs is intended to be a drop-in replacement of the original.

\ ttester.fs is a version that uses T{ and }T instead of { and } and
\ keeps the BASE as it was before loading ttester.fs

\ In spirit of the original, I have strived to avoid any potential
\ non-portabilities and stayed as much within the CORE words as
\ possible; e.g., FLOATING words are used only if the FLOATING wordset
\ is present

\ There are a few things to be noted:

\ - Loading ttester.fs does not change BASE.  Loading tester.fs
\ changes BASE to HEX (like the original tester).  Floating-point
\ input is ambiguous when the base is not decimal, so you have to set
\ it to decimal yourself when you want to deal with decimal numbers.

\ - For FP it is often useful to use approximate equality for checking
\ the results.  You can turn on approximate matching with SET-NEAR
\ (and turn it off (default) with SET-EXACT, and you can tune it by
\ setting the variables REL-NEAR and ABS-NEAR.  If you want your tests
\ to work with a shared stack, you have to specify the types of the
\ elements on the stack by using one of the closing words that specify
\ types, e.g. RRRX}T for checking the stack picture ( r r r x ).
\ There are such words for all combination of R and X with up to 4
\ stack items, and defining more if you need them is straightforward
\ (see source).  If your tests are only intended for a separate-stack
\ system or if you need only exact matching, you can use the plain }T
\ instead.

BASE @
HEX

\ SET THE FOLLOWING FLAG TO TRUE FOR MORE VERBOSE OUTPUT; THIS MAY
\ ALLOW YOU TO TELL WHICH TEST CAUSED YOUR SYSTEM TO HANG.
VARIABLE VERBOSE
   FALSE VERBOSE !

VARIABLE ACTUAL-DEPTH			\ STACK RECORD
CREATE ACTUAL-RESULTS 20 CELLS ALLOT
VARIABLE START-DEPTH
VARIABLE XCURSOR \ FOR ...}T
VARIABLE ERROR-XT

: ERROR ERROR-XT @ EXECUTE ;

: "FLOATING" S" FLOATING" ; \ ONLY COMPILED S" IN CORE
: "FLOATING-STACK" S" FLOATING-STACK" ;
"FLOATING" ENVIRONMENT? [IF]
    [IF]
        TRUE
    [ELSE]
        FALSE
    [THEN]
[ELSE]
    FALSE
[THEN] CONSTANT HAS-FLOATING
"FLOATING-STACK" ENVIRONMENT? [IF]
    [IF]
        TRUE
    [ELSE]
        FALSE
    [THEN]
[ELSE] \ WE DON'T KNOW WHETHER THE FP STACK IS SEPARATE
    HAS-FLOATING \ IF WE HAVE FLOATING, WE ASSUME IT IS
[THEN] CONSTANT HAS-FLOATING-STACK

HAS-FLOATING [IF]
    \ SET THE FOLLOWING TO THE RELATIVE AND ABSOLUTE TOLERANCES YOU
    \ WANT FOR APPROXIMATE FLOAT EQUALITY, TO BE USED WITH F~ IN
    \ FNEARLY=.  KEEP THE SIGNS, BECAUSE F~ NEEDS THEM.
    FVARIABLE REL-NEAR DECIMAL 1E-12 HEX REL-NEAR F!
    FVARIABLE ABS-NEAR    DECIMAL 0E HEX ABS-NEAR F!

    \ WHEN EXACT? IS TRUE, }F USES FEXACTLY=, OTHERWISE FNEARLY=.
    
    TRUE VALUE EXACT?
    : SET-EXACT  ( -- )   TRUE TO EXACT? ;
    : SET-NEAR   ( -- )  FALSE TO EXACT? ;

    DECIMAL
    : FEXACTLY=  ( F: X Y -- S: FLAG )
        (
        LEAVE TRUE IF THE TWO FLOATS ARE IDENTICAL.
        )
        0E F~ ;
    HEX
    
    : FABS=  ( F: X Y -- S: FLAG )
        (
        LEAVE TRUE IF THE TWO FLOATS ARE EQUAL WITHIN THE TOLERANCE
        STORED IN ABS-NEAR.
        )
        ABS-NEAR F@ F~ ;
    
    : FREL=  ( F: X Y -- S: FLAG )
        (
        LEAVE TRUE IF THE TWO FLOATS ARE RELATIVELY EQUAL BASED ON THE
        TOLERANCE STORED IN ABS-NEAR.
        )
        REL-NEAR F@ FNEGATE F~ ;

    : F2DUP  FOVER FOVER ;
    : F2DROP FDROP FDROP ;
    
    : FNEARLY=  ( F: X Y -- S: FLAG )
        (
        LEAVE TRUE IF THE TWO FLOATS ARE NEARLY EQUAL.  THIS IS A
        REFINEMENT OF DIRK ZOLLER'S FEQ TO ALSO ALLOW X = Y, INCLUDING
        BOTH ZERO, OR TO ALLOW APPROXIMATE EQUALITY WHEN X AND Y ARE TOO
        SMALL TO SATISFY THE RELATIVE APPROXIMATION MODE IN THE F~
        SPECIFICATION.
        )
        F2DUP FEXACTLY= IF F2DROP TRUE EXIT THEN
        F2DUP FREL=     IF F2DROP TRUE EXIT THEN
        FABS= ;

    : FCONF= ( R1 R2 -- F )
        EXACT? IF
            FEXACTLY=
        ELSE
            FNEARLY=
        THEN ;
[THEN]

HAS-FLOATING-STACK [IF]
    VARIABLE ACTUAL-FDEPTH
    CREATE ACTUAL-FRESULTS 20 FLOATS ALLOT
    VARIABLE START-FDEPTH
    VARIABLE FCURSOR

    : EMPTY-FSTACK ( ... -- ... )
        FDEPTH START-FDEPTH @ < IF
            FDEPTH START-FDEPTH @ SWAP DO 0E LOOP
        THEN
        FDEPTH START-FDEPTH @ > IF
            FDEPTH START-FDEPTH @ DO FDROP LOOP
        THEN ;

    : F{ ( -- )
        FDEPTH START-FDEPTH ! 0 FCURSOR ! ;

    : F-> ( ... -- ... )
        FDEPTH DUP ACTUAL-FDEPTH !
        START-FDEPTH @ > IF
            FDEPTH START-FDEPTH @ - 0 DO ACTUAL-FRESULTS I FLOATS + F! LOOP
        THEN ;

    : F} ( ... -- ... )
        FDEPTH ACTUAL-FDEPTH @ = IF
            FDEPTH START-FDEPTH @ > IF
                FDEPTH START-FDEPTH @ - 0 DO
                    ACTUAL-FRESULTS I FLOATS + F@ FCONF= INVERT IF
                        S" INCORRECT FP RESULT: " ERROR LEAVE
                    THEN
                LOOP
            THEN
        ELSE
            S" WRONG NUMBER OF FP RESULTS: " ERROR
        THEN ;

    : F...}T ( -- )
        FCURSOR @ START-FDEPTH @ + ACTUAL-FDEPTH @ <> IF
            S" NUMBER OF FLOAT RESULTS BEFORE '->' DOES NOT MATCH ...}T SPECIFICATION: " ERROR
        ELSE FDEPTH START-FDEPTH @ = 0= IF
            S" NUMBER OF FLOAT RESULTS BEFORE AND AFTER '->' DOES NOT MATCH: " ERROR
        THEN THEN ;

    
    : FTESTER ( R -- )
        FDEPTH 0= ACTUAL-FDEPTH @ FCURSOR @ START-FDEPTH @ + 1+ < OR IF
            S" NUMBER OF FLOAT RESULTS AFTER '->' BELOW ...}T SPECIFICATION: " ERROR 
        ELSE ACTUAL-FRESULTS FCURSOR @ FLOATS + F@ FCONF= 0= IF
            S" INCORRECT FP RESULT: " ERROR
        THEN THEN
        1 FCURSOR +! ;
        
[ELSE]
    : EMPTY-FSTACK ;
    : F{ ;
    : F-> ;
    : F} ;
    : F...}T ;

    DECIMAL
    : COMPUTE-CELLS-PER-FP ( -- U )
        DEPTH 0E DEPTH 1- >R FDROP R> SWAP - ;
    HEX

    COMPUTE-CELLS-PER-FP CONSTANT CELLS-PER-FP
    
    : FTESTER ( R -- )
        DEPTH CELLS-PER-FP < ACTUAL-DEPTH @ XCURSOR @ START-DEPTH @ + CELLS-PER-FP + < OR IF
            S" NUMBER OF RESULTS AFTER '->' BELOW ...}T SPECIFICATION: " ERROR EXIT
        ELSE ACTUAL-RESULTS XCURSOR @ CELLS + F@ FCONF= 0= IF
            S" INCORRECT FP RESULT: " ERROR
        THEN THEN
        CELLS-PER-FP XCURSOR +! ;
 [THEN]    

: EMPTY-STACK	\ ( ... -- ) EMPTY STACK: HANDLES UNDERFLOWED STACK TOO.
    DEPTH START-DEPTH @ < IF
        DEPTH START-DEPTH @ SWAP DO 0 LOOP
    THEN
    DEPTH START-DEPTH @ > IF
        DEPTH START-DEPTH @ DO DROP LOOP
    THEN
    EMPTY-FSTACK ;

: ERROR1	\ ( C-ADDR U -- ) DISPLAY AN ERROR MESSAGE FOLLOWED BY
		\ THE LINE THAT HAD THE ERROR.
   TYPE SOURCE TYPE CR			\ DISPLAY LINE CORRESPONDING TO ERROR
   EMPTY-STACK				\ THROW AWAY EVERY THING ELSE
;

' ERROR1 ERROR-XT !

: T{		\ ( -- ) SYNTACTIC SUGAR.
   DEPTH START-DEPTH ! 0 XCURSOR ! F{ ;

: ->		\ ( ... -- ) RECORD DEPTH AND CONTENT OF STACK.
   DEPTH DUP ACTUAL-DEPTH !		\ RECORD DEPTH
   START-DEPTH @ > IF		\ IF THERE IS SOMETHING ON STACK
       DEPTH START-DEPTH @ - 0 DO ACTUAL-RESULTS I CELLS + ! LOOP \ SAVE THEM
   THEN
   F-> ;

: }T		\ ( ... -- ) COMPARE STACK (EXPECTED) CONTENTS WITH SAVED
		\ (ACTUAL) CONTENTS.
   DEPTH ACTUAL-DEPTH @ = IF		\ IF DEPTHS MATCH
      DEPTH START-DEPTH @ > IF		\ IF THERE IS SOMETHING ON THE STACK
         DEPTH START-DEPTH @ - 0 DO	\ FOR EACH STACK ITEM
	    ACTUAL-RESULTS I CELLS + @	\ COMPARE ACTUAL WITH EXPECTED
	    <> IF S" INCORRECT RESULT: " ERROR LEAVE THEN
	 LOOP
      THEN
   ELSE					\ DEPTH MISMATCH
      S" WRONG NUMBER OF RESULTS: " ERROR
   THEN
   F} ;

: ...}T ( -- )
    XCURSOR @ START-DEPTH @ + ACTUAL-DEPTH @ <> IF
        S" NUMBER OF CELL RESULTS BEFORE '->' DOES NOT MATCH ...}T SPECIFICATION: " ERROR
    ELSE DEPTH START-DEPTH @ = 0= IF
        S" NUMBER OF CELL RESULTS BEFORE AND AFTER '->' DOES NOT MATCH: " ERROR
    THEN THEN
    F...}T ;

: XTESTER ( X -- )
    DEPTH 0= ACTUAL-DEPTH @ XCURSOR @ START-DEPTH @ + 1+ < OR IF
        S" NUMBER OF CELL RESULTS AFTER '->' BELOW ...}T SPECIFICATION: " ERROR EXIT
    ELSE ACTUAL-RESULTS XCURSOR @ CELLS + @ <> IF
        S" INCORRECT CELL RESULT: " ERROR
    THEN THEN
    1 XCURSOR +! ;

: X}T XTESTER ...}T ;
: R}T FTESTER ...}T ;
: XX}T XTESTER XTESTER ...}T ;
: XR}T FTESTER XTESTER ...}T ;
: RX}T XTESTER FTESTER ...}T ;
: RR}T FTESTER FTESTER ...}T ;
: XXX}T XTESTER XTESTER XTESTER ...}T ;
: XXR}T FTESTER XTESTER XTESTER ...}T ;
: XRX}T XTESTER FTESTER XTESTER ...}T ;
: XRR}T FTESTER FTESTER XTESTER ...}T ;
: RXX}T XTESTER XTESTER FTESTER ...}T ;
: RXR}T FTESTER XTESTER FTESTER ...}T ;
: RRX}T XTESTER FTESTER FTESTER ...}T ;
: RRR}T FTESTER FTESTER FTESTER ...}T ;
: XXXX}T XTESTER XTESTER XTESTER XTESTER ...}T ;
: XXXR}T FTESTER XTESTER XTESTER XTESTER ...}T ;
: XXRX}T XTESTER FTESTER XTESTER XTESTER ...}T ;
: XXRR}T FTESTER FTESTER XTESTER XTESTER ...}T ;
: XRXX}T XTESTER XTESTER FTESTER XTESTER ...}T ;
: XRXR}T FTESTER XTESTER FTESTER XTESTER ...}T ;
: XRRX}T XTESTER FTESTER FTESTER XTESTER ...}T ;
: XRRR}T FTESTER FTESTER FTESTER XTESTER ...}T ;
: RXXX}T XTESTER XTESTER XTESTER FTESTER ...}T ;
: RXXR}T FTESTER XTESTER XTESTER FTESTER ...}T ;
: RXRX}T XTESTER FTESTER XTESTER FTESTER ...}T ;
: RXRR}T FTESTER FTESTER XTESTER FTESTER ...}T ;
: RRXX}T XTESTER XTESTER FTESTER FTESTER ...}T ;
: RRXR}T FTESTER XTESTER FTESTER FTESTER ...}T ;
: RRRX}T XTESTER FTESTER FTESTER FTESTER ...}T ;
: RRRR}T FTESTER FTESTER FTESTER FTESTER ...}T ;

: TESTING	\ ( -- ) TALKING COMMENT.
   SOURCE VERBOSE @
   IF DUP >R TYPE CR R> >IN !
   ELSE >IN ! DROP
   THEN ;

BASE !
