\ disasm.fs: disassembler for ARM

\ Copyright (C) 2009 Free Software Foundation, Inc.

\ This file is part of Gforth.

\ Gforth is free software; you can redistribute it and/or
\ modify it under the terms of the GNU General Public License
\ as published by the Free Software Foundation, either version 3
\ of the License, or (at your option) any later version.

\ This program is distributed in the hope that it will be useful,
\ but WITHOUT ANY WARRANTY; without even the implied warranty of
\ MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
\ GNU General Public License for more details.

\ You should have received a copy of the GNU General Public License
\ along with this program. If not, see http://www.gnu.org/licenses/.

\ Contributed by Andreas Bolka.

get-current
vocabulary disassembler
also disassembler definitions

\ --

: elem-type ( i u n c -- ) \ for str U of length N, type C chars from U+I*(C+1)
    >r drop swap r@ 1+ * + r> type ;

: lshift32 ( x1 n -- x2 ) \ also works in cross-compilers with 64-bit cells
    lshift $0FFFFFFFF and ;

: rrotate32 ( u n -- u>>>n )
    2dup 32 swap - lshift32 -rot rshift or ;

: sext24>32 ( x -- y ) \ http://graphics.stanford.edu/~seander/bithacks.html
    $00800000 tuck xor swap - ;

: popcnt ( v -- c ) \ http://graphics.stanford.edu/~seander/bithacks.html
    dup 1 rshift $55555555 and -
    dup 2 rshift $33333333 and swap $33333333 and +
    dup 4 rshift + $0F0F0F0F and $01010101 * 24 rshift ;

: cnttz ( v -- c )
    dup negate and 1- popcnt ;

\ --

\ Declaratively describe bit masks and values ("bitchecks").

\ {{ 3 -2 1 }} will leave two values on the stack: TEST and MASK. MASK will be
\ %1110, i.e. bits 3, 2, and 1 set. TEST will be %1010, i.e. bits 3 and 1 set,
\ all other cleared. Given a reference value W, `w mask and test =` will
\ therefore check if bits 3 and 1 are set, and bit 2 is cleared in W.
\
\ Bit 0 (the least-significant bit) can not be used in a "bitcheck".
: {{  ( -- mark ) 77777777 ;
: }}  ( mark bit1 .. bitN -- test mask )
    0 0 2>r begin dup 77777777 <> while
        2r> rot tuck                                ( test bit mask bit )
        %1 swap abs lshift or -rot                  ( mask' test bit )
        dup 0> if %1 swap lshift or else drop endif ( mask' test' )
        swap 2>r
    repeat drop 2r> ;

: b= ( word test mask -- t/f )
    \ true if W matches a bitcheck (TEST MASK)
    rot swap and = ;

: b~ ( w t= m= t<> m<> -- t/f )
    \ true if W matches the first bitcheck (T= M=) _and_ W does not match the
    \ second bitcheck (T<> M<>)
    2>r 2>r dup 2r> b= swap 2r> b= invert and ;

\ --

: fld# ( w mask32 ) dup cnttz tuck rshift ( w s m ) 2 pick rot rshift and ;
: fld: ( mask32 "name" ) create , does> @ fld# ;

\ core fields
$F0000000 fld: %CC#     $01E00000 fld: %opc#    $01800000 fld: %PU#
$00000F00 fld: %rot#    $000000FF fld: %imm8#   $00FFFFFF fld: %off24#
$00000060 fld: %shc#    $00000F80 fld: %shimm#  $00000FFF fld: %off12#
$00E00000 fld: %cpopc1# $000000E0 fld: %cpopc2#
' %rot#  alias %cpnum#  ' %imm8# alias %cpoff8#

$01000000 fld: %b24#    $00800000 fld: %b23#    $00400000 fld: %b22#
$00200000 fld: %b21#    $00100000 fld: %b20#    $00008000 fld: %b15#
$00000020 fld: %b5#     $00000008 fld: %b4#

$F0000 fld: %R16#   $0F000 fld: %R12#   $00F00 fld: %R08#   $0000F fld: %R00#

\ fpa fields
$70000 fld: %F16#   $07000 fld: %F12#   $00007 fld: %F00#

' %shc# alias %fpa-round#

: %bb22,15# ( w -- w u ) %b22# 1 lshift swap %b15# rot or ;
: %bb19,07# ( w -- w u ) $00080000 fld# 1 lshift swap $0080 fld# rot or ;

\ --

\ core tables
: cc-tab        ( -- u n ) s" EQ NE CS CC MI PL VS VC HI LS GE LT GT LE AL NV" ;
: shc-tab       ( -- u n ) s" LSL LSR ASR ROR" ;
: lsm-mode-tab  ( -- u n ) s" DA IA DB IB" ;

\ fpa tables
: fpa-opcd-tab  ( -- u n )
    s" ADF MUF SUF RSF DVF RDF POW RPW RMF FML FDV FRD POL" ;
: fpa-opcm-tab  ( -- u n )
    s" MVF MNF ABS RND SQT LOG LGN EXP SIN COS TAN ASN ACS ATN URD NRM" ;
: fpa-ifm-tab   ( -- u n ) s" 0.0 1.0 2.0 3.0 4.0 5.0 0.5 10." ;
: fpa-prec-tab  ( -- u n ) s" S D E" ;
: fpa-round-tab ( -- u n ) s" P M Z" ;

: dis-CC ( w -- w ) %CC# dup $E <> if cc-tab 2 elem-type space else drop endif ;

: dis-shc ( w -- w ) %shc# shc-tab 3 elem-type space ;

: dis-S ( w -- w ) %b20# if ." S," else ." ," endif ;
: dis-P ( w -- w ) %R12# $F = if ." P," else ." ," endif ;

: Rx. ( n -- )
    case
        13 of ." SP " endof
        14 of ." LR " endof
        15 of ." PC " endof
        ( n ) ." R" dup . ( n )
    endcase ;

: dis-Rn ( w -- w ) %R16# Rx. ;
: dis-Rd ( w -- w ) %R12# Rx. ;
: dis-Rs ( w -- w ) %R08# Rx. ;
: dis-Rm ( w -- w ) %R00# Rx. ;

: CRx. ( n -- ) ." C" . ;
: dis-CRn ( w -- w ) %R16# CRx. ;
: dis-CRd ( w -- w ) %R12# CRx. ;
: dis-CRm ( w -- w ) %R00# CRx. ;

: Fx. ( n -- ) ." F" . ;
: dis-Fn ( w -- w ) %F16# Fx. ;
: dis-Fd ( w -- w ) %F12# Fx. ;
: dis-Fm ( w -- w ) %F00# Fx. ;

: dis-shimmN ( w n -- w ) . ." #" dis-shc ;
: dis-shimm0 ( w -- w )
    %shc# case
        ( LSR ) 1 of 32 dis-shimmN endof
        ( ASR ) 2 of 32 dis-shimmN endof
        ( ROR ) 3 of ." RRX " endof
        ( LSL ) \ noop
    endcase ;

: dis-shimm ( w -- w ) %shimm# ?dup if dis-shimmN else dis-shimm0 endif ;

: dis-mulc ( w -- w ) %b21# if ." MLA" else ." MUL" endif ;

: dis-ls-mode ( w ch -- w )
    swap %b24# 0= if
        ." ]" swap emit \ post-indexed "]x"
    else
        swap emit ." ]" \ offset "x]" or pre-indexed "x]!"
        %b21# 0<> if ." !" endif
    endif space ;

: dis-ls-opc ( w -- w )
    %b20# if ." LDR" else ." STR" endif
    %b22# if ." B" endif
    %b21# swap %b24# 0= rot and if ." T" endif ." ," ;

: dis-cpnum ( w -- w ) ." P" %cpnum# . ;
: dis-cpopc1 ( w -- w ) %cpopc1# hex. ;
: dis-cpopc2 ( w -- w ) %cpopc2# hex. ;

\ --

create dp-table \ [(dis-xt, opcode-str)]
    16 2* cells allot
does> ( i a -- a+2*i )
    swap 2* cells + ;

: dis-opc ( w -- w ) \ lookup opcode-str for w's opc from dp-table and type it
    %opc# dp-table cell+ @ count type ;

\ sh-xts: disassemblers for "shifter operand" modes
: dis-reg/sh ( w -- w ) dis-Rm dis-Rs dis-shc ;
: dis-imm/sh ( w -- w ) dis-Rm dis-shimm ;
: dis-imm/rot ( w -- w ) %rot# 2* swap %imm8# rot rrotate32 . ." # " ;

: dis-shifter ( sh-xt w -- w ) swap execute ;

\ dis-xts: disassemblers for the data processing instr encodings
: dis-mov ( sh-xt w -- w ) dis-Rd        dis-shifter dis-CC dis-opc dis-S ;
: dis-cmp ( sh-xt w -- w )        dis-Rn dis-shifter dis-CC dis-opc dis-P ;
: dis-dat ( sh-xt w -- w ) dis-Rd dis-Rn dis-shifter dis-CC dis-opc dis-S ;

: dpclass: ( dis-xt "defname" -- )
    create ,
does> @ ( opc dis-xt "opname" -- )
    \ stores the dissambler DIS-XT to use for disassembling dp-instrs with OPC
    \ and the opcode's mnemonic "OPNAME" in dp-table
    swap dp-table tuck !
    here parse-word string, swap cell+ ! ;

' dis-mov dpclass: mov-op: ' dis-cmp dpclass: cmp-op: ' dis-dat dpclass: dat-op:

$0 dat-op: AND  $1 dat-op: EOR  $2 dat-op: SUB  $3 dat-op: RSB
$4 dat-op: ADD  $5 dat-op: ADC  $6 dat-op: SBC  $7 dat-op: RSC
$8 cmp-op: TST  $9 cmp-op: TEQ  $A cmp-op: CMP  $B cmp-op: CMN
$C dat-op: ORR  $D mov-op: MOV  $E dat-op: BIC  $F mov-op: MVN

: dis-dp ( shifter-xt w -- w ) swap %opc# dp-table @ execute ;

\ --

: dis-fpa-prec ( n -- )
    assert0( dup 3 < ) \ precision codes >2 are undefined (or deprecated)
    fpa-prec-tab 1 elem-type ;

: dis-fpa-dp-opc ( w -- w )
    %cpopc1# over %b15# nip 0= if
        assert0( dup $D < ) \ opcodes $D $E $F are undefined for dyadic instrs
        fpa-opcd-tab
    else
        fpa-opcm-tab
    endif 3 elem-type ;

: dis-fpa-iFm ( w -- w )
    %b4# 0= if dis-Fm else %F00# fpa-ifm-tab 3 elem-type ."  # " endif ;

\ --

: dis-dp-imm/sh ( w -- w ) ['] dis-imm/sh dis-dp ;

: dis-dp-reg/sh ( w -- w ) ['] dis-reg/sh dis-dp ;

: dis-dp-imm/rot ( w -- w ) ['] dis-imm/rot dis-dp ;

: dis-ls-imm ( w -- w )
    dis-Rd
    dis-Rn
    %b24# >r %b21# 0= >r %off12# 0= r> and r> and if \ 24=1 & 21=0 & off12=0
        ." ] "
    else
        %b23# 0= if ." -" endif %off12# . [char] # dis-ls-mode
    endif
    dis-ls-opc ;

: dis-ls-reg ( w -- w )
    dis-Rd dis-Rn dis-Rm dis-shimm
    %b23# if [char] + else [char] - endif dis-ls-mode
    dis-CC dis-ls-opc ;

: dis-lsm ( w -- w )
    dis-Rn                                      \ Rn
    ." { " 15 0 u+do                            \ register list
        dup i rshift %1 and if ." R" i . endif
    loop ." } "
    %PU# lsm-mode-tab 2 elem-type               \ mode: basic
    %b21# if ." !" endif space                  \ mode: write?
    dis-CC
    %b22# if ." ^" endif                        \ user mode?
    %b20# if ." LDM," else ." STM," endif ;     \ load/store

: dis-b ( a w -- a w )
    2dup %off24# nip            ( a w a o   )
    sext24>32 2 lshift + 8 +    ( a w a+o+8 )
    swap %CC# $F = if
        %b24# 1 lshift rot + hex. ." BLX,"
    else
        swap hex. dis-CC %b24# if ." BL," else ." B," endif
    endif ;

: dis-bx ( a w -- a w ) dis-Rm dis-CC %b5# if ." BLX," else ." BX," endif ;

: dis-mul ( w -- w )
    %b21# if
        dis-Rd dis-Rm dis-Rs dis-Rn
    else
        dis-Rn dis-Rm dis-Rs
    endif dis-CC dis-mulc dis-S ;

: dis-mull ( w -- w )
    dis-Rd dis-Rn dis-Rm dis-Rs
    dis-CC %b22# if ." S" else ." U" endif dis-mulc ." L" dis-S ;

: dis-swi ( w -- w ) %off24# hex. dis-CC ." SWI," ;

: dis-fpa-ls ( w -- w )
    dis-Fd
    dis-Rn
    %b23# 0= if ." -" endif %cpoff8# 2 lshift . [char] # dis-ls-mode
    dis-CC
    %b20# if ." LDF" else ." STF" endif %bb22,15# dis-fpa-prec ." ," ;

: dis-fpa-dp ( w -- w )
    dis-Fd
    %b15# 0= if dis-Fn endif
    dis-fpa-iFm
    dis-CC
    dis-fpa-dp-opc
    %bb19,07# dis-fpa-prec
    %fpa-round# ?dup if 1- fpa-round-tab 1 elem-type endif ." ," ;

: dis-fpa-cmp ( w -- w )
    dis-Fn
    dis-fpa-iFm
    dis-CC
    %b21# if ." CNF" else ." CMF" endif %b22# if ." E" endif ." ," ;

: dis-cp-ls ( w -- w )
    dis-cpnum
    dis-CRd
    dis-Rn
    %b24# 0= swap %b21# 0= rot and if
        %cpoff8# hex. ." ]$ "
    else
        %b23# 0= if ." -" endif %cpoff8# . [char] # dis-ls-mode
    endif
    %CC# $F <> if dis-CC endif
    %b20# if ." LDC" else ." STC" endif
    %CC# $F = if ." 2" endif
    %b22# if ." L" endif ." ," ;

: dis-cp-dp ( w -- w )
    dis-cpnum dis-cpopc1 dis-CRd dis-CRn dis-CRm dis-cpopc2
    %CC# $F = if ." CDP2," else dis-CC ." CDP," endif ;

: dis-cp-tx ( w -- w )
    dis-cpnum dis-cpopc1 dis-Rd dis-CRn dis-CRm dis-cpopc2
    %CC# $F <> if dis-CC endif
    %b20# if ." MRC" else ." MCR" endif
    %CC# $F = if ." 2" endif ." ," ;

\ --

0 constant guard immediate
: G[ ( #when -- orig #when+1 / t/f -- ) 1+ >r postpone if r> ; immediate
: ]G ( orig1 #when -- orig2 #when ) >r postpone else r> ; immediate
: endguard ( orig1..orign #when -- ) 0 ?do postpone then loop ; immediate

set-current

: disasm-inst ( a w -- ) \ disassemble instruction w at address a
    ."   ( " over hex. ." ) " guard
        \ basic arm instruction formats
        dup {{ -27 -26 -25     -4 }}  {{ 24 -23 -20  }} b~ G[ dis-dp-imm/sh ]G
        dup {{ -27 -26 -25  -7  4 }}  {{ 24 -23 -20  }} b~ G[ dis-dp-reg/sh ]G
        dup {{ -27 -26  25        }}  {{ 24 -23 -20  }} b~ G[ dis-dp-imm/rot ]G
        dup {{ -27  26 -25        }}                    b= G[ dis-ls-imm ]G
        dup {{ -27  26  25     -4 }}                    b= G[ dis-ls-reg ]G
        dup {{  27 -26 -25        }}  {{ 31 30 29 28 }} b~ G[ dis-lsm ]G
        dup {{  27 -26  25        }}                    b= G[ dis-b ]G
        dup {{  27  26  25  24    }}  {{ 31 30 29 28 }} b~ G[ dis-swi ]G
        \ miscellaneous instructions
        dup {{ -27 -26 -25  24 -23 -22 21 -20
                -7  -6       4 }}                       b= G[ dis-bx ]G
        \ multiply extensions
        dup {{ -27 -26 -25 -24 -23 -22 7 -6 -5 4 }}     b= G[ dis-mul ]G
        dup {{ -27 -26 -25 -24  23     7 -6 -5 4 }}     b= G[ dis-mull ]G
        \ fpa coprocessor instructions
        dup {{  27  26 -25         -11 -10 -9 8 }}      b= G[ dis-fpa-ls ]G
        dup {{  27  26  25 -24 -4  -11 -10 -9 8 }}      b= G[ dis-fpa-dp ]G
        dup {{  27  26  25 -24  4  -11 -10 -9 8
                23  20 -19  15 14 13 12  -7 -6 -5 }}    b= G[ dis-fpa-cmp ]G
        \ generic coprocessor disassemblers
        dup {{  27  26 -25        }}                    b= G[ dis-cp-ls ]G
        dup {{  27  26  25 -24 -4 }}                    b= G[ dis-cp-dp ]G
        dup {{  27  26  25 -24  4 }}                    b= G[ dis-cp-tx ]G
        \ fallback
        true                                               G[ dup hex. ." t," ]G
    endguard 2drop ;

: disasm ( a n -- ) \ disassemble n instructions starting at address a
    assert0( dup 4 mod 0= ) \ arm instrs are 32b wide
    cr bounds u+do
        i i ul@ disasm-inst cr
    4 +loop ;

' disasm is discode

previous
