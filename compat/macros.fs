\ implementation of ]] ... [[

\ This file is in the public domain. NO WARRANTY.

\ Avoid having to write so many POSTPONEs; Instead of

\ POSTPONE a POSTPONE b POSTPONE c
\ write
\ ]] a b c [[

\ In addition there are some shortcuts for literals (not present in
\ Gforth 0.7.0 and earlier):

\ 1  ]]L               is equivalent to    1  ]]  literal
\ 1. ]]2L              is equivalent to    1. ]] 2literal
\ 1e ]]FL              is equivalent to    1e ]] fliteral
\ parse-name foo ]]SL  is equivalent to    parse-name foo ]] sliteral

\ This program uses the following words
\ from CORE :
\  environment? drop : BEGIN >in @ dup 0= WHILE 2drop and REPEAT ; ! 
\  POSTPONE immediate Literal 
\ from BLOCK-EXT :
\  \ 
\ from DOUBLE :
\  2Literal 
\ from EXCEPTION :
\  throw 
\ from FILE :
\  S" ( 
\ from FILE-EXT :
\  refill 
\ from FLOAT :
\  FLiteral 
\ from STRING :
\  compare SLiteral 
\ from X:parse-name :
\  parse-name 

s" X:parse-name" environment? drop \ just let the system know that we need it

: refilling-parse-name ( -- old->in c-addr u )
    begin
	>in @ parse-name dup 0= while
	    2drop drop refill 0= -39 and throw
    repeat ;

: ]] ( -- )
    \ switch into postpone state
    begin
	refilling-parse-name s" [[" compare while
	    >in ! POSTPONE postpone
    repeat
    drop ; immediate

: postpone-literal  postpone  literal ;
: postpone-2literal postpone 2literal ;
: postpone-fliteral postpone fliteral ;
: postpone-sliteral postpone sliteral ;

: ]]L ( postponing: x -- ; compiling: -- x )
    \ Shortcut for @code{]] literal}.
    ]] postpone-literal ]] [[ ; immediate

: ]]2L ( postponing: x1 x2 -- ; compiling: -- x1 x2 )
    \ Shortcut for @code{]] 2literal}.
    ]] postpone-2literal ]] [[ ; immediate

: ]]FL ( postponing: r -- ; compiling: -- r )
    \ Shortcut for @code{]] fliteral}.
    ]] postpone-fliteral ]] [[ ; immediate

: ]]SL ( postponing: addr1 u -- ; compiling: -- addr2 u )
    \ Shortcut for @code{]] sliteral}; if the string already has been
    \ allocated permanently, you can use @code{]]2L} instead.
    ]] postpone-sliteral ]] [[ ; immediate
