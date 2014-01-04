\ input output basics				(extra since)	02mar97jaw

\ Copyright (C) 1995,1996,1997,1998,2000,2003,2006,2007,2012,2013 Free Software Foundation, Inc.

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

require ./basics.fs
require ../compat/strcomp.fs

\ Output                                               13feb93py

: type ( c-addr u -- ) \ core
  \G If @var{u}>0, display @var{u} characters from a string starting
  \G with the character stored at @var{c-addr}.
    BEGIN dup WHILE
    >r dup c@ emit 1+ r> 1- REPEAT 2drop ;

: (.")     "lit count type ;
: (S")     "lit count ;

\ Input                                                13feb93py

04 constant #eof ( -- c ) \ gforth
07 constant #bell ( -- c ) \ gforth
08 constant #bs ( -- c ) \ gforth
7F constant #del ( -- c ) \ gforth
0D constant #cr   ( -- c ) \ gforth
\ the newline key code
0A constant #lf ( -- c ) \ gforth

: cr ( -- ) \ core c-r
    \G Output a newline (of the favourite kind of the host OS).  Note
    \G that due to the way the Forth command line interpreter inserts
    \G newlines, the preferred way to use @code{cr} is at the start
    \G of a piece of text; e.g., @code{cr ." hello, world"}.
    #lf emit ;
: bell #bell emit ;

: space ( -- ) \ core
  \G Display one space.
  bl emit ;

: spaces ( n -- ) \ core
  \G If n > 0, display n spaces. 
  0 max 0 ?DO space LOOP ;
: backspaces  0 max 0 ?DO  #bs emit  LOOP ;
