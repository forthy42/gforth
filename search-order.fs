\ search order wordset                                 14may93py

$10 constant maxvp
Variable vp
  0 A, 0 A,  0 A, 0 A,   0 A, 0 A,   0 A, 0 A, 
  0 A, 0 A,  0 A, 0 A,   0 A, 0 A,   0 A, 0 A, 

: get-current  ( -- wid )  current @ ;
: set-current  ( wid -- )  current ! ;

: context ( -- addr )  vp dup @ cells + ;
: definitions  ( -- )  context @ current ! ;

\ wordlist Vocabulary also previous                    14may93py

AVariable voclink

: wordlist  ( -- wid )
  here  0 A, Forth-wordlist cell+ @ A, voclink @ A, 0 A,
  dup 2 cells + voclink ! ;

: Vocabulary ( -- ) Create wordlist drop  DOES> context ! ;

: also  ( -- )
  context @ vp @ 1+ dup maxvp > abort" Vocstack full"
  vp ! context ! ;

: previous ( -- )  vp @ 1- dup 0= abort" Vocstack empty" vp ! ;

\ vocabulary find                                      14may93py

: (vocfind)  ( addr count nfa1 -- nfa2 / false )  drop
  1 vp @ DO  2dup vp I cells + @ (search-wordlist)
             dup IF  nip nip UNLOOP exit  THEN  drop
          -1 +LOOP  2drop false ;

Create vocsearch       ] (vocfind) (reveal) [

\ Only root                                            14may93py

wordlist vocsearch over cell+ A!

Vocabulary Forth
Vocabulary Root

: Only  vp off  also Root also definitions ;

\ set initial search order                             14may93py

Forth-wordlist @ ' Forth >body A!

Only Forth also definitions

search A!

\ get-order set-order                                  14may93py

: get-order  ( -- wid1 .. widn n )
  vp @ 0 ?DO  vp cell+ I cells + @  LOOP  vp @ ;

: set-order  ( wid1 .. widn n / -1 -- )
  dup -1 = IF  drop Only exit  THEN  dup vp !
  ?dup IF  1- FOR  vp cell+ I cells + !  NEXT  THEN ;

: seal ( -- )  context @ 1 set-order ;

\ words visible in roots                               14may93py

: .name ( name -- ) name>string type space ;
: words  cr 0 context @
  BEGIN  @ dup  WHILE  2dup cell+ c@ $1F and 2 + dup >r +
         &79 >  IF  cr nip 0 swap  THEN
         dup .name space r> rot + swap  REPEAT 2drop ;

: body> ( data -- cfa )  0 >body - ;

: .voc  body> >name .name ;
: order  1 vp @  DO  vp I cells + @ .voc  -1 +LOOP  2 spaces
  current @ .voc ;
: vocs   voclink  BEGIN  @ dup @  WHILE  dup 2 cells - .voc  REPEAT  drop ;

Root definitions

' words Alias words
' Forth Alias Forth

Forth definitions
