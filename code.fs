\ ASSEMBLER, CODE etc.

\ does not include the actual assembler (which is machine-dependent),
\ only words like CODE that are implementation-dependent, but can be
\ defined for all machines.

vocabulary assembler ( -- ) \ tools-ext

: init-asm ( -- )
    also assembler ;

: code ( -- colon-sys )	\ tools-ext
    \ start a native code definition
    header here >body cfa, defstart init-asm ;

: (;code) ( -- ) \ gforth
    \ execution semantics of @code{;code}
    r> lastxt code-address! ;

: ;code ( colon-sys1 -- colon-sys2 )	\ tools-ext	semicolon-code
    ( create the [;code] part of a low level defining word )
    state @
    IF
	;-hook postpone (;code) ?struc postpone [
    ELSE
	align here lastxt code-address!
    THEN
    defstart init-asm ; immediate

: end-code ( colon-sys -- )	\ gforth	end_code
    ( end a code definition )
    lastxt here over - flush-icache
    previous ?struc reveal ;
