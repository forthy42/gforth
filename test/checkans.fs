\ CHECKANS.STR ANS Forth wordset checker                01may93jaw

\ 1-3MAY93 Jens A. Wilke
\ This program is public domain

DECIMAL

VARIABLE CharCount
30 CONSTANT MaxChars
VARIABLE Flag

CREATE Names 125 CELLS ALLOT
VARIABLE PNT Names PNT !

: INIT TRUE Flag ! 0 CharCount ! ;

: ^     PNT @ DUP @ 1+ SWAP !
        BL WORD FIND
        0= IF PNT @ CELL+ DUP @ 1+ SWAP !
              Flag @ IF CR ." Missing: " FALSE Flag ! THEN
              COUNT DUP CharCount +! TYPE SPACE
              CharCount @ MaxChars U< 0= IF CR 9 SPACES 0 CharCount ! THEN
           ELSE DROP THEN ;

: PLACE ( adr cnt adr -- ) 2DUP C! 1+ SWAP MOVE ;

: WS    INIT
        PNT @ 2 CELLS + PNT !
        BL WORD
        CR CR ." Checking " DUP COUNT TYPE ."  wordset..."
        DUP COUNT PNT @ PLACE COUNT SWAP DROP 1+
        PNT @ + ALIGNED DUP PNT !
        DUP 0 SWAP ! CELL+ 0 SWAP ! ;

S" ./../wordsets.fs" INCLUDED

: END
        CR CR ." Wordset:            Status:  Words:" CR

        Names 2 CELLS +
        BEGIN
                DUP COUNT TYPE
                DUP COUNT SWAP DROP 20 SWAP - SPACES
                COUNT + ALIGNED
                DUP @ OVER CELL+ @
                2DUP 0=
                IF ." complete " . DROP DROP
                ELSE OVER =
                 IF ." missing  " . DROP
                 ELSE ." partial  " OVER SWAP - . ." / " .
                 THEN
                THEN CR
                2 CELLS +
                DUP PNT @ U< 0=
        UNTIL DROP ;

END
