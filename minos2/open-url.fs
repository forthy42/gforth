\ open URL

\ Authors: Bernd Paysan
\ Copyright (C) 2023 Free Software Foundation, Inc.

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

require unix/libc.fs
require unix/os-name.fs

: file>abspath ( file u path -- addr u )
    ['] file>path catch IF
	drop 2drop #0.
    ELSE
	over c@ '/' <> IF
	    [: {: | pwd[ $1000 ] :} pwd[ $1000 get-dir
		type '/' emit type ;] $tmp
	    compact-filename
	THEN
    THEN ;

: >upath ( addr u -- ) { | w^ upath }
    "PATH" getenv upath $!
    upath $@ bounds ?DO I c@ ':' = IF 0 I c! THEN LOOP
    upath file>abspath upath $free ;

: 0$! ( addr u cstr-addr -- )
    >r 1+ over 0= IF  2drop "\0"  THEN
    save-mem over + 1- 0 swap c! r> !@
    ?dup-IF  free throw  THEN ;

[IFDEF] android
    also jni
    : open-url ( addr u -- )
	clazz >o make-jstring to args0 o>
	['] startbrowser post-it ;
    previous
[ELSE]
    [IFDEF] linux
	\ on Linux, you call xdg-open
	0 Value xdg-string
	Variable xdg-open "xdg-open" >upath xdg-open $!
	3 cells buffer: xdg-args
	: !xdg-args ( -- )
	    here >r xdg-args dp !
	    "xdg-open\0" drop ,
	    here to xdg-string 0 ,
	    0 , r> dp ! ;
	!xdg-args
	: open-url ( addr u -- )
	    xdg-string 0$!
	    xdg-open $@ xdg-args fork+exec ;
	\ [: ." xdg-open " type ;] $tmp system ;
    [ELSE]
	[IFDEF] darwin
	    \ on MacOS, you call open
	    0 Value xdg-string
	    Variable xdg-open "open" >upath xdg-open $!
	    3 cells buffer: xdg-args
	    : !xdg-args ( -- )
		here >r xdg-args dp !
		"open\0" drop ,
		here to xdg-string 0 ,
		0 , r> dp ! ;
	    !xdg-args
	    : open-url ( addr u -- )
		xdg-string 0$!
		xdg-open $@ xdg-args fork+exec ;
	    \ [: ." open " type ;] $tmp system ;
	[ELSE]
	    \ we don't know how to open URLs
	[THEN]
    [THEN]
[THEN]
