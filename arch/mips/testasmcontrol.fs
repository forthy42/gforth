\ run this with

\ gforth arch/mips/asm.fs arch/mips/disasm.fs arch/mips/testasmcontrol.fs -e "' foo >body 16 disasm"

\ and it will produce something like

\ ( $400EBA98 ) 1 11 10 sltu,
\ ( $400EBA9C ) 1 0 4 bne,
\ ( $400EBAA0 ) 0 0 4 beq,
\ ( $400EBAA4 ) 0 0 -8 beq,

code foo
    10 11 leu if,
    begin,
    ahead,
    2 cs-roll
    then,
    1 cs-roll
    again,
    then,
end-code

