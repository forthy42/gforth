\ folowing instructions have an extension, depeding on flags

$10A asm-xo add    
$A   asm-xo addc   
$8A  asm-xo adde
$EA  asm-xo addme  
$CA  asm-xo addze  
$1EB asm-xo divw   
$1CB asm-xo divwu  
$4B  asm-xo mulhw  
$B   asm-xo mulhwu
$EB  asm-xo mullw
$68  asm-xo neg
$28  asm-xo subf
$8   asm-xo subfc
$88  asm-xo subfe
$E8  asm-xo subfme
$C8  asm-xo subfze

\ 64 bit instr.
$1E9 asm-xo divd   
$1C9 asm-xo divdu  
$49  asm-xo mulhd  
$9   asm-xo mulhdu 
$E9  asm-xo mulld

$0 asm-xo-flags 
$1 asm-xo-flags .
$2 asm-xo-flags o
$3 asm-xo-flags o.
