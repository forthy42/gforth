\ EXTEND.FS    CORE-EXT Word not fully tested!         12may93jaw

\ May be cross-compiled

decimal

\ .(                                                    12may93jaw

: .(    [char] ) parse type ;

\ VALUE 2>R 2R> 2R@                                     17may93jaw

: value ( n -- )
  (constant) , ;
\ !! 2value

: 2Literal  swap postpone Literal  postpone Literal ;
  immediate restrict

: m*/    ( d1 n2 u3 -- dqout )    >r s>d >r abs -rot
  s>d r> xor r> swap >r >r dabs rot tuck um* 2swap um*
  swap >r 0 d+ r> -rot r@ um/mod -rot r> um/mod nip swap
  r> IF dnegate THEN ;

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

: unused   s0 @ 512 -        \ for stack
           here - ;

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
  loadfile @ dup 0= IF  drop loadline @ 0 min  THEN ;

: save-input ( -- x1 .. xn n )
  >in @
  loadfile @ ?dup
  IF    dup file-position throw loadline @ >tib @ 6
        #tib @ >tib +!
  ELSE  loadline @ blk @ linestart @ >tib @ 5 THEN
;

: restore-input ( x1 .. xn n -- flag )
  swap >tib !
  6 = IF   loadline ! rot dup loadfile !
           reposition-file IF drop true EXIT THEN
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

