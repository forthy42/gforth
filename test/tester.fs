\ From: John Hayes S1I
\ Subject: tester.fr
\ Date: Mon, 27 Nov 95 13:10:09 PST  

\ (C) 1995 JOHNS HOPKINS UNIVERSITY / APPLIED PHYSICS LABORATORY
\ MAY BE DISTRIBUTED FREELY AS LONG AS THIS COPYRIGHT NOTICE REMAINS.
\ VERSION 1.1

\ revised by Anton Ertl 2007-08-12
\ The original has two shortcomings:

\ - It does not work as expected if the stack is non-empty before the {.

\ - It does not check FP results if the system has a separate FP stack.

\ I have revised it to address both shortcomings.  You can find the
\ result at

\ http://www.forth200x.org/tests/tester.fs

\ It is intended to be a drop-in replacement of the original.

\ In spirit of the original, I have strived to avoid any potential
\ non-portabilities and stayed as much within the CORE words as
\ possible; e.g., FLOATING words are used only if the FLOATING wordset
\ is present and the FP stack is separate.

\ There are a few things to be noted:

\ - Following the despicable practice of the original, this version sets
\   the base to HEX for everything that gets loaded later.
\   Floating-point input is ambiguous when the base is not decimal, so
\   you have to set it to decimal yourself when you want to deal with
\   decimal numbers.

\ - The separate-FP-stack code has an fvariable FSENSITIVITY that allows
\   approximate matching of FP results (it's used as the r3 parameter of
\   F~).  However, that's used only in the separate-fp-stack case.  With
\   a shared-fp-stack you get exact matching in any case (actually
\   FSENSITIVITY variable is not even defined in that case).  So if you
\   define an FP test case and want to support shared-FP-stack systems,
\   better do the approximate matching yourself.  E.g., instead of

\   -1e-12 fsensitivity f!
\   { ... computation ... -> 2.345678901e }

\   write

\   { ... computation ... 2.345678901e -1e-12 f~ -> true }
HEX

\ SET THE FOLLOWING FLAG TO TRUE FOR MORE VERBOSE OUTPUT; THIS MAY
\ ALLOW YOU TO TELL WHICH TEST CAUSED YOUR SYSTEM TO HANG.
VARIABLE VERBOSE
   FALSE VERBOSE !

VARIABLE ACTUAL-DEPTH			\ STACK RECORD
CREATE ACTUAL-RESULTS 20 CELLS ALLOT
VARIABLE START-DEPTH
VARIABLE ERROR-XT

: ERROR ERROR-XT @ EXECUTE ;

: "FLOATING" S" FLOATING" ; \ ONLY COMPILED S" IN CORE
: "FLOATING-STACK" S" FLOATING-STACK" ;
"FLOATING" ENVIRONMENT? [IF]
    [IF]
        "FLOATING-STACK" ENVIRONMENT? [IF]
            [IF]
                TRUE
            [ELSE]
                FALSE
            [THEN]
        [ELSE] \ WE DON'T KNOW WHETHER THE FP STACK IS SEPARATE
            TRUE \ SAFER CHOICE TO ASSUME IT IS
        [THEN]  
    [ELSE]
        FALSE
    [THEN]
[ELSE]
    FALSE
[THEN]
[IF] \ WE HAVE FP WORDS AND A SEPARATE FP STACK
    FVARIABLE FSENSITIVITY DECIMAL 0E HEX FSENSITIVITY F!
    VARIABLE ACTUAL-FDEPTH
    CREATE ACTUAL-FRESULTS 20 FLOATS ALLOT
    VARIABLE START-FDEPTH

    : EMPTY-FSTACK ( ... -- ... )
        FDEPTH START-FDEPTH @ < IF
            FDEPTH START-FDEPTH @ SWAP DO 0E LOOP
        THEN
        FDEPTH START-FDEPTH @ > IF
            FDEPTH START-FDEPTH @ DO FDROP LOOP
        THEN ;

    : F{ ( -- )
        FDEPTH START-FDEPTH ! ;

    : F-> ( ... -- ... )
        FDEPTH DUP ACTUAL-FDEPTH !
        START-FDEPTH @ > IF
            FDEPTH START-FDEPTH @ DO ACTUAL-FRESULTS I FLOATS + F! LOOP
        THEN ;

    : F} ( ... -- ... )
        FDEPTH ACTUAL-FDEPTH @ = IF
            FDEPTH START-FDEPTH @ > IF
                FDEPTH START-FDEPTH @ DO
                    ACTUAL-FRESULTS I FLOATS + F@
                    FSENSITIVITY F@ F~ INVERT IF
                        S" INCORRECT RESULT: " ERROR LEAVE
                    THEN
                LOOP
            THEN
        ELSE
            S" WRONG NUMBER OF RESULTS: " ERROR
        THEN ;
[ELSE]
    : EMPTY-FSTACK ;
    : F{ ;
    : F-> ;
    : F} ;
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

: {		\ ( -- ) SYNTACTIC SUGAR.
   DEPTH START-DEPTH ! F{ ;

: ->		\ ( ... -- ) RECORD DEPTH AND CONTENT OF STACK.
   DEPTH DUP ACTUAL-DEPTH !		\ RECORD DEPTH
   START-DEPTH @ > IF		\ IF THERE IS SOMETHING ON STACK
       DEPTH START-DEPTH @ DO ACTUAL-RESULTS I CELLS + ! LOOP \ SAVE THEM
   THEN
   F-> ;

: }		\ ( ... -- ) COMPARE STACK (EXPECTED) CONTENTS WITH SAVED
		\ (ACTUAL) CONTENTS.
   DEPTH ACTUAL-DEPTH @ = IF		\ IF DEPTHS MATCH
      DEPTH START-DEPTH @ > IF		\ IF THERE IS SOMETHING ON THE STACK
         DEPTH START-DEPTH @ DO		\ FOR EACH STACK ITEM
	    ACTUAL-RESULTS I CELLS + @	\ COMPARE ACTUAL WITH EXPECTED
	    <> IF S" INCORRECT RESULT: " ERROR LEAVE THEN
	 LOOP
      THEN
   ELSE					\ DEPTH MISMATCH
      S" WRONG NUMBER OF RESULTS: " ERROR
   THEN
   F} ;

: TESTING	\ ( -- ) TALKING COMMENT.
   SOURCE VERBOSE @
   IF DUP >R TYPE CR R> >IN !
   ELSE >IN ! DROP
   THEN ;

