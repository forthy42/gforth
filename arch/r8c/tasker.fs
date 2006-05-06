\ Multitasker

rom

Variable bgtask $40 cells allot
: pause  bgtask @ 0= ?EXIT
  rp@ bgtask @ sp@ cell+ bgtask ! sp! rp! ;
: task r> bgtask $40 cells + !
  bgtask $40 cells + bgtask $20 cells + !
  bgtask $20 cells + bgtask ! ;
: pkey echo @ IF
     BEGIN pause key? UNTIL THEN (key) ;
' pkey is key

ram