\ Hashed dictionaries                                  15jul94py

9 value hashbits
1 hashbits lshift Value Hashlen

Variable insRule        insRule on
Variable revealed

\ Memory handling                                      10oct94py

Variable HashPointer
Variable HashTable
Variable HashIndex

\ DelFix and NewFix are from bigFORTH                  15jul94py

: DelFix ( addr root -- ) dup @ 2 pick ! ! ;
: NewFix  ( root len # -- addr )
  BEGIN  2 pick @ ?dup  0= WHILE  2dup * allocate throw
         over 0 ?DO  dup 4 pick DelFix 2 pick +  LOOP  drop
  REPEAT  >r drop r@ @ rot ! r@ swap erase r> ;

\ compute hash key                                     15jul94py

: hash ( addr len -- key )
    hashbits (hashkey1) ;
\   (hashkey)
\   Hashlen 1- and ;


: hash-find ( addr len wordlist -- nfa / false )
    $C + @ >r
    2dup hash r> xor cells HashTable @ + @ (hashfind) ;

\ hash vocabularies                                    16jul94py

: lastlink! ( addr link -- )
  BEGIN  dup @ dup  WHILE  nip  REPEAT  drop ! ;

: (reveal ( addr voc -- )  $C + dup @ 0< IF  2drop EXIT  THEN
  @ over cell+ count $1F and Hash xor cells >r
  HashPointer 8 $400 NewFix
  tuck cell+ ! r> HashTable @ + insRule @
  IF  dup @ 2 pick ! !  ELSE  lastlink!  THEN  revealed on ;

: hash-reveal ( -- )  (reveal) last?  IF
  current @ (reveal  THEN ;

: addall  ( -- )
    voclink
    BEGIN  @ dup @  WHILE  dup 'initvoc  REPEAT  drop ;

: clearhash  ( -- )
    HashTable @ Hashlen cells bounds
    DO  I @
        BEGIN  dup  WHILE
               dup @ swap HashPointer DelFix
        REPEAT  I !
    cell +LOOP  HashIndex off ;

: rehash  clearhash addall ;
: (rehash) ( addr -- )
  drop revealed @ IF  rehash revealed off  THEN ;

Create hashsearch  ' hash-find A, ' hash-reveal A, ' (rehash) A,

\ hash allocate and vocabulary initialization          10oct94py

: hash-alloc ( addr -- addr )  HashTable @ 0= IF
  Hashlen cells allocate throw HashTable !
  HashTable @ Hashlen cells erase THEN
  HashIndex @ over !  1 HashIndex +!
  HashIndex @ Hashlen >=
  IF  clearhash
      1 hashbits 1+ dup  to hashbits  lshift  to hashlen
      HashTable @ free
      addall
  THEN ;

: (initvoc) ( addr -- )
    cell+ dup @ 0< IF  drop EXIT  THEN
    insRule @ >r  insRule off  hash-alloc
    3 cells - hashsearch over cell+ ! dup
    BEGIN  @ dup  WHILE  2dup swap (reveal  REPEAT
    2drop  r> insRule ! ;

' (initvoc) IS 'initvoc

\ Hash-Find                                            01jan93py

addall          \ Baum aufbauen
\ Baumsuche ist installiert.

: hash-cold  ( -- ) Defers 'cold
  HashPointer off  HashTable off  HashIndex off
  voclink
  BEGIN  @ dup @  WHILE
         dup cell - @ >r
         dup 'initvoc
         r> over cell - !
  REPEAT  drop ;
' hash-cold IS 'cold

: .words  ( -- )
  base @ >r hex HashTable @  Hashlen 0
  DO  cr  i 2 .r ." : " dup i cells +
      BEGIN  @ dup  WHILE
             dup cell+ @ .name  REPEAT  drop
  LOOP  drop r> base ! ;

\ \ this stuff is for evaluating the hash function
\ : square dup * ;

\ : countwl  ( -- sum sumsq )
\     \ gives the number of words in the current wordlist
\     \ and the sum of squares for the sublist lengths
\     0 0
\     hashtable @ Hashlen cells bounds DO
\        0 i BEGIN
\            @ dup WHILE
\            swap 1+ swap
\        REPEAT
\        drop
\        swap over square +
\        >r + r>
\        1 cells
\    +LOOP ;

\ : chisq ( -- n )
\     \ n should have about the same size as Hashlen
\     countwl Hashlen 2 pick */ swap - ;
