\ implementation of EXECUTE-PARSING

\ This file is in the public domain. NO WARRANTY.

\ execute-parsing   ( ... c-addr u xt - ... )
\ 
\ Make c-addr u the current input source, execute xt ( ... -- ... ),
\ then restore the previous input source.
\ 
\ This word is used like this:
\ 
\ s" test" ' create execute-parsing
\ 
\ and this would be equivalent to 
\ 
\ create test
\ 
\ It can be used to provide the input-stream input of a parsing word
\ without consuming the input stream of the calling word.

\ this implementation copies the string to be parsed elsewhere (while
\ EVALUATE is required to work in-place)

\ The program uses the following words
\ from CORE :
\  Constant : execute source >in ! drop ; >r 1+ r> swap dup chars + r@ move 
\  rot ['] 
\ from BLOCK :
\  evaluate 
\ from BLOCK-EXT :
\  \ 
\ from EXCEPTION :
\  throw catch 
\ from FILE :
\  ( S" 
\ from MEMORY :
\  allocate free 
\ from SEARCH :
\  wordlist get-current set-current get-order set-order 
\ from SEARCH-EXT :
\  previous 

wordlist constant execute-parsing-wordlist

get-current execute-parsing-wordlist set-current

\ X is prepended to the string, then the string is EVALUATEd
: X ( xt -- )
    previous execute
    source >in ! drop ; immediate \ skip remaining input

set-current

: >order ( wid -- )
  >r get-order 1+ r> swap set-order ;

: execute-parsing ( ... c-addr u xt -- ... )
    >r dup >r
    dup 2 chars + allocate throw >r  \ construct the string to be EVALUATEd
    s" X " r@ swap chars move
    r@ 2 chars + swap chars move
    r> r> 2 + r> rot dup >r rot ( xt c-addr1 u1 r: c-addr1 )
    execute-parsing-wordlist >order  \ make sure the right X is executed
    ['] evaluate catch               \ now EVALUATE the string
    r> free throw throw ;            \ cleanup
