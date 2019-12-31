\ Input                                                13feb93py

\ Authors: Bernd Paysan, Anton Ertl, Jens Wilke, Neal Crook
\ Copyright (C) 1995,1996,1997,1999,2003,2004,2005,2006,2007,2016,2017,2018,2019 Free Software Foundation, Inc.

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

user-o edit-out

0 0
umethod insert-char
umethod insert-string
umethod edit-control
umethod everychar
umethod everyline
umethod edit-update ( span addr pos1 -- span addr pos1 )
umethod ctrlkeys
cell uvar edit-linew

: (ins) ( max span addr pos1 key -- max span addr pos2 )
    >r  2over = IF  rdrop bell  EXIT  THEN
    2dup + r> swap c! 1+ rot 1+ -rot ;
: (ins-string) ( max span addr pos1 addr1 u1 -- max span addr pos2 )
    2>r  2over r@ + u> IF  2rdrop bell  EXIT  THEN
    2dup + 2r@ rot swap move  2r@ type r@ + rot r> + -rot rdrop ;
: (bs) ( max span addr pos1 -- max span addr pos2 flag )
    dup IF
	#bs emit space #bs emit 1- rot 1- -rot -1 edit-linew +!
    THEN false ;
: (ret) ( max span addr pos1 -- max span addr pos2 flag )
    true ;
: (edit-control) ( max span addr pos1 ctrl-key -- max span addr pos2 flag )
    cells ctrlkeys + perform ;

Create std-ctrlkeys
    ' false a, ' false a, ' false a, ' false a, 
    ' false a, ' false a, ' false a, ' false a,

    ' (bs)  a, ' false a, ' (ret) a, ' false a, 
    ' false a, ' (ret) a, ' false a, ' false a,

    ' false a, ' false a, ' false a, ' false a, 
    ' false a, ' false a, ' false a, ' false a,

    ' false a, ' false a, ' false a, ' false a, 
    ' false a, ' false a, ' false a, ' false a,

: (edit-update) ( span addr pos -- span addr pos )
    2dup edit-linew @ safe/string type
    dup edit-linew ! ;
: (edit-everyline) ( -- )
    edit-linew off ;

align , , here
' (ins) A,  \ IS insert-char
' (ins-string) A,   \ IS insert-string
' (edit-control) A, \ is edit-control
' noop  A,  \ IS everychar
' (edit-everyline) A,  \ IS everyline
' (edit-update) A, \ IS edit-update
' std-ctrlkeys A,
A, here 0 , AConstant kernel-editor
kernel-editor edit-out !

: >control ( key -- ctrl-key )
    dup -1 =   IF  drop 4  THEN  \ -1 is EOF
    dup #del = IF  drop #bs  THEN ; \ del is rubout

: decode ( max span addr pos1 key -- max span addr pos2 flag )
    \ perform action corresponding to key; addr max is the buffer,
    \ addr span is the current string in the buffer, and pos1 is the
    \ cursor position in the buffer.
    everychar  >control
    dup bl u< \ ctrl key
    over $7FFFFFFF u> \ ekey
    or IF  edit-control  EXIT  THEN
    \ check for end reached
    insert-char key? 0= IF  edit-update  THEN 0 ;

Defer edit-key

: edit-line ( c-addr n1 n2 -- n3 ) \ gforth
    \G edit the string with length @var{n2} in the buffer @var{c-addr
    \G n1}, like @code{accept}.
    everyline  rot over  edit-update
    BEGIN  edit-key decode  UNTIL
    2drop nip ;
    
: accept   ( c-addr +n1 -- +n2 ) \ core
    \G Get a string of up to @var{n1} characters from the user input
    \G device and store it at @var{c-addr}.  @var{n2} is the length of
    \G the received string. The user indicates the end by pressing
    \G @key{RET}.  Gforth supports all the editing functions available
    \G on the Forth command line (including history and word
    \G completion) in @code{accept}.
    dup 0< -&24 and throw \ use edit-line to edit given strings
    0 edit-line space ;
