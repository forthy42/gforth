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

: folder [{: n xt: pop xt: push xt: comp, :}d
	lits# n u>= IF
	    >r pop r> execute push
	ELSE  comp,  THEN ;] ( xt ) ;
: folds {: folder-xt -- :}
    BEGIN  >in @ >r parse-name r> >in !
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
