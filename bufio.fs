\ BUFOUT.STR    Buffered output for Debug               13jun93jaw

CREATE O-Buffer 4000 chars allot align
VARIABLE O-PNT

: O-TYPE        O-PNT @ over chars O-PNT +!
                swap move ;

: O-EMIT        O-PNT @ c! 1 chars O-PNT +! ;

VARIABLE EmitXT
VARIABLE TypeXT

: O-INIT        What's type TypeXT !
                What's emit EmitXT !
                O-Buffer O-PNT !
                ['] o-type IS type
                ['] o-emit IS emit ;

: O-DEINIT      EmitXT @ IS Emit
                TypeXT @ IS Type ;

: O-PNT@        O-PNT @ O-Buffer - ;

