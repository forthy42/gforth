\ compiler definitions						14sep97jaw

\ Copyright (C) 1995,1996,1997,1998 Free Software Foundation, Inc.

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

\ \ Revisions-Log

\	put in seperate file				14sep97jaw	

\ \ here allot , c, A,						17dec92py

: allot ( n -- ) \ core
    \G Reserve or release @var{n} address units of data space; @var{n}
    \G is a signed number. There are restrictions on releasing data
    \G space.
    dup unused u> -8 and throw
    dp +! ;

: c,    ( c -- ) \ core
    \G Reserve data space for one char and store @var{c} in the space.
    here 1 chars allot c! ;

: ,     ( w -- ) \ core
    \G Reserve data space for one cell and store @var{w} in the space.
    here cell allot  ! ;

: 2,	( w1 w2 -- ) \ gforth
    \G Reserve data space for two cells and store the double @var{w1
    \G w2} in the space.
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

: maxalign ( -- ) \ gforth
    here dup maxaligned swap
    ?DO
	bl c,
    LOOP ;

\ the code field is aligned if its body is maxaligned
' maxalign Alias cfalign ( -- ) \ gforth

' , alias A, ( addr -- ) \ gforth

' NOOP ALIAS const

\ \ Header							23feb93py

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

\ \ literals							17dec92py

: Literal  ( compilation n -- ; run-time -- n ) \ core
    \G Compile appropriate code such that, at run-time, @var{n} is placed
    \G on the stack. Interpretation semantics are undefined.
    postpone lit  , ; immediate restrict

: ALiteral ( compilation addr -- ; run-time -- addr ) \ gforth
    postpone lit A, ; immediate restrict

: char   ( '<spaces>ccc' -- c ) \ core
    \G Skip leading spaces. Parse the string @var{ccc} and return @var{c}, the
    \G display code representing the first character of @var{ccc}.
    bl word char+ c@ ;

: [char] ( compilation '<spaces>ccc' -- ; run-time -- c ) \ core bracket-char
    \G Compilation: skip leading spaces. Parse the string
    \G @var{ccc}. Run-time: return @var{c}, the display code
    \G representing the first character of @var{ccc}.  Interpretation
    \G semantics for this word are undefined.
    char postpone Literal ; immediate restrict

\ \ threading							17mar93py

: cfa,     ( code-address -- )  \ gforth	cfa-comma
    here
    dup lastcfa !
    0 A, 0 ,  code-address! ;

: compile, ( xt -- )	\ core-ext	compile-comma
    \G  Compile the word represented by the execution token, @var{xt}.
    A, ;

: !does    ( addr -- ) \ gforth	store-does
    lastxt does-code! ;

: (does>)  ( R: addr -- )
    r> cfaligned /does-handler + !does ;

: dodoes,  ( -- )
  cfalign here /does-handler allot does-handler! ;

: (compile) ( -- ) \ gforth
    r> dup cell+ >r @ compile, ;

: postpone, ( w xt -- ) \ gforth	postpone-comma
    \g Compile the compilation semantics represented by @var{w xt}.
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

struct
    >body
    cell% field interpret/compile-int
    cell% field interpret/compile-comp
end-struct interpret/compile-struct

: interpret/compile: ( interp-xt comp-xt "name" -- ) \ gforth
    Create immediate swap A, A,
DOES>
    abort" executed primary cfa of an interpret/compile: word" ;
\    state @ IF  cell+  THEN  perform ;

\ \ ticks

: name>comp ( nt -- w xt ) \ gforth
    \G @var{w xt} is the compilation token for the word @var{nt}.
    (name>comp)
    1 = if
        ['] execute
    else
        ['] compile,
    then ;

: [(')]  ( compilation "name" -- ; run-time -- nt ) \ gforth bracket-paren-tick
    (') postpone ALiteral ; immediate restrict

: [']  ( compilation. "name" -- ; run-time. -- xt ) \ core      bracket-tick
    \g @var{xt} represents @var{name}'s interpretation
    \g semantics. Performs @code{-14 throw} if the word has no
    \g interpretation semantics.
    ' postpone ALiteral ; immediate restrict

: COMP'    ( "name" -- w xt ) \ gforth  comp-tick
    \g @var{w xt} represents @var{name}'s compilation semantics.
    (') name>comp ;

: [COMP']  ( compilation "name" -- ; run-time -- w xt ) \ gforth bracket-comp-tick
    \g @var{w xt} represents @var{name}'s compilation semantics.
    COMP' swap POSTPONE Aliteral POSTPONE ALiteral ; immediate restrict

\ \ recurse							17may93jaw

: recurse ( compilation -- ; run-time ?? -- ?? ) \ core
    \g Call the current definition.
    lastxt compile, ; immediate restrict

\ \ compiler loop

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

: [ ( -- ) \ core	left-bracket
    \G Enter interpretation state. Immediate word.
    ['] interpreter  IS parser state off ; immediate

: ] ( -- ) \ core	right-bracket
    \G Enter compilation state.
    ['] compiler     IS parser state on  ;

\ \ Strings							22feb93py

: ," ( "string"<"> -- ) [char] " parse
  here over char+ allot  place align ;

: SLiteral ( Compilation c-addr1 u ; run-time -- c-addr2 u ) \ string
    \G Compilation: compile the string specified by @var{c-addr1},
    \G @var{u} into the current definition. Run-time: return
    \G @var{c-addr2 u} describing the address and length of the
    \G string.
    postpone (S") here over char+ allot  place align ;
                                             immediate restrict

\ \ abort"							22feb93py

: abort" ( compilation 'ccc"' -- ; run-time f -- ) \ core,exception-ext	abort-quote
    \G If any bit of @var{f} is non-zero, perform the function of @code{-2 throw},
    \G displaying the string @var{ccc} if there is no exception frame on the
    \G exception stack.
    postpone (abort") ," ;        immediate restrict

\ \ Header states						23feb93py

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
    \G Make the compilation semantics of a word be to @code{execute}
    \G the execution semantics.
    immediate-mask lastflags cset ;

: restrict ( -- ) \ gforth
    \G A synonym for @code{compile-only}
    restrict-mask lastflags cset ;
' restrict alias compile-only ( -- ) \ gforth
\G Remove the interpretation semantics of a word.

\ \ Create Variable User Constant                        	17mar93py

: Alias    ( cfa "name" -- ) \ gforth
    Header reveal
    alias-mask lastflags creset
    dup A, lastcfa ! ;

doer? :dovar [IF]

: Create ( "name" -- ) \ core
    Header reveal dovar: cfa, ;
[ELSE]

: Create ( "name" -- ) \ core
    Header reveal here lastcfa ! 0 A, 0 , DOES> ;
[THEN]

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

\ \ : ;                                                  	24feb93py

defer :-hook ( sys1 -- sys2 )

defer ;-hook ( sys2 -- sys1 )

: (:noname) ( -- colon-sys )
    \ common factor of : and :noname
    docol: cfa, defstart ] :-hook ;

: : ( "name" -- colon-sys ) \ core	colon
    Header (:noname) ;

: :noname ( -- xt colon-sys ) \ core-ext	colon-no-name
    0 last !
    cfalign here (:noname) ;

: ; ( compilation colon-sys -- ; run-time nest-sys ) \ core	semicolon
    ;-hook ?struc postpone exit reveal postpone [ ; immediate restrict

\ \ Search list handling: reveal words, recursive		23feb93py

: last?   ( -- false / nfa nfa )
    last @ ?dup ;

: (reveal) ( nt wid -- )
    wordlist-id dup >r
    @ over ( name>link ) ! 
    r> ! ;

\ make entry in wordlist-map
' (reveal) f83search reveal-method !

Variable warnings ( -- addr ) \ gforth
G -1 warnings T !

: check-shadow  ( addr count wid -- )
    \G prints a warning if the string is already present in the wordlist
    >r 2dup 2dup r> (search-wordlist) warnings @ and ?dup if
	>stderr
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
	else
	    drop
	then
    then ;

: rehash  ( wid -- )
    dup wordlist-map @ rehash-method perform ;

' reveal alias recursive ( compilation -- ; run-time -- ) \ gforth
\g Make the current definition visible, enabling it to call itself
\g recursively.
	immediate restrict
