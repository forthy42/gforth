\ wrap TYPE and EMIT into strings using string.fs
\
\ Copyright (C) 2013 Free Software Foundation, Inc.

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

[IFUNDEF] c$+!
    : c$+! ( char addr -- ) \ gforth-string c-string-plus-store
	\G append a character to a string.
	dup $@len 1+ over $!len $@ + 1- c! ;
[THEN]

Variable tmp$ \ temporary string buffer
tmp$ Value $execstr
: $type ( addr u -- )  $execstr $+! ;
: $emit ( char -- )    $execstr c$+! ;
: $exec ( xt addr -- )
    \G execute xt while the standard output (TYPE, EMIT, and everything
    \G that uses them) is redirected to the string variable addr.
    $execstr action-of type action-of emit { oldstr oldtype oldemit }
    try
	to $execstr \ $execstr @ 0= IF s" " $execstr $! THEN
	['] $type is type
	['] $emit is emit
	execute
	0 \ throw ball
    restore
	oldstr to $execstr
	oldtype is type
	oldemit is emit
    endtry
    throw ;
: $. ( addr -- )
    \G print a string, shortcut
    $@ type ;

: $tmp ( xt -- addr u )
    \G generate a temporary string from the output of a word
    s" " tmp$ $!  tmp$ $exec  tmp$ $@ ;

:noname ( -- )  defers 'cold  tmp$ off ;  is 'cold
