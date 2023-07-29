\ miscelleneous words

\ Authors: Anton Ertl, Bernd Paysan, Neal Crook
\ Copyright (C) 1996,1997,1998,2000,2003,2004,2005,2006,2007,2008,2009,2010,2011,2012,2013,2014,2015,2016,2017,2018,2019,2020,2021,2022 Free Software Foundation, Inc.

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

: save-mem-dict ( addr1 u -- addr2 u )
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

[ifundef] in-dictionary?
: in-dictionary? ( x -- f )
    forthstart dictionary-end within ;
[endif]

: in-return-stack? ( addr -- f )
    rp0 @ [ forthstart 7 cells + ]L @ - $FFF + -$1000 and rp0 @ within ;

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

[IFUNDEF] :level
    Variable :level
[THEN]

: (const-does>) ( w*uw r*ur uw ur target "name" -- )
    \ define a colon definition "name" containing w*uw r*ur as
    \ literals and a call to target.
    { uw ur target }
    ['] on create-from \ start colon def without stack junk
    1 :level +!
    ur compile-fliterals uw compile-literals
    target compile, POSTPONE exit reveal
    -1 :level +! ;

: const-does> ( run-time: w*uw r*ur uw ur "name" -- ) \ gforth-obsolete const-does
    \G Defines @var{name} and returns.
    \G  
    \G @var{name} execution: pushes @var{w*uw r*ur}, then performs the
    \G code following the @code{const-does>}.
    basic-block-end here >r 0 POSTPONE literal
    POSTPONE (const-does>)
    POSTPONE ;
    noname : POSTPONE rdrop
    latestnt r> cell+ ! \ patch the literal
; immediate

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

\ legacy rectype stuff

: rectype>int  ( rectype -- xt ) >body @ ;
: rectype>comp ( rectype -- xt ) cell >body + @ ;
: rectype>post ( rectype -- xt ) 2 cells >body + @ ;

: rectype ( int-xt comp-xt post-xt -- rectype ) \ gforth-obsolete
    \G create a new unnamed recognizer token
    noname translate: latestxt ; 

: rectype: ( int-xt comp-xt post-xt "name" -- ) \ gforth-obsolete
    \G create a new recognizer table
    rectype Constant ;

' notfound AConstant rectype-null
' translate-nt AConstant rectype-nt
' translate-num AConstant rectype-num
' translate-dnum AConstant rectype-dnum

: defers@ ( xt -- xt' )
    dup BEGIN  ['] defer@ catch 0= WHILE  nip  REPEAT  drop ;
: >rec-stack ( xt -- stack )
    dup >code-address docol: =
    IF  >body cell+ @ @  ELSE  >body  THEN ;
: get-rec-sequence ( recs-xt -- x1 .. xtn n )
    defers@ >rec-stack get-stack ;
: set-rec-sequence ( x1 .. xtn n recs-xt -- )
    defers@ >rec-stack set-stack ;

: get-recognizers ( -- xt1 .. xtn n ) \ gforth-experimental
    \G push the content on the recognizer stack
    forth-recognizer get-stack ;
: set-recognizers ( xt1 .. xtn n -- ) \ gforth-experimental
    \G set the recognizer stack from content on the stack
    forth-recognizer set-stack ;

\ ]] ... [[

' noop ' noop
:noname  ] forth-recognizer stack> drop ;
translate: translate-[[
' translate-[[ Constant rectype-[[

: rec-[[ ( addr u -- token ) \ gforth-internal rec-left-bracket-bracket
    \ recognizer for "[["; when it is recognized, postpone state ends.
    s" [[" str=  ['] translate-[[ ['] notfound rot select ;

: ]] ( -- ) \ gforth right-bracket-bracket
    \G Switch into postpone state: All words and recognizers are
    \G processed as if they were preceded by @code{postpone}.
    \G Postpone state ends when @code{[[} is recognized.
    ['] rec-[[ forth-recognizer >stack
    -2 state ! ; immediate restrict

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
        \ don't rely on REPRESENT result
        2drop
        rf f0< if s" -Inf" else rf f0>= if s" Inf" else s" NaN" endif endif
        c-addr ur rot umin dup >r move c-addr ur r> /string blank
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

\ defer stuff

3 to: action-of ( interpretation "name" -- xt; compilation "name" -- ; run-time -- xt ) \ core-ext
\G @i{Xt} is the XT that is currently assigned to @i{name}.

synonym what's action-of ( interpretation "name" -- xt; compilation "name" -- ; run-time -- xt ) \ gforth-obsolete
\G Old name of @code{action-of}


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
        : xd>< ( xd1 -- xd2 ) \ gforth
            \g Convert 64-bit value in \code{xd1} from native byte
            \g order to big-endian or from big-endian to native byte
            \g order (the same operation)
            l>< swap l>< ;
    [else] 1 cells 8 = [if]
            : xd>< ( xd1 -- xd2 ) \ gforth
                \g Convert 64-bit value in \code{xd1} from native byte
                \g order to big-endian or from big-endian to native byte
                \g order (the same operation)
                swap x>< swap ;
        [else] error-no-xd><-for-this-cell-size
        [then]
    [then]
[then]

' noop create-from noop0 ( -- ) \ gforth-internal
\g noop that compiles to nothing
' drop set-optimizer
reveal

1 pad ! pad c@ 1 = [IF] \ little endian
    ' noop0 ' noop0 ' noop0 ' noop0 ' xd><  ' x><   ' l><   ' w><
[else] \ big-endian
    ' xd><  ' x><   ' l><   ' w><   ' noop0 ' noop0 ' noop0 ' noop0
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

' noop0 alias x>s ( x -- n ) \ gforth
\g Sign-extend the 64-bit value in @i{x} to cell @i{n}.
1 cells 4 = [if]
    ' noop0
[else] 1 cells 8 = [if]
        ' s>d
    [else]
        error-no-xd>s-for-this-cell-size
    [then]
[then]
alias xd>s ( xd -- d ) \ gforth
\g Sign-extend the 64-bit value in @var{xd} to double-ceel @var{d}.

: w, ( w -- ) \ gforth w-comma
    here w!  2 allot ;
: l, ( l -- ) \ gforth l-comma
    here l!  4 allot ;
[IFDEF] x!
    : x, ( x -- ) \ gforth x-comma
        here x!  8 allot ;
[THEN]
: xd, ( xd -- ) \ gforth x-d-comma
    here 8 allot xd! ;

' naligned alias *aligned ( addr1 n -- addr2 ) \ gforth
\g @var{addr2} is the aligned version of @var{addr1} with respect to the
\g alignment @var{n}.
: *align ( n -- ) \ gforth
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
    op-vector @ latestxt >body cell+ 2 cells move ;
: derived-output: ( "name" -- )
    ['] noop dup 2dup output:
    op-vector @ latestxt >body cell+ #10 cells move ;

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

: hex.r ( u1 u2 -- )
    ['] u.r #16 base-execute ;

: dump ( addr u -- ) \ tools
    ['] dump $10 base-execute ;
\ wrap dump into base-execute

\ th

: th ( addr1 u -- addr2 )
    cells + ;

\ \\\ - skip to end of file

: \\\ ( -- ) \ gforth
    \G skip remaining source file
    source-id dup 1 -1 within IF
	dup >r file-size throw r> reposition-file throw
	BEGIN  refill 0= UNTIL  postpone \  THEN ; immediate

\ multiple values to and from return stack

: n>r ( x1 .. xn n -- r:xn..x1 r:n ) \ tools-ext n-to-r
    scope r> { n ret }
    0  BEGIN  dup n <  WHILE  swap >r 1+  REPEAT  >r
    ret >r endscope ;
: nr> ( r:xn..x1 r:n -- x1 .. xn n ) \ tools-ext n-r-from
    scope r> r> { ret n }
    0  BEGIN  dup n <  WHILE  r> swap 1+  REPEAT
    ret >r endscope ;

\ x:traverse-wordlist words

' name>interpret alias name>int ( nt -- xt|0 ) \ gforth-obsolete name-to-int
    \G @i{xt} represents the interpretation semantics @i{nt}; returns
    \G 0 if @i{nt} has no interpretation semantics

' name>compile alias name>comp ( nt -- w xt ) \ gforth-obsolete name-to-comp
\G @i{w xt} is the compilation token for the word @i{nt}.

\ 2value

: 2value-compile, ( xt -- )  >body postpone Literal postpone 2@ ;

' >body 2!-table to-method: 2value-to ( addr -- ) \ gforth-internal

create dummy-2value
' 2@ set-does>
' 2value-compile, set-optimizer
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
end-struct buffer% ( u1 u2 -- ) \ gforth-experimental
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
