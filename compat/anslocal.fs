\ This implements a subset of the gforth locals syntax in pure ANS Forth

\ This file is in the public domain. NO WARRANTY.

\ This implementation technique has been described by John Hayes in
\ the SigForth Newsletter 4(2), Fall '92. He did not do the complete
\ job, but left some more mundane parts as an exercise to the reader.

\ I don't implement the "|" part, because 1) gforth does not implement
\ it and 2) it's unnecessary; just put a 0 before the "{" for every
\ additional local you want to declare.

\ The program uses the following words
\ from CORE :
\ : bl word count ; >in @ 2dup 0= IF 2drop [char] ELSE THEN drop
\ recurse swap ! immediate
\ from CORE-EXT :
\ parse true 
\ from BLOCK-EXT :
\ \ 
\ from FILE :
\ ( S" 
\ from LOCAL :
\ (local) 
\ from STRING :
\ compare 

: local ( "name" -- )
    bl word count (local) ;

: {helper ( -- final-offset )
    >in @
    bl word count
    2dup s" --" compare 0= if
	2drop [char] } parse 2drop true
    else
	s" }" compare 0=
    then
    if
	drop >in @
    else
	recurse
	swap >in ! local
    then ;

: { ( -- )
    {helper >in ! 0 0 (local) ; immediate

\ : test-swap { a b -- b a } ." xxx"
\     b a ;

\ 1 2 test-swap . . .s cr
