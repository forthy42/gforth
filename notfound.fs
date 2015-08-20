\ legacy notfound for people who liked the old interface

\ Copyright (C) 2015 Free Software Foundation, Inc.

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

: no.extension -13 throw ;

Defer interpret-notfound1 ' no.extension is interpret-notfound1
Defer compiler-notfound1  ' no.extension is compiler-notfound1
Defer postpone-notfound1  ' no.extension is postpone-notfound1

: r:notfound ( addr u -- ) interpret-notfound1 ;
comp: ( addr u <xt> -- ) drop compiler-notfound1 ;
post: ( addr u <xt> -- ) drop postpone-notfound1 ;
' r:notfound Constant rec:notfound

' rec:notfound get-recognizers 1+ set-recognizers
