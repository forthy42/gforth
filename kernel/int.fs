\ definitions needed for interpreter only

\ Authors: Bernd Paysan, Anton Ertl, Neal Crook, Gerald Wodni, Jens Wilke
\ Copyright (C) 1995,1996,1997,1998,1999,2000,2004,2005,2007,2009,2010,2012,2013,2014,2017,2018,2020,2021,2022 Free Software Foundation, Inc.

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

: >comp  ( xt -- ) name>compile execute ;
' execute set-optimizer

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

Defer parse ( xchar "ccc<xchar>" -- c-addr u ) \ core-ext,xchar-ext
\G Parse @i{ccc}, delimited by @i{xchar}, in the parse
\G area. @i{c-addr u} specifies the parsed string within the
\G parse area. If the parse area was empty, @i{u} is 0.
' (parse) is parse

\ name                                                 13feb93py

[IFUNDEF] (name) \ name might be a primitive

: (name) ( -- c-addr count )
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
	1 >num-warnings +!  EXIT
    endif
    over c@ '#' - dup 4 u<
    IF
	cells bases + @ base ! 1 /string
	1 >num-warnings +!
    ELSE
	drop
    THEN ;

: sign? ( addr u -- addr1 u1 flag )
    over c@ '-' = >num-warnings @ 2 and 0= and  dup >r
    IF
	1 /string  2 >num-warnings +!
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

: s>unumber? ( c-addr u -- ud flag ) \ gforth
    \G converts string c-addr u into ud, flag indicates success
    dpl on >num-warnings off
    over c@ '' = if
	1 /string s'>unumber? exit
    endif
    base @ >r  getbase sign?
    over if
	>r #0. 2swap
	over c@ '. = over 1 u> and IF
	    1 /string dup dpl !  8 4 third select >num-warnings +!  THEN
	\ allow an initial '.' to shadow all floating point without 'e'
        BEGIN ( d addr len )
            dup >r >number_ dup
        WHILE \ there are characters left
                dup r> -
            WHILE \ the last >number_ parsed something
                    dup 1- dpl ! over c@ '. =
                WHILE \ the current char is '.'
			1 /string
			8 4 third select >num-warnings @ or >num-warnings !
                REPEAT  THEN \ there are unparseable characters left
            2drop rdrop false  dpl on  >num-warnings off
        ELSE
            rdrop 2drop r> ?dnegate true
        THEN
    ELSE
	drop 2drop #0. false  dpl on  >num-warnings off  THEN
    r> base ! ;

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
    (number?) dup 0= abort" ?"  0<
    IF
	s>d
    THEN ;

\ \ Comments ( \ \G

: ( ( compilation 'ccc<close-paren>' -- ; run-time -- ) \ core,file	paren
\G Comment, usually till the next @code{)}: parse and discard all
\G subsequent characters in the parse area until ")" is
\G encountered. During interactive input, an end-of-line also acts as
\G a comment terminator. For file input, it does not; if the
\G end-of-file is encountered whilst parsing for the ")" delimiter,
\G Gforth will generate a warning.
    ')' parse 2drop ; immediate

: \ ( compilation 'ccc<newline>' -- ; run-time -- ) \ core-ext,block-ext backslash
\G Comment until the end of line: parse and discard all remaining
\G characters in the parse area, except while @code{load}ing from a
\G block: while @code{load}ing from a block, parse and discard all
\G remaining characters in the 64-byte line.
    [ has? file [IF] ]
    blk @
    IF
	>in @ c/l / 1+ c/l * >in !
	EXIT
    THEN
    [ [THEN] ]
    #lf parse \ source >in @ over >in ! safe/string
    $10 umin s" gforth-obsolete " third $0F = +
    compare 0= IF  obsolete  THEN ; immediate

: \G ( compilation 'ccc<newline>' -- ; run-time -- ) \ gforth backslash-gee
    \G Equivalent to @code{\}.  Used right below the start of a
    \G definition to describe the behaviour of a word.  In Gforth's
    \G source code these comments are those that are then inserted in
    \G the documentation.
    POSTPONE \ ; immediate

\ \ object oriented search list                         17mar93py

\ word list structure:

\ struct
\    2 cells +
\    cell% field reveal-method \ xt: ( nt wid -- )
\    cell% field hash-method   \ xt: ( wid -- )    \ initializes ""
\   \ !! what else
\ end-struct wordlist-map-struct

struct
    has? new-cfa [IF]
	2 cells - \ wordlist-map is at offset -1 cell like vtable
	0 0 field wordlist-start
	cell% field wordlist-exec \ exec pointer for wordlist-map-struct
	cell% field wordlist-map \ pointer to a wordlist-map-struct
    [ELSE]
	1 cells - \ wordlist-map is at offset -1 cell like vtable
	0 0 field wordlist-start
	cell% field wordlist-map \ pointer to a wordlist-map-struct
	cell% field wordlist-exec \ exec pointer for wordlist-map-struct
    [THEN]
    cell% field wordlist-id \ linked list of words (for WORDS etc.)
    cell% field wordlist-link \ link field to other wordlists
    cell% field wordlist-extend \ wordlist extensions (eg bucket offset)
end-struct wordlist-struct

: rec-f83 ( addr len wordlist-id-addr -- nt translate-name / 0 )
    @ (listlfind) nt>rec ;

\ : initvoc		( wid -- )
\   dup wordlist-map @ hash-method perform ;

\ Search list table: find reveal

unlock
hm, cfalign
lock
has? new-cfa [IF]  ' :dodoes A,  [THEN]
unlock
hm-template, hm-noname
lock
here has? new-cfa [IF] [ELSE] ' :dodoes A, [THEN]
NIL A, NIL A, NIL A,
unlock
ghost rec-f83 gset-extra
ghost voc-to gset-to
\ ghost drop gset-defer@
ghost does, gset-optimizer
lock
AValue forth-wordlist ( -- wid ) \ search
  \G @code{Constant} -- @i{wid} identifies the word list that includes
  \G all of the standard words provided by Gforth. When Gforth is
  \G invoked, this word list is the compilation word list and is at
  \G the top of the search order.
\ variable, will be redefined by search.fs
forth-wordlist 1 cells - @ AConstant f83search

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

Defer where,

: (where,) ( nt -- )
    \ store nt and the current source position for use by WHERE
    source-id 1+ 2 u>= and dup if ( nt )
	dup new-where @ <> if
	    new-where !
	    new-where where-struct wheres $+!
	    exit
	then
    then
    drop ;

' (where,) is where,

\ find and friends

forth-wordlist current !

: find-name-in  ( c-addr u wid -- nt | 0 ) \ gforth
    \G Find the name @i{c-addr u} in the word list @i{wid}. Return its
    \G @i{nt}, if found, otherwise 0.
    execute translate-none = IF  0  THEN ;

: search-wordlist ( c-addr count wid -- 0 | xt +-1 ) \ search
    \G Search the word list identified by @i{wid} for the definition
    \G named by the string at @i{c-addr count}.  If the definition is
    \G not found, return 0. If the definition is found return 1 (if
    \G the definition is immediate) or -1 (if the definition is not
    \G immediate) together with the @i{xt}.  In Gforth, the @i{xt}
    \G returned represents the interpretation semantics.  Forth-2012
    \G does not specify clearly what @i{xt} represents.
    find-name-in dup if
	(name>intn)
    then ;

Defer rec-name ( c-addr u -- translation ) \ gforth-experimental
    \G Recognizes (@pxref{Defining recognizers})
    \G a visible local or a visible named word.  If
    \G successful, @i{translation} represents the text-interpretation
    \G semantics (interpreting, compiling, postponing) of that word
    \G (see @word{translate-name}).

: find-name ( c-addr u -- nt | 0 ) \ gforth
    \g Find the name @i{c-addr u} in the current search
    \g order. Return its @i{nt}, if found, otherwise 0.
    ['] rec-name find-name-in ;

\ \ header, finding, ticks                              17dec92py

\ The constants are defined as 32 bits, but then erased
\ and overwritten by the right ones

\ 32-bit systems cannot generate large 64-bit constant in the
\ cross-compiler, so we kludge it by generating a constant and then
\ storing the proper value into it (and that's another kludge).
\ alias- and immediate-masks are no longer used
$80000000. 1 cells 8 = [IF] #32 dlshift [THEN] dconstant restrict-mask
$40000000. 1 cells 8 = [IF] #32 dlshift [THEN] dconstant obsolete-mask
$01000000. 1 cells 8 = [IF] #32 dlshift [THEN] #1. d- dconstant lcount-mask

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

' [noop] Alias ((name>)) ( nfa -- cfa )

struct
    cell% field >hmlink
    cell% field >hmcompile,
    cell% field >hmto
    cell% field >hmextra
    cell% field >hm>int
    cell% field >hm>comp
    cell% field >hm>string
    cell% field >hm>link
2drop \ hmsize is defined below

has? new-cfa [IF]
    1 cells -4 cells \ mini-oof class declaration with methods
    \ the offsets are a bit odd to keep the xt as point of reference
    cell var >f+c
    cell var >link
    cell var >cfa
    cell var >namehm
[ELSE]
    1 cells -3 cells \ mini-oof class declaration with methods
    \ the offsets are a bit odd to keep the xt as point of reference
    cell var >f+c
    cell var >link
    cell var >namehm
    cell var >cfa
[THEN]

method opt-compile, ( xt -- ) \ gforth-internal
\g The intelligent @code{compile,} compiles each word as specified by
\g @code{set-optimizer} for that word.

method (to) ( val operation xt -- ) \ gforth-internal paren-to
\G @i{xt} is of a value like word @i{name}.  Stores @i{val} @code{to}
\G @i{name}.  @i{operation} selects between @code{to} (0), @code{+to} (1),
\G @code{addr} (2), @code{action-of} (3) and @code{is} (4).
opt: ( operation xt-(to -- )
    lits# 0= IF  swap lit, postpone swap :, EXIT THEN  (to), ;

\ method old-defer@ ( xt-deferred -- xt ) \ core-ext defer-fetch
\ \G @i{xt} represents the word currently associated with the deferred
\ \G word @i{xt-deferred}.
\ fold1: ( xt-defer@ -- )
\      defer@, ;

swap cell+ swap \ hmextra

method name>interpret ( nt -- xt ) \ tools-ext name-to-interpret
\G @i{xt} represents the interpretation semantics of the word
\G @i{nt}.

method name>compile ( nt -- xt1 xt2 ) \ tools-ext name-to-compile
\G @i{xt1 xt2} is the compilation token for the word @i{nt}
\G (@pxref{Compilation token}).

method name>string ( nt -- addr u ) \ tools-ext name-to-string
    \g @i{addr count} is the name of the word represented by @i{nt}.
method name>link ( nt1 -- nt2 / 0 ) \ gforth name-to-link
\G For a word @i{nt1}, returns the previous word @i{nt2} in the same
\G wordlist, or 0 if there is no previous word.

drop Constant hmsize \ vtable size

: to-access:exec ( xt -- ) @ swap (to) ;
: to-access:,    ( xt -- ) lits# IF   @ lits> (to),  EXIT  THEN  does, ;

1 to-access: value+! ( n ... xt-value -- ) \ gforth-internal  value-plus-store
    \G Adds @i{n} to the value of the location indicated by
    \G @i{... xt-value}.
2 to-access: defer@ ( ... xt-deferred -- xt ) \ core-ext defer-fetch
    \G If @i{xt-deferred} belongs to a word defined with @code{defer},
    \G @i{xt} represents the word currently associated with the
    \G deferred word @i{xt-deferred}.@* If @i{xt-deferred} belongs to
    \G another defer-flavoured word (e.g., a defer-flavoured field),
    \G @i{xt} is the word associated with the location indicated by
    \G @i{... xt-deferred} (e.g., for a defer-flavoured field @i{...}
    \G is the structure address).@* If @i{xt-deferred} is the xt of a
    \G word that is not defer-flavoured, throw -21 (Unsupported
    \G operation).
3 to-access: defer! ( xt xt-deferred -- ) \ core-ext defer-store If
    \G @i{xt-deferred} belongs to a word defined with @code{defer}, it
    \G is changed to execute @i{xt} on execution.@* If @i{xt-deferred}
    \G belongs to another defer-flavoured word (e.g., a
    \G defer-flavoured field), the location associated with
    \G @i{... xt-deferred} is changed to execut @i{xt}.@* If
    \G @i{xt-deferred} is the xt of a word that is not
    \G defer-flavoured, throw -21 (Unsupported operation).

: >extra ( nt -- addr )
    >namehm @ >hmextra ;

defer compile, ( xt -- ) \ core-ext compile-comma
\G Append the semantics represented by @i{xt} to the current
\G definition.  When the resulting code fragment is run, it behaves
\G the same as if @i{xt} is @code{execute}d.
' opt-compile, is compile,

: ,     ( w -- ) \ core comma
    \G Reserve data space for one cell and store @i{w} in the space.
    cell small-allot ! ;

: immediate? ( nt -- flag ) \ gforth
    \G true if the word @i{nt} has non-default compilation
    \G semantics (that's not quite according to the definition of
    \G immediacy, but many people mean that when they call a word
    \G ``immediate'').
    name>compile nip ['] compile, <> ;
: compile-only? ( nt -- flag ) \ gforth
    \G true if @i{nt} is marked as compile-only.
    >f+c @ restrict-mask and 0<> ;
: ?compile-only ( nt -- nt )
    dup compile-only? IF
	<<# s"  is compile-only" holds dup name>string holds #0. #>
	hold 1- c(warning") #>>
    THEN ;
: obsolete? ( nt -- flag ) \ gforth
    \G true if @i{nt} is obsolete, i.e., will be removed in a future
    \G version of Gforth.
    >f+c @ obsolete-mask and 0<> ;
: ?obsolete ( nt -- nt )
    dup obsolete? warnings @ abs 1 > and IF
	<<# s"  is obsolete" holds dup name>string holds #0. #>
	hold 1- c(warning") #>>
    THEN ;

: name?int ( nt -- xt ) \ gforth-internal name-question-int
\G Like @code{name>interpret}, but warns when encountering a word marked
\G compile-only or obsolete
    \ opportunistic check for speed
    dup >f+c @ lit [ $C0000000. 1 cells 8 = [IF] #32 dlshift [THEN] d, ] and
    IF  ?compile-only ?obsolete  THEN
    name>interpret ;

: named>string ( nt -- addr count ) \ gforth-internal     named-to-string
    >f+c dup @ lcount-mask and tuck - swap ;
: named>link ( nt1 -- nt2 / 0 ) \ gforth-internal	named-to-link
    >link @ ;

: noname>string ( nt -- cfa 0 ) \ gforth-internal    noname-to-string
    drop 0 dup ;
: noname>link ( nt -- 0 ) \ gforth-internal    noname-to-link
    drop 0 ;

\ : name>view ( nt -- addr ) \ gforth   name-to-view
\     name>string drop cell negate and cell- ;

: (name>intn) ( nfa -- xt +-1 )
    dup name>interpret swap name>compile nip ['] execute = flag-sign ;

[IFDEF] prelude-mask
: name>prelude ( nt -- xt )
    dup >f+c @ prelude-mask and if
	[ -1 cells ] literal + @
    else
	drop ['] noop
    then ;
[THEN]

const Create ???

: hm? ( hm -- flag )
    \G check if a hm is actually one
    dup hmtemplate = IF  drop true  EXIT  THEN
    >r  hm-list
    BEGIN  @ dup  WHILE
	    dup r@ = IF  rdrop drop true  EXIT  THEN
    REPEAT  rdrop ;

: xt? ( xt -- f )
    \G check for xt - must be code field or primitive
    dup in-dictionary? IF
	dup >body dup maxaligned = IF
	    dup >namehm @ hm? IF
		dup >code-address tuck body> = swap
		docol:  ['] image-header >link @ >code-address 1+ within or  EXIT
	    THEN
	THEN
    THEN
    drop false ;

: >head-noprim ( xt -- nt ) \ gforth-internal  to-head-noprim
    dup xt? 0= IF  drop ['] ???  THEN ;

has? new-cfa [IF]
0 0 0 0 field >body ( xt -- a-addr ) \ core to-body
\G @i{a-addr} is the address of the body (aka parameter field or data
\G field) of the word represented by @i{xt}
drop drop

0 0 0 0 field body> ( xt -- a_addr )
    drop drop

: >code-address ( xt -- c_addr ) \ gforth
    \G @i{c-addr} is the code address of the word @i{xt}.
    >cfa @ ;
[ELSE]
cell% 0 0 field >body ( xt -- a_addr ) \ core to-body
\G Get the address of the body of the word represented by @i{xt} (the
\G address of the word's data field).
drop drop

cell% -1 * 0 0 field body> ( xt -- a_addr )
    drop drop

: >code-address ( xt -- c_addr ) \ gforth
    \G @i{c-addr} is the code address of the word @i{xt}.
    @ ;
[THEN]

0 0 0 0 field xt>name ( xt -- nt ) \ gforth xt-to-name
\G If @i{xt} is an execution token, produces the same @i{nt} as
\G @word{>name}.  Otherwise, @i{nt} is an arbitrary value.
    drop drop

: >does-code ( xt1 -- xt2 ) \ gforth
    \G If @i{xt1} is the execution token of a child of a
    \G @code{set-does>}-defined word, @i{xt2} is the xt passed to
    \G @code{set-does>}, i.e, the xt of the word that is executed when
    \G executing @i{xt1} (but first the body address of @i{xt1} is
    \G pushed).  If @i{xt1} does not belong to a
    \G @code{set-does>}-defined word, @i{xt2} is 0.
    dup >namehm @ >hmextra @ swap >cfa @ dodoes: = and ;
\    dup >code-address dodoes: = if
\	>extra @
\    else
\	drop 0
\    then ;

: only-code-address! ( c_addr xt -- )
    \ like code-address!, but does not change opt-compile,
    >cfa ! ;

: any-code! ( xt2 xt1 code-addr -- )
    \ for implementing DOES> and ;ABI-CODE: code-address is
    \ stored at cfa of xt1, xt2 at >hmextra; set-optimizer is called in
    \ the caller.
    swap make-latest
    latestnt only-code-address!
    0 >hmextra hm! ;

\ ticks in interpreter

: '-error ( nt translate-name | ... translate-some -- nt | never ) \ gforth-internal
    \G check if there is a name token on the stack, return the corresponding
    \G @var{nt} or throw @code{wrong argument name} if not.
    ?found translate-name? 0= #-32 and throw ;

: (') ( "name" -- nt ) \ gforth-internal
    parse-name name-too-short? rec-forth '-error ;

: '    ( "name" -- xt ) \ core	tick
    \g @i{xt} represents @i{name}'s interpretation semantics.
    (') name?int ;

\ \ the interpreter loop				  mar92py

\ interpret                                            10mar92py

Defer parse-name ( "name" -- c-addr u ) \ core-ext
\G Get the next word from the input buffer
' (name) IS parse-name

Defer before-word ( -- ) \ gforth
\G Deferred word called before the text interpreter parses the next word
' noop IS before-word

Defer before-line ( -- ) \ gforth
\G Deferred word called before the text interpreter parses the next line
' noop IS before-line

defer int-execute ( ... xt -- ... )
\ like EXECUTE, but restores and saves ERRNO if present
' execute IS int-execute

: interpret ( ... -- ... ) \ gforth-internal
    \ interpret/compile the (rest of the) input buffer
    [ cell 4 = [IF] ] false >l [ [THEN] ] \ align LP stack for 32 bit engine
    r> >l rp@ backtrace-rp0 !
    [ has? EC 0= [IF] ] before-line [ [THEN] ]
    BEGIN
	?stack [ has? EC 0= [IF] ] before-word [ [THEN] ] parse-name dup
    WHILE
	rec-forth ?rec-found execute
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
    \g Copy the memory block @i{addr u} to @i{addr2}, which is the
    \g start of a newly heap allocated @i{u}-byte region.
    swap >r
    dup dfaligned allocate throw
    swap 2dup r> -rot move ;

\ \ Quit                                            	13feb93py

Defer 'quit
Defer .status
defer prompt

: (prompt) ( -- )
    ."  ok" ;
' (prompt) is prompt

: (quit1) ( -- )
    \ exits only through THROW etc.
    BEGIN
	[ has? ec [IF] ] cr [ [ELSE] ]
	    ['] cr catch if
	    [ has? OS [IF] ] >stderr [ [THEN] ]
	    cr ." Can't print to stdout, leaving" cr
	    \ if stderr does not work either, already DoError causes a hang
	    -2 (bye)
	endif [ [THEN] ]
    .status get-input-colored WHILE
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

: dec.r ( u n -- ) \ gforth
    \G Display @i{u} as a unsigned decimal number in a field @i{n}
    \G characters wide.
    base @ >r decimal .r r> base ! ;

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
  IF 	abort-string @ ?dup-IF count type  THEN drop
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
    over umin third over type /string ;

: .error-line ( c-addr1 u1 c-addr2 u2 -- )
    \ print error in line c-addr1 u1, where the error-causing lexeme
    \ is c-addr2 u2
    >r third - part-type ( c-addr3 u3 R: u2 )
    mark-start r> part-type mark-end ( c-addr4 u4 )
    type ;

Defer .error-level ( n -- )
: (.error-level) dup
    >r 2 = IF  ." error: "    THEN
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

Variable rec-level

: (DoError) ( throw-code -- )
    dup -1 = IF  drop EXIT  THEN \ -1 is abort, no error message!
    [ has? os [IF] ]
	>stderr error-color
	[ [THEN] ]
    input-error-data 2 .error-frame
    error-stack $@len 0 ?DO
	error>
	2 .error-frame
    /error +LOOP
    drop
    dobacktrace
    default-color  endif? on  rec-level off
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

: do-execute ( xt -- ) \ gforth-internal
    \G C calling us
    execute ( catch dup IF  DoError cr  THEN ) -56 (bye) ;

: do-find ( addr u -- )
    find-name dup IF  name>interpret  THEN  (bye) ;

\ \ Cold Boot                                    	13feb93py

: (c) ( -- )
    ." Copyright " $A9 ( '©' ) xemit ;
: gforth ( -- )
    ." Gforth " version-string type cr
    ." Authors: Anton Ertl, Bernd Paysan, Jens Wilke et al., for more type `authors'" cr
    (c) ."  2025 Free Software Foundation, Inc." cr
    ." License GPLv3+: GNU GPL version 3 or later <https://gnu.org/licenses/gpl.html>" cr
    ." Gforth comes with ABSOLUTELY NO WARRANTY; for details type `license'"
[ has? os [IF] ]
     cr ." Type `help' for basic help"
[ [THEN] ] ;

defer bootmessage ( -- ) \ gforth
\G Hook (deferred word) executed right after interpreting the OS
\G command-line arguments.  Normally prints the Gforth startup
\G message.

' gforth IS bootmessage

Defer 'cold ( -- ) \ gforth  tick-cold
\G Hook (deferred word) for things to do right before interpreting the
\G OS command-line arguments.  Normally does some initializations that
\G you also want to perform.

: cold ( -- ) \ gforth-internal
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

: xt, ( xt -- )
    lits, here xt-location drop , ;

: boot ( path n **argv argc -- )
    threading-method 1 = if
	['] xt, is compile,
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
    ['] cold catch dup -&2049 <> over -1 <> and if \ broken pipe?
	dup >r DoError cr r>
    endif
    (bye) \ determin exit code from throw code
[ [ELSE] ]
    cold
[ [THEN] ]
;

has? os [IF]
Defer bye ( -- ) \ tools-ext
\G Exit Gforth (with exit status 0).    
: kernel-bye ( -- ) \ gforth-internal
[ has? file [IF] ]
    script? 0= IF  .unstatus cr  THEN
[ [ELSE] ]
    cr
[ [THEN] ]
    0 (bye) ;
' kernel-bye is bye
[THEN]

\ **argv may be scanned by the C starter to get some important
\ information, as -display and -geometry for an X client FORTH
\ or space and stackspace overrides

\ 0 arg contains, however, the name of the program.

