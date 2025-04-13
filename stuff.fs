\ miscelleneous words

\ Authors: Anton Ertl, Bernd Paysan, Neal Crook
\ Copyright (C) 1996,1997,1998,2000,2003,2004,2005,2006,2007,2008,2009,2010,2011,2012,2013,2014,2015,2016,2017,2018,2019,2020,2021,2022,2023,2024 Free Software Foundation, Inc.

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

: save-mem-dict ( addr1 u -- addr2 u ) \ gforth
    \G Copy the memory block @i{addr1 u} to a newly @code{allot}ed
    \G memory block of size @i{u}; the target memory block starts at
    \G @i{addr2}.
    here over 2swap mem, ;

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

\ pow2? ctz

[undefined] log2 [if]
: log2 ( x -- n )
    \ integer binary logarithm
    -1 swap begin
	dup while
	    1 rshift 1 under+ repeat
    drop ;
[then]

: ctz ( x -- u ) \ gforth c-t-z
    \g count trailing zeros in binary representation of x
    dup if
	dup negate and log2 exit then
    drop 8 cells ;

\ extend-mem free-mem-var

: extend-mem	( addr1 u1 u -- addr addr2 u2 ) \ gforth-experimental
    \g @i{Addr1 u1} is a memory block in heap memory.  Increase the
    \g size of this memory block by @i{u} aus, possibly reallocating
    \g it.  @i{C-addr2 u2} is the resulting memory block
    \g (@i{u2}=@i{u1}+@i{u}), @i{addr} is the start of the @i{u}
    \g additional aus (@i{addr}=@i{addr2}+@i{u1}).
    over >r + dup >r resize throw
    r> over r> + -rot ;

: free-mem-var ( addr -- ) \ gforth-experimental
    \g @i{Addr} is the address of a 2variable containing a memory
    \g block descriptor @i{c-addr u} in heap memory;
    \g @code{free-mem-var} frees the memory block and stores 0 0 in
    \g the 2variable.
    dup 2@ drop dup
    if ( addr mem-start )
	free throw
	0 0 rot 2!
    else
	2drop
    then ;

\ multiple values to and from return stack

: n>r ( x1 .. xn n -- R:xn..R:x1 R:n ) \ tools-ext n-to-r
    \G In Standard Forth, the order of items on the return stack is
    \G not specified, and the only thing you can do with the items on
    \G the return stack is to use @code{nr>}
    scope r> { n ret }
    0 BEGIN dup n < WHILE swap >r 1+ REPEAT >r
    ret >r endscope ;
: nr> ( R:xn..R:x1 R:n -- x1 .. xn n ) \ tools-ext n-r-from
    \G In Standard Forth, the order of items on the return stack is
    \G not specified, and the only thing you can do with the items on
    \G the return stack is to use @code{nr>}
    scope r> r> { ret n }
    0 BEGIN dup n < WHILE r> swap 1+ REPEAT
    ret >r endscope ;

\ defer stuff

: preserve ( "name" -- ) \ gforth
    \G emit code that reverts a deferred word to the state at
    \G compilation
    ' dup defer@ lit, 4 swap (to), ; immediate

\ easier definer of noname words that are assigned to a deferred word

: :is ( "name" -- ) \ gforth-experimental
    \G define a noname that is assigned to the deffered word @var{name}
    \G at @code{;}.
    :noname colon-sys-xt-offset n>r drop
    record-name (') ['] defer! nr> drop ;
: :method ( class "name" -- ) \ gforth-experimental
    \G define a noname that is assigned to the deffered word @var{name}
    \G in @var{class} at @code{;}.
    :noname colon-sys-xt-offset n>r drop
    swap record-name (') ['] defer! nr> drop ;

\ shell commands

UValue $? ( -- n ) \ gforth dollar-question
\G @code{Value} -- the exit status returned by the most recently executed
\G @code{system} command.

: system ( c-addr u -- ) \ gforth
\G Pass the string specified by @var{c-addr u} to the host operating
\G system for execution in a sub-shell.  Afterwards, @code{$?}
\G produces the exit status of the command. The value of the
\G environment variable @code{GFORTHSYSTEMPREFIX} (or its default
\G value) is prepended to the string (mainly to support using
\G @code{command.com} as shell in Windows instead of whatever shell
\G Cygwin uses by default; @pxref{Environment variables}).
    (system) throw TO $? ;

: sh ( "..." -- ) \ gforth
\G Execute the rest of the command line as shell command(s).
\G Afterwards, @code{$?}  produces the exit status of the command.
    0 parse cr system ;

\ stuff

: ]L ( compilation: n -- ; run-time: -- n ) \ gforth
    \G equivalent to @code{] literal}
    ] postpone literal ;

?: in-dictionary? ( x -- f )
    forthstart dictionary-end within ;

: in-return-stack? ( addr -- f )
    rp0 @ [ forthstart section-desc + #2 cells + ]L @ - $FFF + -$1000 and rp0 @ within ;

\ !! rewrite slurp-file using slurp-fid
: slurp-file ( c-addr1 u1 -- c-addr2 u2 ) \ gforth
    \G @var{c-addr1 u1} is the filename, @var{c-addr2 u2} is the file's contents
    r/o bin open-file throw dup
    >r file-size throw abort" file too large"
    dup allocate throw swap
    2dup r@ read-file throw 2dup <> IF
	nip tuck resize throw swap
    ELSE  nip  THEN
    r> close-file throw ;

: slurp-fid ( fid -- addr u ) \ gforth
\G @var{addr u} is the content of the file @var{fid}
    { fid }
    0 0 begin ( awhole uwhole )
	dup 1024 + dup >r extend-mem ( anew awhole uwhole R: unew )
	rot r@ fid read-file throw ( awhole uwhole uread R: unew )
	r> 2dup =
    while ( awhole uwhole uread unew )
	2drop
    repeat
    - + dup >r resize throw r> ;

\ file>path

: file>path ( c-addr1 u1 path-addr -- c-addr2 u2 ) \ gforth
    \G Searches for a file with the name @i{c-addr1 u1} in path stored
    \G in @i{path-addr}.  If successful, @i{c-addr u2} is the absolute
    \G file name or the file name relative to the current working
    \G directory.  Throws an exception if the file cannot be opened.
    open-path-file throw rot close-file throw ;

: file>fpath ( addr1 u1 -- addr2 u2 ) \ gforth
    \G Searches for a file with the name @i{c-addr1 u1} in the
    \G @code{fpath}.  If successful, @i{c-addr u2} is the absolute
    \G file name or the file name relative to the current working
    \G directory.  Throws an exception if the file cannot be opened.
    fpath file>path ;

\ recognizer sequences

: defers@ ( xt -- xt' )
    BEGIN  dup ['] defer@ catch-nobt 0= WHILE  nip  REPEAT  drop ;

: get-recognizer-sequence ( recs-xt -- x1 .. xtn n )
    defers@ get-stack ;
: set-recognizer-sequence ( x1 .. xtn n recs-xt -- )
    defers@ set-stack ;

Create forth-recognizer ( -- xt ) \ gforth-obsolete
\G backward compatible to Matthias Trute recognizer API.
\G This construct turns a deferred word into a value-like word.
' forth-recognize ,
DOES> @ defer@ ;
opt: @ 3 swap (to), ;
' s-to set-to
: set-forth-recognize ( xt -- ) \ gforth-obsolete
    \G Change the system recognizer
    is forth-recognize ;

: get-recognizers ( -- xt1 .. xtn n ) \ gforth-obsolete
    \G push the content on the recognizer stack
    ['] forth-recognize get-recognizer-sequence ;
: set-recognizers ( xt1 .. xtn n -- ) \ gforth-obsolete
    \G set the recognizer stack from content on the stack
    ['] forth-recognize set-recognizer-sequence ;

: -stack { x stack -- } \ gforth-experimental
    \G Delete every occurence of @i{x} from anywhere in @i{stack}.
    stack get-stack  0 stack set-stack 0 ?DO
        dup x <> IF  stack >back  ELSE	drop  THEN
    LOOP ;

: +after { x1 x2 stack -- } \ gforth-experimental
    \G Insert @var{x1} below every occurence @var{x2} in @i{stack}.
    stack get-stack  0 stack set-stack 0 ?DO
	dup stack >back x2 = IF  x1 stack >back  THEN
    LOOP ;

: try-recognize ( addr u xt -- results | false ) \ gforth-experimental
    \G For nested recognizers: try to recognize @var{addr u}, and execute
    \G @var{xt} to check if the result is desired.  If @var{xt} returns false,
    \G clean up all side effects of the recognizer, and return false.
    \G Otherwise return the results of the call to @var{xt}, of which the
    \G topmost is non-zero.
    { xt: xt }  sp@ fp@ 2>r
    forth-recognize xt dup
    if    2rdrop
    else
	2r> fp! sp! 2drop false
    then ;

\ ]] ... [[

: rec-[[ ( addr u -- token | 0 ) \ gforth-internal rec-left-bracket-bracket
    \ recognizer for "[["; when it is recognized, postpone state ends.
    s" [[" str=  [: ] action-of forth-recognize stack> drop ;] and ;

: ]] ( -- ) \ gforth right-bracket-bracket
    \G Switch into postpone state: All words and recognizers are
    \G processed as if they were preceded by @code{postpone}.
    \G Postpone state ends when @code{[[} is recognized.
    ['] rec-[[ action-of forth-recognize >stack
    ['] postponing set-state ; immediate restrict

\ mem+do...mem+loop mem-do...mem-loop array>mem

\ !! todo: check for matching MEM*LOOP; also support non-constants

s" mem+do and mem-do currently require a constant stride" exception
constant mem*do-noconstant

: array>mem ( uelements uelemsize -- ubytes uelemsize ) \ gforth-experimental
    \G @i{ubytes}=@i{uelements}*@i{uelemsize}
    tuck * swap ;

' fold2-2 foldn-from: opt-array>mem
[: drop lits> dup ]] Literal * Literal [[ ;] 1 set-fold#
' opt-array>mem optimizes array>mem

: const-mem+loop ( +nstride xt do-sys -- )
    cs-item-size 1+ pick ]] literal +loop [[ 2drop ;

: general-mem+loop ( local-offset xt do-sys )
    cs-item-size 1+ pick lp-offset compile-@local ]] +loop [[ 2drop
    ]] endscope [[ ;

: -[do ( compilation -- do-sys ; run-time n1 n2 -- | loop-sys ) \ gforth-experimental minus-bracket-do
    \G Start of a counted loop with negative stride; Skips the loop if
    \G @i{n2}<@i{n1}; such a counted loop ends with @code{+loop} where
    \G the increment is negative; it runs as long as @code{I}>=@i{n1}.
    POSTPONE (-[do) ?do-like ; immediate restrict    

: u-[do ( compilation -- do-sys ; run-time u1 u2 -- | loop-sys ) \ gforth-experimental u-minus-bracket-do
    \G Start of a counted loop with negative stride; Skips the loop if
    \G @i{u2}<@i{u1}; such a counted loop ends with @code{+loop} where
    \G the increment is negative; it runs as long as @code{I}>=@i{u1}.
    POSTPONE (u-[do) ?do-like ; immediate restrict    

: mem-do ( compilation -- w xt do-sys; run-time addr ubytes +nstride -- ) \ gforth-experimental mem-minus-do
    \g Starts a counted loop that starts with @code{I} as
    \g @i{addr}+@i{ubytes}-@i{ustride} and then steps backwards
    \g through memory with -@i{nstride} wide steps as long as
    \g @code{I}>=@i{addr}.  Must be finished with @word{loop}.
    lits# if
        lits> negate ['] const-mem+loop over ]] literal + over + u-[do [[
    else
        ]] scope dup negate [[
        noname-w: ['] general-mem+loop ]] - over + u-[do [[
    then
    1 or ; immediate compile-only

: mem+do ( compilation -- w xt do-sys; run-time addr ubytes +nstride -- ) \ gforth-experimental mem-plus-do
    \g Starts a counted loop that starts with @code{I} as @i{addr} and
    \g then steps upwards through memory with @i{nstride} wide steps
    \g as long as @code{I}<@i{addr}+@i{ubytes}.  Must be finished with
    \g @word{loop}.
    lits# if
        lits> ['] const-mem+loop ]] bounds u+do [[
    else
        ]] scope [[ noname-w: ['] general-mem+loop ]] bounds u+do [[
    then
    1 or ; immediate compile-only

\ f.rdp

: push-right ( c-addr u1 u2 cfill -- )
    \ move string at c-addr u1 right by u2 chars (without exceeding
    \ the original bound); fill the gap with cfill
    >r over min dup >r rot dup >r ( u1 u2 c-addr R: cfill u2 c-addr )
    dup 2swap /string cmove>
    r> r> r> fill ;

: f>buf-rdp-try { f: rf c-addr ur nd up um1 -- um2 }
    \ um1 is the mantissa length to try, um2 is the actual mantissa length
    c-addr ur um1 /string '0 fill
    rf c-addr um1 represent if { nexp fsign }
	nd nexp + up >= up 0= or
	ur nd - 1- dup { beforep } fsign + nexp 0 max >= and if
	    \ fixed-point notation
	    c-addr ur beforep nexp - dup { befored } '0 push-right
            befored 1+ ur >= if \ <=1 digit left, will be pushed out by '.'
                rf fabs f2* 0.1e nd s>d d>f f** f> if \ round last digit
                    '1 c-addr befored + 1- c!
                endif
            endif
	    c-addr beforep 1- befored min dup { beforez } 0 max bl fill
	    fsign if
		'- c-addr beforez 1- 0 max + c!
	    endif
	    c-addr ur beforep /string 1 '. push-right
	    nexp nd +
	else \ exponential notation
	    c-addr ur 1 /string 1 '. push-right
	    fsign if
		c-addr ur 1 '- push-right
	    endif
	    nexp 1- s>d tuck dabs <<# #s rot sign 'E hold #> { explen }
	    ur explen - 1- fsign + { mantlen }
	    mantlen 0< if \ exponent too large
		drop c-addr ur '* fill
	    else
		c-addr ur + 0 explen negate /string move
	    endif
	    #>> mantlen
	endif
    else \ inf or nan
        \ rely on REPRESENT result
	2drop
	\ if you don't want to rely, use this:
	\ rf f0< if s" -Inf" else rf f0>= if s" Inf" else s" NaN" endif endif
        \ c-addr ur rot umin dup >r move c-addr ur r> /string blank
        ur
    endif
    1 max ur min ;

: f>buf-rdp ( rf c-addr +nr +nd +np -- ) \ gforth
\G Convert @i{rf} into a string at @i{c-addr nr}.  The conversion
\G rules and the meanings of @i{nr nd np} are the same as for
\G @code{f.rdp}.
    \ first, get the mantissa length, then convert for real.  The
    \ mantissa length is wrong in a few cases because of different
    \ rounding; In most cases this does not matter, because the
    \ mantissa is shorter than expected and the final digits are 0;
    \ but in a few cases the mantissa gets longer.  Then it is
    \ conceivable that you will see a result that is rounded too much.
    \ However, I have not been able to construct an example where this
    \ leads to an unexpected result.
    swap 0 max swap 0 max
    fdup 2over 2over third f>buf-rdp-try f>buf-rdp-try drop ;

: f>str-rdp ( rf +nr +nd +np -- c-addr nr ) \ gforth
\G Convert @i{rf} into a string at @i{c-addr nr}.  The conversion
\G rules and the meanings of @i{nr +nd np} are the same as for
\G @code{f.rdp}.  The result in in the pictured numeric output buffer
\G and will be destroyed by anything destroying that buffer.
    rot holdptr @ 1- 0 rot negate /string ( rf +nd np c-addr nr )
    over holdbuf u< -&17 and throw
    2tuck 2>r f>buf-rdp 2r> ;

: f.rdp ( rf +nr +nd +np -- ) \ gforth
\G Print float @i{rf} formatted.  The total width of the output is
\G @i{nr}.  For fixed-point notation, the number of digits after the
\G decimal point is @i{+nd} and the minimum number of significant
\G digits is @i{np}.  @code{Set-precision} has no effect on
\G @code{f.rdp}.  Fixed-point notation is used if the number of
\G siginicant digits would be at least @i{np} and if the number of
\G digits before the decimal point would fit.  If fixed-point notation
\G is not used, exponential notation is used, and if that does not
\G fit, asterisks are printed.  We recommend using @i{nr}>=7 to avoid
\G the risk of numbers not fitting at all.  We recommend
\G @i{nr}>=@i{np}+5 to avoid cases where @code{f.rdp} switches to
\G exponential notation because fixed-point notation would have too
\G few significant digits, yet exponential notation offers fewer
\G significant digits.  We recommend @i{nr}>=@i{nd}+2, if you want to
\G have fixed-point notation for some numbers; the smaller the value
\G of @i{np}, the more cases are shown in fixed-point notation (cases
\G where few or no significant digits remain in fixed-point notation).
\G We recommend @i{np}>@i{nr}, if you want to have exponential
\G notation for all numbers.
    f>str-rdp type ;

0 [if]
: testx ( rf ur nd up -- )
    '| emit f.rdp ;

: test ( -- )
    -0.123456789123456789e-20
    40 0 ?do
	cr
	fdup 7 3 1 testx
	fdup 7 3 4 testx
	fdup 7 3 0 testx
	fdup 7 7 1 testx
	fdup 7 5 1 testx
	fdup 7 0 2 testx
	fdup 5 2 1 testx
	fdup 4 2 1 testx
	fdup 18 8 5 testx
	'| emit
	10e f*
    loop ;
[then]

14 Value f.s-precision ( -- u ) \ gforth
\G A @code{value}.  @i{U} is the field width for f.s output. Other
\G precision details are derived from that value.

: f.s ( -- ) \ gforth f-dot-s
\G Display the number of items on the floating-point stack, followed
\G by a list of the items (but not more than specified by
\G @code{maxdepth-.s}; TOS is the right-most item.
    ." <" fdepth 0 .r ." > " fdepth 0 max maxdepth-.s @ min dup 0 ?DO
	dup i - 1- floats fp@ + f@
	f.s-precision 7 max dup 0 f.rdp space LOOP
    drop ; 

: typewhite ( addr n -- ) \ gforth
\G Like type, but white space is printed instead of the characters.
    \ bounds u+do
    0 max bounds ?do
	i c@ #tab = if \ check for tab
	    #tab
	else
	    bl
	then
	emit
    loop ;

\ w and l stuff

environment-wordlist >order

16 address-unit-bits / 1 max constant /w ( -- u ) \ gforth slash-w
\G address units for a 16-bit value
    
32 address-unit-bits / 1 max constant /l ( -- u ) \ gforth slash-l
\G address units for a 32-bit value

64 address-unit-bits / 1 max constant /x ( -- u ) \ gforth slash-x
\G address units for a 64-bit value

previous

[IFUNDEF] xd><
    1 cells 4 = [if]
        ?: xd>< ( xd1 -- xd2 ) \ gforth
            \g Convert 64-bit value in \code{xd1} from native byte
            \g order to big-endian or from big-endian to native byte
            \g order (the same operation)
            l>< swap l>< ;
    [else] 1 cells 8 = [if]
            ?: xd>< ( xd1 -- xd2 ) \ gforth
                \g Convert 64-bit value in \code{xd1} from native byte
                \g order to big-endian or from big-endian to native byte
                \g order (the same operation)
                swap x>< swap ;
        [else] error-no-xd><-for-this-cell-size
        [then]
    [then]
[then]

1 pad ! pad c@ 1 = [IF] \ little endian
    ' [noop] ' [noop] ' [noop] ' [noop] ' xd><   ' x><    ' l><    ' w><
[else] \ big-endian
    ' xd><   ' x><    ' l><    ' w><    ' [noop] ' [noop] ' [noop] ' [noop]
[THEN]

( 8 xts )
alias wbe ( u1 -- u2 ) \ gforth
\g Convert 16-bit value in @i{u1} from native byte order to
\g big-endian or from big-endian to native byte order (the same
\g operation)
alias lbe ( u1 -- u2 ) \ gforth
\g Convert 32-bit value in @i{u1} from native byte order to
\g big-endian or from big-endian to native byte order (the same
\g operation)
alias xbe ( u1 -- u2 ) \ gforth
\g Convert 64-bit value in @i{u1} from native byte order to
\g big-endian or from big-endian to native byte order (the same
\g operation)
alias xdbe ( ud1 -- ud2 ) \ gforth
\g Convert 64-bit value in @i{ud1} from native byte order to
\g big-endian or from big-endian to native byte order (the same
\g operation)
alias wle ( u1 -- u2 ) \ gforth
\g Convert 16-bit value in @i{u1} from native byte order to
\g little-endian or from little-endian to native byte order (the same
\g operation)
alias lle ( u1 -- u2 ) \ gforth
\g Convert 32-bit value in @i{u1} from native byte order to
\g little-endian or from little-endian to native byte order (the same
\g operation)
alias xle ( u1 -- u2 ) \ gforth
\g Convert 64-bit value in @i{u1} from native byte order to
\g little-endian or from little-endian to native byte order (the same
\g operation)
alias xdle ( ud1 -- ud2 ) \ gforth
\g Convert 64-bit value in @i{ud1} from native byte order to
\g little-endian or from little-endian to native byte order (the same
\g operation)

' [noop] alias x>s ( x -- n ) \ gforth
\g Sign-extend the 64-bit value in @i{x} to cell @i{n}.
1 cells 4 = [if]
    ' [noop]
[else] 1 cells 8 = [if]
        ' s>d
    [else]
        error-no-xd>s-for-this-cell-size
    [then]
[then]
alias xd>s ( xd -- d ) \ gforth
\g Sign-extend the 64-bit value in @var{xd} to double-cell @var{d}.

: w, ( x -- ) \ gforth w-comma
    \G Reserve 2 bytes of data space and store the least significant
    \G 16 bits of @i{x} there.
    here w!  2 allot ;

: l, ( l -- ) \ gforth l-comma
    \G Reserve 4 bytes of data space and store the least significant
    \G 32 bits of @i{x} there.
    here l!  4 allot ;
[IFDEF] x!
    : x, ( x -- ) \ gforth x-comma
    \G Reserve 8 bytes of data space and store (the least significant
    \G 64 bits) of @i{x} there.
        \G Reserve 8 bytes of data space and store @i{w} there.
        here x!  8 allot ;
[THEN]
: xd, ( xd -- ) \ gforth x-d-comma
    \G Reserve 8 bytes of data space and store the least significant
    \G 64 bits of @i{x} there.
    here 8 allot xd! ;

' naligned alias *aligned ( addr1 n -- addr2 ) \ gforth star-aligned
\g @var{addr2} is the aligned version of @var{addr1} with respect to the
\g alignment @var{n}; @var{n} must be a power of 2.
: *align ( n -- ) \ gforth star-align
    \G Align @code{here} with respect to the alignment @var{n}.
    here swap naligned ->here ;
: walign ( -- ) \ gforth
    \G Align @code{here} to even.
    2 *align ;
: waligned ( addr -- addr' ) \ gforth
    \G @i{Addr'} is the next even address >= @i{addr}.
    2 *aligned ;
: lalign ( -- ) \ gforth
    \G Align @code{here} to be divisible by 4.
    4 *align ;
: laligned ( addr -- addr' ) \ gforth
    \G @i{Addr'} is the next address >= @i{addr} divisible by 4.
    4 *aligned ;
: xalign ( -- ) \ gforth
    \G Align @code{here} to be divisible by 8.
    8 *align ;
: xaligned ( addr -- addr' ) \ gforth
    \G @i{Addr'} is the next address >= @i{addr} divisible by 8.
    8 *aligned ;

\ safe output redirection

: outfile-execute ( ... xt file-id -- ... ) \ gforth
    \G execute @i{xt} with the output of @code{type} etc. redirected to
    \G @i{file-id}.
    op-vector @ outfile-id { oldout oldfid } try
	default-out op-vector !
	to outfile-id execute 0
    restore
	oldfid to outfile-id
	oldout op-vector !
    endtry
    throw ;

: infile-execute ( ... xt file-id -- ... ) \ gforth
    \G execute @i{xt} with the input of @code{key} etc. redirected to
    \G @i{file-id}.
    ip-vector @ infile-id { oldin oldfid } try
	default-in ip-vector !
	to infile-id execute 0
    restore
	oldfid to infile-id
	oldin ip-vector !
    endtry
    throw ;

User theme-color  0 theme-color !
: execute-theme-color ( xt -- ) \ gforth-internal
    \G execute a theme-color changing xt and return to the previous theme
    \G color
    theme-color @ >r catch r> theme-color! throw ;

\ inherit input/output

: derived-input: ( "name" -- )
    ['] noop dup input:
    op-vector @ latestnt >body cell+ 2 cells move ;
: derived-output: ( "name" -- )
    ['] noop dup 2dup output:
    op-vector @ latestnt >body cell+ #10 cells move ;

\ safe BASE wrapper

: base-execute ( i*x xt u -- j*x ) \ gforth
    \G execute @i{xt} with the content of @code{BASE} being @i{u}, and
    \G restoring the original @code{BASE} afterwards.
    base @ { oldbase } \ use local, because TRY blocks the return stack
    try
	base ! execute 0
    restore
	oldbase base !
    endtry
    throw ;

: dec. ( n -- ) \ gforth
    \G Display @i{n} as a signed decimal number, followed by a space.
    ['] . #10 base-execute ;

: h. ( u -- ) \ gforth
    \G Display @i{u} as an unsigned hex number, prefixed with a "$" and
    \G followed by a space.
    '$' emit ['] u. $10 base-execute ;

synonym hex. h. ( u -- ) \ gforth
    \G Display @i{u} as an unsigned hex number, prefixed with a
    \G @code{$} and followed by a space.  Another name for this word
    \G is @code{h.}, which is present in several other systems, but
    \G not in Gforth before 1.0.

: hex.r ( u1 u2 -- )
    >r 0 <<# ['] #s $10 base-execute '$' hold #> r> type-r #>> ;

: dump ( addr u -- ) \ tools
    ['] dump $10 base-execute ;
\ wrap dump into base-execute

\ th

: th ( addr1 u -- addr2 )
    cells + ;
opt: drop ]] cells + [[ ;

\ \\\ - skip to end of file

: \\\ ( -- ) \ gforth
    \G skip remaining source file
    source-id dup 1 -1 within IF
	dup >r file-size throw r> reposition-file throw
	BEGIN  refill 0= UNTIL  postpone \  THEN ; immediate

\ 2value

' >body 2!-table to-class: 2value-to ( addr -- ) \ gforth-internal

create dummy-2value
' 2@ set-does>
' 2value-to set-to

: 2Value ( d "name" -- ) \ double-ext two-value
    ['] dummy-2value create-from reveal 2, ;

s" help.txt" open-fpath-file throw 2drop slurp-fid save-mem-dict
2>r : basic-help ( -- ) \ gforth-internal
    \G Print some help for the first steps
    [ 2r> ] 2literal type ;

\ rectype-word and rectype-name

:noname drop execute ;
:noname 0> IF execute ELSE compile, THEN ;
' 2lit, >postponer
translate: translate-word
' translate-word Constant rectype-word ( takes xt +/-1, i.e. result of find and search-wordlist )

\ concat recognizers to another recognizer

\ growing buffers that need not be full

struct
    cell% 0 * field buffer-descriptor \ addr u
    cell% field buffer-length
    cell% field buffer-address
    cell% field buffer-maxlength \ >=length
end-struct buffer% ( u1 u2 -- ) \ gforth-experimental buffer-percent
\g @i{u1} is the alignment and @i{u2} is the size of a buffer descriptor.

: init-buffer ( addr -- ) \ gforth-experimental
    \ Initialize a buffer% at addr to empty.
    buffer% %size erase ;

: adjust-buffer ( u addr -- ) \ gforth-experimental
    \G Adjust buffer% at addr to length u.
    \G This may grow the allocated area, but never shrinks it.
    dup >r buffer-maxlength @ over < if ( u )
	r@ buffer-address @ over resize throw r@ buffer-address !
	dup r@ buffer-maxlength ! then
    r> buffer-length ! ;

\ traverse directory

: try-read-dir { buf handle -- n flag }
    BEGIN  buf $@ handle read-dir dup >r  WHILE
	    over buf $@len = over and  WHILE
		buf $@len 3 cells + 2* buf $!len
		rdrop 2drop  REPEAT  buf $free  r> throw  EXIT
    THEN  rdrop ;

: traverse-dir ( addr u xt -- )
    0 { xt w^ buf } open-dir throw { handle }
    [ $100 6 cells - ]L buf $!len
    BEGIN  buf handle try-read-dir  WHILE
	    buf $@ drop swap xt execute  REPEAT
    buf $free  handle close-dir throw ;

: traverse-matched-dir ( addrdir u1 addrmatch u2 xt -- )
    0 { d: match xt w^ buf } open-dir throw { handle }
    [ $100 6 cells - ]L buf $!len
    BEGIN  buf handle try-read-dir  WHILE
	    buf $@ drop swap 2dup match filename-match
	    IF  xt execute  THEN  REPEAT
    buf $free  handle close-dir throw ;

: s+ { c-addr1 u1 c-addr2 u2 -- c-addr u } \ gforth s-plus
    \G @i{c-addr u} is a newly @code{allocate}d string that contains
    \G the concatenation of @i{c-addr1 u1} (first) and @i{c-addr2 u2}
    \G (second).
    u1 u2 + allocate throw { c-addr }
    c-addr1 c-addr u1 move
    c-addr2 c-addr u1 + u2 move
    c-addr u1 u2 + ;

: append { c-addr1 u1 c-addr2 u2 -- c-addr u } \ gforth
    \G @i{C-addr u} is the concatenation of @i{c-addr1 u1} (first) and
    \G @i{c-addr2 u2} (second).  @i{c-addr1 u1} is an @code{allocate}d
    \G string, and @code{append} @code{resize}s it (possibly moving it
    \G to a new address) to accomodate @i{u} characters.
    c-addr1 u1 u2 + dup { u } resize throw { c-addr }
    c-addr2 c-addr u1 + u2 move
    c-addr u ;

\ char/[char]

: char   ( '<spaces>ccc' -- c ) \ core,xchar-ext
    \G Skip leading spaces. Parse the string @i{ccc} and return @i{c}, the
    \G display code representing the first character of @i{ccc}.
    ?parse-name
    2dup x-size u< #-16 and throw
    xc@ ;

: [char] ( compilation '<spaces>ccc' -- ; run-time -- c ) \ core,xchar-ext bracket-char
    \G Compilation: skip leading spaces. Parse the string
    \G @i{ccc}. Run-time: return @i{c}, the display code
    \G representing the first character of @i{ccc}.  Interpretation
    \G semantics for this word are undefined.
    char postpone Literal ; immediate restrict

\ xchar version of parse

: string-parse ( c-addr1 u1 "ccc<string>" -- c-addr2 u2 ) \ gforth
\G Parse @i{ccc}, delimited by the string @i{c-addr1 u1}, in the parse
\G area. @i{c-addr2 u2} specifies the parsed string within the
\G parse area. If the parse area was empty, @i{u2} is 0.
    2>r source  >in @ over min /string ( c-addr1 u1 )
    over swap 2r@ search if
	drop over - r> rdrop
    else
	nip 0 2rdrop
    then
    over + >in +!
    2dup input-lexeme! ;

: (xparse)    ( xchar "ccc<char>" -- c-addr u ) \ gforth-internal
\G Parse @i{ccc}, delimited by @i{xchar}, in the parse
\G area. @i{c-addr u} specifies the parsed string within the
\G parse area. If the parse area was empty, @i{u} is 0.
    dup $80 < if (parse) exit then \ for -1, also possibly faster
    {: | xc[ 2 cells ] :} xc[ tuck xc!+ over - string-parse ;
\ 2 cells are absolutely sufficient

' (xparse) is parse

\ time&date

[IFDEF] >time&date&tz
    : time&date ( -- nsec nmin nhour nday nmonth nyear ) \ facility-ext time-and-date
	\G Report the current time of day. Seconds, minutes and hours are
	\G numbered from 0. Months are numbered from 1.
	utime #1000000 ud/mod rot drop >time&date&tz 2drop 2drop ;
[THEN]

\ swig-warpgate helper functions
: open-warp ( caddr n -- )
    open-lib2 >r \ store handle
    r@ s" __SWIG_forth" r> lib-sym2 call-c
    evaluate \ execute entry code
    drop \ drop handle used by loader
;

\ rpick

: rpick ( R:wu ... R:w0 u -- R:wu ... R:w0 wu ) \ gforth
    \G @i{wu} is the @i{u}th element on the return stack; @code{0
    \G rpick} is equivalent to @code{r@@}.
    1+ cells rp@ + @ ;
fold1:
    case
	0 of  postpone r@  endof
	1 of  postpone r'@  endof
	postpone rpick# dup ,
    endcase ;

\ obsolete place

: place ( c-addr1 u c-addr2 -- ) \ gforth-experimental place
    \G Create a counted string of length @var{u} at @var{c-addr2} and
    \G copy the string @var{c-addr1 u} into that location.  Up to 256
    \G bytes starting at @var{c-addr2} will be written, so make sure
    \G that the buffer at @i{c-addr2} has that much space (or check
    \G that @i{u}+1 does not exceed the buffer size before calling
    \G @code{place})
    swap $ff min swap
    over >r  rot over 1+  r> move c! ;

\ outer recurse

: outer-section ( -- addr )
    section# 1- #extra-sections @ max sections $[] @ ;
' noop Alias outer-recurse ( ... -- ... ) \ core
\g Alias to the current definition.
[: drop ['] lastnt outer-section section-execute @ ;] set->int
' s-to set-to

\ equivalents for defer!

0 to-access: value! ( x xt-value -- ) \ gforth-experimental  to-store
    \G Changes the value of @var{xt-value} to @var{x}
