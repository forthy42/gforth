cr ." Test for asm.fs" cr

: exec ( ... xt u -- ... w1 ... wu )
    >r execute r> 1+ cells cell ?do
	here i - @
    cell +loop ;

: same ( valn ... val0 u -- flag )
    true swap dup 4 + 4 ?do
	i 2dup pick
	rot rot + 1- pick
	= rot and swap
    loop
    swap >r 2* 0 ?do
	drop
    loop
    r> ;

variable asm-xt
variable asm-u
variable asm-z

: save ( xt u -- )
    asm-u !
    dup >name .name asm-xt ! ;

: check ( ... -- )
    asm-xt @ asm-u @ dup >r exec r> same if
	." OK "
    else
	." NOK "
    endif ;

: asm-test0 ( val xt u -- )
    save
    check cr ;

: asm-test2 ( val val xt u -- )
    save
    $ffffffff check
    $1 check cr ;

: asm-test2i ( val val xt u -- )
    save
    $fffffffc check
    $4 check cr ;

: asm-test2-copzi ( val val z xt u -- )
    save asm-z !
    $fffffffc asm-z @ check
    $4 asm-z @ check cr ;

: asm-test4 ( val val val val xt u -- )
    save
    $ffffffff $ffffffff check
    $0 $1 check
    $1 $0 check
    $1 $1 check cr ;

: asm-test4i ( val val val val xt u -- )
    save
    $ffffffff $fffffffc check
    $0 $4 check
    $1 $0 check
    $1 $4 check cr ;

: asm-test4-copz ( val val val val z xt u -- )
    save asm-z !
    $ffffffff $ffffffff asm-z @ check
    $0 $1 asm-z @ check
    $1 $0 asm-z @ check
    $1 $1 asm-z @ check cr ;

: asm-test5 ( val val val val val xt u -- )
    save
    $ffffffff $ffffffff $ffffffff check
    $0 $0 $1 check
    $0 $1 $0 check
    $1 $0 $0 check
    $1 $1 $1 check cr ;

: asm-test5i ( val val val val val xt u -- )
    save
    $ffffffff $ffffffff $fffffffc check
    $0 $0 $4 check
    $0 $1 $0 check
    $1 $0 $0 check
    $1 $1 $4 check cr ;

: asm-test5-copz ( val val val val val z xt u -- )
    save asm-z !
    $ffffffff $ffffffff $ffffffff asm-z @ check
    $0 $0 $1 asm-z @ check
    $0 $1 $0 asm-z @ check
    $1 $0 $0 asm-z @ check
    $1 $1 $1 asm-z @ check cr ;

$00210820 $00000820 $00200020 $00010020 $03fff820 ' add, 1 asm-test5
$20210001 $20010000 $20200000 $20000001 $23ffffff ' addi, 1 asm-test5
$24210001 $24010000 $24200000 $24000001 $27ffffff ' addiu, 1 asm-test5
$00210821 $00000821 $00200021 $00010021 $03fff821 ' addu, 1 asm-test5
$00210824 $00000824 $00200024 $00010024 $03fff824 ' and, 1 asm-test5
$30210001 $30010000 $30200000 $30000001 $33ffffff ' andi, 1 asm-test5
$45000001 $4500ffff 1 ' bczf, 1 asm-test2-copzi
$45010001 $4501ffff 1 ' bczt, 1 asm-test2-copzi
$10210001 $10200000 $10010000 $10000001 $13ffffff ' beq, 1 asm-test5i
$04210001 $04210000 $04010001 $07e1ffff ' bgez, 1 asm-test4i
$04310001 $04310000 $04110001 $07f1ffff ' bgezal, 1 asm-test4i
$1c200001 $1c200000 $1c000001 $1fe0ffff ' bgtz, 1 asm-test4i
$18200001 $18200000 $18000001 $1be0ffff ' blez, 1 asm-test4i
$04200001 $04200000 $04000001 $07e0ffff ' bltz, 1 asm-test4i
$04300001 $04300000 $04100001 $07f0ffff ' bltzal, 1 asm-test4i
$14210001 $14200000 $14010000 $14000001 $17ffffff ' bne, 1 asm-test5i
$0000000d ' break, 1 asm-test0
$44410800 $44410000 $44400800 $445ff800 1 ' cfcz, 1 asm-test4-copz
$44c10800 $44c10000 $44c00800 $44dff800 1 ' ctcz, 1 asm-test4-copz
$0021001a $0020001a $0001001a $03ff001a ' div, 1 asm-test4
$0021001b $0020001b $0001001b $03ff001b ' divu, 1 asm-test4
$08000001 $0bffffff ' j, 1 asm-test2i
$0c000001 $0fffffff ' jal, 1 asm-test2i
$00200809 $00000809 $00200009 $03e0f809 ' jalr, 1 asm-test4
$00200008 $03e00008 ' jr, 1 asm-test2
$80210001 $80010000 $80000001 $80200000 $83ffffff ' lb, 1 asm-test5
$90210001 $90010000 $90000001 $90200000 $93ffffff ' lbu, 1 asm-test5
$84210001 $84010000 $84000001 $84200000 $87ffffff ' lh, 1 asm-test5
$94210001 $94010000 $94000001 $94200000 $97ffffff ' lhu, 1 asm-test5
$3c010001 $3c010000 $3c000001 $3c1fffff ' lui, 1 asm-test4
$8c210001 $8c010000 $8c000001 $8c200000 $8fffffff ' lw, 1 asm-test5
$c4210001 $c4010000 $c4000001 $c4200000 $c7ffffff 1 ' lwcz, 1 asm-test5-copz
$88210001 $88010000 $88000001 $88200000 $8bffffff ' lwl, 1 asm-test5
$98210001 $98010000 $98000001 $98200000 $9bffffff ' lwr, 1 asm-test5
$44010800 $44010000 $44000800 $441ff800 1 ' mfcz, 1 asm-test4-copz
$00000810 $0000f810 ' mfhi, 1 asm-test2
$00000812 $0000f812 ' mflo, 1 asm-test2
$44810800 $44810000 $44800800 $449ff800 1 ' mtcz, 1 asm-test4-copz
$00200011 $03e00011 ' mthi, 1 asm-test2
$00200013 $03e00013 ' mtlo, 1 asm-test2
$00210018 $00200018 $00010018 $03ff0018 ' mult, 1 asm-test4
$00210019 $00200019 $00010019 $03ff0019 ' multu, 1 asm-test4
$00210827 $00000827 $00200027 $00010027 $03fff827 ' nor, 1 asm-test5
$00210825 $00000825 $00200025 $00010025 $03fff825 ' or, 1 asm-test5
$34210001 $34010000 $34200000 $34000001 $37ffffff ' ori, 1 asm-test5
$a0210001 $a0010000 $a0000001 $a0200000 $a3ffffff ' sb, 1 asm-test5
$a4210001 $a4010000 $a4000001 $a4200000 $a7ffffff ' sh, 1 asm-test5
$0021082a $0000082a $0020002a $0001002a $03fff82a ' slt, 1 asm-test5
$28210001 $28010000 $28200000 $28000001 $2bffffff ' slti, 1 asm-test5
$2c210001 $2c010000 $2c200000 $2c000001 $2fffffff ' sltiu, 1 asm-test5
$0021082b $0000082b $0020002b $0001002b $03fff82b ' sltu, 1 asm-test5
$00210822 $00000822 $00200022 $00010022 $03fff822 ' sub, 1 asm-test5
$00210823 $00000823 $00200023 $00010023 $03fff823 ' subu, 1 asm-test5
$ac210001 $ac010000 $ac000001 $ac200000 $afffffff ' sw, 1 asm-test5
$e4210001 $e4010000 $e4000001 $e4200000 $e7ffffff 1 ' swcz, 1 asm-test5-copz
$a8210001 $a8010000 $a8000001 $a8200000 $abffffff ' swl, 1 asm-test5
$b8210001 $b8010000 $b8000001 $b8200000 $bbffffff ' swr, 1 asm-test5
$0000000c ' syscall, 1 asm-test0
$42000008 ' tlbl, 1 asm-test0
$42000001 ' tlbr, 1 asm-test0
$42000002 ' tlbwi, 1 asm-test0
$42000006 ' tlbwr, 1 asm-test0
$00210826 $00000826 $00200026 $00010026 $03fff826 ' xor, 1 asm-test5
$38210001 $38010000 $38200000 $38000001 $3bffffff ' xori, 1 asm-test5

$00200821 $00000821 $00200021 $03e0f821 ' move, 1 asm-test4
$00010823 $00200821 $04210002
$00000823 $00000821 $04010002
$00010023 $00200021 $04210002
$001ff823 $03e0f821 $07e10002 ' abs, 3 asm-test4
$00010823 $00000823 $00010023 $001ff823 ' neg, 1 asm-test4
$00010823 $00000823 $00010023 $001ff823 ' negu, 1 asm-test4
$00200827 $00000827 $00200027 $03e0f827 ' not, 1 asm-test4
$14200001 $0021082a $14200000 $0020082a $14200000
$0001082a $14200001 $0000082a $1420ffff $03ff082a ' blt, 2 asm-test5i
$10200001 $0021082a $10200000 $0001082a $10200000
$0020082a $10200001 $0000082a $1020ffff $03ff082a ' ble, 2 asm-test5i
$14200001 $0021082a $14200000 $0001082a $14200000
$0020082a $14200001 $0000082a $1420ffff $03ff082a ' bgt, 2 asm-test5i
$10200001 $0021082b $10200000 $0020082b $10200000
$0001082b $10200001 $0000082b $1020ffff $03ff082b ' bgeu, 2 asm-test5i
$14200001 $0021082b $14200000 $0020082b $14200000
$0001082b $14200001 $0000082b $1420ffff $03ff082b ' bltu, 2 asm-test5i
$10200001 $0021082b $10200000 $0001082b $10200000
$0020082b $10200001 $0000082b $1020ffff $03ff082b ' bleu, 2 asm-test5i
$14200001 $0021082b $14200000 $0001082b $14200000
$0020082b $14200001 $0000082b $1420ffff $03ff082b ' bgtu, 2 asm-test5i
$10200001 $0021082b $10200000 $0020082b $10200000
$0001082b $10200001 $0000082b $1020ffff $03ff082b ' bgeu, 2 asm-test5i
