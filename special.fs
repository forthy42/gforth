\ state-smart words last, because they override cross' words.

create s"-buffer /line chars allot
:noname    [char] " parse
    /line min >r s"-buffer r@ cmove
    s"-buffer r> ;
:noname    [char] " parse postpone SLiteral ;
special: S" ( compilation 'ccc"' -- ; run-time -- c-addr u )	\ core,file	s-quote

:noname    ' >body ! ;
:noname    ' >body postpone ALiteral postpone ! ;
special: IS ( addr "name" -- ) \ gforth

' IS Alias TO ( addr "name" -- ) \ core-ext
immediate

:noname    ' >body @ ;
:noname    ' >body postpone ALiteral postpone @ ;
special: What's ( "name" -- addr ) \ gforth

:noname    [char] " parse type ;
:noname    postpone (.") ,"  align ;
special: ." ( compilation 'ccc"' -- ; run-time -- )  \ core	dot-quote

\ DOES>                                                17mar93py

:noname    align dodoes, here !does ]
    defstart :-hook ;
:noname    ;-hook postpone (does>) ?struc dodoes,
    defstart :-hook ;
special: DOES>  ( compilation colon-sys1 -- colon-sys2 ; run-time nest-sys -- ) \ core	does
