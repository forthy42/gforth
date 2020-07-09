\ definitions needed for interpreter only

\ Authors: Bernd Paysan, Anton Ertl, Neal Crook, Gerald Wodni, Jens Wilke
\ Copyright (C) 1995-2000,2004,2005,2007,2009,2010,2012,2013,2014,2017,2018 Free Software Foundation, Inc.

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

\ \ Revision-Log

\       put in seperate file				14sep97jaw 

\ \ input stream primitives                       	23feb93py

: >comp  ( xt -- ) name>comp execute ;
: no-to ( xt -- )
    \ default to action: report an error ASAP (even right when COMPILE,ing)
    #-12 throw ;
opt: #-12 throw ; \ 
: no-defer@ ( xt -- ) #-2055 throw ;
opt: #-2055 throw ;

require ./basics.fs 	\ bounds decimal hex ...
require ./io.fs		\ type ...
require ./nio.fs	\ . <# ...
require ./errore.fs	\ .error ...
require kernel/version.fs \ version-string

\ parse                                           23feb93py

: (parse)    ( char "ccc<char>" -- c-addr u )
    >r  source  >in @ over min /string ( c-addr1 u1 )
    over  swap r>  scan >r
    over - dup r@ IF 1+ THEN  >in +!
    2dup r> 0<> - input-lexeme! ;

Defer parse ( xchar "ccc<xchar>" -- c-addr u ) \ core-ext,xchar
\G Parse @i{ccc}, delimited by @i{xchar}, in the parse
\G area. @i{c-addr u} specifies the parsed string within the
\G parse area. If the parse area was empty, @i{u} is 0.
' (parse) is parse

\ name                                                 13feb93py

[IFUNDEF] (name) \ name might be a primitive

: (name) ( -- c-addr count ) \ gforth
    source 2dup >r >r >in @ 2dup u< IF  -18 throw  THEN
    /string (parse-white)
    2dup input-lexeme!
    2dup + r> - 1+ r> min >in ! ;
\    name count ;
[THEN]

: name-too-short? ( c-addr u -- c-addr u )
    dup 0= -&16 and throw ;

: name-too-long? ( c-addr u -- c-addr u )
    dup lcount-mask u> -&19 and throw ;

\ \ Number parsing					23feb93py

\ (number?) number                                       23feb93py

hex
const Create bases   0A , 10 ,   2 ,   0A ,
\                    10   16     2     10
decimal

\ !! protect BASE saving wrapper against exceptions
: getbase ( addr u -- addr' u' )
    2dup s" 0x" string-prefix? >r
    2dup s" 0X" string-prefix? r> or
    base @ &34 < and if
	hex 2 /string
	1 >num-state @ or >num-state !  EXIT
    endif
    over c@ '#' - dup 4 u<
    IF
	cells bases + @ base ! 1 /string
	1 >num-state @ or >num-state !
    ELSE
	drop
    THEN ;

: sign? ( addr u -- addr1 u1 flag )
    over c@ '-' = >num-state @ 2 and 0= and  dup >r
    IF
	1 /string  2 >num-state +!
    THEN
    r> ;

: ?dnegate ( d1 f -- d2 )
    if
        dnegate
    then ;

has? os 0= [IF]
: x@+/string ( addr u -- addr' u' c )
    over c@ >r 1 /string r> ;
[THEN]

: s'>unumber? ( addr u -- ud flag )
    \ convert string "C" or "C'" to character code
    dup 0= if
	false exit
    endif
    x@+/string 0 s" '" 2rot string-prefix? ;

Defer ?warn#  ' noop is ?warn#

: s>unumber? ( c-addr u -- ud flag ) \ gforth
    \G converts string c-addr u into ud, flag indicates success
    dpl on
    over c@ '' = if
	1 /string s'>unumber? exit
    endif
    base @ >r  getbase sign?
    over if
	>r #0. 2swap
	over c@ dp-char @ = IF  1 /string dup dpl !  THEN
	\ allow an initial '.' to shadow all floating point without 'e'
        BEGIN ( d addr len )
            dup >r >number dup
        WHILE \ there are characters left
                dup r> -
            WHILE \ the last >number parsed something
                    dup 1- dpl ! over c@ dp-char @ =
                WHILE \ the current char is '.'
                        1 /string
                REPEAT  THEN \ there are unparseable characters left
            2drop rdrop false  dpl on
        ELSE
            rdrop 2drop r> ?dnegate true
        THEN
    ELSE
        drop 2drop #0. false  dpl on  THEN
    r> base !  ?warn#  >num-state off ;

\ ouch, this is complicated; there must be a simpler way - anton
: s>number? ( addr u -- d f ) \ gforth
    \G converts string addr u into d, flag indicates success
    sign? >r
    s>unumber?
    0= IF
        rdrop false
    ELSE \ no characters left, all ok
	r> ?dnegate
	true
    THEN ;

: s>number ( addr len -- d )
    \ don't use this, there is no way to tell success
    s>number? drop ;

: snumber? ( c-addr u -- 0 / n -1 / d 0> )
    s>number? 0=
    IF
	2drop false  EXIT
    THEN
    dpl @ dup 0< IF
	nip
    ELSE
	1+
    THEN ;

: (number?) ( string -- string 0 / n -1 / d 0> )
    dup >r count snumber? dup if
	rdrop
    else
	r> swap
    then ;

: number ( string -- d )
    (number?) ?dup 0= abort" ?"  0<
    IF
	s>d
    THEN ;

\ \ Comments ( \ \G

: ( ( compilation 'ccc<close-paren>' -- ; run-time -- ) \ thisone- core,file	paren
    \G ** this will not get annotated. The alias in glocals.fs will instead **
    \G It does not work to use "wordset-" prefix since this file is glossed
    \G by cross.fs which doesn't have the same functionalty as makedoc.fs
    ')' parse 2drop ; immediate

: \ ( compilation 'ccc<newline>' -- ; run-time -- ) \ thisone- core-ext,block-ext backslash
    \G ** this will not get annotated. The alias in glocals.fs will instead ** 
    \G It does not work to use "wordset-" prefix since this file is glossed
    \G by cross.fs which doesn't have the same functionalty as makedoc.fs
    [ has? file [IF] ]
    blk @
    IF
	>in @ c/l / 1+ c/l * >in !
	EXIT
    THEN
    [ [THEN] ]
    source >in ! drop ; immediate

: \G ( compilation 'ccc<newline>' -- ; run-time -- ) \ gforth backslash-gee
    \G Equivalent to @code{\} but used as a tag to annotate definition
    \G comments into documentation.
    POSTPONE \ ; immediate

\ \ object oriented search list                         17mar93py

\ word list structure:

struct
  cell% field find-method   \ xt: ( c_addr u wid -- nt )
  cell% field reveal-method \ xt: ( nt wid -- )
  cell% field rehash-method \ xt: ( wid -- )	   \ re-initializes a "search-data" (hashtables)
  cell% field hash-method   \ xt: ( wid -- )    \ initializes ""
\   \ !! what else
end-struct wordlist-map-struct

struct
  cell% field wordlist-map \ pointer to a wordlist-map-struct
  cell% field wordlist-id \ linked list of words (for WORDS etc.)
  cell% field wordlist-link \ link field to other wordlists
  cell% field wordlist-extend \ wordlist extensions (eg bucket offset)
end-struct wordlist-struct

: f83find      ( addr len wordlist -- nt / false )
    wordlist-id @ (listlfind) ;

: initvoc		( wid -- )
  dup wordlist-map @ hash-method perform ;

\ Search list table: find reveal
Create f83search ( -- wordlist-map )
    ' f83find A,  ' drop A,  ' drop A, ' drop A,

here f83search A, NIL A, NIL A, NIL A,
AValue forth-wordlist \ variable, will be redefined by search.fs

AVariable lookup       	forth-wordlist lookup !
\ !! last is user and lookup?! jaw
AVariable current ( -- addr ) \ gforth
\G @code{Variable} -- holds the @i{wid} of the compilation word list.
AVariable voclink	forth-wordlist wordlist-link voclink !
\ lookup AValue context ( -- addr ) \ gforth
Defer context ( -- addr ) \ gforth
\G @code{context} @code{@@} is the @i{wid} of the word list at the
\G top of the search order.

$variable wheres

0
field: where-nt
field: where-loc
constant where-struct
Create new-where where-struct allot

: where-duplicate? ( -- f )
    \ true if the current where tuple would be a duplicate of the last
    \ one; these duplicates occur due to FIND-NAME-IN being called
    \ once for LOOKUP and then again for the individual wordlists.
    wheres $@ dup if ( addr u )
	where-struct - + new-where where-struct tuck str=  exit
    then
    nip ;

: where, ( nt -- )
    \ store nt and the current source position for use by WHERE
    dup if ( nt )
	new-where where-nt !
	current-sourceview new-where where-loc !
	source-id dup -1 <> and if
	    where-duplicate? 0= if
		new-where where-struct wheres $+!
	    then
	then
	exit
    then
    drop ;

\ find and friends

' lookup is context
forth-wordlist current !

: find-name-in  ( c-addr u wid -- nt | 0 )
    \G search the word list identified by @i{wid} for the definition
    \G named by the string at @i{c-addr u}. Return its @i{nt}, if
    \G found, otherwise 0.
    dup wordlist-map @ find-method perform dup where, ;

: search-wordlist ( c-addr count wid -- 0 | xt +-1 ) \ search
    \G Search the word list identified by @i{wid} for the definition
    \G named by the string at @i{c-addr count}.  If the definition is
    \G not found, return 0. If the definition is found return 1 (if
    \G the definition is immediate) or -1 (if the definition is not
    \G immediate) together with the @i{xt}.  In Gforth, the @i{xt}
    \G returned represents the interpretation semantics.  ANS Forth
    \G does not specify clearly what @i{xt} represents.
    find-name-in dup if
	(name>intn)
    then ;

: find-name ( c-addr u -- nt | 0 ) \ gforth
    \g Find the name @i{c-addr u} in the current search
    \g order. Return its @i{nt}, if found, otherwise 0.
    lookup @ find-name-in ;

\ \ header, finding, ticks                              17dec92py

\ The constants are defined as 32 bits, but then erased
\ and overwritten by the right ones

\ 32-bit systems cannot generate large 64-bit constant in the
\ cross-compiler, so we kludge it by generating a constant and then
\ storing the proper value into it (and that's another kludge).
\ $80000000 constant alias-mask
\ 1 bits/char 1 - lshift
\ -1 cells allot  bigendian [IF]   c, 0 1 cells 1- times
\                           [ELSE] 0 1 cells 1- times c, [THEN]
\ $80000000 constant immediate-mask
\ 1 bits/char 1 - lshift
\ -1 cells allot  bigendian [IF]   c, 0 1 cells 1- times
\                           [ELSE] 0 1 cells 1- times c, [THEN]
$40000000 constant restrict-mask
1 bits/char 2 - lshift
-1 cells allot  bigendian [IF]   c, 0 1 cells 1- times
                          [ELSE] 0 1 cells 1- times c, [THEN]
\ $20000000 constant prelude-mask
\ 1 bits/char 3 - lshift
\ -1 cells allot  bigendian [IF]   c, 0 1 cells 1- times
\                           [ELSE] 0 1 cells 1- times c, [THEN]
\ $01000000 constant unused-mask \ defined in locate1.fs, used only temporarily
\ reserve 8 bits for all possible flags in total
$00ffffff constant lcount-mask
0 -1 cells allot  bigendian [IF]   c, -1 1 cells 1- times
                          [ELSE] -1 1 cells 1- times c, [THEN]
[THEN]

\ higher level parts of find

: flag-sign ( f -- 1|-1 )
    \ true becomes 1, false -1
    0= 2* 1+ ;

: ticking-compile-only-error ( ... -- )
    -&2048 throw ;

: compile-only-error ( ... -- )
    -&14 throw ;

: (x>int) ( cfa w -- xt )
    \ get interpretation semantics of name
    restrict-mask and [ has? rom [IF] ] 0= [ [THEN] ]
    if
	drop ['] compile-only-error
    then ;

' noop Alias ((name>)) ( nfa -- cfa )

(field) >vtlink        0 cells ,
(field) >vtcompile,    1 cells ,
(field) >vtto          2 cells ,
(field) >vtdefer@      3 cells ,
(field) >vtextra       4 cells ,
(field) >vt>int        5 cells ,
(field) >vt>comp       6 cells ,
(field) >vt>string     7 cells ,
(field) >vt>link       8 cells ,

1 cells -3 cells \ mini-oof class declaration with methods
\ the offsets are a bit odd to keep the xt as point of reference
cell var >f+c
cell var >link
cell var >namevt

method opt-compile, ( xt -- ) \ gforth-internal
\g The intelligent @code{compile,} compiles each word as specified by
\g @code{set-optimizer} for that word.

method (to) ( val xt -- ) \ gforth paren-int-to
\G @i{xt} is of a value like word @i{name}.  Stores @i{val} @code{to} @i{name}.
opt: ( xt-(to -- )
    ?fold-to (to), ;

method defer@ ( xt-deferred -- xt ) \ gforth defer-fetch
\G @i{xt} represents the word currently associated with the deferred
\G word @i{xt-deferred}.
opt: ( xt-defer@ -- )
    ?fold-to defer@, ;

swap cell+ swap \ vtextra

method name>int ( nt -- xt ) \ gforth name-to-int
\G @i{xt} represents the interpretation semantics of the word
\G @i{nt}.

method name>comp ( nt -- w xt ) \ gforth name-to-comp
\G @i{w xt} is the compilation token for the word @i{nt}.

method name>string ( nt -- addr u ) \ gforth name-to-string
    \g @i{addr count} is the name of the word represented by @i{nt}.
method name>link ( nt1 -- nt2 / 0 ) \ gforth name-to-link

drop Constant vtsize \ vtable size

defer compile, ( xt -- )
\G Append the semantics represented by @i{xt} to the current
\G definition.  When the resulting code fragment is run, it behaves
\G the same as if @i{xt} is @code{execute}d.
' opt-compile, is compile,

: ,     ( w -- ) \ core comma
    \G Reserve data space for one cell and store @i{w} in the space.
    cell small-allot ! ;

: immediate? ( nt -- flag )    name>comp nip ['] compile, <> ;
: compile-only? ( nt -- flag )
    dup name>string nip IF
	>f+c @ restrict-mask and 0<>
    ELSE  drop false  THEN ;
: ?compile-only ( nt -- nt )
    dup compile-only? IF
	<<# s"  is compile-only" holds dup name>string holds #0. #>
	hold 1- c(warning") #>>
    THEN ;

: name?int ( nt -- xt ) \ gforth-obsolete name-question-int
\G Like @code{name>int}, but warns when encountering a word marked
\G compile-only
    ?compile-only name>int ;

: named>string ( nt -- addr count ) \ gforth     named-to-string
    >f+c dup @ lcount-mask and tuck - swap ;
: named>link ( nt1 -- nt2 / 0 ) \ gforth	named-to-link
    >link @ ;

: noname>string ( nt -- vt 0 ) \ gforth    noname-to-string
    cell- 0 ;
: noname>link ( nt -- 0 ) \ gforth    noname-to-link
    drop 0 ;

\ : name>view ( nt -- addr ) \ gforth   name-to-view
\     name>string drop cell negate and cell- ;

\ DEFAULT-NAME>INT is never used, delete?
: default-name>int ( nt -- xt ) \ gforth default-name-to-int
    \G Default @code{name>interpret} implementation.  For words where nt=xt.
;

: (name>intn) ( nfa -- xt +-1 )
    dup name>int swap name>comp nip ['] execute = flag-sign ;

[IFDEF] prelude-mask
: name>prelude ( nt -- xt )
    dup >f+c @ prelude-mask and if
	[ -1 cells ] literal + @
    else
	drop ['] noop
    then ;
[THEN]

const Create ???

: vt? ( vt -- flag )
    \G check if a vt is actually one
    dup vttemplate = IF  drop true  EXIT  THEN
    >r  vtable-list
    BEGIN  @ dup  WHILE
	    dup r@ = IF  rdrop drop true  EXIT  THEN
    REPEAT  rdrop ;

: xt? ( xt -- f )
    \G check for xt - must be code field or primitive
    dup in-dictionary? IF
	dup >body dup maxaligned = IF
	    dup >namevt @ vt? IF
		dup @ tuck body> = swap
		docol:  ['] u#+ @ 1+ within or  EXIT
	    THEN
	THEN
    THEN
    drop false ;

: >head-noprim ( xt -- nt ) \ gforth  to-head-noprim
    dup xt? 0= IF  drop ['] ???  THEN ;

cell% 0 0 field >body ( xt -- a_addr ) \ core to-body
\G Get the address of the body of the word represented by @i{xt} (the
\G address of the word's data field).
drop drop

cell% -1 * 0 0 field body> ( xt -- a_addr )
    drop drop

' @ alias >code-address ( xt -- c_addr ) \ gforth
\G @i{c-addr} is the code address of the word @i{xt}.

: >does-code ( xt -- a_addr ) \ gforth
\G If @i{xt} is the execution token of a child of a @code{DOES>} word,
\G @i{a-addr} is the start of the Forth code after the @code{DOES>};
\G Otherwise @i{a-addr} is 0.
    dup @ dodoes: = if
	>namevt @ >vtextra @ >body
    else
	drop 0
    then ;

' ! alias code-address! ( c_addr xt -- ) \ gforth
\G Create a code field with code address @i{c-addr} at @i{xt}.

: any-code! ( a-addr cfa code-addr -- )
    \ for implementing DOES> and ;ABI-CODE, maybe :
    \ code-address is stored at cfa, a-addr at cfa+cell
    over !  >namevt @ >vtextra ! ;

: does-code! ( xt1 xt2 -- ) \ gforth
\G Create a code field at @i{xt2} for a child of a @code{DOES>}-word;
\G @i{xt1} is the execution token of the assigned Forth code.
    dodoes: any-code! ;

2 cells constant /does-handler ( -- n ) \ gforth
\G The size of a @code{DOES>}-handler (includes possible padding).

\ ticks in interpreter

: '-error ( nt -- nt )
    dup rectype-null = #-13 and throw
    rectype-nt    <> #-2053 and throw ;

: (') ( "name" -- nt ) \ gforth
    parse-name name-too-short? forth-recognizer recognize '-error ;

: '    ( "name" -- xt ) \ core	tick
    \g @i{xt} represents @i{name}'s interpretation
    \g semantics. Perform @code{-14 throw} if the word has no
    \g interpretation semantics.
    (') name?int ;

\ \ the interpreter loop				  mar92py

\ interpret                                            10mar92py

Defer parser1 ( c-addr u -- ... xt)
\ "... xt" is the action to be performed by the text-interpretation of c-addr u

: parser ( c-addr u -- ... )
\ text-interpret the word/number c-addr u, possibly producing a number
    parser1 execute ;
Defer parse-name ( "name" -- c-addr u ) \ gforth
\G Get the next word from the input buffer
' (name) IS parse-name

' parse-name alias parse-word ( -- c-addr u ) \ gforth-obsolete
\G old name for @code{parse-name}; this word has a conflicting
\G behaviour in some other systems.

' parse-name alias name ( -- c-addr u ) \ gforth-obsolete
\G old name for @code{parse-name}
    
: no.extensions  ( -- )
    -&13 throw ;

Defer before-word ( -- ) \ gforth
\ called before the text interpreter parses the next word
' noop IS before-word

Defer before-line ( -- ) \ gforth
\ called before the text interpreter parses the next line
' noop IS before-line

defer int-execute ( ... xt -- ... )
\ like EXECUTE, but restores and saves ERRNO if present
' execute IS int-execute

: interpret ( ... -- ... ) \ gforth
    \ interpret/compile the (rest of the) input buffer
    [ cell 4 = [IF] ] false >l [ [THEN] ] \ align LP stack for 32 bit engine
    r> >l rp@ backtrace-rp0 !
    [ has? EC 0= [IF] ] before-line [ [THEN] ]
    BEGIN
	?stack [ has? EC 0= [IF] ] before-word [ [THEN] ] parse-name dup
    WHILE
	parser1 int-execute
    REPEAT
    2drop @local0 >r lp+ ;

: bt-rp0-catch ( ... xt -- ... ball )
    backtrace-rp0 @ >r	
    catch
    r> backtrace-rp0 ! ;

: bt-rp0-wrapper ( ... xt -- ... )
    bt-rp0-catch throw ;

: interpret2 ['] interpret bt-rp0-wrapper ;

\ interpreter                                 	30apr92py

[IFDEF] prelude-mask
: run-prelude ( nt|0 -- nt|0 )
    \ run the prelude of the name identified by nt (if present).  This
    \ is used in the text interpreter and similar stuff.
    dup if
	dup name>prelude execute
    then ;
[THEN]

\ save-mem extend-mem

: save-mem	( addr1 u -- addr2 u ) \ gforth
    \g copy a memory block into a newly allocated region in the heap
    swap >r
    dup dfaligned allocate throw
    swap 2dup r> -rot move ;

: free-mem-var ( addr -- )
    \ addr is the address of a 2variable containing address and size
    \ of a memory range; frees memory and clears the 2variable.
    dup 2@ drop dup
    if ( addr mem-start )
	free throw
	0 0 rot 2!
    else
	2drop
    then ;

: extend-mem	( addr1 u1 u -- addr addr2 u2 )
    \ extend memory block allocated from the heap by u aus
    \ the (possibly reallocated) piece is addr2 u2, the extension is at addr
    over >r + dup >r resize throw
    r> over r> + -rot ;

\ \ Quit                                            	13feb93py

Defer 'quit
Defer .status
defer prompt

: color-execute ( xt x-color -- ... ) \ gforth
    \G execute a xt using color
    attr! catch default-color attr! throw ;

: (prompt) ( -- )
    ."  ok" ;
' (prompt) is prompt

: (quit1) ( -- )
    \ exits only through THROW etc.
    BEGIN
	[ has? ec [IF] ] cr [ [ELSE] ]
	.status ['] cr catch if
	    [ has? OS [IF] ] >stderr [ [THEN] ]
	    cr ." Can't print to stdout, leaving" cr
	    \ if stderr does not work either, already DoError causes a hang
	    -2 (bye)
	endif [ [THEN] ]
	get-input-colored WHILE
	    interpret prompt
    REPEAT ;

: (quit) ( -- )
    ['] (quit1) bt-rp0-wrapper bye ;

' (quit) IS 'quit

\ \ DOERROR (DOERROR)                        		13jun93jaw

5 has? file 2 and + cells Constant /error
User error-stack  0 error-stack !
\ format of one cell:
\ source ( c-addr u )
\ last parsed lexeme ( c-addr u )
\ line-number
\ Loadfilename ( addr u )

: error> ( --  c-addr1 u1 c-addr2 u2 line# [addr u] )
    error-stack $@ + /error - /error bounds DO
        I @
    cell +LOOP error-stack dup $@len /error - /error $del ;

: >error ( c-addr1 u1 c-addr2 u2 line# [addr u] -- )
    error-stack $@len /error + error-stack $!len
    error-stack $@ + /error - /error cell- bounds swap DO
        I !
    -1 cells +LOOP ;

: input-error-data ( -- c-addr1 u1 c-addr2 u2 line# [addr u] )
    \ error data for the current input, to be used by >error or .error-frame
    source over >r save-mem over r> -
    input-lexeme 2@ >r + r> sourceline#
    [ has? file [IF] ] sourcefilename [ [THEN] ] ;

: dec. ( n -- ) \ gforth
    \G Display @i{n} as a signed decimal number, followed by a space.
    \ !! not used...
    base @ decimal swap . base ! ;

: dec.r ( u n -- ) \ gforth
    \G Display @i{u} as a unsigned decimal number in a field @i{n}
    \G characters wide.
    base @ >r decimal .r r> base ! ;

: hex. ( u -- ) \ gforth
    \G Display @i{u} as an unsigned hex number, prefixed with a "$" and
    \G followed by a space.
    \ !! not used...
    '$' emit base @ swap hex u. base ! ;

: -trailing  ( c_addr u1 -- c_addr u2 ) \ string dash-trailing
\G Adjust the string specified by @i{c-addr, u1} to remove all
\G trailing spaces. @i{u2} is the length of the modified string.
    BEGIN
	dup
    WHILE
	1- 2dup + c@ bl <>
    UNTIL  1+  THEN ;

DEFER DOERROR

has? backtrace [IF]
Defer dobacktrace ( -- )
' noop IS dobacktrace
[THEN]

: .error-string ( throw-code -- )
  dup -2 = 
  IF 	"error @ ?dup IF count type  THEN drop
  ELSE	.error
  THEN ;

[IFUNDEF] umin
: umin ( u1 u2 -- u )
    2dup u>
    if
	swap
    then
    drop ;
[THEN]

Defer mark-start
Defer mark-end

:noname ." >>>" ; IS mark-start
:noname ." <<<" ; IS mark-end

: part-type ( addr1 u1 u -- addr2 u2 )
    \ print first u characters of addr1 u1, addr2 u2 is the rest
    over umin 2 pick over type /string ;

: .error-line ( c-addr1 u1 c-addr2 u2 -- )
    \ print error in line c-addr1 u1, where the error-causing lexeme
    \ is c-addr2 u2
    >r 2 pick - part-type ( c-addr3 u3 R: u2 )
    mark-start r> part-type mark-end ( c-addr4 u4 )
    type ;

Defer .error-level ( n -- )
: (.error-level) >r
    r@ 2 = IF  ." error: "    THEN
    r@ 1 = IF  ." warning: "  THEN
    r@ 0 = IF  ." info: "     THEN  rdrop ;
' (.error-level) is .error-level

: .error-frame ( throwcode addr1 u1 addr2 u2 n2 addr3 u3 errlevel -- throwcode )
    \ addr3 u3: filename of included file
    \ n2:       line number
    \ addr2 u2: parsed lexeme (should be marked as causing the error)
    \ addr1 u1: input line
    \ errlevel: 0: info, 1: warning, 2: error
    >r error-stack $@len
    IF ( throwcode addr1 u1 n0 n1 n2 [addr2 u2] )
	over IF
	    cr ." in file included from "
	    type ." :"
	    0 dec.r ." :" drop nip swap - 1+ 0 dec.r ." : "
	ELSE
	    2drop 2drop 2drop drop
	THEN
    ELSE ( throwcode addr1 u1 n0 n1 n2 [addr2 u2] )
	cr type ." :"
	( throwcode addr1 u1 n0 n1 n2 )
	dup 0 dec.r ." :"
	>r 2over 2over drop nip swap - 1+ 0 dec.r ." : " r>
	r@ .error-level
	5 pick .error-string
	r@ 2 = and warnings @ abs 1 > or
	\ only for errors or novice warnings print line
	IF \ if line# non-zero, there is a line
            cr .error-line
        ELSE
            2drop 2drop
        THEN
    THEN  rdrop ;

defer reset-dpp
:noname section-dp dpp ! ; is reset-dpp

: (DoError) ( throw-code -- )
    dup -1 = IF  drop EXIT  THEN \ -1 is abort, no error message!
    [ has? os [IF] ]
	>stderr error-color attr!
	[ [THEN] ]
    input-error-data 2 .error-frame
    error-stack $@len 0 ?DO
	error>
	2 .error-frame
    /error +LOOP
    drop 
    dobacktrace
    default-color attr! endif? on
  reset-dpp ;

' (DoError) IS DoError

: quit ( ?? -- ?? ) \ core
    \G Empty the return stack, make the user input device
    \G the input source, enter interpret state and start
    \G the text interpreter.
    rp0 @ rp! handler off clear-tibstack
    [ has? new-input 0= [IF] ] >tib @ >r [ [THEN] ]
    BEGIN
	[ has? compiler [IF] ]
	    [compile] [
	[ [THEN] ]
	\ stack depths may be arbitrary here
	['] 'quit CATCH dup
    WHILE
	    <# \ reset hold area, or we may get another error
	    DoError
	    \ stack depths may be arbitrary still (or again), so clear them
	    clearstacks
	    [ has? new-input [IF] ] clear-tibstack
	    [ [ELSE] ] r@ >tib ! r@ tibstack !
	    [ [THEN] ]
    REPEAT
    drop [ has? new-input [IF] ] clear-tibstack
    [ [ELSE] ] r> >tib !
    [ [THEN] ] ;

: do-execute ( xt -- ) \ Gforth
    \G C calling us
    execute ( catch dup IF  DoError cr  THEN ) 0 (bye) ;

: do-find ( addr u -- )
    find-name dup IF  name>int  THEN  (bye) ;

\ \ Cold Boot                                    	13feb93py

: (c) ( -- )
    ." Copyright " $A9 ( '©' ) xemit ;
: gforth ( -- )
    ." Gforth " version-string type cr
    ." Authors: Anton Ertl, Bernd Paysan, Jens Wilke et al., for more type `authors'" cr
    (c) ."  2019 Free Software Foundation, Inc." cr
    ." License GPLv3+: GNU GPL version 3 or later <https://gnu.org/licenses/gpl.html>" cr
    ." Gforth comes with ABSOLUTELY NO WARRANTY; for details type `license'"
[ has? os [IF] ]
     cr ." Type `help' for basic help"
[ [THEN] ] ;

defer bootmessage ( -- ) \ gforth
\G Hook (deferred word) executed right after interpreting the OS
\G command-line arguments.  Normally prints the Gforth startup
\G message.

has? file [IF]
defer process-args
[THEN]

' gforth IS bootmessage

Defer 'cold ( -- ) \ gforth  tick-cold
\G Hook (deferred word) for things to do right before interpreting the
\G OS command-line arguments.  Normally does some initializations that
\G you also want to perform.

: cold ( -- ) \ gforth
[ has? backtrace [IF] ]
    rp@ backtrace-rp0 !
[ [THEN] ]
[ has? file [IF] ]
    os-cold
[ [THEN] ]
[ has? os [IF] ]
    set-encoding-fixed-width
    'cold
[ [THEN] ]
[ has? file [IF] ]
    process-args
    loadline off
[ [THEN] ]
    -56 (bye) ; \ indicate QUIT

: boot ( path n **argv argc -- )
    threading-method 1 = if
	['] , is compile,
    then
[ has? no-userspace 0= [IF] ]
    next-task 0= IF  main-task up!
    ELSE
	next-task @ 0= IF
	    throw-entry main-task udp @ throw-entry next-task -
	    /string >r swap r> move
	    next-task dup next-task 2!  reset-dpp
	THEN
    THEN
[ [THEN] ]
[ has? os [IF] ]
    os-boot
[ [THEN] ]
[ has? rom [IF] ]
    ram-shadow dup @ dup -1 <> >r u> r> and IF
	ram-shadow 2@  ELSE
	ram-mirror ram-size  THEN  ram-start swap move
[ [THEN] ]
    sp@ sp0 !
    boot-strings
[ has? new-input [IF] ]
    current-input off
[ [THEN] ]
    clear-tibstack
    rp@ rp0 !
[ has? floating [IF] ]
    fp@ fp0 !
[ [THEN] ]
[ has? os [IF] ]
    handler off
    ['] cold catch dup -&2049 <> if \ broken pipe?
	dup >r DoError cr r>
    endif
    (bye) \ determin exit code from throw code
[ [ELSE] ]
    cold
[ [THEN] ]
;

has? os [IF]
: bye ( -- ) \ tools-ext
[ has? file [IF] ]
    script? 0= IF  .unstatus cr  THEN
[ [ELSE] ]
    cr
[ [THEN] ]
    0 (bye) ;
[THEN]

\ **argv may be scanned by the C starter to get some important
\ information, as -display and -geometry for an X client FORTH
\ or space and stackspace overrides

\ 0 arg contains, however, the name of the program.

