
hex

label cpu-cold
     FF #. LDX,
           TXS,
           SEI,             \ disable IRQ
           CLD,             \ Decimal off
02 #. LDA,
\ LED BIT1 #. LDA,            \ LED on
        WP STA,
     03 #. LDA,             \ Init Port
   WPSHD   STA,
   WP      STA,
   IntoForth JMP,
end-label

unlock >rom $fff6 tdp !
lock
	  0 , \ FFF6
cpu-cold ,  \ FFF8 Warm-Start
0  ,  \ FFFA IRQ
cpu-cold ,  \ FFFC Cold-Start
0  ,  \ FFFE NMI
