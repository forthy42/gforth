20 1 oprrnnn rlwimi.
20 0 oprrnnn rlwimi ( <- must be after rlwimi. )
21 1 oprrnnn rlwinm.
21 0 oprrnnn rlwinm ( <- must be after rlwinm. )
23 1 oprrrnn rlwnm.
23 0 oprrrnn rlwnm ( <- must be after rlwnm. )

63 711 ~RC opifr mtfsf 
63 711 RC opifr mtfsf. 
63 134 ~RC opcrf0i mtfsfi
63 134 RC opcrf0i mtfsfi.

31 242 ~RC opr0r mtsrin
31 210 ~RC opswrn0 mtsr
31 467 ~RC opswri0 mtspr
31 149 ~RC opr00 mtmsr 
63 70 ~RC opcrb00 mtfsb0
63 70 RC opcrb00 mtfsb0.
63 38 ~RC opcrb00 mtfsb1
63 38 RC opcrb00 mtfsb1.

31 144 ~RC opcrm mtcrf

31 659 ~RC opr0r mfsrin
31 595 ~RC oprn0 mfsr 
31 339 ~RC opri0 mfspr 
31 371 ~RC opri0 mftb 
31 83 ~RC opr00 mfmsr 
31 19 ~RC opr00 mfcr 
63 583 ~RC opfr00 mffs 
31 512 ~RC opcr00 mcrxr

17 opsc sc 
19 50 ~RC opc000 rfi


31 824 ~RC opswrrn srawi
31 824 RC opswrrn srawi.

31 725 ~RC oprrn stswi 
31 306 ~RC op00r tlbie 
3 opnri twi
31 4 ~RC opnrr tw 

19 150 ~RC opc000 isync
19 0 ~RC opcrcr0 mcrf 

63 64 ~RC opfcrcr0 mcrfs 

31 597 ~RC oprrn lswi

59 24 ~RC opsfr0fr fres
59 24 RC opsfr0fr fres.
63 24 ~RC opfr0fr frsp
64 24 RC opfr0fr frsp.
63 26 ~RC opfr0fr frsqrte
63 26 RC opfr0fr frsqrte.
63 22 ~RC opfr0fr fsqrt
63 22 RC opfr0fr fsqrt.
59 22 ~RC opfr0fr fsqrts
59 22 RC opfr0fr fsqrts.

63 23 ~RC opfrfrfrfr fsel
63 23 RC opfrfrfrfr fsel.

63 20 ~RC opfrfrfr fsub
63 20 RC opfrfrfr fsub.
59 20 ~RC opfrfrfr fsubs
59 20 RC opfrfrfr fsubs.

63 31 ~RC opfrfrfrfr fnmadd
63 31 RC opfrfrfrfr fnmadd.
59 31 ~RC opsfrfrfrfr fnmadds
59 31 RC opsfrfrfrfr fnmadds.
63 30 ~RC opfrfrfrfr fnmsub
63 30 RC opfrfrfrfr fnmsub.
59 30 ~RC opsfrfrfrfr fnmsubs
59 30 RC opsfrfrfrfr fnmsubs.

63 136 ~RC opfr0fr fnabs
63 136 RC opfr0fr fnabs.
63 40 ~RC opfr0fr fneg
63 40 RC opfr0fr fneg.

63 25 ~RC opfrfr0fr fmul
63 25 RC opfrfr0fr fmul.
59 25 ~RC opsfrfr0fr fmuls
59 25 RC opsfrfr0fr fmuls.

63 28 ~RC opfrfrfrfr fmsub
63 28 RC opfrfrfrfr fmsub.
59 28 ~RC opsfrfrfrfr fmsubs
59 28 RC opsfrfrfrfr fmsubs.


63 29 ~RC opfrfrfrfr fmadd
63 29 RC opfrfrfrfr fmadd.
59 29 ~RC opsfrfrfrfr fmadds
59 29 RC opsfrfrfrfr fmadds.

63 72 ~RC opfr0fr fmr 
63 72 RC opfr0fr fmr.

63 264 ~RC opfr0fr fabs
63 264 RC opfr0fr fabs.
63 21 ~RC opfrfrfr fadd
63 21 RC opfrfrfr fadd.
59 21 ~RC opsfrfrfr fadds
59 21 RC opsfrfrfr fadds.
63 32 ~RC opcrffrfr fcmpo
63 0 ~RC opcrffrfr fcmpu
63 14 ~RC opfr0fr fctiw
63 14 RC opfr0fr fctiw.
63 15 ~RC opfr0fr fctiwz
63 15 RC opfr0fr fctiwz.
63 18 ~RC opfrfrfr fdiv
63 18 RC opfrfrfr fdiv.
59 18 ~RC opsfrfrfr fdivs
59 18 RC opsfrfrfr fdivs.

31 854 ~RC op000 eieio
31 566 ~RC op000 tlbsync 
31 598 ~RC op000 sync 
31 370 ~RC op000 tlbia 
31 954 ~RC opswrr0 extsb
31 954 RC opswrr0 extsb.
31 922 ~RC opswrr0 extsh
31 922 RC opswrr0 extsh.

31 0 ~RC opcrfrr cmp
31 32 ~RC opcrfrr cmpl
11 opcrfri cmpi
10 opcrfri cmpli

19 528 ~RC opbii bcctr
19 528 RC opbii bcctrl
19 16 ~RC opbii bclr
19 16 RC opbii bclrl

34 oparri lbz
35 oparri lbzu
50 opafrri lfd
51 opafrri lfdu
31 631 ~RC opfrrr lfdux
31 599 ~RC opfrrr lfdx
48 opafrri lfs
49 opafrri lfsu
31 567 ~RC opfrrr lfsux
31 535 ~RC opfrrr lfsx

43 oparri lhau
42 oparri lha
40 oparri lhz
41 oparri lhzu
46 oparri lmw
32 oparri lwz
33 oparri lwzu 
31 124 ~RC opswrrr nor
31 124 RC opswrrr nor.
31 476 ~RC opswrrr nand
31 476 RC opswrrr nand.

31 412 ~RC opswrrr orc
31 412 RC opswrrr orc.
31 284 ~RC opswrrr eqv
31 284 RC opswrrr eqv.

24 opswrri ori
25 opswrri oris

31 24 ~RC opswrrr slw
31 24 RC opswrrr slw.

31 792 ~RC opswrrr sraw
31 792 RC opswrrr sraw.
31 536 ~RC opswrrr srw
31 536 RC opswrrr srw.

38 oparri stb
39 oparri stbu
55 opafrri stfdu
54 opafrri stfd
31 759 ~RC opfrrr stfdux
31 727 ~RC opfrrr stfdx
31 983 ~RC opfrrr stfiwx
52 opafrri stfs
53 opafrri stfsu 
31 695 ~RC opfrrr stfsux
31 663 ~RC opfrrr stfsx

47 oparri stmw
45 oparri sthu
44 oparri sth
36 oparri stw
37 oparri stwu
31 661 ~RC oprrr stswx  

31 316 ~RC opswrrr xor
31 316 RC opswrrr xor.
27 opswrri xoris
26 opswrri xori

31 ~OE 232 or ~RC oprr0 subfme  
31 ~OE 232 or RC oprr0 subfme.  
31 OE 232 or ~RC oprr0 subfmeo  
31 OE 232 or RC oprr0 subfmeo.  

31 ~OE 200 or ~RC oprr0 subfze  
31 ~OE 200 or RC oprr0 subfze.  
31 OE 200 or ~RC oprr0 subfzeo  
31 OE 200 or RC oprr0 subfzeo.  

8 oprri subfic

31 ~OE 136 or ~RC oprrr subfe  
31 ~OE 136 or RC oprrr subfe.  
31 OE 136 or ~RC oprrr subfeo  
31 OE 136 or RC oprrr subfeo.  

31 ~OE 8 or ~RC oprrr subfc  
31 ~OE 8 or RC oprrr subfc.  
31 OE 8 or ~RC oprrr subfco  
31 OE 8 or RC oprrr subfco.  

31 ~OE 40 or ~RC oprrr subf  
31 ~OE 40 or RC oprrr subf.  
31 OE 40 or ~RC oprrr subfo  
31 OE 40 or RC oprrr subfo.  


31 151 ~RC oprrr stwx
31 183 ~RC oprrr stwux
31 150 ~RC oprrr stwcx.
31 662 ~RC oprrr stwbrx
31 439 ~RC oprrr sthux 
31 407 ~RC oprrr sthx 
31 918 ~RC oprrr sthbrx 
31 247 ~RC oprrr stbux
31 215 ~RC oprrr stbx

12 oprri addic
13 oprri addic.
14 oprri addi
15 oprri addis

18 0 opbr b
18 1 opbr ba
18 2 opbr bl
18 3 opbr bla

16 0 opbc bc
16 1 opbc bca
16 2 opbc bcl
16 3 opbc bcla

19 257 ~RC  opcrcrcr crand
19 129 ~RC  opcrcrcr crandc
19 289 ~RC  opcrcrcr creqv
19 225 ~RC  opcrcrcr crnand
19 33 ~RC  opcrcrcr crnor
19 449 ~RC  opcrcrcr cror
19 417 ~RC  opcrcrcr crorc
19 193 ~RC  opcrcrcr crxor

31 75 ~RC oprrr mulhw
31 75 RC oprrr mulhw.
31 11 ~RC oprrr mulhwu
31 11 RC oprrr mulhwu.

31 ~OE 104 or ~RC oprr0 neg  
31 ~OE 104 or RC oprr0 neg.  
31 OE 104 or ~RC oprr0 nego  
31 OE 104 or RC oprr0 nege.  


7 oprri mulli

31 ~OE 235 or ~RC oprrr mullw  
31 ~OE 235 or RC oprrr mullw.  
31 OE 235 or ~RC oprrr mullwo  
31 OE 235 or RC oprrr mullwo.  


31 119 ~RC oprrr lbzux
31 87 ~RC oprrr lbzx
31 357 ~RC oprrr lhaux
31 343 ~RC oprrr lhax
31 790 ~RC oprrr lhbrx
31 311 ~RC oprrr lhzux
31 279 ~RC oprrr lhzx
31 533 ~RC oprrr lswx
31 20 ~RC oprrr lwarx
31 534 ~RC oprrr lwbrx
31 55 ~RC oprrr lwzux
31 23 ~RC oprrr lwzx


28 opswrri andi.
28 opswrri andis.

31 ~OE 10 or ~RC oprrr addc  
31 ~OE 10 or RC oprrr addc.  
31 OE 10 or ~RC oprrr addco  
31 OE 10 or RC oprrr addco.  

31 26 ~RC opswrr0 cntlzw
31 26 RC opswrr0 cntlzw.

31 28 ~RC opswrrr and
31 28 RC opswrrr and.

31 60 ~RC opswrrr andc
31 60 RC opswrrr andc.

31 ~OE 133 or ~RC oprrr adde  
31 ~OE 133 or RC oprrr adde.  
31 OE 133 or ~RC oprrr addeo  
31 OE 133 or RC oprrr addeo.  

31 ~OE 202 or ~RC oprr0 addze  
31 ~OE 202 or RC oprr0 addze.  
31 OE 202 or ~RC oprr0 addzeo  
31 OE 202 or RC oprr0 addoze.  

31 ~OE 234 or ~RC oprr0 addme  
31 ~OE 234 or RC oprr0 addme.  
31 OE 234 or ~RC oprr0 addmeo  
31 OE 234 or RC oprr0 addome.  

31 ~OE 266 or ~RC oprrr add  
31 ~OE 266 or RC oprrr add.  
31 OE 266 or ~RC oprrr addo  
31 OE 266 or RC oprrr addo.  

31 ~OE 491 or ~RC oprrr divw  
31 ~OE 491 or RC oprrr divw.  
31 OE 491 or ~RC oprrr divwo  
31 OE 491 or RC oprrr divwo.  

31 ~OE 459 or ~RC oprrr divwu  
31 ~OE 459 or RC oprrr divwu.  
31 OE 459 or ~RC oprrr divwuo  
31 OE 459 or RC oprrr divwuo.  

31 310 ~RC oprrr eciwx
31 438 ~RC oprrr ecowx



31 758 ~RC op0rr dcba 
31 86 ~RC op0rr dcbf 
31 45 ~RC op0rr dcbst
31 470 ~RC op0rr dcbi
31 278 ~RC op0rr dcbt
31 246 ~RC op0rr dcbtst
31 1014 ~RC op0rr dcbz

31 982 ~RC op0rr icbi

31 444 ~RC opswrrr or ( <- must be last )
31 444 RC opswrrr or. ( <- must be last )
