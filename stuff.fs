\ miscelleneous words

\ Copyright (C) 1996,1997,1998,2000 Free Software Foundation, Inc.

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

require glocals.fs

' require alias needs ( ... "name" -- ... ) \ gforth
\G An alias for @code{require}; exists on other systems (e.g., Win32Forth).
\ needs is an F-PC name. we will probably switch to 'needs' in the future

\ a little more compiler security

\ currently not used by Gforth, but maybe by add-ons e.g., the 486asm
AUser CSP

: !CSP ( -- )
    sp@ csp ! ;

: ?CSP ( -- )
    sp@ csp @ <> -22 and throw ;

\ DMIN and DMAX

: dmin ( d1 d2 -- d ) \ double d-min
    2over 2over d> IF  2swap  THEN 2drop ;


: dmax ( d1 d2 -- d ) \ double d-max
    2over 2over d< IF  2swap  THEN 2drop ;

\ shell commands

0 Value $? ( -- n ) \ gforth dollar-question
\G @code{Value} -- the exit status returned by the most recently executed
\G @code{system} command.

: system ( c-addr u -- ) \ gforth
\G Pass the string specified by @var{c-addr u} to the host operating system
\G for execution in a sub-shell.
    (system) throw TO $? ;

: sh ( "..." -- ) \ gforth
\G Parse a string and use @code{system} to pass it to the host
\G operating system for execution in a sub-shell.
    '# parse cr system ;

\ stuff

: ]L ( compilation: n -- ; run-time: -- n ) \ gforth
    \G equivalent to @code{] literal}
    ] postpone literal ;

[ifundef] in-dictionary?
: in-dictionary? ( x -- f )
    forthstart dictionary-end within ;
[endif]

: in-return-stack? ( addr -- f )
    rp0 @ swap - [ forthstart 6 cells + ]L @ u< ;

\ const-does>

: compile-literals ( w*u u -- ; run-time: -- w*u ) recursive
    \ compile u literals, starting with the bottommost one
    ?dup-if
	swap >r 1- compile-literals
	r> POSTPONE literal
    endif ;

: compile-fliterals ( r*u u -- ; run-time: -- w*u ) recursive
    \ compile u fliterals, starting with the bottommost one
    ?dup-if
	{ F: r } 1- compile-fliterals
	r POSTPONE fliteral
    endif ;

: (const-does>) ( w*uw r*ur uw ur target "name" -- )
    \ define a colon definition "name" containing w*uw r*ur as
    \ literals and a call to target.
    { uw ur target }
    header docol: cfa, \ start colon def without stack junk
    ur compile-fliterals uw compile-literals
    target compile, POSTPONE exit reveal ;

: const-does> ( run-time: w*uw r*ur uw ur "name" -- )
    \G Defines @var{name} and returns.@sp 0
    \G @var{name} execution: pushes @var{w*uw r*ur}, then performs the
    \G code following the @code{const-does>}.
    here >r 0 POSTPONE literal
    POSTPONE (const-does>)
    POSTPONE ;
    noname : POSTPONE rdrop
    lastxt r> cell+ ! \ patch the literal
; immediate

\ !! rewrite slurp-file using slurp-fid
: slurp-file ( c-addr1 u1 -- c-addr2 u2 )
    \G @var{c-addr1 u1} is the filename, @var{c-addr2 u2} is the file's contents
    r/o bin open-file throw >r
    r@ file-size throw abort" file too large"
    dup allocate throw swap
    2dup r@ read-file throw over <> abort" could not read whole file"
    r> close-file throw ;

: slurp-fid { fid -- addr u }
    \G @var{addr u} is the content of the file @var{fid}
    0 0 begin ( awhole uwhole )
	dup 1024 + dup >r extend-mem ( anew awhole uwhole R: unew )
	rot r@ fid read-file throw ( awhole uwhole uread R: unew )
	r> 2dup =
    while ( awhole uwhole uread unew )
	2drop
    repeat
    - + dup >r resize throw r> ;

\ ]] ... [[

: compile-literal ( n -- )
    postpone literal ;

: [[ ( -- )
\G switch from postpone state to compile state
    \ this is only a marker; it is never really interpreted
    compile-only-error ; immediate

: postponer ( c-addr u -- )
    2dup find-name dup if ( c-addr u nt )
	nip nip name>comp
	2dup [comp'] [[ d= if
	    2drop ['] compiler is parser
	else
	    postpone,
	endif
    else
	drop
	2dup snumber? dup if
	    0> IF
		swap postpone literal postpone compile-literal
	    THEN
	    postpone Literal postpone compile-literal
	    2drop
	ELSE
	    drop no.extensions
	THEN
    then ;

: ]] ( -- )
    \ switch into postpone state
    ['] postponer is parser state on ; immediate restrict

