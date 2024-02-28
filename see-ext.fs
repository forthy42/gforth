\ see-ext.fs extentions for see locals, floats

\ Authors: Anton Ertl, Bernd Paysan
\ Copyright (C) 1995,1996,1997,2003,2007,2012,2014,2019,2021,2023 Free Software Foundation, Inc.

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

\ made extra 26jan97jaw

get-current also see-voc definitions

: c-loop-lp+!#  c-loop cell+ ;
: c-?branch-lp+!#  c-?branch cell+ ;
: c-branch-lp+!#   c-branch  cell+ ;

: c-flit
    Display? IF
	dup f@ scratch represent 0=
	IF    2drop  scratch 3 min ['] default-color .string
	ELSE
	    IF  '- cemit  THEN  1-
	    scratch over c@ cemit '. cemit 1 /string ['] default-color .string
	    'E cemit
	    dup abs 0 <# #S rot sign #> ['] default-color .string bl cemit
	THEN THEN
    float+ ;

: c-flit0
    c-flit ;

: c-flit1
    cell+ c-flit ;

create c-extend1
[ifdef] flit ' flit A,      ' c-flit A, [then]
[ifdef] flit0 ' flit0 A,    ' c-flit0 A, [then]
[ifdef] flit1 ' flit1 A,    ' c-flit1 A, [then]
        ' ?branch-lp+!# A,  ' c-?branch-lp+!# A,
        ' branch-lp+!# A,   ' c-branch-lp+!# A,
        ' (loop)-lp+!# A,   ' c-loop-lp+!# A,
        ' (+loop)-lp+!# A,  ' c-loop-lp+!# A,
        ' (s+loop)-lp+!# A, ' c-loop-lp+!# A,
        ' (-loop)-lp+!# A,  ' c-loop-lp+!# A,
[IFDEF] (/loop) ' (/loop)-lp+!# A, ' c-loop-lp+!# A, [THEN]
        ' (next)-lp+!# A,   ' c-loop-lp+!# A,
	0 ,		here 0 ,

\ extend see-table
c-extend1 c-extender @ a!
c-extender !

set-current previous
