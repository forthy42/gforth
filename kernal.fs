\ KERNAL.FS    ANS figFORTH kernal                     17dec92py
\ $ID:
\ Idea and implementation: Bernd Paysan (py)
\ Copyright 1992 by the ANSI figForth Development Group

\ Log:  ', '- usw. durch [char] ... ersetzt
\       man sollte die unterschiedlichen zahlensysteme
\       mit $ und & zumindest im interpreter weglassen
\       schon erledigt!
\       11may93jaw
\ name>         0= nicht vorhanden              17may93jaw
\               nfa can be lfa or nfa!
\ find          splited into find and (find)
\               (find) for later use            17may93jaw
\ search        replaced by lookup because
\               it is a word of the string wordset
\                                               20may93jaw
\ postpone      added immediate                 21may93jaw
\ to            added immediate                 07jun93jaw
\ cfa, header   put "here lastcfa !" in
\               cfa, this is more logical
\               and noname: works wothout
\               extra "here lastcfa !"          08jun93jaw
\ (parse-white) thrown out
\ refill        added outer trick
\               to show there is something
\               going on                        09jun93jaw
\ leave ?leave  somebody forgot UNLOOP!!!       09jun93jaw
\ leave ?leave  unloop thrown out
\               unloop after loop is used       10jun93jaw

HEX

\ Bit string manipulation                              06oct92py

Create bits  80 c, 40 c, 20 c, 10 c, 8 c, 4 c, 2 c, 1 c,
DOES> ( n -- )  + c@ ;

: >bit  ( addr n -- c-addr mask )  8 /mod rot + swap bits ;
: +bit  ( addr n -- )  >bit over c@ or swap c! ;

: relinfo ( -- addr )  forthstart dup @ + ;
: >rel  ( addr -- n )  forthstart - ;
: relon ( addr -- )  relinfo swap >rel cell / +bit ;

\ here allot , c, A,                                   17dec92py

: dp	( -- addr )  dpp @ ;
: here  ( -- here )  dp @ ;
: allot ( n -- )     dp +! ;
: c,    ( c -- )     here 1 chars allot c! ;
: ,     ( x -- )     here cell allot  ! ;
: 2,    ( w1 w2 -- ) \ general
    here 2 cells allot 2! ;

: aligned ( addr -- addr' )
  [ cell 1- ] Literal + [ -1 cells ] Literal and ;
: align ( -- )          here dup aligned swap ?DO  bl c,  LOOP ;

: A!    ( addr1 addr2 -- )  dup relon ! ;
: A,    ( addr -- )     here cell allot A! ;

\ on off                                               23feb93py

: on  ( addr -- )  true  swap ! ;
: off ( addr -- )  false swap ! ;

\ name> found                                          17dec92py

: (name>)  ( nfa -- cfa )    count  $1F and  +  aligned ;
: name>    ( nfa -- cfa )
  dup  (name>) swap  c@ $80 and 0= IF  @ THEN ;

: found ( nfa -- cfa n )  cell+
  dup c@ >r  (name>) r@ $80 and  0= IF  @       THEN
\                  -1 r@ $40 and     IF  1-      THEN
                  -1 r> $20 and     IF  negate  THEN  ;

\ (find)                                               17dec92py

\ : (find) ( addr count nfa1 -- nfa2 / false )
\   BEGIN  dup  WHILE  dup >r
\          cell+ count $1F and dup >r 2over r> =
\          IF  -text  0= IF  2drop r> EXIT  THEN
\          ELSE  2drop drop  THEN  r> @
\   REPEAT nip nip ;

\ place bounds                                         13feb93py

: place  ( addr len to -- ) over >r  rot over 1+  r> move c! ;
: bounds ( beg count -- end beg )  over + swap ;

\ input stream primitives                              23feb93py

: tib   >tib @ ;
Defer source
: (source) ( -- addr count ) tib #tib @ ;
' (source) IS source

\ (word)                                               22feb93py

: scan   ( addr1 n1 char -- addr2 n2 )  >r
  BEGIN  dup  WHILE  over c@ r@ <>  WHILE  1 /string
  REPEAT  THEN  rdrop ;
: skip   ( addr1 n1 char -- addr2 n2 )  >r
  BEGIN  dup  WHILE  over c@ r@  =  WHILE  1 /string
  REPEAT  THEN  rdrop ;

: (word) ( addr1 n1 char -- addr2 n2 )
  dup >r skip 2dup r> scan  nip - ;

\ (word) should fold white spaces
\ this is what (parse-white) does

\ word parse                                           23feb93py

: parse-word  ( char -- addr len )
  source 2dup >r >r >in @ /string
  rot dup bl = IF  drop (parse-white)  ELSE  (word)  THEN
  2dup + r> - 1+ r> min >in ! ;
: word   ( char -- addr )
  parse-word here place  bl here count + c!  here ;

: parse    ( char -- addr len )
  >r  source  >in @ /string  over  swap r>  scan >r
  over - dup r> IF 1+ THEN  >in +! ;

\ name                                                 13feb93py

: capitalize ( addr -- addr )
  dup count chars bounds
  ?DO  I c@ toupper I c! 1 chars +LOOP ;
: (name)  ( -- addr )  bl word ;

\ Literal                                              17dec92py

: Literal  ( n -- )  state @ 0= ?EXIT  postpone lit  , ;
                                                      immediate
: ALiteral ( n -- )  state @ 0= ?EXIT  postpone lit A, ;
                                                      immediate

: char   ( 'char' -- n )  bl word char+ c@ ;
: [char] ( 'char' -- n )  char postpone Literal ; immediate
' [char] Alias Ascii immediate

: (compile) ( -- )  r> dup cell+ >r @ A, ;
: postpone ( "name" -- )
  name find dup 0= abort" Can't compile "
  0> IF  A,  ELSE  postpone (compile) A,  THEN ;
                                             immediate restrict

\ Use (compile) for the old behavior of compile!

\ digit?                                               17dec92py

: digit?   ( char -- digit true/ false )
  base @ $100 = ?dup ?EXIT
  toupper [char] 0 - dup 9 u> IF
    [ 'A '9 1 + -  ] literal -
    dup 9 u<= IF
      drop false EXIT
    THEN
  THEN
  dup base @ u>= IF
    drop false EXIT
  THEN
  true ;

: accumulate ( +d0 addr digit - +d1 addr )
  swap >r swap  base @  um* drop rot  base @  um* d+ r> ;
: >number ( d addr count -- d addr count )
  0 ?DO  count digit? WHILE  accumulate  LOOP 0
  ELSE  1- I' I - UNLOOP  THEN ;

\ number? number                                       23feb93py

Create bases   10 ,   2 ,   A , 100 ,
\              16     2    10   Zeichen
\ !! this saving and restoring base is an abomination! - anton
: getbase ( addr u -- addr' u' )  over c@ [char] $ - dup 4 u<
  IF  cells bases + @ base ! 1 /string  ELSE  drop  THEN ;
: number?  ( string -- string 0 / n -1 )  base @ >r
  dup count over c@ [char] - = dup >r  IF 1 /string  THEN
  getbase  dpl on  0 0 2swap
  BEGIN  dup >r >number dup  WHILE  dup r> -  WHILE
         dup dpl ! over c@ [char] . =  WHILE
         1 /string
  REPEAT  THEN  2drop 2drop rdrop false r> base ! EXIT  THEN
  2drop rot drop rdrop r> IF dnegate THEN
  dpl @ dup 0< IF  nip  THEN  r> base ! ;
: s>d ( n -- d ) dup 0< ;
: number ( string -- d )
  number? ?dup 0= abort" ?"  0< IF s>d THEN ;

\ space spaces ud/mod                                  21mar93py
decimal
Create spaces  bl 80 times \ times from target compiler! 11may93jaw
DOES>   ( u -- )  swap
        0 max 0 ?DO  I' I - &80 min 2dup type  +LOOP  drop ;
hex
: space   1 spaces ;

: ud/mod ( ud1 u2 -- urem udquot )  >r 0 r@ um/mod r> swap >r
                                    um/mod r> ;

: pad    ( -- addr )
  here [ $20 8 2* cells + 2 + cell+ ] Literal + aligned ;

\ hold <# #> sign # #s                                 25jan92py

: hold    ( char -- )         pad cell - -1 chars over +! @ c! ;

: <#                          pad cell - dup ! ;

: #>      ( 64b -- addr +n )  2drop pad cell - dup @ tuck - ;

: sign    ( n -- )            0< IF  [char] - hold  THEN ;

: #       ( +d1 -- +d2 )    base @ 2 max ud/mod rot 9 over <
  IF [ char A char 9 - 1- ] Literal +  THEN  [char] 0 + hold ;

: #s      ( +d -- 0 0 )         BEGIN  # 2dup d0=  UNTIL ;

\ print numbers                                        07jun92py

: d.r      >r tuck  dabs  <# #s  rot sign #>
           r> over - spaces  type ;

: ud.r     >r <# #s #> r> over - spaces type ;

: .r       >r s>d r> d.r ;
: u.r      0 swap ud.r ;

: d.       0 d.r space ;
: ud.      0 ud.r space ;

: .        s>d d. ;
: u.       0 ud. ;

\ catch throw                                          23feb93py
\ bounce                                                08jun93jaw

\ !! what about the other stacks (FP, locals)  anton
\ !! allow the user to add rollback actions    anton
\ !! use a separate exception stack?           anton

: lp@ ( -- addr )
 laddr# [ 0 , ] ;

: catch ( x1 .. xn xt -- y1 .. ym 0 / z1 .. zn error )
  >r sp@ r> swap >r       \ don't count xt! jaw
  fp@ >r
  lp@ >r
  handler @ >r
  rp@ handler !
  execute
  r> handler ! rdrop rdrop 0 ;
: throw ( y1 .. ym error/0 -- y1 .. ym / z1 .. zn error )
  ?DUP IF
    handler @ rp!
    r> handler !
    r> lp!
    r> fp!
    r> swap >r sp! r>
  THEN ;
\ Bouncing is very fine,
\ programming without wasting time...   jaw
: bounce ( y1 .. ym error/0 -- y1 .. ym error / y1 .. ym )
\ a throw without data or fp stack restauration
  ?DUP IF
    handler @ rp!
    r> handler !
    r> lp!
    rdrop
    rdrop
  THEN ;

\ ?stack                                               23feb93py

: ?stack ( ?? -- ?? )  sp@ s0 @ > IF  -4 throw  THEN ;
\ ?stack should be code -- it touches an empty stack!

\ interpret                                            10mar92py

Defer parser
Defer name      ' (name) IS name
Defer notfound

: no.extensions  ( string -- )  IF  &-13 bounce  THEN ;

' no.extensions IS notfound

: interpret
  BEGIN  ?stack name dup c@  WHILE  parser  REPEAT drop ;

\ interpreter compiler                                 30apr92py

: interpreter  ( name -- ) find ?dup
  IF  1 and  IF execute  EXIT THEN  -&14 throw  THEN
  number? 0= IF  notfound THEN ;

' interpreter  IS  parser

: compiler     ( name -- ) find  ?dup
  IF  0> IF  execute EXIT THEN compile, EXIT THEN number? dup
  IF  0> IF  swap postpone Literal  THEN  postpone Literal
  ELSE  drop notfound  THEN ;

: [     ['] interpreter  IS parser state off ; immediate
: ]     ['] compiler     IS parser state on  ;

\ Structural Conditionals                              12dec92py

: ?struc      ( flag -- )       abort" unstructured " ;
: sys?        ( sys -- )        dup 0= ?struc ;
: >mark       ( -- sys )        here  0 , ;
: >resolve    ( sys -- )        here over - swap ! ;
: <resolve    ( sys -- )        here - , ;

: BUT       sys? swap ;                      immediate restrict
: YET       sys? dup ;                       immediate restrict

\ Structural Conditionals                              12dec92py

: AHEAD     postpone branch >mark ;           immediate restrict
: IF        postpone ?branch >mark ;          immediate restrict
: ?DUP-IF \ general
\ This is the preferred alternative to the idiom "?DUP IF", since it can be
\ better handled by tools like stack checkers
    postpone ?dup postpone IF ;       immediate restrict
: ?DUP-NOT-IF \ general
    postpone ?dup postpone 0= postpone if ; immediate restrict
: THEN      sys? dup @ ?struc >resolve ;     immediate restrict
' THEN alias ENDIF immediate restrict \ general
\ Same as "THEN". This is what you use if your program will be seen by
\ people who have not been brought up with Forth (or who have been
\ brought up with fig-Forth).

: ELSE      sys? postpone AHEAD swap postpone THEN ;
                                             immediate restrict

: BEGIN     here ;                           immediate restrict
: WHILE     sys? postpone IF swap ;           immediate restrict
: AGAIN     sys? postpone branch  <resolve ;  immediate restrict
: UNTIL     sys? postpone ?branch <resolve ;  immediate restrict
: REPEAT    over 0= ?struc postpone AGAIN postpone THEN ;
                                             immediate restrict

\ Structural Conditionals                              12dec92py

variable locals-size \ this is the current size of the locals stack
		     \ frame of the current word

: compile-lp+!# ( n -- )
    ?DUP IF
	dup negate locals-size +!
	postpone lp+!#  ,
    THEN ;

\ : EXIT ( -- )
\     locals-size @ compile-lp+!# POSTPONE ;s ; immediate restrict
\ : ?EXIT ( -- )
\     postpone IF postpone EXIT postpone THEN ; immediate restrict

Variable leavings

: (leave)   here  leavings @ ,  leavings ! ;
: LEAVE     postpone branch  (leave) ;  immediate restrict
: ?LEAVE    postpone 0= postpone ?branch  (leave) ;
                                             immediate restrict
: DONE   ( addr -- )
  leavings @
  BEGIN
    2dup u<=
  WHILE
    dup @ swap >resolve
  REPEAT
  leavings ! drop ;                          immediate restrict

\ Structural Conditionals                              12dec92py

: DO        postpone (do)   here ;            immediate restrict

: ?DO       postpone (?do)  (leave) here ;
                                             immediate restrict
: FOR       postpone (for)  here ;            immediate restrict

: loop]     dup <resolve 2 cells - postpone done postpone unloop ;

: LOOP      sys? postpone (loop)  loop] ;     immediate restrict
: +LOOP     sys? postpone (+loop) loop] ;     immediate restrict
: S+LOOP \ general
\ A symmetric version of "+LOOP". I.e., "-high -low ?DO -inc S+LOOP" will iterate as often as "high low ?DO inc S+LOOP". For positive increments it behaves like "+LOOP". Use S+LOOP instead of +LOOP for negative increments.
    sys? postpone (s+loop) loop] ;    immediate restrict
: NEXT      sys? postpone (next)  loop] ;     immediate restrict

\ Strings                                              22feb93py

: ," ( "string"<"> -- ) [char] " parse
  here over char+ allot  place align ;
: "lit ( -- addr )
  r> r> dup count + aligned >r swap >r ;               restrict
: (.")     "lit count type ;                           restrict
: (S")     "lit count ;                                restrict
: SLiteral postpone (S") here over char+ allot  place align ;
                                             immediate restrict
: S"       [char] " parse  state @ IF  postpone SLiteral  THEN ;
                                             immediate
: ."       state @  IF    postpone (.") ,"  align
                    ELSE  [char] " parse type  THEN  ;  immediate
: (        [char] ) parse 2drop ;                       immediate
: \        source >in ! drop ;                          immediate

\ error handling                                       22feb93py
\ 'abort thrown out!                                   11may93jaw

: (abort")      "lit >r IF  r> "error ! -2 throw  THEN
                rdrop ;
: abort"        postpone (abort") ," ;        immediate restrict

\ Header states                                        23feb93py

: flag! ( 8b -- )   last @ cell+ tuck c@ xor swap c! ;
: immediate     $20 flag! ;
\ : restrict      $40 flag! ;
' noop alias restrict

\ Header                                               23feb93py

\ input-stream, nextname and noname are quite ugly (passing
\ information through global variables), but they are useful for dealing
\ with existing/independent defining words

defer header

: name,  ( "name" -- )
    name c@ 1+ chars allot align ;
: input-stream-header ( "name" -- )
    \ !! this is f83-implementation-dependent
    align here last !  -1 A,
    name, $80 flag! ;

: input-stream ( -- )  \ general
\ switches back to getting the name from the input stream ;
    ['] input-stream-header IS header ;

' input-stream-header IS header

\ !! make that a 2variable
create nextname-buffer 32 chars allot

: nextname-header ( -- )
    \ !! f83-implementation-dependent
    nextname-buffer count
    align here last ! -1 A,
    dup c,  here swap chars  dup allot  move  align
    $80 flag!
    input-stream ;

\ the next name is given in the string
: nextname ( c-addr u -- ) \ general
    dup 31 u> -19 and throw ( is name too long? )
    nextname-buffer c! ( c-addr )
    nextname-buffer count move
    ['] nextname-header IS header ;

: noname-header ( -- )
    0 last !
    input-stream ;

: noname ( -- ) \ general
\ the next defined word remains anonymous. The xt of that word is given by lastxt
    ['] noname-header IS header ;

: lastxt ( -- xt ) \ general
\ xt is the execution token of the last word defined. The main purpose of this word is to get the xt of words defined using noname
    lastcfa @ ;

: Alias    ( cfa "name" -- )
  Header reveal , $80 flag! ;

: name>string ( nfa -- addr count )
 cell+ count $1F and ;

Create ???  ," ???"
: >name ( cfa -- nfa )
 $21 cell do
   dup i - count $9F and + aligned over $80 + = if
     i - cell - unloop exit
   then
 cell +loop
 drop ??? ( wouldn't 0 be better? ) ;

\ indirect threading                                   17mar93py

: cfa,     ( code-address -- )
    here lastcfa !
    here  0 A, 0 ,  code-address! ;
: compile, ( xt -- )		A, ;
: !does    ( addr -- )		lastcfa @ does-code! ;
: (;code)  ( R: addr -- )	r> /does-handler + !does ;
: dodoes,  ( -- )
  here /does-handler allot does-handler! ;

\ direct threading is implementation dependent

: Create    Header reveal [ :dovar ] Literal cfa, ;

\ DOES>                                                17mar93py

: DOES>  ( compilation: -- )
    state @
    IF
	;-hook postpone (;code) dodoes,
    ELSE
	dodoes, here !does 0 ]
    THEN 
    :-hook ; immediate

\ Create Variable User Constant                        17mar93py

: Variable  Create 0 , ;
: AVariable Create 0 A, ;
: 2VARIABLE ( "name" -- ) \ double
    create 0 , 0 , ;
    
: User      Variable ;
: AUser     AVariable ;

: (Constant)  Header reveal [ :docon ] Literal cfa, ;
: Constant  (Constant) , ;
: AConstant (Constant) A, ;

: 2CONSTANT
    create ( w1 w2 "name" -- )
        2,
    does> ( -- w1 w2 )
        2@ ;
    
\ IS Defer What's Defers TO                            24feb93py

: Defer
  Create ( -- ) 
    ['] noop A,
  DOES> ( ??? )
    @ execute ;

: IS ( addr "name" -- )
    ' >body
    state @
    IF    postpone ALiteral postpone !  
    ELSE  !
    THEN ;  immediate
' IS Alias TO immediate

: What's ( "name" -- addr )  ' >body
  state @ IF  postpone ALiteral postpone @  ELSE  @  THEN ;
                                             immediate
: Defers ( "name" -- )  ' >body @ compile, ;
                                             immediate restrict

\ : ;                                                  24feb93py

defer :-hook ( sys1 -- sys2 )
defer ;-hook ( sys2 -- sys1 )

: EXIT  ( -- )  postpone ;s ;  immediate

: : ( -- colon-sys )  Header [ :docol ] Literal cfa, 0 ] :-hook ;
: ; ( colon-sys -- )  ;-hook ?struc postpone exit reveal postpone [ ;
  immediate restrict

: :noname ( -- xt colon-sys )  here [ :docol ] Literal cfa, 0 ] :-hook ;

\ Search list handling                                 23feb93py

AVariable current

: last?   ( -- false / nfa nfa )    last @ ?dup ;
: (reveal) ( -- )
  last?
  IF
      dup @ 0<
      IF
	current @ @ over ! current @ !
      ELSE
	drop
      THEN
  THEN ;

\ object oriented search list                          17mar93py

\ word list structure:
\ struct
\   1 cells: field find-method \ xt: ( c_addr u w1 -- name-id ) w1 is a method-\ specific wordlist-id (not the same as wid)
\   1 cells: field reveal-method \ xt: ( -- )
\   \ !! what else
\ end-struct wordlist-map-struct

\ struct
\   1 cells: field wordlist-id \ not the same as wid; representation depends on implementation
\   1 cells: field wordlist-map \ pointer to a wordlist-map-struct
\   1 cells: field ????
\   1 cells: field ????
\ end-struct wordlist-struct


\ Search list table: find reveal
Create f83search    ' (f83find) A,  ' (reveal) A,
Create forth-wordlist  NIL A, G f83search T A, NIL A, NIL A,
AVariable search       G forth-wordlist search T !
G forth-wordlist current T !

: (search-wordlist)  ( addr count wid -- nfa / false )
  dup @ swap cell+ @ @ execute ;

: search-wordlist  ( addr count wid -- 0 / xt +-1 )
  (search-wordlist) dup  IF  found  THEN ;

Variable warnings  G -1 warnings T !

: check-shadow  ( addr count wid -- )
\ prints a warning if the string is already present in the wordlist
\ !! should be refined so the user can suppress the warnings
 >r 2dup 2dup r> (search-wordlist) warnings @ and ?dup if
   ." redefined " name>string 2dup type
   compare 0<> if
     ."  with " type
   else
     2drop
   then
   space space EXIT
 then
 2drop 2drop ;

: find   ( addr -- cfa +-1 / string false )  dup
  count search @ search-wordlist  dup IF  rot drop  THEN ;

: reveal ( -- )
 last? if
   name>string current @ check-shadow
 then
 current @ cell+ @ cell+ @ execute ;

: '    ( "name" -- addr )  name find 0= no.extensions ;
: [']  ( "name" -- addr )  ' postpone ALiteral ; immediate
\ Input                                                13feb93py

07 constant #bell
08 constant #bs
7F constant #del
0D constant #cr                \ the newline key code
0A constant #lf

: bell  #bell emit ;

: backspaces  0 ?DO  #bs emit  LOOP ;
: >string  ( span addr pos1 -- span addr pos1 addr2 len )
  over 3 pick 2 pick chars /string ;
: type-rest ( span addr pos1 -- span addr pos1 back )
  >string tuck type ;
: (del)  ( max span addr pos1 -- max span addr pos2 )
  1- >string over 1+ -rot move
  rot 1- -rot  #bs emit  type-rest bl emit 1+ backspaces ;
: (ins)  ( max span addr pos1 char -- max span addr pos2 )
  >r >string over 1+ swap move 2dup chars + r> swap c!
  rot 1+ -rot type-rest 1- backspaces 1+ ;
: ?del ( max span addr pos1 -- max span addr pos2 0 )
  dup  IF  (del)  THEN  0 ;
: (ret)  type-rest drop true space ;
: back  dup  IF  1- #bs emit  ELSE  #bell emit  THEN 0 ;
: forw 2 pick over <> IF  2dup + c@ emit 1+  ELSE  #bell emit  THEN 0 ;

Create crtlkeys
  ] false false back  false  false false forw  false
    ?del  false (ret) false  false (ret) false false
    false false false false  false false false false
    false false false false  false false false false [

: decode ( max span addr pos1 key -- max span addr pos2 flag )
  dup #del = IF  drop #bs  THEN  \ del is rubout
  dup bl <   IF  cells crtlkeys + @ execute  EXIT  THEN
  >r 2over = IF  rdrop bell 0 EXIT  THEN
  r> (ins) 0 ;

\ decode should better use a table for control key actions
\ to define keyboard bindings later

: accept   ( addr len -- len )
  dup 0< IF    abs over dup 1 chars - c@ tuck type 
\ this allows to edit given strings
         ELSE  0  THEN rot over
  BEGIN  key decode  UNTIL
  2drop nip ;

\ Output                                               13feb93py

DEFER type      \ defer type for a output buffer or fast
                \ screen write

: (type) ( addr len -- )
  bounds ?DO  I c@ emit  LOOP ;

' (TYPE) IS Type

\ DEFER Emit

\ ' (Emit) IS Emit

\ : form  ( -- rows cols )  &24 &80 ;
\ form should be implemented using TERMCAPS or CURSES
\ : rows  form drop ;
\ : cols  form nip  ;

\ Query                                                07apr93py

: refill ( -- flag )
  tib /line
  loadfile @ ?dup
  IF    dup file-position throw linestart 2!
        read-line throw
  ELSE  linestart @ IF 2drop false EXIT THEN
        accept true
  THEN
  1 loadline +!
  swap #tib ! >in off ;

: Query  ( -- )  loadfile off refill drop ;

\ File specifiers                                       11jun93jaw


\ 1 c, here char r c, 0 c,                0 c, 0 c, char b c, 0 c,
\ 2 c, here char r c, char + c, 0 c,
\ 2 c, here char w c, char + c, 0 c, align
4 Constant w/o
2 Constant r/w
0 Constant r/o

\ BIN WRITE-LINE                                        11jun93jaw

\ : bin           dup 1 chars - c@
\                 r/o 4 chars + over - dup >r swap move r> ;

: bin  1+ ;

create nl$ 1 c, A c, 0 c, \ gnu includes usually a cr in dos
                           \ or not unix environments if
                           \ bin is not selected

: write-line    dup >r write-file ?dup IF r> drop EXIT THEN
                nl$ count r> write-file ;

\ include-file                                         07apr93py

: include-file ( i*x fid -- j*x )
  linestart @ >r loadline @ >r loadfile @ >r
  blk @ >r >tib @ >r  #tib @ dup >r  >in @ >r

  >tib +! loadfile !
  0 loadline ! blk off
  BEGIN  refill  WHILE  interpret  REPEAT
  loadfile @ close-file throw

  r> >in !  r> #tib !  r> >tib ! r> blk !
  r> loadfile ! r> loadline ! r> linestart ! ;

: included ( i*x addr u -- j*x )
  r/o open-file throw include-file ;

\ HEX DECIMAL                                           2may93jaw

: decimal a base ! ;
: hex     10 base ! ;

\ DEPTH                                                 9may93jaw

: depth ( -- +n )  sp@ s0 @ swap - cell / ;

\ INCLUDE                                               9may93jaw

: include
        bl word count included ;

\ RECURSE                                               17may93jaw

: recurse  last @ cell+ name> a, ; immediate restrict
\ !! does not work with anonymous words; use lastxt compile,

\ */MOD */                                              17may93jaw

: */mod >r m* r> sm/rem ;

: */ */mod nip ;

\ EVALUATE                                              17may93jaw

: evaluate ( c-addr len -- )
  linestart @ >r loadline @ >r loadfile @ >r
  blk @ >r >tib @ >r  #tib @ dup >r  >in @ >r

  >tib +! dup #tib ! >tib @ swap move
  >in off blk off loadfile off -1 linestart !

  BEGIN  interpret  >in @ #tib @ u>= UNTIL

  r> >in !  r> #tib !  r> >tib ! r> blk !
  r> loadfile ! r> loadline ! r> linestart ! ;


: abort -1 throw ;

\+ environment? true ENV" CORE"
\ core wordset is now complete!

\ Quit                                                 13feb93py

Defer 'quit
Defer .status
: prompt        state @ IF ."  compiled" EXIT THEN ."  ok" ;
: (quit)        BEGIN .status cr query interpret prompt AGAIN ;
' (quit) IS 'quit

\ DOERROR (DOERROR)                                     13jun93jaw

DEFER DOERROR

: (DoError) ( throw-code -- )
         LoadFile @
         IF
         	." Error in line: " Loadline @ . cr
         THEN
         cr source type cr
         source drop >in @ -trailing
         here c@ 1F min dup >r - 1- 0 max nip
         dup spaces 
	 IF
		." ^"
	 THEN
	 r> 0 ?DO
		." -" 
	 LOOP
	 ." ^"
         dup -2 =
         IF 
		"error @ ?dup
		IF
			cr count type 
		THEN
		drop
         ELSE
		.error
	 THEN
         normal-dp dpp ! ;

' (DoError) IS DoError

: quit   r0 @ rp! handler off >tib @ >r
  BEGIN
    postpone [
    ['] 'quit CATCH dup
  WHILE
    DoError r@ >tib !
  REPEAT
  drop r> >tib ! ;

\ Cold                                                 13feb93py

\ : .name ( name -- ) cell+ count $1F and type space ;
\ : words  listwords @
\          BEGIN  @ dup  WHILE  dup .name  REPEAT drop ;

: >len  ( cstring -- addr n )  100 0 scan 0 swap 100 - /string ;
: arg ( n -- addr count )  cells argv @ + @ >len ;
: #!       postpone \ ;  immediate

Variable env
Variable argv
Variable argc

: get-args ( -- )  #tib off
  argc @ 1 ?DO  I arg 2dup source + swap move
                #tib +! drop  bl source + c! 1 #tib +!  LOOP
  >in off #tib @ 0<> #tib +! ;

: script? ( -- flag )  0 arg 1 arg dup 3 pick - /string compare 0= ;

: cold ( -- )  
  argc @ 1 >
  IF  script?
      IF  1 arg ['] included  ELSE   get-args ['] interpret  THEN
      catch ?dup IF  dup >r DoError cr r> (bye)  THEN THEN
  ." ANS FORTH-93 (c) 1993 by the ANS FORTH-93 Team" cr quit ;

: boot ( **env **argv argc -- )
  argc ! argv ! env !  main-task up!
  sp@ dup s0 ! $10 + >tib ! rp@ r0 !  fp@ f0 !  cold ;

: bye  cr 0 (bye) ;

\ **argv may be scanned by the C starter to get some important
\ information, as -display and -geometry for an X client FORTH
\ or space and stackspace overrides

\ 0 arg contains, however, the name of the program.
