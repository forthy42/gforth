\ control-structure add-ons (ENDIF, ?DUP-IF etc.)

\ This file is in the public domain. NO WARRANTY.

\ Hmm, this would be a good application for ]] ... [[

\ The program uses the following words
\ from CORE :
\ : POSTPONE THEN ; immediate ?dup IF 0= 
\ from BLOCK-EXT :
\ \ 
\ from FILE :
\ ( 

: ENDIF ( compilation orig -- ; run-time -- ) \ gforth
    POSTPONE then ; immediate

: ?DUP-IF ( compilation -- orig ; run-time n -- n| ) \ gforth	question-dupe-if
    POSTPONE ?dup POSTPONE if ; immediate

: ?DUP-0=-IF ( compilation -- orig ; run-time n -- n| ) \ gforth	question-dupe-zero-equals-if
    POSTPONE ?dup POSTPONE 0= POSTPONE if ; immediate
