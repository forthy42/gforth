\ definitions needed for interpreter only

\ \ Revision-Log

\       put in seperate file				14sep97jaw 

\ \ input stream primitives                       	23feb93py

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

: sword  ( char -- addr len ) \ gforth
  \G parses like @code{word}, but the output is like @code{parse} output
  \ this word was called PARSE-WORD until 0.3.0, but Open Firmware and
  \ dpANS6 A.6.2.2008 have a word with that name that behaves
  \ differently (like NAME).
  source 2dup >r >r >in @ over min /string
  rot dup bl = IF  drop (parse-white)  ELSE  (word)  THEN
  2dup + r> - 1+ r> min >in ! ;

: word   ( char -- addr ) \ core
  sword here place  bl here count + c!  here ;

: parse    ( char -- addr len ) \ core-ext
  >r  source  >in @ over min /string  over  swap r>  scan >r
  over - dup r> IF 1+ THEN  >in +! ;

\ name                                                 13feb93py

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

\ \ Number parsing					23feb93py

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

\ \ Comments ( \ \G

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

\ \ object oriented search list                         17mar93py

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
    ' f83find A,  ' drop A,  ' drop A, ' drop A,

here NIL A, G f83search T A, NIL A, NIL A,
AValue forth-wordlist \ variable, will be redefined by search.fs

AVariable lookup       	forth-wordlist lookup !
\ !! last is user and lookup?! jaw
AVariable current ( -- addr ) \ gforth
AVariable voclink	forth-wordlist wordlist-link voclink !
lookup AValue context

forth-wordlist current !

\ \ header, finding, ticks                              17dec92py

$80 constant alias-mask \ set when the word is not an alias!
$40 constant immediate-mask
$20 constant restrict-mask

\ higher level parts of find

: flag-sign ( f -- 1|-1 )
    \ true becomes 1, false -1
    0= 2* 1+ ;

: compile-only-error ( ... -- )
    -&14 throw ;

: (cfa>int) ( cfa -- xt )
[ has? compiler [IF] ]
    dup interpret/compile?
    if
	interpret/compile-int @
    then 
[ [THEN] ] ;

: (x>int) ( cfa b -- xt )
    \ get interpretation semantics of name
    restrict-mask and
    if
	drop ['] compile-only-error
    else
	(cfa>int)
    then ;

: name>string ( nt -- addr count ) \ gforth     head-to-string
    \g @var{addr count} is the name of the word represented by @var{nt}.
    cell+ count $1F and ;

: ((name>))  ( nfa -- cfa )
    name>string + cfaligned ;

: (name>x) ( nfa -- cfa b )
    \ cfa is an intermediate cfa and b is the flags byte of nfa
    dup ((name>))
    swap cell+ c@ dup alias-mask and 0=
    IF
        swap @ swap
    THEN ;

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

: (name>comp) ( nt -- w +-1 ) \ gforth
    \G @var{w xt} is the compilation token for the word @var{nt}.
    (name>x) >r 
[ has? compiler [IF] ]
    dup interpret/compile?
    if
        interpret/compile-comp @
    then 
[ [THEN] ]
    r> immediate-mask and flag-sign
    ;

: (name>intn) ( nfa -- xt +-1 )
    (name>x) tuck (x>int) ( b xt )
    swap immediate-mask and flag-sign ;

const Create ???  0 , 3 c, char ? c, char ? c, char ? c,
\ ??? is used by dovar:, must be created/:dovar

: >head ( cfa -- nt ) \ gforth  to-name
 $21 cell do
   dup i - count $9F and + cfaligned over alias-mask + = if
     i - cell - unloop exit
   then
 cell +loop
 drop ??? ( wouldn't 0 be better? ) ;

' >head ALIAS >name

: body> 0 >body - ;

: (search-wordlist)  ( addr count wid -- nt / false )
    dup wordlist-map @ find-method perform ;

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
	    (name>comp)
	else
	    (name>intn)
	then
   then ;

: find ( c-addr -- xt +-1 / c-addr 0 ) \ core,search
    dup count sfind dup
    if
	rot drop
    then ;

\ ticks

: (') ( "name" -- nt ) \ gforth
    name find-name dup 0=
    IF
	drop -&13 bounce
    THEN  ;

: '    ( "name" -- xt ) \ core	tick
    \g @var{xt} represents @var{name}'s interpretation
    \g semantics. Performs @code{-14 throw} if the word has no
    \g interpretation semantics.
    (') name?int ;

\ \ the interpreter loop				  mar92py

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

: interpret ( ?? -- ?? ) \ gforth
    \ interpret/compile the (rest of the) input buffer
    BEGIN
	?stack name dup
    WHILE
	parser
    REPEAT
    2drop ;

\ interpreter                                 	30apr92py

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

' interpreter  IS  parser

\ \ Query Evaluate                                 	07apr93py

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

\ \ Quit                                            	13feb93py

Defer 'quit

Defer .status

: prompt        state @ IF ."  compiled" EXIT THEN ."  ok" ;

: (Query)  ( -- )
    loadfile off  blk off loadline off refill drop ;

: (quit)  BEGIN  .status cr (query) interpret prompt  AGAIN ;

' (quit) IS 'quit

\ \ DOERROR (DOERROR)                        		13jun93jaw

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
    rp0 @ rp! handler off clear-tibstack >tib @ >r
    BEGIN
	[ has? compiler [IF] ]
	postpone [
	[ [THEN] ]
	['] 'quit CATCH dup
    WHILE
	DoError r@ >tib ! r@ tibstack !
    REPEAT
    drop r> >tib ! ;

\ \ Cold Boot                                    	13feb93py

: (bootmessage)
    ." GForth " version-string type 
    ." , Copyright (C) 1994-1998 Free Software Foundation, Inc." cr
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

include ../chains.fs

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

: clear-tibstack ( -- )
[ has? glocals [IF] ]
    lp@ forthstart 7 cells + @ - 
[ [ELSE] ]
    [ has? os [IF] ]
    sp@ $1040 +
    [ [ELSE] ]
    sp@ $40 +
    [ [THEN] ]
[ [THEN] ]
    dup >tib ! tibstack ! #tib off >in off ;

: boot ( path **argv argc -- )
    main-task up!
[ has? os [IF] ]
    stdout TO outfile-id
\ !! [ [THEN] ]
\ !! [ has? file [IF] ]
    argc ! argv ! pathstring 2!
[ [THEN] ]
    sp@ sp0 !
    clear-tibstack
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

