\ EXTEND.FS    CORE-EXT Word not fully tested!         12may93jaw

\ May be cross-compiled

decimal

\ .(                                                    12may93jaw

: .(    [char] ) parse type ;

\ VALUE 2>R 2R> 2R@                                     17may93jaw

: value ( n -- )
  (constant) , ;
\ !! 2value

: 2>r   postpone >r postpone >r ; immediate restrict
: 2r>   postpone r> postpone r> ; immediate restrict
: 2r@   postpone 2r> postpone 2dup postpone 2>r ; immediate restrict

: 2Literal  swap postpone Literal  postpone Literal ;
  immediate restrict

\ CASE OF ENDOF ENDCASE                                 17may93jaw

\ just as described in dpANS5

0 CONSTANT case immediate

: of
        1+ >r
        postpone over postpone = postpone if postpone drop
        r> ; immediate

: endof
        >r postpone else r> ; immediate

: endcase
        postpone drop
        0 ?do postpone then loop ; immediate

\ C"                                                    17may93jaw

: (c")     "lit ;

: CLiteral postpone (c") here over char+ allot  place align ;
                                              immediate restrict
: C"       [char] " parse postpone CLiteral ; immediate restrict

\ UNUSED                                                17may93jaw

: unused   forthstart dup @ over 2 cells + @ -
           512 -        \ for stack
           + here - ;

\ [COMPILE]                                             17may93jaw

: [compile] 
 ' compile, ; immediate

\ MARKER                                                17may93jaw

\ : marker here last @ create , , DOES> dup @ last ! cell+ @ dp ! ;
\ doesn't work now. vocabularies?

\ CONVERT                                               17may93jaw

: convert true >number drop ;

\ ERASE                                                 17may93jaw

: erase ( 0 1 chars um/mod nip )  0 fill ;
: blank ( 0 1 chars um/mod nip ) bl fill ;

\ ROLL                                                  17may93jaw

: roll  dup 1+ pick >r
        cells sp@ 2 cells + dup cell+ rot move drop r> ;

\ SOURCE-ID SAVE-INPUT RESTORE-INPUT                    11jun93jaw

: source-id ( -- 0 | -1 | fileid )
  loadfile @ dup 0= IF drop linestart @ THEN ;

: save-input ( -- x1 .. xn n )
  >in @
  loadfile @ ?dup
  IF    linestart 2@ loadline @ 6
  ELSE  loadline @ blk @ linestart @ 5 THEN
  >tib @ swap ; \ >tib for security

: restore-input ( x1 .. xn n -- flag )
  1- swap >tib @ <> IF discard true EXIT THEN
  5 = IF   loadline ! 2dup linestart 2! rot dup loadfile !
           reposition-file IF drop true EXIT THEN
           refill 0= IF drop true EXIT THEN
      ELSE linestart ! blk !
           dup loadline @ <> IF 2drop true EXIT THEN
           loadline !
      THEN
  >in ! false ;



\ This things we don't need, but for being complete... jaw

\ EXPECT SPAN                                           17may93jaw

variable span

: expect ( c-addr +len -- )
  0 rot over
  BEGIN  key decode >r 2over = r> or  UNTIL
  2drop nip span ! ;

