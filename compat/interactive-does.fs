wordlist constant oneshot-me
get-current
oneshot-me set-current
: ; postpone ; previous execute ; immediate
set-current
: >order  ( wid -- ) >r get-order r> swap 1+ set-order ;
: oneshot: :noname oneshot-me >order ;

: DOES> state @ IF
     postpone DOES>
  ELSE
    oneshot: postpone DOES>
  THEN ; immediate
