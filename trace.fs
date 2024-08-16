\ generate a trace for words defined after loading this file

\ Author: Anton Ertl
\ Copyright (C) 2024 Free Software Foundation, Inc.
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


\ usage example:
\ gforth trace.fs fib.fs -e 'stdout to tracefile 5 fib . cr bye'


0 value tracefile

: .tracepoint ( xpos -- )
    tracefile if
        [: .sourceview ":\n" type ;] tracefile outfile-execute
    else
        drop
    then ;

: compile-.currentpos ( -- )
    current-sourceview ]] literal .tracepoint [[ ;

:noname defers :-hook             compile-.currentpos ; is :-hook
:noname defers if-like            compile-.currentpos ; is if-like
:noname defers until-like         compile-.currentpos ; is until-like
\ :noname defers basic-block-end    compile-.currentpos ; is basic-block-end
:noname defers exit-like          compile-.currentpos ; is exit-like
\ :noname defers before-line        compile-.currentpos ; is before-line
:noname defers then-like          compile-.currentpos ; is then-like

