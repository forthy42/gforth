\ kernel.fs    GForth kernel                        17dec92py

\ Copyright (C) 1995 Free Software Foundation, Inc.

\ This file is part of Gforth.

\ Gforth is free software; you can redistribute it and/or
\ modify it under the terms of the GNU General Public License
\ as published by the Free Software Foundation; either version 2
\ of the License, or (at your option) any later version.

\ This program is distributed in the hope that it will be useful,
\ but WITHOUT ANY WARRANTY; without even the implied warranty of
\ MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
\ GNU General Public License for more details.

\ You should have received a copy of the GNU General Public License
\ along with this program; if not, write to the Free Software
\ Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.

\ Idea and implementation: Bernd Paysan (py)

HEX

\ labels for some code addresses

doer? :docon [IF]
: docon: ( -- addr )	\ gforth
    \G the code address of a @code{CONSTANT}
    ['] bl >code-address ;
[THEN]

: docol: ( -- addr )	\ gforth
    \G the code address of a colon definition
    ['] docol: >code-address ;

doer? :dovar [IF]
: dovar: ( -- addr )	\ gforth
    \G the code address of a @code{CREATE}d word
    ['] udp >code-address ;
[THEN]

doer? :douser [IF]
: douser: ( -- addr )	\ gforth
    \G the code address of a @code{USER} variable
    ['] s0 >code-address ;
[THEN]

doer? :dodefer [IF]
: dodefer: ( -- addr )	\ gforth
    \G the code address of a @code{defer}ed word
    ['] source >code-address ;
[THEN]

doer? :dofield [IF]
: dofield: ( -- addr )	\ gforth
    \G the code address of a @code{field}
    ['] reveal-method >code-address ;
[THEN]

has-prims 0= [IF]
: dodoes: ( -- addr )	\ gforth
    \G the code address of a @code{field}
    ['] spaces >code-address ;
[THEN]

NIL AConstant NIL \ gforth

\ Aliases

' i Alias r@ ( -- w ; R: w -- w ) \ core r-fetch
\ copy w from the return stack to the data stack

\ Bit string manipulation                              06oct92py

\ Create bits  80 c, 40 c, 20 c, 10 c, 8 c, 4 c, 2 c, 1 c,
\ DOES> ( n -- )  + c@ ;

\ : >bit  ( addr n -- c-addr mask )  8 /mod rot + swap bits ;
\ : +bit  ( addr n -- )  >bit over c@ or swap c! ;

\ : relinfo ( -- addr )  forthstart dup @ + !!bug!! ;
\ : >rel  ( addr -- n )  forthstart - ;
\ : relon ( addr -- )  relinfo swap >rel cell / +bit ;

\ here allot , c, A,                                   17dec92py

: dp	( -- addr ) \ gforth
    dpp @ ;
: here  ( -- here ) \ core
    dp @ ;
: allot ( n -- ) \ core
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

\ !! this is machine-dependent, but works on all but the strangest machines

: maxaligned ( addr -- f-addr ) \ float
    [ /maxalign 1 - ] Literal + [ 0 /maxalign - ] Literal and ;
: maxalign ( -- ) \ float
    here dup maxaligned swap
    ?DO
	bl c,
    LOOP ;

\ !! machine-dependent and won't work if "0 >body" <> "0 >body maxaligned"
' maxaligned Alias cfaligned ( addr1 -- addr2 ) \ gforth
\ the code field is aligned if its body is maxaligned
' maxalign Alias cfalign ( -- ) \ gforth

: chars ( n1 -- n2 ) \ core
; immediate


\ : A!    ( addr1 addr2 -- ) \ gforth
\    dup relon ! ;
\ : A,    ( addr -- ) \ gforth
\    here cell allot A! ;
' ! alias A! ( addr1 addr2 -- ) \ gforth
' , alias A, ( addr -- ) \ gforth 


\ on off                                               23feb93py

: on  ( addr -- ) \ gforth
    true  swap ! ;
: off ( addr -- ) \ gforth
    false swap ! ;

\ dabs roll                                           17may93jaw

: dabs ( d1 -- d2 ) \ double
    dup 0< IF dnegate THEN ;

: roll  ( x0 x1 .. xn n -- x1 .. xn x0 ) \ core-ext
  dup 1+ pick >r
  cells sp@ cell+ dup cell+ rot move drop r> ;

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

\ place bounds                                         13feb93py

: place  ( addr len to -- ) \ gforth
    over >r  rot over 1+  r> move c! ;
: bounds ( beg count -- end beg ) \ gforth
    over + swap ;

\ input stream primitives                              23feb93py

: tib ( -- c-addr ) \ core-ext
    \ obsolescent
    >tib @ ;
Defer source ( -- addr count ) \ core
\ used by dodefer:, must be defer
: (source) ( -- addr count )
    tib #tib @ ;
' (source) IS source

\ (word)                                               22feb93py

: scan   ( addr1 n1 char -- addr2 n2 ) \ gforth
    \ skip all characters not equal to char
    >r
    BEGIN
	dup
    WHILE
	over c@ r@ <>
    WHILE
	1 /string
    REPEAT  THEN
    rdrop ;
: skip   ( addr1 n1 char -- addr2 n2 ) \ gforth
    \ skip all characters equal to char
    >r
    BEGIN
	dup
    WHILE
	over c@ r@  =
    WHILE
	1 /string
    REPEAT  THEN
    rdrop ;

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
: (name) ( -- c-addr count )
    source 2dup >r >r >in @ /string (parse-white)
    2dup + r> - 1+ r> min >in ! ;
\    name count ;

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

: (compile) ( -- ) \ gforth
    r> dup cell+ >r @ compile, ;

: postpone, ( w xt -- ) \ gforth postpone-comma
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

\ Use (compile) for the old behavior of compile!

\ digit?                                               17dec92py

: digit?   ( char -- digit true/ false ) \ gforth
  base @ $100 =
  IF
    true EXIT
  THEN
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

: >number ( d addr count -- d addr count ) \ core
    0
    ?DO
	count digit?
    WHILE
	accumulate
    LOOP
        0
    ELSE
	1- I' I -
	UNLOOP
    THEN ;

\ number? number                                       23feb93py

Create bases   10 ,   2 ,   A , 100 ,
\              16     2    10   character
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
: s>d ( n -- d ) \ core		s-to-d
    dup 0< ;
: number ( string -- d )
    number? ?dup 0= abort" ?"  0<
    IF
	s>d
    THEN ;

\ space spaces ud/mod                                  21mar93py
decimal
Create spaces ( u -- ) \ core
bl 80 times \ times from target compiler! 11may93jaw
DOES>   ( u -- )
    swap
    0 max 0 ?DO  I' I - &80 min 2dup type  +LOOP  drop ;
Create backspaces
08 80 times \ times from target compiler! 11may93jaw
DOES>   ( u -- )
    swap
    0 max 0 ?DO  I' I - &80 min 2dup type  +LOOP  drop ;
hex
: space ( -- ) \ core
    1 spaces ;

: ud/mod ( ud1 u2 -- urem udquot ) \ gforth
    >r 0 r@ um/mod r> swap >r
    um/mod r> ;

: pad    ( -- addr ) \ core-ext
  here [ $20 8 2* cells + 2 + cell+ ] Literal + aligned ;

\ hold <# #> sign # #s                                 25jan92py

: hold    ( char -- ) \ core
    pad cell - -1 chars over +! @ c! ;

: <# ( -- ) \ core	less-number-sign
    pad cell - dup ! ;

: #>      ( xd -- addr u ) \ core	number-sign-greater
    2drop pad cell - dup @ tuck - ;

: sign    ( n -- ) \ core
    0< IF  [char] - hold  THEN ;

: #       ( ud1 -- ud2 ) \ core		number-sign
    base @ 2 max ud/mod rot 9 over <
    IF
	[ char A char 9 - 1- ] Literal +
    THEN
    [char] 0 + hold ;

: #s      ( +d -- 0 0 ) \ core	number-sign-s
    BEGIN
	# 2dup or 0=
    UNTIL ;

\ print numbers                                        07jun92py

: d.r ( d n -- ) \ double	d-dot-r
    >r tuck  dabs  <# #s  rot sign #>
    r> over - spaces  type ;

: ud.r ( ud n -- ) \ gforth	u-d-dot-r
    >r <# #s #> r> over - spaces type ;

: .r ( n1 n2 -- ) \ core-ext	dot-r
    >r s>d r> d.r ;
: u.r ( u n -- )  \ core-ext	u-dot-r
    0 swap ud.r ;

: d. ( d -- ) \ double	d-dot
    0 d.r space ;
: ud. ( ud -- ) \ gforth	u-d-dot
    0 ud.r space ;

: . ( n -- ) \ core	dot
    s>d d. ;
: u. ( u -- ) \ core	u-dot
    0 ud. ;

\ catch throw                                          23feb93py
\ bounce                                                08jun93jaw

\ !! allow the user to add rollback actions    anton
\ !! use a separate exception stack?           anton

has-locals [IF]
: lp@ ( -- addr ) \ gforth	l-p-fetch
 laddr# [ 0 , ] ;
[THEN]

Defer 'catch
Defer 'throw

' noop IS 'catch
' noop IS 'throw

: catch ( x1 .. xn xt -- y1 .. ym 0 / z1 .. zn error ) \ exception
    'catch
    sp@ >r
[ has-floats [IF] ]
    fp@ >r
[ [THEN] ]
[ has-locals [IF] ]
    lp@ >r
[ [THEN] ]
    handler @ >r
    rp@ handler !
    execute
    r> handler ! rdrop rdrop rdrop 0 ;

: throw ( y1 .. ym error/0 -- y1 .. ym / z1 .. zn error ) \ exception
    ?DUP IF
	[ has-header [IF] here 9 cells ! [THEN] ] \ entry point for signal handler
	handler @ dup 0= IF
[ has-os [IF] ]
	    2 (bye)
[ [ELSE] ]
	    quit
[ [THEN] ]
	THEN
	rp!
	r> handler !
[ has-locals [IF] ]
        r> lp!
[ [THEN] ]
[ has-floats [IF] ]
	r> fp!
[ [THEN] ]
	r> swap >r sp! drop r>
	'throw
    THEN ;

\ Bouncing is very fine,
\ programming without wasting time...   jaw
: bounce ( y1 .. ym error/0 -- y1 .. ym error / y1 .. ym ) \ gforth
\ a throw without data or fp stack restauration
  ?DUP IF
      handler @ rp!
      r> handler !
[ has-locals [IF] ]
      r> lp!
[ [THEN] ]
[ has-floats [IF] ]
      rdrop
[ [THEN] ]
      rdrop
      'throw
  THEN ;

\ ?stack                                               23feb93py

: ?stack ( ?? -- ?? ) \ gforth
    sp@ s0 @ u> IF    -4 throw  THEN
[ has-floats [IF] ]
    fp@ f0 @ u> IF  -&45 throw  THEN
[ [THEN] ]
;
\ ?stack should be code -- it touches an empty stack!

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
: "lit ( -- addr )
  r> r> dup count + aligned >r swap >r ;
: (.")     "lit count type ;
: (S")     "lit count ;
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

: (abort")
    "lit >r
    IF
	r> "error ! -2 throw
    THEN
    rdrop ;
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

Create ???  0 , 3 c, char ? c, char ? c, char ? c,
: >name ( cfa -- nt ) \ gforth	to-name
 $21 cell do
   dup i - count $9F and + cfaligned over alias-mask + = if
     i - cell - unloop exit
   then
 cell +loop
 drop ??? ( wouldn't 0 be better? ) ;

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
: User Create uallot , DOES> @ up @ + ;
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

AVariable current ( -- addr ) \ gforth

: last?   ( -- false / nfa nfa )
    last @ ?dup ;
: (reveal) ( nt wid -- )
    ( wid>wordlist-id ) dup >r
    @ over ( name>link ) ! 
    r> ! ;

\ object oriented search list                          17mar93py

\ word list structure:

struct
  1 cells: field find-method   \ xt: ( c_addr u wid -- nt )
  1 cells: field reveal-method \ xt: ( nt wid -- ) \ used by dofield:, must be field
  1 cells: field rehash-method \ xt: ( wid -- )
\   \ !! what else
end-struct wordlist-map-struct

struct
  1 cells: field wordlist-id \ not the same as wid; representation depends on implementation
  1 cells: field wordlist-map \ pointer to a wordlist-map-struct
  1 cells: field wordlist-link \ link field to other wordlists
  1 cells: field wordlist-extend \ points to wordlist extensions (eg hash)
end-struct wordlist-struct

: f83find      ( addr len wordlist -- nt / false )
    ( wid>wordlist-id ) @ (f83find) ;

\ Search list table: find reveal
Create f83search ( -- wordlist-map )
    ' f83find A,  ' (reveal) A,  ' drop A,

Create forth-wordlist  NIL A, G f83search T A, NIL A, NIL A,
AVariable lookup       G forth-wordlist lookup T !
G forth-wordlist current T !

\ higher level parts of find

( struct )
0 >body cell
  1 cells: field interpret/compile-int
  1 cells: field interpret/compile-comp
end-struct interpret/compile-struct

: interpret/compile? ( xt -- flag )
    >does-code ['] S" >does-code = ;

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

\ Input                                                13feb93py

07 constant #bell ( -- c ) \ gforth
08 constant #bs ( -- c ) \ gforth
09 constant #tab ( -- c ) \ gforth
7F constant #del ( -- c ) \ gforth
0D constant #cr   ( -- c ) \ gforth
\ the newline key code
0C constant #ff ( -- c ) \ gforth
0A constant #lf ( -- c ) \ gforth

: bell ( -- ) \ gforth
    \g makes a beep and flushes the output buffer
    #bell emit
    outfile-id flush-file drop ;
: cr ( -- ) \ core
    \ emit a newline
    #lf ( sic! ) emit ;

\ : backspaces  0 ?DO  #bs emit  LOOP ;

: (ins) ( max span addr pos1 key -- max span addr pos2 )
    >r 2dup + r@ swap c! r> emit 1+ rot 1+ -rot ;
: (bs) ( max span addr pos1 -- max span addr pos2 flag )
    dup IF
	#bs emit bl emit #bs emit 1- rot 1- -rot
    THEN false ;
: (ret)  true bl emit ;

Create ctrlkeys
  ] false false false false  false false false false
    (bs)  false (ret) false  false (ret) false false
    false false false false  false false false false
    false false false false  false false false false [

defer insert-char
' (ins) IS insert-char
defer everychar
' noop IS everychar

: decode ( max span addr pos1 key -- max span addr pos2 flag )
  everychar
  dup #del = IF  drop #bs  THEN  \ del is rubout
  dup bl <   IF  cells ctrlkeys + perform  EXIT  THEN
  >r 2over = IF  rdrop bell 0 EXIT  THEN
  r> insert-char 0 ;

: accept   ( addr len -- len ) \ core
  dup 0< IF    abs over dup 1 chars - c@ tuck type 
\ this allows to edit given strings
  ELSE  0  THEN rot over
  BEGIN  key decode  UNTIL
  2drop nip ;

\ Output                                               13feb93py

has-os [IF]
0 Value outfile-id ( -- file-id ) \ gforth

: (type) ( c-addr u -- ) \ gforth
    outfile-id write-file drop \ !! use ?DUP-IF THROW ENDIF instead of DROP ?
;

: (emit) ( c -- ) \ gforth
    outfile-id emit-file drop \ !! use ?DUP-IF THROW ENDIF instead of DROP ?
;
[THEN]

Defer type ( c-addr u -- ) \ core
' (type) IS Type

Defer emit ( c -- ) \ core
' (Emit) IS Emit

Defer key ( -- c ) \ core
' (key) IS key

\ Query                                                07apr93py

has-files 0= [IF]
: sourceline# ( -- n )  loadline @ ;
[THEN]

: refill ( -- flag ) \ core-ext,block-ext,file-ext
  blk @  IF  1 blk +!  true  0 >in !  EXIT  THEN
  tib /line
[ has-files [IF] ]
  loadfile @ ?dup
  IF    read-line throw
  ELSE
[ [THEN] ]
      sourceline# 0< IF 2drop false EXIT THEN
      accept true
[ has-files [IF] ]
  THEN
[ [THEN] ]
  1 loadline +!
  swap #tib ! 0 >in ! ;

: query   ( -- ) \ core-ext
    \G obsolescent
    blk off loadfile off
    tib /line accept #tib ! 0 >in ! ;

\ save-mem extend-mem

has-os [IF]
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

\ HEX DECIMAL                                           2may93jaw

: decimal ( -- ) \ core
    a base ! ;
: hex ( -- ) \ core-ext
    10 base ! ;

\ DEPTH                                                 9may93jaw

: depth ( -- +n ) \ core
    sp@ s0 @ swap - cell / ;
: clearstack ( ... -- )
    s0 @ sp! ;

\ RECURSE                                               17may93jaw

: recurse ( compilation -- ; run-time ?? -- ?? ) \ core
    lastxt compile, ; immediate restrict
' reveal alias recursive ( -- ) \ gforth
	immediate

\ */MOD */                                              17may93jaw

\ !! I think */mod should have the same rounding behaviour as / - anton
: */mod ( n1 n2 n3 -- n4 n5 ) \ core	star-slash-mod
    >r m* r> sm/rem ;

: */ ( n1 n2 n3 -- n4 ) \ core	star-slash
    */mod nip ;

\ EVALUATE                                              17may93jaw

has-files 0= [IF]
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
  [ has-os [IF] ] stderr to outfile-id [ [THEN] ] 
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
  [ has-os [IF] ] stdout to outfile-id [ [THEN] ] 
;

' (DoError) IS DoError

: quit ( ?? -- ?? ) \ core
    r0 @ rp! handler off >tib @ >r
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
Defer 'cold ( -- ) \ gforth tick-cold
\ hook (deferred word) for things to do right before interpreting the
\ command-line arguments
' noop IS 'cold

: cold ( -- ) \ gforth
[ has-files [IF] ]
    pathstring 2@ process-path pathdirs 2!
    init-included-files
[ [THEN] ]
    'cold
[ has-files [IF] ]
    argc @ 1 >
    IF
	['] process-args catch ?dup
	IF
	    dup >r DoError cr r> negate (bye)
	THEN
	cr
    THEN
[ [THEN] ]
    ." GForth " version-string type ." , Copyright (C) 1994-1996 Free Software Foundation, Inc." cr
    ." GForth comes with ABSOLUTELY NO WARRANTY; for details type `license'"
[ has-os [IF] ]
     cr ." Type `bye' to exit"
[ [THEN] ]
    loadline off quit ;

: license ( -- ) \ gforth
 cr
 ." This program is free software; you can redistribute it and/or modify" cr
 ." it under the terms of the GNU General Public License as published by" cr
 ." the Free Software Foundation; either version 2 of the License, or" cr
 ." (at your option) any later version." cr cr

 ." This program is distributed in the hope that it will be useful," cr
 ." but WITHOUT ANY WARRANTY; without even the implied warranty of" cr
 ." MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the" cr
 ." GNU General Public License for more details." cr cr

 ." You should have received a copy of the GNU General Public License" cr
 ." along with this program; if not, write to the Free Software" cr
 ." Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA." cr ;

: boot ( path **argv argc -- )
    main-task up!
[ has-os [IF] ]
    stdout TO outfile-id
[ [THEN] ]
[ has-files [IF] ]
    argc ! argv ! pathstring 2!
[ [THEN] ]
    sp@ s0 !
[ has-locals [IF] ]
    lp@ forthstart 7 cells + @ - 
[ [ELSE] ]
    [ has-os [IF] ]
    sp@ $1040 +
    [ [ELSE] ]
    sp@ $40 +
    [ [THEN] ]
[ [THEN] ]
    dup >tib ! tibstack ! #tib off >in off
    rp@ r0 !
[ has-floats [IF] ]
    fp@ f0 !
[ [THEN] ]
    ['] cold catch DoError
[ has-os [IF] ]
    bye
[ [THEN] ]
;

has-os [IF]
: bye ( -- ) \ tools-ext
[ has-files [IF] ]
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


