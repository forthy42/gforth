\ A powerful locals implementation

\ Authors: Anton Ertl, Bernd Paysan, Jens Wilke, Neal Crook
\ Copyright (C) 1995,1996,1997,1998,2000,2003,2004,2005,2007,2011,2012,2013,2014,2015,2016,2017,2018,2019,2020,2021,2022,2023,2024,2025 Free Software Foundation, Inc.

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


\ More documentation can be found in the manual and in
\ http://www.complang.tuwien.ac.at/papers/ertl94l.ps.gz

\ Local variables are quite important for writing readable programs, but
\ IMO (anton) they are the worst part of the standard. There they are very
\ restricted and have an ugly interface.

\ So, we implement the locals wordset, but do not recommend using
\ local-ext (which is a really bad user interface for locals).

\ We also have a nice and powerful user-interface for locals: locals are
\ defined with

\ { local1 local2 ... }
\ or
\ { local1 local2 ... -- ... }
\ (anything after the -- is just a comment)

\ Every local in this list consists of an optional type specification
\ and a name. If there is only the name, it stands for a cell-sized
\ value (i.e., you get the value of the local variable, not it's
\ address). The following type specifiers stand before the name:

\ Specifier	Type	Access
\ W:		Cell	value
\ W^		Cell	address
\ D:		Double	value
\ D^		Double	address
\ F:		Float	value
\ F^		Float	address
\ C:		Char	value
\ C^		Char	address
\ |             nothing switches to zero-initialized values

\ The local variables are initialized with values from the appropriate
\ stack. In contrast to the examples in the standard document our locals
\ take the arguments in the expected way: The last local gets the top of
\ stack, the second last gets the second stack item etc. An example:

\ : CX* { F: Ar  F: Ai  F: Br  F: Bi -- Cr Ci }
\ \ complex multiplication
\  Ar Br f* Ai Bi f* f-
\  Ar Bi f* Ai Br f* f+ ;

\ There will also be a way to add user types, but it is not yet decided,
\ how. Ideas are welcome.

\ Locals defined in this manner live until (!! see below). 
\ Their names can be used during this time to get
\ their value or address; The addresses produced in this way become
\ invalid at the end of the lifetime.

\ Values can be changed with TO, but this is not recomended (TO is a
\ kludge and words lose the single-assignment property, which makes them
\ harder to analyse).

\ As for the internals, we use a special locals stack. This eliminates
\ the problems and restrictions of reusing the return stack and allows
\ to store floats as locals: the return stack is not guaranteed to be
\ aligned correctly, but our locals stack must be float-aligned between
\ words.

require search.fs
require float.fs
require extend.fs \ for case
require compat/caseext.fs
require sections.fs

User locals-size \ this is the current size of the locals stack
		 \ frame of the current word

\ optimize @localn and !localn

: optimizes ( xt "name" -- )
    \ xt is optimizer of "name"
    ' make-latest set-optimizer ;

: xts, ( "name1" .. "namen" -- )
    BEGIN  parse-name  dup WHILE  rec-name '-error ,  REPEAT  2drop ;
    
: opt-table: ( unit -- )
    Create 0 , , xts,
    here latestxt >body dup >r 2 th - cell/ r> !
    DOES> ( xt table -- )
    >r lits# 1 u>= if
        lits> dup r@ cell+ @ /mod swap 0= over r@ @ u< and if
            cells r> 2 th + @ compile, 2drop exit then
	drop >lits then
    rdrop peephole-compile, ;

cell opt-table: opt-@localn @local0 @local1 @local2 @local3 @local4 @local5 @local6 @local7 
latestxt optimizes @localn

cell opt-table: opt-!localn !local0 !local1 !local2 !local3 !local4 !local5 !local6 !local7
latestxt optimizes !localn

\ peephole optimizer enabled 2compile,

$Variable peephole-opts

: 2compile, ( xt1 xt2 -- ) \ gforth-internal two-compile-comma
    \G equivalent to @code{@i{xt1} compile, @i{xt2} compile,}, but
    \G also applies peephole optimization.
    peephole-opts $@ bounds ?DO
	2dup I 2@ d= IF
	    2drop I 2 th@ opt-compile,  UNLOOP  EXIT
	THEN
    3 cells +LOOP
    >r opt-compile, r> compile, ;

\ compile locals with offset n

: compile-@local ( n -- ) \ gforth-internal compile-fetch-local
    \ n is the offset from LP
    lit, postpone @localn ;

: compile-f@local ( n -- ) \ gforth-internal compile-f-fetch-local
    lit, postpone f@localn ;

\ locals stuff needed for control structures

: compile-lp+! ( n -- ) \ gforth	compile-l-p-plus-store
    dup negate locals-size +!
    0 over = if
    else -1 cells  over = if postpone lp-
    else  1 floats over = if postpone lp+
    else  2 floats over = if postpone lp+2
    else dup lit, postpone lp+!
    then then then then drop ;

: adjust-locals-size ( n -- ) \ gforth-internal
    \g sets locals-size to n and generates an appropriate lp+!
    locals-size @ swap - compile-lp+! ;

\ the locals stack grows downwards (see primitives)
\ of the local variables of a group (in braces) the leftmost is on top,
\ i.e. by going onto the locals stack the order is reversed.
\ there are alignment gaps if necessary.
\ lp must have the strictest alignment (usually float) across calls;
\ for simplicity we align it strictly for every group.

slowvoc @
slowvoc on \ we want a linked list for the vocabulary locals
vocabulary locals \ this contains the local variables
' locals >wordlist wordlist-id to locals-list
slowvoc !

: no-post -48 throw ;
Defer locals-post,

translate-name >body 2@ swap
' locals-post,
translate: translate-local ( ... nt -- ... )
\ undocumented for good reasons

: rec-local ( c-addr u -- translation ) \ gforth-experimental
    \G Recognizes (@pxref{Defining recognizers})
    \G a visible local.  If successful, @i{translation}
    \G represents pushing the value of the local at run-time (for
    \G details @pxref{Gforth locals} and @pxref{Macros}).
    [ ' locals >wordlist compile, ]
    dup translate-name = IF drop translate-local THEN ;

' search-order ' rec-local 2 rec-sequence: rec-name/local

: activate-locals   ['] rec-name/local is rec-name ;
: deactivate-locals ['] search-order is rec-name ;

:noname defers wrap@ ['] rec-name defer@ deactivate-locals ; is wrap@
:noname is rec-name defers wrap! ; is wrap!

: alignlp-w ( n1 -- n2 )
    \ cell-align size and generate the corresponding code for aligning lp
    aligned dup adjust-locals-size ;

: alignlp-f ( n1 -- n2 )
    faligned dup adjust-locals-size ;

: maxalign-lp ( -- )
    locals-size @ alignlp-f locals-size ! ;

\ a local declaration group (the braces stuff) is compiled by calling
\ the appropriate compile-pushlocal for the locals, starting with the
\ righmost local; the names are already created earlier, the
\ compile-pushlocal just inserts the offsets from the frame base.

Variable val-part \ contains true before |, false afterwards

: locals, ( addr size -- )
    dup locals-size ! swap ! ;

: compile-pushlocal-w ( a-addr -- ) ( run-time: w -- )
\ compiles a push of a local variable, and adjusts locals-size
\ stores the offset of the local variable to a-addr
    locals-size @ alignlp-w cell+ locals,
    val-part @ IF  postpone false  THEN  postpone >l ;

: compile-pushlocal-f ( a-addr -- ) ( run-time: f -- )
    locals-size @ alignlp-f float+ locals,
    val-part @ IF  postpone 0e  THEN  postpone f>l ;

: 2>l swap >l >l ;
opt: drop postpone swap postpone >l postpone >l ;

: compile-pushlocal-d ( a-addr -- ) ( run-time: w1 w2 -- )
    locals-size @ alignlp-w cell+ cell+ locals,
    val-part @ IF  postpone #0.  THEN  postpone 2>l ;

: compile-pushlocal-c ( a-addr -- ) ( run-time: w -- )
    -1 chars compile-lp+!
    locals-size @ swap !
    val-part @ IF  postpone false  THEN  postpone lp@ postpone c! ;

: compile-pushlocal-[ ( size a-addr -- ) ( run-time: addr -- )
    swap maxaligned dup negate compile-lp+!
    val-part @ IF  drop  ELSE  postpone lp@ lit, postpone move  THEN
    locals-size @ swap ! ;

\ locals list operations

[IFUNDEF] >link ' noop Alias >link [THEN]
[IFUNDEF] >f+c  : >f+c cell+ ;     [THEN]

: list-length ( list -- u )
    0 swap begin ( u1 list1 )
       dup while
           name>link 1 under+
    repeat
    drop ;

: /list ( list1 u -- list2 )
    \ list2 is list1 with the first u elements removed
    0 ?do
	name>link
    loop ;

: common-list ( list1 list2 -- list3 ) \ gforth-internal
    \ list3 is the largest common tail of both lists.
    over list-length over list-length - dup 0< if
	negate >r swap r>
    then ( long short u )
    rot swap /list
    begin ( list3 list4 )
	2dup u<> while
	    name>link swap name>link
    repeat
    drop ;

: sub-list? ( list1 list2 -- f ) \ gforth-internal
    \ true iff list1 is a sublist of list2
    over list-length over list-length swap - 0 max /list = ;

: list-size ( list -- u ) \ gforth-internal
    \ size of the locals frame represented by list
    0 ( list n )
    begin
	over 0<>
    while
	over
	((name>)) >body @ max
	swap name>link swap ( get next )
    repeat
    faligned nip ;

Defer locals-list!
:noname locals-list ! ; is locals-list!

: set-locals-size-list ( list -- )
    dup locals-list!
    list-size locals-size ! ;

: check-begin ( list -- )
\ warn if list is not a sublist of locals-list
 locals-list @ sub-list? 0= if
   \ !! print current position
     >stderr ." compiler was overly optimistic about locals at a BEGIN" cr
   \ !! print assumption and reality
 then ;

(field) locals-name-size+ hmsize cell+ , \ fields + wiggle room, name size must be added

Defer locals-warning  ' noop is locals-warning

: create-local1 ( does-xt to-xt "name" -- a-addr )
    create locals-warning
    immediate restrict
    set-to set-does>
    here 0 , ( place for the offset ) ;

16384 extra-section locals-headers

' where, >code-address dodefer: = [IF]
    : locals-where, ( nt -- )
	dup ['] locals-headers @ 2@ swap bounds within
	IF  defers where,  ELSE  drop  THEN ;
    ' locals-where, is where,
[THEN]

: create-local ( does-xt to-xt "name" -- a-addr )
    \ defines the local "name"; the offset of the local shall be
    \ stored in a-addr
    nextname$ $@ d0= IF
	parse-name nextname THEN
    ['] xt-location defer@ >r ['] noop is xt-location
    ['] create-local1 locals-headers
    r> is xt-location ;

\ offset calculation

: lp-offset ( n1 -- n2 )
\ converts the offset from the frame start to an offset from lp and
\ i.e., the address of the local is lp+locals_size-offset
  locals-size @ swap - ;

: laddr#, ( n -- )
    \ for local with offset n from frame start, compile the address
    lp-offset postpone literal postpone lp+n ;

\ specialized to-class:

: locals-to:exec ( .. u xt1 xt2 -- .. )
    -14 throw ;
: locals-to:,  ( u lits:xt2 table-addr -- )
    @ swap th@ lits> @ lp-offset >lits ['] lp+n swap 2compile, ;

: locals-to-class: ( !-table -- )
    Create , ['] locals-to:exec set-does> ['] locals-to:, set-optimizer ;

: c+! ( c addr -- ) dup >r c@ + r> c! ;
: 2+! ( d addr -- ) dup >r 2@ d+ r> 2! ;

to-table: 2!-table 2! 2+!
to-table: c!-table c! c+!

!-table locals-to-class: to-w:
defer-table locals-to-class: to-xt:
2!-table locals-to-class: to-d:
c!-table locals-to-class: to-c:
f!-table locals-to-class: to-f:

: val-part-off ( -- ) val-part off ;

vocabulary locals-types \ this contains all the type specifyers, -- and }
locals-types definitions

: W: ( compilation "name" -- a-addr xt; run-time x -- ) \ gforth w-colon
    \g Define local @i{name} with the initial value @i{x}.@*
    \g @i{name} execution: @i{( -- x1 )} push the current value of @i{name}.@*
    \g @code{to @i{name}} run-time: @i{( x2 -- )} change the value of
    \g @i{name} to @i{x2}.@*
    \g @code{+to @i{name}} run-time: @i{( n|u -- )} increment the value of
    \g @i{name} by @i{n|u}.
    [: ( Compilation: -- ) ( Run-time: -- w )
	\ compiles a local variable access
	@ lp-offset compile-@local ;]
    ['] to-w: create-local
    \ xt produces the appropriate locals pushing code when executed
    ['] compile-pushlocal-w ;

: W^ ( compilation "name" -- a-addr xt; run-time x -- ) \ gforth w-caret
    \g Define local @i{name}, reserve a cell at @i{a-addr}, and store @i{x} there.@*
    \g @i{name} execution: @i{( -- a-addr )}.@*
    [: ( Compilation: -- ) ( Run-time: -- w )
	@ laddr#, ;]
    ['] n/a create-local
    ['] compile-pushlocal-w ;

: F: ( compilation "name" -- a-addr xt; run-time r -- ) \ gforth f-colon
    \g Define local @i{name} with the initial value @i{r}.@*
    \g @i{name} execution: @i{( -- r1 )} push the current value of @i{name}.@*
    \g @code{to @i{name}} run-time: @i{( r2 -- )} change the value of
    \g @i{name} to @i{r2}.@*
    \g @code{+to @i{name}} run-time: @i{( r3 -- )} increment the value of
    \g @i{name} by @i{r3}.
    [: ( Compilation: -- ) ( Run-time: -- r1 )
	@ lp-offset compile-f@local ;]
    ['] to-f: create-local
    ['] compile-pushlocal-f ;

: F^ ( compilation "name" -- a-addr xt; run-time r -- ) \ gforth f-caret
    \g Define local @i{name}, reserve a float at @i{f-addr}, and store @i{r} there.@*
    \g @i{name} execution: @i{( -- f-addr )}.@*
    W^ drop ['] compile-pushlocal-f ;

: D: ( compilation "name" -- a-addr xt; run-time x1 x2 -- ) \ gforth d-colon
    \g Define local @i{name} with the initial value @i{x1 x2}.@*
    \g @i{name} execution: @i{( -- x3 x4 )} push the current value of @i{name}.@*
    \g @code{to @i{name}} run-time: @i{( x5 x6 -- )} change the value of
    \g @i{name} to @i{x5 x6}.@*
    \g @code{+to @i{name}} run-time: @i{( d|ud -- )} increment the value of
    \g @i{name} by @i{d|ud}.
    [: ( Compilation: -- ) ( Run-time: -- x3 x4 )
	@ laddr#, postpone 2@ ;]
    ['] to-d: create-local
    ['] compile-pushlocal-d ;

: D^ ( compilation "name" -- a-addr xt; run-time x1 x2 -- ) \ gforth d-caret
    \g Define local @i{name}, reserve two cells at @i{a-addr}, and store @i{x1 x2} there.@*
    \g @i{name} execution: @i{( -- a-addr )}.@*
    W^ drop ['] compile-pushlocal-d ;

: C: ( compilation "name" -- a-addr xt; run-time c -- ) \ gforth c-colon
    \g Define local @i{name} with the initial value @i{c}.@*
    \g @i{name} execution: @i{( -- c1 )} push the current value of @i{name}.@*
    \g @code{to @i{name}} run-time: @i{( c2 -- )} change the value of
    \g @i{name} to @i{c2}.@*
    \g @code{+to @i{name}} run-time: @i{( n|u -- )} increment the value of
    \g @i{name} by @i{n|u}.
    [: ( Compilation: -- ) ( Run-time: -- c1 )
	@ laddr#, postpone c@ ;]
    ['] to-c: create-local
    ['] compile-pushlocal-c ;

: C^ ( compilation "name" -- a-addr xt; run-time c -- ) \ gforth c-caret
    \g Define local @i{name}, reserve a cell at @i{c-addr}, and store @i{c} there.@*
    \g @i{name} execution: @i{( -- c-addr )}.@*
    W^ drop ['] compile-pushlocal-c ;

: XT: ( compilation "name" -- a-addr xt; run-time xt1 -- ) \ gforth x-t-colon
    \g Define local @i{name}; set @i{name} to execute @i{xt1}.@*
    \g @i{name} execution: execute the xt that
    \g @i{name} has  most recently been set to execute.@*
    \g @code{Is @i{name}} run-time: @i{( xt2 -- )}
    \g set @i{name} to execute @i{xt2}.@*
    \g @code{Action-of @i{name}} run-time: @i{( -- xt3 )}
    \g @i{xt3} is the xt that @i{name} has most recently been set to execute.
    [: ( Compilation: -- ) ( Run-time: .. -- .. )
	@ lp-offset compile-@local postpone execute ;]
    ['] to-xt: create-local
    ['] compile-pushlocal-w ;

Defer default: ' W: is default:

:noname ( c-addr u1 "name" -- a-addr xt ) \ gforth <local>bracket (unnamed)
    W^ drop ['] compile-pushlocal-[ ;

: | ( -- ) \ local-ext bar
    \G Locals defined behind @code{|} are not initialized from the
    \G stack; so the run-time stack effect of the locals definitions
    \G after @word{|} is @code{( -- )}.  
    val-part on ['] val-part-off ;

\ you may want to make comments in a locals definitions group:
synonym \ \ ( compilation 'ccc<newline>' -- ; run-time -- )
\ The actual documentation is in kernel/int.fs

synonym ( ( ( compilation 'ccc<close-paren>' -- ; run-time -- )
\ The actual documentation is in kernel/int.fs

forth definitions
also locals-types

\ these "locals" are used for comparison in TO/create associated vts
c: some-clocal 2drop
d: some-dlocal 2drop
f: some-flocal 2drop
w: some-wlocal 2drop
xt: some-xtlocal 2drop

\ these "locals" create the associated vts
w^ some-waddr 2drop

\ the following gymnastics are for declaring locals without type specifier.
\ we use a catch-all recognizer to do t' rec-new-locals  hat

>r
: rec-new-locals ( caddr u -- [size] nfa )
\ this is the find method of the new-locals vocabulary
\ make a new local with name caddr u; w is ignored
\ the returned nfa denotes a word that produces what W: produces
\ !! do the whole thing without nextname
    2dup nextname
    + 1- c@ '[' = IF
	['] rec-forth defer@ stack> >r
	']' parse evaluate
	r> ['] rec-forth defer@ >stack
	[ r> ] Literal
    ELSE  ['] default: defer@  THEN  nt>rec ;
previous

' rec-new-locals  ' locals-types >wordlist 2 rec-sequence: new-locals

\ and now, finally, the user interface words
: { ( -- hmaddr u wid 0 ) \ gforth open-brace
    \G Start locals definitions.  The Forth-2012 standard name for this
    \G word is @code{@{:}.
    ( >docolloc ) hmsave \ as locals will mess with their own hmtemplate
    get-current
    ['] new-locals ['] rec-forth defer@ >stack
    ['] locals >wordlist set-current
    val-part off
    0 postpone [ ; immediate

synonym {: { ( -- hmaddr u wid 0 ) \ local-ext open-brace-colon
\G Start locals definitions.

locals-types definitions

: } ( hmaddr u wid 0 xt1 ... xtn -- ) \ gforth close-brace
    \G Ends locals definitions.  The Forth-2012 standard name for this
    \G word is @code{:@}}.
    ]
    ['] rec-forth defer@ stack> drop
    begin
	dup
    while
	execute
    repeat
    drop hm,
    maxalign-lp
    set-current
    hmrestore
    activate-locals ;

synonym :} } ( hmaddr u wid 0 xt1 ... xtn -- ) \ gforth colon-close-brace
\g Ends locals definitions.

: -- ( hmaddr u wid 0 ... -- ) \ locals- local-ext dash-dash
    \G During a locals definitions with @word{{:} everything from
    \G @code{--} to @word{:}} is ignored.  This is typically used
    \G when you want to make a locals definition serve double duty as
    \G a stack effect description.
    }
    BEGIN '}' parse dup WHILE
        + 1- c@ dup bl = swap ':' = or UNTIL
    ELSE 2drop THEN ;

forth definitions

\ A few thoughts on automatic scopes for locals and how they can be
\ implemented:

\ We have to combine locals with the control structures. My basic idea
\ was to start the life of a local at the declaration point. The life
\ would end at any control flow join (THEN, BEGIN etc.) where the local
\ is lot live on both input flows (note that the local can still live in
\ other, later parts of the control flow). This would make a local live
\ as long as you expected and sometimes longer (e.g. a local declared in
\ a BEGIN..UNTIL loop would still live after the UNTIL).

\ The following example illustrates the problems of this approach:

\ { z }
\ if
\   { x }
\ begin
\   { y }
\ [ 1 cs-roll ] then
\   ...
\ until

\ x lives only until the BEGIN, but the compiler does not know this
\ until it compiles the UNTIL (it can deduce it at the THEN, because at
\ that point x lives in no thread, but that does not help much). This is
\ solved by optimistically assuming at the BEGIN that x lives, but
\ warning at the UNTIL that it does not. The user is then responsible
\ for checking that x is only used where it lives.

\ The produced code might look like this (leaving out alignment code):

\ >l ( z )
\ ?branch <then>
\ >l ( x )
\ <begin>:
\ >l ( y )
\ lp+!# 8 ( RIP: x,y )
\ <then>:
\ ...
\ lp+!# -4 ( adjust lp to <begin> state )
\ ?branch <begin>
\ lp+!# 4 ( undo adjust )

\ The BEGIN problem also has another incarnation:

\ AHEAD
\ BEGIN
\   x
\ [ 1 CS-ROLL ] THEN
\   { x }
\   ...
\ UNTIL

\ should be legal: The BEGIN is not a control flow join in this case,
\ since it cannot be entered from the top; therefore the definition of x
\ dominates the use. But the compiler processes the use first, and since
\ it does not look ahead to notice the definition, it will complain
\ about it. Here's another variation of this problem:

\ IF
\   { x }
\ ELSE
\   ...
\ AHEAD
\ BEGIN
\   x
\ [ 2 CS-ROLL ] THEN
\   ...
\ UNTIL

\ In this case x is defined before the use, and the definition
\ dominates the use, but the compiler does not know this until it
\ processes the UNTIL. So what should the compiler assume does live at
\ the BEGIN, if the BEGIN is not a control flow join?  See the
\ documentation for our current approach.

\ Implementation:

\ explicit scoping

:noname ( wid -- )
    dead-code @ IF
	set-locals-size-list
    ELSE
	locals-list @ common-list
	dup list-size adjust-locals-size
	locals-list!
    THEN ;
is adjust-locals-list

\ adapt the hooks

: locals-:-hook ( sys -- sys addr xt n )
    \ addr is the nfa of the defined word, xt its xt
    DEFERS :-hook
    ['] here locals-headers
    clear-leave-stack
    0 locals-size !
    0 locals-list!
    dead-code off
    defstart ;

?: ->here dp ! ;

: locals-;-hook ( sys addr xt sys -- sys )
    ?struc
    deactivate-locals
    [: ->here ['] some-waddr lastnt ! ;] locals-headers
    DEFERS ;-hook ;

\ THEN (another control flow from before joins the current one):
\ The new locals-list is the intersection of the current locals-list and
\ the orig-local-list. The new locals-size is the (alignment-adjusted)
\ size of the new locals-list. The following code is generated:
\ lp+!# (current-locals-size - orig-locals-size)
\ <then>:
\ lp+!# (orig-locals-size - new-locals-size)

\ Of course "lp+!# 0" is not generated. Still this is admittedly a bit
\ inefficient, e.g. if there is a locals declaration between IF and
\ ELSE. However, if ELSE generates an appropriate "lp+!#" before the
\ branch, there will be none after the target <then>.

: (then-like) ( orig -- )
    dead-orig =
    if
	>resolve 2drop after-cs-pop
    else
        dead-code @
        if
	    >resolve set-locals-size-list dead-code off
	else \ both live
	    over list-size adjust-locals-size
	    >resolve
	    adjust-locals-list
	then
	pop-stack-state
    then ;

: (begin-like) ( -- )
    defers begin-like
    dead-code @ if
	\ set up an assumption of the locals visible here.  if the
	\ users want something to be visible, they have to declare
	\ that using ASSUME-LIVE
	backedge-locals @ set-locals-size-list
    then
    dead-code off ;

\ AGAIN (the current control flow joins another, earlier one):
\ If the dest-locals-list is not a subset of the current locals-list,
\ issue a warning (see below). The following code is generated:
\ lp+!# (current-local-size - dest-locals-size)
\ branch <begin>

: (again-like) ( stack-state locals-list addr -- stack-state addr )
    over list-size adjust-locals-size
    swap check-begin  POSTPONE unreachable ;

\ UNTIL (the current control flow may join an earlier one or continue):
\ Similar to AGAIN. The new locals-list and locals-size are the current
\ ones. The following code is generated:
\ ?branch-lp+!# <begin> (current-local-size - dest-locals-size)

: (until-like) ( stack-state list addr xt1 xt2 -- )
    \ list and addr are a fragment of a cs-item
    \ xt1 is the conditional branch without lp adjustment, xt2 is with
    >r >r
    locals-size @ third list-size - dup if ( list dest-addr adjustment )
	r> drop r> compile,
	swap <resolve ( list adjustment ) ,
    else ( list dest-addr adjustment )
	drop
	r> compile, <resolve
	r> drop
    then ( list )
    check-begin pop-stack-state ;

: (exit-like) ( -- )
    0 adjust-locals-size ;

' locals-:-hook IS :-hook
' locals-;-hook IS ;-hook
[ifdef] 0-adjust-locals-size
    :noname 0 adjust-locals-size ; is 0-adjust-locals-size
[then]
[ifdef] colon-sys-xt-offset
2 +to colon-sys-xt-offset
[then]

' (then-like)  IS then-like
' (begin-like) IS begin-like
' (again-like) IS again-like
' (until-like) IS until-like
' (exit-like)  IS exit-like

\ The words in the locals dictionary space are not deleted until the end
\ of the current word. This is a bit too conservative, but very simple.

\ There are a few cases to consider: (see above)

\ after AGAIN, AHEAD, EXIT (the current control flow is dead):
\ We have to special-case the above cases against that. In this case the
\ things above are not control flow joins. Everything should be taken
\ over from the live flow. No lp+!# is generated.

\ About warning against uses of dead locals. There are several options:

\ 1) Do not complain (After all, this is Forth;-)

\ 2) Additional restrictions can be imposed so that the situation cannot
\ arise; the programmer would have to introduce explicit scoping
\ declarations in cases like the above one. I.e., complain if there are
\ locals that are live before the BEGIN but not before the corresponding
\ AGAIN (replace DO etc. for BEGIN and UNTIL etc. for AGAIN).

\ 3) The real thing: i.e. complain, iff a local lives at a BEGIN, is
\ used on a path starting at the BEGIN, and does not live at the
\ corresponding AGAIN. This is somewhat hard to implement. a) How does
\ the compiler know when it is working on a path starting at a BEGIN
\ (consider "{ x } if begin [ 1 cs-roll ] else x endif again")? b) How
\ is the usage info stored?

\ For now I'll resort to alternative 2. When it produces warnings they
\ will often be spurious, but warnings should be rare. And better
\ spurious warnings now and then than days of bug-searching.

\ Explicit scoping of locals is implemented by cs-pushing the current
\ locals-list and -size (and an unused cell, to make the size equal to
\ the other entries) at the start of the scope, and restoring them at
\ the end of the scope to the intersection, like THEN does.


\ And here's finally the ANS standard stuff

: (local) ( addr u -- ) \ local paren-local-paren
    \ a little space-inefficient, but well deserved ;-)
    \ In exchange, there are no restrictions whatsoever on using (local)
    \ as long as you use it in a definition
    dup
    if
	nextname POSTPONE { [ also locals-types ] W: } [ previous ]
    else
	2drop
    endif ;

also locals-types
: noname-w: ( -- n )
    \ generate local; its offset is n
    POSTPONE { 0 0 nextname W: ['] latestxt locals-headers >r } r> @ ;
previous


?: >extra ( nt -- addr )
>namehm @ >hmextra ;

: >definer ( xt -- definer ) \ gforth
    \G @var{Definer} is a unique identifier for the way the @var{xt}
    \G was defined.  Words defined with different @code{does>}-codes
    \G have different definers.  The definer can be used for
    \G comparison and in @code{definer!}.
    dup >code-address case
        dodoes:     of >extra @ >body 1 or endof
        do;abicode: of >extra @ >body 2 or endof
        nip dup
    endcase ;

: locals| ( ... "name ..." -- ) \ local-ext locals-bar
    \G Don't use @samp{locals| this read can't you|}!  Use @code{@{:
    \G you can read this :@}} instead.! A portable and free @word{{:}
    \G implementation can be found in @file{compat/xlocals.fs}.
    BEGIN
	parse-name 2dup s" |" str= 0=
    WHILE
	(local)
    REPEAT
    drop 0 (local) ; immediate restrict

\ POSTPONEing locals

:noname ( locals-nt -- )
    dup name>interpret >does-code case
	[ comp' some-clocal  drop >does-code ] literal of name-compsem postpone lit, endof
	[ comp' some-dlocal  drop >does-code ] literal of name-compsem postpone 2lit, endof
	[ comp' some-flocal  drop >does-code ] literal of name-compsem postpone flit, endof
	[ comp' some-wlocal  drop >does-code ] literal of name-compsem postpone lit, endof
	[ comp' some-xtlocal drop >does-code ] literal of >body @ lp-offset compile-@local postpone compile, endof
	no-post
    endcase ; is locals-post,
' locals-post, translate-local >body 2 th! \ replace stub

\ we define peephole using locals, so it needs to be here

: peephole ( xt1 xt2 "name" -- )
    {: | xts[ 3 cells ] :}
    xts[ 2! ' xts[ 2 th!
    xts[ 3 cells peephole-opts $+! ;

' lp+n ' @ peephole @localn
' lp+n ' ! peephole !localn
' lp+n ' +! peephole +!localn
