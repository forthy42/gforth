\ Constant folding for some primitives

\ Authors: Bernd Paysan, Anton Ertl
\ Copyright (C) 2019,2020,2024 Free Software Foundation, Inc.

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

2 to: action-of ( interpretation "name" ... -- xt; compilation "name" -- ; run-time ... -- xt ) \ core-ext
\G @i{Name} is a defer-flavoured word, @i{...} is optional additional
\G addressing information, e.g., for a defer-flavoured field.  At run-time, perform the
\G @i{action-of @i{name}} semantics: Push the @i{xt}, that @i{name}
\G (possibly with additional addressing data on the stack) executes.

: pow2? ( u -- f ) \ gforth pow-two-query
    \g @i{f} is true if and only if @i{u} is a power of two, i.e., there is
    \g exactly one bit set in @i{u}.
    dup dup 1- and 0= and 0<> ;

: 2lits> ( -- d )  lits> lits> swap ;
: >2lits ( d -- )  swap >lits >lits ;
: 3lits> ( -- t )  2lits> lits> -rot ;
: >3lits ( -- t )  rot >lits >2lits ;
: 4lits> ( -- q )  2lits> 2lits> 2swap ;
: >4lits ( q -- )  2swap >2lits >2lits ;

: nth ( n addr -- ) swap th@ ;
Create nlits> ' noop , ' lits> , ' 2lits> , ' 3lits> , ' 4lits> ,
' nth set-does>
Create n>lits ' noop , ' >lits , ' >2lits , ' >3lits , ' >4lits ,
' nth set-does>

: cfaprim? ( cfa -- flag )
    [ ' noop >code-address ] Literal
    [ ' image-header >link @ >code-address ] Literal
    \ please do not fold this 1+ into the previous literal
    1+ within ;

: noopt-compile, ( xt -- ) \ gforth-experimental
    \G compiles @var{xt} using the (unoptimized) default method.
    case dup >code-address
	docol:      of  :,              endof
	dodoes:     of  does,           endof
	docon:      of  constant,       endof
	dovar:      of  variable,       endof
	douser:     of  user,           endof
	dodefer:    of  defer,          endof
	doabicode:  of  abi-code,       endof
	do;abicode: of  ;abi-code,      endof
	dup cfaprim? ?of  drop  peephole-compile, endof
	over        ?of peephole-compile, endof \ code word
	lit, lits, postpone execute 0
    endcase ;

0 Value lastfold
: set-fold# ( xt i -- ) 1+ cells lastfold + ! ;
: set-foldmax ( -- addr )
    lastfold @ set-fold# ;
: get-foldmax ( opt-xt -- xt )
    dup @ 1+ th@ ;

: (foldn:) ( xt n "name" -- )
    create  latestxt to lastfold
  DOES>
    >r lits# r@ @ umin 1+ cells r> + @ execute-exit ;

: foldn: ( xt n "name" -- )
    \ name is a constant-folding word that dispatches between n
    \ constant-folding words for different numbers of available
    \ constants.  The entries are initialized with xt.
    (foldn:) dup , 1+ 0 ?DO dup , LOOP drop ;

: foldn-from: ( xt "name" -- )
    \ name is a constant-folding word that dispatches between n
    \ constant-folding words for different numbers of available
    \ constants.  The entries are copied from the foldn-style word xt.
    (foldn:) dup @ 2 + cells here swap dup allot move ;

: folding ( n -- )
    latest >namehm @ >hmcompile, @ swap
    next-section noname foldn: previous-section
    lastfold set-optimizer ;

: fold-constant: ( popn pushn "name" -- )
    n>lits swap >r ['] noopt-compile, r@ foldn:
    noname Create latestxt r@ set-fold# , r@ n>lits , r> nlits> ,
  DOES> ( xt -- )
    >r >r
    i' cell+ cell+ perform r> catch-nobt 0<> cell and r> + @ execute-exit ;

: folds ( folder-xt "name1" ... "namen" <eol> -- )
    {: folder-xt :} BEGIN
	>in @ >r parse-name r> >in !
	nip  WHILE
	    folder-xt optimizes
    REPEAT ;

1 0 fold-constant: fold1-0
' fold1-0 folds drop

1 1 fold-constant: fold1-1
' fold1-1 folds invert abs negate >pow2
' fold1-1 folds 1+ 1- 2* 2/ cells cell/ cell+ cell-
' fold1-1 folds floats sfloats dfloats float+
' fold1-1 folds float/ sfloat/ dfloat/
' fold1-1 folds c>s w>s l>s w>< l>< x><
' fold1-1 folds wcwidth
' fold1-1 folds 0> 0= 0<

1 2 fold-constant: fold1-2
' fold1-2 folds dup s>d

2 0 fold-constant: fold2-0
' fold2-0 folds 2drop

2 1 fold-constant: fold2-1
' fold2-1 folds * and or xor
' fold2-1 folds min max umin umax
' fold2-1 folds nip
' fold2-1 folds rshift lshift arshift rol ror
' fold2-1 folds d0> d0< d0=
' fold2-1 folds /s mods

2 2 fold-constant: fold2-2
' fold2-2 folds m* um* swap d2* /modf /mods u/mod bounds

2 3 fold-constant: fold2-3
' fold2-3 folds over tuck

3 1 fold-constant: fold3-1
' fold3-1 folds within select mux */f */s u*/

3 2 fold-constant: fold3-2
' fold3-2 folds um/mod fm/mod sm/rem du/mod */modf */mods u*/mod under+

3 3 fold-constant: fold3-3
' fold3-3 folds rot -rot

4 1 fold-constant: fold4-1
' fold4-1 folds d= d> d>= d< d<= du> du>= du< du<=

4 2 fold-constant: fold4-2
' fold4-2 folds d+ d- 2nip

4 4 fold-constant: fold4-4
' fold4-4 folds 2swap

\ optimize +loop (not quite folding)
: replace-(+loop) ( xt1 -- xt2 )
    case
	['] (+loop)       of ['] (/loop) endof
	['] (+loop)-lp+!# of ['] (/loop)-lp+!# endof
	-21 throw
    endcase ;

: (+loop)-optimizer ( xt -- )
    lits# 1 u>= if
	lits> dup >lits 0> if
	    replace-(+loop) then
    then
    peephole-compile, ;

' (+loop)-optimizer optimizes (+loop)
' (+loop)-optimizer optimizes (+loop)-lp+!#

\ optimize pick and fpick

:noname ( xt -- )
    lits# 1 u>= if
	lits> case
	    0 of postpone dup  drop exit endof
	    1 of postpone over drop exit endof
	    [defined] fourth [if]
		2 of postpone third drop exit endof
		3 of postpone fourth drop exit endof
	    [then]
	    dup >lits
	endcase
    then
    peephole-compile, ;
optimizes pick

:noname ( xt -- )
    lits# 1 u>= if
	lits> case
	    0 of postpone fdup  drop exit endof
	    1 of postpone fover drop exit endof
	    [defined] ffourth [if]
		2 of postpone fthird drop exit endof
		3 of postpone ffourth drop exit endof
	    [then]
	    dup >lits
	endcase
    then
    peephole-compile, ;
optimizes fpick

\ optimize + -

' fold2-1 noname foldn-from:
[: 0 lits> rot execute ?dup-if ['] lit+ peephole-compile, , then ;] 1 set-fold#
latestxt folds + -
' fold2-1 noname foldn-from:
[: drop lits> ?dup-if ['] lit+ peephole-compile, cells , then ;] 1 set-fold#
latestxt folds th

' fold2-1 noname foldn-from:
[: lits> case
	0    of postpone drop 0 lit, endof
	2    of postpone 2*    endof
	[ cell 1 sfloats <> ] [IF]
	    1 sfloats of postpone sfloats  endof [THEN]
	cell of postpone cells endof
	[ cell 1 dfloats <> ] [IF]
	    1 dfloats of postpone dfloats  endof [THEN]
	dup pow2? ?of log2 lit, postpone lshift endof
	dup lit, over peephole-compile,
    endcase drop ;] 1 set-fold#
latestxt optimizes *

\ optimize lit @ into lit@
' fold1-1 noname foldn-from:
[: drop lits> ['] lit@ peephole-compile, , ;] 1 set-fold#
latestxt optimizes @

\ optimize lit execute into call
' fold1-1 noname foldn-from:
[: ( xt -- ) drop lits> compile, ;] 1 set-fold#
latestxt optimizes execute

\ optimize 0+comparison

: 0lit? ( lit:0 -- true | lit:x -- lit:x false )
    lits# dup IF  drop lits> dup IF  >lits true  THEN invert  THEN ;

:noname drop false ;
opt: drop postpone drop postpone false ;
:noname drop true ;
opt: drop postpone drop postpone true ;

Create ~>0~
' = , ' 0= ,
' <> , ' 0<> ,
' < , ' 0< ,
' > , ' 0> ,
' <= , ' 0<= ,
' >= , ' 0>= ,
' u<= , ' 0= ,
' u> , ' 0<> ,
' u>= , ,
' u< , ,
here latestxt - >r
DOES>  [ r> ] Literal bounds DO
      dup I @ = IF  drop I cell+ @  UNLOOP  EXIT  THEN
      2 cells +LOOP ;

' fold2-1 noname foldn-from:
[: ( xt -- )  0lit? IF  ~>0~ compile,   ELSE  peephole-compile,  THEN ;] 1 set-fold#
latestxt folds = <> < > <= >= u<= u>= u< u>
