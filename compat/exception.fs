\ exception

\ This file is in the public domain. NO WARRANTY.

\ this implementation tries to be as close to the following proposal
\ as possible (we cannot guarantee unusedness nor are we allowed to
\ use values in the range -4095..-256).

\ EXCEPTION ( c-addr u -- n ) exception

\ n is a previously unused THROW value in the range
\ {-4095...-256}. Consecutive calls to EXCEPTION return consecutive
\ decreasing numbers.

\ The system may use the string denoted by c-addr u when reporting
\ that exception (if it is not caught).

\ Typical Use

\ s" Out of GC-managed memory" EXCEPTION CONSTANT gc-out-of-memory
\ ...
\ ... gc-out-of-memory THROW ...

\ The program uses the following words
\ from CORE :
\ Variable ! : 2drop @ +! ; 
\ from BLOCK-EXT :
\ \ 
\ from FILE :
\ ( 

variable next-exception -10753 next-exception !

: exception ( c-addr u -- n )
    2drop
    next-exception @
    -1 next-exception +! ;
