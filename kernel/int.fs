\ definitions needed for interpreter only

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

\ !! protect BASE saving wrapper against exceptions
: getbase ( addr u -- addr' u' )
    over c@ [char] $ - dup 4 u<
    IF
	cells bases + @ base ! 1 /string
    ELSE
	drop
    THEN ;

: sign? ( addr u -- addr u flag )
    over c@ '- =  dup >r
    IF
	1 /string
    THEN
    r> ;

: s>unumber? ( addr u -- ud flag )
    base @ >r  dpl on  getbase
    0. 2swap
    BEGIN ( d addr len )
	dup >r >number dup
    WHILE \ there are characters left
	dup r> -
    WHILE \ the last >number parsed something
	dup 1- dpl ! over c@ [char] . =
    WHILE \ the current char is '.'
	1 /string
    REPEAT  THEN \ there are unparseable characters left
	2drop false
    ELSE
	rdrop 2drop true
    THEN
    r> base ! ;

\ ouch, this is complicated; there must be a simpler way - anton
: s>number? ( addr len -- d f )
    \ converts string addr len into d, flag indicates success
    sign? >r
    s>unumber?
    0= IF
        rdrop false
    ELSE \ no characters left, all ok
	r>
	IF
	    dnegate
	THEN
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
    \G ** this will not get annotated. The alias in glocals.fs will instead **
    [char] ) parse 2drop ; immediate

: \ ( -- ) \ core-ext,block-ext backslash
    \G ** this will not get annotated. The alias in glocals.fs will instead **
    [ has? file [IF] ]
    blk @
    IF
	>in @ c/l / 1+ c/l * >in !
	EXIT
    THEN
    [ [THEN] ]
    source >in ! drop ; immediate

: \G ( -- ) \ gforth backslash-gee
    \G Equivalent to @code{\} but used as a tag to annotate definition
    \G comments into documentation.
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
  cell% field wordlist-map \ pointer to a wordlist-map-struct
  cell% field wordlist-id \ linked list of words (for WORDS etc.)
  cell% field wordlist-link \ link field to other wordlists
  cell% field wordlist-extend \ wordlist extensions (eg bucket offset)
end-struct wordlist-struct

: f83find      ( addr len wordlist -- nt / false )
    wordlist-id @ (f83find) ;

: initvoc		( wid -- )
  dup wordlist-map @ hash-method perform ;

\ Search list table: find reveal
Create f83search ( -- wordlist-map )
    ' f83find A,  ' drop A,  ' drop A, ' drop A,

here G f83search T A, NIL A, NIL A, NIL A,
AValue forth-wordlist \ variable, will be redefined by search.fs

AVariable lookup       	forth-wordlist lookup !
\ !! last is user and lookup?! jaw
AVariable current ( -- addr ) \ gforth
\G VARIABLE: holds the wid of the current compilation word list.
AVariable voclink	forth-wordlist wordlist-link voclink !
lookup AValue context ( -- addr ) \ gforth
\G VALUE: @code{context} @code{@@} is the wid of the word list at the
\G top of the search order stack.

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

: head? ( addr -- f )
    \G heuristic check whether addr is a name token; may deliver false
    \G positives; addr must be a valid address
    \ we follow the link fields and check for plausibility; two
    \ iterations should catch most false addresses: on the first
    \ iteration, we may get an xt, on the second a code address (or
    \ some code), which is typically not in the dictionary.
    2 0 do
	dup @ dup
	if ( addr addr1 )
	    dup rot forthstart within
	    if \ addr1 is outside forthstart..addr, not a head
		drop false unloop exit
	    then ( addr1 )
	else \ 0 in the link field, no further checks
	    2drop true unloop exit
	then
    loop
    \ in dubio pro:
    drop true ;

const Create ???  0 , 3 c, char ? c, char ? c, char ? c,
\ ??? is used by dovar:, must be created/:dovar

: >head ( cfa -- nt ) \ gforth  to-head
    $21 cell do ( cfa )
	dup i - count $9F and + cfaligned over alias-mask + =
	if ( cfa )
	    dup i - cell - dup head?
	    if
		nip unloop exit
	    then
	    drop
	then
	cell +loop
    drop ??? ( wouldn't 0 be better? ) ;

' >head ALIAS >name

: body> 0 >body - ;

: (search-wordlist)  ( addr count wid -- nt / false )
    dup wordlist-map @ find-method perform ;

: search-wordlist ( c-addr count wid -- 0 / xt +-1 ) \ search
    \G Search the word list identified by wid
    \G for the definition named by the string at c-addr count.
    \G If the definition is not found, return 0. If the definition
    \G is found return 1 (if the definition is immediate) or -1
    \G (if the definition is not immediate) together with the xt.
    \G The xt returned represents the interpretation semantics.
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
    \G Search all word lists in the current search order
    \G for the definition named by the counted string at c-addr.
    \G If the definition is not found, return 0. If the definition
    \G is found return 1 (if the definition is immediate) or -1
    \G (if the definition is not immediate) together with the xt.
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
    rp@ backtrace-rp0 !
    BEGIN
	?stack name dup
    WHILE
	parser
    REPEAT
    2drop ;

\ interpreter                                 	30apr92py

\ not the most efficient implementations of interpreter and compiler
| : interpreter ( c-addr u -- ) 
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
: sourceline# ( -- n )  1 ;
[THEN]

: refill ( -- flag ) \ core-ext,block-ext,file-ext
    [ has? file [IF] ]
	blk @  IF  1 blk +!  true  0 >in !  EXIT  THEN
	[ [THEN] ]
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
	1 loadline +!
	[ [THEN] ]
    swap #tib ! 0 >in ! ;

: query   ( -- ) \ core-ext
    \G obsolescent
    [ has? file [IF] ]
	blk off loadfile off
	[ [THEN] ]
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
  tibstack @ >r  >tib @ >r  #tib @ >r
  >tib @ tibstack @ = IF  r@ tibstack +!  THEN
  tibstack @ >tib ! >in @ >r  >r ;

: pop-file   ( throw-code -- throw-code )
  r>
  r> >in !  r> #tib !  r> >tib !  r> tibstack !  >r ;
[THEN]

: evaluate ( c-addr len -- ) \ core,block
  push-file  #tib ! >tib !
  >in off
  [ has? file [IF] ]
      blk off loadfile off -1 loadline !
      [ [THEN] ]
  ['] interpret catch
  pop-file throw ;

\ \ Quit                                            	13feb93py

Defer 'quit

Defer .status

: prompt        state @ IF ."  compiled" EXIT THEN ."  ok" ;

: (Query)  ( -- )
    [ has? file [IF] ]
	loadfile off  blk off loadline off
	[ [THEN] ]
    refill drop ;

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
    \G Display n as a signed decimal number, followed by a space.
    \G !! not used...
    base @ decimal swap . base ! ;

: dec.r ( u -- ) \ gforth
    \G Display u as a unsigned decimal number
    base @ decimal swap 0 .r base ! ;

: hex. ( u -- ) \ gforth
    \G Display u as an unsigned hex number, prefixed with a "$" and
    \G followed by a space.
    \G !! not used...
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
Defer dobacktrace ( -- )
' noop IS dobacktrace

: .error-string ( throw-code -- )
  dup -2 = 
  IF 	"error @ ?dup IF count type  THEN drop
  ELSE	.error
  THEN ;

: .error-frame ( throwcode addr1 u1 n1 n2 addr2 u2 -- throwcode )
\ addr2 u2: 	filename of included file
\ n2:		line number
\ n1:		error position in input line
\ addr1 u1:	input line

  cr error-stack @
  IF
     ." in file included from "
     type ." :" dec.r  drop 2drop
  ELSE
     type ." :" dec.r ." : " 3 pick .error-string cr
     dup 2over type cr drop
     nip -trailing 1- ( line-start index2 )
     0 >r  BEGIN
                  2dup + c@ bl >  WHILE
		  r> 1+ >r  1- dup 0<  UNTIL  THEN  1+
     ( line-start index1 )
     typewhite
     r> 1 max 0 ?do \ we want at least one "^", even if the length is 0
                  [char] ^ emit
     loop
  THEN ;

: (DoError) ( throw-code -- )
  [ has? os [IF] ]
      >stderr
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
  dobacktrace
  normal-dp dpp ! ;

' (DoError) IS DoError

: quit ( ?? -- ?? ) \ core
    rp0 @ rp! handler off clear-tibstack >tib @ >r
    BEGIN
	[ has? compiler [IF] ]
	postpone [
	[ [THEN] ]
	['] 'quit CATCH dup
    WHILE
	<# \ reset hold area, or we may get another error
	DoError r@ >tib ! r@ tibstack !
    REPEAT
    drop r> >tib ! ;

\ \ Cold Boot                                    	13feb93py

: (bootmessage)
    ." GForth " version-string type 
    ." , Copyright (C) 1998 Free Software Foundation, Inc." cr
    ." GForth comes with ABSOLUTELY NO WARRANTY; for details type `license'"
[ has? os [IF] ]
     cr ." Type `bye' to exit"
[ [THEN] ] ;

defer bootmessage
defer process-args

' (bootmessage) IS bootmessage

Defer 'cold ( -- ) \ gforth  tick-cold
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
    process-args
    loadline off
[ [THEN] ]
    bootmessage
    quit ;

: clear-tibstack ( -- )
[ has? glocals [IF] ]
    lp@ forthstart 7 cells + @ - 
[ [ELSE] ]
    [ has? os [IF] ]
    r0 @ forthstart 6 cells + @ -
    [ [ELSE] ]
    sp@ $10 cells +
    [ [THEN] ]
[ [THEN] ]
    dup >tib ! tibstack ! #tib off >in off ;

: boot ( path **argv argc -- )
    main-task up!
[ has? os [IF] ]
    stdout TO outfile-id
    stdin  TO infile-id
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
    ['] cold catch DoError cr
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

