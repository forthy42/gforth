\ LOOK.FS      xt -> lfa                               22may93jaw

\ Look checks first if the word is a primitive. If yes then the
\ vocabulary in the primitive area is beeing searched, meaning
\ creating for each word a xt and comparing it...

\ If a word is no primitive look searches backwards to find the nfa.
\ Problems: A compiled xt via compile, might be created with noname:
\           a noname: leaves now a empty name field

decimal

\ >NAME PRIMSTART                                       22may93jaw

\ : >name ( xt -- nfa )
\         BEGIN   1 chars -
\                 dup c@ 128 and
\         UNTIL ;

: PrimStart ['] true >name ;

\ look                                                  17may93jaw

: (look)  ( xt startlfa -- lfa flag )
        false swap
        BEGIN @ dup
        WHILE dup cell+ name>
              3 pick = IF nip dup THEN
        REPEAT
        drop nip
        dup 0<> ;

: look ( cfa -- lfa flag )
        dup forthstart u<
        IF PrimStart (look)
        ELSE >name true THEN ;

