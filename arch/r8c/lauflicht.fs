\ lauflicht

rom
: licht!  led!  &100 0 DO  pause 1 ms  LOOP ;
: lauf  1 licht! 2 licht! 4 licht! 8 licht!
  4 licht! 2 licht! ;
: dauerlauf
  task  BEGIN  lauf  AGAIN ;

ram
