\ report stack depth changes in source code in various (optional) ways

\ Copyright (C) 2004,2007 Free Software Foundation, Inc.

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


\ Use this program like this:
\ include it, then the program you want to check
\ e.g., start it with
\  gforth depth-changes.fs myprog.fs

\ By default this will report stack depth changes at every empty line
\ in interpret state.  You can vary this by using

\  gforth depth-changes.fs -e "' <word> IS depth-changes-filter" myprog.fs

\ with the following values for <word>:

\ <word>      meaning
\ all-lines   every line in interpret state
\ most-lines  every line in interpret state not ending with "\"

2variable last-depths

defer depth-changes-filter ( -- f )
\G true if the line should be checked for depth changes
    
: all-lines ( -- f )
    state @ 0= ;

: empty-lines ( -- f )
    source (parse-white) nip 0= all-lines and ;

: most-lines ( -- f )
    source dup if
	1- chars + c@ '\ <>
    else
	2drop true
    endif
    all-lines and ;

' empty-lines is depth-changes-filter

: check-line ( -- )
    depth-changes-filter if
	sp@ fp@ last-depths 2@
	2over last-depths 2!
	d<> if
	    ['] ~~ execute
	endif
    endif ;

sp@ fp@ last-depths 2!

' check-line is line-end-hook
