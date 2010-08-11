abi-code mmxdup
   \ SP passed in di, returned in ax,  address of FP passed in si
   -16 di d) ax lea        \ compute new sp in result reg
   di )  mm0 movupd 
   mm0 ax )  movups
   ret
end-code

abi-code my-f+
   \ SP passed in di, returned in ax,  address of FP passed in si
   si )  ax mov            \ load FP into ax
   8 ax d)  dx lea         \ computed resulting FP in dx
   ax )  xmm0 movsd        \ load floating point TOS into xmm0
   dx )  xmm0 addsd        \ added from NOS to xmm0
   xmm0 dx )  movsd        \ write TOS to FP stack
   dx  si ) mov            \ write resulting FP back
   di ax mov               \ return SP unchanged
   ret
end-code

abi-code testasm
   \ mmx/3dnow tests (this was completely bugged!)
   sfence 
   femms
   ax ) mm1 pfadd
   ax ) mm1 pfsub
   ax ) mm1 pfmul
   ax ) prefetch
   ax ) prefetchw

   
\   cx xmm2 movd   \ these cannot be distinguished. ouch.  bastard opcode!
\   xmm2 cx movd
\   xmm8 ax ) movd  \ either this doesn't work or GDB disassembler is buggy
   ax ) xmm1 movd
   xmm1 ax ) movd
   dx xmm1   movd
   
   ax ) xmm1 movq
   xmm1 ax ) movq
   xmm1 xmm2 movq

   ax ) xmm1  movups
   xmm1 ax )  movups
   xmm1 xmm8  movups
   ax ) xmm1 movupd
   
   ax ) xmm1 movlps   ax ) xmm1 movlpd
   ax ) xmm1 movhps   ax ) xmm1 movhpd
   ax ) xmm1 movaps   ax ) xmm1 movapd
   ax ) xmm1 movss    ax ) xmm1 movsd

   ax ) xmm1 addps   ax ) xmm1 addpd  ax ) xmm1 addss  ax ) xmm1 addsd
   ax ) xmm1 addps   ax ) xmm1 addpd  ax ) xmm1 addss  ax ) xmm1 addsd
   ax ) xmm1 subps   ax ) xmm1 subpd  ax ) xmm1 subss  ax ) xmm1 subsd
   ax ) xmm1 maxps   ax ) xmm1 maxpd  ax ) xmm1 maxss  ax ) xmm1 maxsd
   ax ) xmm1 minps   ax ) xmm1 minpd  ax ) xmm1 minss  ax ) xmm1 minsd
   ax ) xmm1 mulps   ax ) xmm1 mulpd  ax ) xmm1 mulss  ax ) xmm1 mulsd
   ax ) xmm1 divps   ax ) xmm1 divpd  ax ) xmm1 divss  ax ) xmm1 divsd
   ax ) xmm8 addps
   
   ax ) xmm1 andps
   ax ) xmm1 andpd
   ax ) xmm1 andnps
   ax ) xmm1 andnpd
   ax ) xmm1 orps
   ax ) xmm1 orpd
   ax ) xmm1 xorps
   ax ) xmm1 xorpd
   ax ) xmm1 ucomiss
   ax ) xmm1 ucomisd
   ax ) xmm1 comiss
   ax ) xmm1 comisd
   ax ) xmm1 cvtdq2ps
   ax ) xmm1 cvtps2dq
   ax ) xmm1 cvttps2dq
   ax ) xmm1 cvtdq2pd
   ax ) xmm1 cvtpd2dq
   ax ) xmm1 cvttpd2dq
   ax ) xmm1 cvtps2pi
   ax ) xmm1 cvtpd2pi
   ax ) xmm1 cvtpi2ps
   ax ) xmm1 cvtpi2pd
   ax ) xmm1 divps
   ax ) xmm1 divpd
   ax ) xmm1 haddps
   ax ) xmm1 haddpd
   ax ) xmm1 hsubps
   ax ) xmm1 hsubpd
   ax ) xmm1 addsubps
   ax ) xmm1 addsubpd
   ax ) xmm1 cmpeqps
   ax ) xmm1 cmpltps
   ax ) xmm1 cmpleps
   ax ) xmm1 cmpunordps
   ax ) xmm1 cmpneqps
   ax ) xmm1 cmpnltps
   ax ) xmm1 cmpnleps
   ax ) xmm1 cmpordps
   ax ) xmm1 cmpeqpd
   ax ) xmm1 cmpltpd
   ax ) xmm1 cmplepd
   ax ) xmm1 cmpunordpd
   ax ) xmm1 cmpneqpd
   ax ) xmm1 cmpnltpd
   ax ) xmm1 cmpnlepd
   ax ) xmm1 cmpordpd
   
   ax ) xmm1 cmpeqss
   ax ) xmm1 cmpltss
   ax ) xmm1 cmpless
   ax ) xmm1 cmpunordss
   ax ) xmm1 cmpneqss
   ax ) xmm1 cmpnltss
   ax ) xmm1 cmpnless
   ax ) xmm1 cmpordss
   
   ax ) xmm1 cmpeqsd
   ax ) xmm1 cmpltsd
   ax ) xmm1 cmplesd
   ax ) xmm1 cmpunordsd
   ax ) xmm1 cmpneqsd
   ax ) xmm1 cmpnltsd
   ax ) xmm1 cmpnlesd
   ax ) xmm1 cmpordsd
   
end-code

see testasm
