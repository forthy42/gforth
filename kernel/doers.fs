

\ If we leave out the compiler we need the runtime code for our defining
\ words. This file defines the defining words without the
\ interpretative/compiling part.

has? compiler 0= [IF]

\ fillers for interpreter only mode
.( Do-Fillers: )

: (does>) ;    

doer? :dofield 0= [IF] .( DOFIELD )
| : (Field)  DOES> @ + ;
[THEN]

doer? :dodefer 0= [IF] .( DODEFER )
| : Defer ( "name" -- ) DOES> @ execute ;
[THEN]

| : 2Constant ( w1 w2 "name" -- ) \ double
    DOES> ( -- w1 w2 )
        2@ ;

doer? :docon 0= [IF] .( DOCON )
| : (Constant)  DOES> @ ;
[THEN]

doer? :douser 0= [IF] .( DOUSER )
| : User DOES> @ up @ + ;
[THEN]

doer? :dovar 0= [IF] .( DOVAR )
| : Create ( "name" -- ) \ core
    DOES> ;

.( .)
[THEN]

