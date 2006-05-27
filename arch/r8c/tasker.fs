\ Multitasker

rom

Variable bgtask ram $20 cells allot rom
:noname  bgtask @ 0= ?EXIT
    rp@ bgtask @ sp@ cell+ bgtask ! sp! rp! ;
IS pause
: task r> bgtask $20 cells + !
  bgtask $20 cells + bgtask $10 cells + !
  bgtask $10 cells + bgtask ! ;
:noname echo @ IF
     BEGIN pause key? UNTIL THEN (key) ;
is key

ram