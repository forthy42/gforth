\ VARS.FS      Kernal variables

\ Copyright (C) 1995,1996,1997,1998 Free Software Foundation, Inc.

\ This file is part of Gforth.

\ Gforth is free software; you can redistribute it and/or
\ modify it under the terms of the GNU General Public License
\ as published by the Free Software Foundation; either version 2
\ of the License, or (at your option) any later version.

\ This program is distributed in the hope that it will be useful,
\ but WITHOUT ANY WARRANTY; without even the implied warranty of
\ MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
\ GNU General Public License for more details.

\ You should have received a copy of the GNU General Public License
\ along with this program; if not, write to the Free Software
\ Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.

hex \ everything now hex!                               11may93jaw

\ important constants                                  17dec92py

\ dpANS6 (sect 3.1.3.1) says 
\ "a true flag ... [is] a single-cell value with all bits set"
\ better definition: 0 0= constant true ( no dependence on 2's compl)
 -1 Constant true ( -- f ) \ core-ext
\G CONSTANT: f is a cell with all bits set.
\ see starts looking for primitives after this word!

  0 Constant false ( -- f ) \ core-ext
\G CONSTANT: f is a cell with all bits clear.

1 cells Constant cell ( -- u ) \ gforth
1 floats Constant float ( -- u ) \ gforth

20 Constant bl ( -- c-char ) \ core
\G c-char is the character value for a space.
\ used by docon:, must be constant

FF Constant /line

40 Constant c/l
10 Constant l/s
400 Constant chars/block

$20 8 2* cells + 2 + cell+ constant word-pno-size ( -- u )
84 constant pad-minsize ( -- u )

\ that's enough so long

\ User variables                                       13feb93py

\ initialized by COLD

Create main-task  has? OS [IF] 100 [ELSE] 40 [THEN] cells allot

\ set user-pointer from cross-compiler right
main-task 
UNLOCK tup ! LOCK

Variable udp \ user area size? -anton

AUser next-task        main-task next-task !
AUser prev-task        main-task prev-task !
AUser save-task        0 save-task !
AUser sp0 ( -- a-addr ) \ gforth
\G USER VARIABLE: Initial value of the data stack pointer.
\ sp0 is used by douser:, must be user
    ' sp0 Alias s0 ( -- a-addr ) \ gforth
\G OBSOLETE alias of @code{sp0}

AUser rp0 ( -- a-addr ) \ gforth
\G USER VARIABLE: Initial value of the return stack pointer.
    ' rp0 Alias r0 ( -- a-addr ) \ gforth
\G OBSOLETE alias of @code{rp0}

AUser fp0 ( -- a-addr ) \ gforth
\G USER VARIABLE: Initial value of the floating-point stack pointer.
\ no f0, because this leads to unexpected results when using hex

AUser lp0 ( -- a-addr ) \ gforth
\G USER VARIABLE: Initial value of the locals stack pointer.
    ' lp0 Alias l0 ( -- a-addr ) \ gforth
\G OBSOLETE alias of @code{lp0}

AUser handler	\ pointer to last throw frame
AUser backtrace-empty \ true if the next THROW should store a backtrace
\ AUser output
\ AUser input

AUser errorhandler

AUser "error            0 "error !

[IFUNDEF] #tib		\ in ec-Version we may define this ourself
 User tibstack		\ saves >tib in execute
 User >tib		\ pointer to terminal input buffer
 User #tib ( -- a-addr ) \ core-ext
 \G USER VARIABLE: a-addr is the address of a cell containing
 \G the number of characters in the terminal input buffer.
 \G OBSOLESCENT: @code{source} superceeds the function of this word.

 User >in ( -- a-addr ) \ core
 \G USER VARIABLE: a-addr is the address of a cell containing the
 \G char offset from the start of the terminal input buffer to the
 \G start of the parse area
                        0 >in ! \ char number currently processed in tib
[THEN]
has? file [IF]
 User blk ( -- a-addr ) \ block
 \G USER VARIABLE: a-addr is the address of a cell containing zero
 \G (in which case the input source is not a block and can be identified
 \G by @code{source-id}) or the number of the block currently being
 \G interpreted. A Standard program should not alter @code{blk} directly.
			0 blk !

 User loadfile          0 loadfile !

 User loadfilename#	0 loadfilename# !

 User loadline          \ number of the currently interpreted
                        \ (in TIB) line if the interpretation
                        \ is in a textfile
                        \ the first line is 1

2User linestart         \ starting file postition of
                        \ the current interpreted line (in TIB)
[THEN]

 User base ( -- a-addr ) \ core
 \G USER VARIABLE: a-addr is the address of a cell that stores the
 \G number base used by default for number conversion during input and output.
                        A base !
 User dpl               -1 dpl !

 User state ( -- a-addr ) \ core,tools-ext
 \G USER VARIABLE: a-addr is the address of a cell containing
 \G the compilation state flag. 0 => interpreting, -1 => compiling.
 \G A program shall not directly alter the value of @code{state}. The
 \G following Standard words alter the value in @code{state}:
 \G @code{:} (colon) @code{;} (semicolon) @code{abort} @code{quit}
 \G @code{:noname} @code{[} (left-bracket) @code{]} (right-bracket)
 \G @code{;code}
			0 state !

AUser normal-dp		\ the usual dictionary pointer
AUser dpp		normal-dp dpp !
			\ the pointer to the current dictionary pointer
                        \ ist reset to normal-dp on (doerror)
                        \  (i.e. any throw caught by quit)
AUser LastCFA
AUser Last

has? glocals [IF]
User locals-size \ this is the current size of the locals stack
		 \ frame of the current word
[THEN]

