\ To test some of the ANS Forth Programming Tools and extension wordset

\ This program was written by Gerry Jackson in 2006, with contributions from
\ others where indicated, and is in the public domain - it can be distributed
\ and/or modified in any way but please retain this notice.

\ This program is distributed in the hope that it will be useful,
\ but WITHOUT ANY WARRANTY; without even the implied warranty of
\ MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.

\ The tests are not claimed to be comprehensive or correct 

\ ------------------------------------------------------------------------------
\ Version 0.11 8 April Added tests for N>R NR> SYNONYM TRAVERSE-WORDLIST
\              NAME>COMPILE NAME>INTERPRET NAME>STRING
\         0.6  1 April 2012 Tests placed in the public domain.
\              Further tests on [IF] [ELSE] [THEN]
\         0.5  30 November 2009 <true> and <false> replaced with TRUE and FALSE
\         0.4  6 March 2009 ENDIF changed to THEN. {...} changed to T{...}T
\         0.3  20 April 2007 ANS Forth words changed to upper case
\         0.2  30 Oct 2006 updated following GForth test to avoid
\              changing stack depth during a colon definition
\         0.1  Oct 2006 First version released

\ ------------------------------------------------------------------------------
\ The tests are based on John Hayes test program

\ Words tested in this file are:
\     AHEAD [IF] [ELSE] [THEN] CS-PICK CS-ROLL [DEFINED] [UNDEFINED]
\     N>R NR> SYNONYM TRAVERSE-WORDLIST NAME>COMPILE NAME>INTERPRET
\     NAME>STRING
\     

\ Words not tested:
\     .S ? DUMP SEE WORDS
\     ;CODE ASSEMBLER BYE CODE EDITOR FORGET STATE 
\ ------------------------------------------------------------------------------
\ Assumptions and dependencies:
\     - tester.fr or ttester.fs has been loaded prior to this file
\     - testing TRAVERSE-WORDLIST uses WORDLIST SEARCH-WORDLIST GET-CURRENT
\       SET-CURRENT and FORTH-WORDLIST from the Search-order word set
\ ------------------------------------------------------------------------------

DECIMAL

\ ------------------------------------------------------------------------------
TESTING AHEAD

T{ : pt1 AHEAD 1111 2222 THEN 3333 ; -> }T
T{ pt1 -> 3333 }T

\ ------------------------------------------------------------------------------
TESTING [IF] [ELSE] [THEN]

T{ TRUE  [IF] 111 [ELSE] 222 [THEN] -> 111 }T
T{ FALSE [IF] 111 [ELSE] 222 [THEN] -> 222 }T

T{ TRUE  [IF] 1     \ Code spread over more than 1 line
             2
          [ELSE]
             3
             4
          [THEN] -> 1 2 }T
T{ FALSE [IF]
             1 2
          [ELSE]
             3 4
          [THEN] -> 3 4 }T

T{ TRUE  [IF] 1 TRUE  [IF] 2 [ELSE] 3 [THEN] [ELSE] 4 [THEN] -> 1 2 }T
T{ FALSE [IF] 1 TRUE  [IF] 2 [ELSE] 3 [THEN] [ELSE] 4 [THEN] -> 4 }T
T{ TRUE  [IF] 1 FALSE [IF] 2 [ELSE] 3 [THEN] [ELSE] 4 [THEN] -> 1 3 }T
T{ FALSE [IF] 1 FALSE [IF] 2 [ELSE] 3 [THEN] [ELSE] 4 [THEN] -> 4 }T

\ ------------------------------------------------------------------------------
TESTING immediacy of [IF] [ELSE] [THEN]

T{ : pt2 [  0 ] [IF] 1111 [ELSE] 2222 [THEN]  ; pt2 -> 2222 }T
T{ : pt3 [ -1 ] [IF] 3333 [ELSE] 4444 [THEN]  ; pt3 -> 3333 }T
: pt9 bl WORD FIND ;
T{ pt9 [IF]   NIP -> 1 }T
T{ pt9 [ELSE] NIP -> 1 }T
T{ pt9 [THEN] NIP -> 1 }T

\ -----------------------------------------------------------------------------
TESTING [IF] and [ELSE] carry out a text scan by parsing and discarding words
\ so that an [ELSE] or [THEN] in a comment or string is recognised

: pt10 REFILL DROP REFILL DROP ;

T{ 0  [IF]            \ Words ignored up to [ELSE] 2
      [THEN] -> 2 }T
T{ -1 [IF] 2 [ELSE] 3 s" [THEN] 4 pt10 ignored to end of line"
      [THEN]          \ Precaution in case [THEN] in string isn't recognised
   -> 2 4 }T

\ ------------------------------------------------------------------------------
TESTING CS-PICK and CS-ROLL

\ Test pt5 based on example in ANS document p 176.

: ?repeat
   0 CS-PICK POSTPONE UNTIL
; IMMEDIATE

VARIABLE pt4

T{ : pt5  ( n1 -- )
      pt4 !
      BEGIN
         -1 pt4 +!
         pt4 @ 4 > 0= ?repeat \ Back to BEGIN if false
         111
         pt4 @ 3 > 0= ?repeat
         222
         pt4 @ 2 > 0= ?repeat
         333
         pt4 @ 1 =
      UNTIL
; -> }T

T{ 6 pt5 -> 111 111 222 111 222 333 111 222 333 }T


T{ : ?DONE POSTPONE IF 1 CS-ROLL ; IMMEDIATE -> }T  \ Same as WHILE
T{ : pt6 
      >R
      BEGIN
         R@
      ?DONE
         R@
         R> 1- >R
      REPEAT
      R> DROP
   ; -> }T

T{ 5 pt6 -> 5 4 3 2 1 }T

: mix_up 2 CS-ROLL ; IMMEDIATE  \ cs-rot

: pt7    ( f3 f2 f1 -- ? )
   IF 1111 ROT ROT         ( -- 1111 f3 f2 )     ( cs: -- orig1 )
      IF 2222 SWAP         ( -- 1111 2222 f3 )   ( cs: -- orig1 orig2 )
         IF                                      ( cs: -- orig1 orig2 orig3 )
            3333 mix_up    ( -- 1111 2222 3333 ) ( cs: -- orig2 orig3 orig1 )
         THEN                                    ( cs: -- orig2 orig3 )
         4444        \ Hence failure of first IF comes here and falls through
      THEN                                      ( cs: -- orig2 )
      5555           \ Failure of 3rd IF comes here
   THEN                                         ( cs: -- )
   6666              \ Failure of 2nd IF comes here
;

T{ -1 -1 -1 pt7 -> 1111 2222 3333 4444 5555 6666 }T
T{  0 -1 -1 pt7 -> 1111 2222 5555 6666 }T
T{  0  0 -1 pt7 -> 1111 0    6666 }T
T{  0  0  0 pt7 -> 0    0    4444 5555 6666 }T

: [1cs-roll] 1 CS-ROLL ; IMMEDIATE

T{ : pt8 
      >R
      AHEAD 111
      BEGIN 222 
         [1cs-roll]
         THEN
         333
         R> 1- >R
         R@ 0<
      UNTIL
      R> DROP
   ; -> }T

T{ 1 pt8 -> 333 222 333 }T

\ ------------------------------------------------------------------------------
TESTING [DEFINED] [UNDEFINED]

CREATE def1

T{ [DEFINED]   def1 -> TRUE  }T
T{ [UNDEFINED] def1 -> FALSE }T
T{ [DEFINED]   12345678901234567890 -> FALSE }T
T{ [UNDEFINED] 12345678901234567890 -> TRUE  }T
T{ : def2 [DEFINED]   def1 [IF] 1 [ELSE] 2 [THEN] ; -> }T
T{ : def3 [UNDEFINED] def1 [IF] 3 [ELSE] 4 [THEN] ; -> }T
T{ def2 -> 1 }T
T{ def3 -> 4 }T

\ ------------------------------------------------------------------------------
TESTING N>R NR>

T{ : ntr  N>R -1 NR> ; -> }T
T{ 1 2 3 4 5 6 7 4 ntr -> 1 2 3 -1 4 5 6 7 4 }T
T{ 1 0 ntr -> 1 -1 0 }T
T{ : ntr2 N>R N>R -1 NR> -2 NR> ;
T{ 1 2 2 3 4 5 3 ntr2 -> -1 1 2 2 -2 3 4 5 3 }T
T{ 1 0 0 ntr2 -> 1 -1 0 -2 0 }T

\ ------------------------------------------------------------------------------
TESTING SYNONYM

: syn1 1234 ;
T{ SYNONYM new-syn1 syn1 -> }T
T{ new-syn1 -> 1234 }T
: syn2 2345 ; IMMEDIATE
T{ SYNONYM new-syn2 syn2 -> }T
T{ new-syn2 -> 2345 }T
T{ : syn3 syn2 LITERAL ; syn3 -> 2345 }T

\ ------------------------------------------------------------------------------
TESTING TRAVERSE-WORDLIST NAME>COMPILE NAME>INTERPRET NAME>STRING

GET-CURRENT CONSTANT curr-wl
WORDLIST CONSTANT trav-wl
: wdct ( n nt -- n+1 f ) DROP 1+ TRUE ;
T{ 0 ' wdct trav-wl TRAVERSE-WORDLIST -> 0 }T

trav-wl SET-CURRENT
: trav1 1 ;
T{ 0 ' wdct trav-wl TRAVERSE-WORDLIST -> 1 }T
: trav2 2 ; : trav3 3 ; : trav4 4 ; : trav5 5 ; : trav6 6 ; IMMEDIATE
curr-wl SET-CURRENT
T{ 0 ' wdct trav-wl TRAVERSE-WORDLIST -> 6 }T  \ Traverse whole wordlist

\ Terminate TRAVERSE-WORDLIST after n words & check it compiles
: (part-of-wl)  ( ct n nt -- ct+1 n-1 )  DROP DUP IF SWAP 1+ SWAP 1- THEN DUP ;
: part-of-wl  ( n -- ct 0 | ct+1 n-1)
   0 SWAP ['] (part-of-wl) trav-wl TRAVERSE-WORDLIST DROP
;
T{ 0 part-of-wl -> 0 }T
T{ 1 part-of-wl -> 1 }T
T{ 4 part-of-wl -> 4 }T
T{ 9 part-of-wl -> 6 }T  \ Traverse whole wordlist

\ Testing NAME>.. words require a name token. It will be easier to test them
\ if there is a way of obtaining the name token of a given word. To get this we
\ need a definition to compare a given name with the result of NAME>STRING.
\ The output from NAME>STRING has to be copied into a buffer and converted to a
\ known case as a given Forth system may store names as lower, upper or mixed case.

create lcbuf 32 chars allot    \ The buffer

\ Convert string to lower case and save in the buffer.

: >lowcase  ( caddr u -- caddr' u )
   32 MIN DUP >R lcbuf ROT ROT
   OVER + SWAP
   DO
      I C@ DUP [CHAR] A [CHAR] Z 1+ WITHIN IF 32 OR THEN
      OVER C! CHAR+
   LOOP DROP
   lcbuf R>
;

\ Compare string (caddr u) with name associated with nt, f=0 if the same
: name?  ( caddr u nt -- caddr u f )   \ f = true for name = (caddr u) string
   NAME>STRING >lowcase 2OVER COMPARE 0=
;

\ The word to be executed by TRAVERSE-WORDLIST
: get-nt  ( caddr u 0 nt -- caddr u nt false | caddr u 0 nt ) \ nt <> 0
   2>R R@ name? IF R> R> ELSE 2R> THEN
;

\ Get name token of (caddr u) in wordlist wid, return 0 if not present
: get-name-token  ( caddr u wid -- nt | 0 )
   0 ['] get-nt ROT TRAVERSE-WORDLIST >R 2DROP R>
;

\ Test NAME>STRING via TRAVERSE-WORDLIST
T{ S" abcde" trav-wl get-name-token 0= -> TRUE  }T \ Not in wordlist
T{ S" trav4" trav-wl get-name-token 0= -> FALSE }T

\ Test NAME>INTERPRET on a word with interpretation semantics
T{ S" trav3" trav-wl get-name-token NAME>INTERPRET EXECUTE -> 3 }T

\ Test NAME>INTERPRET on a word without interpretation semantics. It is
\ difficult to choose a suitable word because:
\    - a user cannot define one in a standard system
\    - a Forth system may choose to define interpretation semantics for a word
\      despite the standard stating they are undefined.
\ Standard words that are not likely to have interpretation semantics defined
\ could be: ; EXIT ['] [CHAR] RECURSE
\ ['] will be used since it has an equivalent in interpretation mode, if that
\ doesn't work in a given system choose another word for that system.
\ FORTH-WORDLIST is needed

T{ S" [']" FORTH-WORDLIST get-name-token NAME>INTERPRET -> 0 }T

\ Test NAME>COMPILE
: n>c  ( caddr u -- )  trav-wl get-name-token NAME>COMPILE EXECUTE ; IMMEDIATE
T{ : n>c1  ( -- n )  [ S" trav2" ] n>c ; n>c1 -> 2 }T          \ Not immediate
T{ : n>c2  ( -- n )  [ S" trav6" ] n>c LITERAL ; n>c2 -> 6 }T  \ Immediate word
T{ S" trav6" trav-wl get-name-token NAME>COMPILE EXECUTE -> 6 }T

\ Test the order of finding words with the same name
trav-wl SET-CURRENT
: trav3 33 ; : trav3 333 ; : trav7 7 ; : trav3 3333 ;
curr-wl SET-CURRENT

: get-all  ( caddr u nt -- [n] caddr u true )
   DUP >R name? IF R@ NAME>INTERPRET EXECUTE ROT ROT THEN
   R> DROP TRUE
; 

: get-all  ( caddr u -- i*x )
   ['] get-all trav-wl TRAVERSE-WORDLIST 2DROP
;

T{ S" trav3" get-all -> 3333 333 33 3 }T

\ ------------------------------------------------------------------------------

TOOLS-ERRORS SET-ERROR-COUNT

CR .( End of Programming Tools word tests) CR
