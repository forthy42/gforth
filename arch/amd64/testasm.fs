abi-code ssedup
   \ SP passed in di, returned in ax,  address of FP passed in si
   -16 di d) ax lea        \ compute new sp in result reg
   di )  xmm0 movupd 
   xmm0 ax )  movups
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

abi-code my-s>f
   \ SP passed in di, returned in ax,  address of FP passed in si
   8 di d) ax lea          \ compute new sp in result reg
   8 #  si ) sub           \ adjust FP
   si )  si mov            \ load FP into si
   .q di )  xmm2 cvtsi2sd  \ convert
   xmm2  si ) movsd        \ store FP result
   ret
end-code

abi-code testasm

   .q ax dx mov
   .d ax dx mov
   $1234567890abcdef # ax mov    \ only GPR moves can use 64-bit immediates

   \ scaled index addressing with various address/operand sizes
   dx cx i)  dx lea    
   .da dx cx i)  dx lea
   .d .da dx cx i)  dx lea
   .d dx cx i)  dx lea

   dx cx *2 i)  dx lea
   .da dx cx *2 i)  dx lea
   .d .da dx cx *2 i)  dx lea
   .d dx cx *2 i)  dx lea

   dx cx *4 i)  dx lea
   .da dx cx *4 i)  dx lea
   .d .da dx cx *4 i)  dx lea
   .d dx cx *4 i)  dx lea

   dx cx *8 i)  dx lea
   .da dx cx *8 i)  dx lea
   .d .da dx cx *8 i)  dx lea
   .d dx cx *8 i)  dx lea
   
   \ opcodes that do not get REX prefix
   ax push
   r8 push
   r10 pop
   fs pop
   gs pop
   fs push
   gs push 
   popf
   pushf
   8 # 0 enter 
   leave
   8 # push
   $10000000 # push 
   here rel) call
   here rel) jcxz
   here rel) jnz
   here rel) jmp
   here loop 
   here loopne
   ret
   dx ) lgdt 
   dx ) lidt 
   dx ) lldt  
   ax ltr
   ax 1 cr movxr 
   1 dr ax movxr  

\   ax dx ) arpl    broken?

   \ mmx
   ax ) mm0  paddsw
   mm0  3 # psllq
   mm0  4 # psrlq
   femms
   .q mm0 ax ) movd
   .q ax ) xmm1 movd
   .q mm1 ax ) movd
   .q dx xmm1   movd
   .d mm0 ax ) movd
   .d ax ) xmm1 movd
   .d mm1 ax ) movd
   .d dx mm0   movd
   mm1 mm2  maskmovq

   \ 3dnow tests
   ax ) mm1 pfadd
   ax ) mm1 pfsub
   ax ) mm1 pfmul
   ax ) prefetch
   ax ) prefetchw
   sfence 
  
   \ sse 
   cx xmm2 movd   
\   xmm2 cx movd  \ these cannot be distinguished. ouch.  bastard opcode!
   .q xmm8 ax ) movd
   .q ax ) xmm1 movd
   .q xmm1 ax ) movd
   .q dx xmm1   movd
   .d xmm8 ax ) movd
   .d ax ) xmm1 movd
   .d xmm1 ax ) movd
   .d dx xmm1   movd
   
   ax ) xmm1 movq
   xmm1 ax ) movq
   xmm1 xmm2 movq

   .d xmm8 ax ) movd
   .d ax ) xmm1 movd
   .d xmm1 ax ) movd
   .d dx xmm1   movd

   xmm8 ax ) movdqa
   ax ) xmm1 movdqa
   xmm1 ax ) movdqu
   dx xmm1   movdqu

   ax ) xmm1  movups
   xmm1 ax )  movups
   xmm1 xmm8  movups
   ax ) xmm1 movupd
   
   mm1 mm2  maskmovdqu

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

   .d xmm2  dx  cvtss2si
   .d ax )  dx  cvtss2si   
   .q xmm2  dx  cvtss2si
   .q ax )  dx  cvtss2si
   .d xmm2  dx cvtsd2si
   .d dx    xmm2 cvtsi2sd
   .d ax )  xmm2 cvtsi2sd
   .d dx    xmm2 cvtsi2ss
   .d ax )  xmm2 cvtsi2ss
   .q dx    xmm2 cvtsi2ss
   .q ax )  xmm2 cvtsi2ss
   
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

   \ sse2 (i.e. mostlq MMX opcodes with XMM 128-bit regs)
   ax ) xmm0  paddsw
   xmm0  3 # psllq
   xmm0  4 # psrlq
   xmm0  3 # pslldq
   xmm0  4 # psrldq

end-code

see testasm
