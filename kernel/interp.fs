\ definitions needed for interpreter / compiler only

\ here allot , c, A,                                   17dec92py

: allot ( n -- ) \ core
    dup unused u> -8 and throw
    dp +! ;
: c,    ( c -- ) \ core
    here 1 chars allot c! ;
: ,     ( x -- ) \ core
    here cell allot  ! ;
: 2,	( w1 w2 -- ) \ gforth
    here 2 cells allot 2! ;

\ : aligned ( addr -- addr' ) \ core
\     [ cell 1- ] Literal + [ -1 cells ] Literal and ;
: align ( -- ) \ core
    here dup aligned swap ?DO  bl c,  LOOP ;

\ : faligned ( addr -- f-addr ) \ float
\     [ 1 floats 1- ] Literal + [ -1 floats ] Literal and ;

: falign ( -- ) \ float
    here dup faligned swap
    ?DO
	bl c,
    LOOP ;

: maxalign ( -- ) \ float
    here dup maxaligned swap
    ?DO
	bl c,
    LOOP ;

\ the code field is aligned if its body is maxaligned
' maxalign Alias cfalign ( -- ) \ gforth

' , alias A, ( addr -- ) \ gforth

' NOOP ALIAS const

\ name> found                                          17dec92py

$80 constant alias-mask \ set when the word is not an alias!
$40 constant immediate-mask
$20 constant restrict-mask

: ((name>))  ( nfa -- cfa )
    name>string +  cfaligned ;

: (name>x) ( nfa -- cfa b )
    \ cfa is an intermediate cfa and b is the flags byte of nfa
    dup ((name>))
    swap cell+ c@ dup alias-mask and 0=
    IF
	swap @ swap
    THEN ;

\ input stream primitives                              23feb93py

: tib ( -- c-addr ) \ core-ext
    \ obsolescent
    >tib @ ;
Defer source ( -- addr count ) \ core
\ used by dodefer:, must be defer
: (source) ( -- addr count )
    tib #tib @ ;
' (source) IS source

: (word) ( addr1 n1 char -- addr2 n2 )
  dup >r skip 2dup r> scan  nip - ;

\ (word) should fold white spaces
\ this is what (parse-white) does

\ word parse                                           23feb93py

: parse-word  ( char -- addr len ) \ gforth
  source 2dup >r >r >in @ over min /string
  rot dup bl = IF  drop (parse-white)  ELSE  (word)  THEN
  2dup + r> - 1+ r> min >in ! ;
: word   ( char -- addr ) \ core
  parse-word here place  bl here count + c!  here ;

: parse    ( char -- addr len ) \ core-ext
  >r  source  >in @ over min /string  over  swap r>  scan >r
  over - dup r> IF 1+ THEN  >in +! ;

\ name                                                 13feb93py

: capitalize ( addr len -- addr len ) \ gforth
  2dup chars chars bounds
  ?DO  I c@ toupper I c! 1 chars +LOOP ;

[IFUNDEF] (name) \ name might be a primitive
: (name) ( -- c-addr count )
    source 2dup >r >r >in @ /string (parse-white)
    2dup + r> - 1+ r> min >in ! ;
\    name count ;
[THEN]

: name-too-short? ( c-addr u -- c-addr u )
    dup 0= -&16 and throw ;

: name-too-long? ( c-addr u -- c-addr u )
    dup $1F u> -&19 and throw ;

\ Literal                                              17dec92py

: Literal  ( compilation n -- ; run-time -- n ) \ core
    postpone lit  , ; immediate restrict
: ALiteral ( compilation addr -- ; run-time -- addr ) \ gforth
    postpone lit A, ; immediate restrict

: char   ( 'char' -- n ) \ core
    bl word char+ c@ ;
: [char] ( compilation 'char' -- ; run-time -- n )
    char postpone Literal ; immediate restrict

\ threading                                   17mar93py

: cfa,     ( code-address -- )  \ gforth	cfa-comma
    here
    dup lastcfa !
    0 A, 0 ,  code-address! ;
: compile, ( xt -- )	\ core-ext	compile-comma
    A, ;
: !does    ( addr -- ) \ gforth	store-does
    lastxt does-code! ;
: (does>)  ( R: addr -- )
    r> cfaligned /does-handler + !does ;
: dodoes,  ( -- )
  cfalign here /does-handler allot does-handler! ;

: (compile) ( -- ) \ gforth
    r> dup cell+ >r @ compile, ;

: postpone, ( w xt -- )
    \g Compiles the compilation semantics represented by @var{w xt}.
    dup ['] execute =
    if
	drop compile,
    else
	dup ['] compile, =
	if
	    drop POSTPONE (compile) compile,
	else
	    swap POSTPONE aliteral compile,
	then
    then ;

: POSTPONE ( "name" -- ) \ core
    \g Compiles the compilation semantics of @var{name}.
    COMP' postpone, ; immediate restrict

: interpret/compile: ( interp-xt comp-xt "name" -- ) \ gforth
    Create immediate swap A, A,
DOES>
    abort" executed primary cfa of an interpret/compile: word" ;
\    state @ IF  cell+  THEN  perform ;

\ number? number                                       23feb93py

hex
const Create bases   10 ,   2 ,   A , 100 ,
\                     16     2    10   character
\ !! this saving and restoring base is an abomination! - anton
: getbase ( addr u -- addr' u' )
    over c@ [char] $ - dup 4 u<
    IF
	cells bases + @ base ! 1 /string
    ELSE
	drop
    THEN ;
: s>number ( addr len -- d )
    base @ >r  dpl on
    over c@ '- =  dup >r
    IF
	1 /string
    THEN
    getbase  dpl on  0 0 2swap
    BEGIN
	dup >r >number dup
    WHILE
	dup r> -
    WHILE
	dup dpl ! over c@ [char] . =
    WHILE
	1 /string
    REPEAT  THEN
        2drop rdrop dpl off
    ELSE
	2drop rdrop r>
	IF
	    dnegate
	THEN
    THEN
    r> base ! ;

: snumber? ( c-addr u -- 0 / n -1 / d 0> )
    s>number dpl @ 0=
    IF
	2drop false  EXIT
    THEN
    dpl @ dup 0> 0= IF
	nip
    THEN ;

: number? ( string -- string 0 / n -1 / d 0> )
    dup >r count snumber? dup if
	rdrop
    else
	r> swap
    then ;

: number ( string -- d )
    number? ?dup 0= abort" ?"  0<
    IF
	s>d
    THEN ;

\ interpret                                            10mar92py

Defer parser
Defer name ( -- c-addr count ) \ gforth
\ get the next word from the input buffer
' (name) IS name
Defer compiler-notfound ( c-addr count -- )
Defer interpreter-notfound ( c-addr count -- )

: no.extensions  ( addr u -- )
    2drop -&13 bounce ;
' no.extensions IS compiler-notfound
' no.extensions IS interpreter-notfound

: compile-only-error ( ... -- )
    -&14 throw ;

: interpret ( ?? -- ?? ) \ gforth
    \ interpret/compile the (rest of the) input buffer
    BEGIN
	?stack name dup
    WHILE
	parser
    REPEAT
    2drop ;

\ interpreter compiler                                 30apr92py

\ not the most efficient implementations of interpreter and compiler
: interpreter ( c-addr u -- ) 
    2dup find-name dup
    if
	nip nip name>int execute
    else
	drop
	2dup 2>r snumber?
	IF
	    2rdrop
	ELSE
	    2r> interpreter-notfound
	THEN
    then ;

: compiler ( c-addr u -- )
    2dup find-name dup
    if ( c-addr u nt )
	nip nip name>comp execute
    else
	drop
	2dup snumber? dup
	IF
	    0>
	    IF
		swap postpone Literal
	    THEN
	    postpone Literal
	    2drop
	ELSE
	    drop compiler-notfound
	THEN
    then ;

' interpreter  IS  parser

: [ ( -- ) \ core	left-bracket
    ['] interpreter  IS parser state off ; immediate
: ] ( -- ) \ core	right-bracket
    ['] compiler     IS parser state on  ;

\ Strings                                              22feb93py

: ," ( "string"<"> -- ) [char] " parse
  here over char+ allot  place align ;
: SLiteral ( Compilation c-addr1 u ; run-time -- c-addr2 u ) \ string
    postpone (S") here over char+ allot  place align ;
                                             immediate restrict
: ( ( compilation 'ccc<close-paren>' -- ; run-time -- ) \ core,file	paren
    [char] ) parse 2drop ; immediate

: \ ( -- ) \ core-ext backslash
    blk @
    IF
	>in @ c/l / 1+ c/l * >in !
	EXIT
    THEN
    source >in ! drop ; immediate

: \G ( -- ) \ gforth backslash
    POSTPONE \ ; immediate

\ error handling                                       22feb93py
\ 'abort thrown out!                                   11may93jaw

: abort" ( compilation 'ccc"' -- ; run-time f -- ) \ core,exception-ext	abort-quote
    postpone (abort") ," ;        immediate restrict

\ Header states                                        23feb93py

: cset ( bmask c-addr -- )
    tuck c@ or swap c! ; 
: creset ( bmask c-addr -- )
    tuck c@ swap invert and swap c! ; 
: ctoggle ( bmask c-addr -- )
    tuck c@ xor swap c! ; 

: lastflags ( -- c-addr )
    \ the address of the flags byte in the last header
    \ aborts if the last defined word was headerless
    last @ dup 0= abort" last word was headerless" cell+ ;

: immediate ( -- ) \ core
    immediate-mask lastflags cset ;
: restrict ( -- ) \ gforth
    restrict-mask lastflags cset ;
' restrict alias compile-only ( -- ) \ gforth

\ Header                                               23feb93py

\ input-stream, nextname and noname are quite ugly (passing
\ information through global variables), but they are useful for dealing
\ with existing/independent defining words

defer (header)
defer header ( -- ) \ gforth
' (header) IS header

: string, ( c-addr u -- ) \ gforth
    \G puts down string as cstring
    dup c, here swap chars dup allot move ;

: header, ( c-addr u -- ) \ gforth
    name-too-long?
    align here last !
    current @ 1 or A,	\ link field; before revealing, it contains the
			\ tagged reveal-into wordlist
    string, cfalign
    alias-mask lastflags cset ;

: input-stream-header ( "name" -- )
    name name-too-short? header, ;
: input-stream ( -- )  \ general
    \G switches back to getting the name from the input stream ;
    ['] input-stream-header IS (header) ;

' input-stream-header IS (header)

\ !! make that a 2variable
create nextname-buffer 32 chars allot

: nextname-header ( -- )
    nextname-buffer count header,
    input-stream ;

\ the next name is given in the string
: nextname ( c-addr u -- ) \ gforth
    name-too-long?
    nextname-buffer c! ( c-addr )
    nextname-buffer count move
    ['] nextname-header IS (header) ;

: noname-header ( -- )
    0 last ! cfalign
    input-stream ;

: noname ( -- ) \ gforth
\ the next defined word remains anonymous. The xt of that word is given by lastxt
    ['] noname-header IS (header) ;

: lastxt ( -- xt ) \ gforth
\ xt is the execution token of the last word defined. The main purpose of this word is to get the xt of words defined using noname
    lastcfa @ ;

: Alias    ( cfa "name" -- ) \ gforth
    Header reveal
    alias-mask lastflags creset
    dup A, lastcfa ! ;

: name>string ( nt -- addr count ) \ gforth	name-to-string
    \g @var{addr count} is the name of the word represented by @var{nt}.
    cell+ count $1F and ;

: head>string
  cell+ count $1F and ;


const Create ???  0 , 3 c, char ? c, char ? c, char ? c,
\ ??? is used by dovar:, must be created/:dovar

: >head ( cfa -- nt ) \ gforth	to-name
 $21 cell do
   dup i - count $9F and + cfaligned over alias-mask + = if
     i - cell - unloop exit
   then
 cell +loop
 drop ??? ( wouldn't 0 be better? ) ;

' >head ALIAS >name 

: body> 0 >body - ;

doer? :dovar [IF]
: Create ( "name" -- ) \ core
    Header reveal dovar: cfa, ;
[ELSE]
: Create ( "name" -- ) \ core
    Header reveal here lastcfa ! 0 A, 0 , DOES> ;
[THEN]

\ Create Variable User Constant                        17mar93py

: Variable ( "name" -- ) \ core
    Create 0 , ;
: AVariable ( "name" -- ) \ gforth
    Create 0 A, ;
: 2Variable ( "name" -- ) \ double
    create 0 , 0 , ;

: uallot ( n -- )  udp @ swap udp +! ;

doer? :douser [IF]
: User ( "name" -- ) \ gforth
    Header reveal douser: cfa, cell uallot , ;
: AUser ( "name" -- ) \ gforth
    User ;
[ELSE]
: User Create cell uallot , DOES> @ up @ + ;
: AUser User ;
[THEN]

doer? :docon [IF]
    : (Constant)  Header reveal docon: cfa, ;
[ELSE]
    : (Constant)  Create DOES> @ ;
[THEN]
: Constant ( w "name" -- ) \ core
    \G Defines constant @var{name}
    \G  
    \G @var{name} execution: @var{-- w}
    (Constant) , ;
: AConstant ( addr "name" -- ) \ gforth
    (Constant) A, ;
: Value ( w "name" -- ) \ core-ext
    (Constant) , ;

: 2Constant ( w1 w2 "name" -- ) \ double
    Create ( w1 w2 "name" -- )
        2,
    DOES> ( -- w1 w2 )
        2@ ;
    
doer? :dofield [IF]
    : (Field)  Header reveal dofield: cfa, ;
[ELSE]
    : (Field)  Create DOES> @ + ;
[THEN]
\ IS Defer What's Defers TO                            24feb93py

doer? :dodefer [IF]
: Defer ( "name" -- ) \ gforth
    \ !! shouldn't it be initialized with abort or something similar?
    Header Reveal dodefer: cfa,
    ['] noop A, ;
[ELSE]
: Defer ( "name" -- ) \ gforth
    Create ['] noop A,
DOES> @ execute ;
[THEN]

: Defers ( "name" -- ) \ gforth
    ' >body @ compile, ; immediate

\ : ;                                                  24feb93py

defer :-hook ( sys1 -- sys2 )
defer ;-hook ( sys2 -- sys1 )

: : ( "name" -- colon-sys ) \ core	colon
    Header docol: cfa, defstart ] :-hook ;
: ; ( compilation colon-sys -- ; run-time nest-sys ) \ core	semicolon
    ;-hook ?struc postpone exit reveal postpone [ ; immediate restrict

: :noname ( -- xt colon-sys ) \ core-ext	colon-no-name
    0 last !
    cfalign here docol: cfa, 0 ] :-hook ;

\ Search list handling                                 23feb93py

: last?   ( -- false / nfa nfa )
    last @ ?dup ;
: (reveal) ( nt wid -- )
    ( wid>wordlist-id ) dup >r
    @ over ( name>link ) ! 
    r> ! ;

\ object oriented search list                          17mar93py

\ word list structure:

struct
  cell% field find-method   \ xt: ( c_addr u wid -- nt )
  cell% field reveal-method \ xt: ( nt wid -- ) \ used by dofield:, must be field
  cell% field rehash-method \ xt: ( wid -- )	   \ re-initializes a "search-data" (hashtables)
  cell% field hash-method   \ xt: ( wid -- )    \ initializes ""
\   \ !! what else
end-struct wordlist-map-struct

struct
  cell% field wordlist-id \ not the same as wid; representation depends on implementation
  cell% field wordlist-map \ pointer to a wordlist-map-struct
  cell% field wordlist-link \ link field to other wordlists
  cell% field wordlist-extend \ points to wordlist extensions (eg hashtables)
end-struct wordlist-struct

: f83find      ( addr len wordlist -- nt / false )
    ( wid>wordlist-id ) @ (f83find) ;

: initvoc		( wid -- )
  dup wordlist-map @ hash-method perform ;

\ Search list table: find reveal
Create f83search ( -- wordlist-map )
    ' f83find A,  ' (reveal) A,  ' drop A, ' drop A,

here NIL A, G f83search T A, NIL A, NIL A,
AValue forth-wordlist \ variable, will be redefined by search.fs

AVariable lookup       	forth-wordlist lookup !
\ !! last is user and lookup?! jaw
AVariable current ( -- addr ) \ gforth
AVariable voclink	forth-wordlist wordlist-link voclink !
lookup AValue context

forth-wordlist current !

\ higher level parts of find

struct
    >body
    cell% field interpret/compile-int
    cell% field interpret/compile-comp
end-struct interpret/compile-struct

: (cfa>int) ( cfa -- xt )
    dup interpret/compile?
    if
	interpret/compile-int @
    then ;

: (x>int) ( cfa b -- xt )
    \ get interpretation semantics of name
    restrict-mask and
    if
	drop ['] compile-only-error
    else
	(cfa>int)
    then ;

: name>int ( nt -- xt ) \ gforth
    \G @var{xt} represents the interpretation semantics of the word
    \G @var{nt}. Produces @code{' compile-only-error} if
    \G @var{nt} is compile-only.
    (name>x) (x>int) ;

: name?int ( nt -- xt ) \ gforth
    \G Like name>int, but throws an error if compile-only.
    (name>x) restrict-mask and
    if
	compile-only-error \ does not return
    then
    (cfa>int) ;

: name>comp ( nt -- w xt ) \ gforth
    \G @var{w xt} is the compilation token for the word @var{nt}.
    (name>x) >r dup interpret/compile?
    if
	interpret/compile-comp @
    then
    r> immediate-mask and if
	['] execute
    else
	['] compile,
    then ;

: (search-wordlist)  ( addr count wid -- nt / false )
    dup wordlist-map @ find-method perform ;

: flag-sign ( f -- 1|-1 )
    \ true becomes 1, false -1
    0= 2* 1+ ;

: (name>intn) ( nfa -- xt +-1 )
    (name>x) tuck (x>int) ( b xt )
    swap immediate-mask and flag-sign ;

: search-wordlist ( addr count wid -- 0 / xt +-1 ) \ search
    \ xt is the interpretation semantics
    (search-wordlist) dup if
	(name>intn)
    then ;

: find-name ( c-addr u -- nt/0 ) \ gforth
    \g Find the name @var{c-addr u} in the current search
    \g order. Return its nt, if found, otherwise 0.
    lookup @ (search-wordlist) ;

: sfind ( c-addr u -- 0 / xt +-1  ) \ gforth-obsolete
    find-name dup
    if ( nt )
	state @
	if
	    name>comp ['] execute = flag-sign
	else
	    (name>intn)
	then
    then ;

: find ( c-addr -- xt +-1 / c-addr 0 ) \ core,search
    dup count sfind dup
    if
	rot drop
    then ;

: (') ( "name" -- nt ) \ gforth
    name find-name dup 0=
    IF
	drop -&13 bounce
    THEN  ;

: [(')]  ( compilation "name" -- ; run-time -- nt ) \ gforth	bracket-paren-tick
    (') postpone ALiteral ; immediate restrict

: '    ( "name" -- xt ) \ core	tick
    \g @var{xt} represents @var{name}'s interpretation
    \g semantics. Performs @code{-14 throw} if the word has no
    \g interpretation semantics.
    (') name?int ;
: [']  ( compilation. "name" -- ; run-time. -- xt ) \ core	bracket-tick
    \g @var{xt} represents @var{name}'s interpretation
    \g semantics. Performs @code{-14 throw} if the word has no
    \g interpretation semantics.
    ' postpone ALiteral ; immediate restrict

: COMP'    ( "name" -- w xt ) \ gforth	c-tick
    \g @var{w xt} represents @var{name}'s compilation semantics.
    (') name>comp ;
: [COMP']  ( compilation "name" -- ; run-time -- w xt ) \ gforth	bracket-comp-tick
    \g @var{w xt} represents @var{name}'s compilation semantics.
    COMP' swap POSTPONE Aliteral POSTPONE ALiteral ; immediate restrict

\ reveal words

Variable warnings ( -- addr ) \ gforth
G -1 warnings T !

: check-shadow  ( addr count wid -- )
\G prints a warning if the string is already present in the wordlist
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

: reveal ( -- ) \ gforth
    last?
    if \ the last word has a header
	dup ( name>link ) @ 1 and
	if \ it is still hidden
	    dup ( name>link ) @ 1 xor		( nt wid )
	    2dup >r name>string r> check-shadow ( nt wid )
	    dup wordlist-map @ reveal-method perform
	then
    then ;

: rehash  ( wid -- )
    dup wordlist-map @ rehash-method perform ;

\ Query                                                07apr93py

has? file 0= [IF]
: sourceline# ( -- n )  loadline @ ;
[THEN]

: refill ( -- flag ) \ core-ext,block-ext,file-ext
  blk @  IF  1 blk +!  true  0 >in !  EXIT  THEN
  tib /line
[ has? file [IF] ]
  loadfile @ ?dup
  IF    read-line throw
  ELSE
[ [THEN] ]
      sourceline# 0< IF 2drop false EXIT THEN
      accept true
[ has? file [IF] ]
  THEN
[ [THEN] ]
  1 loadline +!
  swap #tib ! 0 >in ! ;

: query   ( -- ) \ core-ext
    \G obsolescent
    blk off loadfile off
    tib /line accept #tib ! 0 >in ! ;

\ save-mem extend-mem

has? os [IF]
: save-mem	( addr1 u -- addr2 u ) \ gforth
    \g copy a memory block into a newly allocated region in the heap
    swap >r
    dup allocate throw
    swap 2dup r> -rot move ;

: extend-mem	( addr1 u1 u -- addr addr2 u2 )
    \ extend memory block allocated from the heap by u aus
    \ the (possibly reallocated piece is addr2 u2, the extension is at addr
    over >r + dup >r resize throw
    r> over r> + -rot ;
[THEN]

\ RECURSE                                               17may93jaw

: recurse ( compilation -- ; run-time ?? -- ?? ) \ core
    \g calls the current definition.
    lastxt compile, ; immediate restrict
' reveal alias recursive ( compilation -- ; run-time -- ) \ gforth
\g makes the current definition visible, enabling it to call itself
\g recursively.
	immediate restrict

\ EVALUATE                                              17may93jaw

has? file 0= [IF]
: push-file  ( -- )  r>
  sourceline# >r  tibstack @ >r  >tib @ >r  #tib @ >r
  >tib @ tibstack @ = IF  r@ tibstack +!  THEN
  tibstack @ >tib ! >in @ >r  >r ;

: pop-file   ( throw-code -- throw-code )
  r>
  r> >in !  r> #tib !  r> >tib !  r> tibstack !  r> loadline !  >r ;
[THEN]

: evaluate ( c-addr len -- ) \ core,block
  push-file  #tib ! >tib !
  >in off blk off loadfile off -1 loadline !
  ['] interpret catch
  pop-file throw ;

: abort ( ?? -- ?? ) \ core,exception-ext
    -1 throw ;

\+ environment? true ENV" CORE"
\ core wordset is now complete!

\ Quit                                                 13feb93py

Defer 'quit
Defer .status
: prompt        state @ IF ."  compiled" EXIT THEN ."  ok" ;
: (Query)  ( -- )
    loadfile off  blk off  refill drop ;
: (quit)  BEGIN  .status cr (query) interpret prompt  AGAIN ;
' (quit) IS 'quit

\ DOERROR (DOERROR)                                     13jun93jaw

8 Constant max-errors
Variable error-stack  0 error-stack !
max-errors 6 * cells allot
\ format of one cell:
\ source ( addr u )
\ >in
\ line-number
\ Loadfilename ( addr u )

: dec. ( n -- ) \ gforth
    \ print value in decimal representation
    base @ decimal swap . base ! ;

: hex. ( u -- ) \ gforth
    \ print value as unsigned hex number
    '$ emit base @ swap hex u. base ! ;

: typewhite ( addr u -- ) \ gforth
    \ like type, but white space is printed instead of the characters
    bounds ?do
	i c@ #tab = if \ check for tab
	    #tab
	else
	    bl
	then
	emit
    loop ;

DEFER DOERROR

: .error-frame ( addr1 u1 n1 n2 addr2 u2 -- )
  cr error-stack @
  IF
     ." in file included from "
     type ." :" dec.  drop 2drop
  ELSE
     type ." :" dec.
     cr dup 2over type cr drop
     nip -trailing 1- ( line-start index2 )
     0 >r  BEGIN
                  2dup + c@ bl >  WHILE
		  r> 1+ >r  1- dup 0<  UNTIL  THEN  1+
     ( line-start index1 )
     typewhite
     r> 1 max 0 ?do \ we want at least one "^", even if the length is 0
                  [char] ^ emit
     loop
  THEN
;

: (DoError) ( throw-code -- )
  [ has? os [IF] ]
      outfile-id dup flush-file drop >r
      stderr to outfile-id
  [ [THEN] ] 
  sourceline# IF
               source >in @ sourceline# 0 0 .error-frame
  THEN
  error-stack @ 0 ?DO
    -1 error-stack +!
    error-stack dup @ 6 * cells + cell+
    6 cells bounds DO
      I @
    cell +LOOP
    .error-frame
  LOOP
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
  normal-dp dpp ! 
  [ has? os [IF] ] r> to outfile-id [ [THEN] ]
  ;

' (DoError) IS DoError

: quit ( ?? -- ?? ) \ core
    rp0 @ rp! handler off >tib @ >r
    BEGIN
	postpone [
	['] 'quit CATCH dup
    WHILE
	DoError r@ >tib ! r@ tibstack !
    REPEAT
    drop r> >tib ! ;

\ Cold                                                 13feb93py

\ : .name ( name -- ) name>string type space ;
\ : words  listwords @
\          BEGIN  @ dup  WHILE  dup .name  REPEAT drop ;

: (bootmessage)
    ." GForth " version-string type 
    ." , Copyright (C) 1994-1997 Free Software Foundation, Inc." cr
    ." GForth comes with ABSOLUTELY NO WARRANTY; for details type `license'"
[ has? os [IF] ]
     cr ." Type `bye' to exit"
[ [THEN] ] ;

defer bootmessage
defer process-args

' (bootmessage) IS bootmessage

Defer 'cold 
\ hook (deferred word) for things to do right before interpreting the
\ command-line arguments
' noop IS 'cold

include chains.fs

Variable init8

: cold ( -- ) \ gforth
[ has? file [IF] ]
    pathstring 2@ fpath only-path 
    init-included-files
[ [THEN] ]
    'cold
    init8 chainperform
[ has? file [IF] ]
    ['] process-args catch ?dup
    IF
      dup >r DoError cr r> negate (bye)
    THEN
    argc @ 1 >
    IF	\ there may be some unfinished line, so let's finish it
	cr
    THEN
[ [THEN] ]
    bootmessage
    loadline off quit ;

: boot ( path **argv argc -- )
    main-task up!
[ has? os [IF] ]
    stdout TO outfile-id
[ [THEN] ]
[ has? file [IF] ]
    argc ! argv ! pathstring 2!
[ [THEN] ]
    sp@ sp0 !
[ has? glocals [IF] ]
    lp@ forthstart 7 cells + @ - 
[ [ELSE] ]
    [ has? os [IF] ]
    sp@ $1040 +
    [ [ELSE] ]
    sp@ $40 +
    [ [THEN] ]
[ [THEN] ]
    dup >tib ! tibstack ! #tib off >in off
    rp@ rp0 !
[ has? floating [IF] ]
    fp@ fp0 !
[ [THEN] ]
    ['] cold catch DoError
[ has? os [IF] ]
    bye
[ [THEN] ]
;

has? os [IF]
: bye ( -- ) \ tools-ext
[ has? file [IF] ]
    script? 0= IF  cr  THEN
[ [ELSE] ]
    cr
[ [THEN] ]
    0 (bye) ;
[THEN]

\ **argv may be scanned by the C starter to get some important
\ information, as -display and -geometry for an X client FORTH
\ or space and stackspace overrides

\ 0 arg contains, however, the name of the program.
