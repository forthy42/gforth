\ Local variables are quite important for writing readable programs, but
\ IMO (anton) they are the worst part of the standard. There they are very
\ restricted and have an ugly interface.

\ So, we implement the locals wordset, but do not recommend using
\ locals-ext (which is a really bad user interface for locals).

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

\ Other things about the internals are pretty unclear now.

\ Currently locals may only be
\ defined at the outer level and TO is not supported.

include search-order.fs
include float.fs

: compile-@local ( n -- )
 case
    0       of postpone @local0 endof
    1 cells of postpone @local1 endof
    2 cells of postpone @local2 endof
    3 cells of postpone @local3 endof
   ( otherwise ) dup postpone @local# ,
 endcase ;

: compile-f@local ( n -- )
 case
    0        of postpone f@local0 endof
    1 floats of postpone f@local1 endof
   ( otherwise ) dup postpone f@local# ,
 endcase ;

\ the locals stack grows downwards (see primitives)
\ of the local variables of a group (in braces) the leftmost is on top,
\ i.e. by going onto the locals stack the order is reversed.
\ there are alignment gaps if necessary.
\ lp must have the strictest alignment (usually float) across calls;
\ for simplicity we align it strictly for every group.

slowvoc @
slowvoc on \ we want a linked list for the vocabulary locals
vocabulary locals \ this contains the local variables
' locals >body ' locals-list >body !
slowvoc !

create locals-buffer 1000 allot \ !! limited and unsafe
    \ here the names of the local variables are stored
    \ we would have problems storing them at the normal dp

variable locals-dp \ so here's the special dp for locals.

: alignlp-w ( n1 -- n2 )
    \ cell-align size and generate the corresponding code for aligning lp
    aligned dup adjust-locals-size ;

: alignlp-f ( n1 -- n2 )
    faligned dup adjust-locals-size ;

\ a local declaration group (the braces stuff) is compiled by calling
\ the appropriate compile-pushlocal for the locals, starting with the
\ righmost local; the names are already created earlier, the
\ compile-pushlocal just inserts the offsets from the frame base.

: compile-pushlocal-w ( a-addr -- ) ( run-time: w -- )
\ compiles a push of a local variable, and adjusts locals-size
\ stores the offset of the local variable to a-addr
    locals-size @ alignlp-w cell+ dup locals-size !
    swap !
    postpone >l ;

: compile-pushlocal-f ( a-addr -- ) ( run-time: f -- )
    locals-size @ alignlp-f float+ dup locals-size !
    swap !
    postpone f>l ;

: compile-pushlocal-d ( a-addr -- ) ( run-time: w1 w2 -- )
    locals-size @ alignlp-w cell+ cell+ dup locals-size !
    swap !
    postpone swap postpone >l postpone >l ;

: compile-pushlocal-c ( a-addr -- ) ( run-time: w -- )
    -1 chars compile-lp+!
    locals-size @ swap !
    postpone lp@ postpone c! ;

: create-local ( " name" -- a-addr )
	\ defines the local "name"; the offset of the local shall be stored in a-addr
    create
	immediate
	here 0 , ( place for the offset ) ;

: lp-offset ( n1 -- n2 )
\ converts the offset from the frame start to an offset from lp and
\ i.e., the address of the local is lp+locals_size-offset
  locals-size @ swap - ;

: lp-offset, ( n -- )
\ converts the offset from the frame start to an offset from lp and
\ adds it as inline argument to a preceding locals primitive
  lp-offset , ;

vocabulary locals-types \ this contains all the type specifyers, -- and }
locals-types definitions

: W:
    create-local ( "name" -- a-addr xt )
	\ xt produces the appropriate locals pushing code when executed
	['] compile-pushlocal-w
    does> ( Compilation: -- ) ( Run-time: -- w )
        \ compiles a local variable access
	@ lp-offset compile-@local ;

: W^
    create-local ( "name" -- a-addr xt )
	['] compile-pushlocal-w
    does> ( Compilation: -- ) ( Run-time: -- w )
	postpone laddr# @ lp-offset, ;

: F:
    create-local ( "name" -- a-addr xt )
	['] compile-pushlocal-f
    does> ( Compilation: -- ) ( Run-time: -- w )
	@ lp-offset compile-f@local ;

: F^
    create-local ( "name" -- a-addr xt )
	['] compile-pushlocal-f
    does> ( Compilation: -- ) ( Run-time: -- w )
	postpone laddr# @ lp-offset, ;

: D:
    create-local ( "name" -- a-addr xt )
	['] compile-pushlocal-d
    does> ( Compilation: -- ) ( Run-time: -- w )
	postpone laddr# @ lp-offset, postpone 2@ ;

: D^
    create-local ( "name" -- a-addr xt )
	['] compile-pushlocal-d
    does> ( Compilation: -- ) ( Run-time: -- w )
	postpone laddr# @ lp-offset, ;

: C:
    create-local ( "name" -- a-addr xt )
	['] compile-pushlocal-c
    does> ( Compilation: -- ) ( Run-time: -- w )
	postpone laddr# @ lp-offset, postpone c@ ;

: C^
    create-local ( "name" -- a-addr xt )
	['] compile-pushlocal-c
    does> ( Compilation: -- ) ( Run-time: -- w )
	postpone laddr# @ lp-offset, ;

\ you may want to make comments in a locals definitions group:
' \ alias \ immediate
' ( alias ( immediate

forth definitions

\ the following gymnastics are for declaring locals without type specifier.
\ we exploit a feature of our dictionary: every wordlist
\ has it's own methods for finding words etc.
\ So we create a vocabulary new-locals, that creates a 'w:' local named x
\ when it is asked if it contains x.

also locals-types

: new-locals-find ( caddr u w -- nfa )
\ this is the find method of the new-locals vocabulary
\ make a new local with name caddr u; w is ignored
\ the returned nfa denotes a word that produces what W: produces
\ !! do the whole thing without nextname
    drop nextname
    ['] W: >name ;

previous

: new-locals-reveal ( -- )
  true abort" this should not happen: new-locals-reveal" ;

create new-locals-map ' new-locals-find A, ' new-locals-reveal A,

vocabulary new-locals
new-locals-map ' new-locals >body cell+ A! \ !! use special access words

variable old-dpp

\ and now, finally, the user interface words
: { ( -- addr wid 0 )
    dp old-dpp !
    locals-dp dpp !
    also new-locals
    also get-current locals definitions  locals-types
    0 TO locals-wordlist
    0 postpone [ ; immediate

locals-types definitions

: } ( addr wid 0 a-addr1 xt1 ... -- )
    \ ends locals definitions
    ] old-dpp @ dpp !
    begin
	dup
    while
	execute
    repeat
    drop
    locals-size @ alignlp-f locals-size ! \ the strictest alignment
    set-current
    previous previous
    locals-list TO locals-wordlist ;

: -- ( addr wid 0 ... -- )
    }
    [char] } word drop ;

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

\ In this case x is defined before the use, and the definition dominates
\ the use, but the compiler does not know this until it processes the
\ UNTIL. So what should the compiler assume does live at the BEGIN, if
\ the BEGIN is not a control flow join? The safest assumption would be
\ the intersection of all locals lists on the control flow
\ stack. However, our compiler assumes that the same variables are live
\ as on the top of the control flow stack. This covers the following case:

\ { x }
\ AHEAD
\ BEGIN
\   x
\ [ 1 CS-ROLL ] THEN
\   ...
\ UNTIL

\ If this assumption is too optimistic, the compiler will warn the user.

\ Implementation: migrated to kernal.fs

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

\ explicit scoping

: scope ( -- scope )
 cs-push-part scopestart ; immediate

: endscope ( scope -- )
 scope?
 drop
 locals-list @ common-list
 dup list-size adjust-locals-size
 locals-list ! ; immediate

\ adapt the hooks

: locals-:-hook ( sys -- sys addr xt n )
    \ addr is the nfa of the defined word, xt its xt
    DEFERS :-hook
    last @ lastcfa @
    clear-leave-stack
    0 locals-size !
    locals-buffer locals-dp !
    0 locals-list !
    dead-code off
    defstart ;

: locals-;-hook ( sys addr xt sys -- sys )
    def?
    0 TO locals-wordlist
    0 adjust-locals-size ( not every def ends with an exit )
    lastcfa ! last !
    DEFERS ;-hook ;

' locals-:-hook IS :-hook
' locals-;-hook IS ;-hook

\ The words in the locals dictionary space are not deleted until the end
\ of the current word. This is a bit too conservative, but very simple.

\ There are a few cases to consider: (see above)

\ after AGAIN, AHEAD, EXIT (the current control flow is dead):
\ We have to special-case the above cases against that. In this case the
\ things above are not control flow joins. Everything should be taken
\ over from the live flow. No lp+!# is generated.

\ !! The lp gymnastics for UNTIL are also a real problem: locals cannot be
\ used in signal handlers (or anything else that may be called while
\ locals live beyond the lp) without changing the locals stack.

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

: (local) ( addr u -- )
    \ a little space-inefficient, but well deserved ;-)
    \ In exchange, there are no restrictions whatsoever on using (local)
    \ as long as you use it in a definition
    dup
    if
	nextname POSTPONE { [ also locals-types ] W: } [ previous ]
    else
	2drop
    endif ;

: >definer ( xt -- definer )
    \ this gives a unique identifier for the way the xt was defined
    \ words defined with different does>-codes have different definers
    \ the definer can be used for comparison and in definer!
    dup >code-address [ ' bits >code-address ] Literal =
    \ !! this definition will not work on some implementations for `bits'
    if  \ if >code-address delivers the same value for all does>-def'd words
	>does-code 1 or \ bit 0 marks special treatment for does codes
    else
	>code-address
    then ;

: definer! ( definer xt -- )
    \ gives the word represented by xt the behaviour associated with definer
    over 1 and if
	does-code!
    else
	code-address!
    then ;

\ !! untested
: TO ( c|w|d|r "name" -- )
\ !! state smart
 0 0 0. 0.0e0 { c: clocal w: wlocal d: dlocal f: flocal }
 ' dup >definer
 state @ 
 if
   case
     [ ' locals-wordlist >definer ] literal \ value
     OF >body POSTPONE Aliteral POSTPONE ! ENDOF
     [ ' clocal >definer ] literal
     OF POSTPONE laddr# >body @ lp-offset, POSTPONE c! ENDOF
     [ ' wlocal >definer ] literal
     OF POSTPONE laddr# >body @ lp-offset, POSTPONE ! ENDOF
     [ ' dlocal >definer ] literal
     OF POSTPONE laddr# >body @ lp-offset, POSTPONE d! ENDOF
     [ ' flocal >definer ] literal
     OF POSTPONE laddr# >body @ lp-offset, POSTPONE f! ENDOF
     abort" can only store TO value or local value"
   endcase
 else
   [ ' locals-wordlist >definer ] literal =
   if
     >body !
   else
     abort" can only store TO value"
   endif
 endif ; immediate

: locals|
    BEGIN
	name 2dup s" |" compare 0<>
    WHILE
	(local)
    REPEAT
    drop 0 (local) ;  immediate restrict
