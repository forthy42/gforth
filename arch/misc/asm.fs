\ PIE MISC assembler

Vocabulary assembler
also assembler also definitions forth

\ sources

$0 Constant PC		$1 Constant PC+2
$2 Constant PC+4	$3 Constant PC+6

$8 Constant ACCU	$9 Constant SF
$A Constant ZF		$C Constant CF

\ destinations

$0 Constant JMP		$1 Constant JS
$2 Constant JZ		$4 Constant JC

$7 Constant *ACCU
( $8 Constant ACCU )	$9 Constant SUB
( $A Constant SUBR )	$B Constant ADD
$C Constant XOR		$D Constant OR
$E Constant AND		$F Constant SHR

$FFFC Constant tx
\ $FFF0 Constant tx

: end-label previous ;

Create marks $10 cells allot

: ahere s" here" evaluate 2/ ;

: m ( n -- ) cells marks + ahere 2* swap ! 0 ;
: r ( n -- ) cells marks + @ ahere swap s" !" evaluate 0 ;

\ intel hex dump

: 0.r ( n1 n2 -- ) 0 swap <# 0 ?DO # LOOP #> type ;

: tohex ( dest addr u -- )  base @ >r hex
  ." :" swap >r >r
  r@ dup 2 0.r  over 4 0.r  ." 00"
  over 8 rshift + +
  r> r> swap bounds ?DO  I ( 1 xor ) c@ dup 2 0.r +  LOOP
  negate $FF and 2 0.r  r> base ! ;

: 2hex ( dest addr u -- )
  BEGIN  dup WHILE
         >r 2dup r@ $10 min tohex cr
         r> $10 /string 0 max rot $10 + -rot
  REPEAT  drop 2drop ;

: sym base @ >r hex
    cr ." sym:s/PC=" ahere 4 0.r ." /" bl word count type ." /g" cr
    r> base ! ;

: label ahere Constant ;

also forth definitions

: label also assembler label ;

: (code) also assembler ;
: (end-code) previous ;

previous previous previous
