\ proof of concept for loops with the target address on the return stack

: DO ( compilation -- do-sys ; run-time w1 w2 -- loop-sys ) \ core
    \G @xref{Counted Loops}.
    POSTPONE (do)-rstack
    POSTPONE begin drop do-dest ; immediate restrict

: ?DO ( compilation -- do-sys ; run-time w1 w2 -- | loop-sys )  \ core-ext     question-do
    \G @xref{Counted Loops}.
    POSTPONE (?do)-rstack ?do-like ; immediate restrict

: error! ( -- )
    assert( 0 ) ;
' error! set-optimizer

: until-like ( stack-state list addr xt1 xt2 -- )
    drop compile, drop drop pop-stack-state ;

: loop-like ( do-sys xt1 xt2 -- )
    >r >r 0 cs-pick swap cell- swap 1 cs-roll r> r> rot do-dest?
    until-like  POSTPONE done  POSTPONE unloop postpone rdrop ;

: LOOP ( compilation do-sys -- ; run-time loop-sys1 -- | loop-sys2 )    \ core
    \G @xref{Counted Loops}.
 ['] (loop)-rstack ['] error! loop-like ; immediate restrict

: +LOOP ( compilation do-sys -- ; run-time loop-sys1 n -- | loop-sys2 ) \ core plus-loop
    \G @xref{Counted Loops}.
 ['] (+loop)-rstack ['] error! loop-like ; immediate restrict
