\ TOOLSEXT.FS [IF] [ELSE] [THEN] and more              20may93jaw

\ This here is fully ans compatible
\ May be cross-compiled

\ ( \ added 09jun93jaw

\ very close to dpANS5

decimal

CREATE Opennest  7 chars allot
CREATE Closenest 7 chars allot

: SKIPNEST
      1 BEGIN
          BEGIN name count dup WHILE
                2dup Opennest count compare 0=
                IF   2drop 1+
                ELSE    Closenest count compare 0= IF 1- THEN
                THEN
                ?dup 0= ?EXIT
          REPEAT
          2drop refill 0=
        UNTIL drop ;

\ : (     s" (" Opennest place
\         s" )" Closenest place
\         SKIPNEST ; immediate

: comment? ( c-addr u -- c-addr u )
        2dup s" (" compare 0=
        IF    postpone (
        ELSE  2dup s" \" compare 0= IF postpone \ THEN
        THEN ;

: [ELSE]
      1 BEGIN
          BEGIN name count dup WHILE
                comment?
                2dup s" [IF]" compare 0=
                IF   2drop 1+
                ELSE 2dup s" [ELSE]" compare 0=
                     IF   2drop 1- dup IF 1+ THEN
                     ELSE s" [THEN]" compare 0= IF 1- THEN
                     THEN
                THEN
                ?dup 0= ?EXIT
          REPEAT
          2drop refill 0=
        UNTIL drop ; immediate

: [THEN] ( -- ) ;    immediate

: [IF] ( flag -- )
        0= IF postpone [ELSE] THEN ; immediate

\ [IFUNDEF] [IFDEF]                                     9may93jaw

: [IFUNDEF]
        name find nip 0= postpone [IF] ; immediate
: [IFDEF]
        name find nip 0<> postpone [IF] ; immediate


\ [IF]?                                                 9jun93jaw

\ same as comment? but skips [IF] .... [THEN]

: [if]?   ( c-addr u -- c-addr u )
        2dup s" [IF]" compare 0= >r
        2dup s" [ELSE]" compare 0= >r
        2dup s" [IFUNDEF]" compare 0= >r
        2dup s" [IFDEF]" compare 0= r> or r> or r> or
        IF   s" [IF]" Opennest place
             s" [THEN]" Closenest place
             SKIPNEST THEN ;

