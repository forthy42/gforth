\ compiler definitions						14sep97jaw

\ Authors: Bernd Paysan, Anton Ertl, Neal Crook, Jens Wilke, David Kühling, Gerald Wodni
\ Copyright (C) 1995,1996,1997,1998,2000,2003,2004,2005,2006,2007,2008,2009,2010,2011,2012,2013,2014,2015,2016,2017,2018,2019,2020,2021,2022 Free Software Foundation, Inc.

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
    ->here ;
[THEN]

: small-allot ( n -- addr )
    dp @ tuck + dp ! ;

: c,    ( c -- ) \ core c-comma
    \G Reserve data space for one char and store @i{c} in the space.
    1 chars small-allot c! ;

: 2,	( w1 w2 -- ) \ gforth
    \G Reserve data space for two cells and store the double @i{w1
    \G w2} there, @i{w2} first (lower address).
    2 cells small-allot 2! ;

\ : aligned ( addr -- addr' ) \ core
\     [ cell 1- ] Literal + [ -1 cells ] Literal and ;

: >align ( addr a-addr -- ) \ gforth-internal
    \G add enough spaces to reach a-addr
    swap ?DO  bl c,  LOOP ;

: align ( -- ) \ core
    \G If the data-space pointer is not aligned, reserve enough space to align it.
    here dup aligned >align ;

\ : faligned ( addr -- f-addr ) \ float f-aligned
\     [ 1 floats 1- ] Literal + [ -1 floats ] Literal and ; 

: falign ( -- ) \ floating f-align
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
\g Reserve data space for one cell, and store @i{addr} there.  For our
\g cross-compiler this provides the type information necessary for a
\g relocatable image; normally, though, this is equivalent to
\g @code{,}.

' NOOP ALIAS const

\ \ Header							23feb93py

\ input-stream, nextname and noname are quite ugly (passing
\ information through global variables), but they are useful for dealing
\ with existing/independent defining words

: mem, ( addr u -- ) \ gforth
    \ allot the memory block HERE (do alignment yourself)
    here over allot swap move ;

: string, ( c-addr u -- ) \ gforth
    \G puts down string as cstring
    dup c, mem, ;

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

: encode-pos ( nline nchar -- npos )
    $ff min swap 8 lshift + ;

: current-sourcepos3 ( -- nfile nline nchar )
    loadfilename# @ sourceline# input-lexeme 2@ drop source drop - ;

: encode-view ( nfile nline nchar -- xpos )
    encode-pos $7fffff min swap 23 lshift or ;

0 Value replace-sourceview \ used by #loc to modify view,

: current-sourceview ( -- xpos )
    current-sourcepos3 encode-view ;

: current-view ( -- xpos )
    replace-sourceview current-sourceview over select ;

Defer check-shadow ( addr u wid -- )
:noname drop 2drop ; is check-shadow

' noop Alias recurse ( ... -- ... ) \ core
\g Alias to the current definition.

unlock tlastcfa @ lock >body AConstant lastnt
\ this is the alias pointer in the recurse header, named lastnt.
\ changing lastnt now changes where recurse aliases to
\ it's always an alias of the current definition
\ it won't work in a flash/rom environment, therefore for Gforth EC
\ we stick to the traditional implementation

: name, ( c-addr u -- ) \ gforth-internal
    \G compile the named part of a header
    name-too-long?
    dup here + dup cfaligned >align
    tuck mem, ,
    get-current 1 or A,
    here xt-location drop
    \ link field; before revealing, it contains the
    \ tagged reveal-into wordlist
    [ has? new-cfa [IF] ] 0 A, [ [THEN] ]
    here cell+ dup last ! lastnt ! ; \ set last header
: 0name, ( -- )
    cfalign 0 last ! 0 lastnt !
    here xt-location drop
    [ has? new-cfa [IF] ] 0 A, [ [THEN] ] ;
: namehm, ( namehm -- )
    , here lastnt ! ; \ add location stamps on hm+cf

: noname-hm ( -- )
    \G modify hm for noname words
    default-i/c
    ['] noname>string set-name>string
    ['] noname>link set-name>link ;
: named-hm ( -- )
    \G modify hm for named words
    default-i/c
    ['] named>string set-name>string
    ['] named>link set-name>link ;
: ?noname-hm ( -- ) last @ 0= IF  noname-hm  ELSE  named-hm  THEN ;

: noname, ( -- ) \ gforth-internal
    \G create an empty header for an unnamed word
    hm, 0name, cell negate allot  hmtemplate namehm, noname-hm ;

defer record-name ( -- )
' noop is record-name
\ record next name in tags file
defer header-name,
defer header-extra ' noop is header-extra
: header ( -- ) \ gforth-internal
    \G create a header for a word
    hm, header-name, hmtemplate namehm, ?noname-hm header-extra ;

: create-from ( nt "name" -- ) \ gforth
    \G Create a word @i{name} that behaves like @i{nt}, but with an
    \G empty body.  @i{nt} must be the nt of a named word.  The
    \G resulting header is not yet revealed.  Creating a word with
    \G @code{create-from} without using any @code{set-} words is
    \G faster than if you create a word using @code{set-} words,
    \G @code{immediate}, or @code{does>}.  You can use @code{noname}
    \G with @code{create-from}.
    hm, header-name,
    [ has? new-cfa [IF] ] >cfa 2@ swap [ [ELSE] ] >namehm 2@ [ [THEN] ]
    , cfa, last @ 0= IF  here hmcopy noname-hm  THEN
    header-extra ;

: noname-from ( xt -- ) \ gforth
    \G Create a nameless word that behaves like @i{xt}, but with an
    \G empty body.  @i{xt} must be the nt of a nameless word.
    hm, 0name,
    [ has? new-cfa [IF] ] >cfa 2@ swap [ [ELSE] ] >namehm 2@ [ [THEN] ]
    , cfa, ;

: input-stream-header ( "name" -- )
    parse-name name-too-short? name, ;

: input-stream ( -- )  \ gforth-internal
    \G switches back to getting the name from the input stream ;
    ['] input-stream-header IS header-name, ;

' input-stream-header IS header-name,

variable nextname$

: nextname-header ( -- )
    nextname$ $@ name, nextname$ $free  input-stream ;

\ the next name is given in the string

: nextname ( c-addr u -- ) \ gforth
    \g The next defined word will have the name @var{c-addr u}; the
    \g defining word will leave the input stream alone.
    name-too-long? nextname$ $!
    ['] nextname-header IS header-name, ;

: noname-header ( -- )
    noname, input-stream ;

: noname ( -- ) \ gforth
    \g The next defined word will be anonymous. The defining word will
    \g leave the input stream alone. The xt of the defined word will
    \g be given by @code{latestxt}.
    ['] noname-header IS header-name, ;

: latestnt ( -- nt ) \ gforth
    \G @i{nt} is the name token of the last word defined.
    \ The main purpose of this word is to get the nt of words defined using noname
    lastnt @ ;
: latestxt ( -- xt ) \ gforth
    \G @i{xt} is the execution token of the last word defined.
    \ The main purpose of this word is to get the xt of words defined using noname
    lastnt @ name>interpret ;

: latest ( -- nt ) \ gforth
\G @var{nt} is the name token of the last word defined; it is 0 if the
\G last word has no name.
    last @ ;

\ \ literals							17dec92py

: Literal  ( compilation n -- ; run-time -- n ) \ core
    \G Compilation semantics: compile the run-time semantics.@*
    \G Run-time Semantics: push @i{n}.@*
    \G Interpretation semantics: undefined.
    >lits ; \ threading-method 1 = IF  postpone lit ,  ELSE  >lits  THEN ;
immediate restrict

: 2Literal ( compilation w1 w2 -- ; run-time  -- w1 w2 ) \ double two-literal
    \G Compile appropriate code such that, at run-time, @i{w1 w2} are
    \G placed on the stack. Interpretation semantics are undefined.
    swap postpone Literal  postpone Literal ; immediate restrict

: ALiteral ( compilation addr -- ; run-time -- addr ) \ gforth
    \g Works like @code{literal}, but (when used in cross-compiled
    \g code) tells the cross-compiler that the literal is an address.
    postpone Literal ; immediate restrict

: ?parse-name ( -- addr u )
    \G same as parse-name, but fails with an error
    parse-name dup 0= #-16 and throw ;

\ \ threading							17mar93py

Variable litstack

: >lits ( x -- ) litstack >stack ;
: lits> ( -- x ) litstack stack> ;
: lits# ( -- u ) litstack stack# ;
: lits, ( -- )
    litstack $@ bounds  litstack @ >r  litstack off
    ?DO  postpone lit  I @ ,  cell +LOOP
    r> free throw ;
: clear-litstack ( -- )
    0 litstack set-stack ;

has? new-cfa [IF]
    : cfa,     ( code-address -- )  \ gforth-internal	cfa-comma
	here  dup lastnt !
	only-code-address! ;
[ELSE]
    : cfa,     ( code-address -- )  \ gforth-internal	cfa-comma
	here
	dup lastnt !
	0 A,
	only-code-address! ;
[THEN]

defer basic-block-end ( -- )

:noname ( -- )
    lits, 0 compile-prim1 ;
is basic-block-end

\ record locations

40 value bt-pos-width

Defer xt-location
: xt-location1 ( addr -- addr )
\ note that an xt was compiled at addr, for backtrace-locate functionality
    dup section-start @ - cell/ >r
    current-view dup r> 1+ locs[] $[] cell- 2!
    0 to replace-sourceview ;
' xt-location1 is xt-location

Defer addr>view
:noname ( ip-addr -- view / 0 )
    \G give @i{view} information for instruction address @i{ip-addr}
    dup cell- section-start @ section-dp @ within
    section-start @ and ?dup-IF
	- cell/ 1- locs[] $[] @  EXIT
    THEN  drop 0 ; is addr>view
' addr>view alias name>view ( nt -- view / 0 )
\G give @i{view} information for name token @i{nt}

has? primcentric [IF]
    has? peephole [IF]
	defer prim-check ( xt -- xt ) ' noop is prim-check
	\ hook for stack depth (and maybe later type) checker
	
	: peephole-compile, ( xt -- )
	    \ compile xt, appending its code to the current dynamic superinstruction
	    lits, prim-check here swap , xt-location compile-prim1 ;
    [ELSE]
	: peephole-compile, ( xt -- addr )
	    lits, here xt-location drop , ;
    [THEN]
[ELSE]
    ' xt, is compile,
[THEN]

\ \ ticks

' compile, AConstant default-name>comp ( nt -- w xt ) \ gforth-internal default-name-to-comp

: default-i/c ( -- )
    ['] noop set->int
    ['] default-name>comp set->comp ;

: [']  ( compilation. "name" -- ; run-time. -- xt ) \ core      bracket-tick
    \g @i{xt} represents @i{name}'s interpretation
    \g semantics. Perform @code{-14 throw} if the word has no
    \g interpretation semantics.
    ' postpone Literal ; immediate restrict

: COMP'    ( "name" -- w xt ) \ gforth  comp-tick
    \g Compilation token @i{w xt} represents @i{name}'s compilation semantics.
    ?parse-name forth-recognize '-error name>compile ;

: [COMP']  ( compilation "name" -- ; run-time -- w xt ) \ gforth bracket-comp-tick
    \g Compilation token @i{w xt} represents @i{name}'s compilation semantics.
    COMP' swap POSTPONE literal POSTPONE Literal ; immediate restrict

: postpone, ( w xt -- ) \ gforth	postpone-comma
    \g Compile the compilation semantics represented by the
    \g compilation token @i{w xt}.
    dup ['] execute =
    if
	drop compile,
    else
	swap POSTPONE literal compile,
    then ;

include ./recognizer.fs

\ \ Strings							22feb93py

: ," ( "string"<"> -- )
    '"' parse string, ;

\ \ Header states						23feb93py

\ problematic only for big endian machines

: or! ( bmask addr -- )
    tuck @ or swap ! ; 

\ cset creset ctoggle: incompatible change between 0.5 and 0.6,
\   removed in 2020 for name cooldown

: lastflags ( -- c-addr )
    \ the address of the flags byte in the last header
    \ aborts if the last defined word was headerless
    latest dup 0= abort" last word was nameless"
    >f+c ;

: imm>comp  name>interpret ['] execute ;
: immediate ( -- ) \ core
    \G Make the compilation semantics of a word be to @code{execute}
    \G the execution semantics.
    ['] imm>comp set->comp ;

: restrict ( -- ) \ gforth
    \G A synonym for @code{compile-only}
    restrict-mask lastflags or! ;

' restrict alias compile-only ( -- ) \ gforth
\G Mark the last definition as compile-only; as a result, the text
\G interpreter and @code{'} will warn when they encounter such a word.

: obsolete ( -- ) \ gforth
    \G Mark the last word as obsolete
    obsolete-mask lastflags or! ;

\ !!FIXME!! new flagless versions:
\ : compile-only [: drop ['] compile-only-error ;] set->int ;

\ \ Create Variable User Constant                        	17mar93py

: defer@, ( xt -- )
    dup lit, >namehm @ >hmdefer@ @ opt!-compile, ;

: a>int ( nt -- )  >body @ ;
: a>comp ( nt -- xt1 xt2 )  name>interpret ['] compile, ;

: s>int ( nt -- xt )  >body @ name>interpret ;
: s>comp ( nt -- xt1 xt2 )  >body @ name>compile ;
: s-to ( val nt -- )
    >body @ (to) ;
opt: ( xt -- ) ?fold-to >body @ (to), ;
: s-defer@ ( xt1 -- xt2 )
    >body @ defer@ ;
opt: ( xt -- ) ?fold-to >body @ defer@, ;
: s-compile, ( xt -- )  >body @ compile, ;

: synonym, ( nt int comp -- ) \ gforth-internal
    set->comp set->int
    ['] s-to       set-to
    ['] s-defer@   set-defer@
    ['] s-compile, set-optimizer
    A, ;

: Alias    ( xt "name" -- ) \ gforth
    \G Define @i{name} as a word that performs @i{xt}.  Unlike for
    \G deferred words, aliases don't have an indirection overhead when
    \G compiled.
    ['] parser create-from ['] a>int ['] a>comp synonym, reveal ;

: alias? ( nt -- flag )
    >namehm @ >hm>int 2@ ['] a>comp ['] a>int d= ;

$BF000000 Value synonym-mask \ do not copy obsolete flag
$BF -1 cells allot  bigendian [IF]   c, 0 1 cells 1- c,s
                          [ELSE] 0 1 cells 1- c,s c, [THEN]

: Synonym ( "name" "oldname" -- ) \ tools-ext
    \G Define @i{name} to behave the same way as @i{oldname}: Same
    \G interpretation semantics, same compilation semantics, same
    \G @code{to}/@code{defer!} and @code{defer@@} semantics.
    ['] parser create-from
    ?parse-name find-name dup 0= #-13 and throw
    dup >f+c @ synonym-mask and latest >f+c +!
    ['] s>int ['] s>comp synonym, reveal ;

: synonym? ( nt -- flag )
    >namehm @ >hm>int 2@ ['] s>comp ['] s>int d= ;

: Create ( "name" -- ) \ core
    ['] udp create-from reveal ;

: buffer: ( u "name" -- ) \ core-ext buffer-colon
    \g Define @i{name} and reserve @i{u} bytes starting at @i{addr}.
    \g @i{name} run-time: @code{( -- addr )}.  Gforth initializes the
    \g reserved bytes to 0, but the standard does not guarantee this.
    Create here over 0 fill allot ;

: Variable ( "name" -- ) \ core
    \g Define @i{name} and reserve a cell starting at @i{addr}.
    \g @i{name} run-time: @code{( -- addr )}.
    Create 0 , ;

: AVariable ( "name" -- ) \ gforth
    \g Works like @code{variable}, but (when used in cross-compiled
    \g code) tells the cross-compiler that the cell stored in the
    \g variable is an address.
    Create 0 A, ;

: 2Variable ( "name" -- ) \ double two-variable
    Create 0 , 0 , ;

: uallot ( n -- n' ) \ gforth
    \g Reserve @i{n} bytes in every user space.
    udp @ swap udp +! ;

: User ( "name" -- ) \ gforth
    ['] sp0 create-from reveal cell uallot , ;

: AUser ( "name" -- ) \ gforth
    \g Define a user variable for containing an addres (this only
    \g makes a difference in the cross-compiler).
    User ;

: (Constant) ['] bl create-from reveal ;

: (Value)    ['] def#tib create-from reveal ;

: Constant ( w "name" -- ) \ core
    \G Define a constant @i{name} with value @i{w}.
    \G  
    \G @i{name} execution: @i{-- w}
    (Constant) , ;

: AConstant ( addr "name" -- ) \ gforth
    \G Like @code{constant}, but defines a constant for an address
    \G (this only makes a difference in the cross-compiler).
    (Constant) A, ;

: Value ( w "name" -- ) \ core-ext
    \g Define @i{name} with the initial value @i{w}; this value can be
    \g changed with @code{to @i{name}} or @code{->@i{name}}.
    \g  
    \g @i{name} execution: @i{-- w2}
    (Value) , ;

: AValue ( w "name" -- ) \ gforth
    \G Like @code{value}, but defines a value for an address
    \G (this only makes a difference in the cross-compiler).
    (Value) A, ;

: -/- ( -- ) \ gforth-experimental not-available
    \G this word can be ticked, but throws an exception on interpretation
    \G and compilation
    #-21 throw ;
' execute set-optimizer

Create !-table ' ! A, ' +! A, ' -/- A, ' -/- A,
Create defer-table ' ! A, ' -/- A, ' -/- A, ' @ A,
Variable to-style# 0 to-style# !

4 Constant to-table-size#

: to-!, ( table -- )
    0 to-style# !@ dup to-table-size# u< IF  cells + @ compile,  ELSE  2drop  THEN ;
: to-!exec ( table -- )
    0 to-style# !@ dup to-table-size# u< IF  cells + perform  ELSE  2drop  THEN ;

: (Field)  ['] wordlist-map create-from reveal ;

\ IS Defer What's Defers TO                            24feb93py

defer defer-default ( -- )
' abort is defer-default
\ default action for deferred words (overridden by a warning later)

: Defer ( "name" -- ) \ core-ext
\G Define a deferred word @i{name}; its execution semantics can be
\G set with @code{defer!} or @code{is} (and they have to, before first
\G executing @i{name}.
    ['] parser create-from reveal
    ['] defer-default A, ;

: defer-defer@ ( xt -- )
    \ The defer@ implementation of children of DEFER
    >body @ ;
opt: ( xt -- )
    ?fold-to >body lit, postpone @ ;

: Defers ( compilation "name" -- ; run-time ... -- ... ) \ gforth
    \G Compiles the present contents of the deferred word @i{name}
    \G into the current definition.  I.e., this produces static
    \G binding as if @i{name} was not deferred.
    ' defer@ compile, ; immediate

\ No longer used for DOES>; integrate does>-like with ;abi-code, and
\ eliminate the extra stuff?

: does>-like ( xt -- colon-sys )
    \ xt ( addr -- ) is !does or !;abi-code etc, addr is the address
    \ that should be stored right after the code address.
    >r ;-hook
    exit-like
    here [ has? peephole [IF] ] 5 [ [ELSE] ] 4 [ [THEN] ] cells +
    postpone literal r> compile, [compile] exit
    ?colon-sys [ has? peephole [IF] ] finish-code [ [THEN] ]
    colon-sys ;

\ call with locals - unused

\ docolloc-dummy (docolloc-dummy)

\ opt: to define compile, action

Create hmtemplate
0 A,                   \ link field
' peephole-compile, A, \ compile, field
' no-to A,             \ to field
' no-defer@ A,         \ defer@
0 A,                   \ extra field
' noop A,  \ name>interpret field
' default-name>comp A, \ name>comp field
' named>string A,      \ name>string field
' named>link A,        \ name>link field

\ initialize to one known hm

: hm-activate ( xt -- )
    >namehm hmtemplate over ! hmtemplate ! ;
: hmcopy ( xt -- ) \ gforth-internal
    >namehm @ hmtemplate 0 >hm>int move
    here hm-activate ;

: hmcopy,     ( xt -- )  \ gforth-experimental hmcopy-comma
    \g While constructing a header, allocate the code field, and use
    \g @i{xt} as prototype for setting the code field and the header
    \g methods.
    dup hmcopy here >r dup >code-address cfa, cell+ @ r> cell+ ! ;

: hmsave ( -- addr u ) \ gforth-internal
    \g save hmtemplate for nested definitions
    hmtemplate hmsize save-mem  hmtemplate off ;

: hmrestore ( addr u -- ) \ gforth-internal
    \g restore hmtemplate
    over >r hmtemplate swap move r> free throw ;

: hm= ( hm1 hm2 -- flag )
    cell+ swap hmsize cell /string tuck compare 0= ;

: (hm,) ( -- )
    align  here hmsize allot hmtemplate over hmsize move
    hm-list @ over !  dup hm-list !
    hmtemplate @ !  hmtemplate off ;

: hm, ( -- )
    hmtemplate @ 0= IF EXIT THEN
    hm-list
    BEGIN  @ dup  WHILE
	    dup hmtemplate hm= IF  hmtemplate @ !  hmtemplate off  EXIT  THEN
    REPEAT  drop (hm,) ;

: make-latest ( nt -- ) \ gforth
    \G Make @i{nt} the latest definition, which can be manipulated by
    \G @code{immediate} and @code{set-*} operations.  If you have used
    \G (especially compiled) the word referred to by nt already, do
    \G not change the behaviour of the word (only its implementation),
    \G otherwise you may get a surprising mix of behaviours that is
    \G not consistent between Gforth engines and versions.
    hm, dup lastnt !
    dup name>string nip 0<> and last !  ;

: ?hm ( -- )
    \ check if deduplicated, duplicate if necessary
    lastnt @ >namehm @ hmtemplate <> IF
	lastnt @
	dup >namehm @ hmtemplate hmsize move
	hm-activate
    THEN ;

: !namehm ( addr -- )  latestnt >namehm ! ;

: general-compile, ( xt -- )
    \ a (slow) implementation of @code{compile,} that works for every word
    postpone literal postpone execute ;

: set-optimizer ( xt -- ) \ gforth
    \G Changes the current word such that @code{compile,}ing it
    \G executes @i{xt} (with the same stack contents as passed to
    \G @code{compile,}).  Note that @code{compile,} must be consistent
    \G with @code{execute}, so you must use @code{set-optimizer} only
    \G to install a more efficient implementation of the same
    \G behaviour.
    ?hm hmtemplate >hmcompile, ! ;

: set-execute ( ca -- ) \ gforth
    \G Changes the current word such that it jumps to the native code
    \G at @i{ca}.  Also changes the @code{compile,} implementation to
    \G the most general (and slowest) one.  Call @code{set-optimizer}
    \G afterwards if you want a more efficient @code{compile,}
    \G implementation.
    ['] general-compile, set-optimizer
    latestnt only-code-address! ;

: does-code! ( xt2 xt1 -- ) \ gforth
    \G Change @i{xt1} to be a @code{@i{xt2} set-does>}-defined word.
    dodoes: any-code!
    ['] does, set-optimizer ;

: set-does> ( xt -- ) \ gforth
    \G Changes the current word such that it pushes its body address
    \G and then executes @i{xt}.  Also changes the @code{compile,}
    \G implementation accordingly.  Call @code{set-optimizer}
    \G afterwards if you want a more efficient implementation.
    ['] does, set-optimizer
    hmtemplate >hmextra !
    dodoes: latestnt only-code-address! ;
: set-to ( to-xt -- ) \ gforth
    \G Sets the implementation of the @code{(to) ( val xt -- )} method
    \G of the current word to @i{to-xt}.
    ?hm hmtemplate >hmto ! ;
: set-defer@ ( defer@-xt -- ) \ gforth set-defer-fetch
    \G Sets the implementation of the @code{defer@@ ( xt-deferred -- xt )}
    \G method of the current word to @i{defer@@-xt}.
    ?hm hmtemplate >hmdefer@ ! ;
: set->int ( xt -- ) \ gforth set-to-int
    \G Sets the implementation of the @code{name>interpret ( nt -- xt2 )}
    \G method of the current word to @i{xt}.
    ?hm hmtemplate >hm>int ! ;
: set->comp ( xt -- ) \ gforth set-to-comp
    \G Sets the implementation of the @code{name>compile ( nt -- w xt2 )}
    \G method of the current word to @i{xt}.
    ?hm hmtemplate >hm>comp ! ;
: set-name>string ( xt -- ) \ gforth set-name-to-string
    \G Sets the implementation of the @code{name>string ( nt -- addr u )}
    \G method of the current word to @i{xt}.
    ?hm hmtemplate >hm>string ! ;
: set-name>link ( xt -- ) \ gforth set-name-to-link
    \G Sets the implementation of the @code{name>link ( nt1 -- nt2|0 )}
    \G method of the current word to @i{xt}.
 ?hm hmtemplate >hm>link ! ;

: int-opt; ( flag lastxt -- )
    nip >r hm, wrap! r> set-optimizer ;

: opt: ( compilation -- colon-sys2 ; run-time -- nest-sys ) \ gforth
    \G Starts a nameless colon definition; when it is complete, this
    \G colon definition will become the @code{compile,} implementation
    \G of the latest word (before the @code{opt:}).
    int-[:
    ['] int-opt; colon-sys-xt-offset stick ;

: opt!-compile, ( xt -- ) \ gforth-internal
    \G force optimizing compile,
    ['] compile, @ >r \ we optimize defer@ here directly
    ['] opt-compile, is compile,
    ['] compile, catch
    r> is compile,  throw ;

: (to), ( xt -- ) ( generated code: v -- )
    \g in compiled @code{to @i{name}}, xt is that of @i{name}.  This
    \g word generates code for storing v (of type appropriate for
    \g @i{name}) there.  This word is a factor of @code{to}.
    dup >lits >namehm @ >hmto @ opt!-compile,
    \ OPT: part of the SET-TO part of the defining word of <name>.
    \ This here needs to be optimizing even for gforth-itc, because
    \ otherwise this code won't work: for locals, the xt is no longer
    \ valid at run-time, so we have to optimize it away at compile
    \ time; this is achived by explicitly calling >LITS and
    \ OPT!-COMPILE,.
;

: ?fold-to ( <to>-xt -- name-xt )
    \G Prepare partial constant folding for @code{(to)} methods: if
    \G there's no literal on the folding stack, just compile the
    \G @code{(to)} method as is.  If there is, drop the xt of the
    \G \code{(to)} method, and retrieve the @i{name-xt} of the word TO
    \G is applied to from the folding stack.
    lits# 0= IF :, rdrop EXIT THEN drop lits> ;
: to-opt: ( -- colon-sys ) \ gforth-internal
    \G Defines a part of the TO <name> run-time semantics used with compiled
    \G @code{TO}.  The stack effect of the code following @code{to-opt:} must
    \G be: @code{( xt -- ) ( generated: v -- )}.  The generated code stores
    \G @i{v} in the storage represented by @i{xt}.
    opt: postpone ?fold-to ;

\ defer and friends

' to-opt: alias defer@-opt: ( -- colon-sys ) \ gforth-internal
\g Optimized code generation for compiled @code{action-of @i{name}}.
\g The stack effect of the following code must be ( xt -- ), where xt
\g represents @i{name}; this word generates code with stack effect (
\g -- xt1 ), where xt1 is the result of xt @code{defer@}.

' (to) Alias defer! ( xt xt-deferred -- ) \ core-ext  defer-store
\G Changes the @code{defer}red word @var{xt-deferred} to execute @var{xt}.

' (to) Alias reveal! ( xt wid -- )
' >hmto Alias reveal-method ( wid -- addr )

: value-to ( n value-xt -- ) \ gforth-internal
    \g this is the TO-method for normal values
    >body !-table to-!exec ;
opt: ( value-xt -- ) \ run-time: ( n -- )
    drop postpone >body !-table to-!, ;

: defer-is ( n value-xt -- ) \ gforth-internal
    \g this is the TO-method for deferred words
    >body defer-table to-!exec ;
opt: ( value-xt -- ) \ run-time: ( n -- )
    drop postpone >body defer-table to-!, ;

: <IS> ( "name" xt -- ) \ gforth-internal angle-is
    \g Changes the @code{defer}red word @var{name} to execute @var{xt}.
    record-name (') (to) ;

: [IS] ( compilation "name" -- ; run-time xt -- ) \ gforth-internal bracket-is
    \g At run-time, changes the @code{defer}red word @var{name} to
    \g execute @var{xt}.
    record-name (') (to), ; immediate restrict

' <IS> ' [IS] interpret/compile: TO ( value "name" -- ) \ core-ext
\g changes the value of @var{name} to @var{value}
' <IS> ' [IS] interpret/compile: IS ( value "name" -- ) \ core-ext
\g changes the @code{defer}red word @var{name} to execute @var{value}

: <+TO>  1 to-style# ! <IS> ;
: [+TO]  1 to-style# ! postpone [IS] ; immediate restrict

' <+TO> ' [+TO] interpret/compile: +TO ( value "name" -- ) \ gforth
\g increments the value of @var{name} by @var{value}

\ \ : ;                                                  	24feb93py

defer :-hook ( sys1 -- sys2 ) ' clear-litstack is :-hook
defer ;-hook ( sys2 -- sys1 )
defer 0-adjust-locals-size ( -- )

1 value colon-sys-xt-offset
\g you get the xt in a colon-sys with COLON-SYS-XT-OFFSET PICK

Create defstart
: colon-sys ( -- colon-sys )
    \ a colon-sys consists of an xt for an action to be executed at
    \ the end of the definition, possibly some data consumed by the xt
    \ below that, and a DEFSTART tag on top; the stack effect of xt is
    \ ( ... -- ), where the ... is the additional data in the
    \ colon-sys.  The :-hook may add more stuff (which is then removed
    \ by ;-hook before this stuff here is processed).
    ['] noop defstart ;

: : ( "name" -- colon-sys ) \ core	colon
    ['] on create-from colon-sys ] :-hook ;

:noname ; aconstant dummy-noname
: :noname ( -- xt colon-sys ) \ core-ext	colon-no-name
    dummy-noname noname-from
    latestnt colon-sys ] :-hook ;

: ; ( compilation colon-sys -- ; run-time nest-sys ) \ core	semicolon
    ;-hook [compile] exit ?colon-sys
    [ has? peephole [IF] ] finish-code [ [THEN] ]
    reveal postpone [ ; immediate restrict

: concat ( xt1 xt2 -- xt )
    \ concat two xts into one
    >r >r :noname r> compile, r> compile, postpone ; ;

\ does>

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
    nip >r hm, wrap! r> set-does> ;

: int-does> ( -- colon-sys )
    int-[:
    ['] int-does>; colon-sys-xt-offset stick \ replace noop with :does>;
;

' int-does> ' comp-does>
interpret/compile: does> ( compilation colon-sys1 -- colon-sys2 ) \ core does

\ for cross-compiler's interpret/compile:

: i/c>comp ( nt -- xt1 xt2 )
    >body cell+ @ ['] execute ;

\ \ Search list handling: reveal words, recursive		23feb93py

: (nocheck-reveal) ( nt wid -- )
    wordlist-id dup >r
    @ over >link ! 
    r> ! ;
: (reveal) ( nt wid -- )
    over name>string third check-shadow
    (nocheck-reveal) ;

: reveal ( -- ) \ gforth
    \G Put the current word in the wordlist current at the time of the
    \G header definition.
    latest ?dup-if \ the last word has a header
	dup >link @ 1 and
	if \ it is still hidden
	    dup >link @ 1 xor		( nt wid )
	    reveal!
	else
	    drop
	then
    then ;

Create voc-table ' (reveal) A, ' -/- A, ' -/- A, ' drop A,

: voc-to ( n voc-xt -- ) \ gforth-internal
    \g this is the TO-method for normal values
    >body voc-table to-!exec ;

' reveal alias recursive ( compilation -- ; run-time -- ) \ gforth
\g Make the current definition visible, enabling it to call itself
\g recursively.
	immediate restrict
