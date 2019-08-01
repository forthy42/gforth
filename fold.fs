\ Constant folding for some primitives

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

: folder ( m xt-pop xt-push xt-compile, -- xt1 )
    \ xt1 ( xt -- ) compiles xt with constant folding: xt ( m*n -- l*n ).
    \ xt-pop pops m items from literal stack to data stack, xt-push
    \ pushes l items from data stack to literal stack.
    [{: m xt: pop xt: push xt: comp, :}d
	lits# m u>= IF
	    >r pop r> execute push
	ELSE  comp,  THEN ;] ;

: folds ( folder-xt "name1" ... "namen" <eol> -- )
    {: folder-xt :} BEGIN
	>in @ >r parse-name r> >in !
	nip  WHILE
	    vt, ' dup (make-latest)
	    folder-xt set-optimizer
    REPEAT ;

1 ' lits>  ' >lits ' peephole-compile, folder
dup folds invert abs negate >pow2
dup folds 1+ 1- 2* 2/ cells cell/
dup folds floats sfloats dfloats
dup folds float/ sfloat/ dfloat/
dup folds c>s w>s l>s w>< l>< x><
dup folds wcwidth
folds 0> 0= 0<
1 ' lits>  ' >2lits ' peephole-compile, folder
    folds dup
1 ' lits> ' >2lits ' :, folder
    folds s>d
2 ' 2lits> ' >lits ' peephole-compile, folder
dup folds + - * / mod u/ umod and or xor
dup folds min max umin umax
dup folds drop nip
dup folds rshift lshift arshift rol ror
dup folds = > >= < <= u> u>= u< u<=
    folds d0> d0< d0=
2 ' 2lits> ' >2lits ' peephole-compile, folder
    folds m* um* /mod swap d2*
2 ' 2lits> ' >2lits ' :, folder
    folds bounds
2 ' 2lits> ' >3lits ' peephole-compile, folder
    folds over tuck
3 ' 3lits> ' >lits ' peephole-compile, folder
    folds */ within
3 ' 3lits> ' >2lits ' peephole-compile, folder
    folds um/mod fm/mod sm/rem */mod du/mod
3 ' 3lits> ' >3lits ' peephole-compile, folder
    folds rot -rot
4 ' 4lits> ' >lits ' peephole-compile, folder
    folds d= d> d>= d< d<= du> du>= du< du<=
4 ' 4lits> ' >2lits ' peephole-compile, folder
dup folds d+ d-
    folds 2drop 2nip
4 ' 4lits> ' >4lits ' peephole-compile, folder
    folds 2swap

: optimizes ( xt "name" -- )
    \ xt is optimizer of "name"
    vt, ' dup (make-latest) set-optimizer ;

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
