\ required

\ This file is in the public domain. NO WARRANTY.

\ s" filename" required
\ includes the file if no file name "filename" has been included before
\ warning: does not deal correctly with accesses to the same file through
\ different path names; but since ANS Forth does not specify path handling...

\ The program uses the following words
\ from CORE :
\ 0= : swap >r dup 2dup r> rot move ; cells r@ @ over ! cell+ 2! BEGIN
\ WHILE 2@ IF drop 2drop EXIT THEN REPEAT ELSE Variable
\ from CORE-EXT :
\ 2>r 2r@ 2r> true 
\ from BLOCK-EXT :
\ \ 
\ from EXCEPTION :
\ throw 
\ from FILE :
\ S" ( included 
\ from MEMORY :
\ allocate 
\ from SEARCH :
\ forth-wordlist search-wordlist 
\ from STRING :
\ compare 
\ from TOOLS-EXT :
\ [IF] [THEN] 

s" required" forth-wordlist search-wordlist [if]
    drop
[else]

\ we use a linked list of names

: save-mem	( addr1 u -- addr2 u ) \ gforth
    \ copy a memory block into a newly allocated region in the heap
    swap >r
    dup allocate throw
    swap 2dup r> rot rot move ;

: name-add ( addr u listp -- )
    >r save-mem ( addr1 u )
    3 cells allocate throw \ allocate list node
    r@ @ over ! \ set next pointer
    dup r> ! \ store current node in list var
    cell+ 2! ;
    
: name-present? ( addr u list -- f )
    rot rot 2>r begin ( list R: addr u )
	dup
    while
	dup cell+ 2@ 2r@ compare 0= if
	    drop 2r> 2drop true EXIT
	then
	@
    repeat
    ( drop 0 ) 2r> 2drop ;

: name-join ( addr u list -- )
    >r 2dup r@ @ name-present? if
	r> drop 2drop
    else
	r> name-add
    then ;

variable included-names 0 included-names !

: included ( i*x addr u -- j*x )
    2dup included-names name-join
    included ;

: required ( i*x addr u -- j*x )
    2dup included-names @ name-present? 0= if
	included
    else
	2drop
    then ;

[then]
