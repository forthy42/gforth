\ High level floating point                            14jan94py

\ Authors: Bernd Paysan, Anton Ertl, Neal Crook, Jens Wilke, Lennart Benschop
\ Copyright (C) 1995,1997,2003,2004,2005,2006,2007,2009,2010,2011,2012,2013,2014,2015,2016,2017,2018,2019,2020,2021,2022,2023,2024,2025 Free Software Foundation, Inc.

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

\ 1 cells 4 = [IF]
\ ' cells   Alias sfloats
\ ' cell+   Alias sfloat+
\ ' align   Alias sfalign
\ ' aligned Alias sfaligned
\ [ELSE]
\ : sfloats  2* 2* ;
\ : sfloat+  4 + ;
\ : sfaligned ( addr -- addr' )  3 + -4 and ;
\ : sfalign ( -- )  here dup sfaligned swap ?DO  bl c,  LOOP ;
\ [THEN]

\ 1 floats 8 = [IF]
\ ' floats   Alias dfloats
\ ' float+   Alias dfloat+
\ ' falign   Alias dfalign
\ ' faligned Alias dfaligned
\ [ELSE]
\ : dfloats  2* 2* 2* ;
\ : dfloat+  8 + ;
\ : dfaligned ( addr -- addr' )  7 + -8 and ;
\ : dfalign ( -- )  here dup dfaligned swap ?DO  bl c,  LOOP ;
\ [THEN]

: sfalign ( -- ) \ floating-ext s-f-align
    \G If the data-space pointer is not single-float-aligned, reserve
    \G enough space to align it.
    here dup sfaligned swap ?DO  bl c,  LOOP ;
: dfalign ( -- ) \ floating-ext d-f-align
    \G If the data-space pointer is not double-float-aligned, reserve
    \G enough space to align it.
    here dup dfaligned swap ?DO  bl c,  LOOP ;

(Field) sfloat+ ( sf-addr1 -- sf-addr2 ) \ floating-ext s-float-plus
\G @code{1 sfloats +}.
    1 sfloats ,

(Field) dfloat+ ( df-addr1 -- df-addr2 ) \ floating-ext d-float-plus
\G @code{1 dfloats +}.
    1 dfloats ,
    
: f, ( f -- ) \ gforth f-comma
    \G Reserve data space for one floating-point number and store
    \G @i{f} in the space.
    1 floats small-allot f! ;

: flit, ( r -- ) \ gforth
    \G This is a non-immediate variant of @word{fliteral}@*
    \G Execution semantics: ( @i{r --} ) Compile the following semantics:@*
    \G Compiled semantics: ( @i{ -- r} ).
    [ cell 4 = ] [IF]
	here cell+ dup faligned <>
	IF  postpone flit1 0 ,  ELSE  postpone flit0  THEN
    [ELSE]
        postpone flit
    [THEN]
    f, ;

: FLiteral ( compilation r -- ; run-time -- r ) \ floating f-literal
    \G Compilation semantics: ( @i{r --} ) compile the run-time semantics.@*
    \G Run-time Semantics: ( @i{ -- r} ).@*
    \G Interpretation semantics: not defined in the standard.
    flit, ;  immediate

: opt-fcon ( xt -- )  >body f@ postpone FLiteral ;

: fconstant  ( r "name" -- ) \ floating f-constant
    \G Define @i{name}.@*
    \G @i{name} execution: @i{( -- r )}
    Create f,
    ['] f@ set-does>
    ['] opt-fcon set-optimizer ;

: f+! ( r addr -- ) dup f@ f+ f! ;

to-table: f!-table f! f+!

' >body f!-table to-class: fvalue-to ( r xt-fvalue -- ) \ gforth-internal

create dummy-fvalue
' f@ set-does>
' fvalue-to set-to

: fvalue ( r "name" -- ) \ floating-ext f-value
    \g Define @i{name} with the initial value @i{r} @*
    \g @i{name} execution: @i{( -- r2 )} push the current value of @i{name}.@*
    \g @code{to @i{name}} run-time: @i{( r3 -- )} change the value of
    \g @i{name} to @i{r3}.@*
    \g @code{+to @i{name}} run-time: @i{( r4 -- )} increment the value of
    \g @i{name} by @i{r4}
    ['] dummy-fvalue create-from reveal f, ;

: fdepth ( -- +n ) \ floating f-depth
    \G @i{+n} is the current number of (floating-point) values on the
    \G floating-point stack.
    fp0 @ fp@ - [ 1 floats ] Literal / ;

: fclearstack ( r0 .. rn -- ) \ gforth f-clearstack
    \G clear the floating point stack
    fp0 @ fp! ;

&15 Value precision ( -- u ) \ floating-ext
\G @i{u} is the number of significant digits currently used by
\G @code{F.} @code{FE.} and @code{FS.} 
: set-precision ( u -- ) \ floating-ext
    \G Set the number of significant digits currently used by
    \G @code{F.} @code{FE.} and @code{FS.} to @i{u}.
    to precision ;

: scratch ( -- addr len )
    \ scratchpad for floating point - use space at the end of the user area
    next-task udp @ + precision ;

: zeros ( n -- )   0 max 0 ?DO  '0 emit  LOOP ;

: -zeros ( addr u -- addr' u' )
    BEGIN  dup  WHILE  1- 2dup + c@ '0 <>  UNTIL  1+  THEN ;

: f$ ( f -- n )
    scratch represent 0=
    IF  2drop  scratch -trailing type  rdrop  EXIT  THEN
    IF  '- emit  THEN ;
: f$space ( f -- n )
    scratch represent 0=
    IF  2drop  scratch -trailing type space  rdrop  EXIT  THEN
    IF  '- emit  THEN ;

: f.  ( r -- ) \ floating-ext f-dot
\G Display (the floating-point number) @i{r} without exponent,
\G followed by a space.
  f$space dup >r 0<=
  IF    '0 emit
  ELSE  scratch r@ min type  r@ precision - zeros  THEN
  '. emit r@ negate zeros
  scratch r> 0 max /string 0 max -zeros type space ;
\ I'm afraid this does not really implement ansi semantics wrt precision.
\ Shouldn't precision indicate the number of places shown after the point?

\ Why do you think so? ANS Forth appears ambiguous on this point. -anton.

: fe. ( r -- ) \ floating-ext f-e-dot
\G Display @i{r} using engineering notation (with exponent dividable
\G by 3), followed by a space.
  f$ 1- s>d 3 fm/mod 3 * >r 1+ >r
  scratch r@ tuck min tuck - >r type r> zeros
  '. emit scratch r> /string type
  'E emit r> . ;

: fs. ( r -- ) \ floating-ext f-s-dot
\G Display @i{r} using scientific notation (with exponent), followed
\G by a space.
  f$ 1-
  scratch over c@ emit '. emit 1 /string type
  'E emit . ;

: sfnumber ( c-addr u -- r true | false ) >float ;

Create si-prefixes ," Q  R  Y  Z  X  P  T  G  M  kh  d%m  u  n  p  f  a  z  y  r  q"
si-prefixes count 2/ + Constant zero-exp

: prefix-number ( c-addr u -- r true | false )
    >num-warnings off
    base @ #10 <> IF  2drop false  EXIT  THEN
    2dup 'e' scan nip >r 2dup 'E' scan nip r> or dup
    >r 0= IF
	si-prefixes count bounds DO
	    I c@ bl <> IF
		2dup 1 safe/string I c@ scan nip dup 0<> IF
		    1 = IF  1- '.'  ELSE  I c@  THEN
		    >float1
		    dup IF  #10 s>f zero-exp I - s>f f** f*
			$20 >num-warnings +!
		    THEN
		    UNLOOP  rdrop EXIT  THEN  drop
	    THEN
	LOOP
    THEN
    \ check for e/E/.
    2dup '.' scan nip r@ or
    IF
	'.' >float1
	dup r@ 0= and $10 and >num-warnings +!
    ELSE
	2drop false
    THEN  rdrop ;

: fp. ( r -- ) \ floating-ext f-p-dot
\G Display @i{r} using SI prefix notation (with exponent dividable
\G by 3, converted into SI prefixes if available), followed by a space.
    f$ 1- s>d 3 fm/mod 3 * >r 1+ >r
    scratch r@ tuck min tuck - >r type r> zeros
    '. emit scratch r> /string type
    r@ abs [ zero-exp si-prefixes 1+ - ] Literal <= IF
	zero-exp r> - c@ emit space
    ELSE  'E emit r> .  THEN ;

: >postponer ( xt1 xt2 -- xt1 xt3 )
    >r dup >r
    :noname r> r> compile, lit, postpone compile, postpone ; ;

' ?warn# :noname ?warn# postpone fliteral ; dup >postponer
translate: translate-float ( r -- translation ) \ gforth-experimental
\G Interpreting run-time: @code{( @i{ -- r} )}
' translate-float Constant rectype-float

: rec-float ( c-addr u -- translation ) \ gforth-experimental
    \G Recognizes (@pxref{Defining recognizers}) a floating-point
    \G number, @i{translation} represents pushing that number at
    \G run-time (see @word{translate-float}).  For recognizing a
    \G string as a float, Gforth requires decimal @word{base}; it also
    \G requires the string to contain an exponent (@code{e} followed
    \G by an optional sign and 0 or more exponent digits) or a decimal
    \G point (the syntax with the decimal point only is shadowed by
    \G the double-cell syntax by default, see @word{rec-number}); in
    \G Gforth there can also be an SI prefix (e.g., @code{M}) instead
    \G of the decimal point, but then no @code{e} is allowed.
    \G Examples: @code{1234e}, @code{1234.}, @code{1.234e3},
    \G @code{12340e-1}, @code{1k234}.
    >num-warnings @ >r prefix-number
    IF rdrop translate-float
    ELSE r> >num-warnings !  translate-none THEN ;

Variable user-flagmask 1 user-flagmask !

: or! ( x addr -- )  tuck @ or swap ! ;
: and! ( x addr -- )  tuck @ and swap ! ;
: userflag! ( flag addr -- )
    @ swap IF  user-flags or!  ELSE  invert user-flags and!  THEN ;
: -userflag! ( flag addr -- ) swap invert swap userflag! ;

to-table: userflag!-table userflag!
' >body userflag!-table to-class: userflag-to
to-table: -userflag!-table -userflag!
' >body -userflag!-table to-class: -userflag-to

: user-flag: ( "name" -- ) \ gforth-experimental
    \G Create a new user flag. User flags are bits in the user variable
    \G \code{user-flags}, so you can save and restore all of them in one go.@*
    \G @i{name} execution: @i{( -- flag )}@*
    \G @code{to @i{name}} run-time: @i{( x -- )} If @i{x}=0, change
    \G the value of @i{name} to false, otherwise to true.
    Create user-flagmask @ dup , user-flagmask +!
    [: @ user-flags @ and 0<> ;] set-does>
    ['] userflag-to set-to ;
: -user-flag: ( "name" -- ) \ gforth-experimental
    \G Create a new inverted user flag. User flags are bits in the user variable
    \G \code{user-flags}, so you can save and restore all of them in one go.@*
    \G @i{name} execution: @i{( -- flag )}@*
    \G @code{to @i{name}} run-time: @i{( x -- )} If @i{x}=0, change
    \G the value of @i{name} to false, otherwise to true.
    Create user-flagmask @ dup , user-flagmask +!
    [: @ user-flags @ and 0= ;] set-does>
    ['] -userflag-to set-to ;

user-flag: .-is-dcell? ( -- flag ) \ gforth-experimental
\G If this user flag is true (default), @word{rec-number} recognizes
\G numbers without prefix that contain a decimal point as double-cell
\G numbers.  Otherwise @word{rec-number} does not recognize the
\G number, and, if present, @word{rec-float} will recognize it as a
\G floating-point number.@* @code{to .-is-dcell?}  run-time: @i{( x
\G -- )} If @i{x}=0 change the value of @word{.-is-double} to false,
\G otherwise to true.
true to .-is-dcell?

user-flagmask @ 2/ user-flagmask !
-user-flag: .-is-float? ( -- flag ) \ gforth-experimental
\G inverted interface to @code{.-is-dcell?}.

: rec-number ( c-addr u -- translation ) \ gforth-experimental
    \G Recognizes (@pxref{Defining recognizers})
    \G a single or double number (without or with prefix), or
    \G a character.  If successful, @i{translation} represents pushing
    \G that number at run-time (see @word{translate-cell} and
    \G @word{translate-dcell}).  If and only if @word{.-is-dcell?}
    \G is true, strings without prefix that contain a dot are
    \G recognized as double numbers.
    dpl @ >num-warnings @ 2>r snumber?  dup
    IF
	dup 0> >num-warnings @ 1 and 0= and .-is-float? and IF
	    2drop
	ELSE
	    0> translate-dcell translate-cell rot select
	    2rdrop EXIT
	THEN
    THEN
    2r> >num-warnings !  dpl ! drop translate-none ;

' rec-forth defer@ >r
r@ back> drop ' rec-number r@ >back ' rec-float r> >back

: fvariable ( "name" -- ) \ floating f-variable
    \g Define @i{name} and reserve a float at @i{f-addr}.@* @i{name}
    \g execution: @code{( -- f-addr )}.
    Create 0.0E0 f, ;
    \ does> ( -- f-addr )

1.0e0 fasin 2.0e0 f* fconstant pi ( -- r ) \ gforth
\G @code{Fconstant} -- @i{r} is the value pi; the ratio of a circle's area
\G to its diameter.

: f2* ( r1 -- r2 ) \ gforth
    \G Multiply @i{r1} by 2.0e0
    2.0e0 f* ;

: f2/ ( r1 -- r2 ) \ gforth
    \G Multiply @i{r1} by 0.5e0
    0.5e0 f* ;

: 1/f ( r1 -- r2 ) \ gforth
    \G Divide 1.0e0 by @i{r1}.
    1.0e0 fswap f/ ;

0.e0 1/f fconstant infinity ( -- r ) \ gforth
\G floating point infinity
synonym inf infinity ( -- r ) \ gforth
\G Synonym of @code{infinity} to allow copying and pasting from the
\G output of @code{...}, @xref{Examining data}.

infinity fnegate fconstant -infinity ( -- r ) \ gforth
\G floating point -infinity
synonym -inf -infinity ( -- r ) \ gforth
\G Synonym of @code{-infinity} to allow copying and pasting from the
\G output of @code{...}, @xref{Examining data}.

0.e0 0.e0 f/ fconstant NaN ( -- r ) \ gforth
\G floating point Not a Number

get-current environment-wordlist set-current
1.7976931348623157e308 FConstant max-float ( -- r ) \ environment
\g The largest usable floating-point number (implemented as largest
\g finite number in Gforth)
set-current

\ We now have primitives for these, so we need not define them

\ : falog ( f -- 10^f )  [ 10.0e0 fln ] FLiteral f* fexp ;

\ : fsinh    fexpm1 fdup fdup 1.0e0 f+ f/ f+ f2/ ;
\ : fcosh    fexp fdup 1/f f+ f2/ ;
\ : ftanh    f2* fexpm1 fdup 2.0e0 f+ f/ ;

\ : fatanh   fdup f0< >r fabs 1.0e0 fover f- f/  f2* flnp1 f2/
\            r> IF  fnegate  THEN ;
\ : facosh   fdup fdup f* 1.0e0 f- fsqrt f+ fln ;
\ : fasinh   fdup fdup f* 1.0e0 f+ fsqrt f/ fatanh ;

: f~abs ( r1 r2 r3 -- flag ) \ gforth
    \G Approximate equality with absolute error: |r1-r2|<r3.
    frot frot f- fabs fswap f< ;

: f~rel ( r1 r2 r3 -- flag ) \ gforth
    \G Approximate equality with relative error: |r1-r2|<r3*|r1+r2|.
	frot frot fover fabs fover fabs f+ frot frot
	f- fabs frot frot f* f< ;

: f~ ( r1 r2 r3 -- flag ) \ floating-ext f-proximate
    \G Forth-2012 medley for comparing r1 and r2 for equality: r3>0:
    \G @code{f~abs}; r3=0: bitwise comparison; r3<0: @code{fnegate f~rel}.
    fdup f0=
    IF \ bitwise comparison
	fp@ float+ 1 floats over float+ over str=
	fdrop fdrop fdrop
	EXIT
    THEN
    fdup f0>
    IF
	f~abs
    ELSE
	fnegate f~rel
    THEN ;

-0e
fp@ 0 + c@ $80 = [if] 0 constant fsign-offset [then]
fp@ 7 + c@ $80 = [if] 7 constant fsign-offset [then]
fdrop

: fcopysign ( r1 r2 -- r3 ) \ gforth
\G r3 takes its absolute value from r1 and its sign from r2
    \ !! implementation relies on IEEE DP format
    fp@ fsign-offset + dup c@ $80 and >r ( r1 r2 addr-r1sign )
    float+ dup c@ $7f and r> or swap c!
    fdrop ;

\ proposals from Krishna Myeni in <cjsp2d$47l$1@ngspool-d02.news.aol.com>
\ not sure if they are a good idea

: fmod ( r1 r2 -- r )
    \ modulus of r1/r2
    fover fover f/ floor f* f- ;

: ftrunc ( r1 -- r2 ) \ floating-ext f-trunc
    \G round towards 0
    fdup fabs floor fswap fcopysign ;

: frem ( r1 r2 -- r )
    \ modulus of r1/r2
    fover fover f/ ftrunc f* f- ;

