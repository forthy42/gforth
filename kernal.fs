\ KERNAL.FS    GNU FORTH kernal                        17dec92py
\ $ID:
\ Idea and implementation: Bernd Paysan (py)
\ Copyright 1992 by the ANSI figForth Development Group

\ Log:  ', '- usw. durch [char] ... ersetzt
\       man sollte die unterschiedlichen zahlensysteme
\       mit $ und & zumindest im interpreter weglassen
\       schon erledigt!
\       11may93jaw
\ name>         0= nicht vorhanden              17may93jaw
\               nfa can be lfa or nfa!
\ find          splited into find and (find)
\               (find) for later use            17may93jaw
\ search        replaced by lookup because
\               it is a word of the string wordset
\                                               20may93jaw
\ postpone      added immediate                 21may93jaw
\ to            added immediate                 07jun93jaw
\ cfa, header   put "here lastcfa !" in
\               cfa, this is more logical
\               and noname: works wothout
\               extra "here lastcfa !"          08jun93jaw
\ (parse-white) thrown out
\ refill        added outer trick
\               to show there is something
\               going on                        09jun93jaw
\ leave ?leave  somebody forgot UNLOOP!!!       09jun93jaw
\ leave ?leave  unloop thrown out
\               unloop after loop is used       10jun93jaw

HEX

\ labels for some code addresses

: docon: ( -- addr )	\ gforth
    \ the code address of a @code{CONSTANT}
    ['] bl >code-address ;

: docol: ( -- addr )	\ gforth
    \ the code address of a colon definition
    ['] docon: >code-address ;

: dovar: ( -- addr )	\ gforth
    \ the code address of a @code{CREATE}d word
    ['] udp >code-address ;

: douser: ( -- addr )	\ gforth
    \ the code address of a @code{USER} variable
    ['] s0 >code-address ;

: dodefer: ( -- addr )	\ gforth
    \ the code address of a @code{defer}ed word
    ['] source >code-address ;

: dofield: ( -- addr )	\ gforth
    \ the code address of a @code{field}
    ['] reveal-method >code-address ;

\ Bit string manipulation                              06oct92py

Create bits  80 c, 40 c, 20 c, 10 c, 8 c, 4 c, 2 c, 1 c,
DOES> ( n -- )  + c@ ;

: >bit  ( addr n -- c-addr mask )  8 /mod rot + swap bits ;
: +bit  ( addr n -- )  >bit over c@ or swap c! ;

: relinfo ( -- addr )  forthstart dup @ + ;
: >rel  ( addr -- n )  forthstart - ;
: relon ( addr -- )  relinfo swap >rel cell / +bit ;

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

: aligned ( addr -- addr' ) \ core
  [ cell 1- ] Literal + [ -1 cells ] Literal and ;
: align ( -- ) \ core
    here dup aligned swap ?DO  bl c,  LOOP ;

: faligned ( addr -- f-addr ) \ float
    [ 1 floats 1- ] Literal + [ -1 floats ] Literal and ;

: falign ( -- ) \ float
    here dup faligned swap
    ?DO
	bl c,
    LOOP ;

\ !! this is machine-dependent, but works on all but the strangest machines
' faligned Alias maxaligned ( addr1 -- addr2 ) \ gforth
' falign Alias maxalign ( -- ) \ gforth

\ !! machine-dependent and won't work if "0 >body" <> "0 >body maxaligned"
' maxaligned Alias cfaligned ( addr1 -- addr2 ) \ gforth
\ the code field is aligned if its body is maxaligned
' maxalign Alias cfalign ( -- ) \ gforth

: chars ( n1 -- n2 ) \ core
; immediate


: A!    ( addr1 addr2 -- ) \ gforth
    dup relon ! ;
: A,    ( addr -- ) \ gforth
    here cell allot A! ;

\ on off                                               23feb93py

: on  ( addr -- ) \ gforth
    true  swap ! ;
: off ( addr -- ) \ gforth
    false swap ! ;

\ name> found                                          17dec92py

: (name>)  ( nfa -- cfa )
    count  $1F and  +  cfaligned ;
: name>    ( nfa -- cfa ) \ gforth
    cell+
    dup  (name>) swap  c@ $80 and 0= IF  @ THEN ;

: found ( nfa -- cfa n ) \ gforth
    cell+
    dup c@ >r  (name>) r@ $80 and  0= IF  @       THEN
                    -1 r@ $40 and     IF  1-      THEN
                       r> $20 and     IF  negate  THEN  ;

\ (find)                                               17dec92py

\ : (find) ( addr count nfa1 -- nfa2 / false )
\   BEGIN  dup  WHILE  dup >r
\          cell+ count $1F and dup >r 2over r> =
\          IF  -text  0= IF  2drop r> EXIT  THEN
\          ELSE  2drop drop  THEN  r> @
\   REPEAT nip nip ;

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

: Literal  ( compilation: n -- ; run-time: -- n ) \ core
    state @ IF postpone lit  , THEN ; immediate
: ALiteral ( compilation: addr -- ; run-time: -- addr ) \ gforth
    state @ IF postpone lit A, THEN ;
                                                      immediate

: char   ( 'char' -- n ) \ core
    bl word char+ c@ ;
: [char] ( compilation: 'char' -- ; run-time: -- n )
    char postpone Literal ; immediate
' [char] Alias Ascii immediate

: (compile) ( -- ) \ gforth
    r> dup cell+ >r @ compile, ;
: postpone ( "name" -- ) \ core
  name sfind dup 0= abort" Can't compile "
  0> IF  compile,  ELSE  postpone (compile) A,  THEN ;
                                             immediate restrict

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
\              16     2    10   Zeichen
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

: pad    ( -- addr ) \ core
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
	# 2dup d0=
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

: lp@ ( -- addr ) \ gforth	l-p-fetch
 laddr# [ 0 , ] ;

: catch ( x1 .. xn xt -- y1 .. ym 0 / z1 .. zn error ) \ exception
  >r sp@ r> swap >r       \ don't count xt! jaw
  fp@ >r
  lp@ >r
  handler @ >r
  rp@ handler !
  execute
  r> handler ! rdrop rdrop rdrop 0 ;

: throw ( y1 .. ym error/0 -- y1 .. ym / z1 .. zn error ) \ exception
    ?DUP IF
	[ here 4 cells ! ]
	handler @ rp!
	r> handler !
	r> lp!
	r> fp!
	r> swap >r sp! r>
    THEN ;

\ Bouncing is very fine,
\ programming without wasting time...   jaw
: bounce ( y1 .. ym error/0 -- y1 .. ym error / y1 .. ym ) \ gforth
\ a throw without data or fp stack restauration
  ?DUP IF
    handler @ rp!
    r> handler !
    r> lp!
    rdrop
    rdrop
  THEN ;

\ ?stack                                               23feb93py

: ?stack ( ?? -- ?? ) \ gforth
    sp@ s0 @ > IF    -4 throw  THEN
    fp@ f0 @ > IF  -&45 throw  THEN  ;
\ ?stack should be code -- it touches an empty stack!

\ interpret                                            10mar92py

Defer parser
Defer name ( -- c-addr count ) \ gforth
\ get the next word from the input buffer
' (name) IS name
Defer notfound ( c-addr count -- )

: no.extensions  ( addr u -- )
    2drop -&13 bounce ;
' no.extensions IS notfound

: interpret ( ?? -- ?? ) \ gforth
    \ interpret/compile the (rest of the) input buffer
    BEGIN
	?stack name dup
    WHILE
	parser
    REPEAT
    2drop ;

\ interpreter compiler                                 30apr92py

: interpreter  ( c-addr u -- ) \ gforth
    \ interpretation semantics for the name/number c-addr u
    2dup sfind dup
    IF
	1 and
	IF \ not restricted to compile state?
	    nip nip execute EXIT
	THEN
	-&14 throw
    THEN
    drop
    2dup 2>r snumber?
    IF
	2rdrop
    ELSE
	2r> notfound
    THEN ;

' interpreter  IS  parser

: compiler     ( c-addr u -- ) \ gforth
    \ compilation semantics for the name/number c-addr u
    2dup sfind dup
    IF
	0>
	IF
	    nip nip execute EXIT
	THEN
	compile, 2drop EXIT
    THEN
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
	drop notfound
    THEN ;

: [ ( -- ) \ core	left-bracket
    ['] interpreter  IS parser state off ; immediate
: ] ( -- ) \ core	right-bracket
    ['] compiler     IS parser state on  ;

\ locals stuff needed for control structures

: compile-lp+! ( n -- ) \ gforth	compile-l-p-plus-store
    dup negate locals-size +!
    0 over = if
    else -1 cells  over = if postpone lp-
    else  1 floats over = if postpone lp+
    else  2 floats over = if postpone lp+2
    else postpone lp+!# dup ,
    then then then then drop ;

: adjust-locals-size ( n -- ) \ gforth
    \ sets locals-size to n and generates an appropriate lp+!
    locals-size @ swap - compile-lp+! ;


here 0 , \ just a dummy, the real value of locals-list is patched into it in glocals.fs
AConstant locals-list \ acts like a variable that contains
		      \ a linear list of locals names


variable dead-code \ true if normal code at "here" would be dead
variable backedge-locals
    \ contains the locals list that BEGIN will assume to be live on
    \ the back edge if the BEGIN is unreachable from above. Set by
    \ ASSUME-LIVE, reset by UNREACHABLE.

: UNREACHABLE ( -- ) \ gforth
    \ declares the current point of execution as unreachable
    dead-code on
    0 backedge-locals ! ; immediate

: ASSUME-LIVE ( orig -- orig ) \ gforth
    \ used immediateliy before a BEGIN that is not reachable from
    \ above.  causes the BEGIN to assume that the same locals are live
    \ as at the orig point
    dup orig?
    2 pick backedge-locals ! ; immediate
    
\ locals list operations

: common-list ( list1 list2 -- list3 )
\ list1 and list2 are lists, where the heads are at higher addresses than
\ the tail. list3 is the largest sublist of both lists.
 begin
   2dup u<>
 while
   2dup u>
   if
     swap
   then
   @
 repeat
 drop ;

: sub-list? ( list1 list2 -- f )
\ true iff list1 is a sublist of list2
 begin
   2dup u<
 while
   @
 repeat
 = ;

: list-size ( list -- u )
\ size of the locals frame represented by list
 0 ( list n )
 begin
   over 0<>
 while
   over
   name> >body @ max
   swap @ swap ( get next )
 repeat
 faligned nip ;

: set-locals-size-list ( list -- )
 dup locals-list !
 list-size locals-size ! ;

: check-begin ( list -- )
\ warn if list is not a sublist of locals-list
 locals-list @ sub-list? 0= if
   \ !! print current position
   ." compiler was overly optimistic about locals at a BEGIN" cr
   \ !! print assumption and reality
 then ;

\ Control Flow Stack
\ orig, etc. have the following structure:
\ type ( defstart, live-orig, dead-orig, dest, do-dest, scopestart) ( TOS )
\ address (of the branch or the instruction to be branched to) (second)
\ locals-list (valid at address) (third)

\ types
0 constant defstart
1 constant live-orig
2 constant dead-orig
3 constant dest \ the loopback branch is always assumed live
4 constant do-dest
5 constant scopestart

: def? ( n -- )
    defstart <> abort" unstructured " ;

: orig? ( n -- )
 dup live-orig <> swap dead-orig <> and abort" expected orig " ;

: dest? ( n -- )
 dest <> abort" expected dest " ;

: do-dest? ( n -- )
 do-dest <> abort" expected do-dest " ;

: scope? ( n -- )
 scopestart <> abort" expected scope " ;

: non-orig? ( n -- )
 dest scopestart 1+ within 0= abort" expected dest, do-dest or scope" ;

: cs-item? ( n -- )
 live-orig scopestart 1+ within 0= abort" expected control flow stack item" ;

3 constant cs-item-size

: CS-PICK ( ... u -- ... destu ) \ tools-ext
 1+ cs-item-size * 1- >r
 r@ pick  r@ pick  r@ pick
 rdrop
 dup non-orig? ;

: CS-ROLL ( destu/origu .. dest0/orig0 u -- .. dest0/orig0 destu/origu ) \ tools-ext
 1+ cs-item-size * 1- >r
 r@ roll r@ roll r@ roll
 rdrop
 dup cs-item? ; 

: cs-push-part ( -- list addr )
 locals-list @ here ;

: cs-push-orig ( -- orig )
 cs-push-part dead-code @
 if
   dead-orig
 else
   live-orig
 then ;   

\ Structural Conditionals                              12dec92py

: ?struc      ( flag -- )       abort" unstructured " ;
: sys?        ( sys -- )        dup 0= ?struc ;
: >mark ( -- orig )
 cs-push-orig 0 , ;
: >resolve    ( addr -- )        here over - swap ! ;
: <resolve    ( addr -- )        here - , ;

: BUT
    1 cs-roll ;                      immediate restrict
: YET
    0 cs-pick ;                       immediate restrict

\ Structural Conditionals                              12dec92py

: AHEAD ( compilation: -- orig ; run-time: -- ) \ tools-ext
    POSTPONE branch  >mark  POSTPONE unreachable ; immediate restrict

: IF ( compilation: -- orig ; run-time: f -- ) \ core
 POSTPONE ?branch >mark ; immediate restrict

: ?DUP-IF ( compilation: -- orig ; run-time: n -- n| ) \ gforth	question-dupe-if
\ This is the preferred alternative to the idiom "?DUP IF", since it can be
\ better handled by tools like stack checkers
    POSTPONE ?dup POSTPONE if ;       immediate restrict
: ?DUP-0=-IF ( compilation: -- orig ; run-time: n -- n| ) \ gforth	question-dupe-zero-equals-if
    POSTPONE ?dup POSTPONE 0= POSTPONE if ; immediate restrict

: THEN ( compilation: orig -- ; run-time: -- ) \ core
    dup orig?
    dead-orig =
    if
        >resolve drop
    else
        dead-code @
        if
	    >resolve set-locals-size-list dead-code off
	else \ both live
	    over list-size adjust-locals-size
	    >resolve
	    locals-list @ common-list dup list-size adjust-locals-size
	    locals-list !
	then
    then ; immediate restrict

' THEN alias ENDIF ( compilation: orig -- ; run-time: -- ) \ gforth
immediate restrict
\ Same as "THEN". This is what you use if your program will be seen by
\ people who have not been brought up with Forth (or who have been
\ brought up with fig-Forth).

: ELSE ( compilation: orig1 -- orig2 ; run-time: f -- ) \ core
    POSTPONE ahead
    1 cs-roll
    POSTPONE then ; immediate restrict


: BEGIN ( compilation: -- dest ; run-time: -- ) \ core
    dead-code @ if
	\ set up an assumption of the locals visible here.  if the
	\ users want something to be visible, they have to declare
	\ that using ASSUME-LIVE
	backedge-locals @ set-locals-size-list
    then
    cs-push-part dest
    dead-code off ; immediate restrict

\ AGAIN (the current control flow joins another, earlier one):
\ If the dest-locals-list is not a subset of the current locals-list,
\ issue a warning (see below). The following code is generated:
\ lp+!# (current-local-size - dest-locals-size)
\ branch <begin>
: AGAIN ( compilation: dest -- ; run-time: -- ) \ core-ext
    dest?
    over list-size adjust-locals-size
    POSTPONE branch
    <resolve
    check-begin
    POSTPONE unreachable ; immediate restrict

\ UNTIL (the current control flow may join an earlier one or continue):
\ Similar to AGAIN. The new locals-list and locals-size are the current
\ ones. The following code is generated:
\ ?branch-lp+!# <begin> (current-local-size - dest-locals-size)
: until-like ( list addr xt1 xt2 -- )
    \ list and addr are a fragment of a cs-item
    \ xt1 is the conditional branch without lp adjustment, xt2 is with
    >r >r
    locals-size @ 2 pick list-size - dup if ( list dest-addr adjustment )
	r> drop r> compile,
	swap <resolve ( list adjustment ) ,
    else ( list dest-addr adjustment )
	drop
	r> compile, <resolve
	r> drop
    then ( list )
    check-begin ;

: UNTIL ( compilation: dest -- ; run-time: f -- ) \ core
    dest? ['] ?branch ['] ?branch-lp+!# until-like ; immediate restrict

: WHILE ( compilation: dest -- orig dest ; run-time: f -- ) \ core
    POSTPONE if
    1 cs-roll ; immediate restrict

: REPEAT ( compilation: orig dest -- ; run-time: -- ) \ core
    POSTPONE again
    POSTPONE then ; immediate restrict


\ counted loops

\ leave poses a little problem here
\ we have to store more than just the address of the branch, so the
\ traditional linked list approach is no longer viable.
\ This is solved by storing the information about the leavings in a
\ special stack.

\ !! remove the fixed size limit. 'Tis not hard.
20 constant leave-stack-size
create leave-stack  60 cells allot
Avariable leave-sp  leave-stack 3 cells + leave-sp !

: clear-leave-stack ( -- )
    leave-stack leave-sp ! ;

\ : leave-empty? ( -- f )
\  leave-sp @ leave-stack = ;

: >leave ( orig -- )
    \ push on leave-stack
    leave-sp @
    dup [ leave-stack 60 cells + ] Aliteral
    >= abort" leave-stack full"
    tuck ! cell+
    tuck ! cell+
    tuck ! cell+
    leave-sp ! ;

: leave> ( -- orig )
    \ pop from leave-stack
    leave-sp @
    dup leave-stack <= IF
       drop 0 0 0  EXIT  THEN
    cell - dup @ swap
    cell - dup @ swap
    cell - dup @ swap
    leave-sp ! ;

: DONE ( compilation: orig -- ; run-time: -- ) \ gforth
    \ !! the original done had ( addr -- )
    drop >r drop
    begin
	leave>
	over r@ u>=
    while
	POSTPONE then
    repeat
    >leave rdrop ; immediate restrict

: LEAVE ( compilation: -- ; run-time: loop-sys -- ) \ core
    POSTPONE ahead
    >leave ; immediate restrict

: ?LEAVE ( compilation: -- ; run-time: f | f loop-sys -- ) \ gforth	question-leave
    POSTPONE 0= POSTPONE if
    >leave ; immediate restrict

: DO ( compilation: -- do-sys ; run-time: w1 w2 -- loop-sys )
    POSTPONE (do)
    POSTPONE begin drop do-dest
    ( 0 0 0 >leave ) ; immediate restrict

: ?do-like ( -- do-sys )
    ( 0 0 0 >leave )
    >mark >leave
    POSTPONE begin drop do-dest ;

: ?DO ( compilation: -- do-sys ; run-time: w1 w2 -- | loop-sys )	\ core-ext	question-do
    POSTPONE (?do) ?do-like ; immediate restrict

: +DO ( compilation: -- do-sys ; run-time: w1 w2 -- | loop-sys )	\ gforth	plus-do
    POSTPONE (+do) ?do-like ; immediate restrict

: U+DO ( compilation: -- do-sys ; run-time: w1 w2 -- | loop-sys )	\ gforth	u-plus-do
    POSTPONE (u+do) ?do-like ; immediate restrict

: -DO ( compilation: -- do-sys ; run-time: w1 w2 -- | loop-sys )	\ gforth	minus-do
    POSTPONE (-do) ?do-like ; immediate restrict

: U-DO ( compilation: -- do-sys ; run-time: w1 w2 -- | loop-sys )	\ gforth	u-minus-do
    POSTPONE (u-do) ?do-like ; immediate restrict

: FOR ( compilation: -- do-sys ; run-time: w -- loop-sys )	\ gforth
    POSTPONE (for)
    POSTPONE begin drop do-dest
    ( 0 0 0 >leave ) ; immediate restrict

\ LOOP etc. are just like UNTIL

: loop-like ( do-sys xt1 xt2 -- )
    >r >r 0 cs-pick swap cell - swap 1 cs-roll r> r> rot do-dest?
    until-like  POSTPONE done  POSTPONE unloop ;

: LOOP ( compilation: do-sys -- ; run-time: loop-sys1 -- | loop-sys2 )	\ core
 ['] (loop) ['] (loop)-lp+!# loop-like ; immediate restrict

: +LOOP ( compilation: do-sys -- ; run-time: loop-sys1 n -- | loop-sys2 )	\ core	plus-loop
 ['] (+loop) ['] (+loop)-lp+!# loop-like ; immediate restrict

\ !! should the compiler warn about +DO..-LOOP?
: -LOOP ( compilation: do-sys -- ; run-time: loop-sys1 u -- | loop-sys2 )	\ gforth	minus-loop
 ['] (-loop) ['] (-loop)-lp+!# loop-like ; immediate restrict

\ A symmetric version of "+LOOP". I.e., "-high -low ?DO -inc S+LOOP"
\ will iterate as often as "high low ?DO inc S+LOOP". For positive
\ increments it behaves like "+LOOP". Use S+LOOP instead of +LOOP for
\ negative increments.
: S+LOOP ( compilation: do-sys -- ; run-time: loop-sys1 n -- | loop-sys2 )	\ gforth	s-plus-loop
 ['] (s+loop) ['] (s+loop)-lp+!# loop-like ; immediate restrict

: NEXT ( compilation: do-sys -- ; run-time: loop-sys1 -- | loop-sys2 ) \ gforth
 ['] (next) ['] (next)-lp+!# loop-like ; immediate restrict

\ Structural Conditionals                              12dec92py

: EXIT ( compilation: -- ; run-time: nest-sys -- ) \ core
    0 adjust-locals-size
    POSTPONE ;s
    POSTPONE unreachable ; immediate restrict

: ?EXIT ( -- ) ( compilation: -- ; run-time: nest-sys f -- | nest-sys ) \ gforth
     POSTPONE if POSTPONE exit POSTPONE then ; immediate restrict

\ Strings                                              22feb93py

: ," ( "string"<"> -- ) [char] " parse
  here over char+ allot  place align ;
: "lit ( -- addr )
  r> r> dup count + aligned >r swap >r ;               restrict
: (.")     "lit count type ;                           restrict
: (S")     "lit count ;                                restrict
: SLiteral ( Compilation: c-addr1 u ; run-time: -- c-addr2 u ) \ string
    postpone (S") here over char+ allot  place align ;
                                             immediate restrict
create s"-buffer /line chars allot
: S" ( compilation: 'ccc"' -- ; run-time: -- c-addr u )	\ core,file	s-quote
    [char] " parse
    state @
    IF
	postpone SLiteral
    ELSE
	/line min >r s"-buffer r@ cmove
	s"-buffer r>
    THEN ; immediate

: ." ( compilation: 'ccc"' -- ; run-time: -- )  \ core	dot-quote
    state @  IF    postpone (.") ,"  align
                    ELSE  [char] " parse type  THEN  ;  immediate
: ( ( compilation: 'ccc<close-paren>' -- ; run-time: -- ) \ core,file	paren
    [char] ) parse 2drop ;                       immediate
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
: abort" ( compilation: 'ccc"' -- ; run-time: f -- ) \ core,exception-ext	abort-quote
    postpone (abort") ," ;        immediate restrict

\ Header states                                        23feb93py

: flag! ( 8b -- )
    last @ dup 0= abort" last word was headerless"
    cell+ tuck c@ xor swap c! ;
: immediate     $20 flag! ;
: restrict      $40 flag! ;
\ ' noop alias restrict

\ Header                                               23feb93py

\ input-stream, nextname and noname are quite ugly (passing
\ information through global variables), but they are useful for dealing
\ with existing/independent defining words

defer (header)
defer header ( -- ) \ gforth
' (header) IS header

: string, ( c-addr u -- ) \ gforth
    \ puts down string as cstring
    dup c, here swap chars dup allot move ;

: name,  ( "name" -- ) \ gforth
    name name-too-short? name-too-long?
    string, cfalign ;
: input-stream-header ( "name" -- )
    \ !! this is f83-implementation-dependent
    align here last !  -1 A,
    name, $80 flag! ;

: input-stream ( -- )  \ general
\ switches back to getting the name from the input stream ;
    ['] input-stream-header IS (header) ;

' input-stream-header IS (header)

\ !! make that a 2variable
create nextname-buffer 32 chars allot

: nextname-header ( -- )
    \ !! f83-implementation-dependent
    nextname-buffer count
    align here last ! -1 A,
    string, cfalign
    $80 flag!
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
  Header reveal , $80 flag! ;

: name>string ( nfa -- addr count ) \ gforth	name-to-string
 cell+ count $1F and ;

Create ???  0 , 3 c, char ? c, char ? c, char ? c,
: >name ( cfa -- nfa ) \ gforth	to-name
 $21 cell do
   dup i - count $9F and + cfaligned over $80 + = if
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
    r> /does-handler + !does ;
: dodoes,  ( -- )
  here /does-handler allot does-handler! ;

: Create ( -- ) \ core
    Header reveal dovar: cfa, ;

\ DOES>                                                17mar93py

: DOES>  ( compilation: colon-sys1 -- colon-sys2 ; run-time: nest-sys -- ) \ core	does
    state @
    IF
	;-hook postpone (does>) ?struc dodoes,
    ELSE
	align dodoes, here !does ]
    THEN 
    defstart :-hook ; immediate

\ Create Variable User Constant                        17mar93py

: Variable ( -- ) \ core
    Create 0 , ;
: AVariable ( -- ) \ gforth
    Create 0 A, ;
: 2VARIABLE ( "name" -- ) \ double
    create 0 , 0 , ;
    
: User
    Variable ;
: AUser
    AVariable ;

: (Constant)  Header reveal docon: cfa, ;
: Constant ( w -- ) \ core
    (Constant) , ;
: AConstant ( addr -- ) \ gforth
    (Constant) A, ;

: 2Constant ( d -- ) \ double
    Create ( w1 w2 "name" -- )
        2,
    DOES> ( -- w1 w2 )
        2@ ;
    
\ IS Defer What's Defers TO                            24feb93py

: Defer ( -- ) \ gforth
    \ !! shouldn't it be initialized with abort or something similar?
    Header Reveal dodefer: cfa,
    ['] noop A, ;
\     Create ( -- ) 
\ 	['] noop A,
\     DOES> ( ??? )
\ 	@ execute ;

: IS ( addr "name" -- ) \ gforth
    ' >body
    state @
    IF    postpone ALiteral postpone !  
    ELSE  !
    THEN ;  immediate
' IS Alias TO ( addr "name" -- ) \ core-ext
immediate

: What's ( "name" -- addr ) \ gforth
    ' >body
    state @
    IF
	postpone ALiteral postpone @
    ELSE
	@
    THEN ; immediate
: Defers ( "name" -- ) \ gforth
    ' >body @ compile, ; immediate

\ : ;                                                  24feb93py

defer :-hook ( sys1 -- sys2 )
defer ;-hook ( sys2 -- sys1 )

: : ( -- colon-sys ) \ core	colon
    Header docol: cfa, defstart ] :-hook ;
: ; ( compilation: colon-sys -- ; run-time: nest-sys ) \ core	semicolon
    ;-hook ?struc postpone exit reveal postpone [ ; immediate restrict

: :noname ( -- xt colon-sys ) \ core-ext	colon-no-name
    0 last !
    here docol: cfa, 0 ] :-hook ;

\ Search list handling                                 23feb93py

AVariable current ( -- addr ) \ gforth

: last?   ( -- false / nfa nfa )
    last @ ?dup ;
: (reveal) ( -- )
    last?
    IF
	dup @ 0<
	IF
	    current @ @ over ! current @ !
	ELSE
	    drop
	THEN
    THEN ;

\ object oriented search list                          17mar93py

\ word list structure:

struct
  1 cells: field find-method   \ xt: ( c_addr u wid -- name-id )
  1 cells: field reveal-method \ xt: ( -- ) \ used by dofield:, must be field
  1 cells: field rehash-method \ xt: ( wid -- )
\   \ !! what else
end-struct wordlist-map-struct

struct
  1 cells: field wordlist-id \ not the same as wid; representation depends on implementation
  1 cells: field wordlist-map \ pointer to a wordlist-map-struct
  1 cells: field wordlist-link \ link field to other wordlists
  1 cells: field wordlist-extend \ points to wordlist extensions (eg hash)
end-struct wordlist-struct

: f83find      ( addr len wordlist -- nfa / false )  @ (f83find) ;

\ Search list table: find reveal
Create f83search       ' f83find A,  ' (reveal) A,  ' drop A,

Create forth-wordlist  NIL A, G f83search T A, NIL A, NIL A,
AVariable lookup       G forth-wordlist lookup T !
G forth-wordlist current T !

: (search-wordlist)  ( addr count wid -- nfa / false )
  dup wordlist-map @ find-method @ execute ;

: search-wordlist  ( addr count wid -- 0 / xt +-1 ) \ search
    (search-wordlist) dup  IF  found  THEN ;

Variable warnings ( -- addr ) \ gforth
G -1 warnings T !

: check-shadow  ( addr count wid -- )
\ prints a warning if the string is already present in the wordlist
\ !! should be refined so the user can suppress the warnings
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

: sfind ( c-addr u -- xt n / 0 ) \ gforth
    lookup @ search-wordlist ;

: find   ( addr -- cfa +-1 / string false ) \ core,search
    \ !! not ANS conformant: returns +-2 for restricted words
    dup count sfind dup if
	rot drop
    then ;

: reveal ( -- ) \ gforth
 last? if
   name>string current @ check-shadow
 then
 current @ wordlist-map @ reveal-method @ execute ;

: rehash  ( wid -- )
    dup wordlist-map @ rehash-method @ execute ;

: '    ( "name" -- addr ) \ core	tick
    name sfind 0= if -&13 bounce then ;
: [']  ( compilation: "name" -- ; run-time: --addr ) \ core	bracket-tick
    ' postpone ALiteral ; immediate
\ Input                                                13feb93py

07 constant #bell ( -- c ) \ gforth
08 constant #bs ( -- c ) \ gforth
09 constant #tab ( -- c ) \ gforth
7F constant #del ( -- c ) \ gforth
0D constant #cr   ( -- c ) \ gforth
\ the newline key code
0C constant #ff ( -- c ) \ gforth
0A constant #lf ( -- c ) \ gforth

: bell  #bell emit ;

\ : backspaces  0 ?DO  #bs emit  LOOP ;
: >string  ( span addr pos1 -- span addr pos1 addr2 len )
  over 3 pick 2 pick chars /string ;
: type-rest ( span addr pos1 -- span addr pos1 back )
  >string tuck type ;
: (del)  ( max span addr pos1 -- max span addr pos2 )
  1- >string over 1+ -rot move
  rot 1- -rot  #bs emit  type-rest bl emit 1+ backspaces ;
: (ins)  ( max span addr pos1 char -- max span addr pos2 )
  >r >string over 1+ swap move 2dup chars + r> swap c!
  rot 1+ -rot type-rest 1- backspaces 1+ ;
: ?del ( max span addr pos1 -- max span addr pos2 0 )
  dup  IF  (del)  THEN  0 ;
: (ret)  type-rest drop true space ;
: back  dup  IF  1- #bs emit  ELSE  #bell emit  THEN 0 ;
: forw 2 pick over <> IF  2dup + c@ emit 1+  ELSE  #bell emit  THEN 0 ;
: eof  2 pick 0=  IF  bye  ELSE  (ret)  THEN ;

Create ctrlkeys
  ] false false back  false  eof   false forw  false
    ?del  false (ret) false  false (ret) false false
    false false false false  false false false false
    false false false false  false false false false [

defer everychar
' noop IS everychar

: decode ( max span addr pos1 key -- max span addr pos2 flag )
  everychar
  dup #del = IF  drop #bs  THEN  \ del is rubout
  dup bl <   IF  cells ctrlkeys + @ execute  EXIT  THEN
  >r 2over = IF  rdrop bell 0 EXIT  THEN
  r> (ins) 0 ;

\ decode should better use a table for control key actions
\ to define keyboard bindings later

: accept   ( addr len -- len ) \ core
  dup 0< IF    abs over dup 1 chars - c@ tuck type 
\ this allows to edit given strings
         ELSE  0  THEN rot over
  BEGIN  key decode  UNTIL
  2drop nip ;

\ Output                                               13feb93py

Defer type ( c-addr u -- ) \ core
\ defer type for a output buffer or fast
\ screen write

\ : (type) ( addr len -- )
\   bounds ?DO  I c@ emit  LOOP ;

' (type) IS Type

Defer emit ( c -- ) \ core
' (Emit) IS Emit

Defer key ( -- c ) \ core
' (key) IS key

\ : form  ( -- rows cols )  &24 &80 ;
\ form should be implemented using TERMCAPS or CURSES
\ : rows  form drop ;
\ : cols  form nip  ;

\ Query                                                07apr93py

: refill ( -- flag ) \ core-ext,block-ext,file-ext
  blk @  IF  1 blk +!  true  0 >in !  EXIT  THEN
  tib /line
  loadfile @ ?dup
  IF    read-line throw
  ELSE  loadline @ 0< IF 2drop false EXIT THEN
        accept true
  THEN
  1 loadline +!
  swap #tib ! 0 >in ! ;

: Query  ( -- ) \ core-ext
    \ obsolescent
    loadfile off  blk off  refill drop ;

\ File specifiers                                       11jun93jaw


\ 1 c, here char r c, 0 c,                0 c, 0 c, char b c, 0 c,
\ 2 c, here char r c, char + c, 0 c,
\ 2 c, here char w c, char + c, 0 c, align
4 Constant w/o ( -- fam ) \ file	w-o
2 Constant r/w ( -- fam ) \ file	r-o
0 Constant r/o ( -- fam ) \ file	r-w

\ BIN WRITE-LINE                                        11jun93jaw

\ : bin           dup 1 chars - c@
\                 r/o 4 chars + over - dup >r swap move r> ;

: bin ( fam1 -- fam2 ) \ file
    1 or ;

create nl$ 1 c, A c, 0 c, \ gnu includes usually a cr in dos
                           \ or not unix environments if
                           \ bin is not selected

: write-line ( c-addr u fileid -- ior ) \ file
    dup >r write-file
    ?dup IF
	r> drop EXIT
    THEN
    nl$ count r> write-file ;

\ include-file                                         07apr93py

: push-file  ( -- )  r>
  loadline @ >r loadfile @ >r
  blk @ >r >tib @ >r  #tib @ dup >r  >tib +!  >in @ >r  >r ;

: pop-file   ( throw-code -- throw-code )
  dup IF
         source >in @ loadline @ loadfilename 2@
	 error-stack dup @ dup 1+
	 max-errors 1- min error-stack !
	 6 * cells + cell+
	 5 cells bounds swap DO
	                    I !
	 -1 cells +LOOP
  THEN
  r>
  r> >in !  r> #tib !  r> >tib !  r> blk !
  r> loadfile ! r> loadline !  >r ;

: read-loop ( i*x -- j*x )
  BEGIN  refill  WHILE  interpret  REPEAT ;

: include-file ( i*x fid -- j*x ) \ file
  push-file  loadfile !
  0 loadline ! blk off  ['] read-loop catch
  loadfile @ close-file swap 2dup or
  pop-file  drop throw throw ;

create pathfilenamebuf 256 chars allot \ !! make this grow on demand

\ : check-file-prefix  ( addr len -- addr' len' flag )
\   dup 0=                    IF  true EXIT  THEN 
\   over c@ '/ =              IF  true EXIT  THEN 
\   over 2 S" ./" compare 0=  IF  true EXIT  THEN 
\   over 3 S" ../" compare 0= IF  true EXIT  THEN
\   over 2 S" ~/" compare 0=
\   IF     1 /string
\          S" HOME" getenv tuck pathfilenamebuf swap move
\          2dup + >r pathfilenamebuf + swap move
\          pathfilenamebuf r> true
\   ELSE   false
\   THEN ;

: open-path-file ( c-addr1 u1 -- file-id c-addr2 u2 ) \ gforth
    \ opens a file for reading, searching in the path for it (unless
    \ the filename contains a slash); c-addr2 u2 is the full filename
    \ (valid until the next call); if the file is not found (or in
    \ case of other errors for each try), -38 (non-existant file) is
    \ thrown. Opening for other access modes makes little sense, as
    \ the path will usually contain dirs that are only readable for
    \ the user
    \ !! use file-status to determine access mode?
    2dup [char] / scan nip ( 0<> )
    if \ the filename contains a slash
	2dup r/o open-file throw ( c-addr1 u1 file-id )
	-rot >r pathfilenamebuf r@ cmove ( file-id R: u1 )
	pathfilenamebuf r> EXIT
    then
    pathdirs 2@ 0
\    check-file-prefix 0= 
\    IF  pathdirs 2@ 0
    ?DO ( c-addr1 u1 dirnamep )
	dup >r 2@ dup >r pathfilenamebuf swap cmove ( addr u )
	2dup pathfilenamebuf r@ chars + swap cmove ( addr u )
	pathfilenamebuf over r> + dup >r r/o open-file 0=
	IF ( addr u file-id )
	    nip nip r> rdrop 0 LEAVE
	THEN
	rdrop drop r> cell+ cell+
    LOOP
\    ELSE   2dup open-file throw -rot  THEN 
    0<> -&38 and throw ( file-id u2 )
    pathfilenamebuf swap ;

create included-files 0 , 0 , ( pointer to and count of included files )
create image-included-files 0 , 0 , ( pointer to and count of included files )
\ included-files points to ALLOCATEd space, while image-included-files
\ points to ALLOTed objects, so it survives a save-system

: init-included-files ( -- )
    image-included-files 2@ 2* cells save-string drop ( addr )
    image-included-files 2@ nip included-files 2! ;

: included? ( c-addr u -- f ) \ gforth
    \ true, iff filename c-addr u is in included-files
    included-files 2@ 0
    ?do ( c-addr u addr )
	dup >r 2@ 2over compare 0=
	if
	    2drop rdrop unloop
	    true EXIT
	then
	r> cell+ cell+
    loop
    2drop drop false ;

: add-included-file ( c-addr u -- ) \ gforth
    \ add name c-addr u to included-files
    included-files 2@ tuck 1+ 2* cells resize throw
    swap 2dup 1+ included-files 2!
    2* cells + 2! ;

: save-string		( addr1 u -- addr2 u ) \ gforth
    \ !! not a string, but a memblock word
    swap >r
    dup allocate throw
    swap 2dup r> -rot move ;

: included1 ( i*x file-id c-addr u -- j*x ) \ gforth
    \ include the file file-id with the name given by c-addr u
    loadfilename 2@ >r >r
    save-string 2dup loadfilename 2! add-included-file ( file-id )
    ['] include-file catch
    r> r> loadfilename 2!  throw ;
    
: included ( i*x addr u -- j*x ) \ gforth
    open-path-file included1 ;

: required ( i*x addr u -- j*x ) \ gforth
    \ include the file with the name given by addr u, if it is not
    \ included already. Currently this works by comparing the name of
    \ the file (with path) against the names of earlier included
    \ files; however, it would probably be better to fstat the file,
    \ and compare the device and inode. The advantages would be: no
    \ problems with several paths to the same file (e.g., due to
    \ links) and we would catch files included with include-file and
    \ write a require-file.
    open-path-file 2dup included?
    if
	2drop close-file throw
    else
	included1
    then ;

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

\ INCLUDE                                               9may93jaw

: include  ( "file" -- ) \ gforth
  name included ;

: require  ( "file" -- ) \ gforth
  name required ;

\ RECURSE                                               17may93jaw

: recurse ( compilation: -- ; run-time: ?? -- ?? ) \ core
    lastxt compile, ; immediate restrict
: recursive ( -- ) \ gforth
    reveal last off ; immediate

\ */MOD */                                              17may93jaw

\ !! I think */mod should have the same rounding behaviour as / - anton
: */mod ( n1 n2 n3 -- n4 n5 ) \ core	star-slash-mod
    >r m* r> sm/rem ;

: */ ( n1 n2 n3 -- n4 ) \ core	star-slash
    */mod nip ;

\ EVALUATE                                              17may93jaw

: evaluate ( c-addr len -- ) \ core,block
  push-file  dup #tib ! >tib @ swap move
  >in off blk off loadfile off -1 loadline !
\  BEGIN  interpret  >in @ #tib @ u>= UNTIL
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
: (quit)        BEGIN .status cr query interpret prompt AGAIN ;
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

: typewhite ( addr u -- ) \ gforth
    \ like type, but white space is printed instead of the characters
    bounds ?do
	i c@ 9 = if \ check for tab
	    9
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
  loadline @ IF
               source >in @ loadline @ 0 0 .error-frame
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
  normal-dp dpp ! ;

' (DoError) IS DoError

: quit ( ?? -- ?? ) \ core
    r0 @ rp! handler off >tib @ >r
    BEGIN
	postpone [
	['] 'quit CATCH dup
    WHILE
	DoError r@ >tib !
    REPEAT
    drop r> >tib ! ;

\ Cold                                                 13feb93py

\ : .name ( name -- ) cell+ count $1F and type space ;
\ : words  listwords @
\          BEGIN  @ dup  WHILE  dup .name  REPEAT drop ;

: cstring>sstring  ( cstring -- addr n ) \ gforth	cstring-to-sstring
    -1 0 scan 0 swap 1+ /string ;
: arg ( n -- addr count ) \ gforth
    cells argv @ + @ cstring>sstring ;
: #!       postpone \ ;  immediate

Create pathstring 2 cells allot \ string
Create pathdirs   2 cells allot \ dir string array, pointer and count
Variable argv
Variable argc

0 Value script? ( -- flag )

: process-path ( addr1 u1 -- addr2 u2 )
    \ addr1 u1 is a path string, addr2 u2 is an array of dir strings
    here >r
    BEGIN
	over >r [char] : scan
	over r> tuck - ( rest-str this-str )
	dup
	IF
	    2dup 1- chars + c@ [char] / <>
	    IF
		2dup chars + [char] / swap c!
		1+
	    THEN
	    2,
	ELSE
	    2drop
	THEN
	dup
    WHILE
	1 /string
    REPEAT
    2drop
    here r> tuck - 2 cells / ;

: do-option ( addr1 len1 addr2 len2 -- n )
    2swap
    2dup s" -e"         compare  0= >r
    2dup s" --evaluate" compare  0= r> or
    IF  2drop dup >r ['] evaluate catch
	?dup IF  dup >r DoError r> negate (bye)  THEN
	r> >tib +!  2 EXIT  THEN
    ." Unknown option: " type cr 2drop 1 ;

: process-args ( -- )
    >tib @ >r
    argc @ 1
    ?DO
	I arg over c@ [char] - <>
	IF
	    required 1
	ELSE
	    I 1+ argc @ =  IF  s" "  ELSE  I 1+ arg  THEN
	    do-option
	THEN
    +LOOP
    r> >tib ! ;

Defer 'cold ' noop IS 'cold

: cold ( -- ) \ gforth
    pathstring 2@ process-path pathdirs 2!
    init-included-files
    'cold
    argc @ 1 >
    IF
	true to script?
	['] process-args catch ?dup
	IF
	    dup >r DoError cr r> negate (bye)
	THEN
	cr
    THEN
    false to script?
    ." GNU Forth " version-string type ." , Copyright (C) 1994 Free Software Foundation, Inc." cr
    ." GNU Forth comes with ABSOLUTELY NO WARRANTY; for details type `license'" cr
    ." Type `bye' to exit"
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
  argc ! argv ! cstring>sstring pathstring 2!  main-task up!
  sp@ dup s0 ! $10 + >tib ! #tib off >in off
  rp@ r0 !  fp@ f0 !  cold ;

: bye ( -- ) \ tools-ext
    script? 0= IF  cr  THEN  0 (bye) ;

\ **argv may be scanned by the C starter to get some important
\ information, as -display and -geometry for an X client FORTH
\ or space and stackspace overrides

\ 0 arg contains, however, the name of the program.
