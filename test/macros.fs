\ test some gforth extension words

\ Copyright (C) 2009 Free Software Foundation, Inc.

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

require ./tester.fs
decimal

{ : macro1 ]] dup * [[ ; immediate -> }
{ : word1 macro1 ; -> }
{ 5 word1 -> 25 }

{ : macro2 7 ]]l + [[ ; immediate -> }
{ : word2 macro2 ; -> }
{ 8 word2 -> 15 }

{ : macro3 s" test" ]]2l compare [[ ; immediate -> }
{ : word3 macro3 ; -> }
{ s" tess" word3 -> -1 }
{ s" test" word3 -> 0 }
{ s" tesu" word3 -> 1 }

{ : macro4 4e ]]fl f+ [[ ; immediate -> }
{ : word4 macro4 ; -> }
{ 5e word4 -> 9e }

{ : macro5 ]] 0< if macro1 [[ ; immediate -> }
{ : word5 dup macro5 then ; -> }
{ -5 word5 -> 25 }
{ 5 word5 -> 5 }

\ test multi-line ]]
: macro6 ]]
0<
     

    if 
	macro1 [[ ; immediate
{ : word6 dup macro6 then ; -> }
{ -5 word6 -> 25 }
{ 5 word6 -> 5 }
