\ scripting extensions

: sh-eval ( addr u -- )
    \G evaluate string + rest of command line
    2dup 2>r >in @ >r negate
    source >in @ 1- /string + c@ bl <> + >in +! drop sh
    $? IF  r> >in ! 2r> defers interpreter-notfound
    ELSE  rdrop 2rdrop  THEN ;
' sh-eval IS interpreter-notfound

2Variable sh$  0. sh$ 2!
: sh-get ( addr u -- addr' u' )
    \G open command addr u, and read in the result
    sh$ free-mem-var
    r/o open-pipe throw dup >r slurp-fid
    r> close-pipe throw to $? 2dup sh$ 2! ;

:noname '` parse sh-get ;
:noname '` parse postpone SLiteral postpone sh-get ;
interpret/compile: s`