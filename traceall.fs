\ replacement for name

: trace-name .s ." | " source >in @ /string type cr (name) ;

: traceall  ['] trace-name IS name ;
: notrace   [ what's name ] Literal IS name ;