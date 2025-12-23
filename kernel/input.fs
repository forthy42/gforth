\ Input handling (object oriented)                      22oct00py

\ Authors: Anton Ertl, Bernd Paysan, Gerald Wodni
\ Copyright (C) 2000,2003,2004,2005,2006,2007,2011,2013,2014,2015,2016,2017,2018,2019,2020,2021,2022,2023,2024 Free Software Foundation, Inc.

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

\ helper words

: input-lexeme! ( c-addr u -- )
    \ record that the current lexeme us c-addr u
    input-lexeme 2!
    current-sourceview 0 new-where 2! ;

: input-start-line ( -- )
    >in off  source drop 0 input-lexeme! ;

\ terminal input implementation

: .scanning ( -- )
    ." scanning for [THEN]" cr ;
Defer .unstatus ' noop is .unstatus

:noname ( in line# 1 -- )
    2 <> -12 and throw
    loadline @ <> -12 and throw
    >in ! ;           \ restore-input
:noname ( -- in line# 1 ) >in @ loadline @ 2 ;     \ save-input
' false                \ source-id
:noname ( -- flag )
    [ has? file [IF] ] stdin file-eof?  IF  false  EXIT  THEN [ [THEN] ]
    scanning? IF  warning-color .scanning default-color  THEN
    tib max#tib @ accept #tib !
    .unstatus
    input-start-line true 1 loadline +! ;     \ refill
:noname ( -- addr u ) tib #tib @ ;   \ source

| Create terminal-input   A, A, A, A, A,
:noname ( -- addr u ) tib @ #tib @ ; \ source
| Create evaluate-input
    A,                  \ source
    ' false A,          \ refill
    ' true A,           \ source-id
    terminal-input 3 cells + @ A, \ terminal::restore-input
    terminal-input 4 cells + @ A, \ terminal::save-input

\ file input implementation

has? file [IF]
: read-line ( c_addr u1 wfileid -- u2 flag wior ) \ file
    \G Reads a line from @i{wfileid} into the buffer at @i{c_addr u1}.
    \G Gforth supports all three common line terminators: LF, CR and
    \G CRLF.  A non-zero @i{wior} indicates an error.  A false
    \G @i{flag} indicates that @code{read-line} has been invoked at
    \G the end of the file. @i{u2} indicates the line length (without
    \G terminator): @i{u2}<@i{u1} indicates that the line is @i{u2}
    \G chars long; @i{u2}=@i{u1} indicates that the line is at least
    \G @i{u1} chars long, @i{u1} chars of the buffer have been
    \G filled with chars from the line, and the next slice of the line
    \G will be read with the next @code{read-line}.  If the line is
    \G @i{u1} chars long, the first @code{read-line} returns
    \G @i{u2}=@i{u1} and the next read-line returns @i{u2}=0.
    (read-line) nip ;

:noname  ( in line# udpos file 5 -- )
    5 <> -12 and throw
    loadfile @ <> -12 and throw
    loadfile @ reposition-file throw
    refill 0= -36 and throw \ should never throw
    loadline ! >in ! ; \ restore-input
:noname  ( -- in line# udpos 5 )
    >in @ sourceline#
    loadfile @ file-position throw #fill-bytes @ 0 d-
    loadfile @ 5 ;     \ save-input
:noname  ( -- file ) loadfile @ ;  \ source-id
:noname  ( -- flag )
    #tib off #fill-bytes off input-start-line
    BEGIN
	tib max#tib @ #tib @ /string dup #fill-bytes @ + >r
	loadfile @ (read-line) throw #fill-bytes +!
	swap #tib +!
	\ auto-expanding the tib
	dup r> #fill-bytes @ = and  WHILE
	    #tib @ #fill-bytes @ =  WHILE
		drop max#tib @ 2* expand-tib
    REPEAT  THEN
    #tib @ 0<> or  1 loadline +! ;
                       \ refill
terminal-input @       \ source -> terminal-input::source

| Create file-input  A, A, A, A, A,
[THEN]

\ push-file, pop-file

: new-tib ( method n -- ) \ gforth-internal
    \G Create a new entry of the tib stack, size @i{n}, method table
    \G @i{method}.
    dup >r tib+ + dup cell+ allocate throw tuck swap 0 fill
    current-input @ over cell+ current-input ! old-input ! r> max#tib !
    ! ;
: expand-tib ( n -- )
    dup tib+ +
    current-input @ cell- swap cell+ resize throw cell+ current-input !
    max#tib ! tib max#tib @ #tib @ /string 0 fill ;

: push-file  ( -- ) \ gforth-internal
    \G Create a new file input buffer
    file-input def#tib new-tib ;

: pop-file ( throw-code -- throw-code ) \ gforth-internal
    \G pop and free the current top input buffer
    dup IF
	input-error-data >error
    THEN
    current-input @ old-input @ current-input ! cell- free throw ;

\ save-input, restore-input

: save-input ( -- x1 .. xn n ) \ core-ext
    \G The @i{n} entries @i{xn - x1} describe the current state of the
    \G input source specification, in some platform-dependent way that can
    \G be used by @code{restore-input}.
    (save-input) current-input @ swap 1+ ;

: restore-input ( x1 .. xn n -- flag ) \ core-ext
    \G Attempt to restore the input source specification to the state
    \G described by the @i{n} entries @i{xn - x1}. @i{flag} is true if
    \G the restore fails.  In Gforth 1.0, it is
    \G possible to save and restore between different active input
    \G streams. Note that closing the input streams must happen in the
    \G reverse order as they have been opened, but as long as they are
    \G both active, everything is allowed.  These Gforth versions only
    \G produce non-zero flags as results of @word{catch}ing some
    \G exception, and the @i{flag} itself is the @code{throw}n ball
    \G and can be re@code{throw}n.
    current-input @ >r swap current-input ! 1- dup >r
    ['] (restore-input) catch
    dup IF
        r> 0 ?DO
            nip LOOP
        r> current-input ! first-throw on 0<> EXIT
    THEN
    rdrop rdrop ;

\ create terminal input block

: create-input ( -- )
    \G create a new terminal input
    terminal-input def#tib new-tib  -1 loadfilename# ! ;

: execute-parsing-wrapper ( ... addr1 u1 xt addr2 u2 -- ... ) \ gforth-internal
    \ addr1 u1 is the string to be processed, xt is the word for
    \ processing it, addr2 u2 is the name of the input source
    rot >r 2>r evaluate-input cell new-tib 2r> 
[ has? file [IF] ]
    str>loadfilename# loadfilename# !
[ [ELSE] ]
    2drop
[ [THEN] ]
    -1 loadline ! #tib ! tib !
    r> catch pop-file throw ;

: execute-parsing ( ... addr u xt -- ... ) \ gforth
\G Make @i{addr u} the current input source, execute @i{xt @code{(
\G ... -- ... )}}, then restore the previous input source.
    s" *evaluated string*" execute-parsing-wrapper ;

: evaluate ( ... addr u -- ... ) \ core,block
\G Save the current input source.  Store @code{-1} in @code{source-id}
\G and @code{0} in @code{blk}. Set @code{>IN} to @code{0} and make the
\G string @i{c-addr u} the input source and input
\G buffer. Tex-interpret the input buffer. When the parse area is
\G empty, restore the input source.
    ['] interpret2 execute-parsing ;

\ clear tibstack

: clear-tibstack ( -- ) \ gforth-internal
    \G clears the tibstack; if there is none, create the bottom entry:
    \G the terminal input buffer.
    current-input @ 0= IF  create-input  THEN
    BEGIN  old-input @  WHILE  0 pop-file drop  REPEAT ;

: query ( -- ) \ gforth-obsolete
    \G Make the user input device the input source. Receive input into
    \G the Terminal Input Buffer. Set @code{>IN} to zero. OBSOLETE:
    \G This Forth-94 word has been de-standardized in Forth-2012.  It
    \G is superceeded by @code{accept}.
    clear-tibstack refill 0= -39 and throw ;

\ load a file

defer line-end-hook ( -- ) \ gforth
\G Deferred word called at every end-of-line when text-interpreting
\G from a file.
\ alternatively we could use a wrapper for REFILL
' noop is line-end-hook
Defer eof-warning
' noop is eof-warning

: read-loop1 ( i*x -- j*x )
    BEGIN  refill  WHILE  interpret line-end-hook REPEAT ;

: read-loop ( i*x -- j*x ) \ gforth-internal
    \G refill and interpret a file until EOF
    ['] read-loop1 bt-rp0-wrapper eof-warning ;

Variable second-ctrl-c 0 second-ctrl-c !

: ?end-input ( throwcode -- throwcode )
    dup -56 =
    over -28 = dup second-ctrl-c !@ and or IF  bye  THEN  throw ;

: get-input ( -- flag ) \ gforth-internal
    \G read a line of input
    ['] refill catch ?end-input ;

: get-input-colored ( -- flag ) \ gforth-internal
    \G perform get-input colored with input-color
    input-color ['] refill catch default-color ?end-input ;

Defer ?set-current-view ( -- )
' noop is ?set-current-view

: execute-parsing-named-file ( i*x wfileid filename-addr filename-u xt -- j*x )
    >r push-file \ dup 2* cells included-files 2@ drop + 2@ type
    str>loadfilename# loadfilename# !  loadfile !  error-stack $free
    r> catch  dup IF  ?set-current-view  THEN
    loadfile @ close-file swap 2dup or
    pop-file  drop throw throw ;

: execute-parsing-file ( i*x fileid xt -- j*x ) \ gforth
\G Make @i{fileid} the current input source, execute @i{xt @code{( i*x
\G -- j*x )}}, then restore the previous input source.
    s" *a file*" rot execute-parsing-named-file ;

: include-file ( i*x wfileid -- j*x ) \ file
    \G Interpret (process using the text interpreter) the contents of
    \G the file @var{wfileid}.
    ['] read-loop execute-parsing-file ;
