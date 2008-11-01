\ compiler definitions						14sep97jaw

\ Copyright (C) 1995,1996,1997,1998,2000,2003,2004,2005,2006,2007,2008 Free Software Foundation, Inc.

\ This file is part of Gforth.

\ Gforth is free software; you can redistribute it and/or
\ modify it under the terms of the GNU General Public License
\ as published by the Free Software Foundation, either version 3
\ of the License, or (at your option) any later version.

\ This program is distributed in the hope that it will be useful,
\ but WITHOUT ANY WARRANTY; without even the implied warranty of
\ MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
\ GNU General Public License for more details.

\ You should have received a copy of the GNU General Public License
\ along with this program. If not, see http://www.gnu.org/licenses/.

\ \ Revisions-Log

\	put in seperate file				14sep97jaw	

\ \ here allot , c, A,						17dec92py

[IFUNDEF] allot
[IFUNDEF] forthstart
: allot ( n -- ) \ core
    dup unused u> -8 and throw
    dp +! ;
[THEN]
[THEN]

\ we default to this version if we have nothing else 05May99jaw
[IFUNDEF] allot
: allot ( n -- ) \ core
    \G Reserve @i{n} address units of data space without
    \G initialization. @i{n} is a signed number, passing a negative
    \G @i{n} releases memory.  In ANS Forth you can only deallocate
    \G memory from the current contiguous region in this way.  In
    \G Gforth you can deallocate anything in this way but named words.
    \G The system does not check this restriction.
    here +
    dup 1- usable-dictionary-end forthstart within -8 and throw
    dp ! ;
[THEN]

: c,    ( c -- ) \ core c-comma
    \G Reserve data space for one char and store @i{c} in the space.
    here 1 chars allot [ has? flash [IF] ] flashc! [ [ELSE] ] c! [ [THEN] ] ;

: ,     ( w -- ) \ core comma
    \G Reserve data space for one cell and store @i{w} in the space.
    here cell allot [ has? flash [IF] ] flash! [ [ELSE] ] ! [ [THEN] ] ;

: 2,	( w1 w2 -- ) \ gforth
    \G Reserve data space for two cells and store the double @i{w1
    \G w2} there, @i{w2} first (lower address).
    here 2 cells allot  [ has? flash [IF] ] tuck flash! cell+ flash!
	[ [ELSE] ] 2! [ [THEN] ] ;

\ : aligned ( addr -- addr' ) \ core
\     [ cell 1- ] Literal + [ -1 cells ] Literal and ;

: align ( -- ) \ core
    \G If the data-space pointer is not aligned, reserve enough space to align it.
    here dup aligned swap ?DO  bl c,  LOOP ;

\ : faligned ( addr -- f-addr ) \ float f-aligned
\     [ 1 floats 1- ] Literal + [ -1 floats ] Literal and ; 

: falign ( -- ) \ float f-align
    \G If the data-space pointer is not float-aligned, reserve
    \G enough space to align it.
    here dup faligned swap
    ?DO
	bl c,
    LOOP ;

: maxalign ( -- ) \ gforth
    \G Align data-space pointer for all alignment requirements.
    here dup maxaligned swap
    ?DO
	bl c,
    LOOP ;

\ the code field is aligned if its body is maxaligned
' maxalign Alias cfalign ( -- ) \ gforth
\G Align data-space pointer for code field requirements (i.e., such
\G that the corresponding body is maxaligned).

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
    dup [ has? rom [IF] ] $E0 [ [ELSE] ] alias-mask [ [THEN] ] or c,
[ has? flash [IF] ]
    bounds ?DO  I c@ c,  LOOP
[ [ELSE] ]
    here swap chars dup allot move
[ [THEN] ] ;

: longstring, ( c-addr u -- ) \ gforth
    \G puts down string as longcstring
    dup , here swap chars dup allot move ;

: header, ( c-addr u -- ) \ gforth
    name-too-long?
    dup max-name-length @ max max-name-length !
    align here last !
[ has? ec [IF] ]
    -1 A,
[ [ELSE] ]
    current @ 1 or A,	\ link field; before revealing, it contains the
			\ tagged reveal-into wordlist
[ [THEN] ]
[ has? f83headerstring [IF] ]
	string,
[ [ELSE] ]
	longstring, alias-mask lastflags cset
[ [THEN] ]
    cfalign ;

: input-stream-header ( "name" -- )
    parse-name name-too-short? header, ;

: input-stream ( -- )  \ general
    \G switches back to getting the name from the input stream ;
    ['] input-stream-header IS (header) ;

' input-stream-header IS (header)

2variable nextname-string

has? OS [IF]
: nextname-header ( -- )
    nextname-string 2@ header,
    nextname-string free-mem-var
    input-stream ;
[THEN]

\ the next name is given in the string

has? OS [IF]
: nextname ( c-addr u -- ) \ gforth
    \g The next defined word will have the name @var{c-addr u}; the
    \g defining word will leave the input stream alone.
    name-too-long?
    nextname-string free-mem-var
    save-mem nextname-string 2!
    ['] nextname-header IS (header) ;
[THEN]

: noname-header ( -- )
    0 last ! cfalign
    input-stream ;

: noname ( -- ) \ gforth
    \g The next defined word will be anonymous. The defining word will
    \g leave the input stream alone. The xt of the defined word will
    \g be given by @code{latestxt}.
    ['] noname-header IS (header) ;

: latestxt ( -- xt ) \ gforth
    \G @i{xt} is the execution token of the last word defined.
    \ The main purpose of this word is to get the xt of words defined using noname
    lastcfa @ ;

' latestxt alias lastxt \ gforth-obsolete
\G old name for @code{latestxt}.

: latest ( -- nt ) \ gforth
\G @var{nt} is the name token of the last word defined; it is 0 if the
\G last word has no name.
    last @ ;

\ \ literals							17dec92py

: Literal  ( compilation n -- ; run-time -- n ) \ core
    \G Compilation semantics: compile the run-time semantics.@*
    \G Run-time Semantics: push @i{n}.@*
    \G Interpretation semantics: undefined.
[ [IFDEF] lit, ]
    lit,
[ [ELSE] ]
    postpone lit ,
[ [THEN] ] ; immediate restrict

: 2Literal ( compilation w1 w2 -- ; run-time  -- w1 w2 ) \ double two-literal
    \G Compile appropriate code such that, at run-time, @i{w1 w2} are
    \G placed on the stack. Interpretation semantics are undefined.
    swap postpone Literal  postpone Literal ; immediate restrict

: ALiteral ( compilation addr -- ; run-time -- addr ) \ gforth
[ [IFDEF] alit, ]
    alit,
[ [ELSE] ]
    postpone lit A, 
[ [THEN] ] ; immediate restrict

Defer char@ ( addr u -- char addr' u' )
:noname  over c@ -rot 1 /string ; IS char@

: char   ( '<spaces>ccc' -- c ) \ core
    \G Skip leading spaces. Parse the string @i{ccc} and return @i{c}, the
    \G display code representing the first character of @i{ccc}.
    parse-name char@ 2drop ;

: [char] ( compilation '<spaces>ccc' -- ; run-time -- c ) \ core bracket-char
    \G Compilation: skip leading spaces. Parse the string
    \G @i{ccc}. Run-time: return @i{c}, the display code
    \G representing the first character of @i{ccc}.  Interpretation
    \G semantics for this word are undefined.
    char postpone Literal ; immediate restrict

\ \ threading							17mar93py

: cfa,     ( code-address -- )  \ gforth	cfa-comma
    here
    dup lastcfa !
    [ has? rom [IF] ] 2 cells allot [ [ELSE] ] 0 A, 0 , [ [THEN] ]
    code-address! ;

[IFUNDEF] compile,
defer compile, ( xt -- )	\ core-ext	compile-comma
\G  Compile the word represented by the execution token @i{xt}
\G  into the current definition.

' , is compile,
[THEN]

has? ec 0= [IF]
defer basic-block-end ( -- )

:noname ( -- )
    0 compile-prim1 ;
is basic-block-end
[THEN]

has? peephole [IF]

\ dynamic only    
: peephole-compile, ( xt -- )
    \ compile xt, appending its code to the current dynamic superinstruction
    here swap , compile-prim1 ;
    
: compile-to-prims, ( xt -- )
    \G compile xt to use primitives (and their peephole optimization)
    \G instead of ","-ing the xt.
    \ !! all POSTPONEs here postpone primitives; this can be optimized
    dup >does-code if
	['] does-exec peephole-compile, , EXIT
	\ dup >body POSTPONE literal ['] call peephole-compile, >does-code , EXIT
    then
    dup >code-address CASE
	dovalue: OF >body ['] lit@ peephole-compile, , EXIT ENDOF
	docon:   OF >body @ ['] lit peephole-compile, , EXIT ENDOF
	\ docon:   OF >body POSTPONE literal ['] @ peephole-compile, EXIT ENDOF
	\ docon is also used by VALUEs, so don't @ at compile time
	docol:   OF >body ['] call peephole-compile, , EXIT ENDOF
	dovar:   OF >body ['] lit peephole-compile, , EXIT ENDOF
	douser:  OF >body @ ['] useraddr peephole-compile, , EXIT ENDOF
	dodefer: OF >body ['] lit-perform peephole-compile, , EXIT ENDOF
	dofield: OF >body @ ['] lit+ peephole-compile, , EXIT ENDOF
	\ dofield: OF >body @ POSTPONE literal ['] + peephole-compile, EXIT ENDOF
	\ code words and ;code-defined words (code words could be optimized):
	dup in-dictionary? IF drop POSTPONE literal ['] execute peephole-compile, EXIT THEN
    ENDCASE
    peephole-compile, ;

' compile-to-prims, IS compile,
[ELSE]
' , is compile,
[THEN]

: !does    ( addr -- ) \ gforth	store-does
    latestxt does-code! ;

\ !! unused, but ifdefed/gosted in some places
: (does>)  ( R: addr -- )
    r> cfaligned /does-handler + !does ; \ !! no gforth-native

: (does>2)  ( addr -- )
    cfaligned /does-handler + !does ;

: dodoes,  ( -- )
  cfalign here /does-handler allot does-handler! ;

: (compile) ( -- ) \ gforth-obsolete: dummy
    true abort" (compile) doesn't work, use POSTPONE instead" ;

\ \ ticks

: name>comp ( nt -- w xt ) \ gforth name-to-comp
    \G @i{w xt} is the compilation token for the word @i{nt}.
    (name>comp)
    1 = if
        ['] execute
    else
        ['] compile,
    then ;

: [(')]  ( compilation "name" -- ; run-time -- nt ) \ gforth bracket-paren-tick
    (') postpone ALiteral ; immediate restrict

: [']  ( compilation. "name" -- ; run-time. -- xt ) \ core      bracket-tick
    \g @i{xt} represents @i{name}'s interpretation
    \g semantics. Perform @code{-14 throw} if the word has no
    \g interpretation semantics.
    ' postpone ALiteral ; immediate restrict

: COMP'    ( "name" -- w xt ) \ gforth  comp-tick
    \g Compilation token @i{w xt} represents @i{name}'s compilation semantics.
    (') name>comp ;

: [COMP']  ( compilation "name" -- ; run-time -- w xt ) \ gforth bracket-comp-tick
    \g Compilation token @i{w xt} represents @i{name}'s compilation semantics.
    COMP' swap POSTPONE Aliteral POSTPONE ALiteral ; immediate restrict

: postpone, ( w xt -- ) \ gforth	postpone-comma
    \g Compile the compilation semantics represented by the
    \g compilation token @i{w xt}.
    dup ['] execute =
    if
	drop compile,
    else
	swap POSTPONE aliteral compile,
    then ;

: POSTPONE ( "name" -- ) \ core
    \g Compiles the compilation semantics of @i{name}.
    COMP' postpone, ; immediate

\ \ recurse							17may93jaw

: recurse ( compilation -- ; run-time ?? -- ?? ) \ core
    \g Call the current definition.
    latestxt compile, ; immediate restrict

\ \ compiler loop

: compiler1 ( c-addr u -- ... xt )
    2dup find-name dup
    if ( c-addr u nt )
	nip nip name>comp
    else
	drop
	2dup 2>r snumber? dup
	IF
	    0>
	    IF
		['] 2literal
	    ELSE
		['] literal
	    THEN
	    2rdrop
	ELSE
	    drop 2r> compiler-notfound1
	THEN
    then ;

: [ ( -- ) \  core	left-bracket
    \G Enter interpretation state. Immediate word.
    ['] interpreter1  IS parser1 state off ; immediate

: ] ( -- ) \ core	right-bracket
    \G Enter compilation state.
    ['] compiler1     IS parser1 state on  ;

\ \ Strings							22feb93py

: S, ( addr u -- )
    \ allot string as counted string
[ has? flash [IF] ]
    dup c, bounds ?DO  I c@ c,  LOOP
[ [ELSE] ]
    here over char+ allot  place align
[ [THEN] ] ;

: mem, ( addr u -- )
    \ allot the memory block HERE (do alignment yourself)
[ has? flash [IF] ]
    bounds ?DO  I c@ c,  LOOP
[ [ELSE] ]
    here over allot swap move
[ [THEN] ] ;

: ," ( "string"<"> -- )
    [char] " parse s, ;

\ \ Header states						23feb93py

\ problematic only for big endian machines

has? f83headerstring [IF]
: cset ( bmask c-addr -- )
    tuck c@ or swap c! ; 

: creset ( bmask c-addr -- )
    tuck c@ swap invert and swap c! ; 

: ctoggle ( bmask c-addr -- )
    tuck c@ xor swap c! ; 
[ELSE]
: cset ( bmask c-addr -- )
    tuck @ or swap ! ; 

: creset ( bmask c-addr -- )
    tuck @ swap invert and swap ! ; 

: ctoggle ( bmask c-addr -- )
    tuck @ xor swap ! ; 
[THEN]

: lastflags ( -- c-addr )
    \ the address of the flags byte in the last header
    \ aborts if the last defined word was headerless
    latest dup 0= abort" last word was headerless" cell+ ;

: immediate ( -- ) \ core
    \G Make the compilation semantics of a word be to @code{execute}
    \G the execution semantics.
    immediate-mask lastflags [ has? rom [IF] ] creset [ [ELSE] ] cset [ [THEN] ] ;

: restrict ( -- ) \ gforth
    \G A synonym for @code{compile-only}
    restrict-mask lastflags [ has? rom [IF] ] creset [ [ELSE] ] cset [ [THEN] ] ;

' restrict alias compile-only ( -- ) \ gforth
\G Remove the interpretation semantics of a word.

\ \ Create Variable User Constant                        	17mar93py

: Alias    ( xt "name" -- ) \ gforth
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

has? flash [IF]
    : (variable) dpp @ normal-dp = IF  Create dpp @
	ELSE  normal-dp @ Constant dpp @ ram  THEN ;
: Variable ( "name" -- ) \ core
    (Variable) 0 , dpp ! ;

: AVariable ( "name" -- ) \ gforth
    (Variable) 0 A, dpp ! ;

: 2Variable ( "name" -- ) \ double two-variable
    (Variable) 0 , 0 , dpp ! ;
[ELSE]
: Variable ( "name" -- ) \ core
    Create 0 , ;

: AVariable ( "name" -- ) \ gforth
    Create 0 A, ;

: 2Variable ( "name" -- ) \ double two-variable
    Create 0 , 0 , ;
[THEN]

has? no-userspace 0= [IF]
: uallot ( n -- ) \ gforth
    udp @ swap udp +! ;

doer? :douser [IF]

: User ( "name" -- ) \ gforth
    Header reveal douser: cfa, cell uallot , ;

: AUser ( "name" -- ) \ gforth
    User ;
[ELSE]

: User Create cell uallot , DOES> @ up @ + ;

: AUser User ;
[THEN]
[THEN]

doer? :docon [IF]
    : (Constant)  Header reveal docon: cfa, ;
[ELSE]
    : (Constant)  Create DOES> @ ;
[THEN]

doer? :dovalue [IF]
    : (Value)  Header reveal dovalue: cfa, ;
[ELSE]
    has? rom [IF]
	: (Value)  Create DOES> @ @ ;
    [ELSE]
	: (Value)  Create DOES> @ ;
    [THEN]
[THEN]

: Constant ( w "name" -- ) \ core
    \G Define a constant @i{name} with value @i{w}.
    \G  
    \G @i{name} execution: @i{-- w}
    (Constant) , ;

: AConstant ( addr "name" -- ) \ gforth
    (Constant) A, ;

has? flash [IF]
: Value ( w "name" -- ) \ core-ext
    (Value) dpp @ >r here cell allot >r
    ram here >r , r> r> flash! r> dpp ! ;

' Value alias AValue
[ELSE]
: Value ( w "name" -- ) \ core-ext
    (Value) , ;

: AValue ( w "name" -- ) \ core-ext
    (Value) A, ;
[THEN]

: 2Constant ( w1 w2 "name" -- ) \ double two-constant
    Create ( w1 w2 "name" -- )
        2,
    DOES> ( -- w1 w2 )
        2@ ;
    
doer? :dofield [IF]
    : (Field)  Header reveal dofield: cfa, ;
[ELSE]
    : (Field)  Create DOES> @ + ;
[THEN]

\ \ interpret/compile:

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

\ IS Defer What's Defers TO                            24feb93py

defer defer-default ( -- )
' abort is defer-default
\ default action for deferred words (overridden by a warning later)
    
doer? :dodefer [IF]

: Defer ( "name" -- ) \ gforth
\G Define a deferred word @i{name}; its execution semantics can be
\G set with @code{defer!} or @code{is} (and they have to, before first
\G executing @i{name}.
    Header Reveal dodefer: cfa,
    [ has? rom [IF] ] here >r cell allot
    dpp @ ram here r> flash! ['] defer-default A, dpp !
    [ [ELSE] ] ['] defer-default A, [ [THEN] ] ;

[ELSE]

    has? rom [IF]
	: Defer ( "name" -- ) \ gforth
	    Create here >r cell allot
	    dpp @ ram here r> flash! ['] defer-default A, dpp !
	  DOES> @ @ execute ;
    [ELSE]
	: Defer ( "name" -- ) \ gforth
	    Create ['] defer-default A,
	  DOES> @ execute ;
    [THEN]
[THEN]

: defer@ ( xt-deferred -- xt ) \ gforth defer-fetch
\G @i{xt} represents the word currently associated with the deferred
\G word @i{xt-deferred}.
    >body @ [ has? rom [IF] ] @ [ [THEN] ] ;

: Defers ( compilation "name" -- ; run-time ... -- ... ) \ gforth
    \G Compiles the present contents of the deferred word @i{name}
    \G into the current definition.  I.e., this produces static
    \G binding as if @i{name} was not deferred.
    ' defer@ compile, ; immediate

:noname
    dodoes, here !does ]
    defstart :-hook ;
:noname
    ;-hook ?struc
    [ has? xconds [IF] ] exit-like [ [THEN] ]
    here [ has? peephole [IF] ] 5 [ [ELSE] ] 4 [ [THEN] ] cells +
    postpone aliteral postpone (does>2) [compile] exit
    [ has? peephole [IF] ] finish-code [ [THEN] ] dodoes,
    defstart :-hook ;
interpret/compile: DOES>  ( compilation colon-sys1 -- colon-sys2 ; run-time nest-sys -- ) \ core        does

: defer! ( xt xt-deferred -- ) \ gforth  defer-store
\G Changes the @code{defer}red word @var{xt-deferred} to execute @var{xt}.
    >body [ has? rom [IF] ] @ [ [THEN] ] ! ;
    
: <IS> ( "name" xt -- ) \ gforth
    \g Changes the @code{defer}red word @var{name} to execute @var{xt}.
    ' defer! ;

: [IS] ( compilation "name" -- ; run-time xt -- ) \ gforth bracket-is
    \g At run-time, changes the @code{defer}red word @var{name} to
    \g execute @var{xt}.
    ' postpone ALiteral postpone defer! ; immediate restrict

' <IS>
' [IS]
interpret/compile: IS ( compilation/interpretation "name-deferred" -- ; run-time xt -- ) \ gforth
\G Changes the @code{defer}red word @var{name} to execute @var{xt}.
\G Its compilation semantics parses at compile time.

' <IS>
' [IS]
interpret/compile: TO ( w "name" -- ) \ core-ext

: interpret/compile? ( xt -- flag )
    >does-code ['] DOES> >does-code = ;

\ \ : ;                                                  	24feb93py

defer :-hook ( sys1 -- sys2 )

defer ;-hook ( sys2 -- sys1 )

0 Constant defstart

[IFDEF] docol,
: (:noname) ( -- colon-sys )
    \ common factor of : and :noname
    docol, ]comp
[ELSE]
: (:noname) ( -- colon-sys )
    \ common factor of : and :noname
    docol: cfa,
[THEN]
    defstart ] :-hook ;

: : ( "name" -- colon-sys ) \ core	colon
    Header (:noname) ;

: :noname ( -- xt colon-sys ) \ core-ext	colon-no-name
    0 last !
    cfalign here (:noname) ;

[IFDEF] fini,
: ; ( compilation colon-sys -- ; run-time nest-sys ) \ core   semicolon
    ;-hook ?struc fini, comp[ reveal postpone [ ; immediate restrict
[ELSE]
: ; ( compilation colon-sys -- ; run-time nest-sys ) \ core	semicolon
    ;-hook ?struc [compile] exit
    [ has? peephole [IF] ] finish-code [ [THEN] ]
    reveal postpone [ ; immediate restrict
[THEN]

\ \ Search list handling: reveal words, recursive		23feb93py

: last?   ( -- false / nfa nfa )
    latest ?dup ;

Variable warnings ( -- addr ) \ gforth
G -1 warnings T !

has? ec [IF]
: reveal ( -- ) \ gforth
    last?
    if \ the last word has a header
	dup ( name>link ) @ -1 =
	if \ it is still hidden
	    forth-wordlist dup >r @ over
	    [ has? flash [IF] ] flash! [ [ELSE] ] ! [  [THEN] ] r> !
	else
	    drop
	then
    then ;
[ELSE]
: (reveal) ( nt wid -- )
    wordlist-id dup >r
    @ over ( name>link ) ! 
    r> ! ;

\ make entry in wordlist-map
' (reveal) f83search reveal-method !

: check-shadow  ( addr count wid -- )
    \G prints a warning if the string is already present in the wordlist
    >r 2dup 2dup r> (search-wordlist) warnings @ and ?dup if
	>stderr
	." redefined " name>string 2dup type
	str= 0= if
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
[THEN]

' reveal alias recursive ( compilation -- ; run-time -- ) \ gforth
\g Make the current definition visible, enabling it to call itself
\g recursively.
	immediate restrict
