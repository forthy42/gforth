\ VARS.FS      Kernal variables

\ Copyright (C) 1995 Free Software Foundation, Inc.

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
 -1 Constant true               \ see starts looking for
                                \ primitives after this word!
  0 Constant false

1 cells Constant cell

20 Constant bl \ used by docon:, must be constant

FF Constant /line

40 Constant c/l
10 Constant l/s
400 Constant chars/block

\ that's enough so long

\ User variables                                       13feb93py

\ doesn't produce real user variables now, but normal variables

Create main-task  100 cells allot
main-task tup H ! minimal

Variable udp \ used by dovar:, must be variable

AUser next-task        main-task next-task !
AUser prev-task        main-task prev-task !
AUser save-task        0 save-task !
AUser s0 \ used by douser:, must be user
AUser r0
AUser f0
AUser l0
AUser handler
\ AUser output
\ AUser input

AUser errorhandler

AUser "error            0 "error !

 User >tib
 User #tib
 User >in               0 >in !
 User blk               0 blk !
 User loadfile          0 loadfile !
\  2User loadfilename	0 0 loadfilename 2!
\ here ," the terminal" align here swap count 2, Value loadfilename
\ align here 0 , 0 , Value loadfilename \ !! should be a `user-value'
 User loadfilename#	0 loadfilename# !

 User loadline          \ number of the currently interpreted
                        \ (in TIB) line if the interpretation
                        \ is in a textfile
                        \ the first line is 1

2User linestart         \ starting file postition of
                        \ the current interpreted line (in TIB)

 User base              A base !
 User dpl               -1 dpl !

 User state             0 state !
AUser normal-dp		\ the usual dictionary pointer
AUser dpp		normal-dp dpp !
			\ the pointer to the current dictionary pointer
                        \ ist reset to normal-dp on (doerror)
                        \  (i.e. any throw caught by quit)
AUser LastCFA
AUser Last

User locals-size \ this is the current size of the locals stack
		 \ frame of the current word


