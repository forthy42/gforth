\ Hashed dictionaries                                  15jul94py

$80 Value Hashlen

Variable insRule        insRule on

\ Memory handling                                      15jul94py

Variable HashPointer

: hash-alloc ( addr -- addr )  dup @ 0= IF
  Hashlen cells allocate throw over !
  dup @ Hashlen cells erase THEN ;

\ DelFix and NewFix is from bigFORTH                   15jul94py

: DelFix ( addr root -- ) dup @ 2 pick ! ! ;
: NewFix  ( root len # -- addr )
  BEGIN  2 pick @ ?dup  0= WHILE  2dup * allocate throw
         over 0 ?DO  dup 4 pick DelFix 2 pick +  LOOP  drop
  REPEAT  >r drop r@ @ rot ! r@ swap erase r> ;

\ compute hash key                                     15jul94py

: hash ( addr len -- key )  (hashkey)
\  tuck bounds  ?DO  I c@ toupper +  LOOP
  Hashlen 1- and ;

: hash-find ( addr len wordlist -- nfa / false ) $C + @ >r
  2dup hash cells r> + @ (hashfind) ;
\  BEGIN  dup  WHILE
\         2@ >r >r dup r@ cell+ c@ $1F and =
\         IF  2dup r@ cell+ char+ capscomp 0=
\	     IF  2drop r> rdrop  EXIT  THEN  THEN
\	 rdrop r>
\  REPEAT nip nip ;

\ hash vocabularies                                    16jul94py

: lastlink! ( addr link -- )
  BEGIN  dup @ dup  WHILE  nip  REPEAT  drop ! ;

: (reveal ( addr voc -- )  $C + dup @ 0< IF  2drop EXIT  THEN
  hash-alloc @ over cell+ count $1F and Hash cells + >r
  HashPointer 8 $400 NewFix
  tuck cell+ ! r> insRule @
  IF  dup @ 2 pick ! !  ELSE  lastlink!  THEN ;

: hash-reveal ( -- )  (reveal) last?  IF
  current @ (reveal  THEN ;

Create hashsearch  ' hash-find A,  ' hash-reveal A,  ' drop A,

: (initvoc ( addr -- )  cell+ dup @ 0< IF  drop EXIT  THEN
  insRule @ >r  insRule off  hash-alloc
  3 cells - hashsearch over cell+ ! dup
  BEGIN  @ dup  WHILE  2dup swap (reveal  REPEAT
  2drop  r> insRule ! ;

' (initvoc IS 'initvoc

: addall  ( -- )  voclink
  BEGIN  @ dup @  WHILE  dup (initvoc  REPEAT  drop ;

\ Hash-Find                                            01jan93py

addall          \ Baum aufbauen
\ Baumsuche ist installiert.

: .words  ( -- )
  base @ >r hex context @ 3 cells +  HashLen 0
  DO  cr  i 2 .r ." : " dup @ i cells +
      BEGIN  @ dup  WHILE
             dup cell+ @ .name  REPEAT  drop
  LOOP  drop r> base ! ;

