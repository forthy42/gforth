\ Input                                                13feb93py

\ Copyright (C) 1995,1996,1997,1999,2003,2004,2005,2006,2007 Free Software Foundation, Inc.

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

: (ins) ( max span addr pos1 key -- max span addr pos2 )
    >r  2over = IF  rdrop bell  EXIT  THEN
    2dup + r@ swap c! r> emit 1+ rot 1+ -rot ;
: (bs) ( max span addr pos1 -- max span addr pos2 flag )
    dup IF
	#bs emit space #bs emit 1- rot 1- -rot
    THEN false ;
: (ret) ( max span addr pos1 -- max span addr pos2 flag )
    true space ;

Create ctrlkeys
    ' false a, ' false a, ' false a, ' false a, 
    ' false a, ' false a, ' false a, ' false a,

    ' (bs)  a, ' false a, ' (ret) a, ' false a, 
    ' false a, ' (ret) a, ' false a, ' false a,

    ' false a, ' false a, ' false a, ' false a, 
    ' false a, ' false a, ' false a, ' false a,

    ' false a, ' false a, ' false a, ' false a, 
    ' false a, ' false a, ' false a, ' false a,

defer insert-char
' (ins) IS insert-char
defer everychar
' noop IS everychar
defer everyline
' noop IS everyline

: decode ( max span addr pos1 key -- max span addr pos2 flag )
    \ perform action corresponding to key; addr max is the buffer,
    \ addr span is the current string in the buffer, and pos1 is the
    \ cursor position in the buffer.
    everychar
    dup -1 =   IF  drop 4  THEN  \ -1 is EOF
    dup #del = IF  drop #bs  THEN  \ del is rubout
    dup bl u<  IF  cells ctrlkeys + perform  EXIT  THEN
    \ check for end reached
    insert-char 0 ;

: edit-line ( c-addr n1 n2 -- n3 ) \ gforth
    \G edit the string with length @var{n2} in the buffer @var{c-addr
    \G n1}, like @code{accept}.
    everyline
    rot over
    2dup type
    BEGIN  xkey decode  UNTIL
    2drop nip ;
    
: accept   ( c-addr +n1 -- +n2 ) \ core
    \G Get a string of up to @var{n1} characters from the user input
    \G device and store it at @var{c-addr}.  @var{n2} is the length of
    \G the received string. The user indicates the end by pressing
    \G @key{RET}.  Gforth supports all the editing functions available
    \G on the Forth command line (including history and word
    \G completion) in @code{accept}.
    dup 0< -&24 and throw \ use edit-line to edit given strings
    0 edit-line ;
