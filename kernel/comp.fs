\ compiler definitions						14sep97jaw

\ Copyright (C) 1995,1996,1997,1998,2000,2003,2004,2005,2006,2007,2008,2009,2010,2011,2012,2013,2014,2015 Free Software Foundation, Inc.

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
    here 1 chars allot c! ;

: ,     ( w -- ) \ core comma
    \G Reserve data space for one cell and store @i{w} in the space.
    here cell allot ! ;

: 2,	( w1 w2 -- ) \ gforth
    \G Reserve data space for two cells and store the double @i{w1
    \G w2} there, @i{w2} first (lower address).
    here 2 cells allot 2! ;

\ : aligned ( addr -- addr' ) \ core
\     [ cell 1- ] Literal + [ -1 cells ] Literal and ;

: >align ( addr a-addr -- ) \ gforth
    \G add enough spaces to reach a-addr
    swap ?DO  bl c,  LOOP ;

: align ( -- ) \ core
    \G If the data-space pointer is not aligned, reserve enough space to align it.
    here dup aligned >align ;

\ : faligned ( addr -- f-addr ) \ float f-aligned
\     [ 1 floats 1- ] Literal + [ -1 floats ] Literal and ; 

: falign ( -- ) \ float f-align
    \G If the data-space pointer is not float-aligned, reserve
    \G enough space to align it.
    here dup faligned >align ;

: maxalign ( -- ) \ gforth
    \G Align data-space pointer for all alignment requirements.
    here dup maxaligned >align ;

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

: string, ( c-addr u -- ) \ gforth
    \G puts down string as cstring
    dup c, here swap chars dup allot move ;

: longstring, ( c-addr u -- ) \ gforth
    \G puts down string as longcstring
    dup , here swap chars dup allot move ;

: nlstring, ( c-addr u -- ) \ gforth
    \G puts down string as longcstring
    tuck here swap chars dup allot move , ;


[IFDEF] prelude-mask
variable next-prelude

: prelude, ( -- )
    next-prelude @ if
	align next-prelude @ ,
    then ;
[THEN]

: get-current  ( -- wid ) \ search
  \G @i{wid} is the identifier of the current compilation word list.
  current @ ;

Defer wlscope ' get-current is wlscope

: str>loadfilename# ( addr u -- n )
    included-files $@ bounds ?do ( addr u )
	2dup I $@ str= if
	    2drop I included-files $@ drop - cell/ unloop exit
	endif
    cell +loop
    2drop -1 ;

: encode-pos ( nline nchar -- npos )
    $ff min swap 8 lshift + ;

: current-sourcepos ( -- nfile npos )
    sourcefilename str>loadfilename# sourceline# >in @ encode-pos ;

: current-sourcepos1 ( -- xpos )
    current-sourcepos $7fffff min swap 23 lshift + ;

: view, ( -- ) current-sourcepos1 , ;

: header, ( c-addr u -- ) \ gforth
    name-too-long?  vt,
    wlscope >r
    dup max-name-length @ max max-name-length !
    [ [IFDEF] prelude-mask ] prelude, [ [THEN] ]
    dup aligned here + dup maxaligned >align
    view,
    dup cell+ here + dup maxaligned >align
    nlstring,
    r> 1 or A, 0 A, here last !  \ link field; before revealing, it contains the
    \ tagged reveal-into wordlist
    alias-mask lastflags cset
    next-prelude @ 0<> prelude-mask and lastflags cset
    next-prelude off
    cfalign ;

defer record-name ( -- )
' noop is record-name
\ record next name in tags file
defer (header)
defer header ( -- ) \ gforth
' (header) IS header

: input-stream-header ( "name" -- )
    parse-name name-too-short? header, ;

: input-stream ( -- )  \ general
    \G switches back to getting the name from the input stream ;
    ['] input-stream-header IS (header) ;

' input-stream-header IS (header)

2variable nextname-string

: nextname-header ( -- )
    nextname-string 2@ header,
    nextname-string free-mem-var
    input-stream ;

\ the next name is given in the string

: nextname ( c-addr u -- ) \ gforth
    \g The next defined word will have the name @var{c-addr u}; the
    \g defining word will leave the input stream alone.
    name-too-long?
    nextname-string free-mem-var
    save-mem nextname-string 2!
    ['] nextname-header IS (header) ;

: noname, ( -- )
    0 last ! vt,  here cell+ dup cfaligned >align alias-mask , 0 , 0 , ;
: noname-header ( -- )
    noname, input-stream ;

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
    postpone lit , ; immediate restrict

: 2Literal ( compilation w1 w2 -- ; run-time  -- w1 w2 ) \ double two-literal
    \G Compile appropriate code such that, at run-time, @i{w1 w2} are
    \G placed on the stack. Interpretation semantics are undefined.
    swap postpone Literal  postpone Literal ; immediate restrict

: ALiteral ( compilation addr -- ; run-time -- addr ) \ gforth
    postpone lit A, ; immediate restrict

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

' noop Alias recurse
\g Call the current definition.
unlock tlastcfa @ lock AConstant lastcfa
\ this is the alias pointer in the recurse header, named lastcfa.
\ changing lastcfa now changes where recurse aliases to
\ it's always an alias of the current definition
\ it won't work in a flash/rom environment, therefore for Gforth EC
\ we stick to the traditional implementation

: cfa,     ( code-address -- )  \ gforth	cfa-comma
    here
    dup lastcfa !
    0 A, 0 ,
    code-address! ;

defer basic-block-end ( -- )

:noname ( -- )
    0 compile-prim1 ;
is basic-block-end

has? primcentric [IF]
    has? peephole [IF]
	\ dynamic only    
	: peephole-compile, ( xt -- )
	    \ compile xt, appending its code to the current dynamic superinstruction
	    here swap , compile-prim1 ;
    [ELSE]
	: peephole-compile, ( xt -- addr ) @ , ;
    [THEN]
[ELSE]
' , is compile,
[THEN]

\ \ ticks

: default-name>comp ( nt -- w xt ) \ gforth name-to-comp
    \G @i{w xt} is the compilation token for the word @i{nt}.
    (name>x) (x>comp)
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
    parse-name do-recognizer '-error name>comp ;

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

include ./recognizer.fs

\ \ Strings							22feb93py

: S, ( addr u -- )
    \ allot string as counted string
    here over char+ allot  place align ;

: mem, ( addr u -- )
    \ allot the memory block HERE (do alignment yourself)
    here over allot swap move ;

: ," ( "string"<"> -- )
    [char] " parse s, ;

\ \ Header states						23feb93py

\ problematic only for big endian machines

: cset ( bmask c-addr -- )
    tuck @ or swap ! ; 

: creset ( bmask c-addr -- )
    tuck @ swap invert and swap ! ; 

: ctoggle ( bmask c-addr -- )
    tuck @ xor swap ! ; 

: lastflags ( -- c-addr )
    \ the address of the flags byte in the last header
    \ aborts if the last defined word was headerless
    latest dup 0= abort" last word was headerless"
    >f+c ;

: immediate ( -- ) \ core
    \G Make the compilation semantics of a word be to @code{execute}
    \G the execution semantics.
    immediate-mask lastflags cset ;

: restrict ( -- ) \ gforth
    \G A synonym for @code{compile-only}
    restrict-mask lastflags cset ;

' restrict alias compile-only ( -- ) \ gforth
\G Mark the last definition as compile-only; as a result, the text
\G interpreter and @code{'} will warn when they encounter such a word.

\ !!FIXME!! new flagless versions:
\ : immediate [: name>int ['] execute ;] set->comp ;
\ : compile-only [: drop ['] compile-only-error ;] set->int ;

\ \ Create Variable User Constant                        	17mar93py

\ : a>comp ( nt -- xt1 xt2 )  name>int ['] compile, ;

: s>int ( nt -- xt )  @ name>int ;
: s>comp ( nt -- xt1 xt2 )  @ name>comp ;
: s-to ( val nt -- )  @ (int-to) ;
comp: drop @ (comp-to) ;

: Alias    ( xt "name" -- ) \ gforth
    Header reveal ['] on vtcopy
    alias-mask lastflags creset
    dup A, lastcfa ! ;

: Synonym ( "name" "oldname" -- ) \ Forth200x
    Header  ['] on vtcopy
    parse-name find-name dup A,
    dup compile-only? IF  compile-only  THEN  name>int lastcfa !
    ['] s>int set->int ['] s>comp set->comp ['] s-to set-to reveal ;

: Create ( "name" -- ) \ core
    Header reveal dovar, ;

: buffer: ( u "name" -- ) \ core ext
    Create here over 0 fill allot ;

: Variable ( "name" -- ) \ core
    Create 0 , ;

: AVariable ( "name" -- ) \ gforth
    Create 0 A, ;

: 2Variable ( "name" -- ) \ double two-variable
    Create 0 , 0 , ;

: uallot ( n -- n' ) \ gforth
    udp @ swap udp +! ;

: User ( "name" -- ) \ gforth
    Header reveal douser, cell uallot , ;

: AUser ( "name" -- ) \ gforth
    User ;

: (Constant)  Header reveal docon, ;

: (Value)  Header reveal dovalue, ;

: Constant ( w "name" -- ) \ core
    \G Define a constant @i{name} with value @i{w}.
    \G  
    \G @i{name} execution: @i{-- w}
    (Constant) , ;

: AConstant ( addr "name" -- ) \ gforth
    (Constant) A, ;

: Value ( w "name" -- ) \ core-ext
    (Value) , ;

: AValue ( w "name" -- ) \ core-ext
    (Value) A, ;

: u-to ( n uvalue-xt -- ) >body @ next-task + ! ;
comp: ( uvalue-xt to-xt -- )
    drop >body @ postpone useraddr , postpone ! ;
\g u-to is the to-method for user values; it's xt is only
\g there to be consumed by @code{set-to}.
: u-compile, ( xt -- )  >body @ postpone useraddr , postpone @ ;

: UValue ( "name" -- )
    \G Define a per-thread value
    Create cell uallot , ['] u-to set-to
    ['] u-compile, set-compiler
  DOES> @ next-task + @ ;

: 2Constant ( w1 w2 "name" -- ) \ double two-constant
    Create ( w1 w2 "name" -- )
        2,
    DOES> ( -- w1 w2 )
        2@ ;

: (Field)  Header reveal dofield, ;

\ IS Defer What's Defers TO                            24feb93py

defer defer-default ( -- )
' abort is defer-default
\ default action for deferred words (overridden by a warning later)
    
: Defer ( "name" -- ) \ gforth
\G Define a deferred word @i{name}; its execution semantics can be
\G set with @code{defer!} or @code{is} (and they have to, before first
\G executing @i{name}.
    Header Reveal dodefer,
    ['] defer-default A, ;

: >body@ >body @ ;

: Defers ( compilation "name" -- ; run-time ... -- ... ) \ gforth
    \G Compiles the present contents of the deferred word @i{name}
    \G into the current definition.  I.e., this produces static
    \G binding as if @i{name} was not deferred.
    ' defer@ compile, ; immediate

\ No longer used for DOES>; integrate does>-like with ;abi-code, and
\ eliminate the extra stuff?

: does>-like ( xt -- defstart )
    \ xt ( addr -- ) is !does or !;abi-code etc, addr is the address
    \ that should be stored right after the code address.
    >r ;-hook ?struc
    exit-like
    here [ has? peephole [IF] ] 5 [ [ELSE] ] 4 [ [THEN] ] cells +
    postpone aliteral r> compile, [exit]
    [ has? peephole [IF] ] finish-code [ [THEN] ]
    defstart ;

extra>-dummy (doextra-dummy)
: !extra   ( addr -- ) \ gforth store-extra
    vttemplate >vtcompile, @ ['] udp >namevt @ >vtcompile, @ =
    IF
	['] extra, set-compiler
    THEN
    latestxt extra-code! ;

\ call with locals

docolloc-dummy (docolloc-dummy)

\ comp: to define compile, action

Create vttemplate
0 A,                   \ link field
' peephole-compile, A, \ compile, field
' noop A,              \ post, field
0 A,                   \ extra field
' no-to A,             \ to field
' default-name>int A,  \ name>int field
' default-name>comp A, \ name>comp field
' >body@ A,            \ defer@

\ initialize to one known vt

: (make-latest) ( xt1 xt2 -- )
    swap >namevt @ vttemplate vtsize move
    >namevt vttemplate over ! vttemplate ! ;
: vtcopy ( xt -- ) \ gforth vtcopy
    here (make-latest) ;

: vtcopy,     ( xt -- )  \ gforth	vtcopy-comma
    dup vtcopy here >r dup >code-address cfa, cell+ @ r> cell+ ! ;

: vtsave ( -- addr u ) \ gforth
    \g save vttemplate for nested definitions
    vttemplate vtsize save-mem  vttemplate off ;

: vtrestore ( addr u -- ) \ gforth
    \g restore vttemplate
    over >r vttemplate swap move r> free throw ;

: vt= ( vt1 vt2 -- flag )
    cell+ swap vtsize cell /string tuck compare 0= ;

: (vt,) ( -- )
    align  here vtsize allot vttemplate over vtsize move
    vtable-list @ over !  dup vtable-list !
    vttemplate @ !  vttemplate off ;

: vt, ( -- )
    vttemplate @ 0= IF EXIT THEN
    vtable-list
    BEGIN  @ dup  WHILE
	    dup vttemplate vt= IF  vttemplate @ !  vttemplate off  EXIT  THEN
    REPEAT  drop (vt,) ;

: make-latest ( xt -- )
    vt, dup last ! dup lastcfa ! dup (make-latest) ;

: !namevt ( addr -- )  latestxt >namevt ! ;

: start-xt ( -- colonsys xt ) \ incomplete, will not be a full xt
    here >r docol: cfa, colon-sys ] :-hook r> ;
: start-xt-like ( colonsys xt -- colonsys )
    reveal does>-like drop start-xt drop ;

: set-optimizer ( xt -- ) vttemplate >vtcompile, ! ;
' set-optimizer alias set-compiler
: set-lit,      ( xt -- ) vttemplate >vtlit, ! ;
: set-to        ( xt -- ) vttemplate >vtto ! ;
: set-defer@    ( xt -- ) vttemplate >vtdefer@ ! ;
: set->int      ( xt -- ) vttemplate >vt>int ! ;
: set->comp     ( xt -- ) vttemplate >vt>comp ! ;
: set-does>     ( xt -- ) !doesxt ; \ more work than the aboves

:noname ( -- colon-sys ) start-xt  set-compiler ;
:noname ['] set-compiler start-xt-like ;
over over
interpret/compile: opt:
interpret/compile: comp:
( compilation colon-sys1 -- colon-sys2 ; run-time nest-sys -- ) \ gforth

:noname ( -- colon-sys )
    start-xt  set-lit, ;
:noname ['] set-lit, start-xt-like ;
interpret/compile: lit,:
( compilation colon-sys1 -- colon-sys2 ; run-time nest-sys -- ) \ gforth

\ defer and friends

: defer! ( xt xt-deferred -- ) \ gforth  defer-store
\G Changes the @code{defer}red word @var{xt-deferred} to execute @var{xt}.
    >body ! ;

: (comp-to) ( xt -- )
    \g TO uses the TO-xt for interpretation and compilation.
    \g Interpretation is straight-forward execute with ( value xt -- )
    \g on the stack, so a normal >BODY ! (with the appropriate !) does
    \g the TO action.  Compilation uses the compile,-Method of this
    \g xt, i.e. that method will see ( value-xt to-xt -- ) as stack
    \g effect.
    dup >namevt @ >vtto @ compile, ;

: value! ( n value-xt -- ) \ gforth  value-store
    \g this is the TO-method for normal values; it's tickable, but
    \g the only purpose of its xt is to be consumed by @code{set-to}.
    >body ! ;
comp: ( value-xt to-xt -- )
    drop >body postpone ALiteral postpone ! ;
    
: <IS> ( "name" xt -- ) \ gforth
    \g Changes the @code{defer}red word @var{name} to execute @var{xt}.
    record-name (') (name>x) drop (int-to) ;

: [IS] ( compilation "name" -- ; run-time xt -- ) \ gforth bracket-is
    \g At run-time, changes the @code{defer}red word @var{name} to
    \g execute @var{xt}.
    record-name (') (name>x) drop (comp-to) ; immediate restrict

' <IS> ' [IS] interpret/compile: TO ( value "name" -- )
' <IS> ' [IS] interpret/compile: IS ( value "name" -- )

\ \ : ;                                                  	24feb93py

defer :-hook ( sys1 -- sys2 )
defer free-old-local-names ( -- )
defer ;-hook ( sys2 -- sys1 )
defer 0-adjust-locals-size ( -- )
1 value colon-sys-xt-offset
\ you get get the xt in a colon-sys with COLON-SYS-XT-OFFSET PICK

0 Constant defstart
: colon-sys ( -- colon-sys )
    \ a colon-sys consists of an xt for an action to be executed at
    \ the end of the definition, possibly some data consumed by the xt
    \ below that, and a DEFSTART tag on top; the stack effect of xt is
    \ ( ... -- ), where the ... is the additional data in the
    \ colon-sys.  The :-hook may add more stuff (which is then removed
    \ by ;-hook before this stuff here is processed).
    ['] noop defstart ;

: (noname->comp) ( nt -- nt xt )  ['] compile, ;
: (:noname) ( -- colon-sys )
    \ common factor of : and :noname
    docol, colon-sys ] :-hook unlocal-state off ;

: : ( "name" -- colon-sys ) \ core	colon
    free-old-local-names
    Header (:noname) ;

: :noname ( -- xt colon-sys ) \ core-ext	colon-no-name
    noname, here (:noname)
    ['] noop set->int  ['] (noname->comp) set->comp ;

: ; ( compilation colon-sys -- ; run-time nest-sys ) \ core	semicolon
    ;-hook [exit] ?colon-sys
    [ has? peephole [IF] ] finish-code [ [THEN] ]
    reveal postpone [ ; immediate restrict

: concat ( xt1 xt2 -- xt )
    \ concat two xts into one
    >r >r :noname r> compile, r> compile, postpone ; ;

: recognizer: ( int-xt comp-xt post-xt "name" -- )
    \G create a new recognizer table
    >r  ['] drop swap concat >r
    >r :noname r> compile, postpone ;
    r> set-compiler r> set-lit,  Constant ;

\ does>

: doesxt, ( xt -- )
    dup >body postpone literal  cell+ @ compile, ;

: !doesxt ( xt -- ) \ gforth store-doesxt
    latestxt doesxt-code!
    ['] doesxt, set-compiler ;

: !does    ( addr -- ) \ gforth	store-does
    vttemplate >vtcompile, @ ['] udp >namevt @ >vtcompile, @ =
    IF
	['] spaces >namevt @ >vtcompile, @ set-compiler
    THEN
    latestxt does-code! ;

: comp-does>; ( some-sys flag lastxt -- )
    \ used as colon-sys xt; this is executed after ";" has removed the
    \ colon-sys produced by [:
    nip (;]) postpone set-does> postpone ; ;

: comp-does> ( compilation colon-sys1 -- colon-sys2 )
    state @ >r
    comp-[:
    r> 0= if postpone [ then \ don't change state
    ['] comp-does>; colon-sys-xt-offset stick \ replace noop with comp-does>;
; immediate

: int-does>; ( flag lastxt -- )
    nip >r vt, wrap! r> set-does> ;

: int-does> ( -- colon-sys )
    int-[:
    ['] int-does>; colon-sys-xt-offset stick \ replace noop with :does>;
;

' int-does> ' comp-does> interpret/compile: does> ( compilation colon-sys1 -- colon-sys2 )

\ new interpret/compile:

: i/c>int ( nt -- xt )  @ ;
: i/c>comp ( nt -- xt1 xt2 ) cell+ @ ['] execute ;

\ : interpret/compile? ( xt -- flag ) >namevt @ >vt>int @ ['] i/c>int = ;

: interpret/compile: ( interp-xt comp-xt "name" -- ) \ gforth
    Header reveal
    ['] on vtcopy \ vtable template from normal colon definition
    ['] i/c>int  set->int   \ special name>interpret method
    ['] i/c>comp set->comp  \ special name>compile method
    swap , , ;

\ \ Search list handling: reveal words, recursive		23feb93py

: last?   ( -- false / nfa nfa )
    latest ?dup ;

Variable warnings ( -- addr ) \ gforth
G -1 warnings T !

: (reveal) ( nt wid -- )
    wordlist-id dup >r
    @ over >link ! 
    r> ! ;

\ make entry in wordlist-map
' (reveal) f83search reveal-method !

: check-shadow  ( addr count wid -- )
    \G prints a warning if the string is already present in the wordlist
    >r 2dup r> find-name-in warnings @ and ?dup if
	<<#
	name>string 2over 2over str= 0=
	IF  2over holds s"  with " holds  THEN
	holds s" redefined " holds
	0. #> hold 1- c(warning") #>>
    then
    2drop ;

: reveal ( -- ) \ gforth
    last?
    if \ the last word has a header
	dup >link @ 1 and
	if \ it is still hidden
	    dup >link @ 1 xor		( nt wid )
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
