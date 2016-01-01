\ callable object
\ Copyright (C) 2013 Free Software Foundation, Inc.

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

' spaces cell- @ callable vtsize move

: do-callable ( body -- )
    body> >o call-xt perform o> ;

: callable! ( xt callable -- )
   ['] do-callable >body over does-code! >o call-xt ! o> ;