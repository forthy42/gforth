\ Simple debugging aids

\ Copyright (C) 1995,1997,1999,2002,2003,2004,2005,2006,2007 Free Software Foundation, Inc.

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


\ They are meant to support a different style of debugging than the
\ tracing/stepping debuggers used in languages with long turn-around
\ times.

\ IMO, a much better (faster) way in fast-compilig languages is to add
\ printing code at well-selected places, let the program run, look at
\ the output, see where things went wrong, add more printing code, etc.,
\ until the bug is found.

\ We support fast insertion and removal of the printing code.

\ !!Warning: the default debugging actions will destroy the contents
\ of the pictured numeric output string (i.e., don't use ~~ between <#
\ and #>).

require source.fs

defer printdebugdata ( -- ) \ gforth print-debug-data
' .s IS printdebugdata
defer .debugline ( nfile nline -- ) \ gforth print-debug-line
\G Print the source code location indicated by @var{nfile nline}, and
\G additional debugging information; the default @code{.debugline}
\G prints the additional information with @code{printdebugdata}.

: (.debugline) ( nfile nline -- )
    cr .sourcepos ." :"
    \ it would be nice to print the name of the following word,
    \ but that's not easily possible for primitives
    printdebugdata
    cr ;

' (.debugline) IS .debugline

: .debugline-stderr ( nfile nline -- )
    ['] .debugline stderr outfile-execute ;

:noname ( -- )
    current-sourcepos .debugline-stderr ;
:noname ( compilation  -- ; run-time  -- )
    compile-sourcepos POSTPONE .debugline-stderr ;
interpret/compile: ~~ ( -- ) \ gforth tilde-tilde
\G Prints the source code location of the @code{~~} and the stack
\G contents with @code{.debugline}.

