\ get some data on potential (partial) inlining

\ Copyright (C) 2004,2007 Free Software Foundation, Inc.

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


\ relies on some Gforth internals

\ !! assumption: each file is included only once; otherwise you get
\ the counts for just one of the instances of the file.  This can be
\ fixed by making sure that every source position occurs only once as
\ a profile point.

true constant count-calls? \ do some profiling of colon definitions etc.

\ for true COUNT-CALLS?:

\ What data do I need for evaluating the effectiveness of (partial) inlining?

\ static and dynamic counts of everything:

\ original BB length (histogram and average)
\ BB length with partial inlining (histogram and average)
\   since we cannot partially inline library calls, we use a parameter
\   that represents the amount of partial inlining we can expect there.
\ number of tail calls (original and after partial inlining)
\ number of calls (original and after partial inlining)
\ reason for BB end: call, return, execute, branch

\ how many static calls are there to a word?  How many of the dynamic
\ calls call just a single word?

\ how much does inlining called-once words help?
\ how much does inlining words without control flow help?
\ how much does partial inlining help?
\ what's the overlap?
\ optimizing return-to-returns (tail calls), return-to-calls, call-to-calls

struct
    cell% field list-next
end-struct list%

list%
    cell% 2* field profile-count \ how often this profile point is performed
    cell% 2* field profile-sourcepos
    cell% field profile-char \ character position in line
    cell% field profile-bblen \ number of primitives in BB
    cell% field profile-bblenpi \ bblen after partial inlining
    cell% field profile-callee-postlude \ 0 or (for calls) callee postlude len
    cell% field profile-tailof \ 0 or (for tail bbs) pointer to coldef bb
    cell% field profile-colondef? \ is this a colon definition start
    cell% field profile-calls \ static calls to the colon def (calls%)
    cell% field profile-straight-line \ may contain calls, but no other CF
    cell% field profile-calls-from \ static calls in the colon def
    cell% field profile-exits \ number of exits in this colon def
    cell% 2* field profile-execs \ number of EXECUTEs etc. of this colon def
    cell% field profile-prelude \ first BB-len of colon def (incl. callee)
    cell% field profile-postlude \ last BB-len of colon def (incl. callee)
end-struct profile% \ profile point 

list%
    cell% field calls-call \ ptr to profile point of bb containing the call
end-struct calls%

variable profile-points \ linked list of profile%
0 profile-points !
variable next-profile-point-p \ the address where the next pp will be stored
profile-points next-profile-point-p !
variable last-colondef-profile \ pointer to the pp of last colon definition
variable current-profile-point
variable library-calls 0 library-calls ! \ list of calls to library colon defs
variable in-compile,? in-compile,? off
variable all-bbs 0 all-bbs ! \ list of all basic blocks

\ list stuff

: map-list ( ... list xt -- ... )
    { xt } begin { list }
	list while
	    list xt execute
	    list list-next @
    repeat ;

: drop-1+ drop 1+ ;

: list-length ( list -- u )
    0 swap ['] drop-1+ map-list ;

: insert-list ( listp listpp -- )
    \ insert list node listp into list pointed to by listpp in front
    tuck @ over list-next !
    swap ! ;

: insert-list-end ( listp listppp -- )
    \ insert list node listp into list, with listppp indicating the
    \ position to insert at, and indicating the position behind the
    \ new element afterwards.
    2dup @ insert-list
    swap list-next swap ! ;

\ calls

: new-call ( profile-point -- call )
    calls% %alloc tuck calls-call ! ;

\ profile-point stuff   

: new-profile-point ( -- addr )
    profile% %alloc >r
    0. r@ profile-count 2!
    current-sourcepos r@ profile-sourcepos 2!
    >in @ r@ profile-char !
    0 r@ profile-callee-postlude !
    0 r@ profile-tailof !
    r@ profile-colondef? off
    0 r@ profile-bblen !
    -100000000 r@ profile-bblenpi !
    current-profile-point @ profile-bblenpi @ -100000000 = if
	current-profile-point @ dup profile-bblen @ swap profile-bblenpi !
    endif
    0 r@ profile-calls !
    r@ profile-straight-line on
    0 r@ profile-calls-from !
    0 r@ profile-exits !
    0. r@ profile-execs 2!
    0 r@ profile-prelude !
    0 r@ profile-postlude !
    r@ next-profile-point-p insert-list-end
    r@ current-profile-point !
    r@ new-call all-bbs insert-list
    r> ;

: print-profile ( -- )
    profile-points @ begin
	dup while
	    dup >r
	    r@ profile-sourcepos 2@ .sourcepos ." :"
	    r@ profile-char @ 0 .r ." : "
	    r@ profile-count 2@ 0 d.r cr
	    r> list-next @
    repeat
    drop ;

: print-profile-coldef ( -- )
    profile-points @ begin
	dup while
	    dup >r
	    r@ profile-colondef? @ if
		r@ profile-sourcepos 2@ .sourcepos ." :"
		r@ profile-char @ 3 .r ." : "
		r@ profile-count 2@ 10 d.r
		r@ profile-straight-line @ space 2 .r
		r@ profile-calls @ list-length 4 .r
		cr
	    endif
	    r> list-next @
    repeat
    drop ;

: 1= ( u -- f )
    1 = ;

: 2= ( u -- f )
    2 = ;

: 3= ( u -- f )
    3 = ;

: 1u> ( u -- f )
    1 u> ;

: call-count+ ( ud1 callp -- ud2 )
    calls-call @ profile-count 2@ d+ ;

: count-dyncalls ( calls -- ud )
    0. rot ['] call-count+ map-list ;

: add-calls ( statistics1 xt-test profpp -- statistics2 xt-test )
    \ add statistics for callee profpp up, if the number of static
    \ calls to profpp satisfies xt-test ( u -- f ); see below for what
    \ statistics are computed.
    { xt-test p }
    p profile-colondef? @ if
	p profile-calls @ { calls }
	calls list-length { stat }
	stat xt-test execute if
	    { d: ud-dyn-callee d: ud-dyn-caller u-stat u-exec-callees u-callees }
	    ud-dyn-callee p profile-count 2@ 2dup { d: de } d+
	    ud-dyn-caller calls count-dyncalls 2dup { d: dr } d+
	    u-stat stat +
	    u-exec-callees de dr d<> -
	    u-callees 1+
	endif
    endif
    xt-test ;

: print-stat-line ( xt -- )
    >r 0. 0. 0 0 0 r> profile-points @ ['] add-calls map-list drop
    ( ud-dyn-callee ud-dyn-caller u-stat )
    6 u.r 7 u.r 7 u.r 12 ud.r 12 ud.r space ;

: print-library-stats ( -- )
    library-calls @ list-length 20 u.r \ static callers
    library-calls @ count-dyncalls 12 ud.r \ dynamic callers
    13 spaces ;

: bblen+ ( u1 callp -- u2 )
    calls-call @ profile-bblen @ + ;

: dyn-bblen+ ( ud1 callp -- ud2 )
    calls-call @ dup profile-count 2@ rot profile-bblen @ 1 m*/ d+ ;
    
: print-bb-statistics ( -- )
    ." static     dynamic" cr
    all-bbs @ list-length 6 u.r all-bbs @ count-dyncalls 12 ud.r ."  basic blocks" cr
    0 all-bbs @ ['] bblen+ map-list 6 u.r
    0. all-bbs @ ['] dyn-bblen+ map-list 12 ud.r ."  primitives" cr
    ;

: print-statistics ( -- )
    ." callee exec'd static  dyn-caller  dyn-callee   condition" cr
    ['] 0=  print-stat-line ." calls to coldefs with 0 callers" cr
    ['] 1=  print-stat-line ." calls to coldefs with 1 callers" cr
    ['] 2=  print-stat-line ." calls to coldefs with 2 callers" cr
    ['] 3=  print-stat-line ." calls to coldefs with 3 callers" cr
    ['] 1u> print-stat-line ." calls to coldefs with >1 callers" cr
    print-library-stats     ." library calls" cr
    print-bb-statistics
    ;

: dinc ( profilep -- )
    \ increment double pointed to by d-addr
    profile-count dup 2@ 1. d+ rot 2! ;

: profile-this ( -- )
    in-compile,? @ in-compile,? on
    new-profile-point POSTPONE literal POSTPONE dinc
    in-compile,? ! ;

\ Various words trigger PROFILE-THIS.  In order to avoid getting
\ several calls to PROFILE-THIS from a compiling word (like ?EXIT), we
\ just wait until the next word is parsed by the text interpreter (in
\ compile state) and call PROFILE-THIS only once then.  The whole
\ BEFORE-WORD hooking etc. is there for this.

\ The reason that we do this is because we use the source position for
\ the profiling information, and there's only one source position for
\ ?EXIT.  If we used the threaded code position instead, we would see
\ that ?EXIT compiles to several threaded-code words, and could use
\ different profile points for them.  However, usually dealing with
\ the source is more practical.

\ Another benefit is that we can ask for profiling anywhere in a
\ control-flow word (even before it compiles its own stuff).

\ Potential problem: Consider "COMPILING ] [" where COMPILING compiles
\ a whole colon definition (and triggers our profiler), but during the
\ compilation of the colon definition there is no parsing.  Afterwards
\ you get interpret state at first (no profiling, either), but after
\ the "]" you get parsing in compile state, and PROFILE-THIS gets
\ called (and compiles code that is never executed).  It would be
\ better if we had a way of knowing whether we are in a colon def or
\ not (and used that knowledge instead of STATE).

Defer before-word-profile ( -- )
' noop IS before-word-profile

: before-word1 ( -- )
    before-word-profile defers before-word ;

' before-word1 IS before-word

: profile-this-compiling ( -- )
    state @ if
	profile-this
	['] noop IS before-word-profile
    endif ;

: cock-profiler ( -- )
    \ as in cock the gun - pull the trigger
    ['] profile-this-compiling IS before-word-profile
    [ count-calls? ] [if] \ we are at a non-colondef profile point
	last-colondef-profile @ profile-straight-line off
    [endif]
;

: hook-profiling-into ( "name" -- )
    \ make (deferred word) "name" call cock-profiler, too
    ' >body >r :noname
    POSTPONE cock-profiler
    r@ @ compile, \ old hook behaviour
    POSTPONE ;
    r> ! ; \ change hook behaviour

: note-execute ( -- )
    \ end of BB due to execute, dodefer, perform
    profile-this \ should actually happen after the word, but the
                 \ error is probably small
;

: note-call ( addr -- )
    \ addr is the body address of a called colon def or does handler
    dup ['] (does>2) >body = if \ adjust does handler address
	4 cells here 1 cells - +!
    endif
    { addr }
    current-profile-point @ { lastbb }
    profile-this
    current-profile-point @ { thisbb }
    thisbb new-call { call-node }
    over 3 cells + @ ['] dinc >body = if
	\ non-library call
    !! update profile-bblenpi of last and current pp
	addr cell+ @ { callee-pp }
	callee-pp profile-postlude @ thisbb profile-callee-postlude !
	call-node callee-pp profile-calls insert-list
    else ( addr call-prof-point )
	call-node library-calls insert-list
    endif ;

: prof-compile, ( xt -- )
    in-compile,? @ if
	DEFERS compile, EXIT
    endif
    1 current-profile-point @ profile-bblen +!
    dup CASE
	['] execute of note-execute endof
	['] perform of note-execute endof
	dup >does-code if
	    dup >does-code note-call
	then
	dup >code-address CASE
	    docol:   OF dup >body note-call ENDOF
	    dodefer: OF note-execute ENDOF
	    \ dofield: OF >body @ POSTPONE literal ['] + peephole-compile, EXIT ENDOF
	    \ code words and ;code-defined words (code words could be optimized):
	ENDCASE
    ENDCASE
    DEFERS compile, ;

: :-hook-profile ( -- )
    defers :-hook
    next-profile-point-p @
    profile-this
    @ dup last-colondef-profile ! ( current-profile-point )
    1 over profile-bblenpi !
    profile-colondef? on ;

: exit-hook-profile ( -- )
    defers exit-hook
    1 last-colondef-profile @ profile-exits +! ;

: ;-hook-profile ( -- )
    \ ;-hook is called before the POSTPONE EXIT
    defers ;-hook
    last-colondef-profile @ { col }
    current-profile-point @ { bb }
    col profile-bblen @ col profile-prelude +!
    col profile-exits @ 0= if
	col bb profile-tailof !
	bb profile-bblen @ bb profile-callee-postlude @ +
	col profile-postlude !
	1 bb profile-bblenpi !
	\ not counting the EXIT
    endif ;

hook-profiling-into then-like
\ hook-profiling-into if-like    \ subsumed by other-control-flow
\ hook-profiling-into ahead-like \ subsumed by other-control-flow
hook-profiling-into other-control-flow
hook-profiling-into begin-like
hook-profiling-into again-like
hook-profiling-into until-like
' :-hook-profile IS :-hook
' prof-compile, IS compile,
' exit-hook-profile IS exit-hook
' ;-hook-profile IS ;-hook
