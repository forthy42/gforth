\ Interpretative Structuren                            16feb92py

Variable countif

: dummy ;  immediate
: >exec  >r ; restrict ( :-)
: scanIF   f83find  dup 0=  IF  drop ['] dummy >name  THEN  ;

Create [struct]-search    ' scanIF A,  ' (reveal) A,  ' drop A,
Create [struct]-voc       NIL A,       G [struct]-search T A,
                          NIL A,       NIL A,

: ?if  countif @ 0<
  IF  [ [struct]-voc 3 cells + ] ALiteral @ lookup !  THEN ;

UNLOCK Tlast @ NIL Tlast ! LOCK

: [IF]      1 countif +! ?if ;       immediate
: [THEN]   -1 countif +! ?if ;       immediate
: [ELSE]   postpone [THEN] r> >exec postpone [IF] ;
                                     immediate
' [IF]   Alias [IFDEF]               immediate
' [IF]   Alias [IFUNDEF]             immediate
' [THEN] Alias [ENDIF]                immediate
' [IF]   Alias [BEGIN]               immediate
' [IF]   Alias [WHILE]               immediate
' [THEN] Alias [UNTIL]               immediate
' [THEN] Alias [AGAIN]               immediate
' [IF]   Alias [DO]                  immediate
' [IF]   Alias [?DO]                 immediate
' [THEN] Alias [LOOP]                immediate
' [THEN] Alias [+LOOP]               immediate
: [REPEAT]  postpone [AGAIN] postpone [THEN] ;
                                     immediate
' ( Alias (                          immediate
' \ Alias \                          immediate

UNLOCK Tlast @ swap Tlast ! LOCK
1 cells - G [struct]-voc T !

\ Interpretative Structuren                            30apr92py

: defined   bl word find nip 0<> ; immediate
: [IF] 0= IF  countif off
              lookup @ [ [struct]-voc 3 cells + ] ALiteral !
	      [struct]-voc lookup !
          THEN ;                                      immediate
: [IFDEF]   postpone defined    postpone [IF] ;       immediate
: [IFUNDEF] postpone defined 0= postpone [IF] ;       immediate
: [ELSE] 0 postpone [IF] ;                            immediate
: [THEN] ;                                            immediate
: [ENDIF] ;                                           immediate

\ Structs for interpreter                              28nov92py

User (i)

: [DO]  ( start end -- )  >in @ -rot
  DO   I (i) ! dup >r >in ! interpret r> swap +LOOP  drop ;
                                                      immediate
: [?DO] 2dup = IF 2drop postpone [ELSE] ELSE postpone [DO] THEN ;
                                                      immediate
: [+LOOP] ( n -- ) rdrop rdrop ;                      immediate
: [LOOP] ( -- ) 1 rdrop rdrop ;                       immediate
: [FOR] ( n -- )  0 swap postpone [DO] ;              immediate
: [NEXT] ( n -- ) -1 rdrop rdrop ;                    immediate
: [I] ( -- index ) (I) @ postpone Literal ;           immediate
: [BEGIN] >in @ >r BEGIN r@ >in ! interpret UNTIL rdrop ;
                                                      immediate
' [+LOOP]  Alias [UNTIL] immediate
: [REPEAT]  ( -- )  false rdrop rdrop ;               immediate
' [REPEAT] Alias [AGAIN] immediate
: [WHILE]   ( flag -- )
  0= IF   postpone [ELSE] true rdrop rdrop 1 countif +!  THEN ;
                                                      immediate

