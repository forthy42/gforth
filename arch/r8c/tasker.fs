\ Multitasker

rom

Variable bgtask ram $20 cells allot rom
: pause  bgtask @ 0= ?EXIT
  rp@ bgtask @ sp@ cell+ bgtask ! sp! rp! ;
: task r> bgtask $20 cells + !
  bgtask $20 cells + bgtask $10 cells + !
  bgtask $10 cells + bgtask ! ;
: pkey echo @ IF
     BEGIN pause key? UNTIL THEN (key) ;
: ms ( n -- )  timer @ +
    BEGIN  pause dup timer @ - 0<  UNTIL  drop ;
' pkey is key

ram