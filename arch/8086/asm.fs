\ **************************************************************
\ File:         ASM.FS
\                 8086-Assembler for PC
\ Autor:        Klaus Kohl (adaptet from volksFORTH_PC)
\ Log:          30.07.97 KK: file generated
\
\ * Register using see PRIMS.FS


include asm/basic.fs

also  Assembler Definitions

: | ;
: restrict ;
: u2/   1 rshift ;
: 8/    3 rshift ;
: 8*    3 lshift ;
: case? over = IF drop TRUE ELSE FALSE THEN ;
: (0<   $8000 and  $8000 = ;

\ 8086 registers
  0 Constant ax   1 Constant cx   2 Constant dx   3 Constant bx
  4 Constant sp   5 Constant bp   6 Constant si   7 Constant di
  8 Constant al   9 Constant cl  $a Constant dl  $b Constant bl
 $c Constant ah  $d Constant ch  $e Constant dh  $f Constant bh

 $100 Constant es          $101 Constant cs
 $102 Constant ss          $103 Constant ds

| Variable isize        ( specifies Size by prefix)
| : Size: ( n -- )  Create c,  Does>  c@ isize ! ;
  0 Size: byte      1 Size: word  word    2 Size: far


\ 8086 Assembler  System variables              ( 10.08.90/kk )
| Variable direction    \ 0 reg>EA, -1 EA>reg
| Variable size         \ 1 word, 0 byte, -1 undefined
| Variable displaced    \ 1 direct, 0 nothing, -1 displaced
| Variable displacement

| : setsize              isize @  size ! ;
| : long?   ( n -- f )   $FF80 and dup (0< invert ?exit $FF80 xor ;
| : ?range               dup long? abort" out of range" ;
| : wexit                rdrop word ;
| : moderr               word true Abort" invalid" ;
| : ?moderr ( f -- )     IF moderr THEN ;
| : ?word                size @ 1- ?moderr ;
| : far?    ( -- f )     size @ 2 = ;


\ 8086 addressing modes                         ( 24.05.91/KK )
| Create (ea  7 c, 0 c, 6 c, 4 c, 5 c,
| : ()  ( 8b1 -- 8b2 )
     3 - dup 4 u> over 1 = or ?moderr (ea + c@ ;

 -1 Constant #       $c6 Constant #)       -1 Constant c*

  : )   ( u1 -- u2 )
     () 6 case? IF  0 $86 exit  THEN  $C0 or ;
  : I)  ( u1 u2 -- u3 )  + 9 - dup 3 u> ?moderr $C0 or ;

  : D)  ( n u1 -- n u2 )
     () over long? IF  $40  ELSE  $80  THEN or ;
  : DI) ( n u1 u2 -- n u3 )
     I) over long? IF  $80  ELSE  $40  THEN xor ;

\ 8086 Registers and addressing modes             ks 25 mai 87

| : displaced?  ( [n] u1 -- [n] u1 f )
     dup #) = IF  1 exit  THEN
     dup $C0 and dup $40 = swap $80 = or ;

| : displace    ( [n] u1 -- u1 )
     displaced? ?dup
     IF displaced @ ?moderr   displaced !   swap displacement ! THEN ;

| : rmode   ( u1 -- u2 )
     1 size !  dup 8 and
     IF  size off  $FF07 and  THEN ;

| : mmode?  ( 9b - 9b f)     dup $C0 and ;

| : rmode?  ( 8b1 - 8b1 f)   mmode? $C0 = ;


\ 8086  decoding addressing modes                 ks 25 mai 87
| : 2address  ( [n] source [displ] dest -- 15b / [n] 16b )
     size on   displaced off   dup # = ?moderr   mmode?
     IF  displace False  ELSE  rmode True  THEN  direction !
     >r # case?  IF    r> $80C0 xor  size @  1+ ?exit  setsize exit
                 THEN  direction @
     IF  r> 8* >r mmode? IF  displace
         ELSE  dup 8/ 1 and  size @ = ?moderr $FF07 and  THEN
     ELSE  rmode 8*
     THEN  r> or $C0 xor ;

| : 1address  ( [displ] 9b -- 9b )
     # case? ?moderr   size on   displaced off   direction off
     mmode? IF  displace setsize  ELSE  rmode  THEN  $C0 xor ;


\ 8086 assembler                                  ks 25 mai 87
| : immediate?   ( u -- u f )  dup (0< ;

| : nonimmediate ( u -- u )    immediate? ?moderr ;

| : r/m                        7 and ;

| : reg                        $38 and ;

| : ?akku  ( u -- u ff / tf )  dup r/m 0= dup IF nip THEN ;

| : smode? ( u1 -- u1 ff / u2 tf )  dup $F00 and
     IF  dup $100 and IF  dup r/m 8* swap reg 8/
                          or $C0 or  direction off
                      THEN  True exit
     THEN  False ;

\ 8086 Registers and addressing modes             ks 25 mai 87
| : w,          size @ or  X c, ;

| : dw,         size @  or  direction @ IF  2 xor  THEN  X c, ;

| : ?word,  ( u1 f -- )  IF   X ,  exit  THEN  X c, ;

| : direct,
     displaced @
     IF  displacement @ dup long?  displaced @ 1+ or ?word, THEN ;

| : r/m,        X c,  direct, ;

| : data,       size @ ?word, ;



\ 8086 Arithmetic instructions                  ( 24.05.91/KK )
| : Arith: ( code -- )
    Create [ FORTH ] , [ Assembler ]
    Does> @ >r   2address  immediate?
     IF  rmode? IF  ?akku IF  r> size @
                              IF  5 or  X c,  X ,  wexit  THEN
                              4 or  X c,  X c, wexit  THEN THEN
         r@ or  $80 size @ or   r> (0<
         IF  size @ IF  2 pick long? 0= IF  2 or  size off  THEN
         THEN       THEN  X c,  X c, direct,  data,  wexit
     THEN  r> dw, r/m,  wexit ;

  $8000 Arith: add,     $0008 Arith: or,
  $8010 Arith: adc,     $8018 Arith: sbb,
  $0020 Arith: and,     $8028 Arith: sub,
  $0030 Arith: xor,     $8038 Arith: cmp,

\ 8086 move push pop                            ( 24.05.91/KK )
  : mov,
     2address  immediate?
     IF    rmode? IF  r/m $B0 or size @ IF  8 or  THEN
                    X c, data,  wexit
                THEN  $C6 w, r/m, data, wexit
     THEN  6 case? IF  $A2 dw, direct, wexit  THEN
     smode? IF  $8C direction @ IF  2 or  THEN  X c,  r/m, wexit
            THEN  $88 dw,  r/m,  wexit ;

| : pupo
     >r  1address  ?word
     smode? IF  reg 6 r> IF  1+  THEN  or  X c,  wexit  THEN
     rmode? IF  r/m $50 or r> or  X c,  wexit  THEN
     r> IF  $8F  ELSE  $30 or $FF  THEN  X c,  r/m, wexit ;

  : push, 0 pupo ;        : pop,  8 pupo ;

\ 8086 inc & dec , effective addresses          ( 24.05.91/KK )
| : inc/dec
     >r 1address   rmode?
     IF  size @ IF  r/m $40 or r> or  X c,  wexit  THEN
     THEN  $FE w, r> or r/m, wexit ;

  : dec,  8 inc/dec ;         : inc,  0 inc/dec ;

| : EA:  ( code -- )
    Create c,
    Does> >r 2address nonimmediate
     rmode? direction @ 0= or ?moderr r> c@  X c,  r/m, wexit ;

  $c4 EA: les,  $8d EA: lea,  $c5 EA: lds,


\ 8086  xchg  segment prefix                    ( 24.05.91/KK )
  : xchg,
   2address nonimmediate rmode?
   IF  size @ IF  dup r/m 0=
                  IF  8/ true  ELSE  dup $38 and 0=  THEN
                  IF  r/m $90 or  X c,  wexit  THEN
   THEN       THEN  $86 w, r/m, wexit ;

| : 1addr:  ( code -- )
    Create c,
    Does> c@ >r 1address $F6 w, r> or r/m, wexit ;

  $10 1addr: com,    $18 1addr: neg,
  $20 1addr: mul,    $28 1addr: imul,
  $38 1addr: idiv,   $30 1addr: div,

  : seg,  ( 8b -)
     $100 xor dup $FFFC and ?moderr  8* $26 or X c, ;

\ 8086  test not neg mul imul div idiv          ( 24.05.91/KK )
  : test,
     2address immediate?
     IF  rmode? IF  ?akku IF  $a8 w, data, wexit  THEN THEN
         $f6 w, r/m, data, wexit
     THEN  $84 w, r/m, wexit ;

| : in/out
     >r 1address setsize
     $C2 case? IF  $EC r> or w, wexit  THEN
     6 - ?moderr  $E4 r> or w,  displacement @  X c,  wexit ;

  : out, 2 in/out ;          : in,  0 in/out ;

  : int,   3 case? IF  $cc  X c,  wexit  THEN  $cd  X c,   X c,  wexit ;


\ 8086 shifts  and  string instructions         ( 24.05.91/KK )
| : Shifts:  ( code -- )
    Create c,
    Does> c@ >r C* case? >r 1address
        r> direction !  $D0 dw, r> or r/m, wexit ;

  $00 Shifts: rol,    $08 Shifts: ror,
  $10 Shifts: rcl,    $18 Shifts: rcr,
  $20 Shifts: shl,    $28 Shifts: shr,
  $38 Shifts: sar,    ' shl, Alias sal,

| : Str:  ( code -- )   Create c,
  Does> c@ setsize w, wexit ;

  $a6 Str: cmps,     $ac Str: lods,    $a4 Str: movs,
  $ae Str: scas,     $aa Str: stos,

\ implied 8086 instructions                     ( 24.05.91/KK )
  : Byte:  ( code -- )
    Create c,
    Does> c@  X c,  ;
  : Word:  ( code -- )
    Create [ FORTH ] , [ Assembler ]
    Does> @  X ,  ;

 $37 Byte: aaa,   $ad5 Word: aad,   $ad4 Word: aam,
 $3f Byte: aas,    $98 Byte: cbw,    $f8 Byte: clc,
 $fc Byte: cld,    $fa Byte: cli,    $f5 Byte: cmc,
 $99 Byte: cwd,    $27 Byte: daa,    $2f Byte: das,
 $f4 Byte: hlt,    $ce Byte: into,   $cf Byte: iret,
 $9f Byte: lahf,   $f0 Byte: lock,   $90 Byte: nop,
 $9d Byte: popf,   $9c Byte: pushf,  $9e Byte: sahf,
 $f9 Byte: stc,    $fd Byte: std,    $fb Byte: sti,
 $9b Byte: wait,   $d7 Byte: xlat,
 $c3 Byte: ret,    $cb Byte: lret,
 $f2 Byte: rep,    $f2 Byte: 0<>rep, $f3 Byte: 0=rep,

\ 8086  jmp call  conditions                    ( 24.05.91/KK )
| : jmp/call
     >r setsize # case?
     IF  far? IF  r> IF $EA ELSE $9A THEN   X c,  swap  X ,   X ,  wexit
              THEN   X here  X cell+  - r>
         IF  dup long? 0= IF  $EB  X c,   X c,  wexit  THEN  $E9
         ELSE  $E8  THEN   X c,  1-  X ,  wexit
     THEN  1address $FF  X c,  $10 or r> +
     far? IF  8 or  THEN  r/m, wexit ;
  : call,   0 jmp/call ;         : jmp,  $10 jmp/call ;

 $75 Constant 0=   $74 Constant 0<>   $79 Constant 0<
 $78 Constant 0>=  $7d Constant <     $7c Constant >=
 $7f Constant <=   $7e Constant >     $73 Constant u<
 $72 Constant u>=  $77 Constant u<=   $76 Constant u>
 $71 Constant ov   $70 Constant nov   $e1 Constant <>c0=
 $e2 Constant c0=  $e0 Constant ?c0=  $e3 Constant C0<>

\ 8086 conditional branching                    ( 24.05.91/KK )
  : +ret,    $c2  X c,   X ,  ;
  : +lret,   $ca  X c,   X ,  ;

  : IF,          X ,   X here  1- ;
  : THEN,        X here  over 1+ - ?range swap X c!  ;
  : ELSE,       $eb IF, swap THEN, ;
  : WHILE,      IF, swap ;
  : BEGIN,       X here  ;
  : UNTIL,       X c,   X here  1+ - ?range  X c,  ;
  : AGAIN,      $eb UNTIL, ;
  : REPEAT,     AGAIN, THEN, ;

  : j,          1 xor  UNTIL, ;


\ (Code)-8086   (End-Code)-8086
  : (Code)-8086
    (code)-1 ;          ' (Code)-8086 IS (code)

  : (End-Code)-8086
    (end-code)-1 ;      ' (End-Code)-8086 IS (end-code)

