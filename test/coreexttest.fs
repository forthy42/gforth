\ To test the ANS Forth Core Extension word set

\ This program was written by Gerry Jackson in 2006, with contributions from
\ others where indicated, and is in the public domain - it can be distributed
\ and/or modified in any way but please retain this notice.

\ This program is distributed in the hope that it will be useful,
\ but WITHOUT ANY WARRANTY; without even the implied warranty of
\ MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.

\ The tests are not claimed to be comprehensive or correct 

\ ------------------------------------------------------------------------------
\ Version 0.11 7 April 2015
\              Added tests for PARSE-NAME HOLDS BUFFER:
\              S\" tests added
\              DEFER IS ACTION-OF DEFER! DEFER@ tests added
\              Empty CASE statement test added
\              [COMPILE] tests removed because it is obsolescent in Forth 2012
\         0.10 1 August 2014
\             Added tests contributed by James Bowman for:
\                <> U> 0<> 0> NIP TUCK ROLL PICK 2>R 2R@ 2R>
\                HEX WITHIN UNUSED AGAIN MARKER
\             Added tests for:
\                .R U.R ERASE PAD REFILL SOURCE-ID 
\             Removed ABORT from NeverExecuted to enable Win32
\             to continue after failure of RESTORE-INPUT.
\             Removed max-intx which is no longer used.
\         0.7 6 June 2012 Extra CASE test added
\         0.6 1 April 2012 Tests placed in the public domain.
\             SAVE-INPUT & RESTORE-INPUT tests, position
\             of T{ moved so that tests work with ttester.fs
\             CONVERT test deleted - obsolete word removed from Forth 200X
\             IMMEDIATE VALUEs tested
\             RECURSE with :NONAME tested
\             PARSE and .( tested
\             Parsing behaviour of C" added
\         0.5 14 September 2011 Removed the double [ELSE] from the
\             initial SAVE-INPUT & RESTORE-INPUT test
\         0.4 30 November 2009  max-int replaced with max-intx to
\             avoid redefinition warnings.
\         0.3  6 March 2009 { and } replaced with T{ and }T
\                           CONVERT test now independent of cell size
\         0.2  20 April 2007 ANS Forth words changed to upper case
\                            Tests qd3 to qd6 by Reinhold Straub
\         0.1  Oct 2006 First version released
\ -----------------------------------------------------------------------------
\ The tests are based on John Hayes test program for the core word set

\ Words tested in this file are:
\     .( .R 0<> 0> 2>R 2R> 2R@ :NONAME <> ?DO AGAIN C" CASE COMPILE, ENDCASE
\     ENDOF ERASE FALSE HEX MARKER NIP OF PAD PARSE PICK REFILL
\     RESTORE-INPUT ROLL SAVE-INPUT SOURCE-ID TO TRUE TUCK U.R U> UNUSED
\     VALUE WITHIN [COMPILE]

\ Words not tested or partially tested:
\     \ because it has been extensively used already and is, hence, unnecessary
\     REFILL and SOURCE-ID from the user input device which are not possible
\     when testing from a file such as this one
\     UNUSED as the value returned is system dependent
\     Obsolescent words #TIB CONVERT EXPECT QUERY SPAN TIB as they have been
\     removed from the Forth 200X standard

\ Results from words that output to the user output device have to visually
\ checked for correctness. These are .R U.R .(

\ -----------------------------------------------------------------------------
\ Assumptions:
\     - tester.fr (or ttester.fs) and core-fr have been included prior to this
\       file
\ -----------------------------------------------------------------------------
TESTING Core Extension words

DECIMAL

TESTING TRUE FALSE

T{ TRUE  -> 0 INVERT }T
T{ FALSE -> 0 }T

\ -----------------------------------------------------------------------------
TESTING <> U>   (contributed by James Bowman)

T{ 0 0 <> -> <FALSE> }T
T{ 1 1 <> -> <FALSE> }T
T{ -1 -1 <> -> <FALSE> }T
T{ 1 0 <> -> <TRUE> }T
T{ -1 0 <> -> <TRUE> }T
T{ 0 1 <> -> <TRUE> }T
T{ 0 -1 <> -> <TRUE> }T

T{ 0 1 U> -> <FALSE> }T
T{ 1 2 U> -> <FALSE> }T
T{ 0 MID-UINT U> -> <FALSE> }T
T{ 0 MAX-UINT U> -> <FALSE> }T
T{ MID-UINT MAX-UINT U> -> <FALSE> }T
T{ 0 0 U> -> <FALSE> }T
T{ 1 1 U> -> <FALSE> }T
T{ 1 0 U> -> <TRUE> }T
T{ 2 1 U> -> <TRUE> }T
T{ MID-UINT 0 U> -> <TRUE> }T
T{ MAX-UINT 0 U> -> <TRUE> }T
T{ MAX-UINT MID-UINT U> -> <TRUE> }T

\ -----------------------------------------------------------------------------
TESTING 0<> 0>   (contributed by James Bowman)

T{ 0 0<> -> <FALSE> }T
T{ 1 0<> -> <TRUE> }T
T{ 2 0<> -> <TRUE> }T
T{ -1 0<> -> <TRUE> }T
T{ MAX-UINT 0<> -> <TRUE> }T
T{ MIN-INT 0<> -> <TRUE> }T
T{ MAX-INT 0<> -> <TRUE> }T

T{ 0 0> -> <FALSE> }T
T{ -1 0> -> <FALSE> }T
T{ MIN-INT 0> -> <FALSE> }T
T{ 1 0> -> <TRUE> }T
T{ MAX-INT 0> -> <TRUE> }T

\ -----------------------------------------------------------------------------
TESTING NIP TUCK ROLL PICK   (contributed by James Bowman)

T{ 1 2 NIP -> 2 }T
T{ 1 2 3 NIP -> 1 3 }T

T{ 1 2 TUCK -> 2 1 2 }T
T{ 1 2 3 TUCK -> 1 3 2 3 }T

T{ : ro5 100 200 300 400 500 ; -> }T
T{ ro5 3 ROLL -> 100 300 400 500 200 }T
T{ ro5 2 ROLL -> ro5 ROT }T
T{ ro5 1 ROLL -> ro5 SWAP }T
T{ ro5 0 ROLL -> ro5 }T

T{ ro5 2 PICK -> 100 200 300 400 500 300 }T
T{ ro5 1 PICK -> ro5 OVER }T
T{ ro5 0 PICK -> ro5 DUP }T

\ -----------------------------------------------------------------------------
TESTING 2>R 2R@ 2R>   (contributed by James Bowman)

T{ : rr0 2>R 100 R> R> ; -> }T
T{ 300 400 rr0 -> 100 400 300 }T
T{ 200 300 400 rr0 -> 200 100 400 300 }T

T{ : rr1 2>R 100 2R@ R> R> ; -> }T
T{ 300 400 rr1 -> 100 300 400 400 300 }T
T{ 200 300 400 rr1 -> 200 100 300 400 400 300 }T

T{ : rr2 2>R 100 2R> ; -> }T
T{ 300 400 rr2 -> 100 300 400 }T
T{ 200 300 400 rr2 -> 200 100 300 400 }T

\ -----------------------------------------------------------------------------
TESTING HEX   (contributed by James Bowman)

T{ BASE @ HEX BASE @ DECIMAL BASE @ - SWAP BASE ! -> 6 }T

\ -----------------------------------------------------------------------------
TESTING WITHIN   (contributed by James Bowman)

T{ 0 0 0 WITHIN -> <FALSE> }T
T{ 0 0 MID-UINT WITHIN -> <TRUE> }T
T{ 0 0 MID-UINT+1 WITHIN -> <TRUE> }T
T{ 0 0 MAX-UINT WITHIN -> <TRUE> }T
T{ 0 MID-UINT 0 WITHIN -> <FALSE> }T
T{ 0 MID-UINT MID-UINT WITHIN -> <FALSE> }T
T{ 0 MID-UINT MID-UINT+1 WITHIN -> <FALSE> }T
T{ 0 MID-UINT MAX-UINT WITHIN -> <FALSE> }T
T{ 0 MID-UINT+1 0 WITHIN -> <FALSE> }T
T{ 0 MID-UINT+1 MID-UINT WITHIN -> <TRUE> }T
T{ 0 MID-UINT+1 MID-UINT+1 WITHIN -> <FALSE> }T
T{ 0 MID-UINT+1 MAX-UINT WITHIN -> <FALSE> }T
T{ 0 MAX-UINT 0 WITHIN -> <FALSE> }T
T{ 0 MAX-UINT MID-UINT WITHIN -> <TRUE> }T
T{ 0 MAX-UINT MID-UINT+1 WITHIN -> <TRUE> }T
T{ 0 MAX-UINT MAX-UINT WITHIN -> <FALSE> }T
T{ MID-UINT 0 0 WITHIN -> <FALSE> }T
T{ MID-UINT 0 MID-UINT WITHIN -> <FALSE> }T
T{ MID-UINT 0 MID-UINT+1 WITHIN -> <TRUE> }T
T{ MID-UINT 0 MAX-UINT WITHIN -> <TRUE> }T
T{ MID-UINT MID-UINT 0 WITHIN -> <TRUE> }T
T{ MID-UINT MID-UINT MID-UINT WITHIN -> <FALSE> }T
T{ MID-UINT MID-UINT MID-UINT+1 WITHIN -> <TRUE> }T
T{ MID-UINT MID-UINT MAX-UINT WITHIN -> <TRUE> }T
T{ MID-UINT MID-UINT+1 0 WITHIN -> <FALSE> }T
T{ MID-UINT MID-UINT+1 MID-UINT WITHIN -> <FALSE> }T
T{ MID-UINT MID-UINT+1 MID-UINT+1 WITHIN -> <FALSE> }T
T{ MID-UINT MID-UINT+1 MAX-UINT WITHIN -> <FALSE> }T
T{ MID-UINT MAX-UINT 0 WITHIN -> <FALSE> }T
T{ MID-UINT MAX-UINT MID-UINT WITHIN -> <FALSE> }T
T{ MID-UINT MAX-UINT MID-UINT+1 WITHIN -> <TRUE> }T
T{ MID-UINT MAX-UINT MAX-UINT WITHIN -> <FALSE> }T
T{ MID-UINT+1 0 0 WITHIN -> <FALSE> }T
T{ MID-UINT+1 0 MID-UINT WITHIN -> <FALSE> }T
T{ MID-UINT+1 0 MID-UINT+1 WITHIN -> <FALSE> }T
T{ MID-UINT+1 0 MAX-UINT WITHIN -> <TRUE> }T
T{ MID-UINT+1 MID-UINT 0 WITHIN -> <TRUE> }T
T{ MID-UINT+1 MID-UINT MID-UINT WITHIN -> <FALSE> }T
T{ MID-UINT+1 MID-UINT MID-UINT+1 WITHIN -> <FALSE> }T
T{ MID-UINT+1 MID-UINT MAX-UINT WITHIN -> <TRUE> }T
T{ MID-UINT+1 MID-UINT+1 0 WITHIN -> <TRUE> }T
T{ MID-UINT+1 MID-UINT+1 MID-UINT WITHIN -> <TRUE> }T
T{ MID-UINT+1 MID-UINT+1 MID-UINT+1 WITHIN -> <FALSE> }T
T{ MID-UINT+1 MID-UINT+1 MAX-UINT WITHIN -> <TRUE> }T
T{ MID-UINT+1 MAX-UINT 0 WITHIN -> <FALSE> }T
T{ MID-UINT+1 MAX-UINT MID-UINT WITHIN -> <FALSE> }T
T{ MID-UINT+1 MAX-UINT MID-UINT+1 WITHIN -> <FALSE> }T
T{ MID-UINT+1 MAX-UINT MAX-UINT WITHIN -> <FALSE> }T
T{ MAX-UINT 0 0 WITHIN -> <FALSE> }T
T{ MAX-UINT 0 MID-UINT WITHIN -> <FALSE> }T
T{ MAX-UINT 0 MID-UINT+1 WITHIN -> <FALSE> }T
T{ MAX-UINT 0 MAX-UINT WITHIN -> <FALSE> }T
T{ MAX-UINT MID-UINT 0 WITHIN -> <TRUE> }T
T{ MAX-UINT MID-UINT MID-UINT WITHIN -> <FALSE> }T
T{ MAX-UINT MID-UINT MID-UINT+1 WITHIN -> <FALSE> }T
T{ MAX-UINT MID-UINT MAX-UINT WITHIN -> <FALSE> }T
T{ MAX-UINT MID-UINT+1 0 WITHIN -> <TRUE> }T
T{ MAX-UINT MID-UINT+1 MID-UINT WITHIN -> <TRUE> }T
T{ MAX-UINT MID-UINT+1 MID-UINT+1 WITHIN -> <FALSE> }T
T{ MAX-UINT MID-UINT+1 MAX-UINT WITHIN -> <FALSE> }T
T{ MAX-UINT MAX-UINT 0 WITHIN -> <TRUE> }T
T{ MAX-UINT MAX-UINT MID-UINT WITHIN -> <TRUE> }T
T{ MAX-UINT MAX-UINT MID-UINT+1 WITHIN -> <TRUE> }T
T{ MAX-UINT MAX-UINT MAX-UINT WITHIN -> <FALSE> }T

T{ MIN-INT MIN-INT MIN-INT WITHIN -> <FALSE> }T
T{ MIN-INT MIN-INT 0 WITHIN -> <TRUE> }T
T{ MIN-INT MIN-INT 1 WITHIN -> <TRUE> }T
T{ MIN-INT MIN-INT MAX-INT WITHIN -> <TRUE> }T
T{ MIN-INT 0 MIN-INT WITHIN -> <FALSE> }T
T{ MIN-INT 0 0 WITHIN -> <FALSE> }T
T{ MIN-INT 0 1 WITHIN -> <FALSE> }T
T{ MIN-INT 0 MAX-INT WITHIN -> <FALSE> }T
T{ MIN-INT 1 MIN-INT WITHIN -> <FALSE> }T
T{ MIN-INT 1 0 WITHIN -> <TRUE> }T
T{ MIN-INT 1 1 WITHIN -> <FALSE> }T
T{ MIN-INT 1 MAX-INT WITHIN -> <FALSE> }T
T{ MIN-INT MAX-INT MIN-INT WITHIN -> <FALSE> }T
T{ MIN-INT MAX-INT 0 WITHIN -> <TRUE> }T
T{ MIN-INT MAX-INT 1 WITHIN -> <TRUE> }T
T{ MIN-INT MAX-INT MAX-INT WITHIN -> <FALSE> }T
T{ 0 MIN-INT MIN-INT WITHIN -> <FALSE> }T
T{ 0 MIN-INT 0 WITHIN -> <FALSE> }T
T{ 0 MIN-INT 1 WITHIN -> <TRUE> }T
T{ 0 MIN-INT MAX-INT WITHIN -> <TRUE> }T
T{ 0 0 MIN-INT WITHIN -> <TRUE> }T
T{ 0 0 0 WITHIN -> <FALSE> }T
T{ 0 0 1 WITHIN -> <TRUE> }T
T{ 0 0 MAX-INT WITHIN -> <TRUE> }T
T{ 0 1 MIN-INT WITHIN -> <FALSE> }T
T{ 0 1 0 WITHIN -> <FALSE> }T
T{ 0 1 1 WITHIN -> <FALSE> }T
T{ 0 1 MAX-INT WITHIN -> <FALSE> }T
T{ 0 MAX-INT MIN-INT WITHIN -> <FALSE> }T
T{ 0 MAX-INT 0 WITHIN -> <FALSE> }T
T{ 0 MAX-INT 1 WITHIN -> <TRUE> }T
T{ 0 MAX-INT MAX-INT WITHIN -> <FALSE> }T
T{ 1 MIN-INT MIN-INT WITHIN -> <FALSE> }T
T{ 1 MIN-INT 0 WITHIN -> <FALSE> }T
T{ 1 MIN-INT 1 WITHIN -> <FALSE> }T
T{ 1 MIN-INT MAX-INT WITHIN -> <TRUE> }T
T{ 1 0 MIN-INT WITHIN -> <TRUE> }T
T{ 1 0 0 WITHIN -> <FALSE> }T
T{ 1 0 1 WITHIN -> <FALSE> }T
T{ 1 0 MAX-INT WITHIN -> <TRUE> }T
T{ 1 1 MIN-INT WITHIN -> <TRUE> }T
T{ 1 1 0 WITHIN -> <TRUE> }T
T{ 1 1 1 WITHIN -> <FALSE> }T
T{ 1 1 MAX-INT WITHIN -> <TRUE> }T
T{ 1 MAX-INT MIN-INT WITHIN -> <FALSE> }T
T{ 1 MAX-INT 0 WITHIN -> <FALSE> }T
T{ 1 MAX-INT 1 WITHIN -> <FALSE> }T
T{ 1 MAX-INT MAX-INT WITHIN -> <FALSE> }T
T{ MAX-INT MIN-INT MIN-INT WITHIN -> <FALSE> }T
T{ MAX-INT MIN-INT 0 WITHIN -> <FALSE> }T
T{ MAX-INT MIN-INT 1 WITHIN -> <FALSE> }T
T{ MAX-INT MIN-INT MAX-INT WITHIN -> <FALSE> }T
T{ MAX-INT 0 MIN-INT WITHIN -> <TRUE> }T
T{ MAX-INT 0 0 WITHIN -> <FALSE> }T
T{ MAX-INT 0 1 WITHIN -> <FALSE> }T
T{ MAX-INT 0 MAX-INT WITHIN -> <FALSE> }T
T{ MAX-INT 1 MIN-INT WITHIN -> <TRUE> }T
T{ MAX-INT 1 0 WITHIN -> <TRUE> }T
T{ MAX-INT 1 1 WITHIN -> <FALSE> }T
T{ MAX-INT 1 MAX-INT WITHIN -> <FALSE> }T
T{ MAX-INT MAX-INT MIN-INT WITHIN -> <TRUE> }T
T{ MAX-INT MAX-INT 0 WITHIN -> <TRUE> }T
T{ MAX-INT MAX-INT 1 WITHIN -> <TRUE> }T
T{ MAX-INT MAX-INT MAX-INT WITHIN -> <FALSE> }T

\ -----------------------------------------------------------------------------
TESTING UNUSED   (contributed by James Bowman)

T{ UNUSED DROP -> }T

\ -----------------------------------------------------------------------------
TESTING AGAIN   (contributed by James Bowman)

T{ : ag0 701 BEGIN DUP 7 MOD 0= IF EXIT THEN 1+ AGAIN ; -> }T
T{ ag0 -> 707 }T

\ -----------------------------------------------------------------------------
TESTING MARKER   (contributed by James Bowman)

T{ : ma? BL WORD FIND NIP 0<> ; -> }T
T{ MARKER ma0 -> }T
T{ : ma1 111 ; -> }T
T{ MARKER ma2 -> }T
T{ : ma1 222 ; -> }T
T{ ma? ma0 ma? ma1 ma? ma2 -> <TRUE> <TRUE> <TRUE> }T
T{ ma1 ma2 ma1 -> 222 111 }T
T{ ma? ma0 ma? ma1 ma? ma2 -> <TRUE> <TRUE> <FALSE> }T
T{ ma0 -> }T
T{ ma? ma0 ma? ma1 ma? ma2 -> <FALSE> <FALSE> <FALSE> }T

\ -----------------------------------------------------------------------------
TESTING ?DO

: qd ?DO I LOOP ;
T{ 789 789 qd -> }T
T{ -9876 -9876 qd -> }T
T{ 5 0 qd -> 0 1 2 3 4 }T

: qd1 ?DO I 10 +LOOP ;
T{ 50 1 qd1 -> 1 11 21 31 41 }T
T{ 50 0 qd1 -> 0 10 20 30 40 }T

: qd2 ?DO I 3 > IF LEAVE ELSE I THEN LOOP ;
T{ 5 -1 qd2 -> -1 0 1 2 3 }T

: qd3 ?DO I 1 +LOOP ;
T{ 4  4 qd3 -> }T
T{ 4  1 qd3 -> 1 2 3 }T
T{ 2 -1 qd3 -> -1 0 1 }T

: qd4 ?DO I -1 +LOOP ;
T{  4 4 qd4 -> }T
T{  1 4 qd4 -> 4 3 2 1 }T
T{ -1 2 qd4 -> 2 1 0 -1 }T

: qd5 ?DO I -10 +LOOP ;
T{   1 50 qd5 -> 50 40 30 20 10 }T
T{   0 50 qd5 -> 50 40 30 20 10 0 }T
T{ -25 10 qd5 -> 10 0 -10 -20 }T

VARIABLE iters
VARIABLE incrmnt

: qd6 ( limit start increment -- )
   incrmnt !
   0 iters !
   ?DO
      1 iters +!
      I
      iters @  6 = IF LEAVE THEN
      incrmnt @
   +LOOP iters @
;

T{  4  4 -1 qd6 -> 0 }T
T{  1  4 -1 qd6 -> 4 3 2 1 4 }T
T{  4  1 -1 qd6 -> 1 0 -1 -2 -3 -4 6 }T
T{  4  1  0 qd6 -> 1 1 1 1 1 1 6 }T
T{  0  0  0 qd6 -> 0 }T
T{  1  4  0 qd6 -> 4 4 4 4 4 4 6 }T
T{  1  4  1 qd6 -> 4 5 6 7 8 9 6 }T
T{  4  1  1 qd6 -> 1 2 3 3 }T
T{  4  4  1 qd6 -> 0 }T
T{  2 -1 -1 qd6 -> -1 -2 -3 -4 -5 -6 6 }T
T{ -1  2 -1 qd6 -> 2 1 0 -1 4 }T
T{  2 -1  0 qd6 -> -1 -1 -1 -1 -1 -1 6 }T
T{ -1  2  0 qd6 -> 2 2 2 2 2 2 6 }T
T{ -1  2  1 qd6 -> 2 3 4 5 6 7 6 }T
T{  2 -1  1 qd6 -> -1 0 1 3 }T

\ -----------------------------------------------------------------------------
TESTING BUFFER:

T{ 8 BUFFER: buf:test -> }T
T{ buf:test DUP ALIGNED = -> TRUE }T
T{ 111 buf:test ! 222 buf:test cell+ ! -> }T
T{ buf:test @ buf:test cell+ @ -> 111 222 }T

\ -----------------------------------------------------------------------------
TESTING VALUE TO

T{ 111 VALUE val1 -999 VALUE val2 -> }T
T{ val1 -> 111 }T
T{ val2 -> -999 }T
T{ 222 TO val1 -> }T
T{ val1 -> 222 }T
T{ : vd1 val1 ; -> }T
T{ vd1 -> 222 }T
T{ : vd2 TO val2 ; -> }T
T{ val2 -> -999 }T
T{ -333 vd2 -> }T
T{ val2 -> -333 }T
T{ val1 -> 222 }T
T{ 123 VALUE val3 IMMEDIATE val3 -> 123 }T
T{ : vd3 val3 LITERAL ; vd3 -> 123 }T

\ -----------------------------------------------------------------------------
TESTING CASE OF ENDOF ENDCASE

: cs1 CASE 1 OF 111 ENDOF
           2 OF 222 ENDOF
           3 OF 333 ENDOF
           >R 999 R>
      ENDCASE
;

T{ 1 cs1 -> 111 }T
T{ 2 cs1 -> 222 }T
T{ 3 cs1 -> 333 }T
T{ 4 cs1 -> 999 }T

\ Nested CASE's

: cs2 >R CASE -1 OF CASE R@ 1 OF 100 ENDOF
                            2 OF 200 ENDOF
                           >R -300 R>
                    ENDCASE
                 ENDOF
              -2 OF CASE R@ 1 OF -99  ENDOF
                            >R -199 R>
                    ENDCASE
                 ENDOF
                 >R 299 R>
         ENDCASE R> DROP
;

T{ -1 1 cs2 ->  100 }T
T{ -1 2 cs2 ->  200 }T
T{ -1 3 cs2 -> -300 }T
T{ -2 1 cs2 -> -99  }T
T{ -2 2 cs2 -> -199 }T
T{  0 2 cs2 ->  299 }T

\ Boolean short circuiting using CASE

: cs3  ( n1 -- n2 )
   CASE 1- FALSE OF 11 ENDOF
        1- FALSE OF 22 ENDOF
        1- FALSE OF 33 ENDOF
        44 SWAP
   ENDCASE
;

T{ 1 cs3 -> 11 }T
T{ 2 cs3 -> 22 }T
T{ 3 cs3 -> 33 }T
T{ 9 cs3 -> 44 }T

\ Empty CASE statements with/without default

T{ : cs4 CASE ENDCASE ; 1 cs4 -> }T
T{ : cs5 CASE 2 SWAP ENDCASE ; 1 cs5 -> 2 }T
T{ : cs6 CASE 1 OF ENDOF 2 ENDCASE ; 1 cs6 -> }T
T{ : cs7 CASE 3 OF ENDOF 2 ENDCASE ; 1 cs7 -> 1 }T

\ -----------------------------------------------------------------------------
TESTING :NONAME RECURSE

VARIABLE nn1
VARIABLE nn2
:NONAME 1234 ; nn1 !
:NONAME 9876 ; nn2 !
T{ nn1 @ EXECUTE -> 1234 }T
T{ nn2 @ EXECUTE -> 9876 }T

T{ :NONAME ( n -- 0,1,..n ) DUP IF DUP >R 1- RECURSE R> THEN ;
   CONSTANT rn1 -> }T
T{ 0 rn1 EXECUTE -> 0 }T
T{ 4 rn1 EXECUTE -> 0 1 2 3 4 }T

:NONAME  ( n -- n1 )    \ Multiple RECURSEs in one definition
   1- DUP
   CASE 0 OF EXIT ENDOF
        1 OF 11 SWAP RECURSE ENDOF
        2 OF 22 SWAP RECURSE ENDOF
        3 OF 33 SWAP RECURSE ENDOF
        DROP ABS RECURSE EXIT
   ENDCASE
; CONSTANT rn2

T{  1 rn2 EXECUTE -> 0 }T
T{  2 rn2 EXECUTE -> 11 0 }T
T{  4 rn2 EXECUTE -> 33 22 11 0 }T
T{ 25 rn2 EXECUTE -> 33 22 11 0 }T

\ -----------------------------------------------------------------------------
TESTING C"

T{ : cq1 C" 123" ; -> }T
T{ cq1 COUNT EVALUATE -> 123 }T
T{ : cq2 C" " ; -> }T
T{ cq2 COUNT EVALUATE -> }T
T{ : cq3 C" 2345"COUNT EVALUATE ; cq3 -> 2345 }T

\ -----------------------------------------------------------------------------
TESTING COMPILE,

:NONAME DUP + ; CONSTANT dup+
T{ : q dup+ COMPILE, ; -> }T
T{ : as1 [ q ] ; -> }T
T{ 123 as1 -> 246 }T

\ -----------------------------------------------------------------------------
\ Cannot automatically test SAVE-INPUT and RESTORE-INPUT from a console source

TESTING SAVE-INPUT and RESTORE-INPUT with a file source

VARIABLE siv -1 siv !

: NeverExecuted
	CR ." This should never be executed" CR
;

T{ 11111 SAVE-INPUT

siv @

[IF]
	0 siv !
	RESTORE-INPUT
	NeverExecuted
	33333
[ELSE]

TESTING the -[ELSE]- part is executed
22222

[THEN]

   -> 11111 0 22222 }T	\ 0 comes from RESTORE-INPUT

TESTING SAVE-INPUT and RESTORE-INPUT with a string source

VARIABLE si_inc 0 si_inc !

: si1
	si_inc @ >IN +!
	15 si_inc !
;

: s$ S" SAVE-INPUT si1 RESTORE-INPUT 12345" ;

T{ s$ EVALUATE si_inc @ -> 0 2345 15 }T

TESTING nested SAVE-INPUT, RESTORE-INPUT and REFILL from a file

: read_a_line
	REFILL 0=
	ABORT" REFILL failed"
;

0 si_inc !

2VARIABLE 2res -1. 2res 2!

: si2
	read_a_line
	read_a_line
	SAVE-INPUT
	read_a_line
	read_a_line
	s$ EVALUATE 2res 2!
	RESTORE-INPUT
;

\ WARNING: do not delete or insert lines of text after si2 is called
\ otherwise the next test will fail

T{ si2
33333					\ This line should be ignored
2res 2@ 44444		\ RESTORE-INPUT should return to this line

55555
TESTING the nested results
 -> 0 0 2345 44444 55555 }T

\ End of warning

\ -----------------------------------------------------------------------------
TESTING .(

CR CR .( Output from .() 
T{ CR .( You should see -9876: ) -9876 . -> }T
T{ CR .( and again: ).( -9876)CR -> }T

CR CR .( On the next 2 lines you should see First then Second messages:)
T{ : dotp  CR ." Second message via ." [CHAR] " EMIT    \ Check .( is immediate
     [ CR ] .( First message via .( ) ; dotp -> }T
CR CR
T{ : imm? bl word find NIP ; imm? .( -> 1 }T

\ -----------------------------------------------------------------------------
TESTING .R and U.R - has to handle different cell sizes

\ Create some large integers
MAX-INT 73 79 */ CONSTANT li1
MIN-INT 71 73 */ CONSTANT li2

li1 0 <# #S #> NIP CONSTANT lenli1

: (.R&U.R)  ( u1 u2 -- )  \ u1 <= string length, u2 is required indentation
   TUCK + >R
   li1 OVER SPACES  . CR R@    li1 SWAP  .R CR
   li2 OVER SPACES  . CR R@ 1+ li2 SWAP  .R CR
   li1 OVER SPACES U. CR R@    li1 SWAP U.R CR
   li2 SWAP SPACES U. CR R>    li2 SWAP U.R CR
;

: .R&U.R  ( -- )
   CR ." You should see lines duplicated:" CR
   ." indented by 0 spaces" CR 0      0 (.R&U.R) CR
   ." indented by 0 spaces" CR lenli1 0 (.R&U.R) CR \ Just fits required width
   ." indented by 5 spaces" CR lenli1 5 (.R&U.R) CR
;

CR CR .( Output from .R and U.R)
T{ .R&U.R -> }T

\ -----------------------------------------------------------------------------
TESTING PAD ERASE
\ Must handle different size characters i.e. 1 CHARS >= 1 

84 CONSTANT chars/pad      \ Minimum size of PAD in chars
chars/pad CHARS CONSTANT aus/pad
: checkpad  ( caddr u ch -- f )  \ f = TRUE if u chars = ch
   SWAP 0
   ?DO
      OVER I CHARS + C@ OVER <>
      IF 2DROP UNLOOP FALSE EXIT THEN
   LOOP  
   2DROP TRUE
;

T{ PAD DROP -> }T
T{ 0 INVERT PAD C! -> }T
T{ PAD C@ CONSTANT maxchar -> }T
T{ PAD chars/pad 2DUP maxchar FILL maxchar checkpad -> TRUE }T
T{ PAD chars/pad 2DUP CHARS ERASE 0 checkpad -> TRUE }T  
T{ PAD chars/pad 2DUP maxchar FILL PAD 0 ERASE maxchar checkpad -> TRUE }T
T{ PAD 43 CHARS + 9 CHARS ERASE -> }T
T{ PAD 43 maxchar checkpad -> TRUE }T
T{ PAD 43 CHARS + 9 0 checkpad -> TRUE }T
T{ PAD 52 CHARS + chars/pad 52 - maxchar checkpad -> TRUE }T

\ Check that use of WORD and pictured numeric output do not corrupt PAD
\ Minimum size of buffers for these are 33 chars and (2*n)+2 chars respectively
\ where n is number of bits per cell

PAD chars/pad ERASE
2 BASE !
MAX-UINT MAX-UINT <# #S CHAR 1 DUP HOLD HOLD #> 2DROP
DECIMAL
BL WORD 12345678123456781234567812345678 DROP
T{ PAD chars/pad 0 checkpad -> TRUE }T

\ -----------------------------------------------------------------------------
TESTING PARSE

T{ CHAR | PARSE 1234| DUP ROT ROT EVALUATE -> 4 1234 }T
T{ CHAR ^ PARSE  23 45 ^ DUP ROT ROT EVALUATE -> 7 23 45 }T
: pa1 [CHAR] $ PARSE DUP >R PAD SWAP CHARS MOVE PAD R> ;
T{ pa1 3456
   DUP ROT ROT EVALUATE -> 4 3456 }T
T{ CHAR a PARSE a SWAP DROP -> 0 }T
T{ CHAR z PARSE
   SWAP DROP -> 0 }T
T{ CHAR " PARSE 4567 "DUP ROT ROT EVALUATE -> 5 4567 }T
 
\ -----------------------------------------------------------------------------
TESTING PARSE-NAME  (Forth 2012)
\ Adapted from the PARSE-NAME RfD tests

T{ PARSE-NAME abcd  s" abcd"  COMPARE -> 0 }T        \ No leading spaces
T{ PARSE-NAME      abcde s" abcde" COMPARE -> 0 }T   \ Leading spaces

\ Test empty parse area, new lines are necessary
T{ PARSE-NAME
  NIP -> 0 }T
\ Empty parse area with spaces after PARSE-NAME
T{ PARSE-NAME         
  NIP -> 0 }T

T{ : parse-name-test ( "name1" "name2" -- n )
    PARSE-NAME PARSE-NAME COMPARE ; -> }T
T{ parse-name-test abcd abcd -> 0 }T
T{ parse-name-test  abcd   abcd   -> 0 }T
T{ parse-name-test abcde abcdf -> -1 }T
T{ parse-name-test abcdf abcde -> 1 }T
T{ parse-name-test abcde abcde
  -> 0 }T
T{ parse-name-test abcde abcde  
  -> 0 }T

\ -----------------------------------------------------------------------------
TESTING DEFER DEFER@ DEFER! IS ACTION-OF (Forth 2012)
\ Adapted from the Forth 200X RfD tests

T{ DEFER defer1 -> }T
T{ : my-defer DEFER ; -> }T
T{ : is-defer1 IS defer1 ; -> }T
T{ : action-defer1 ACTION-OF defer1 ; -> }T
T{ : def! DEFER! ; -> }T
T{ : def@ DEFER@ ; -> }T

T{ ' * ' defer1 DEFER! -> }T
T{ 2 3 defer1 -> 6 }T
T{ ' defer1 DEFER@ -> ' * }T
T{ ' defer1 def@ -> ' * }T
T{ ACTION-OF defer1 -> ' * }T
T{ action-defer1 -> ' * }T
T{ ' + IS defer1 -> }T
T{ 1 2 defer1 -> 3 }T
T{ ' defer1 DEFER@ -> ' + }T
T{ ' defer1 def@ -> ' + }T
T{ ACTION-OF defer1 -> ' + }T
T{ action-defer1 -> ' + }T
T{ ' - is-defer1 -> }T
T{ 1 2 defer1 -> -1 }T
T{ ' defer1 DEFER@ -> ' - }T
T{ ' defer1 def@ -> ' - }T
T{ ACTION-OF defer1 -> ' - }T
T{ action-defer1 -> ' - }T

T{ my-defer defer2 -> }T
T{ ' DUP IS defer2 -> }T
T{ 1 defer2 -> 1 1 }T

\ -----------------------------------------------------------------------------
TESTING HOLDS  (Forth 2012)

: htest S" Testing HOLDS" ;
: htest2 S" works" ;
: htest3 S" Testing HOLDS works 123" ;
T{ 0. <#  htest HOLDS #> htest COMPARE -> 0 }T
T{ 123. <# #S BL HOLD htest2 HOLDS BL HOLD htest HOLDS #>  htest3 COMPARE -> 0 }T
T{ : hld HOLDS ; -> }T
T{ 0. <#  htest hld #> htest COMPARE -> 0 }T

\ -----------------------------------------------------------------------------
TESTING REFILL SOURCE-ID
\ REFILL and SOURCE-ID from the user input device can't be tested from a file,
\ can only be tested from a string via EVALUATE

T{ : rf1  S" REFILL" EVALUATE ; rf1 -> FALSE }T
T{ : sid1  S" SOURCE-ID" EVALUATE ; sid1 -> -1 }T

\ ------------------------------------------------------------------------------
TESTING S\"  (Forth 2012 compilation mode)
\ Extended the Forth 200X RfD tests
\ Note this tests the Core Ext definition of S\" which has unedfined
\ interpretation semantics. S\" in interpretation mode is tested in the tests on
\ the File-Access word set

T{ : ssq1 S\" abc" s" abc" COMPARE ; -> }T  \ No escapes
T{ ssq1 -> 0 }T
T{ : ssq2 S\" " ; ssq2 SWAP DROP -> 0 }T    \ Empty string

T{ : ssq3 S\" \a\b\e\f\l\m\q\r\t\v\x0F0\x1Fa\xaBx\z\"\\" ; -> }T
T{ ssq3 SWAP DROP          ->  20 }T    \ String length
T{ ssq3 DROP            C@ ->   7 }T    \ \a   BEL  Bell
T{ ssq3 DROP  1 CHARS + C@ ->   8 }T    \ \b   BS   Backspace
T{ ssq3 DROP  2 CHARS + C@ ->  27 }T    \ \e   ESC  Escape
T{ ssq3 DROP  3 CHARS + C@ ->  12 }T    \ \f   FF   Form feed
T{ ssq3 DROP  4 CHARS + C@ ->  10 }T    \ \l   LF   Line feed
T{ ssq3 DROP  5 CHARS + C@ ->  13 }T    \ \m        CR of CR/LF pair
T{ ssq3 DROP  6 CHARS + C@ ->  10 }T    \           LF of CR/LF pair
T{ ssq3 DROP  7 CHARS + C@ ->  34 }T    \ \q   "    Double Quote
T{ ssq3 DROP  8 CHARS + C@ ->  13 }T    \ \r   CR   Carriage Return
T{ ssq3 DROP  9 CHARS + C@ ->   9 }T    \ \t   TAB  Horizontal Tab
T{ ssq3 DROP 10 CHARS + C@ ->  11 }T    \ \v   VT   Vertical Tab
T{ ssq3 DROP 11 CHARS + C@ ->  15 }T    \ \x0F      Given Char
T{ ssq3 DROP 12 CHARS + C@ ->  48 }T    \ 0    0    Digit follow on
T{ ssq3 DROP 13 CHARS + C@ ->  31 }T    \ \x1F      Given Char
T{ ssq3 DROP 14 CHARS + C@ ->  97 }T    \ a    a    Hex follow on
T{ ssq3 DROP 15 CHARS + C@ -> 171 }T    \ \xaB      Insensitive Given Char
T{ ssq3 DROP 16 CHARS + C@ -> 120 }T    \ x    x    Non hex follow on
T{ ssq3 DROP 17 CHARS + C@ ->   0 }T    \ \z   NUL  No Character
T{ ssq3 DROP 18 CHARS + C@ ->  34 }T    \ \"   "    Double Quote
T{ ssq3 DROP 19 CHARS + C@ ->  92 }T    \ \\   \    Back Slash

\ The above does not test \n as this is a system dependent value.
\ Check it displays a new line
CR .( The next test should display:)
CR .( One line...)
CR .( another line)
T{ : ssq4 S\" \nOne line...\nanotherLine\n" type ; ssq4 -> }T

\ Test bare escapable characters appear as themselves
t{ : ssq5 s\" abeflmnqrtvxz" ; ssq5 s" abeflmnqrtvxz" COMPARE -> 0 }t

t{ : ssq6 s\" a\""2drop 1111 ; ssq6 -> 1111 }t \ Parsing behaviour

T{ : ssq7  S\" 111 : ssq8 s\\\" 222\" EVALUATE ; ssq8 333" EVALUATE ; -> }T
T{ ssq7 -> 111 222 333 }T
T{ : ssq9  S\" 11 : ssq10 s\\\" \\x32\\x32\" EVALUATE ; ssq10 33" EVALUATE ; -> }T
T{ ssq9 -> 11 22 33 }T

\ -----------------------------------------------------------------------------
CORE-EXT-ERRORS SET-ERROR-COUNT

CR .( End of Core Extension word tests) CR


