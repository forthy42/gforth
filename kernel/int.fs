\ definitions needed for interpreter only

\ Copyright (C) 1995-2000 Free Software Foundation, Inc.

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

require ./basics.fs 	\ bounds decimal hex ...
require ./io.fs		\ type ...
require ./nio.fs	\ . <# ...
require ./errore.fs	\ .error ...
require kernel/version.fs	\ version-string
require ./../chains.fs

: tib ( -- c-addr ) \ core-ext t-i-b
    \G @i{c-addr} is the address of the Terminal Input Buffer.
    \G OBSOLESCENT: @code{source} superceeds the function of this word.
    >tib @ ;

Defer source ( -- c-addr u ) \ core
\ used by dodefer:, must be defer
\G @i{c-addr} is the address of the input buffer and @i{u} is the
\G number of characters in it.

: (source) ( -- c-addr u )
    tib #tib @ ;
' (source) IS source

: (word) ( addr1 n1 char -- addr2 n2 )
  dup >r skip 2dup r> scan  nip - ;

\ (word) should fold white spaces
\ this is what (parse-white) does

\ word parse                                           23feb93py

: sword  ( char -- addr len ) \ gforth s-word
    \G Parses like @code{word}, but the output is like @code{parse} output.
    \G @xref{core-idef}.
  \ this word was called PARSE-WORD until 0.3.0, but Open Firmware and
  \ dpANS6 A.6.2.2008 have a word with that name that behaves
  \ differently (like NAME).
  source 2dup >r >r >in @ over min /string
  rot dup bl = IF  drop (parse-white)  ELSE  (word)  THEN
  2dup + r> - 1+ r> min >in ! ;

: word   ( char "<chars>ccc<char>-- c-addr ) \ core
    \G Skip leading delimiters. Parse @i{ccc}, delimited by
    \G @i{char}, in the parse area. @i{c-addr} is the address of a
    \G transient region containing the parsed string in
    \G counted-string format. If the parse area was empty or
    \G contained no characters other than delimiters, the resulting
    \G string has zero length. A program may replace characters within
    \G the counted string. OBSOLESCENT: the counted string has a
    \G trailing space that is not included in its length.
    sword here place  bl here count + c!  here ;

: parse    ( char "ccc<char>" -- c-addr u ) \ core-ext
    \G Parse @i{ccc}, delimited by @i{char}, in the parse
    \G area. @i{c-addr u} specifies the parsed string within the
    \G parse area. If the parse area was empty, @i{u} is 0.
    >r  source  >in @ over min /string  over  swap r>  scan >r
  over - dup r> IF 1+ THEN  >in +! ;

\ name                                                 13feb93py

[IFUNDEF] (name) \ name might be a primitive

: (name) ( -- c-addr count ) \ gforth
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
    over c@ [char] - =  dup >r
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

: ( ( compilation 'ccc<close-paren>' -- ; run-time -- ) \ thisone- core,file	paren
    \G ** this will not get annotated. The alias in glocals.fs will instead **
    \G It does not work to use "wordset-" prefix since this file is glossed
    \G by cross.fs which doesn't have the same functionalty as makedoc.fs
    [char] ) parse 2drop ; immediate

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
\G @code{Variable} -- holds the @i{wid} of the compilation word list.
AVariable voclink	forth-wordlist wordlist-link voclink !
\ lookup AValue context ( -- addr ) \ gforth
Defer context ( -- addr ) \ gforth
\G @code{context} @code{@@} is the @i{wid} of the word list at the
\G top of the search order.

' lookup is context
forth-wordlist current !

\ \ header, finding, ticks                              17dec92py

hex
80 constant alias-mask \ set when the word is not an alias!
40 constant immediate-mask
20 constant restrict-mask

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
    \g @i{addr count} is the name of the word represented by @i{nt}.
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
    \G @i{xt} represents the interpretation semantics of the word
    \G @i{nt}. If @i{nt} has no interpretation semantics (i.e. is
    \G @code{compile-only}), @i{xt} is the execution token for
    \G @code{compile-only-error}, which performs @code{-14 throw}.
    (name>x) (x>int) ;

: name?int ( nt -- xt ) \ gforth
    \G Like @code{name>int}, but perform @code{-14 throw} if @i{nt}
    \G has no interpretation semantics.
    (name>x) restrict-mask and
    if
	compile-only-error \ does not return
    then
    (cfa>int) ;

: (name>comp) ( nt -- w +-1 ) \ gforth
    \G @i{w xt} is the compilation token for the word @i{nt}.
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

[IFDEF] forthstart
\ if we have a forthstart we can define head? with it
\ otherwise leave out the head? check

: head? ( addr -- f )
    \G heuristic check whether addr is a name token; may deliver false
    \G positives; addr must be a valid address
    \ we follow the link fields and check for plausibility; two
    \ iterations should catch most false addresses: on the first
    \ iteration, we may get an xt, on the second a code address (or
    \ some code), which is typically not in the dictionary.
    2 0 do
	dup dup aligned <> if \ protect @ against unaligned accesses
	    drop false unloop exit
	then
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

: >head-noprim ( cfa -- nt ) \ gforth  to-head-noprim
    $25 cell do ( cfa )
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

[ELSE]

: >head-noprim ( cfa -- nt ) \ gforth  to-head-noprim
    $25 cell do ( cfa )
	dup i - count $9F and + cfaligned over alias-mask + =
	if ( cfa ) i - cell - unloop exit
	then
	cell +loop
    drop ??? ( wouldn't 0 be better? ) ;

[THEN]

: body> 0 >body - ;

: (search-wordlist)  ( addr count wid -- nt | false )
    dup wordlist-map @ find-method perform ;

: search-wordlist ( c-addr count wid -- 0 | xt +-1 ) \ search
    \G Search the word list identified by @i{wid} for the definition
    \G named by the string at @i{c-addr count}.  If the definition is
    \G not found, return 0. If the definition is found return 1 (if
    \G the definition is immediate) or -1 (if the definition is not
    \G immediate) together with the @i{xt}.  In Gforth, the @i{xt}
    \G returned represents the interpretation semantics.  ANS Forth
    \G does not specify clearly what @i{xt} represents.
    (search-wordlist) dup if
	(name>intn)
    then ;

: find-name ( c-addr u -- nt | 0 ) \ gforth
    \g Find the name @i{c-addr u} in the current search
    \g order. Return its @i{nt}, if found, otherwise 0.
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

: find ( c-addr -- xt +-1 | c-addr 0 ) \ core,search
    \G Search all word lists in the current search order for the
    \G definition named by the counted string at @i{c-addr}.  If the
    \G definition is not found, return 0. If the definition is found
    \G return 1 (if the definition has non-default compilation
    \G semantics) or -1 (if the definition has default compilation
    \G semantics).  The @i{xt} returned in interpret state represents
    \G the interpretation semantics.  The @i{xt} returned in compile
    \G state represented either the compilation semantics (for
    \G non-default compilation semantics) or the run-time semantics
    \G that the compilation semantics would @code{compile,} (for
    \G default compilation semantics).  The ANS Forth standard does
    \G not specify clearly what the returned @i{xt} represents (and
    \G also talks about immediacy instead of non-default compilation
    \G semantics), so this word is questionable in portable programs.
    \G If non-portability is ok, @code{find-name} and friends are
    \G better (@pxref{Name token}).
    dup count sfind dup
    if
	rot drop
    then ;

\ ticks in interpreter

: (') ( "name" -- nt ) \ gforth
    name name-too-short?
    find-name dup 0=
    IF
	drop -&13 throw
    THEN  ;

: '    ( "name" -- xt ) \ core	tick
    \g @i{xt} represents @i{name}'s interpretation
    \g semantics. Perform @code{-14 throw} if the word has no
    \g interpretation semantics.
    (') name?int ;

has? compiler 0= [IF]	\ interpreter only version of IS and TO

: IS ' >body ! ;
' IS Alias TO

[THEN]

\ \ the interpreter loop				  mar92py

\ interpret                                            10mar92py

Defer parser ( c-addr u -- )
Defer name ( -- c-addr count ) \ gforth
\G Get the next word from the input buffer
' (name) IS name
Defer compiler-notfound ( c-addr count -- )
Defer interpreter-notfound ( c-addr count -- )

: no.extensions  ( addr u -- )
    2drop -&13 throw ;
' no.extensions IS compiler-notfound
' no.extensions IS interpreter-notfound

: interpret ( ?? -- ?? ) \ gforth
    \ interpret/compile the (rest of the) input buffer
[ has? backtrace [IF] ]
    rp@ backtrace-rp0 !
[ [THEN] ]
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
: sourceline# ( -- n )  1 ;
[THEN]

Variable #fill-bytes
\G number of bytes read via (read-line) by the last refill

: refill ( -- flag ) \ core-ext,block-ext,file-ext
    \G Attempt to fill the input buffer from the input source.  When
    \G the input source is the user input device, attempt to receive
    \G input into the terminal input device. If successful, make the
    \G result the input buffer, set @code{>IN} to 0 and return true;
    \G otherwise return false. When the input source is a block, add 1
    \G to the value of @code{BLK} to make the next block the input
    \G source and current input buffer, and set @code{>IN} to 0;
    \G return true if the new value of @code{BLK} is a valid block
    \G number, false otherwise. When the input source is a text file,
    \G attempt to read the next line from the file. If successful,
    \G make the result the current input buffer, set @code{>IN} to 0
    \G and return true; otherwise, return false.  A successful result
    \G includes receipt of a line containing 0 characters.
    [ has? file [IF] ]
	blk @  IF  1 blk +!  true  0 >in !  EXIT  THEN
	[ [THEN] ]
    tib /line
    [ has? file [IF] ]
	loadfile @ ?dup
	IF    (read-line) #fill-bytes ! throw
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
    \G Make the user input device the input source. Receive input into
    \G the Terminal Input Buffer. Set @code{>IN} to zero. OBSOLESCENT:
    \G superceeded by @code{accept}.
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

: evaluate ( c-addr u -- ) \ core,block
    \G Save the current input source specification. Store @code{-1} in
    \G @code{source-id} and @code{0} in @code{blk}. Set @code{>IN} to
    \G @code{0} and make the string @i{c-addr u} the input source
    \G and input buffer. Interpret. When the parse area is empty,
    \G restore the input source specification.
    loadfilename# @ >r
    1 loadfilename# ! \ "\evaluated string/"
    push-file #tib ! >tib !
    >in off
    [ has? file [IF] ]
	blk off loadfile off -1 loadline !
	[ [THEN] ]
    ['] interpret catch
    pop-file
    r> loadfilename# !
    throw ;

\ \ Quit                                            	13feb93py

Defer 'quit

Defer .status

: prompt        state @ IF ."  compiled" EXIT THEN ."  ok" ;

: (Query)  ( -- )
    [ has? file [IF] ]
	loadfile off  blk off loadline off
	[ [THEN] ]
    refill drop ;

: (quit) ( -- )
    \ exits only through THROW etc.
\    sp0 @ cell - handler @ &12 + ! \ !! kludge: fix the stack pointer
    \ stored in the system's CATCH frame, so the stack depth will be 0
    \ after the next THROW it catches (it may be off due to BOUNCEs or
    \ because process-args left something on the stack)
    BEGIN
	.status cr (query) interpret prompt
    AGAIN ;

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
    \G Display @i{n} as a signed decimal number, followed by a space.
    \ !! not used...
    base @ decimal swap . base ! ;

: dec.r ( u -- ) \ gforth
    \G Display @i{u} as a unsigned decimal number
    base @ decimal swap 0 .r base ! ;

: hex. ( u -- ) \ gforth
    \G Display @i{u} as an unsigned hex number, prefixed with a "$" and
    \G followed by a space.
    \ !! not used...
    [char] $ emit base @ swap hex u. base ! ;

: typewhite ( addr u -- ) \ gforth
    \G Like type, but white space is printed instead of the characters.
    bounds ?do
	i c@ #tab = if \ check for tab
	    #tab
	else
	    bl
	then
	emit
    loop ;

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
      type ." :" dup >r dec.r ." : " 3 pick .error-string
      r> IF \ if line# non-zero, there is a line
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
      ELSE
	  2drop drop
      THEN
  THEN ;

: (DoError) ( throw-code -- )
  [ has? os [IF] ]
      >stderr
  [ [THEN] ] 
  source >in @ sourceline# sourcefilename .error-frame
  error-stack @ 0 ?DO
    -1 error-stack +!
    error-stack dup @ 6 * cells + cell+
    6 cells bounds DO
      I @
    cell +LOOP
    .error-frame
  LOOP
  drop 
[ has? backtrace [IF] ]
  dobacktrace
[ [THEN] ]
  normal-dp dpp ! ;

' (DoError) IS DoError

: quit ( ?? -- ?? ) \ core
    \G Empty the return stack, make the user input device
    \G the input source, enter interpret state and start
    \G the text interpreter.
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
    ." , Copyright (C) 1995-2000 Free Software Foundation, Inc." cr
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


Variable init8

: cold ( -- ) \ gforth
[ has? backtrace [IF] ]
    rp@ backtrace-rp0 !
[ [THEN] ]
[ has? file [IF] ]
    pathstring 2@ fpath only-path 
    init-included-files
[ [THEN] ]
    'cold
    init8 chainperform
[ has? file [IF] ]
    process-args
    loadline off
    loadfilename# off
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
    handler off
    ['] cold catch DoError cr
[ has? os [IF] ]
    1 (bye) \ !! determin exit code from throw code?
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

