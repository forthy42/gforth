\ input output basics				(extra since)	02mar97jaw

\ Authors: Bernd Paysan, Anton Ertl, Jens Wilke, Neal Crook, Gerald Wodni
\ Copyright (C) 1995,1996,1997,1998,2000,2003,2006,2007,2012,2013,2014,2015,2016,2017,2018,2019,2020,2021,2022,2023,2024 Free Software Foundation, Inc.

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

UValue infile-id ( -- file-id ) \ gforth
\G @i{File-id} is used by @code{key}, @code{?key}, and anything that
\G refers to the "user input device".  By default @code{infile-id}
\G produces the process's @code{stdin}, unless changed with
\G @code{infile-execute}.
UValue outfile-id ( -- file-id ) \ gforth
\G @i{File-id} is used by @code{emit}, @code{type}, and any output
\G word that does not take a file-id as input.  By default
\G @code{outfile-id} produces the process's @code{stdout}, unless
\G changed with @code{outfile-execute}.
UValue debug-fid ( -- file-id ) \ gforth @i{File-id} is used by
\G debugging words for output.  By default it is the process's
\G @code{stderr}.

User out ( -- addr ) \ gforth
\g @i{Addr} contains a number that tries to give the position of
\g the cursor within the current line on the user output device: It
\g resets to 0 on @code{cr}, increases by the number of characters by
\g @code{type} and @code{emit}, and decreases on @code{backspaces}.
\g Unfortunately, it does not take into account tabs, multi-byte
\g characters, or the existence of Unicode characters with width 0 and
\g 2, so it only works for simple cases.

: (type) ( c-addr u -- )
    dup out +!
    outfile-id write-file drop \ !! use ?DUP-IF THROW ENDIF instead of DROP ?
;

: (emit) ( c -- )
    1 out +!
    outfile-id emit-file drop \ !! use ?DUP-IF THROW ENDIF instead of DROP ?
;

: (err-type) ( c-addr u -- )
    dup out +!
    debug-fid write-file drop \ !! use ?DUP-IF THROW ENDIF instead of DROP ?
;

: (err-emit) ( c -- )
    1 out +!
    debug-fid emit-file drop \ !! use ?DUP-IF THROW ENDIF instead of DROP ?
;

#-512 Constant EOK
#-516 Constant EINTR \ error returned for window change
unlock H base @ decimal lock T \ suppress warning about EBADF being a literal
#-521 Constant EBADF
unlock H base ! lock T

: key-file ( fd -- key ) \ gforth
    \G Read one character @i{n} from @i{wfileid}.  This word disables
    \G buffering for @i{wfileid}.  If you want to read characters from a
    \G terminal in non-canonical (raw) mode, you have to put the terminal
    \G in non-canonical mode yourself (using the C interface); the
    \G exception is @code{stdin}: Gforth automatically puts it into
    \G non-canonical mode.
    BEGIN  dup (key-file) dup EINTR =  WHILE  drop  REPEAT
    dup EOK = over EBADF = or  IF  2drop -1  EXIT  THEN \ eof = -1
    dup 0< IF  throw  THEN  nip ;

: (key) ( -- c / ior )
    0 winch? atomic!@ IF  EINTR  ELSE  infile-id (key-file)  THEN ;

: (key?) ( -- flag )
    infile-id key?-file  winch? @ or ;

user-o op-vector
0 0
umethod type ( c-addr u -- ) \ core
  \G If @var{u}>0, display @var{u} characters from a string starting
  \G with the character stored at @var{c-addr}.
umethod emit ( c -- ) \ core
  \G Send the byte @i{c} to the current output; for ASCII characters,
  \G @code{emit} is equivalent to @code{xemit}.
umethod cr ( -- ) \ core c-r
    \G Output a newline (of the favourite kind of the host OS).  Note
    \G that due to the way the Forth command line interpreter inserts
    \G newlines, the preferred way to use @code{cr} is at the start
    \G of a piece of text; e.g., @code{cr ." hello, world"}.
[IFDEF] (form)
    umethod form ( -- nlines ncols ) \ gforth
[THEN]
umethod page ( -- ) \ facility
\G Clear the screen
umethod at-xy ( x y -- ) \ facility at-x-y
\G Put the curser at position @i{x y}.  The top left-hand corner of
\G the display is at 0 0.

umethod at-deltaxy ( dx dy -- ) \ gforth
\G With the current position at @i{x y}, put the cursor at @i{x+dx
\G y+dy}.

umethod attr! ( attr -- )
\G apply attribute to terminal (i.e. set color)
umethod control-sequence ( n char -- )
\G send a control sequence to the terminal
umethod theme-color! ( u -- )
\G Set the terminal to theme-color index @var{u}
2drop

user-o ip-vector
0 0
umethod key-ior ( -- char|ior ) \ gforth
\G Receive (but do not display) one character, @var{char}, in case of an
\G error or interrupt, return the negative @var{ior} instead.
umethod key? ( -- flag ) \ facility key-question
\G Determine whether a character is available. If a character is
\G available, @var{flag} is true; the next call to @code{key} will
\G yield the character. Once @code{key?} returns true, subsequent
\G calls to @code{key?} before calling @code{key} or @code{ekey} will
\G also return true.
2drop

: (cr) ( -- )
    newline type 0 out ! ;

: key ( -- char ) \ core
\G Receive (but do not display) one character, @var{char}.
    BEGIN  key-ior dup EINTR =  WHILE  drop  REPEAT
    dup 0< IF  throw  THEN ;

here
' (type) A,
' (emit) A,
' (cr) A,
[IFDEF] (form) ' (form) A, [THEN]
' n/a A, \ page
' n/a A, \ at-xy
' n/a A, \ at-deltaxy
' n/a A, \ attr!
' n/a A, \ control-sequence
' drop A, \ theme-color!
A, here AConstant default-out

here
' (err-type) A,
' (err-emit) A,
' (cr) A,
[IFDEF] (form) ' (form) A, [THEN]
' n/a A, \ page
' n/a A, \ at-xy
' n/a A, \ at-deltaxy
' n/a A, \ attr!
' n/a A, \ control-sequence
' drop A, \ theme-color!
A, here AConstant debug-out

default-out op-vector !

AUser debug-vector
debug-out debug-vector !

here
' (key) A,
' (key?) A,
A, here AConstant default-in

default-in ip-vector !

: input: ( key-xt key?-xt -- )
    Create here cell+ , swap , ,
  DOES> cell+ ip-vector ! ;

: output: ( type-xt emit-xt cr-xt form-xt -- )
    Create here cell+ , swap 2swap swap , , , ,
    ['] n/a , ['] n/a , ['] n/a , ['] n/a , ['] n/a , ['] drop ,
  DOES> cell+ op-vector ! ;

\ Input                                                13feb93py

04 constant #eof ( -- c ) \ gforth number-e-o-f
\G actually EOT (ASCII code 4 aka @code{^D})
07 constant #bell ( -- c ) \ gforth number-bell
08 constant #bs ( -- c ) \ gforth number-b-s
09 constant #tab ( -- c ) \ gforth number-tab
1B Constant #esc ( -- c ) \ gforth number-esc
7F constant #del ( -- c ) \ gforth number-del
0D constant #cr   ( -- c ) \ gforth number-c-r
\ the newline key code
0C constant #ff ( -- c ) \ gforth number-f-f
0A constant #lf ( -- c ) \ gforth number-l-f

: bell  #bell emit [ has? os [IF] ] outfile-id flush-file drop [ [THEN] ] ;

: space ( -- ) \ core
  \G Display one space.
  bl emit ;

\ theme colors

Defer theme!  ' 2drop is theme!
Defer theme@  ' noop is theme@

Create theme-table ' theme! A, ' n/a A, ' n/a A, ' theme@ A,

' [noop] theme-table to-class: theme-to ( n value-xt -- ) \ gforth-internal

Variable theme-color#
: theme-color: ( "name" -- )
    Create 1 theme-color# +!@ ,
    ['] theme-to set-to
  DOES> @ theme-color! ;

theme-color: default-color ( -- ) \ gforth
\G use system-default color
theme-color: error-color   ( -- ) \ gforth
\G error color: red
theme-color: warning-color ( -- ) \ gforth
\G color for warnings: blue/yellow on black terminals
theme-color: info-color    ( -- ) \ gforth
\G color for info: green/cyan on black terminals
theme-color: success-color ( -- ) \ gforth
\G color for success: green
theme-color: input-color   ( -- ) \ gforth
\G color for user-input: black/white (both bold)
theme-color: error-hl-ul ( -- ) \ gforth
\G color mod for error highlight underline
theme-color: error-hl-inv ( -- ) \ gforth
\G color mod for error highlight inverse
theme-color: status-color ( -- ) \ gforth
\G color mod for status bar
theme-color: compile-color ( -- ) \ gforth
\G color mod for status bar in compile mode
theme-color: postpone-color ( -- ) \ gforth
\G color mod for status bar in compile mode

\ space spaces		                                21mar93py

decimal
: spaces-loop ( n addr -- )
    swap  0 max 0 ?DO  delta-I &80 min 2dup type  +LOOP  drop ;
Create spaces ( u -- ) \ core
\G Display @var{u} spaces. 
bl 80 c,s \ c,s from target compiler! 11may93jaw
DOES>   ( u -- ) spaces-loop ;
Create backspaces \ gforth
08 80 c,s \ c,s from target compiler! 11may93jaw
DOES>   ( u -- ) over 2* negate out +! spaces-loop ;
hex

Defer deadline ( d -- )
\G wait to absolute time @var{d} in ns since 1970-1-1 0:00:00+000
: kernel-deadline ( d -- )
    BEGIN  2dup ntime d- 2dup d0< IF  2drop #0.  THEN
    #1000000000 um/mod (ns) EINTR <> UNTIL
    2drop ;
' kernel-deadline IS deadline
: ns ( d -- ) \ gforth
    ntime d+ deadline ;
: ms ( n -- ) \ facility-ext
    #1000000 um* ns ;
