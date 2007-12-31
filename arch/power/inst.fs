\ PPC32/64 instruction encoding descriptions common to asm.fs and disasm.fs

\ Copyright (C) 2006,2007 Free Software Foundation, Inc.

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

\ xo-form
$10A asm-xo-1 add    
$A   asm-xo-1 addc   
$8A  asm-xo-1 adde
$1EB asm-xo-1 divw   
$1CB asm-xo-1 divwu  
$4B  asm-xo-1 mulhw  
$B   asm-xo-1 mulhwu
$EB  asm-xo-1 mullw
$28  asm-xo-1 subf
$8   asm-xo-1 subfc
$88  asm-xo-1 subfe

$EA  asm-xo-2 addme  
$CA  asm-xo-2 addze  
$68  asm-xo-2 neg
$E8  asm-xo-2 subfme
$C8  asm-xo-2 subfze


\ 64 bit instr.
$1E9 asm-xo-1 divd   
$1C9 asm-xo-1 divdu  
$49  asm-xo-1 mulhd  
$9   asm-xo-1 mulhdu 
$E9  asm-xo-1 mulld

\ d-form
$E   asm-d-oper-1 addi
$C   asm-d-oper-1 addic
$D   asm-d-oper-1 addic.
$F   asm-d-oper-1 addis
$1C  asm-d-oper-2 andi.
$1D  asm-d-oper-2 andis.
$B   asm-d-compare-1 cmpi
$A   asm-d-compare-2 cmpli
$22  asm-d-load-store lbz
$23  asm-d-load-store lbzu
$32  asm-d-load-store lfd
$33  asm-d-load-store lfdu
$30  asm-d-load-store lfs
$31  asm-d-load-store lfsu
$2A  asm-d-load-store lha
$2B  asm-d-load-store lhau
$28  asm-d-load-store lhz
$29  asm-d-load-store lhzu
$2E  asm-d-load-store lmw
$20  asm-d-load-store lwz
$21  asm-d-load-store lwzu
$7   asm-d-oper-1 mulli
$18  asm-d-oper-2 ori
$19  asm-d-oper-2 oris
$26  asm-d-load-store stb
$27  asm-d-load-store stbu
$36  asm-d-load-store stfd
$37  asm-d-load-store stfdu
$34  asm-d-load-store stfs
$35  asm-d-load-store stfsu
$2C  asm-d-load-store sth
$2D  asm-d-load-store sthu
$2F  asm-d-load-store stmw
$24  asm-d-load-store stw
$25  asm-d-load-store stwu
$8   asm-d-oper-1 subfic
$3   asm-d-oper-1 twi
$1A  asm-d-oper-2 xori
$1B  asm-d-oper-2 xoris

\ 64-bit
$2   asm-d-oper-1 tdi

\ ds-form, 64 bit
$0  asm-ds-1 ld
$1  asm-ds-1 ldu
$2  asm-ds-1 lwa

$0  asm-ds-2 std
$1  asm-ds-2 stdu


\ x-form
$1C  asm-x-1 and
$3C  asm-x-1 andc
$11C asm-x-1 eqv
$18  asm-x-1 slw
$318 asm-x-1 sraw
$338 asm-x-1 srawi
$218 asm-x-1 srw
$1DC asm-x-1 nand
$7C  asm-x-1 nor
$1BC asm-x-1 or
$19C asm-x-1 orc
$13C asm-x-1 xor
$F7  asm-x-2 stbux
$D7  asm-x-2 stbx
$2F7 asm-x-2 stfdux
$2D7 asm-x-2 stfdx
$3D7 asm-x-2 stfiwx
$2B7 asm-x-2 stfsux
$297 asm-x-2 stfsx
$396 asm-x-2 sthbrx
$1B7 asm-x-2 sthux
$197 asm-x-2 sthx
$295 asm-x-2 stswx
$296 asm-x-2 stwbrx
$96  asm-x-2-1 stwcx
$B7  asm-x-2 stwux
$97  asm-x-2 stwx
$136 asm-x-2 eciwx
$1B6 asm-x-2 ecowx
$77  asm-x-2 lbzux
$57  asm-x-2 lbzx
$277 asm-x-2 lfdux
$257 asm-x-2 lfdx
$237 asm-x-2 lfsux
$217 asm-x-2 lfsx
$177 asm-x-2 lhaux
$157 asm-x-2 lhax
$316 asm-x-2 lhbrx
$137 asm-x-2 lhzux
$117 asm-x-2 lhzx
$215 asm-x-2 lswx
$14  asm-x-2 lwarx
$216 asm-x-2 lwbrx
$37  asm-x-2 lwzux
$17  asm-x-2 lwzx
$4   asm-x-2 tw
$0   asm-x-3 cmp
$20  asm-x-3 cmpl
$356 asm-x-4 eieio
$256 asm-x-4 sync
$172 asm-x-4 tlbia
$236 asm-x-4 tlbsync
$1A  asm-x-5 cntlzw
$3BA asm-x-5 extsb
$39A asm-x-5 extsh
$2F6 asm-x-6 dcba
$56  asm-x-6 dcbf
$1D6 asm-x-6 dcbi
$36  asm-x-6 dcbst
$116 asm-x-6 dcbt
$F6  asm-x-6 dcbtst
$3F6 asm-x-6 dcbz
$3D6 asm-x-6 icbi
$200 asm-x-7 mcrxr
$13  asm-x-8-31 mfcr
$53  asm-x-8-31 mfmsr
$92  asm-x-8-31 mtmsr
$247 asm-x-8-63 mffs
$46  asm-x-8-63 mtfsb0
$26  asm-x-8-63 mtfsb1
$253 asm-x-9 mfsr
$293 asm-x-10-31 mfsrin
$72  asm-x-10-31 mtsrdin
$108 asm-x-10-63 fabs
$E   asm-x-10-63 fctiw
$F   asm-x-10-63 fctiwz
$48  asm-x-10-63 fmr
$88  asm-x-10-63 fnabs
$28  asm-x-10-63 fneg
$C   asm-x-10-63 frsp
$D2  asm-x-11 mtsr
$52  asm-x-11 mtsrd
$132 asm-x-12 tlbie
$20  asm-x-13 fcmpo
$0   asm-x-13 fcmpu
$40  asm-x-14 mcrfs
$86  asm-x-15 mtfsfi
$255 asm-x-16 lswi
$2D5 asm-x-16 stswi

\ 32-bit only
$F2  asm-x-10-31 mtsrin

\ 64-bit
$1B  asm-x-1 sld
$31A asm-x-1 srad
$21B asm-x-1 srd
$D6  asm-x-2-1 stdcx
$B5  asm-x-2 stdux
$95  asm-x-2 stdx
$44  asm-x-2 td
$54  asm-x-2 ldarx
$35  asm-x-2 ldux
$15  asm-x-2 ldx
$175 asm-x-2 lwaux
$155 asm-x-2 lwax
$1F2 asm-x-4 slbia
$3A  asm-x-5 cntlzd
$3DA asm-x-5 extsw
$B2  asm-x-8-31 mtmsrd
$34E asm-x-10-63 fcfid
$32E asm-x-10-63 fctid
$32F asm-x-10-63 fctidz
$1B2 asm-x-12 slbie

\ xfx-form
$153 asm-xfx-1 mfspr
$173 asm-xfx-1 mftb
$90  asm-xfx-2 mtcrf
$1D3 asm-xfx-3 mtspr

\ xs-form
$19D asm-xs sradi

\ xl-form
$210 asm-xl-1 bcctr
$10  asm-xl-1 bclr
$101 asm-xl-2 crand
$81  asm-xl-2 crandc
$121 asm-xl-2 creqv
$E1  asm-xl-2 crnand
$21  asm-xl-2 crnor
$1C1 asm-xl-2 cror
$1A1  asm-xl-2 crorc
$C1  asm-xl-2 crxor
$96  asm-xl-3 isync
$0   asm-xl-4 mcrf
$32  asm-xl-3 rfi

\ 64 bit
$12  asm-xl-3 rfid

\ m-form

$14 asm-m rlwimi
$15 asm-m rlwinm
$17 asm-m rlwnm

\ md-form 64 bit

$2 asm-md rldic
$0 asm-md rldicl
$1 asm-md rldicr
$3 asm-md rldimi

\ mds-form 64 bit

$8 asm-mds rldcl
$9 asm-mds rldcr

\ a-form
$15 asm-a-1-63 fadd
$12 asm-a-1-63 fdiv
$14 asm-a-1-63 fsub
$1D asm-a-2-63 fmadd
$1C asm-a-2-63 fmsub
$1F asm-a-2-63 fnmadd
$1E asm-a-2-63 fnmsub
$17 asm-a-2-63 fsel
$19 asm-a-3-63 fmul
$1A asm-a-4-63 frsqrte
$16 asm-a-4-63 fsqrt

$15 asm-a-1-59 fadds
$12 asm-a-1-59 fdivs
$14 asm-a-1-59 fsubs
$1D asm-a-2-59 fmadds
$1C asm-a-2-59 fmsubs
$1F asm-a-2-59 fnmadds
$1E asm-a-2-59 fnmsubs
$19 asm-a-3-59 fmuls
$18 asm-a-4-59 fres
$16 asm-a-4-59 fsqrts

\ b-form

$0 asm-i-reladdr b
$1 asm-i-absaddr b
$0 asm-b-reladdr bc
$1 asm-b-absaddr bc

\ xfl-form

$2C7 asm-xfl mtfsf

\ sc-form

$11 asm-sc sc
