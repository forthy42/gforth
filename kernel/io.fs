\ input output basics				(extra since)	02mar97jaw

\ Authors: Bernd Paysan, Anton Ertl, Jens Wilke, Neal Crook, Gerald Wodni
\ Copyright (C) 1995,1996,1997,1998,2000,2003,2006,2007,2012,2013,2014,2015,2016,2017,2018,2019,2020,2021 Free Software Foundation, Inc.

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
UValue outfile-id ( -- file-id ) \ gforth
UValue debug-fid ( -- file-id ) \ gforth

User out ( -- addr ) \ gforth
\g counts number of characters TYPEd or EMITed; CR resets it

: (type) ( c-addr u -- ) \ gforth
    dup out +!
    outfile-id write-file drop \ !! use ?DUP-IF THROW ENDIF instead of DROP ?
;

: (emit) ( c -- ) \ gforth
    1 out +!
    outfile-id emit-file drop \ !! use ?DUP-IF THROW ENDIF instead of DROP ?
;

: (err-type) ( c-addr u -- ) \ gforth
    dup out +!
    debug-fid write-file drop \ !! use ?DUP-IF THROW ENDIF instead of DROP ?
;

: (err-emit) ( c -- ) \ gforth
    1 out +!
    debug-fid emit-file drop \ !! use ?DUP-IF THROW ENDIF instead of DROP ?
;

#-512 Constant EOK
#-516 Constant EINTR \ error returned for window change
#-521 Constant EBADF

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

: (key) ( -- c / ior ) \ gforth
    infile-id (key-file) ;

: (key?) ( -- flag ) \ gforth
    infile-id key?-file ;

user-o op-vector
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
[IFDEF] (form)
    umethod form ( -- nlines ncols ) \ gforth
[THEN]
umethod page ( -- ) \ facility
umethod at-xy ( x y -- ) \ facility at-x-y
umethod at-deltaxy ( dx dy -- ) \ gforth
umethod attr! ( attr -- ) \ gforth
\G apply attribute to terminal (i.e. set color)
umethod control-sequence ( n char -- ) \ gforth
\G send a control sequence to the terminal
umethod theme-color! ( u -- ) \ gforth
\G Set the terminal to theme-color index @var{u}
2drop

user-o ip-vector
0 0
umethod key-ior ( -- char / ior ) \ core
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
    BEGIN  key-ior dup EINTR =  WHILE  drop winch? off  REPEAT
    dup 0< IF  throw  THEN ;

here
' (type) A,
' (emit) A,
' (cr) A,
[IFDEF] (form) ' (form) A, [THEN]
' noop A, \ page
' 2drop A, \ at-xy
' 2drop A, \ at-deltaxy
' drop A, \ attr!
' 2drop A, \ control-sequence
' drop A, \ theme-color!
A, here AConstant default-out

here
' (err-type) A,
' (err-emit) A,
' (cr) A,
[IFDEF] (form) ' (form) A, [THEN]
' noop A, \ page
' 2drop A, \ at-xy
' 2drop A, \ at-deltaxy
' drop A, \ attr!
' 2drop A, \ control-sequence
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
    ['] noop , ['] 2drop , ['] 2drop , ['] drop , ['] 2drop , ['] drop ,
  DOES> cell+ op-vector ! ;

\ Input                                                13feb93py

04 constant #eof ( -- c ) \ gforth
07 constant #bell ( -- c ) \ gforth
08 constant #bs ( -- c ) \ gforth
09 constant #tab ( -- c ) \ gforth
1B Constant #esc ( -- c ) \ gforth
7F constant #del ( -- c ) \ gforth
0D constant #cr   ( -- c ) \ gforth
\ the newline key code
0C constant #ff ( -- c ) \ gforth
0A constant #lf ( -- c ) \ gforth

: bell  #bell emit [ has? os [IF] ] outfile-id flush-file drop [ [THEN] ] ;

: space ( -- ) \ core
  \G Display one space.
  bl emit ;

\ theme colors

Defer theme!  ' 2drop is theme!
Defer theme@  ' noop is theme@

Variable theme-color#
: theme-color: ( "name" -- )
    Create 1 theme-color# +!@ ,
    ['] theme! set-to
    ['] theme@ set-defer@
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
\G color mod for error highlight inverse

\ space spaces		                                21mar93py

decimal
: spaces-loop ( n addr -- )
    swap  0 max 0 ?DO  I' I - &80 min 2dup type  +LOOP  drop ;
Create spaces ( u -- ) \ core
\G Display @var{n} spaces. 
bl 80 times \ times from target compiler! 11may93jaw
DOES>   ( u -- ) spaces-loop ;
Create backspaces
08 80 times \ times from target compiler! 11may93jaw
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
