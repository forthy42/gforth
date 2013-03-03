\ anonymous definitions in a definition

: [: ( -- quotation-sys )
\G Starts a quotation
    false :noname ;
comp: drop  locals-wordlist last @ lastcfa @ leave-sp @
    postpone AHEAD
    locals-list @ locals-list off
    postpone SCOPE
    true  :noname  ;

: ;] ( compile-time: quotation-sys -- ; run-time: -- xt )
    \g ends a quotation
    POSTPONE ; >r IF
	]  postpone ENDSCOPE
	locals-list !
	postpone THEN
	leave-sp ! lastcfa ! last ! to locals-wordlist
	r> postpone ALiteral
    ELSE  r>  THEN ( xt ) ; immediate

0 [IF] \ tests
: if-else ( ... f xt1 xt2 -- ... )
\ Postscript-style if-else
    rot IF
       drop
    ELSE
       nip
    THEN
    execute ;

: test ( f -- )
    [: ." true" ;]
    [: ." false" ;]
    if-else ;
   
1 test cr \ writes "true"
0 test cr \ writes "false"

\ locals within quotations

: foo { a b } a b
    [: { x y } x y + ;] execute . a . b . ;
2 3 foo
[THEN]