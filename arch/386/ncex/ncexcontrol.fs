\ Native code extensions of control flow.

: tc-ahead POSTPONE branch >mark POSTPONE unreachable ; 
: tc-if POSTPONE ?branch >mark ;
: tc-else POSTPONE ahead  1 cs-roll  POSTPONE then ;
: tc-then  dup orig?  then-like ;

: nc-ahead (opt-flush) regalloc-flush tc-ahead ;
: nc-if (opt-flush) regalloc-flush tc-if ;
: nc-else (opt-flush) regalloc-flush tc-else ;
: nc-then (opt-flush) regalloc-flush tc-then ; 
