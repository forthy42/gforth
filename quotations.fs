\ anonymous definitions in a definition

:noname  false :noname ;
:noname  locals-wordlist last @ lastcfa @
    postpone SCOPE postpone AHEAD  true  :noname ;
interpret/compile: [:

: ;] ( compile-time: orig colon-sys -- ; run-time: -- xt )
    POSTPONE ; >r IF
	]  postpone THEN  r> postpone ALiteral  postpone ENDSCOPE
	lastcfa ! last ! to locals-wordlist
    ELSE  r>  THEN ( xt ) ; immediate

\\\
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
