\ OTHER.FS     Ansforth extentions for CROSS           9may93jaw

\ make ansforth compatible                              9may93jaw
\ the cross compiler should run
\ with any ansforth environment

: ?EXIT    s" IF EXIT THEN" evaluate ; immediate
: bounds   over + swap ;
: capitalize ( addr -- addr )
  dup count chars bounds
  ?DO  I c@ [char] a [char] { within
       IF  I c@ bl - I c!  THEN  1 chars +LOOP ;
: name bl word capitalize ;
: on true swap ! ;
: off false swap ! ;
: place ( adr len adr )
        2dup c! char+ swap move ;
: +place ( adr len adr )
        2dup c@ + over c!
        dup c@ char+ + swap move ;
: -rot  rot rot ;

include toolsext.fs

