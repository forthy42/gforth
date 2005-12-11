\ lib.fs	shared library support package 		11may97py

\ Copyright (C) 1995,1996,1997,1998,2000,2003 Free Software Foundation, Inc.

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
\ Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111, USA.

[IFDEF] av-call-int
    include fflib.fs
[ELSE]
    [IFDEF] ffi-call
	include libffi.fs
    [ELSE]
	include oldlib.fs
    [THEN]
[THEN]

\ testing stuff

[ifdef] testing

library libc libc.so.6
                
libc sleep int (int) sleep
libc open  int int ptr (int) open
libc lseek int llong int (llong) lseek64
libc read  int ptr int (int) read
libc close int (int) close

library libm libm.so.6

libm fmodf sf sf (sf) fmodf
libm fmod  df df (fp) fmod

\ example for a windows callback
    
callback wincall (int) int int int int callback;

:noname ( a b c d -- e )  2drop 2drop 0 ; wincall do_timer

\ test a callback

callback 2:1 (int) int int callback;

: cb-test ( a b -- c )
    cr ." Testing callback"
    cr ." arguments: " .s
    cr ." result " + .s cr ;
' cb-test 2:1 c_plus

fptr: 2:1call int int (int)

: test  c_plus 2:1call ;

\ 3 4 test

\ bigFORTH legacy library test

library libX11 libX11.so.6

legacy on

1 libX11 XOpenDisplay XOpenDisplay    ( name -- dpy )
5 libX11 XInternAtoms XInternAtoms    ( atoms flag count names dpy -- status )

legacy off

[then]    
