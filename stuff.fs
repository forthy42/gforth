\ miscelleneous words

\ Copyright (C) 1996 Free Software Foundation, Inc.

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

' require alias needs
\ needs is an F-PC name. we will probably switch to 'needs' in the future

\ a little more compiler security

\ currently not used by Gforth, but maybe by add-ons e.g., the 486asm
AUser CSP

: !CSP ( -- )
    sp@ csp ! ;

: ?CSP ( -- )
    sp@ csp @ <> -22 and throw ;

\ DMIN and DMAX

: dmin ( d1 d2 -- d ) \ double
    2over 2over d< IF  2swap  THEN 2drop ;

: dmax ( d1 d2 -- d ) \ double
    2over 2over d> IF  2swap  THEN 2drop ;

\ shell commands

0 Value $?

: system ( addr u -- ) \ gforth
    (system) throw TO $? ;
: sh ( "..." -- ) \ gforth
    '# parse cr system ;

