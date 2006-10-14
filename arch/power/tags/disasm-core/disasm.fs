\ convention for disasm words, please refer to ibm man pages 655ff
\ since for example BO refers to bits 6 till 10 of instruction bcx und D refers
\ to the same bits, i won't define two words disasm-bo and disasm-d but only one
\ disasm-6,10 if only one number is given it refers to a single bit
: disasm-0,5 ( w -- u )
  26 rshift ;

: disasm-31 ( w -- u )
  $1 and ;

: disasm-30 ( w -- u )
  1 rshift $1 and ;

\ I-FORM
: disasm-6,29 ( w -- u )
  2 rshift $FFFFFF and ;

: disasm-6,10 ( w -- u )
  21 rshift $1F and ;

: disasm-11,15 ( w -- u )
  16 rshift $1F and ;

: disasm-16,29 ( w -- u )
  2 rshift $3FFF and ;

: disasm-16,31 ( w -- u )
  $FFFF and ;

: disasm-6,8 ( w -- u )
  23 rshift $7 and ;

: disasm-10 ( w -- u )
  21 rshift $1 and ;

: disasm-30,31 ( w -- u )
  $3 and ;

: disasm-16,20 ( w -- u )
  11 rshift $1F and ;

: disasm-21,30 ( w -- u )
  1 rshift $3FF and ;

: disasm-11,13 ( w -- u )
  18 rshift $7 and ;

: disasm-12,15 ( w -- u )
  16 rshift $F and ;

: disasm-16,19 ( w -- u )
  12 rshift $F and ;

: disasm-11,20 ( w -- u )
  11 rshift $3FF and ;

: disasm-12,19 ( w -- u )
  12 rshift $FF and ;

: disasm-7,14 ( w -- u )
  17 rshift $FF and ;

\ XS form
: disasm-21,29 ( w -- u )
  2 rshift $1FF and ;

\ XO form
: disasm-21 ( w -- u )
  10 rshift $1 and ;

: disasm-22,30 ( w -- u )
  1 rshift $1ff and ;

\ A form
: disasm-26,30 ( w -- u )
  1 rshift $1F and ;

: disasm-21,25 ( w -- u )
  6 rshift $1F and ;

\ M form

\ MD form
: disasm-21,26 ( w -- u )
  5 rshift $3F and ;

: disasm-27,29 ( w -- u )
  2 rshift $7 and ;

: disasm-27,30 ( w -- u )
  1 rshift $F and ;

: disasm-xo-flags ( w -- u )
  dup disasm-21 1 lshift swap disasm-31 or ;

: illegal-flags ( addr w -- )
  ."  illegal opcode" drop ;
  \ XXX maybe an empty string here ?!

: flags-table ( n "name" -- )
  create 0 ?do
    ['] illegal-flags ,
  loop
does> ( u -- addr )
  swap cells + ;

$20 flags-table flags-tab-entry
$4  flags-table xo-flags-tab-entry

: def-flags-format ( table-xt )
  create ,
does> ( u "inst" )
  @ swap here name string,
  noname create , swap
  execute lastxt swap !
does> ( u )
  @ count type ;

' xo-flags-tab-entry def-flags-format asm-xo-flags

: disasm-xo-ext ( w -- )
  disasm-xo-flags xo-flags-tab-entry @ execute ;

' disasm-xo-ext 31 flags-tab-entry !

: disasm-illegal ( addr w -- )
  hex. ." , ( illegal inst ) " drop ;

: disasm-table ( n "name" -- )
  create 0 ?do
    ['] disasm-illegal ,
  loop
does> ( u -- addr )
  swap cells + ;
  
\ XXX resize
$40 disasm-table opc-tab-entry
$1EB disasm-table xo-tab-entry

: disasm-inst ( addr w -- )
  dup disasm-0,5 opc-tab-entry @ execute ;

: disasm ( addr u -- )
  bounds u+do
    cr ." ( " i hex. ." ) " i i @ disasm-inst
    1 cells +loop
  cr ;

: disasm-xo ( addr w -- )
   dup disasm-22,30 xo-tab-entry @ execute ;
' disasm-xo 31 opc-tab-entry !

: disasm-6,10-11,15-16,20 ( addr w -- )
  dup disasm-6,10 .
  dup disasm-11,15 .
  disasm-16,20 .
  drop ;

: define-format ( disasm-xt table-xt -- )
  create 2,
does> ( u "inst" -- )
  2@ swap here name string, 
  noname create 2,
  execute lastxt swap !
does> ( addr w -- )
  over { w }
  2@ >r
  execute
  r> count type
  w dup disasm-0,5 flags-tab-entry @ execute ;

' disasm-6,10-11,15-16,20 ' xo-tab-entry define-format asm-xo

include ./inst.fs


\ XXX comp of rel on ppc64
: comp-reladdr ( addr w -- addr )
  \ check sign extension
    disasm-6,29 2 lshift $FC000000 or + ;
