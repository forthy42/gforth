\ MS-DOS key interpreter                               17oct94py

Create translate $100 allot
translate $100 erase

: trans:  char translate + c! ;

: dos-decode ( max span addr pos1 -- max span addr pos2 flag )
  key translate + c@ dup IF  decode  THEN ;

ctrl B trans: K
ctrl F trans: M
ctrl P trans: H
ctrl N trans: P
ctrl A trans: G
ctrl E trans: O

' dos-decode  ctrlkeys !
