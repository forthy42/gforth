\ VT100.STR     VT100 excape sequences                  20may93jaw

decimal

: pn    base @ swap decimal 0 u.r base ! ;
: ;pn   [char] ; emit pn ;
: ESC[  27 emit [char] [ emit ;
: at-xy 1+ swap 1+ swap ESC[ pn ;pn [char] H emit ;
: page  ESC[ ." 2J" 0 0 at-xy ;

