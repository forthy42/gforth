\ VARS.FS      Kernal variables

hex \ everything now hex!                               11may93jaw

\ important constants                                  17dec92py

\ dpANS6 (sect 3.1.3.1) says 
\ "a true flag ... [is] a single-cell value with all bits set"
\ better definition: 0 0= constant true ( no dependence on 2's compl)
 -1 Constant true               \ see starts looking for
                                \ primitives after this word!
  0 Constant false

1 cells Constant cell

20 Constant bl

FF Constant /line

\ that's enough so long

\ User variables                                       13feb93py

\ doesn't produce real user variables now, but normal variables

AUser s0
AUser r0
AUser f0
AUser handler
\ AUser output
\ AUser input

AUser errorhandler

AUser "error            0 "error !

 User >tib
 User #tib
 User >in               0 >in !
 User blk               0 blk !
 User loadfile          0 loadfile !

 User loadline          \ number of the currently interpreted
                        \ (in TIB) line if the interpretation
                        \ is in a textfile
                        \ the first line is 1

 2User linestart        \ starting file postition of
                        \ the current interpreted line (in TIB)

 User base              $A base !
 User dpl               -1 dpl !

 User state             0 state !
AUser dp
AUser LastCFA
AUser Last


