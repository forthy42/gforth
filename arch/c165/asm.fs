\ Assembler für den 80C165                      ( 25.07.97/KK )
\
\ System:         KKF_PC V1.2/2
\ Änderungen:     25.07.97  KK: Umsetzung auf GFORTH
\
\                    Hinweise:
\ Dieser Assembler wurde für das KK-FORTH entwickelt und unter-
\ liegt dem Copyright von Ing. Büro Klaus Kohl. Es unterstützt
\ auch die Prozessoren 80C166 und 80C167, wobei dann die zusätz-
\ lichen Befehle nicht genutzt werden dürfen.
\ (c) 1995/1996 Ing. Büro Klaus Kohl


include asm/basic.fs

also  Assembler Definitions

: | ;
: restrict ;
: u2/ 1 rshift ;

\ Redefinitionen wegen Namenskonflikte          ( 14.01.96/KK )
| ' ,           Alias dic,
| ' c,          Alias dic,c
  ' Forth       Alias [Forth]       immediate

  Forth Definitions
  ' Assembler   Alias [Assembler]   immediate
  Assembler Definitions


\ Alias-Definitionen                            ( 06.01.96/KK )
  : asshere ( -- dp ) X here ; 	\ Zieladresse
  : ass,c   ( byte -- ) X c, ;	\ Byte compilieren
  : ass,    ( word -- ) X , ;	\ Wort compilieren, Lowbyte zuerst
  : ass@    ( addr -- w ) X @ ; \ Wort holen
  : ass@c   ( addr -- w ) X c@ ; \ Byte holen
  : ass!    ( w addr -- ) X ! ;  \ Wort schreiben
  : ass!c   ( w addr -- ) X c! ; \ Byte schreiben

\ Variablen                                     ( 25.07.97/KK )
| Create mode  4 chars allot    \ Adreßmode
| Variable offset               \ Offsetwert für Opcode
| Variable komma                \ Flag, ob Komma verwendet wurde
| Variable ssp                  \ Gespeicherte Stackposition
| Variable <ssp>                \ Stacktiefe 1. Operand
| Create $tab &20 cells allot   \ Tabelle mit Adressen

| : reset       ( -- ) \ Setzt Flags zurück
    mode 4 chars erase   0 offset !   0 komma !
    depth   dup ssp !   <ssp> ! ;

: clearstack
   depth <ssp> @ - dup 0> 
   IF 0 ?DO drop LOOP ELSE ABORT" stackschwund!" THEN ;

| : -reset?     ( ... f -- -1 | ... 0 )
    dup IF  clearstack
	    reset  true  THEN ;

  : ready       ( -- ) \ Alle Assembler-Register zurücksetzen
    reset  $tab &20 cells  erase ;

\ Tools                                         ( 06.01.96/KK )
\ | : (aerror"    ( flag -- )
\    -reset? IF   r>  $7fff  error             \ Fehler ausgeben
\            ELSE r>  count 1+  +  align >r THEN ; restrict
\ | : AError"     ( string" ; f -- ) \ Assembler-Fehlerausgabe
\    compile (aerror"   Ascii " $, ;          immediate restrict

: AError" postpone -reset? postpone abort" ; immediate

| : mode@       ( -- n ) \ nur Lowbyte holen
    mode c@ ;

| : mode!       ( n -- ) \ Modus setzen
    mode c! ;

| : +mode!      ( n -- ) \ Wert zu MODE addieren
    mode c@  +  mode! ;

\ Tests                                         ( 25.07.97/KK )
| : stack?      ( -- n )
    depth ssp @ - ;

| : ?arg        ( n -- )
    stack? 1-  -  Aerror" Falsche Anzahl von Argumenten" ;

| : ?mode0      ( -- ) \ Test, ob Mode $00
    mode@  Aerror" Komma erwartet" ;

| : ?wreg       ( -- ) \ Test auf Wortregister
    mode@  $30 $20 within  Aerror" Wortregister erwartet" ;

| : ?waddr      ( addr -- ) \ Test auf Wortadresse
    dup 1 and  AError" Nur Wortadresse erlaubt" ;

| : ?baddr      ( baddr -- bitoff )
    ?waddr
    dup $fd00 $fe00 within IF $fd00 - u2/  exit THEN
    dup $ff00 $ffe0 within IF $fe00 - u2/  exit THEN
    -1  AError" Bitadresse erwartet" ;

| : ?rel        ( addr -- rel ) \ 16Bit-Befehl erwartet
    asshere X cell+  -  X cell /
    dup $80 $ff80 within  AError" Relativsprungziel zu fern" ;

| : ?8b         ( n -- n ) \ Test auf 8Bit-Wert
    dup $ff u>  AError" Datenbyte größer als 8 Bit" ;
| ' ?8b  Alias  ?seg       \ Test auf Segment
| : ?page       ( n -- n ) \ Test auf Page (10 Bit)
    dup $3ff u>  AError" Pagenummer größer als 10 Bit" ;


\ Registerdefinition (8Bit)                     ( 30.12.95/KK )
| : Rb:         ( -- ; C: Reg -- ) \ Register definieren
    Create $10 +  dic,c
    Does>  c@ >r   0 ?arg  ?mode0                       \ Tests
              r>  +mode! ;

  $00 Rb: rl0     $01 Rb: rh0     $02 Rb: rl1     $03 Rb: rh1
  $04 Rb: rl2     $05 Rb: rh2     $06 Rb: rl3     $07 Rb: rh3
  $08 Rb: rl4     $09 Rb: rh4     $0a Rb: rl5     $0b Rb: rh5
  $0c Rb: rl6     $0d Rb: rh6     $0e Rb: rl7     $0f Rb: rh7

\ Registerdefinition (16Bit)                    ( 15.01.96/KK )
| : Rw:         ( -- ; C: Reg -- ) \ Register definieren
    Create $20 +  dic,c
    Does>  c@ >r   0 ?arg  ?mode0                       \ Tests
              r>  +mode! ;

  $00 Rw: r0      $01 Rw: r1      $02 Rw: r2      $03 Rw: r3
  $04 Rw: r4      $05 Rw: r5      $06 Rw: r6      $07 Rw: r7
  $08 Rw: r8      $09 Rw: r9      $0a Rw: r10     $0b Rw: r11
  $0c Rw: r12     $0d Rw: r13     $0e Rw: r14     $0f Rw: r15

\ Adreßmodes                                    ( 29.12.95/KK )
  : ]+      ( -- ) \ Wortregister mit Postincrement
    ?wreg   0 ?arg   $10 +mode! ;

  : -]      ( -- ) \ Wortregister mit Predecrement
    ?wreg   0 ?arg   $20 +mode! ;

  : ]       ( -- ) \ Wortregister indirekt, evtl. Displacement
    mode@  ?dup
    IF   $20 $30 within IF $30 +mode!  exit THEN
    ELSE stack? 1 =  over 1 and 0=  or IF $60 +mode!  exit THEN
    THEN -1 AError" Wortregister oder -adresse erwartet" ;

  : #]      ( -- ) \ Wortregister mit Displacement
    ?wreg   1 ?arg   $50 +mode! ;

  : s#          ( data -- data ) \ Kurzwert
    ?mode0   1 ?arg    $80 +mode! ;

  : #           ( data -- data ) \ Langwert
    ?mode0   1 ?arg   $90 +mode! ;

  : .           ( data|wreg -- data ) \ Bitadresse
    mode@  ?dup
    IF   dup $20 $30 within IF $d0 +  $a0 mode!  exit THEN
    ELSE 1 ?arg   ?baddr   $a0 mode!  exit
    THEN -1  AError" Bitadresse erwartet" ;


\ Bedingungen                                   ( 03.01.96/KK )
| : CC:         ( -- ; C: cc -- ) \ Bedingungen definieren
    Create $b0 +  dic,c
    Does>  c@ >r   0 ?arg  ?mode0                       \ Tests
              r>  +mode! ;

  $00 CC: cc_uc   $01 CC: cc_net  $02 CC: cc_z    $02 CC: cc_eq
  $03 CC: cc_nz   $03 CC: cc_ne   $04 CC: cc_v    $05 CC: cc_nv
  $06 CC: cc_n    $07 CC: cc_nn   $08 CC: cc_c    $08 CC: cc_ult
  $09 CC: cc_nc   $09 CC: cc_uge  $0a CC: cc_sgt  $0b CC: cc_sle
  $0c CC: cc_slt  $0d CC: cc_sge  $0e CC: cc_ugt  $0f CC: cc_ule


\ Lokale Labels                                 ( 30.12.95/KK )
| : $:          ( -- ; C: # -- ) \ Adresse merken
    Create 2 cells *  cell+  dic,c
    Does>  c@  $tab  +
           dup @  AError" Label schon verwendet"
           asshere swap ! ;
  $00 $:  1$:     $01 $:  2$:     $02 $:  3$:     $03 $:  4$:
  $04 $:  5$:     $05 $:  6$:     $06 $:  7$:     $07 $:  8$:
  $08 $:  9$:     $09 $: 10$:

| : $           ( -- ; C: # -- ) \ Referenz merken
    Create $c0 +  dic,c
    Does>  >r   0 ?arg   ?mode0   r> c@  +mode! ;
  $00 $   1$      $01 $   2$      $02 $   3$      $03 $   4$
  $04 $   5$      $05 $   6$      $06 $   7$      $07 $   8$
  $08 $   9$      $09 $  10$

  : check       ( -- ) \ teste $TAB
   $tab  &10 2* cells  +   $tab
   DO   i @  ?dup
        IF i cell+ @  tuck 0= AError" Label nicht definiert"
           BEGIN  dup ass@        >r
                  over swap ass!  r> ?dup 0=
           UNTIL  drop
        THEN
   4 +LOOP ready ;

\ Adreßmodes                                    ( 01.01.96/KK )
  : ,       ( -- ) \ Es folgt nächster Operand
    komma @  2 u>  AError" Komma maximal zweimal erlaubt"
    mode@  dup >r  0=
    IF       stack? 1-  AError" Ein Operand erwartet"
    ELSE r@ $60 u<  r@ $af u>  or
          IF stack?     AError" Kein Operand erlaubt"    THEN
         r@ $60 $a0 within
          IF stack? 1-  AError" Nur ein Operand erlaubt" THEN
         r@ $a0 =
          IF stack? 2 -  AError" . erwartet Bitnummer"
             dup $0f u> AError" Bitnummer ungültig"        THEN
    THEN mode 2 + c@  mode 3 + c!   mode 1+ c@  mode 2 +  c!
         r>          mode 1+  c!   0 mode!
         1 komma +!   depth  ssp ! ;

\ Tools für Umsetzung                           ( 14.01.96/KK )
| : modes@      ( -- modes ) \ alle Modes holen
    ,   stack?   dup 4 0 within AError" Stacktiefe"  2* 2*
        komma @                       or
        mode 3 + c@ $f0 and  8 lshift  or
        mode 2 +  c@ $f0 and  4 lshift  or
        mode 1+  c@ $f0 and           or ;

| : n@          ( -- m ) \ Register aus Mode+2 holen
    mode 2 + c@  $0f and ;
| : n@<<4       ( -- $m0 ) \ Register ins High-Nibble
    n@ 2* 2* 2* 2* ;
| : m@          ( -- n ) \ Register aus Mode+1 holen
    mode 1+ c@  $0f and ;
| : o@          ( -- n ) \ Register aus Mode+2 holen
    mode 3 + c@  $0f and ;

| : reg?        ( addr -- addr 0 | reg -1 ) \ Test auf Register
    dup 1 and IF 0  exit THEN              \ Keine Wortadresse
    dup $fe00 $ffe0 within IF $fe00 -  u2/  -1  exit THEN
    0 ;
| : ?reg        ( addr -- reg )
    reg? 0=  AError" Register erwartet" ;

| : code,       ( opcode -- ) \ Opcode mit Offset compilieren
    offset @ +   ass,c ;
| : codexx,     ( opcode -- ) \ Achtung: Low/Highbyte vertauscht
    offset @ +   ass, ;
| : protect,    ( opcode -- )
    offset @ +   dup ass,c  dup invert ass,c  dup ass,c  ass,c ;
| : label,      ( -- ) \ Label als letzter Operant
    asshere   m@ 2 cells * $tab +   dup @  ass,  ! ;


\ Umsetzung                                     ( 18.03.96/KK )
| : w,          ( opcode -- )
    code,   m@ $f0               +  ass,c ;
| : w0,         ( opcode -- ) \ Modus: Rw
    code,   m@ 2* 2* 2* 2*          ass,c ;
| : ww,         ( opcode -- ) \ Modus: Rw
    code,   m@  dup 2* 2* 2* 2*  +  ass,c ;
| : #d2,        ( data offset -- ) \ Modus:  data2 #
    over 5 1 within  AError" Nur 1..4 zulässig"
    $d1 ass,c   swap 1- 2* 2* 2* 2* +  code, ;
| : w,w         ( opcode -- ) \ Modus: Rw,Rw  oder  Rb,Rb
    code,   n@<<4  m@ +  ass,c ;
| : -w,w    code,  m@ 2* 2* 2* 2* n@ +  ass,c ;
| : w,w+d       ( data opcode -- ) \ Modus: [Rw],[Rw+#data16]
    w,w   ass, ;
| : -w,w+d  -w,w  ass, ;
| : w,l         ( label opcode -- )
                      code,   n@ $f0 +  ass,c   label, ;
| : w,mem       ( mem opcode -- ) \ Modus: Rw,mem
    swap ?waddr  swap code,   n@ $f0 +  ass,c   ass, ;
| : w],mem      ( mem opcode -- ) \ Modus: [Rw],mem
    swap ?waddr  swap code,   n@        ass,c   ass, ;
| : mem,w]      ( mem opcode -- ) \ Modus: mem,[Rw]
    swap ?waddr  swap code,   m@        ass,c   ass, ;

| : b,bmem      ( mem opcode -- ) \ Modus: Rb,mem
                      code,   n@ $f0 +  ass,c   ass, ;
| : mem,w       ( mem opcode -- ) \ Modus: mem,Rw
    swap ?waddr  swap code,   m@ $f0 +  ass,c   ass, ;
| : bmem,b      ( mem opcode -- ) \ Modus: mem,Rb
                      code,   m@ $f0 +  ass,c   ass, ;
| : w,#         ( data opcode -- ) \ Modus: Rw,#data16
    code,   n@ $f0 +  ass,c   ass, ;
| : w,#4        ( data4 opcode -- ) \ Modus: Rw,(s)#data4
    over &15 u> AError" Nur 0..15 erlaubt"
    code,   2* 2* 2* 2* n@ +  ass,c ;
| : b,#         ( data opcode -- ) \ Modus: Rb,#data8
    swap ?8b swap   w,# ;

| : reg,#       ( reg data opcode -- ) \ Modus: reg,#data16
    rot ?reg   swap code,   ass,c   ass, ;
| : breg,#      ( reg data opcode -- ) \ Modus: reg,#data8
    swap ?8b swap   reg,# ;

| : w,s#        ( data3 opcode -- ) \ Modus: Rw,#data3
    over 7 u> AError" Nur 0..7 erlaubt"
    code,   n@<<4  +  ass,c ;

| : w,w]        ( opcode -- ) \ Modus: Rw,[Rw] (Register<4)
    m@ 3 u>  AError" Nur Wortregister 0..3 erlaubt"
    code,   n@<<4  m@ $08 +  +  ass,c ;

| : w,w+]       ( opcode -- ) \ Modus: Rw,[Rw+] (Register<4)
    m@ 3 u>  AError" Nur Wortregister 0..3 erlaubt"
    code,   n@<<4  m@ $0c +  +  ass,c ;

| : (r|bm,bm|r  ( a1 a2 code offset -- ) \ Offset für mem,reg
    >r >r  swap reg?
    IF       r> code,  rdrop  ass,c   ass,   exit
    ELSE swap reg?
         IF  r> r> +   code,  ass,c   ass,   exit THEN
         rdrop rdrop  drop drop
    THEN -1 AError" Speicher zu Speicher nicht erlaubt" ;
| : r|bm,bm|r2  2 (r|bm,bm|r ;
| : r|bm,bm|r3  3 (r|bm,bm|r ;
| : r|bm,bm|r4  4 (r|bm,bm|r ;

| : reg,bmem    ( reg mem code -- )
    >r            swap ?reg   r> code,   ass,c   ass, ;

| : (r|m,m|r    ( a1 a2 code offset -- ) \ Offset für mem,reg
    >r >r  swap reg?
    IF       swap ?waddr swap
             r> code,  rdrop  ass,c   ass,   exit
    ELSE swap reg?
         IF  swap ?waddr swap
             r> r> +   code,  ass,c   ass,   exit THEN
         rdrop rdrop  drop drop
    THEN -1 AError" Speicher zu Speicher nicht erlaubt" ;
| : r|m,m|r2    2 (r|m,m|r ;
| : r|m,m|r3    3 (r|m,m|r ;
| : r|m,m|r4    4 (r|m,m|r ;

| : reg,mem     ( reg mem code -- )
    swap ?waddr swap   reg,bmem ;

| : bit,        ( addr bit opcode -- )
    swap 4 lshift  +  code,  ass,c ;
| : bit,bit     ( addr bit addr bit opcode -- )
    code,   4 lshift >r  ass,c   swap ass,c   r> or  ass,c ;

| : ba_rel,     ( baddr bit addr opcode -- )
    swap ?waddr X cell - ?rel  swap code,
    rot ass,c   ass,c   4 lshift ass,c ;

| : rmd,        ( mask data opcode -- )
    rot ?8b >r   swap ?8b >r   >r
    r> code,   o@ ass,c   r> ass,c   r> ass,c ;
| : amd,        ( baddr mask data opcode -- )
    rot ?8b >r   swap ?8b >r   >r   ?baddr
    r> code,   ass,c   r> ass,c   r> ass,c ;

| : segaddr,    ( seg addr opcode -- )
    >r  ?waddr  swap ?8b  r>   code,  ass,c  ass, ;

| : ca_l,       ( opcode -- )
    code,  n@<<4  ass,c   label, ;
| : ca_a,       ( addr opcode -- )
    swap ?waddr swap  code,  n@<<4  ass,c   ass, ;
| : ci_r,       ( opcode -- )
    code,  n@<<4  m@  or  ass,c ;
| : cr_a,       ( addr opcode -- )
    swap ?waddr ?rel  swap code,  ass,c ;
| : cs_a,       ( seg addr opcode -- )
    >r  ?waddr  swap ?seg  r> code,  ass,c  ass, ;

| : $dc_#d2,    ( data offset -- ) \ Modus:  data2 #
    over 5 1 within  AError" Nur 1..4 zulässig"
    $dc ass,c   swap 1- 2* 2* 2* 2* + n@ +  code, ;

| : $d7_#p_#d2, ( page data offset -- ) \ Modus:  data2 #
    >r >r  ?page  r> r>                         \ Test auf Page
    over 5 1 within  AError" Nur 1..4 zulässig"
    $d7 ass,c   swap 1- 2* 2* 2* 2* +  code,   ass, ;

| : $d7_#s_#d2, ( page data offset -- ) \ Modus:  data2 #
    >r >r  ?8b  r> r>                        \ Test auf Segment
    over 5 1 within  AError" Nur 1..4 zulässig"
    $d7 ass,c   swap 1- 2* 2* 2* 2* +  code,   ass, ;

| : reg,        ( reg opcode -- )
    swap ?reg  swap code,  ass,c ;
| : reg,l       ( reg opcode -- )
    reg, label, ;

| : $xd_rel,    ( addr opcode -- )
    swap ?waddr ?rel swap   n@<<4 +  code,   ass,c ;
| : $yd_rel,    ( addr opcode -- ) \ Bedingung negieren
    n@ 2 u< AError" cc_UC und cc_NET nicht erlaubt"
    swap ?waddr ?rel swap
    n@  1 xor  2* 2* 2* 2* +  code,   ass,c ;

| : trap7,      ( addr opcode -- )
    over $7f u> AError" Nur 0..127 erlaubt"
    code,  2* ass,c ;


\ Generierung der Befehlstabellen               ( 02.01.96/KK )
| : Table:      ( Name ; -- addr )
    Create  here   reset   0 dic,
    Does>   >r  modes@   r@ @   r> cell+
            ?DO  dup i @  =               \ Adreßmodes gleich ?
               IF drop  i cell+  unloop  dup @ swap \ code addr
                  cell+  perform  reset  exit THEN  \ ausführen
            6 +LOOP drop  -1 AError" Adreßmode nicht erlaubt" ;

| : opc:        ( routine ; ??? opcode -- )
    >r   modes@ dic,   clearstack    r> dic,   ' dic,  reset ;

| : ;Table      ( addr -- ) \ Anzahl compilieren
    here swap ! ;

| : +Table:     ( Name ; cfa offset -- )
    Create  dic,  dic,
    Does>   dup @  offset !   cell+ perform ;

| : 0Table:     ( Name ; cfa opcode -- )
    Create  dic,  dic,
    Does>   >r  stack?  mode@  or  komma @  or
             AError" Keine Parameter erwartet"
            r@ @  r> cell+  perform ;


\ Opcodes  (ohne Parameter)                     ( 06.01.96/KK )
  ' codexx,     $00cc 0Table: nop,
  ' codexx,     $00cb 0Table: ret,
  ' codexx,     $88fb 0Table: reti,
  ' codexx,     $00db 0Table: rets,

  ' protect,      $a5 0Table: diswdt,
  ' protect,      $b5 0Table: einit,
  ' protect,      $87 0Table: idle,
  ' protect,      $97 0Table: pwrdn,
  ' protect,      $b7 0Table: srst,
  ' protect,      $a7 0Table: srvwdt,


\ Opcodes  (ein Parameter)                      ( 25.03.96/KK )
  Table: atomic,  1 #                 $00  opc: #d2,     ;Table
  ' atomic, $80 +Table: extr,

  Table: cpl,     r0                  $91  opc: w0,      ;Table
  ' cpl,    $f0 +Table: neg,

  Table: cplb,    rl0                 $b1  opc: w0,      ;Table
  ' cplb,   $f0 +Table: negb,

  Table: push,  ( -- )
    r0                  $ec  opc: w,
    0                   $ec  opc: reg,
   ;Table
  ' push,   $10 +Table: pop,
  ' push,   $ff +Table: retp,


\ Opcodes  (Bitoperationen)                     ( 14.01.96/KK )
  Table: bclr,    r0 . 0              $0e  opc: bit,     ;Table
  ' bclr,   $01 +Table: bset,

  Table: band,    r0 . 0 , r0 . 0     $6a  opc: bit,bit  ;Table
  ' band,   $c0 +Table: bcmp,
  ' band,   $e0 +Table: bmov,
  ' band,   $d0 +Table: bmovn,
  ' band,   $f0 +Table: bor,
  ' band,   $10 +Table: bxor,

  Table: bfldh,
    r0    , 0 # , 0 #   $1a  opc: rmd,
    0     , 0 # , 0 #   $1a  opc: amd,
   ;Table
  ' bfldh,  $f0 +Table: bfldl,


\ Opcodes  (Shift)                              ( 06.01.96/KK )
  Table: ashr,  ( -- )
    r0    , r1          $AC  opc: w,w
    r0    , 1     #     $BC  opc: w,#4
   ;Table
  ' ashr,   $a0 +Table: shl,
  ' ashr,   $c0 +Table: shr,

  ' ashr,   $60 +Table: rol,
  ' ashr,   $80 +Table: ror,

  Table: prior,   r0 , r0             $2b  opc: w,w      ;Table


\ Opcodes  (Division und Multiplikationen)      ( 06.01.96/KK )
  Table: div,     r0                  $4b  opc: ww,      ;Table
  ' div,    $20 +Table: divl,
  ' div,    $30 +Table: divlu,
  ' div,    $10 +Table: divu,

  Table: mul,     r0 , r0             $0b  opc: w,w      ;Table
  ' mul,    $10 +Table: mulu,


\ Opcodes  (Call´s)                             ( 06.01.96/KK )
  Table: calla,
    cc_nc , 1$          $ca  opc: ca_l,
    cc_nc , 0           $ca  opc: ca_a,
   ;Table

  Table: calli,   cc_nc , r0 ]        $ab  opc: ci_r,    ;Table
  Table: callr,   0                   $bb  opc: cr_a,    ;Table
  Table: calls,   0 , 0               $da  opc: cs_a,    ;Table

  Table: pcall,
    r0    , 1$          $e2  opc: w,l
    r0    , 0           $e2  opc: w,mem
    0     , 1$          $e2  opc: reg,l
    0     , 0           $e2  opc: reg,mem
   ;Table

\ Opcodes  (Jumps und TRAP)                     ( 06.01.96/KK )
  Table: jb,      r0 . 0 , 0          $8a  opc: ba_rel,  ;Table
  ' jb,     $20 +Table: jbc,
  ' jb,     $10 +Table: jnb,
  ' jb,     $30 +Table: jnbs,

  Table: jmpr,    cc_nc , 0           $0d  opc: $xd_rel, ;Table
| Table: -jmpr,   cc_nc , 0           $0d  opc: $yd_rel, ;Table

  ' calla,  $20 +Table: jmpa,

  ' calli,  $f1 +Table: jmpi,

  Table: jmps,    0 , 0               $fa  opc: segaddr, ;Table

  Table: trap,    0 #                 $9b  opc: trap7,   ;Table


\ Opcodes  (EXTS ... EXTPR)                     ( 14.01.96/KK )
  Table: exts,  ( -- )
    r0    , 0     #     $00  opc: $dc_#d2,
    0 #   , 0     #     $00  opc: $d7_#s_#d2,
   ;Table
  ' exts,   $80 +Table: extsr,

  Table: extp,  ( -- )
    r0    , 0     #     $40  opc: $dc_#d2,
    0 #   , 0     #     $40  opc: $d7_#p_#d2,
   ;Table
  ' extp,   $80 +Table: extpr,


\ Opcodes  (ADD,-Type)                          ( 06.01.96/KK )
  Table: add,   ( -- )
    r0    , r0          $00  opc: w,w
    r0    , 0           $02  opc: w,mem
    0     , 0           $02  opc: r|m,m|r2
    0     , r0          $04  opc: mem,w
    r0    , 0     #     $06  opc: w,#
    $fe00 , 0     #     $06  opc: reg,#
    r0    , r0    ]     $08  opc: w,w]
    r0    , r0    ]+    $08  opc: w,w+]
    r0    , 0     s#    $08  opc: w,s#
   ;Table

  ' add,    $10 +Table: addc,     ' add,    $20 +Table: sub,
  ' add,    $30 +Table: subc,     ' add,    $50 +Table: xor,
  ' add,    $60 +Table: and,      ' add,    $70 +Table: or,

\ Opcodes  (ADDB-Types)                         ( 06.01.96/KK )
  Table: addb,  ( -- )
    rl0   , rl0         $01  opc: w,w
    rl0   , 0           $03  opc: b,bmem
    0     , 0           $03  opc: r|bm,bm|r2
    0     , rl0         $05  opc: bmem,b
    rl0   , 0     #     $07  opc: b,#
    $fe00 , 0     #     $07  opc: breg,#
    rl0   , r0    ]     $09  opc: w,w]
    rl0   , r0    ]+    $09  opc: w,w+]
    rl0   , 0     s#    $09  opc: w,s#
   ;Table

  ' addb,   $10 +Table: addbc,    ' addb,   $20 +Table: subb,
  ' addb,   $30 +Table: subbc,    ' addb,   $50 +Table: xorb,
  ' addb,   $60 +Table: andb,     ' addb,   $70 +Table: orb,

\ Opcodes  (CMP)                                ( 06.01.96/KK )
  Table: cmp,   ( -- )
    r0    , r0          $40  opc: w,w
    r0    , 0           $42  opc: w,mem
    $fe00 , 0           $42  opc: reg,mem
    r0    , 0     #     $46  opc: w,#
    $fe00 , 0     #     $46  opc: reg,#
    r0    , r0    ]     $48  opc: w,w]
    r0    , r0    ]+    $48  opc: w,w+]
    r0    , 0     s#    $48  opc: w,s#
   ;Table

\ Opcodes  (CMPB)                               ( 06.01.96/KK )
  Table: cmpb,  ( -- )
    rl0   , rl0         $41  opc: w,w
    rl0   , 0           $43  opc: b,bmem
    $fe00 , 0           $43  opc: reg,bmem
    rl0   , 0     #     $47  opc: b,#
    $fe00 , 0     #     $47  opc: breg,#
    rl0   , r0    ]     $49  opc: w,w]
    rl0   , r0    ]+    $49  opc: w,w+]
    rl0   , 0     s#    $49  opc: w,s#
   ;Table

\ Opcodes  (CMPD1 ... CMPI2)                    ( 06.01.96/KK )
  Table: cmpd1, ( -- )
    r0    , 0     s#    $a0  opc: w,#4
    r0    , 0           $a2  opc: w,mem
    r0    , 0     #     $a6  opc: w,#
   ;Table
  ' cmpd1,  $10 +Table: cmpd2,

  ' cmpd1,  $e0 +Table: cmpi1,
  ' cmpd1,  $f0 +Table: cmpi2,

\ Opcodes  (MOV)                                ( 18.03.96/KK )
  Table: mov,   ( -- )
    r0    , r0          $f0  opc: w,w
    r0    , 0     s#    $e0  opc: w,#4
    r0    , 0     #     $e6  opc: w,#
    $fe00 , 0     #     $e6  opc: reg,#
    r0    , r0    ]     $a8  opc: w,w
    r0    , r0    ]+    $98  opc: w,w
    r0 ]  , r0          $b8  opc: -w,w
    r0 -] , r0          $88  opc: -w,w
    r0 ]  , r0    ]     $c8  opc: w,w
    r0 ]+ , r0    ]     $d8  opc: w,w
    r0 ]  , r0    ]+    $e8  opc: w,w
    r0    , r0 0  #]    $d4  opc: w,w+d
    r0 0 #] , r0        $c4  opc: -w,w+d
    r0 ]  , 0           $84  opc: w],mem
    0     , r0 ]        $94  opc: mem,w]
    r0    , 0           $f2  opc: w,mem
    0     , 0           $f2  opc: r|m,m|r4
    0     , r0          $f6  opc: mem,w
   ;Table

\ Opcodes  (MOVB)                               ( 18.03.96/KK )
  Table: movb,  ( -- )
    rl0   , rl0         $f1  opc: w,w
    rl0   , 0     s#    $e1  opc: w,#4
    rl0   , 0     #     $e7  opc: b,#
    $fe00 , 0     #     $e7  opc: breg,#
    rl0   , r0    ]     $a9  opc: w,w
    rl0   , r0    ]+    $99  opc: w,w
    r0 ]  , rl0         $b9  opc: -w,w
    r0 -] , rl0         $89  opc: -w,w
    r0 ]  , r0    ]     $c9  opc: w,w
    r0 ]+ , r0    ]     $d9  opc: w,w
    r0 ]  , r0    ]+    $e9  opc: w,w
    rl0   , r0 0  #]    $f4  opc: w,w+d
    r0 0 #] , rl0       $e4  opc: -w,w+d
    r0 ]  , 0           $a4  opc: w],mem
    0     , r0 ]        $b4  opc: mem,w]
    rl0   , 0           $f3  opc: b,bmem
    0     , 0           $f3  opc: r|bm,bm|r4
    0     , rl0         $f7  opc: bmem,b
   ;Table

\ Opcodes  (MOVBS  MOVBZ  SCXT)                 ( 14.01.96/KK )
  Table: movbs, ( -- )
    r0    , rl0         $d0  opc: w,w
    r0    , 0           $d2  opc: b,bmem
    0     , 0           $d2  opc: r|m,m|r3
    0     , rl0         $d5  opc: bmem,b
   ;Table
  ' movbs,  $f0 +Table: movbz,

  Table: scxt,  ( -- )
    r0    , 0     #     $c6  opc: w,#
    $fe00 , 0     #     $c6  opc: reg,#
    r0    , 0           $d6  opc: w,mem
    0     , 0           $d6  opc: reg,mem
   ;Table


\ Zusätze für Kontrollstrukturen                ( 25.07.97/KK )
| : >jrcc       ( -- ) \ JMPR mit Bedingung compilieren
    ,  asshere X cell+  -jmpr, ;
| : <jrcc       ( addr flag -- addr flag )
    ,  over        -jmpr, ;

| : -?rel       ( addr -- rel ) \ 16Bit-Befehl erwartet
    asshere  swap - X cell /
    dup $80 $ff80 within  AError" Relativsprungziel zu fern" ;
| : >jrres      ( addr -- )
    dup -?rel  swap 1- ass!c ;
| : >jrresume   ( 2 [ addr -2 ] -- )
    BEGIN -2 = WHILE >jrres REPEAT  reset ;

: ?pairs - ABORT" C165: unstructured!" ;

\ Kontrollstrukturen                            ( 06.01.96/KK )
  : IF,         ( -- addr 1 ) \ Bedingung erwartet
    >jrcc  asshere  1  reset ;

  : ELSE,       ( addr 1 -- addr2 -1 ) \ Nichts vorher erlaubt
    dup 1 ?pairs                                         \ Test
    cc_UC , asshere X cell+ jmpr,       \ unbedingter Vorwärtssprung
    drop >jrres                                  \ IF, auflösen
    asshere  -1  reset ;              \ Flags/Adresse für THEN,

  : THEN,       ( addr 1|addr2 -1 -- )
    dup abs 1 ?pairs
    drop >jrres  reset ;


  : BEGIN,      ( -- 2 addr 2 )
    2 asshere 2  reset ;

  : WHILE,      ( addr 2 -- addr2 -2 addr 2 )
    dup 2 ?pairs
    >jrcc  asshere -2  2swap  reset ;

  : REPEAT,     ( 2 [ addr2 -2 ] addr 2 -- )
    dup 2 ?pairs
    drop >r   depth 2 + ssp !   cc_UC ,  r> jmpr,
    >jrresume ;

  : UNTIL,      ( 2 [ addr2 -2 ] addr 2 -- )
    dup 2 ?pairs   <jrcc   2drop >jrresume ;



\ Register des 80C165                           ( 03.02.96/KK )
  $f108 Constant _rp0h          \ System Startup Conf. R.
  $ff12 Constant _syscon        \ CPU System Conf. R.
  $ff10 Constant _psw           \ CPU Program Status Word
  $fe10 Constant _cp            \ CPU Context Ptr R.

  $ffae Constant _wdtcon        \ Watchdog Timer C.R.
  $feae Constant _wdt           \ Watchdog Timer R.

  $f1c0 Constant _exicon        \ External Interrupt C.R.
  $f186 Constant _xp0ic         \ X-Peripheral 0 Interrupt C.R.
  $f18e Constant _xp1ic         \ X-Peripheral 1 Interrupt C.R.
  $f196 Constant _xp2ic         \ X-Peripheral 2 Interrupt C.R.
  $f19e Constant _xp3ic         \ X-Peripheral 3 Interrupt C.R.

  $ffac Constant _tfr           \ Trap Flag R.

\ Register des 80C165 (Stack, Page Ptr)         ( 03.02.96/KK )
  $fe12 Constant _sp            \ CPU System Stack Ptr R.
  $fe14 Constant _stkov         \ CPU Stack Overflow Ptr R.
  $fe16 Constant _stkun         \ CPU Stack Underflow Ptr R.

  $fe08 Constant _csp           \ CPU Code Segment Ptr R.
  $fe00 Constant _dpp0          \ CPU Data Page Ptr 0 R.
  $fe02 Constant _dpp1          \ CPU Data Page Ptr 1 R.
  $fe04 Constant _dpp2          \ CPU Data Page Ptr 2 R.
  $fe06 Constant _dpp3          \ CPU Data Page Ptr 3 R.

\ Register des 80C165 (PEC, 0, -1, MD)          ( 03.02.96/KK )
  $fec0 Constant _pecc0         \ PEC Channel 0 C.R.
  $fec2 Constant _pecc1         \ PEC Channel 1 C.R.
  $fec4 Constant _pecc2         \ PEC Channel 2 C.R.
  $fec6 Constant _pecc3         \ PEC Channel 3 C.R.
  $fec8 Constant _pecc4         \ PEC Channel 4 C.R.
  $feca Constant _pecc5         \ PEC Channel 5 C.R.
  $fecc Constant _pecc6         \ PEC Channel 6 C.R.
  $fece Constant _pecc7         \ PEC Channel 7 C.R.

  $ff1c Constant _zeros         \ Constant 0
  $ff1e Constant _ones          \ Constant $ffff

  $ff0e Constant _mdc           \ CPU Multiply Divide C.R.
  $fe0c Constant _mdh           \ CPU Multiply Divide R. (High)
  $fe0e Constant _mdl           \ CPU Multiply Divide R. (Low)

\ Register des 80C165                           ( 03.02.96/KK )
  $ff0c Constant _buscon0       \ Bus Configuration R. 0
  $ff14 Constant _buscon1       \ Bus Conf. R. 1
  $ff16 Constant _buscon2       \ Bus Conf. R. 2
  $ff18 Constant _buscon3       \ Bus Conf. R. 3
  $ff1a Constant _buscon4       \ Bus Conf. R. 4

  $fe18 Constant _addrsel1      \ Adress Select R. 1
  $fe1a Constant _addrsel2      \ Adress Select R. 2
  $fe1c Constant _addrsel3      \ Adress Select R. 3
  $fe1e Constant _addrsel4      \ Adress Select R. 4

  $fe4a Constant _caprel        \ GPT1 Capture/Reload R.
  $ff6a Constant _cric          \ GPT2 CAPREL Interrupt C.R.

\ Register des 80C165 (Timer)                   ( 03.02.96/KK )
  $ff40 Constant _t2con         \ GPT1 Timer 2 C.R.
  $ff42 Constant _t3con         \ GPT1 Timer 3 C.R.
  $ff44 Constant _t4con         \ GPT1 Timer 4 C.R.
  $ff46 Constant _t5con         \ GPT2 Timer 5 C.R.
  $ff48 Constant _t6con         \ GPT2 Timer 6 C.R.
  $ff60 Constant _t2ic          \ GPT1 Timer 2 Interrupt C.R.
  $ff62 Constant _t3ic          \ GPT1 Timer 3 Interrupt C.R.
  $ff64 Constant _t4ic          \ GPT1 Timer 4 Interrupt C.R.
  $ff66 Constant _t5ic          \ GPT2 Timer 5 Interrupt C.R.
  $ff68 Constant _t6ic          \ GPT2 Timer 6 Interrupt C.R.
  $fe40 Constant _t2            \ GPT1 Timer 2 R.
  $fe42 Constant _t3            \ GPT1 Timer 3 R.
  $fe44 Constant _t4            \ GPT1 Timer 4 R.
  $fe46 Constant _t5            \ GPT1 Timer 5 R.
  $fe48 Constant _t6            \ GPT1 Timer 6 R.

\ Register des 80C165 (Ports)                   ( 03.02.96/KK )
  $f100 Constant _dp0l          \ P0L Direction Control Register
  $ff00 Constant _p0l           \ Port 0 Low R.
  $f102 Constant _dp0h          \ P0H Direction C.R.
  $ff02 Constant _p0h           \ Port 0 High R.

  $f104 Constant _dp1l          \ P1L Direction C.R.
  $ff04 Constant _p1l           \ Port 1 Low R.
  $f106 Constant _dp1h          \ P1H Direction C.R.
  $ff06 Constant _p1h           \ Port 1 High R.

  $ffc2 Constant _dp2           \ Port 2 Direction C.R.
  $f1c2 Constant _odp2          \ Port 2 Open Drain C.R
  $ffc0 Constant _p2            \ Port 2 R.

  $ffc6 Constant _dp3           \ Port 3 Direction C.R.
  $f1c6 Constant _odp3          \ Port 3 Open Drain C.R
  $ffc4 Constant _p3            \ Port 3 R.

  $ffca Constant _dp4           \ Port 4 Direction C.R.
  $ffc8 Constant _p4            \ Port 4 R. (8 Bit)

  $ffa2 Constant _p5            \ Port 5 R.

  $ffce Constant _dp6           \ Port 6 Direction C.R.
  $f1ce Constant _odp6          \ Port 6 Open Drain C.R
  $ffcc Constant _p6            \ Port 6 R. (8 Bit)

\ Register des 80C165 (SC0 und SCC)             ( 03.02.96/KK )
  $feb0 Constant _s0tbuf        \ SC0 TX Data
  $feb2 Constant _s0rbuf        \ SC0 RX Data
  $feb4 Constant _s0bg          \ SC0 Baud Rate Generator Reload
  $f19c Constant _s0tbic        \ SC0 TX Interrupt C.R.
  $ff6c Constant _s0tic         \ SC0 TX Interrupt C.R.
  $ff6e Constant _s0ric         \ SC0 RX Interrupt C.R.
  $ff70 Constant _s0eic         \ SC0 Error Interrupt C.R.
  $ffb0 Constant _s0con         \ SC0 C.R.
  $f0b0 Constant _ssctb         \ SSC Transmit Buffer
  $f0b2 Constant _sscrb         \ SSC Receive Buffer
  $f0b4 Constant _sscbr         \ SSC Baudrate Register
  $ff72 Constant _ssctic        \ SSC TX Interrupt C.R.
  $ff74 Constant _sscric        \ SSC RX Interrupt C.R.
  $ff76 Constant _ssceic        \ SSC Error Interrupt C.R.
  $ffb2 Constant _ssccon        \ SSC C.R.

\ End-Proc   End-Code                           ( 02.01.96/KK )
\  : End-Proc    ( -- )
\    check  reveal  Forth ;

\  : End-Code    ( -- )
\    [compile] end-proc ;

\ ;Code  Proc  Code                             ( 25.07.97/KK )

\ forth definitions

\  : ;Code       ( 0 -- )
\    dup  0 ?pairs  drop
\    compile (;code   [compile] [
\    [Assembler] ready [Forth]   Assembler ;  immediate

\  : Proc        ( name ; -- )
\    Create  hide
\    [Assembler] ready [Forth]   Assembler ;

: (Code)-c165  ( name ; -- )
    (code)-1 ready ; 	' (code)-c165 IS (code)

: (end-code)-c165    
    check (end-code)-1 ;

previous definitions

\ Beschreibung der Adreßmodes                   ( 25.07.97/KK )
\ mode ->  $00xx : Nur ein Operant (kein Komma)
\          $xxyy : xx=1. Mode ; yy=2. Mode
\
\ Mode  Stack  Typ                Mode  Stack  Typ
\ $00    0/1   ---                $1n     0    Byteregister
\ $2n     0    Wortregister       $3n     0    [Wortregister+]
\ $4n     0    [-Wortregister]    $5n     0    [Wortregister]
\ $60     1    [Memory]           $7n     1    [Rw+d]
\ $80     1    3Bit-Wert          $90     1    Wert
\ $A0     2    Bitadresse         $Bc     0    Bedingung
\ $Cn     0    Label

\ -------------------------------------------------------------
\ Logbuch                                       ( 25.07.97/KK )
\
\ 27.12.95  KK: File angefangen
\ 06.01.95  KK: Assembler fertig
\ 03.02.96  KK: Register hinzugefügt
\ 12.02.96  KK: Korrektur in JB, (Sprung+2) und 0Table:
\ 18.03.96  KK: IP auf Register R3 gelegt (wegen BRANCH)
\               Korrekturen in MOV, und MOVB, (n<>m)
\ 25.03.96  KK: POP korrigiert (Offset $10 statt $6B)
\               w,w] korrigiert ($08 statt $04 addieren)
\ 04.01.96  KK: Opcode für JMPS korrigiert
\ 12.02.96  KK: Korrekturen
\ 25.07.97  KK: Anpassung an GFORTH (BLK->Seq.)




