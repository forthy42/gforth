\ callable object
\ Authors: Bernd Paysan, Anton Ertl
\ Copyright (C) 2013,2016,2019 Free Software Foundation, Inc.

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

require mini-oof2.fs

object class
    >body cell var call-xt
    nip vtsize swap
end-class callable

' spaces >namevt @ callable vtsize move

: do-callable ( body -- )
    body> >o call-xt perform o> ;
' do-callable callable >vtextra !

: callable! ( xt callable -- )
   >o call-xt ! o> ;

: new-callable ( class -- o )  dup >osize @ cell+ cell+
    allocater >o :allocate o> swap over cell+ ! dodoes: over !
    cell+ cell+ dup dup cell- @ >osize @ erase ;
: dispose-callable ( o:o -- o:0 )  o cell- dup dup @ >osize @ cell+ erase
    cell- allocater >o :free o>  0 >o rdrop ;
: clone-callable ( o:o -- o' )
    o cell- @ new-callable o cell- cell- over cell- cell- dup cell+ @
    >osize @ cell+ cell+ move ;
