\ Simple debugging aids

\ They are meant to support a different style of debugging than the
\ tracing/stepping debuggers used in languages with long turn-around
\ times.

\ IMO, a much better (faster) way in fast-compilig languages is to add
\ printing code at well-selected places, let the program run, look at
\ the output, see where things went wrong, add more printing code, etc.,
\ until the bug is found.

\ We support fast insertion and removal of the printing code.

\ !!Warning: the default debugging actions will destroy the contents of pad


defer printdebugdata ( -- )
' .s IS printdebugdata
defer printdebugline ( addr -- )

: (printdebugline) ( addr -- )
    cr
    dup 2@ type ." :" cell+ cell+
    @ 0 .r ." :"
    \ it would be nice to print the name of the following word,
    \ but that's not easily possible for primitives
    printdebugdata
    cr ;

' (printdebugline) IS printdebugline

: (~~) ( -- )
    r@ printdebugline
    r> 3 cells + >r ;

: ~~ ( -- )
    POSTPONE (~~) loadfilename 2@ 2, loadline @ , ; immediate

