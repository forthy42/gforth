\ input output basics				(extra since)	02mar97jaw

\ Copyright (C) 1995,1996,1997,1998,2000,2003,2006,2007 Free Software Foundation, Inc.

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

\ Output                                               13feb93py

has? os [IF]
0 Value outfile-id ( -- file-id ) \ gforth
0 Value infile-id ( -- file-id ) \ gforth
    
: (type) ( c-addr u -- ) \ gforth
    outfile-id write-file drop \ !! use ?DUP-IF THROW ENDIF instead of DROP ?
;

: (emit) ( c -- ) \ gforth
    outfile-id emit-file drop \ !! use ?DUP-IF THROW ENDIF instead of DROP ?
;

: (key) ( -- c ) \ gforth
    infile-id key-file ;

: (key?) ( -- flag ) \ gforth
    infile-id key?-file ;
[THEN]

undef-words

Defer type ( c-addr u -- ) \ core
  \G If @var{u}>0, display @var{u} characters from a string starting
  \G with the character stored at @var{c-addr}.
[IFDEF] write-file
: (type) 0 write-file drop ;
[ELSE]
: (type) BEGIN dup WHILE
    >r dup c@ (emit) 1+ r> 1- REPEAT 2drop ;
[THEN]

[IFDEF] (type) ' (type) IS Type [THEN]

Defer emit ( c -- ) \ core
  \G Display the character associated with character value c.
: (emit) ( c -- ) \ gforth
    0 emit-file drop \ !! use ?DUP-IF THROW ENDIF instead of DROP ?
;

[IFDEF] (emit) ' (emit) IS emit [THEN]

Defer key ( -- char ) \ core
\G Receive (but do not display) one character, @var{char}.
: (key) ( -- c ) \ gforth
    infile-id key-file ;
: infile-id  stdin ;

[IFDEF] (key) ' (key) IS key [THEN]

Defer key? ( -- flag ) \ facility key-question
\G Determine whether a character is available. If a character is
\G available, @var{flag} is true; the next call to @code{key} will
\G yield the character. Once @code{key?} returns true, subsequent
\G calls to @code{key?} before calling @code{key} or @code{ekey} will
\G also return true.
: (key?) ( -- flag ) \ gforth
    infile-id key?-file ;
: infile-id  stdin ;

[IFDEF] (key?) ' (key?) IS key? [THEN]

all-words

: (.")     "lit count type ;
: (S")     "lit count ;

\ Input                                                13feb93py

04 constant #eof ( -- c ) \ gforth
07 constant #bell ( -- c ) \ gforth
08 constant #bs ( -- c ) \ gforth
09 constant #tab ( -- c ) \ gforth
7F constant #del ( -- c ) \ gforth
0D constant #cr   ( -- c ) \ gforth
\ the newline key code
0C constant #ff ( -- c ) \ gforth
0A constant #lf ( -- c ) \ gforth

: bell  #bell emit [ has? os [IF] ] outfile-id flush-file drop [ [THEN] ] ;
: cr ( -- ) \ core c-r
    \G Output a newline (of the favourite kind of the host OS).  Note
    \G that due to the way the Forth command line interpreter inserts
    \G newlines, the preferred way to use @code{cr} is at the start
    \G of a piece of text; e.g., @code{cr ." hello, world"}.
    newline type ;

: space ( -- ) \ core
  \G Display one space.
  bl emit ;

has? os 0= [IF]
: spaces ( n -- ) \ core
  \G If n > 0, display n spaces. 
  0 max 0 ?DO space LOOP ;
: backspaces  0 max 0 ?DO  #bs emit  LOOP ;
[ELSE]
\ space spaces		                                21mar93py
decimal
Create spaces ( u -- ) \ core
  \G Display @var{n} spaces. 
  bl 80 times \ times from target compiler! 11may93jaw
DOES>   ( u -- )
  swap
  0 max 0 ?DO  I' I - &80 min 2dup type  +LOOP  drop ;
Create backspaces
  08 80 times \ times from target compiler! 11may93jaw
DOES>   ( u -- )
  swap
  0 max 0 ?DO  I' I - &80 min 2dup type  +LOOP  drop ;
hex
[THEN]

