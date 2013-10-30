\ This implements the X:locals syntax in pure ANS Forth with X:parse-name
\ It does not implement the increased number of locals in that extension

\ This file is in the public domain. NO WARRANTY.

\ This implementation technique has been described by John Hayes in
\ the SigForth Newsletter 4(2), Fall '92. He did not do the complete
\ job, but left some more mundane parts as an exercise to the reader.

\ Here we use PARSE-NAME, so we don't need to play around with >IN,
\ resulting in significant simplification.

: parse-name1 ( "name" -- c-addr u )
    parse-name dup 0= -16 and throw ;

: parse-rest ( "name" ... "namen" -- )
    begin
        parse-name1
    s" :}" compare 0= until ;

: {helper ( f "name1"..."namen" -- )
    parse-name1
    2dup s" |" compare 0= if
        2drop drop true parse-name1 then
    2dup s" --" compare 0= if
	2drop drop parse-rest exit then
    2dup s" :}" compare 0= if
        2drop drop exit then
    rot dup if \ we are in the | part
        0 postpone literal then
    recurse (local) ;

: {: ( "X:locals definition" -- )
    false {helper 0 0 (local) ; immediate

0 [if]
: test-swap {: a b -- b a :} ." xxx"
    b a ;

1 2 test-swap . . .s cr

: test| {: a b | c d -- e f :}
    a . b . c . d . ;

3 4 test| .s cr
[then]
