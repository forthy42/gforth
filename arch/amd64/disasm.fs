\ disasm.fs	disassembler file (for AMD64 64-bit mode)
\
\ Copyright (C) 2000 Free Software Foundation, Inc.

\ This file is part of Gforth.

\ Gforth is free software; you can redistribute it and/or
\ modify it under the terms of the GNU General Public License
\ as published by the Free Software Foundation; either version 2
\ of the License, or (at your option) any later version.

\ This program is distributed in the hope that it will be useful,
\ but WITHOUT ANY WARRANTY; without even the implied warranty of
\ MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
\ GNU General Public License for more details.

\ You should have received a copy of the GNU General Public License
\ along with this program; if not, write to the Free Software
\ Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111, USA.

\ This architecture has very funny instruction encodings, all
\ documented nicely in
\ http://www.amd.com/us-en/assets/content_type/white_papers_and_tech_docs/24594.pdf

\ Here's an even more condensed version:

\ legacy-prefix* REX (Opcode1 | OF Opcode2) ( modrm sib? )? disp? imm?

\ where the legacy prefixes are:
\ 66 67 2e 3e 26 64 65 36 f0 f3 f2
\ The 66 f2 f3 prefix are also used as part of the opcode for MMX, SSE, SSE2

\ 67 changes the size of implicit operands of some instructions (e.g. LOOP)
\ see table 1-4

\ The REX prefixes supply 4 bits to the operands: WRXB; W=operand
\ width; R=ModRM reg field ext; X=SIB index field ext; B=ModRM r/m
\ field, SIB base field, or opcode reg field; also, the presence of a
\ REX prefix makes the difference between SIL/DIL/BPL/SPL (present)
\ and AH/BH/CH/DH (absent); the additional bits are ignored for the
\ special cases of ModRM and SIB bytes.

\ 3DNow instructions have opcode formed by 0F 0F and an imm byte

\ prelude
: c@+ count ;

: th ( addr1 n -- addr2 )
    \ cell indexing
    cells + ;

: cell-fill ( addr u w -- )
    rot rot 0 ?do
	2dup i th !
    loop
    2drop ;

: save-mem-here ( addr1 u -- addr2 u )
    here >r
    dup chars allot
    tuck r@ swap chars move
    r> swap ;

: string-table ( n n*"string" -- addr )
    here over 2* cells allot
    swap 0 ?do
	parse-word save-mem-here 2 pick i 2* cells + 2!
    loop ;

\ : bounds over + swap ;
\ : rdrop postpone r> postpone drop ; immediate

\ state coming from prefixes:
variable operand-size \ true if prefix
variable address-size \ true if prefix
variable repeat-prefix \ 0, f2 or f3, depending on prefix
variable rex-prefix \ 0 or 40-4f, depending on prefix

: clear-prefixes ( -- )
    operand-size off
    address-size off
    repeat-prefix off
    rex-prefix off ;

create opcode1-table \ xt table for decoding first opcode byte
$100 cells allot

: disasm-addr1 ( addr1 -- addr2 )
    \ disassemble instruction with some prefixes set
    opcode1-table over c@ th perform ;

: disasm-addr ( addr1 -- addr2 )
    dup clear-prefixes disasm-addr1
    ."  \ " dup rot
\     2drop ;
    ?do
	i c@ hex.
    loop ;

: disasm ( addr u -- ) \ gforth
\G disassemble u aus starting at addr
    over + >r begin
	dup r@ u< while
	    cr ." ( " dup hex. ." ) " disasm-addr
    repeat
    drop rdrop ;

' disasm is discode


: print-rep ( -- )
    repeat-prefix @ case
	$f2 of ." repnz " endof
	$f3 of ." repz " endof
    endcase ;

: illegal-inst ( addr1 -- addr2 )
    print-rep dup c@ hex. 1+ ;
   
opcode1-table $100 ' illegal-inst cell-fill

: repeat-prefix-disasm ( addr1 -- addr2 )
    dup c@ repeat-prefix !
    1+ disasm-addr1 ;

' repeat-prefix-disasm opcode1-table $f2 th !
' repeat-prefix-disasm opcode1-table $f3 th !

: rex-prefix-disasm ( addr1 -- addr2 )
    dup c@ rex-prefix !
    1+ disasm-addr1 ;

opcode1-table $40 th $10 ' rex-prefix-disasm cell-fill

: immediate-prefix ( c "name" -- )
    \ prefix that can be printed immediately and then forgotten
    :noname
    parse-word postpone sliteral postpone type postpone space postpone 1+
    postpone disasm-addr1
    postpone ;
    opcode1-table rot th ! ;

$2e immediate-prefix cs:
$3e immediate-prefix ds:
$26 immediate-prefix es:
$64 immediate-prefix fs:
$65 immediate-prefix gs:
$36 immediate-prefix ss:
$f0 immediate-prefix lock

: operand-size-disasm  ( addr1 -- addr2 )
    operand-size on
    1+ disasm-addr1 ;

' operand-size-disasm opcode1-table $66 th !

: address-size-disasm ( addr1 -- addr2 )
    address-size on
    1+ disasm-addr1 ;

' address-size-disasm opcode1-table $67 th !


create reg8-names
8 string-table al cl dl bl spl bpl sil dil drop

create reg8-names-norex
8 string-table al cl dl bl ah ch dh bh drop

create reg16-names
8 string-table ax cx dx bx sp bp si di drop

: dec.- ( u -- )
    base @ decimal swap 0 .r base ! ;

: .regn ( u -- )
    \ print r#
    'r emit dec.- ;

: .reg8 ( u -- )
    dup 8 < if
	2* cells
	rex-prefix @ if
	    reg8-names
	else
	    reg8-names-norex
	endif
	+ 2@ type
    else
	.regn 'b emit
    endif ;

: .reg16 ( u -- )
    dup 8 < if
	2* cells reg16-names + 2@ type
    else
	.regn 'w emit
    endif ;

: .reg32 ( u -- )
    dup 8 < if
	'e emit 2* cells reg16-names + 2@ type
    else
	.regn 'd emit
    endif ;

: .reg64 ( u -- )
    dup 8 < if
	'r emit 2* cells reg16-names + 2@ type
    else
	.regn
    endif ;

: Gnum ( addr -- u )
    \ decode modRM reg field
    c@ 3 rshift 7 and rex-prefix @ 4 and 2* + ;
    
: Gb ( addr -- )
    \ decode and print modRM reg field as reg8
    Gnum .reg8 ;

: .regv ( u -- )
    \ print register according to operand width
    rex-prefix c@ 8 and if
	.reg64
    else
	operand-size @ if
	    .reg16
	else
	    .reg32
	endif
    endif ;

: .width ( -- )
    \ print [wdq] according to operand width
    rex-prefix c@ 8 and if
	'q
    else
	operand-size @ if
	    'w
	else
	    'd
	endif
    endif
    emit ;

: Gv ( addr -- )
    \ decode and print modRM reg field according to operand width
    Gnum .regv ;

: Ox ( addr -- )
    \ absolute addressing without modRM or SIB
    dup @ hex. ." d[]" 8 + ; \ !! address-size override?

create displacement-info
  0 0 2,  1 $ff 2,  4 $ffffffff 2, \ size mask

: masksx ( w1 mask -- w2 )
    \ apply the mask of the form 0..01..1 in a sign-extending way
    2dup dup 1 rshift invert and and 0<> ( w1 mask fneg )
    over invert and ( w1 mask highbits )
    rot rot and or ;

: base-regnum ( modRM/SIB/opcode -- u )
    \ extract modRM r/m or SIB base or opcode register number
    7 and rex-prefix @ 1 and 3 lshift + ;

: print-base ( sib -- )
    '[ emit base-regnum .reg64 '] emit ;

: mem-SIB ( dispsize mask addr1 -- addr2 )
    \ decode memory operand described by SIB (mask gives the displacement size)
    \ !! change output to stuff like 5 eax edx d[r][r*8]
    >r
    r@ c@ 7 and 5 = over 0= and if
	2drop 4 $ffff ['] drop
    else
	['] print-base
    endif
    if ( dispsize mask xt-base )
	r@ 1+ @ 2 pick masksx . 'd emit
    endif
    r@ c@ swap execute \ print base ( dispsize mask )
    r@ c@ 3 rshift 7 and rex-prefix @ 2 and 2 lshift + ( d m index-reg )
    dup 4 <> if
	'[ emit .reg64 '* emit
	1 r@ c@ 6 rshift lshift dec.- '] emit
    endif
    2drop r@ 1+ + ;

: mem-modRM ( addr1 -- addr2 )
    \ decode memory operand described by modRM
    >r
    \ get the displacement mask
    displacement-info r@ c@ 6 rshift 2* th 2@ ( dispsize mask r: addr1 )
    r@ c@ 7 and 4 = if
	r> 1+ mem-SIB exit
    endif
    r@ c@ $c7 and 5 = if \ rip+disp32
	2drop 4 $ffffffff r@ 1+ @ swap masksx . ."  [rip] "
    else dup if
	    r@ 1+ @ swap masksx .
	    r@ c@ base-regnum .reg64 ."  d[r] "
	else
	    r@ c@ base-regnum .reg64 ."  [r] "
	endif endif
    r> 1+ + ;

: Ext ( addr1 xt -- addr2 )
    \ decode and print modRM mod and r/m fields as r/m with width given by xt
    >r dup c@ $c0 and $c0 = if
	c@+ base-regnum r> execute exit
    endif
    rdrop mem-modRM ;

: Eb ( addr1 -- addr2 )
    \ decode and print modRM mod and r/m fields as r/m8
    ['] .reg8 Ext ;

: Ed ( addr1 -- addr2 )
    ['] .reg32 Ext ;

: Ev ( addr1 -- addr2 )
    \ decode and print modRM mod and r/m fields as r/m8
    ['] .regv Ext ;

: Iz ( addr1 -- addr2 )
    \ print immediate operand
    >r
    rex-prefix c@ 8 and 0= operand-size @ and if
	2 $ffff
    else
	4 $ffffffff
    endif
    r@ @ swap masksx . ." # "
    r> + ;

\ add-like instruction types

: Eb,Gb ( addr1 addr u -- addr2 )
    2>r 1+ dup Eb space swap Gb space
    2r> type ." b," ;

: Ev,Gv ( addr1 addr u -- addr2 )
    2>r 1+ dup Ev space swap Gv space
    2r> type .width ', emit ;

: Gb,Eb ( addr1 addr u -- addr2 )
    2>r 1+ dup Gb space swap Eb space
    2r> type ." b," ;

: Gv,Ev ( addr1 addr u -- addr2 )
    2>r 1+ dup Gv space swap Ev space
    2r> type .width ', emit ;

: AL,Ib ( addr1 addr u -- addr2 )
    2>r 0 .reg8 space 1+ c@+ $ff masksx . ." # " 2r> type ." b," ;

: RAX,Iz ( addr1 addr u -- addr2 )
    2>r 0 .regv space 1+ Iz 2r> type .width ', emit ;

: set-add-like ( addr u type-xt opcode -- )
    >r >r 2>r
    :noname 2r> postpone sliteral r> compile, postpone ;
    opcode1-table r> th ! ;

: set-add-likes ( addr u base-opcode -- )
    >r
    2dup ['] Eb,Gb  r@ 0 + set-add-like
    2dup ['] Ev,Gv  r@ 1 + set-add-like
    2dup ['] Gb,Eb  r@ 2 + set-add-like
    2dup ['] Gv,Ev  r@ 3 + set-add-like
    2dup ['] AL,Ib  r@ 4 + set-add-like
    2dup ['] RAX,Iz r@ 5 + set-add-like
    2drop rdrop ;

s" add" $00 set-add-likes
s" adc" $10 set-add-likes
s" and" $20 set-add-likes
s" xor" $30 set-add-likes
s" or"  $08 set-add-likes
s" sbb" $18 set-add-likes
s" sub" $28 set-add-likes
s" cmp" $38 set-add-likes

: push-reg ( addr1 -- addr2 )
    c@+ base-regnum .reg64 space ." pushq," ;

opcode1-table $50 th 8 ' push-reg cell-fill

: movsxd ( addr1 -- addr2 )
    1+ dup Gv space swap Ed ."  movsxd," ;

' movsxd opcode1-table $63 th !

create conditions
16 string-table o no c nc z nz na a s ns p np l ge le g

: Jb ( addr1 -- )
    c@ $ff masksx over 2 + + hex. ;

: jcc-short ( addr1 -- addr2 )
    dup 1+ Jb
    'j emit dup c@ $f and 2* cells conditions + 2@ type ', emit
    2 + ;

opcode1-table $70 th $10 ' jcc-short cell-fill

s" test" ' Eb,Gb $84 set-add-like
s" test" ' Ev,Gv $85 set-add-like
s" xchg" ' Eb,Gb $86 set-add-like
s" xchg" ' Ev,Gv $87 set-add-like

: xchg-ax ( addr1 -- addr2 )
    c@+ base-regnum dup 0= if
	drop ." nop," exit
    endif
    .regv space 0 .regv ."  xchg," ;

opcode1-table $90 th 8 ' xchg-ax cell-fill

:noname \ mov-al,Ob ( addr1 -- addr2 )
    0 .reg8 space 1+ Ox ." movb," ;
opcode1-table $a0 th !

:noname \ mov-xAx,Ov ( addr1 -- addr2 )
    0 .regv space 1+ Ox ." mov" .width ', emit ;
opcode1-table $a1 th !

:noname \ mov-Ob,al ( addr1 -- addr2 )
    1+ Ox 0 .reg8 space ." movb," ;
opcode1-table $a2 th !

:noname \ mov-Ov,xAx ( addr1 -- addr2 )
    1+ Ox 0 .regv space ." mov" .width ', emit ;
opcode1-table $a3 th !

:noname ( addr1 -- addr2 )
    ." movsb," 1+ ;
opcode1-table $a4 th !
    
:noname ( addr1 -- addr2 )
    ." movs" .width ', emit 1+ ;
opcode1-table $a5 th !

:noname ( addr1 -- addr2 )
    ." cmpsb," 1+ ;
opcode1-table $a6 th !
    
:noname ( addr1 -- addr2 )
    ." cmps" .width ', emit 1+ ;
opcode1-table $a7 th !

: mov-reg8-ib ( addr1 -- addr2 )
    c@+ base-regnum .reg8 c@+ $ff masksx . ." # mov," ;

opcode1-table $b0 th 8 ' xchg-ax cell-fill

:noname ( addr1 -- addr2 )
    1+ dup @ $ffff masksx, . ." ret#," 2 + ;
opcode1-table $c2 th !

:noname ( addr1 -- addr2 )
    ." ret," 1+ ;
opcode1-table $c3 th !

:noname ( addr1 -- addr2 )
    ." xlatb," 1+ ;
opcode1-table $d7 th !

:noname ( addr1 -- addr2 )
    dup 1+ Jb ." loopnz," ;
opcode1-table $e0 th !

:noname ( addr1 -- addr2 )
    dup 1+ Jb ." loopz," ;
opcode1-table $e1 th !

:noname ( addr1 -- addr2 )
    dup 1+ Jb ." loop," ;
opcode1-table $e2 th !

:noname ( addr1 -- addr2 )
    dup 1+ Jb 'j emit 1 .regv ." z," ;
opcode1-table $e3 th !

:noname ( addr1 -- addr2 )
    0 .reg8 1+ c@+ hex. ." inb," ;
opcode1-table $e4 th !

:noname ( addr1 -- addr2 )
    0 .regv 1+ c@+ hex. ." in" .width ', emit ;
opcode1-table $e5 th !

:noname ( addr1 -- addr2 )
    1+ c@+ hex. 0 .reg8 ." outb," ;
opcode1-table $e4 th !

:noname ( addr1 -- addr2 )
    1+ c@+ hex. 0 .regv ." out" .width ', emit ;
opcode1-table $e5 th !

:noname ( addr1 -- addr2 )
    ." int1," 1+ ;
opcode1-table $f1 th !

:noname ( addr1 -- addr2 )
    ." hlt," 1+ ;
opcode1-table $f4 th !

:noname ( addr1 -- addr2 )
    ." cmc," 1+ ;
opcode1-table $f5 th !


\ !! 80-83: Group1
\ !! c0,c1,d0-d3: Group2
\ !! c6,c7: Group11
\ !! f6,f7: Group3
