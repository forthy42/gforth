\ ERRORE.FS English error strings                      9may93jaw

\ The errors are defined by a linked list, for easy adding
\ and deleting. Speed is not neccassary at this point.

AVARIABLE ErrLink              \ Linked list entry point
0 ErrLink !

: ERR" ( n -- )
       ErrLink linked
       ,
       [char] " word count
       string, align ;

decimal

-1 ERR" Aborted"
ErrLink @ unlock reloff lock \ make sure that the terminating 0 is not relocated
-3 ERR" Stack overflow"                 -4 ERR" Stack underflow"
-5 ERR" Return stack overflow"          -6 ERR" Return stack undeflow"
-7 ERR" Do-loops nested too deeply"     -8 ERR" Dictionary overflow"
-9 ERR" Invalid memory address"         -10 ERR" Division by zero"
-11 ERR" Result out of range"           -12 ERR" Argument type mismatch"
-13 ERR" Undefined word"                -14 ERR" Interpreting a compile-only word"
-15 ERR" Invalid FORGET"                -16 ERR" Attempt to use zero-length string as a name"
-17 ERR" Pictured numeric ouput string overflow"
-18 ERR" Parsed string overflow"        -19 ERR" Word name too long"
-20 ERR" Write to a read-only location" -21 ERR" Unsupported operation"
-22 ERR" Control structure mismatch"    -23 ERR" Address alignment exception"
-24 ERR" Invalid numeric argument"      -25 ERR" Return stack imbalance"
-26 ERR" Loop parameters unavailable"   -27 ERR" Invalid recursion"
-28 ERR" User interupt"                 -29 ERR" Compiler nesting"
-30 ERR" Obsolescent feature"           -31 ERR" >BODY used on non-CREATEd definition"
-32 ERR" Invalid name argument"         -33 ERR" Block read exception"
-34 ERR" Block write exception"         -35 ERR" Invalid block number"
-36 ERR" Invalid file position"         -37 ERR" File I/O exception"
-38 ERR" Non-existent file"             -39 ERR" Unexpected end of file"
-40 ERR" Invalid BASE for floating point conversion"
-41 ERR" Loss of precision"             -42 ERR" Floating-point divide by zero"
-43 ERR" Floating-point result out of range"
-44 ERR" Floating-point stack overflow" -45 ERR" Floating-point stack underflow"
-46 ERR" Floating-point invalid argument"
-47 ERR" Compilation word list deleted" -48 ERR" invalid POSTPONE"
-49 ERR" Search-order overflow"         -50 ERR" Search-order underflow"
-51 ERR" Compilation word list changed" -52 ERR" Control-flow stack overflow"
-53 ERR" Exception stack overflow"      -54 ERR" Floating-point underflow"
-55 ERR" Floating-point unidentified fault"
-56 ERR" QUIT"                          -57 ERR" Error in sending or receiving a character"
-58 ERR" [IF], [ELSE], [THEN] error"
\ signals: ( We list them all, execpt those already present, just in case )
-256 ERR" Hangup signal"
-257 ERR" Quit signal"
-258 ERR" Illegal Instruction"
-259 ERR" Trace Trap"
-260 ERR" IOT instruction"
-261 ERR" EMT instruction" \ abort() call?
-262 ERR" Kill signal" \ cannot be caught but so what
-263 ERR" Bad arg to system call"
-264 ERR" Broken pipe"
-265 ERR" Alarm signal"
-266 ERR" Terminate signal"
-267 ERR" User signal 1"
-268 ERR" User signal 2"
\ error numbers between -512 and -2047 are for OS errors and are
\ handled with strerror


: .error ( n -- )
    cr ." Error: "
    ErrLink
    BEGIN @ dup
    WHILE
	2dup cell+ @ =
	IF 2 cells + count type drop exit THEN
    REPEAT
    drop
    dup -2047 -511 within
    IF
	512 + negate strerror type exit
    THEN
    . ;

