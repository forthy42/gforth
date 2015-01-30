\ fsl-utilg.fth      An auxiliary file for the Forth Scientific Library
\                    For GForth

\ Contains commonly needed definitions for the FSL modules.

\ S>F  F>S                 conversion between (single) integer and float
\ -FROT                    reverse the effect of FROT
\ cell-                    back up one cell
\ F2DUP                    FDUP two floats
\ F2DROP                   FDROP two floats
\ PI  F1.0                 floating point constants
\ dxor, dor, dand          double xor, or, and
\ sd*                      single * double = double_product
\ %                        parse next token as a FLOAT
\ v: defines use( &        for defining and settting execution vectors
\ Public: Private: Reset_Search_Order   control the visibility of words
\ INTEGER, DOUBLE          for setting up array types
\ ARRAY DARRAY             for declaring static and dynamic arrays
\ }                        for getting an ARRAY or DARRAY element address
\ &!                       for storing ARRAY aliases in a DARRAY
\ PRINT-WIDTH              number of elements per line for printing arrays
\ }IPRINT  }FPRINT         print out integer or fp arrays
\ }FCOPY                   copy one array into another
\ }FPUT                    move values from fp stack into an array
\ MATRIX  DMATRIX          for declaring a static or dynamic 2-D array
\ }}                       gets a Matrix element address
\ }}IPRINT  }}FPRINT       print out an integer or fp matrix
\ }}FCOPY                  copy one matrix into another
\ }}FPUT                   move values from fp stack into a matrix
\ FRAME| |FRAME            set up/remove a local variable frame
\ a b c d e f g h          local FVARIABLE values
\ &a &b &c &d &e &f &g &h  local FVARIABLE addresses
\    The words  F,  F=  F2*  F2/  PI  FLOAT  are already present in Gforth

\ This code is released to the public domain Everett Carter July 1994

\ CR .( FSL-UTILG.FTH    V1.17        12 Jun 1996 10:13:12      EFC )
\ CR .(  fsl-utilg.fth     V2.0         Thursday 16 October 2008  )
\    cgm:   reorganized file,
\           removed words already in Gforth,
\           Gforth DEFER and IS used for vectoring,
\           alternative definition for fp locals.

\ The code conforms with ANS requiring:
\   1. Words from the wordsets CORE, CORE-EXT, BLOCK-EXT, EXCEPTION-EXT,
\       FILE, FLOAT, FLOAT-EXT, LOCAL, SEARCH, SEARCH-EXT, and TOOLS-EXT
\   2. Gforth words  Defer  Alias  -rot  float  f,
\

BASE @ DECIMAL

\ ================= compilation control =============================

\ for control of conditional compilation of test code
FALSE VALUE TEST-CODE?
FALSE VALUE ?TEST-CODE          \ obsolete, for backward compatibility

\ for control of conditional compilation of Dynamic memory
TRUE CONSTANT HAS-MEMORY-WORDS?

\ ================= FSL NonANS words ================================

: -frot FROT FROT  ;
: cell-  [ 1 CELLS ] LITERAL - ;   \ back up one cell
: F2DUP   FOVER FOVER ;
: F2DROP  FDROP FDROP ;
1.0E0 FCONSTANT F1.0

: dxor  ( d1 d2 -- d )  ROT XOR >R XOR R>  ;          \ double xor
: dor   ( d1 d2 -- d )  ROT OR >R OR R>    ;          \ double or
: dand  ( d1 d2 -- d )  ROT AND >R AND R>  ;          \ double and

: sd*   ( multiplicand multiplier_double -- product_double )
      2 PICK * >R   UM*   R> +  ;                \ single * double = double

: % BL WORD COUNT >FLOAT 0= ABORT" NAN"
                  STATE @ IF POSTPONE FLITERAL THEN ; IMMEDIATE

\ ================= function vector definition ======================
\  use Forth200x words  DEFER  and  IS  for FSL words  v: and  defines
\  defines  is already a synonym for  IS  in Gforth

' Defer Alias v:

: use(  STATE @ IF POSTPONE ['] ELSE ' THEN ;  IMMEDIATE
: &     POSTPONE use( ; IMMEDIATE

\ ================= vocabulary management ===========================

WORDLIST CONSTANT hidden-wordlist

: Reset-Search-Order
        FORTH-WORDLIST 1 SET-ORDER
        FORTH-WORDLIST SET-CURRENT
;

: Public:
        FORTH-WORDLIST hidden-wordlist 2 SET-ORDER
        FORTH-WORDLIST SET-CURRENT
;

: Private:
        FORTH-WORDLIST hidden-wordlist 2 SET-ORDER
        hidden-wordlist SET-CURRENT
;

: Reset_Search_Order   Reset-Search-Order ;     \ for backward compatibility

\ ================= array words =====================================

0 VALUE TYPE-ID               \ for building structures
FALSE VALUE STRUCT-ARRAY?

\ for dynamically allocating a structure or array
TRUE  VALUE is-static?     \ TRUE for statically allocated structs and arrays
: dynamic ( -- )     FALSE TO is-static? ;

1 CELLS CONSTANT INTEGER        \ size of a regular integer
2 CELLS CONSTANT DOUBLE         \ size of a double integer
\  1 FLOATS CONSTANT FLOAT      \ size of a regular float
1 CELLS CONSTANT POINTER        \ size of a pointer (for readability)

\ 1-D array definition
\    -----------------------------
\    | cell_size | data area     |
\    -----------------------------

: MARRAY ( n cell_size -- | -- addr )             \ monotype array
     CREATE
       DUP , * ALLOT
     DOES> CELL+
;

\    -----------------------------
\    | id | cell_size | data area |
\    -----------------------------

: SARRAY ( n cell_size -- | -- id addr )          \ structure array
     CREATE
       TYPE-ID ,
       DUP , * ALLOT
     DOES> DUP @ SWAP [ 2 CELLS ] LITERAL +
;

: ARRAY
     STRUCT-ARRAY? IF   SARRAY FALSE TO STRUCT-ARRAY?
                   ELSE MARRAY
                   THEN
;

\ word for creation of a dynamic array (no memory allocated)

\ Monotype
\    ------------------------
\    | data_ptr | cell_size |
\    ------------------------

: DMARRAY   ( cell_size -- )  CREATE  0 , ,
                              DOES>
                                    @ CELL+
;

\ Structures
\    ----------------------------
\    | data_ptr | cell_size | id |
\    ----------------------------

: DSARRAY   ( cell_size -- )  CREATE  0 , , TYPE-ID ,
                              DOES>
                                    DUP [ 2 CELLS ] LITERAL + @ SWAP
                                    @ CELL+
;

: DARRAY   ( cell_size -- )
     STRUCT-ARRAY? IF   DSARRAY FALSE TO STRUCT-ARRAY?
                   ELSE DMARRAY
                   THEN
;

\ word for aliasing arrays,
\  typical usage:  a{ & b{ &!  sets b{ to point to a{'s data

: &!    ( addr_a &b -- )
        SWAP cell- SWAP >BODY  !
;


: }   ( addr n -- addr[n])       \ word that fetches 1-D array addresses
          OVER [ 1 CELLS ] LITERAL -  @ * + 
;

VARIABLE print-width      6 print-width !

: }iprint ( n addr -- )       \ print n elements of an integer array
        SWAP 0 DO I print-width @ MOD 0= I AND IF CR THEN
                  DUP I } @ . LOOP
        DROP
;

: }fprint ( n addr -- )       \ print n elements of a float array
        SWAP 0 DO I print-width @ MOD 0= I AND IF CR THEN
                  DUP I } F@ F. LOOP
        DROP
;

: }fcopy ( 'src 'dest n -- )         \ copy one array into another
     0 DO      OVER I } F@     DUP  I } F!    LOOP
        2DROP
;

: }fput ( r1 ... r_n n 'a -- )   \ store r1 ... r_n into array of size n
     SWAP DUP 0 ?DO   1- 2DUP 2>R } F! 2R>   LOOP  2DROP ;

\ 2-D array definition,

\ Monotype
\    ------------------------------
\    | m | cell_size |  data area |
\    ------------------------------

: MMATRIX  ( n m size -- )           \ defining word for a 2-d matrix
        CREATE
           OVER , DUP ,
           * * ALLOT
        DOES>  [ 2 CELLS ] LITERAL +
;

\ Structures
\    -----------------------------------
\    | id | m | cell_size |  data area |
\    -----------------------------------

: SMATRIX  ( n m size -- )           \ defining word for a 2-d matrix
        CREATE TYPE-ID ,
           OVER , DUP ,
           * * ALLOT
        DOES>  DUP @ TO TYPE-ID
               [ 3 CELLS ] LITERAL +
;


: MATRIX  ( n m size -- )           \ defining word for a 2-d matrix
     STRUCT-ARRAY? IF   SMATRIX FALSE TO STRUCT-ARRAY?
                   ELSE MMATRIX
                   THEN
;

: DMATRIX ( size -- )      DARRAY ;

: }}    ( addr i j -- addr[i][j] )    \ word to fetch 2-D array addresses
               >R >R
               DUP cell- cell- 2@     \ &a[0][0] size m
               R> * R> + *
               +
;

: }}iprint ( n m addr -- )       \ print nXm elements of an integer 2-D array
        ROT ROT SWAP 0 DO    DUP 0 DO    OVER J I  }} @ .
                                   LOOP
                             CR
                       LOOP
        2DROP
;


: }}fprint ( n m addr -- )       \ print nXm elements of a float 2-D array
        ROT ROT SWAP 0 DO    DUP 0 DO    OVER J I  }} F@ F.
                                   LOOP
                             CR
                       LOOP
        2DROP
;

: }}fcopy ( 'src 'dest n m  -- )      \ copy nXm elements of 2-D array src to dest
        SWAP 0 DO    DUP 0 DO    2 PICK J I  }} F@
                                 OVER   J I  }} F!
                           LOOP
               LOOP
        DROP 2DROP
;

: }}fput ( r11 r12 ... r_nm  n m 'A -- | store r11 ... r_nm into nxm matrix )
      -ROT 2DUP * >R 1- SWAP 1- SWAP }} R> 
      0 ?DO  DUP >R F! R> FLOAT -  LOOP  DROP ;

\ ================= Floating-point local variables ==================
(
  loosely based upon Wil Baden's idea presented at FORML 1992.
  The idea is to have a fixed number of variables with fixed names.

  example:  : test  2e 3e FRAME| a b |  a F. b F. |FRAME ;
            test <cr> 3.0000 2.0000 ok

  Don't forget to use |FRAME before leaving a word that uses FRAME|.
)

8 CONSTANT /FLOCALS       \ number of variables provided

: (frame) ( n -- ) FLOATS ALLOT ;
: (unframe) ( addr -- ) HERE - ALLOT ;

: FRAME|
        POSTPONE HERE POSTPONE FALIGN POSTPONE >R
        0 >R
        BEGIN   BL WORD  COUNT  1 =
                SWAP C@  [CHAR] | =
                AND 0=
        WHILE   POSTPONE F,  R> 1+ >R
        REPEAT
        /FLOCALS R> - DUP 0< ABORT" too many flocals"
        POSTPONE LITERAL  POSTPONE (frame) ; IMMEDIATE

: |FRAME ( -- ) POSTPONE R> POSTPONE  (unframe) ; IMMEDIATE

\ use a defining word to build locals   cgm
: lcl  ( n -- ) CREATE ,
                DOES>  @ FLOATS NEGATE HERE +
;

8 lcl &a      7 lcl &b      6 lcl &c      5 lcl &d
  : a &a F@ ;   : b &b F@ ;   : c &c F@ ;   : d &d F@ ; 
4 lcl &e      3 lcl &f      2 lcl &g      1 lcl &h
  : e &e F@ ;   : f &f F@ ;   : g &g F@ ;   : h &h F@ ; 

BASE !
\                   end of file
