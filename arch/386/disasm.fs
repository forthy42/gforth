\ 80386 Disassembler
\ Andrew McKewan, April 1994
\ Tom Zimmer,  05/18/94 port to Win32f
\ Modified to word in decimal 08/03/94 10:04 tjz
\ 06-??-95 SMuB NEXT sequence defined in FKERNEL
\ 06-21-95 SMuB removed redundant COUNT calls from txb, lxs.
\ 04-??-97 Extended by C.L. to include P6 and MMX instructions

\ cr .( Loading 80486 Disassembler...)

\ Gforth stuff

260 chars constant maxstring

128 CONSTANT SPCS-MAX  ( optimization for SPACES )

CREATE SPCS         SPCS-MAX ALLOT
       SPCS         SPCS-MAX BLANK

      255 CONSTANT MAXCOUNTED   \ maximum length of contents of a counted string

\ : cincr ( c-addr -- )
\     dup c@ 1+ swap c! ;
       
\ : C+PLACE       ( c1 a1 -- )    \ append char c1 to the counted string at a1
\     dup cincr count + 1- c! ;

: (D.)          ( d -- addr len )       TUCK DABS  <# #S ROT SIGN #> ;

\ : nfa-count ( nfa -- addr u )
\     name>string ;

: w@ ( addr -- w )
    @ $ffff and ;

: col ( n -- ) drop space ;

\ original stuff

only forth also definitions

0 value default-16bit?

: default-16bit ( -- )
                true to default-16bit? ;

: default-32bit ( -- )
                false to default-16bit? ;

defer show-name   ( cfa -- )      \ display nearest symbol
' abort is show-name

0 value base-addr

vocabulary disassembler
disassembler also definitions

decimal

create s-buf MAXSTRING allot

: >s            ( a1 n1 -- )
                s-buf +place ;

: 0>s           ( -- )  \ reset s-buf
                s-buf off ;

: sspaces       ( n1 -- )
                0 max spcs swap s-buf +place ;

: sspace        ( -- )
                1 sspaces ;

\ : emit>s        ( c1 -- )
\                 s-buf c+place ;

\ : s>            ( -- a1 n1 )
\                 s-buf count ;

: .s"           ( 'text' -- )
    [char] " parse POSTPONE sliteral POSTPONE s-buf POSTPONE +place ; immediate

: d.r>s         ( d w -- )
                >r (d.) r> over - sspaces >s ;

: .r>s          ( n w -- )
                >r  s>d  r>  d.r>s ;

\ : u.r>s         ( u w -- )
\                 0 swap d.r>s ;

: h.>s          ( u -- )
                base @ swap  hex 0 (d.) >s sspace   base ! ;

\ : h.r>s           ( n1 n2 -- )
\                 base @ >r hex >r
\                 0 <# #s #> r> over - sspaces >s
\                 r> base ! ;

\ : .id>s         ( nfa -- )
\                 nfa-count >s sspace ;

\ : .name>s       ( xt -- )
\                 dup >name dup 0=     \ if not found
\                 if      drop [char] " emit>s .s" 0x" 1 h.r>s [char] " emit>s sspace
\                 else    .id>s drop
\                 then    ;

\ : ?.name>s      ( cfa -- )
\ \ eliminate " 0x"
\                 dup ?name ?dup
\                 if      .name>s
\                 else    dup 1 h.r>s sspace
\                 then    drop ;

\ ' ?.name>s is show-name
' h.>s is show-name

32 constant comment-col

0 value size
0 value 16-bit-data
0 value 16-bit-addr
0 value prefix-op
0 value mmx-reg

: @+  ( addr -- addr n )  dup cell+ swap @ ;
: W@+ ( addr -- addr n )  dup 2 + swap W@ ;

: sext  ( byte -- n )  dup $80 and if $FFFFFF00 or then ;
: mod/sib ( mod-r-r/m -- r/m r mod ) \ r including general, special, segment, MMX
          ( mod-op-r/m -- r/m op mod )
          ( s-i-b -- b i s )
          255 and 8 /mod 8 /mod ;

: ???   ( n1 -- )
        .s" ??? " drop ;

: ss. ( n adr len w )  >r drop  swap r@ * +  r> >s sspace ;

: tttn ( code -- ) 15 and S" o nob aee nebea s nsp npl geleg " 2 ss. ;

: sreg  ( sreg -- )  3 rshift 7 and S" escsssdsfsgsXXXX" 2 ss. ;
: creg  ( eee --  )  3 rshift 7 and S" cr0???cr2cr3cr4?????????" 3 ss. ;
: dreg  ( eee --  )  3 rshift 7 and S" dr0dr1dr2dr3??????dr6dr7" 3 ss. ;
: treg  ( eee --  )  3 rshift 7 and S" ?????????tr3tr4tr5tr6tr7" 3 ss. ; \ obsolete
: mreg  ( n -- )  7 and S" mm0mm1mm2mm3mm4mm5mm6mm7" 3 ss. ;

: reg8  ( n -- )  7 and S" alcldlblahchdhbh" 2 ss. ;
: reg16 ( n -- )  7 and S" axcxdxbxspbpsidi" 2 ss. ;
: reg32 ( n -- )  7 and S" eaxecxedxebxespebpesiedi" 3 ss. ;
: reg16/32      ( n -- )
                16-bit-data
                if   reg16
                else reg32
                then ;
: reg   ( a n -- a )
        mmx-reg
        if   mreg
        else size
             if   reg16/32
             else reg8
             then
        then ;

: [base16] ( r/m -- )   4 - S" [si][di][bp][bx]" 4 ss. ;
                        \ r/m = 4 , 5 , 6 , 7
: [ind16]  ( r/m -- )   S" [bx+si][bx+di][bp+si][bp+di]" 7 ss. ;
                        \ r/m = 0  ,   1  ,   2  ,   3
: [reg16]  ( r/m -- )   dup 4 <
                        if    [ind16]
                        else  [base16]
                        then ;
: [reg32]  ( n -- )     7 and S" [eax][ecx][edx][ebx][esp][ebp][esi][edi]" 5 ss. ;

\ : [reg]    ( r/m -- )  16-bit-addr
\                        if   [reg16]
\                        else [reg32]
\                        then sspace ;

\ : [reg] ( n -- )
\        7 and
\        16-bit-addr
\        if      S" [bx+si] [bx+di] [bp+si] [bp+di] [si] [di] [bp] [bx]"
\                rot 0
\                ?do     bl skip bl scan
\                loop    bl skip 2dup bl scan nip - >s 2 sspaces
\        else    S" [eax][ecx][edx][ebx][esp][ebp][esi][edi]" 5 ss. sspace
\        then    ;

: [reg*2]  ( i -- )     S" [eax*2][ecx*2][edx*2][ebx*2][XXX*2][ebp*2][esi*2][edi*2]" 7 ss. ;
: [reg*4]  ( i -- )     S" [eax*4][ecx*4][edx*4][ebx*4][XXX*4][ebp*4][esi*4][edi*4]" 7 ss. ;
: [reg*8]  ( i -- )     S" [eax*8][ecx*8][edx*8][ebx*8][XXX*8][ebp*8][esi*8][edi*8]" 7 ss. ;
: [index]  ( sib -- )   mod/sib over 4 =
                        if    2drop                     \ no esp scaled index
                        else  case ( s )
                                0 of [reg32] endof
                                1 of [reg*2] endof
                                2 of [reg*4] endof
                                3 of [reg*8] endof
                              endcase
                        then drop ;

: disp8  ( adr -- adr' )  count h.>s ;
: disp16 ( adr -- adr' )  w@+ show-name ;
: disp32 ( adr -- adr' ) @+ ( body> ) show-name ;
: disp16/32 ( adr -- adr' )
            16-bit-addr
            if   disp16
            else disp32
            then ;

: .,     ( -- )           .s" , " ;

: .#  ., .s" # " ;

: imm8   ( adr -- adr' )  .# count h.>s ;

\ : imm16  ( adr -- adr' )  .# w@+ h.>s ;

: imm16/32  ( adr -- adr' )
        .# 16-bit-data
        IF   w@+
        ELSE @+
        THEN h.>s ;

: sib   ( adr mod -- adr )
        >r count tuck 7 and 5 = r@ 0= and
        if    disp32 swap [index] r> drop       \ ebp base and mod = 00
        else  r> case ( mod )
                   1 of disp8  endof
                   2 of disp32 endof
                 endcase
              swap dup [reg32] [index]
        then ;

\ : [*]  ( sib --  )
\       .s" sib = " h.>s ;

\ : sib ( adr ext -- adr' )
\ ?? wrong version
\        swap count >r swap  6 rshift 3 and
\        ?dup if 1 = if disp8 else disp32 then then
\        r> dup 7 and dup 5 =
\        if      drop [*]
\        else    [reg]
\                dup $38 and $20 =
\                if      drop
\                else    .s" [" dup 3 rshift reg32 -1 s-buf c+!
\                        5 rshift 6 and
\                        dup 6 = if 2 + then
\                       ?dup if .s" *" 0 .r>s  then .s" ] "
\                then
\        then ;

: mod-r/m32     ( adr r/m mod -- adr' )
                dup 3 =
                if    drop reg                          \ mod = 3, register case
		else  size IF  16-bit-data 0= IF .s" d" THEN .s" word"
		      ELSE  .s" byte" THEN .s"  ptr "
		      over 4 =
                      if nip sib                        \ r/m = 4, sib case
                      else  2dup 0= swap 5 = and        \ mod = 0, r/m = 5,
                            if 2drop disp32             \ disp32 case
                            else rot swap
                                 case ( mod )
                                   1 of disp8  endof
                                   2 of disp32 endof
                                 endcase
                                 swap [reg32]
                            then
                      then
                then ;

: mod-r/m16     ( adr r/m mod -- adr' )
                2dup 0= swap 6 = and
                if   2drop disp16                       \ disp16 case
                else case ( mod )
                       0 of [reg16]                     endof
                       1 of swap disp8  swap [reg16]    endof
                       2 of swap disp16 swap [reg16]    endof
                       3 of reg                         endof
                     endcase
                then ;

: mod-r/m ( adr modr/m -- adr' )
          mod/sib nip 16-bit-addr
          if    mod-r/m16
          else  mod-r/m32
          then ;

\ : mod-r/m  ( adr ext -- adr' )
\        dup 0xC7 and 5 =                \ 32bit displacement
\        16-bit-addr 0= and              \ and not 16bit addressing
\        if      drop disp32 .s" [] "
\                EXIT
\        then
\        dup 0xC0 and 0xC0 < over 7 and 4 = and
\        16-bit-addr 0= and              \ and not 16bit addressing
\        if      sib
\                EXIT
\        then
\        dup 0xC7 and 6 =                \ 16bit displacement
\        16-bit-addr and                 \ and 16bit addressing
\        if      drop disp32 .s" [] "
\                EXIT
\        then
\        dup 6 rshift
\        case
\          0 of  .s" 0 " [reg]  endof
\          1 of  swap disp8  swap [reg]  endof
\          2 of  swap disp32 swap [reg]  endof
\          3 of  reg  endof
\        endcase ;




: r/m8      0 to size mod-r/m ;
: r/m16/32  1 to size mod-r/m ;
: r/m16     true to 16-bit-data r/m16/32 ;

: r,r/m  ( adr -- adr' )
        count dup 3 rshift reg ., mod-r/m ;

: r/m,r  ( adr -- adr' )
        count dup >r mod-r/m ., r> 3 rshift reg ;

: r/m  ( adr op -- adr' )
        2 and if r,r/m else r/m,r then ;

\ -------------------- Simple Opcodes --------------------

: inh   ( -<name>- )
        create
        bl word count here place
        here c@ 1+ allot
        does> count >s sspace drop ;

inh clc  clc
inh stc  stc
inh cld  cld
inh std  std
\ inh rpnz repnz
\ inh repz repz
inh cbw  cbw
inh cdq  cdq
inh daa  daa
inh das  das
inh aaa  aaa
inh aas  aas
\ inh lock lock
inh inb  insb
inh osb  outsb
inh sah  sahf
inh lah  lahf
\ inh aam  aam
\ inh aad  aad
inh hlt  hlt
inh cmc  cmc
inh xlt  xlat
inh cli  cli
inh sti  sti

inh clt clts
inh inv invd
inh wiv wbinvd
inh ud2 ud2
inh wmr wrmsr
inh rtc rdtsc
inh rmr rdmsr
inh rpc rdpmc
inh ems emms
inh rsm rsm
inh cpu cpuid
inh ud1 ud1
\ inh lss lss
\ inh lfs lfs
\ inh lgs lgs

\ inh d16: d16:
\ inh a16: a16:
\ inh es:  es:
\ inh cs:  cs:
\ inh ds:  ds:
\ inh fs:  fs:
\ inh gs:  gs:

: aam   ( adr code -- adr' )
        .s" aam" drop count drop ;

: aad   ( adr code -- adr' )
        .s" aad" drop count drop ;

: d16   ( adr code -- adr' )
        drop .s" d16:"
        true to 16-bit-data
        true to prefix-op
        ;

: a16   ( adr code -- adr' )
        drop .s" a16:"
        true to 16-bit-addr
        true to prefix-op
        ;

: rpz   ( adr code -- adr' )
        drop .s" repnz"
        true to prefix-op
        ;

: rep   ( adr code -- adr' )
        drop .s" repz"
        true to prefix-op
        ;

: lok   ( adr code -- adr' )  \ This should have error checking added
        drop .s" lock"
        true to prefix-op
        ;

: cs:   ( adr code -- adr' )
        drop .s" cs:"
        true to prefix-op
        ;

: ds:   ( adr code -- adr' )
        drop .s" ds:"
        true to prefix-op
        ;

: ss:   ( adr code -- adr' )
        drop .s" ss:"
        true to prefix-op
        ;

: es:   ( adr code -- adr' )
        drop .s" es:"
        true to prefix-op
        ;

: gs:   ( adr code -- adr' )
        drop .s" gs:"
        true to prefix-op
        ;

: fs:   ( adr code -- adr' )
        drop .s" fs:"
        true to prefix-op
        ;

: isd   ( adr code -- adr' )
        drop 16-bit-data
        IF      .s" insw    "
        ELSE    .s" insd    "
        THEN ;

: osd   ( adr code -- adr' )
        drop 16-bit-data
        IF      .s" outsw    "
        ELSE    .s" outsd    "
        THEN ;

: inp   ( addr code -- addr' )
        .s" in      " 1 and
        IF      16-bit-data
                IF      .s" ax , "
                ELSE    .s" eax , "
                THEN
        ELSE    .s" al , "
        THEN
        count h.>s ;

: otp   ( addr code -- addr' )
        .s" out     " 1 and
        IF      count h.>s 16-bit-data
                IF      .s" , ax"
                ELSE    .s" , eax"
                THEN
        ELSE    count h.>s .s" , al"
        THEN
        ;

: ind   ( addr code -- addr' )
        .s" in      " 1 and
        IF      16-bit-data
                IF      .s" ax , dx"
                ELSE    .s" eax , dx"
                THEN
        ELSE    .s" al , dx"
        THEN
        ;

: otd   ( addr code -- addr' )
        .s" out     " 1 and
        IF      16-bit-data
                IF      .s" dx , ax"
                ELSE    .s" dx , eax"
                THEN
        ELSE    .s" dx , al"
        THEN
        ;

\ -------------------- ALU Opcodes --------------------

: .alu  ( n -- )
        7 and S" addor adcsbbandsubxorcmp" 3 ss. 4 sspaces ;

: alu  ( adr op -- adr' )
        dup 3 rshift .alu r/m ;

: ali ( adr op -- adr' )
        >r count
        dup 3 rshift .alu
        mod-r/m
        r> 3 and ?dup
        if      1 =
                if      imm16/32
                else   .# count sext 0 .r>s sspace
                then
        else    imm8
        then ;

: ala  ( adr op -- adr' )
        dup 3 rshift .alu
        1 and if 0 reg imm16/32 else 0 reg8 imm8 then ;


\ -------------------- Test/Xchg --------------------

: txb   ( addr op -- addr' )
        dup 3 and S" testtestxchgxchg" 4 ss. 3 sspaces
        1 and
        IF      1 to size r,r/m     \ SMuB removed COUNT
        ELSE    0 to size r,r/m     \ SMuB removed COUNT
        THEN
        ;

: tst   ( addr op -- addr' )
        .s" test    " 1 and
        IF      16-bit-data
                IF   .s" ax , "
                ELSE .s" eax , "
                THEN
                imm16/32
        ELSE    .s" al , " imm8
        THEN
        ;

\ -------------------- Inc/Dec ----------------------

: inc  ( addr op -- addr' )
        .s" inc     " reg16/32 ;

: dec  ( addr op -- addr' )
        .s" dec     " reg16/32 ;


\ -------------------- Push/Pop --------------------

: psh   ( addr op -- addr' )
        .s" push    " reg16/32 ;

: pop   ( addr op -- addr' )
        .s" pop     " reg16/32 ;

: pss   ( addr op -- addr' )
        .s" push    " sreg ;

: pps   ( addr op -- addr' )
        .s" pop     " sreg ;

: psa   ( addr op -- addr' )
        drop 16-bit-data
        IF      .s" pusha   "
        ELSE    .s" pushad  "
        THEN ;

: ppa   ( addr op -- addr' )
        drop 16-bit-data
        IF      .s" popa    "
        ELSE    .s" popad   "
        THEN ;

: psi   ( addr op -- addr' )
        .s" push    " 2 and
        IF      imm8
        ELSE    imm16/32
        THEN ;

: psf   ( addr op -- addr' )
        drop 16-bit-data
        IF      .s" pushf   "
        ELSE    .s" pushfd  "
        THEN ;

: ppf   ( addr op -- addr' )
        drop 16-bit-data
        IF      .s" popf    "
        ELSE    .s" popfd   "
        THEN ;

: 8F.   ( addr op -- addr' )
        drop count .s" pop     " r/m16/32 ;

\ -------------------- Move --------------------

: mov  ( addr op -- addr' )
        .s" mov     " r/m ;

: mri  ( addr op -- addr' ) ( mov register, imm )
        .s" mov     " dup 8 and
        IF      reg16/32 imm16/32
        ELSE    reg8 imm8
        THEN ;

: mvi  ( adr op -- adr' )   ( mov mem, imm )
        .s" mov     " drop count mod-r/m
        size
        IF      imm16/32
        ELSE    imm8
        THEN
        ;

: mrs   ( addr op -- addr' )
\ ? remove redundant >r , r>
        16-bit-data
        IF      .s" mov     " drop
                1 to size
                count dup mod-r/m .,
                sreg
        ELSE    ???
        THEN ;

: msr   ( addr op -- addr' )
        16-bit-data
        IF      .s" mov     " drop
                1 to size
                count dup sreg .,
                mod-r/m
        ELSE    ???
        THEN ;

: mrc   ( addr op -- addr' )
        .s" mov     "
        drop count dup reg32 .s" , "
        creg ;

: mcr   ( addr op -- addr' )
        .s" mov     "
        drop count dup creg .s" , "
        reg32 ;

: mrd   ( addr op -- addr' )
        .s" mov     "
        drop count dup reg32 .s" , "
        dreg ;

: mdr   ( addr op -- addr' )
        .s" mov     "
        drop count dup dreg .s" , "
        reg32 ;

: mrt   ( addr op -- addr' )
\ obsolete
        .s" mov     "
        drop count dup reg32 .s" , "
        treg ;

: mtr   ( addr op -- addr' )
\ obsolete
        .s" mov     "
        drop count dup treg .s" , "
        reg32 ;

: mv1   ( addr op -- addr' )
        .s" mov     " 1 and
        IF      16-bit-data
                IF      .s" ax , "
                ELSE    .s" eax , "
                THEN
        ELSE    .s" al , "
        THEN
        disp16/32 ;

: mv2   ( addr op -- addr' )
        >r .s" mov     " disp16/32 .,
        r> 1 and
        IF      16-bit-data
                IF      .s"  ax"
                ELSE    .s"  eax"
                THEN
        ELSE    .s"  al"
        THEN
        ;

: lea  ( addr op -- addr' )
        .s" lea     " drop  1 to size r,r/m ;

: lxs   ( addr op -- addr' )
        1 and
        IF      .s" lds     "
        ELSE    .s" les     "
        THEN
        r,r/m   \ SMuB removed COUNT
        ;

: bnd  ( addr op -- addr' )
        .s" bound   " drop  1 to size r,r/m ;

: arp   ( addr op -- addr' )
        .s" arpl    " drop
        1 to size
        true to 16-bit-data
        r,r/m
        ;

: mli   ( addr op -- addr' )
        1 to size
        .s" imul    " $69 =
        IF      r,r/m imm16/32
        ELSE    r,r/m imm8
        THEN ;

\ -------------------- Jumps and Calls --------------------

: rel8  ( addr op -- addr' )
        count sext over + h.>s ;

: rel16/32 ( addr op -- addr' )
        16-bit-addr
        IF      w@+
        ELSE    @+
        THEN    over + base-addr - show-name ;

: jsr  ( addr op -- addr' )
        .s" call    " drop rel16/32 ;

: jmp  ( addr op -- addr' )
        .s" jmp     " 2 and if rel8 else rel16/32 then ;

: .jxx  ( addr op -- addr' )
        .s" j" tttn 4 sspaces ;

: bra  ( addr op -- addr' )
        .jxx rel8 ;

: lup  ( addr op -- addr' )
        3 and S" loopnzloopz loop  jecxz " 6 ss. 1 sspaces rel8 ;

: lbr  ( addr op -- addr' )
        .jxx rel16/32 ;

: rtn  ( addr op -- addr' )
        .s" ret     near " 1 and 0=
        IF      w@+ h.>s
        THEN ;

: rtf  ( addr op -- addr' )
        .s" ret     far " 1 and 0=
        IF      w@+ h.>s
        THEN ;

: ent  ( addr op -- addr' )
        .s" enter   " drop w@+ h.>s ., count h.>s ;

: cis   ( addr op -- addr' )
        $9a =
        IF      .s" call    "
        ELSE    .s" jmp     "
        THEN
        16-bit-data
        IF      .s" ptr16:16 "
        ELSE    .s" ptr16:32 "
        THEN
        count mod-r/m ;

: nt3   ( addr op -- addr' )
        drop .s" int     3 "
        ;

: int   ( addr op -- addr' )
        drop .s" int     "
        count h.>s ;

inh lev leave
inh irt  iret
inh nto  into

\ -------------------- string ops --------------------

: str   inh does> count >s  1 and if .s" d" else .s" b" then ;

str mvs movs
str cps cmps
str sts stos
str lds lods
str scs scas

\ -------------------- Exchange --------------------

: xga  ( addr op -- addr' )
        .s" xchg     eax, " reg16/32 ;

\ : xch  ( addr op -- addr' )
\       .s" xchg    " drop r,r/m ;


\ -------------------- Shifts & Rotates --------------------

: .shift ( n -- )
        7 and S" rolrorrclrcrshlshrxxxsar" 3 ss.  4 sspaces ;

: shf  ( addr op -- addr' )
        >r count
        dup 3 rshift .shift
        mod-r/m .,
        r> $D2 and
        case
           $C0 of count h.>s      endof
           $D0 of 1 h.>s          endof
           $D2 of 1 reg8          endof
        endcase ;

\ -------------------- Extended Opcodes --------------------

: wf1  ( addr -- addr' )
        1+ count dup
        $0c0 <
        IF      dup
                3 rshift 7 and
                case 6 of     .s" fstenv  "      mod-r/m   endof
                     7 of     .s" fstcw   word " mod-r/m   endof
                     2drop 2 - dup .s" fwait   "
                endcase
        ELSE    drop 2 - .s" fwait   "
        THEN ;

: wf2  ( addr -- addr' )
        1+ count
        case $e2 of   .s" fclex   "  endof
             $e3 of   .s" finit   "  endof
             swap 2 - swap .s" fwait   "
        endcase ;

: wf3  ( addr -- addr' )
        1+ count dup 3 rshift 7 and
        case 6 of     .s" fsave   "      mod-r/m   endof
             7 of     .s" fstsw   word " mod-r/m   endof
             2drop 2 - dup .s" fwait   "
        endcase ;

: wf4  ( addr -- addr' )
        1+ count $e0 =
        IF      .s" fstsw   ax "
        ELSE    2 - .s" fwait   "
        THEN ;

: fwaitops   ( addr op -- addr' )
        case $d9 of    wf1     endof
             $db of    wf2     endof
             $dd of    wf3     endof
             $df of    wf4     endof
             .s" fwait   "
        endcase ;

: w8f   ( addr op -- addr' )
        drop dup c@ dup $f8 and $d8 =
        IF      fwaitops
        ELSE    drop .s" wait    "
        THEN ;

: falu1   ( xopcode -- )
        3 rshift 7 and
        S" fadd fmul fcom fcompfsub fsubrfdiv fdivr"
        5 ss. 2 sspaces ;

: falu5   ( xopcode -- )
        3 rshift 7 and
        s" fadd fmul ???? ???? fsubrfsub fdivrfdiv "
        5 ss. 2 sspaces ;

: sti.   ( op -- )
        7 and .s" ST(" 1 .r>s .s" )";

\ : sti.st   ( op -- )
\        7 and
\        .s" ST(" 1 .r>s .s" )" .s"  ST " ;

: fd8   ( addr opcode -- addr' )
        drop count dup falu1
        dup $c0 <
        IF      .s" float " mod-r/m
        ELSE    dup $f0 and $d0 =
                IF      sti.
                ELSE    .s" ST , " sti.
                THEN
        THEN ;

: fdc   ( addr opcode -- addr' )
        drop count
        dup dup $c0 <
        IF      falu1 .s" double " mod-r/m
        ELSE    falu5 sti. .s"  , ST"
        THEN ;

: fnullary-f   ( op -- )
        $0f and dup 8 <
        IF
           S" f2xm1  fyl2x  fptan  fpatan fxtractfprem1 fdecstpfincstp"
        ELSE  8 -
           S" fprem  fyl2xp1fsqrt  fsincosfrndintfscale fsin   fcos   "
        THEN
        7 ss. ;

: fnullary-e   ( op -- )
        $0f and dup 8 <
        IF
           S" fchs   fabs   ???    ???    ftst   fxam   ???    ???    "
        ELSE  8 -
           S" fld1   fldl2t fldl2e fldpi  fldlg2 fldln2 fldz   ???    "
        THEN
        7 ss. ;

: fnullary   ( op -- )
        dup $ef >
        IF      fnullary-f EXIT
        THEN
        dup $e0 <
        IF      $d0 =
                IF      .s" fnop"
                ELSE    dup ???
                THEN
                EXIT
        THEN
        fnullary-e ;


\ : falu2   ( op -- )
\        3 rshift 7 and
\        S" fld    ???    fst    fstp   fldenv fldcw  fnstenvfnstcw "
\        7 ss. ;

: fd9   ( addr op -- addr' )
        drop count dup $c0 <
        IF      dup $38 and
                CASE
                        $00 OF .s" fld     float "  endof
                        $10 OF .s" fst     float "  endof
                        $18 OF .s" fstp    float "  endof
                        $20 OF .s" fldenv  "        endof
                        $28 OF .s" fldcw   word "   endof
                        $30 OF .s" fnstenv "        endof
                        $38 OF .s" fnstcw  word "   endof
                            dup ???
                ENDCASE
                mod-r/m
        ELSE
                dup $d0 <
                IF      dup $c8 <
                        IF      .s" fld     "
                        ELSE    .s" fxch    "
                        THEN
                        sti.
                ELSE    fnullary
                THEN
        THEN ;

: falu3   ( op -- )
        3 rshift 7 and
        S" fiadd fimul ficom ficompfisub fisubrfidiv fidivr"
        6 ss. 1 sspaces ;

: fcmova  ( op -- )
        3 rshift 7 and
        S" fcmovb fcmove fcmovbefcmovu ???    ???    ???    ???    "
        7 ss. ;

: fda   ( addr op -- addr' )
    drop count dup $c0 < IF ( addr1 op1 )
	dup falu3 .s" dword " mod-r/m
    ELSE
	dup $e9 = IF ( addr1 op1 )
	    drop .s" fucompp"
	ELSE
	    dup fcmova sti.
	THEN
    THEN ;

: falu7  ( op -- )
        3 rshift 7 and
        S" faddp fmulp ???   ???   fsubrpfsubp fdivrpfdivp "
        6 ss. sspace ;

: fde   ( addr op -- addr' )
        drop count dup $c0 <
        IF      dup falu3 .s" word " mod-r/m
        ELSE    dup $d9 =
                if    .s" fcompp" drop
                else  dup falu7 sti.
                then
        THEN ;

: fcmovb  ( op -- )
        3 rshift 7 and
        S" fcmovnb fcmovne fcmovnbefcmovnu ???     fucomi  fcomi   ???     "
        8 ss. ;

: fdb   ( addr op -- addr' )
        drop count dup $c0 <
        IF      dup $38 and
                CASE    $00 OF .s" fild    dword "    endof
                        $10 OF .s" fist    dword "    endof
                        $18 OF .s" fistp   dword "    endof
                        $28 OF .s" fld     extended " endof
                        $38 OF .s" fstp    extended " endof
                            dup ???
                ENDCASE
                mod-r/m
        ELSE
                CASE    $e2 OF .s" fnclex" endof
                        $e3 OF .s" fninit" endof
                            dup dup fcmovb sti.
                ENDCASE
        THEN ;

: falu6  ( op -- )
        3 rshift 7 and
        S" ffree ???   fst   fstp  fucom fucomp???   ???   "
        6 ss. sspace ;

: fdd   ( addr op -- addr' )
        drop count dup $c0 <
        IF      dup $38 and
                CASE    $00 OF .s" fld     double "  endof
                        $10 OF .s" fst     double "  endof
                        $18 OF .s" fstp    double "  endof
                        $20 OF .s" frstor  "         endof
                        $30 OF .s" fnsave  "         endof
                        $38 OF .s" fnstsw  word   "  endof
                            dup ???
                ENDCASE
                mod-r/m
        ELSE    dup falu6 sti.
        THEN ;

: fdf   ( addr op -- addr' )
        drop count dup $c0 <
        IF      dup $38 and
                CASE    $00 OF .s" fild    word "   endof
                        $10 OF .s" fist    word "   endof
                        $18 OF .s" fistp   word "   endof
                        $20 OF .s" fbld    tbyte "  endof
                        $28 OF .s" fild    qword "  endof
                        $30 OF .s" fbstp   tbyte "  endof
                        $38 OF .s" fistp   qword "  endof
                            dup ???
                ENDCASE
                mod-r/m
        ELSE    dup $e0 =
                IF      .s" fnstsw  ax " drop
                ELSE    dup $38 and
                        CASE    $28 OF .s" fucomip " sti. endof
                                $30 OF .s" fcomip  " sti. endof
                                        ???
                        ENDCASE
                THEN
        THEN ;

: gp6 ( addr op -- addr' )
        drop count dup 3 rshift
        7 and S" sldtstr lldtltr verrverw??? ???" 4 ss. 3 sspaces
        r/m16 ;

: gp7 ( addr op -- addr' )
        drop count dup 3 rshift
        7 and dup S" sgdt  sidt  lgdt  lidt  smsw  ???   lmsw  invlpg" 6 ss. 1 sspaces
        4 and 4 =
        if   r/m16
        else r/m16/32
        then ;

: btx.  ( n -- )
        3 rshift
        3 and S" bt btsbtrbtc" 3 ss. 4 sspaces ;

: gp8 ( addr op -- addr' )
        drop count dup btx.
        r/m16/32 imm8 ;

: lar ( addr op -- addr' )
        .s" lar     " drop r,r/m ;

: lsl ( addr op -- addr' )
        .s" lsl     " drop r,r/m ;

: lss ( addr op -- addr' )
        .s" lss     " drop r,r/m ;

: lfs ( addr op -- addr' )
        .s" lfs     " drop r,r/m ;

: lgs ( addr op -- addr' )
        .s" lgs     " drop r,r/m ;

: btx ( addr op -- addr' )
        btx. r/m,r ;

: sli ( addr op -- addr' )
        .s" shld    " drop r/m,r imm8 ;

: sri ( addr op -- addr' )
        .s" shrd    " drop r/m,r imm8 ;

: slc ( addr op -- addr' )
        .s" shld    " drop r/m,r .s" , cl" ;

: src ( addr op -- addr' )
        .s" shrd    " drop r/m,r .s" , cl" ;

: iml ( addr op -- addr' )
        .s" imul    " drop r,r/m ;

: cxc ( addr op -- addr' )
        .s" cmpxchg " 1 and to size r/m,r ;

: mvx ( addr op -- addr' )
        dup 8 and
        if      .s" movsx   "
        else    .s" movzx   "
        then
        1 and >r
        count mod/sib r>                        \ size bit
        if    swap reg32 .,                     \ word to dword case
              3 =
              if   reg16
              else .s" word ptr " mod-r/m
              then
        else  swap reg16/32 .,                  \ byte case
              3 =
              if   reg8
              else .s" byte ptr " mod-r/m
              then
        then ;

: xad ( addr op -- addr' )
        .s" xadd    " 1 and to size r/m,r ;

: bsf ( addr op -- addr' )
        .s" bsf     " drop r,r/m ;

: bsr ( addr op -- addr' )
        .s" bsr     " drop r,r/m ;

: cx8 ( addr op -- addr' )
        .s" cmpxchg8b " drop count r/m16/32 ;

: bsp ( addr op -- addr' )
        .s" bswap   " reg32 ;

\ : 0F.   ( addr op -- addr' )
\        drop count
\        case
\                0x00 of   gp6                             endof
\                0x01 of   gp7                             endof
\                0x02 of   .s" lar     " 1 to size r,r/m   endof
\                0x03 of   .s" lsl     " 1 to size r,r/m   endof
\                0x06 of   .s" clts    "                   endof
\                0x08 of   .s" invd    "                   endof
\                0x09 of   .s" wbinvd  "                   endof
\                0x20 of   mrc                             endof
\                0x21 of   mrd                             endof
\                0x22 of   mcr                             endof
\                0x23 of   mdr                             endof
\                0x24 of   mrt                             endof \ obsolete
\                0x26 of   mtr                             endof \ obsolete
\                0x30 of   .s" wrmsr   "                   endof
\                0x31 of   .s" rdtsc   "                   endof
\                0x32 of   .s" rdmsr   "                   endof
\                0x80 of   .s" jo      " rel16/32          endof
\                0x81 of   .s" jno     " rel16/32          endof
\                0x82 of   .s" jc      " rel16/32          endof
\                0x83 of   .s" jnc     " rel16/32          endof
\                0x84 of   .s" jz      " rel16/32          endof
\                0x85 of   .s" jne     " rel16/32          endof
\                0x86 of   .s" jbe     " rel16/32          endof
\                0x87 of   .s" ja      " rel16/32          endof
\                0x88 of   .s" js      " rel16/32          endof
\                0x89 of   .s" jns     " rel16/32          endof
\                0x8A of   .s" jpe     " rel16/32          endof
\                0x8B of   .s" jpo     " rel16/32          endof
\                0x8C of   .s" jnge    " rel16/32          endof
\                0x8D of   .s" jge     " rel16/32          endof
\                0x8E of   .s" jng     " rel16/32          endof
\                0x8F of   .s" jg      " rel16/32          endof
\                0x90 of   .s" seto    byte " r/m8         endof
\                0x91 of   .s" setno   byte " r/m8         endof
\                0x92 of   .s" setc    byte " r/m8         endof
\                0x93 of   .s" setnc   byte " r/m8         endof
\                0x94 of   .s" setz    byte " r/m8         endof
\                0x95 of   .s" setnz   byte " r/m8         endof
\                0x96 of   .s" setbe   byte " r/m8         endof
\                0x97 of   .s" seta    byte " r/m8         endof
\                0x98 of   .s" sets    byte " r/m8         endof
\                0x99 of   .s" setns   byte " r/m8         endof
\                0x9A of   .s" setp    byte " r/m8         endof
\                0x9B of   .s" setnp   byte " r/m8         endof
\                0x9C of   .s" setl    byte " r/m8         endof
\                0x9D of   .s" setge   byte " r/m8         endof
\                0x9E of   .s" setle   byte " r/m8         endof
\                0x9F of   .s" setg    byte " r/m8         endof
\                0xA0 of   .s" push    fs "                endof
\                0xA1 of   .s" pop     fs "                endof
\                0xA2 of   .s" cpuid      "                endof
\                0xA3 of   .s" bt      " 1 to size r/m,r   endof
\                0xA4 of   .s" shld    " r/m,r imm8        endof
\                0xA5 of   .s" shld    " r/m,r .s" , cl"   endof
\                0xA8 of   .s" push    gs "                endof
\                0xA9 of   .s" pop     gs "                endof
\                0xAA of   .s" rsm     "                   endof
\                0xAB of   .s" bts     " 1 to size r/m,r   endof
\                0xAC of   .s" shrd    " r/m,r imm8        endof
\                0xAD of   .s" shrd    " r/m,r .s" , cl"   endof
\                0xAF of   .s" imul    " r,r/m             endof
\                0xB0 of   .s" cmpxch  " 0 to size r/m,r   endof
\                0xB1 of   .s" cmpxch  " 1 to size r/m,r   endof
\                0xB2 of   .s" lss     " 1 to size r,r/m   endof
\                0xB3 of   .s" btr     " 1 to size r/m,r   endof
\                0xB4 of   .s" lfs     " 1 to size r,r/m   endof
\                0xB5 of   .s" lgs     " 1 to size r,r/m   endof
\                0xB6 of   .s" movzx   " 0 to size r,r/m   endof
\                0xB7 of   .s" movzx   " 1 to size r,r/m   endof
\                0xBA of   gp8                             endof
\                0xBB of   .s" btc     " 1 to size r/m,r   endof
\                0xBC of   .s" bsf     " 1 to size r,r/m   endof
\                0xBD of   .s" bsr     " 1 to size r,r/m   endof
\                0xBE of   .s" movsx   " 0 to size r,r/m   endof
\                0xBF of   .s" movsx   " 1 to size r,r/m   endof
\                0xC0 of   .s" xadd    " 0 to size r/m,r   endof
\                0xC1 of   .s" xadd    " 1 to size r/m,r   endof
\                0xC7 of   .s" cmpxchg8b " r/m16/32        endof
\                0xC8 of   .s" bswap   eax "               endof
\                0xC9 of   .s" bswap   ecx "               endof
\                0xCA of   .s" bswap   edx "               endof
\                0xCB of   .s" bswap   ebx "               endof
\                0xCC of   .s" bswap   esp "               endof
\                0xCD of   .s" bswap   ebp "               endof
\                0xCE of   .s" bswap   esi "               endof
\                0xCF of   .s" bswap   edi "               endof
\                ( else )  dup ???
\        endcase
\        ;

: F6.  ( addr op -- addr' )
\ ??
        >r count
        dup 3 rshift 7 and dup >r S" testXXXXnot neg mul imuldiv idiv" 4 ss. 3 sspaces
        mod-r/m
        r> 0= if
                r@ 1 and if imm16/32
                         else imm8
                         then
              then
        r> drop ;

: FE.  ( addr op -- addr' )
        drop count
        dup 3 rshift 7 and
        case
                0 of .s" inc     "  endof
                1 of .s" dec     "  endof
                     ???
        endcase r/m8 ;

: FF.  ( addr op -- addr' )
        drop count
        dup 3 rshift 7 and
        case
                0 of .s" inc     "      endof
                1 of .s" dec     "      endof
                2 of .s" call    "      endof
                3 of .s" call    far "  endof
                4 of .s" jmp     "      endof
                5 of .s" jmp     far "  endof
                6 of .s" push    "      endof
                     ???
        endcase r/m16/32 ;

\ --------------------- conditional move ---------------

: set   ( adr op -- )
        .s" set"
        tttn 2 sspaces
        count r/m8 ;

: cmv   ( adr op -- )
        .s" cmov"
        tttn 1 sspaces
        count r,r/m ;

\ --------------------- MMX Operations -----------------

: mmx-size ( op -- )
        3 and S" bwdq" 1 ss. ;

: upl   ( adr op -- adr' )
        3 and S" punpcklbwpunpcklwdpunpckldq" 9 ss. r,r/m ;

: uph   ( adr op -- adr' )
        3 and S" punpckhbwpunpckhwdpunpckhdq" 9 ss. r,r/m ;

: cgt   ( adr op -- adr' )
        .s" pcmpgt" mmx-size r,r/m ;

: ceq   ( adr op -- adr' )
        .s" pcmpeq" mmx-size r,r/m ;

: psh.  ( op -- )
        $30 and
        case
             $10 of .s" psrl" endof
             $20 of .s" psra" endof
             $30 of .s" psll" endof
        endcase ;

: gpa   ( adr op -- adr' )
        >r count dup psh. r> mmx-size 2 sspaces mreg imm8 ;

: puw   ( adr op -- adr' )
        .s" packusdw " drop r,r/m ;

: psb   ( adr op -- adr' )
        .s" packsswb " drop r,r/m ;

: psw   ( adr op -- adr' )
        .s" packssdw " drop r,r/m ;

: mpd   ( adr op -- adr' )
        .s" movd    " drop count mod/sib
        swap mreg ., 3 =
        if   reg32
        else mod-r/m
        then ;

: mdp   ( adr op -- adr' )
        .s" movd    " drop count mod/sib
        3 =
        if   swap reg32
        else swap mod-r/m
        then ., mreg ;

: mpq   ( adr op -- adr' )
        .s" movq    " drop r,r/m ;

: mqp   ( adr op -- adr' )
        .s" movq    " drop r/m,r ;

: shx   ( adr op -- adr' )
        dup psh. mmx-size 2 sspaces r,r/m ;

: mll   ( adr op -- adr' )
        .s" pmullw  " drop r,r/m ;

: mlh   ( adr op -- adr' )
        .s" pmulhw  " drop r,r/m ;

: mad   ( adr op -- adr' )
        .s" pmaddwd " drop r,r/m ;

: sus   ( adr op -- adr' )
        .s" psubus" mmx-size r,r/m ;

: sbs   ( adr op -- adr' )
        .s" psubs" mmx-size sspace r,r/m ;

: sub   ( adr op -- adr' )
        .s" psub" mmx-size 2 sspaces r,r/m ;

: aus   ( adr op -- adr' )
        .s" paddus" mmx-size r,r/m ;

: ads   ( adr op -- adr' )
        .s" padds" mmx-size sspace r,r/m ;

: add   ( adr op -- adr' )
        .s" padd" mmx-size 2 sspaces r,r/m ;

: pad   ( adr op -- adr' )
        .s" pand    " drop r,r/m ;

: por   ( adr op -- adr' )
        .s" por     " drop r,r/m ;

: pan   ( adr op -- adr' )
        .s" pandn   " drop r,r/m ;

: pxr   ( adr op -- adr' )
        .s" pxor    " drop r,r/m ;

\ -------------------- Opcode Table --------------------

: ops $10 0 do ' , loop ;

create op-table2

\     0   1   2   3    4   5   6   7    8   9   A   B    C   D   E   F

ops  gp6 gp7 lar lsl  ??? ??? clt ???  inv wiv ??? ud2  ??? ??? ??? ???  \ 0
ops  ??? ??? ??? ???  ??? ??? ??? ???  ??? ??? ??? ???  ??? ??? ??? ???  \ 1
ops  mrc mrd mcr mdr  mrt ??? mtr ???  ??? ??? ??? ???  ??? ??? ??? ???  \ 2
ops  wmr rtc rmr rpc  ??? ??? ??? ???  ??? ??? ??? ???  ??? ??? ??? ???  \ 3

ops  cmv cmv cmv cmv  cmv cmv cmv cmv  cmv cmv cmv cmv  cmv cmv cmv cmv  \ 4
ops  ??? ??? ??? ???  ??? ??? ??? ???  ??? ??? ??? ???  ??? ??? ??? ???  \ 5
ops  upl upl upl puw  cgt cgt cgt psb  uph uph uph psw  ??? ??? mpd mpq  \ 6
ops  ??? gpa gpa gpa  ceq ceq ceq ems  ??? ??? ??? ???  ??? ??? mdp mqp  \ 7

ops  lbr lbr lbr lbr  lbr lbr lbr lbr  lbr lbr lbr lbr  lbr lbr lbr lbr  \ 8
ops  set set set set  set set set set  set set set set  set set set set  \ 9
ops  pss pps cpu btx  sli slc ??? ???  pss pps rsm btx  sri src ??? iml  \ A
ops  cxc cxc lss btx  lfs lgs mvx mvx  ??? ud1 gp8 btx  bsf bsr mvx mvx  \ B

ops  xad xad ??? ???  ??? ??? ??? cx8  bsp bsp bsp bsp  bsp bsp bsp bsp  \ C
ops  ??? shx shx shx  ??? mll ??? ???  sus sus ??? pad  aus aus ??? pan  \ D
ops  ??? shx shx ???  ??? mlh ??? ???  sbs sbs ??? por  ads ads ??? pxr  \ E
ops  ??? ??? shx shx  ??? mad ??? ???  sub sub sub ???  add add add ???  \ F

\     0   1   2   3    4   5   6   7    8   9   A   B    C   D   E   F

: 0F.  ( adr code -- )
        drop count dup
        dup $70 and $50 $80 within to mmx-reg
        cells op-table2 + @ execute
        0 to mmx-reg ;

create op-table

\     0   1   2   3    4   5   6   7    8   9   A   B    C   D   E   F

ops  alu alu alu alu  ala ala pss pps  alu alu alu alu  ala ala pss 0F.  \ 0
ops  alu alu alu alu  ala ala pss pps  alu alu alu alu  ala ala pss pps  \ 1
ops  alu alu alu alu  ala ala es: daa  alu alu alu alu  ala ala cs: das  \ 2
ops  alu alu alu alu  ala ala ss: aaa  alu alu alu alu  ala ala ds: aas  \ 3

ops  inc inc inc inc  inc inc inc inc  dec dec dec dec  dec dec dec dec  \ 4
ops  psh psh psh psh  psh psh psh psh  pop pop pop pop  pop pop pop pop  \ 5
ops  psa ppa bnd arp  fs: gs: d16 a16  psi mli psi mli  inb isd osb osd  \ 6
ops  bra bra bra bra  bra bra bra bra  bra bra bra bra  bra bra bra bra  \ 7

ops  ali ali ??? ali  txb txb txb txb  mov mov mov mov  mrs lea msr 8F.  \ 8
ops  xga xga xga xga  xga xga xga xga  cbw cdq cis w8f  psf ppf sah lah  \ 9
ops  mv1 mv1 mv2 mv2  mvs mvs cps cps  tst tst sts sts  lds lds scs scs  \ A
ops  mri mri mri mri  mri mri mri mri  mri mri mri mri  mri mri mri mri  \ B

ops  shf shf rtn rtn  lxs lxs mvi mvi  ent lev rtf rtf  nt3 int nto irt  \ C
ops  shf shf shf shf  aam aad ??? xlt  fd8 fd9 fda fdb  fdc fdd fde fdf  \ D
ops  lup lup lup lup  inp inp otp otp  jsr jmp cis jmp  ind ind otd otd  \ E
ops  lok ??? rpz rep  hlt cmc F6. F6.  clc stc cli sti  cld std FE. FF.  \ F

\     0   1   2   3    4   5   6   7    8   9   A   B    C   D   E   F

: dis-op  ( adr -- adr' )
        0>s
        false to prefix-op           \ SMuB
        count
        dup 1 and to size
        dup cells op-table +  @ execute
        prefix-op 0=
        if default-16bit? 0=
           if   false to 16-bit-data
                false to 16-bit-addr
           else true  to 16-bit-data
                true  to 16-bit-addr
           then
        then ;

0 value next-inst

: inst  ( adr -- adr' )
        dup to next-inst
        cols $29 <
        if      dis-op
                s-buf count type
        else    dup dis-op
                over base-addr - ." ( " hex. ( 6 h.r )  ." ) "
                comment-col col s-buf count type ."  \ "
		dup rot
                2dup - $10 u> abort" decompiler error"
                do i c@ hex. ( 2 h.n ) loop
        then    dup to next-inst ;

forth definitions

: disasm ( addr u -- ) \ gforth
    over + >r
    begin
	dup r@ u<
    while
	cr inst
    repeat
    cr rdrop drop ;
    
' disasm is discode

only forth also definitions

