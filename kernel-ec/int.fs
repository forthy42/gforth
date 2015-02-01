\ definitions needed for interpreter only

\ Copyright (C) 1995-2000,2004,2005,2007,2009,2010,2012,2013,2014 Free Software Foundation, Inc.

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

require ./basics.fs 	\ bounds decimal hex ...
require ./io.fs		\ type ...
require ./nio.fs	\ . <# ...
require ./errore.fs	\ .error ...
require kernel/version.fs \ version-string

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

\ parse                                           23feb93py

: parse    ( char "ccc<char>" -- c-addr u ) \ core-ext
\G Parse @i{ccc}, delimited by @i{char}, in the parse
\G area. @i{c-addr u} specifies the parsed string within the
\G parse area. If the parse area was empty, @i{u} is 0.
    >r  source  >in @ over min /string ( c-addr1 u1 )
    over  swap r>  scan >r
    over - dup r> IF 1+ THEN  >in +! ;

\ name                                                 13feb93py

[IFUNDEF] (name) \ name might be a primitive

: (name) ( -- c-addr count ) \ gforth
    source 2dup >r >r >in @ /string (parse-white)
[ has? new-input [IF] ]
    2dup input-lexeme!
[ [THEN] ]
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

\ !! protect BASE saving wrapper against exceptions
: getbase ( addr u -- addr' u' )
    over c@ [char] # - dup 4 u<
    IF
	cells bases + @ base ! 1 /string
    ELSE
	drop
    THEN ;

: sign? ( addr u -- addr1 u1 flag )
    over c@ [char] - =  dup >r
    IF
	1 /string
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
    dpl on
    over c@ '' = if
	1 /string s'>unumber? exit
    endif
    base @ >r  getbase sign?
    over if
        >r 0. 2swap
        BEGIN ( d addr len )
            dup >r >number dup
        WHILE \ there are characters left
                dup r> -
            WHILE \ the last >number parsed something
                    dup 1- dpl ! over c@ dp-char @ =
                WHILE \ the current char is '.'
                        1 /string
                REPEAT  THEN \ there are unparseable characters left
            2drop rdrop false
        ELSE
            rdrop 2drop r> ?dnegate true
        THEN
    ELSE
        drop 2drop 0. false THEN
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
    (number?) ?dup 0= abort" ?"  0<
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

AVariable forth-wordlist
: find-name ( c-addr u -- nt | 0 ) \ gforth
    \g Find the name @i{c-addr u} in the current search
    \g order. Return its @i{nt}, if found, otherwise 0.
    forth-wordlist (f83find) ;

\ \ header, finding, ticks                              17dec92py

\ The constants are defined as 32 bits, but then erased
\ and overwritten by the right ones

\ to save space, Gforth EC limits words to 31 characters
\ also, there's no predule concept in Gforth EC
$80 constant alias-mask
$40 constant immediate-mask
$20 constant restrict-mask
$1f constant lcount-mask

\ higher level parts of find

: flag-sign ( f -- 1|-1 )
    \ true becomes 1, false -1
    0= 2* 1+ ;

: ticking-compile-only-error ( ... -- )
    -&2048 throw ;

: compile-only-error ( ... -- )
    -&14 throw ;

: >does-code ( xt -- a_addr ) \ gforth
\G If @i{xt} is the execution token of a child of a @code{DOES>} word,
\G @i{a-addr} is the start of the Forth code after the @code{DOES>};
\G Otherwise @i{a-addr} is 0.
    dup @ dodoes: = if
	cell+ @
    else
	drop 0
    endif ;

: (x>int) ( cfa w -- xt )
    \ get interpretation semantics of name
    restrict-mask and [ has? rom [IF] ] 0= [ [THEN] ]
    if
	drop ['] compile-only-error
    then ;

: name>string ( nt -- addr count ) \ gforth     name-to-string
    \g @i{addr count} is the name of the word represented by @i{nt}.
    cell+ count lcount-mask and ;

: ((name>))  ( nfa -- cfa )
    name>string + cfaligned ;

: (name>x) ( nfa -- cfa w )
    \ cfa is an intermediate cfa and w is the flags cell of nfa
    dup ((name>))
    swap cell+ c@ dup alias-mask and 0=
    IF
        swap @ swap
    THEN ;

: name>int ( nt -- xt ) \ gforth name-to-int
    \G @i{xt} represents the interpretation semantics of the word
    \G @i{nt}. If @i{nt} has no interpretation semantics (i.e. is
    \G @code{compile-only}), @i{xt} is the execution token for
    \G @code{ticking-compile-only-error}, which performs @code{-2048 throw}.
    (name>x) (x>int) ;

: name?int ( nt -- xt ) \ gforth name-question-int
    \G Like @code{name>int}, but perform @code{-2048 throw} if @i{nt}
    \G has no interpretation semantics.
    (name>x) restrict-mask and [ has? rom [IF] ] 0= [ [THEN] ]
    if
	ticking-compile-only-error \ does not return
    then ;

: (name>comp) ( nt -- w +-1 ) \ gforth
    \G @i{w xt} is the compilation token for the word @i{nt}.
    (name>x) >r 
    r> immediate-mask and [ has? rom [IF] ] 0= [ [THEN] ] flag-sign
    ;

: (name>intn) ( nfa -- xt +-1 )
    (name>x) tuck (x>int) ( w xt )
    swap immediate-mask and [ has? rom [IF] ] 0= [ [THEN] ] flag-sign ;

[IFDEF] prelude-mask
: name>prelude ( nt -- xt )
    dup cell+ @ prelude-mask and if
	[ -1 cells ] literal + @
    else
	drop ['] noop
    then ;
[THEN]

const Create ???  0 , 3 , char ? c, char ? c, char ? c,
\ ??? is used by dovar:, must be created/:dovar

: >head-noprim ( cfa -- nt ) \ gforth  to-head-noprim
    $25 cell do ( cfa )
	dup i - dup @ [ alias-mask lcount-mask or ] literal
	[ 1 bits/char 3 - lshift 1 - 1 bits/char 1 - lshift or
	-1 cells allot bigendian [IF]   c, -1 1 cells 1- times
	[ELSE] -1 1 cells 1- times c, [THEN] ]
	and ( cfa len|alias )
	swap + cell + cfaligned over alias-mask + =
	if ( cfa ) i - cell - unloop exit
	then
	cell +loop
    drop ??? ( wouldn't 0 be better? ) ;

cell% 2* 0 0 field >body ( xt -- a_addr ) \ core to-body
\G Get the address of the body of the word represented by @i{xt} (the
\G address of the word's data field).
drop drop

cell% -2 * 0 0 field body> ( xt -- a_addr )
    drop drop

has? standardthreading has? compiler and [IF]

' @ alias >code-address ( xt -- c_addr ) \ gforth
\G @i{c-addr} is the code address of the word @i{xt}.

has? prims [IF]
    : flash! ! ;
    : flashc! c! ;
[THEN]

has? flash [IF] ' flash! [ELSE] ' ! [THEN]
alias code-address! ( c_addr xt -- ) \ gforth
\G Create a code field with code address @i{c-addr} at @i{xt}.

: any-code! ( a-addr cfa code-addr -- )
    \ for implementing DOES> and ;ABI-CODE, maybe :
    \ code-address is stored at cfa, a-addr at cfa+cell
    over ! cell+ ! ;
    
: does-code! ( a-addr xt -- ) \ gforth
\G Create a code field at @i{xt} for a child of a @code{DOES>}-word;
\G @i{a-addr} is the start of the Forth code after @code{DOES>}.
    [ has? flash [IF] ]
    dodoes: over flash! cell+ flash!
    [ [ELSE] ]
    dodoes: any-code! 
    [ [THEN] ] ;

2 cells constant /does-handler ( -- n ) \ gforth
\G The size of a @code{DOES>}-handler (includes possible padding).

[THEN]	

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
    parse-name name-too-short?
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

Defer parser1 ( c-addr u -- ... xt)
\ "... xt" is the action to be performed by the text-interpretation of c-addr u

: parser ( c-addr u -- ... )
\ text-interpret the word/number c-addr u, possibly producing a number
    parser1 execute ;

' (name) Alias parse-name
: no.extensions  2drop -&13 throw ;
' no.extensions Alias compiler-notfound1
' no.extensions Alias interpreter-notfound1

: interpret ( ... -- ... )
    BEGIN
	?stack parse-name dup
    WHILE
	parser1 execute
    REPEAT
    2drop ;

\ interpreter                                 	30apr92py

\ not the most efficient implementations of interpreter and compiler
: interpreter1 ( c-addr u -- ... xt ) 
    2dup find-name [ [IFDEF] prelude-mask ] run-prelude [ [THEN] ] dup
    if
	nip nip name>int
    else
	drop
	2dup 2>r snumber?
	IF
	    2rdrop ['] noop
	ELSE
	    2r> interpreter-notfound1
	THEN
    then ;

' interpreter1  IS  parser1

\ \ Query Evaluate                                 	07apr93py

: sourceline# ( -- n )  1 ;

: input-start-line ( -- )  >in off ;
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
    tib /line swap #tib !
    input-start-line ;

: query   ( -- ) \ core-ext
    \G Make the user input device the input source. Receive input into
    \G the Terminal Input Buffer. Set @code{>IN} to zero. OBSOLESCENT:
    \G superceeded by @code{accept}.
    refill drop ;

\ EVALUATE                                              17may93jaw

: push-file  ( -- )  r>
  tibstack @ >r  >tib @ >r  #tib @ >r
  >tib @ tibstack @ = IF  r@ tibstack +!  THEN
  tibstack @ >tib ! >in @ >r  >r ;

: pop-file   ( throw-code -- throw-code )
  r>
  r> >in !  r> #tib !  r> >tib !  r> tibstack !  >r ;

: evaluate ( c-addr u -- ) \ core,block
    \G Save the current input source specification. Store @code{-1} in
    \G @code{source-id} and @code{0} in @code{blk}. Set @code{>IN} to
    \G @code{0} and make the string @i{c-addr u} the input source
    \G and input buffer. Interpret. When the parse area is empty,
    \G restore the input source specification.
    push-file #tib ! >tib !
    input-start-line
    ['] interpret catch
    pop-file
    throw ;

\ \ Quit                                            	13feb93py

Defer 'quit

[IFUNDEF] bye
    : (bye)     ( 0 -- ) \ back to DOS
	drop 5 emit ;
    
    : bye ( -- )  0 (bye) ;
[THEN]

: prompt        state @ IF ."  compiled" EXIT THEN ."  ok" ;

: (quit) ( -- )
    \ exits only through THROW etc.
    BEGIN
	cr ['] cr catch if
	    [ has? OS [IF] ] >stderr [ [THEN] ]
	    cr ." Can't print to stdout, leaving" cr
	    \ if stderr does not work either, already DoError causes a hang
	    -2 (bye)
	endif [ [THEN] ]
	refill  WHILE
	    interpret prompt
    REPEAT
    bye ;

' (quit) IS 'quit

: dec.  base @ >r decimal . r> base ! ;
: DoError ( throw-code -- )
    cr source drop >in @ type ." <<< "
    dup -2 =  IF  "error @ type  drop  EXIT  THEN
    .error ;

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
    catch dup IF  DoError cr  THEN  (bye) ;

: do-find ( addr u -- )
    find-name dup IF  name>int  THEN  (bye) ;

\ \ Cold Boot                                    	13feb93py

: gforth ( -- )
    ." Gforth " version-string type 
    ." , Copyright (C) 1995-2012,2013,2014 Free Software Foundation, Inc." cr
    ." Gforth comes with ABSOLUTELY NO WARRANTY; for details type `license'"
[ has? os [IF] ]
     cr ." Type `bye' to exit"
[ [THEN] ] ;

defer bootmessage ( -- ) \ gforth
\G Hook (deferred word) executed right after interpreting the OS
\G command-line arguments.  Normally prints the Gforth startup
\G message.

has? file [IF]
defer process-args
[THEN]

' gforth IS bootmessage

has? os [IF]
Defer 'cold ( -- ) \ gforth  tick-cold
\G Hook (deferred word) for things to do right before interpreting the
\G OS command-line arguments.  Normally does some initializations that
\G you also want to perform.
' noop IS 'cold
[THEN]

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
    1 (bye) ;

has? new-input 0= [IF]
: clear-tibstack ( -- )
[ has? glocals [IF] ]
    lp@ forthstart 7 cells + @ - 
[ [ELSE] ]
    [ has? os [IF] ]
    r0 @ forthstart 6 cells + @ -
    [ [ELSE] ]
    sp@ cell+
    [ [THEN] ]
[ [THEN] ]
    dup >tib ! tibstack ! #tib off
    input-start-line ;
[THEN]

: boot ( path n **argv argc -- )
[ has? no-userspace 0= [IF] ]
    next-task 0= IF  main-task up!
    ELSE
	next-task @ 0= IF
	    throw-entry main-task udp @ throw-entry next-task -
	    /string >r swap r> move
	    next-task dup next-task 2!  normal-dp dpp !
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
[ has? peephole [IF] ]
    \ only needed for greedy static superinstruction selection
    \ primtable prepare-peephole-table TO peeptable
[ [THEN] ]
[ has? new-input [IF] ]
    current-input off
[ [THEN] ]
    clear-tibstack
    0 0 includefilename 2!
    rp@ rp0 !
[ has? floating [IF] ]
    fp@ fp0 !
[ [THEN] ]
[ has? os [IF] ]
    handler off
    ['] cold catch dup -&2049 <> if \ broken pipe?
	DoError cr
    endif
[ [ELSE] ]
    cold
[ [THEN] ]
[ has? os [IF] ]
    -1 (bye) \ !! determin exit code from throw code?
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

