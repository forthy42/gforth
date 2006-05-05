\ Multitasker

rom

Variable bgtask $20 cells allot
: pause  bgtask @ 0= ?EXIT
  rp@ bgtask @ sp@ cell+ bgtask ! sp! rp! ;
: task r> bgtask $20 cells + !
  bgtask $20 cells + bgtask $10 cells + !
  bgtask $10 cells + bgtask ! ;
: pkey BEGIN pause key? UNTIL (key) ;
' pkey is key

ram