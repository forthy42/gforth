\ vocabulary

\ This file is in the public domain. NO WARRANTY.

\ The program uses the following words
\ from CORE :
\ : Create , DOES> @ >r dup 0= and r> swap ; 
\ from CORE-EXT :
\ nip 
\ from BLOCK-EXT :
\ \ 
\ from EXCEPTION :
\ throw 
\ from FILE :
\ ( 
\ from SEARCH :
\ wordlist get-order set-order 

: vocabulary ( -- )
    wordlist create ,
does> ( -- )
    \ replaces the wordlist on the top of the search list with the
    \ vocabulary's wordlist
    @ >r
    get-order dup 0= -50 and throw \ search-order underflow
    nip r> swap
    set-order ;

