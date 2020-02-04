\ Constant folding for some primitives

\ Authors: Bernd Paysan, Anton Ertl
\ Copyright (C) 2019 Free Software Foundation, Inc.

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

: 2lits> ( -- d )  lits> lits> swap ;
: >2lits ( d -- )  swap >lits >lits ;
: 3lits> ( -- t )  2lits> lits> -rot ;
: >3lits ( -- t )  rot >lits >2lits ;
: 4lits> ( -- q )  2lits> 2lits> 2swap ;
: >4lits ( q -- )  2swap >2lits >2lits ;


: folder1 ( m xt-pop xt-unpop xt-push -- xt1 )
    \ xt1 ( xt -- ) compiles xt with constant folding: xt ( m*n -- l*n ).
    \ xt-pop pops m items from literal stack to data stack, xt-push
    \ pushes l items from data stack to literal stack.
    [{: m xt: pop xt: unpop xt: push :}d {: xt -- }
	lits# m u>= if
	    pop xt catch 0= if
		push rdrop exit then
	    unpop then
	xt dup >code-address docol: = if
	    :,
	else
	    peephole-compile, then
    ;] ;

: folds ( folder-xt "name1" ... "namen" <eol> -- )
    {: folder-xt :} BEGIN
	>in @ >r parse-name r> >in !
	nip  WHILE
	    ' make-latest
	    folder-xt set-optimizer
    REPEAT ;

: optimizes ( xt "name" -- )
    \ xt is optimizer of "name"
    ' make-latest set-optimizer ;

1 ' lits> ' >lits ' noop folder1
    folds drop
1 ' lits> ' >lits ' >lits folder1
    dup folds invert abs negate >pow2
    dup folds 1+ 1- 2* 2/ cells cell/ cell+ cell-
    dup folds floats sfloats dfloats float+
    dup folds float/ sfloat/ dfloat/
    dup folds c>s w>s l>s w>< l>< x><
    dup folds wcwidth
    dup folds 0> 0= 0<
    drop
1 ' lits> ' >lits ' >2lits folder1
    folds dup s>d
2 ' 2lits> ' >lits ' noop folder1
    folds 2drop
2 ' 2lits> ' >2lits ' >lits folder1
    dup folds * and or xor
    dup folds min max umin umax
    dup folds nip
    dup folds rshift lshift arshift rol ror
    dup folds = > >= < <= u> u>= u< u<=
    dup folds d0> d0< d0=
    drop
2 ' 2lits> ' >2lits ' >2lits folder1
    folds m* um* swap d2* /modf /mods u/mod bounds
2 ' 2lits> ' >2lits ' >3lits folder1
    folds over tuck
3 ' 3lits> ' >3lits ' >lits folder1
    folds within select mux */f */s u*/
3 ' 3lits> ' >3lits ' >2lits folder1
    folds um/mod fm/mod sm/rem du/mod */modf */mods u*/mod
3 ' 3lits> ' >3lits ' >3lits folder1
    folds rot -rot
4 ' 4lits> ' >4lits ' >lits folder1
    folds d= d> d>= d< d<= du> du>= du< du<=
4 ' 4lits> ' >4lits ' >2lits folder1
    folds d+ d- 2nip
4 ' 4lits> ' >4lits ' >4lits folder1
    folds 2swap

\ optimize +loop (not quite folding)
: replace-(+loop) ( xt1 -- xt2 )
    case
	['] (+loop)       of ['] (/loop)# endof
	['] (+loop)-lp+!# of ['] (/loop)#-lp+!# endof
	-21 throw
    endcase ;

: (+loop)-optimizer ( xt -- )
    lits# 1 u>= if
	lits> dup 0> if
	    swap replace-(+loop) peephole-compile, , exit then
	>lits then
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

:noname {: xt: op -- :}
    lits# 2 u>= if
	2lits> op >lits exit then
    lits# 1 = if
        0 lits> op ?dup-if
            ['] lit+ peephole-compile, , then
        exit then
    action-of op peephole-compile, ;
dup optimizes +
optimizes -

\ optimize division instructions

2 ' 2lits> ' >2lits ' >lits folder1
dup optimizes /f
dup optimizes modf
dup optimizes /s
dup optimizes mods
dup optimizes u/
dup optimizes umod
drop

