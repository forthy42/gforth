\ get some data on potential (partial) inlining

\ Copyright (C) 2004 Free Software Foundation, Inc.

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
\ Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111, USA.


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
    cell% 2* field profile-count
    cell% 2* field profile-sourcepos
    cell%    field profile-char \ character position in line
    count-calls? [if]
	cell% field profile-colondef? \ is this a colon definition start
	cell% field profile-calls \ static calls to the colon def (calls%)
	cell% field profile-straight-line \ may contain calls, but no other CF
	cell% field profile-calls-from \ static calls in the colon def
    [endif]
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
variable library-calls \ list of calls to library colon defs

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
    [ count-calls? ] [if]
	r@ profile-colondef? off
	0 r@ profile-calls !
	r@ profile-straight-line on
	0 r@ profile-calls-from !
    [endif]
    r@ next-profile-point-p insert-list-end
    r@ current-profile-point !
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

: add-calls ( ud-dyn-callee1 ud-dyn-caller1 u-stat1 xt-test profpp --
              ud-dyn-callee2 ud-dyn-caller2 u-stat2 xt-test )
    \ add the static and dynamic call counts to profpp up, if the
    \ number of static calls to profpp satisfies xt-test ( u -- f )
    { xt-test p }
    p profile-colondef? @ if ( u-dyn1 u-stat1 )
	p profile-calls @ { calls }
	calls list-length { stat }
	stat xt-test execute if ( u-dyn u-stat )
	    stat + >r
	    0. calls ['] call-count+ map-list d+ 2>r
	    p profile-count 2@ d+
	    2r> r>
	endif
    endif
    xt-test ;

: print-stat-line ( xt -- )
    >r 0. 0. 0 r> profile-points @ ['] add-calls map-list drop
    ( ud-dyn-callee ud-dyn-caller u-stat )
    7 u.r 12 ud.r 12 ud.r space ;

: print-statistics ( -- )
    ."  static  dyn-caller  dyn-callee   condition" cr
    ['] 0=  print-stat-line ." calls to coldefs with 0 callers" cr
    ['] 1=  print-stat-line ." calls to coldefs with 1 callers" cr
    ['] 2=  print-stat-line ." calls to coldefs with 2 callers" cr
    ['] 3=  print-stat-line ." calls to coldefs with 3 callers" cr
    ['] 1u> print-stat-line ." calls to coldefs with >1 callers" cr
    ;

: dinc ( profilep -- )
    \ increment double pointed to by d-addr
    profile-count dup 2@ 1. d+ rot 2! ;

: profile-this ( -- )
    new-profile-point POSTPONE literal POSTPONE dinc ;

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
    \ end of BB due to execute
;

: note-call ( addr -- )
    \ addr is the body address of a called colon def or does handler
    dup 3 cells + @ ['] dinc >body = if ( addr )
	current-profile-point @ new-call over cell+ @ profile-calls insert-list
    endif
    drop ;
    
: prof-compile, ( xt -- )
    dup >does-code if
	dup >does-code note-call
    then
    dup >code-address CASE
	docol:   OF dup >body note-call ENDOF
	dodefer: OF note-execute ENDOF
	dofield: OF >body @ ['] lit+ peephole-compile, , EXIT ENDOF
	\ dofield: OF >body @ POSTPONE literal ['] + peephole-compile, EXIT ENDOF
	\ code words and ;code-defined words (code words could be optimized):
	dup in-dictionary? IF drop POSTPONE literal ['] execute peephole-compile, EXIT THEN
    ENDCASE
    DEFERS compile, ;

\ hook-profiling-into then-like
\ \ hook-profiling-into if-like    \ subsumed by other-control-flow
\ \ hook-profiling-into ahead-like \ subsumed by other-control-flow
\ hook-profiling-into other-control-flow
\ hook-profiling-into begin-like
\ hook-profiling-into again-like
\ hook-profiling-into until-like

: :-hook-profile ( -- )
    defers :-hook
    next-profile-point-p @
    profile-this
    @ dup last-colondef-profile !
    profile-colondef? on ;

' :-hook-profile IS :-hook
' prof-compile, IS compile,