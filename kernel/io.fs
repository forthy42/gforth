\ input output basics				(extra since)	02mar97jaw

\ Copyright (C) 1995,1996,1997,1998,2000,2003,2006,2007,2012 Free Software Foundation, Inc.

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
0 Value infile-id ( -- file-id ) \ gforth
0 Value outfile-id ( -- file-id ) \ gforth
0 Value debug-fid ( -- file-id ) \ gforth
    
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

user-o current-out
0 0
umethod type ( c-addr u -- ) \ core
  \G If @var{u}>0, display @var{u} characters from a string starting
  \G with the character stored at @var{c-addr}.
umethod emit ( c -- ) \ core
  \G Display the character associated with character value c.
umethod cr ( -- ) \ core c-r
    \G Output a newline (of the favourite kind of the host OS).  Note
    \G that due to the way the Forth command line interpreter inserts
    \G newlines, the preferred way to use @code{cr} is at the start
    \G of a piece of text; e.g., @code{cr ." hello, world"}.
[IFDEF] (form) umethod form [THEN]
umethod page ( -- )
umethod at-xy ( x y -- )
umethod at-deltaxy ( dx dy -- )
umethod attr! ( attr -- )
2drop

user-o current-in
0 0
umethod key ( -- char ) \ core
\G Receive (but do not display) one character, @var{char}.
umethod key? ( -- flag ) \ facility key-question
\G Determine whether a character is available. If a character is
\G available, @var{flag} is true; the next call to @code{key} will
\G yield the character. Once @code{key?} returns true, subsequent
\G calls to @code{key?} before calling @code{key} or @code{ekey} will
\G also return true.
2drop

undef-words

[IFDEF] write-file
: (type) 0 write-file drop ;
[ELSE]
: (type) BEGIN dup WHILE
    >r dup c@ (emit) 1+ r> 1- REPEAT 2drop ;
[THEN]

: (emit) ( c -- ) \ gforth
    0 emit-file drop \ !! use ?DUP-IF THROW ENDIF instead of DROP ?
;

: (key) ( -- c ) \ gforth
    infile-id key-file ;
: infile-id  stdin ;

: (key?) ( -- flag ) \ gforth
    infile-id key?-file ;
: infile-id  stdin ;

: (cr) ( -- )
    newline type ;

all-words

here
' (type) A,
' (emit) A,
' (cr) A,
[IFDEF] (form) ' (form) A, [THEN]
' noop A, \ page
' 2drop A, \ at-xy
' 2drop A, \ at-deltaxy
' noop A, \ attr!
A, here AConstant default-out

default-out current-out !

here
' (key) A,
' (key?) A,
A, here AConstant default-in

default-in current-in !

: input: ( key-xt key?-xt -- )
    Create here cell+ , swap , ,
  DOES> cell+ current-in ! ;

: output: ( type-xt emit-xt cr-xt form-xt -- )
    Create here cell+ , swap 2swap swap , , , ,
    ['] noop , ['] 2drop , ['] 2drop , ['] noop ,
  DOES> cell+ current-out ! ;

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
: spaces-loop ( n addr -- )  swap
  0 max 0 ?DO  I' I - &80 min 2dup type  +LOOP  drop ;
Create spaces ( u -- ) \ core
  \G Display @var{n} spaces. 
  bl 80 times \ times from target compiler! 11may93jaw
DOES>   ( u -- ) spaces-loop ;
Create backspaces
  08 80 times \ times from target compiler! 11may93jaw
DOES>   ( u -- ) spaces-loop ;
hex
[THEN]

