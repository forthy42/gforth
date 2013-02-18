\ list.fs  linked list stuff 

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

struct
    cell% field list-next
    1 0   field list-payload
end-struct list%

: list-insert { node list -- }
    list list-next @ node list-next !
    node list list-next ! ;

: list-append { node endlistp -- }
    \ insert node at place pointed to by endlistp
    node endlistp @ list-insert
    node list-next endlistp ! ;

: list-map ( ... list xt -- ... )
    \ xt ( ... node -- ... )
    { xt } begin { node }
	node while
	    node xt execute
	    node list-next @
    repeat ;
