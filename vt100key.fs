\ MS-DOS key interpreter                               17oct94py

Create translate $100 allot
translate $100 erase

: trans:  char translate + c! ;

: vt100-decode ( max span addr pos1 -- max span addr pos2 flag )
  key '[ = IF    key translate + c@ dup IF  decode  THEN
           ELSE  0  THEN ;

ctrl B trans: D
ctrl F trans: C
ctrl P trans: A
ctrl N trans: B

' vt100-decode  ctrlkeys $1B cells + !
