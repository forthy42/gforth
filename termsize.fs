\ terminal size stuff

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


\ Currently we get the size from the environment. If the variables
\ LINES and COLUMNS are not set or wrong, do an "eval `resize`" before
\ starting Gforth. If you change the window size after starting
\ Gforth, you are out of luck.

\ To do: An implementation that uses termcap and/or ioctl TIOCGWINSZ.

\ # rows and columns that the terminal has.
\ these words are also present in PFE.
&80 Value cols
&24 Value rows

: getenv-unum ( udefault c-addr ucount -- u )
    getenv over
    IF
	0. 2swap >number 0=
	IF ( udefault d c-addr2 )
	    drop drop nip
	ELSE ( udefault djunk c-addr2 )
	    drop 2drop
	ENDIF
    ELSE ( udefault c-addr ucount )
	2drop
    ENDIF ;

:noname ( -- )
    defers 'cold
    cols s" COLUMNS" getenv-unum TO cols
    rows s" LINES" getenv-unum TO rows ;
IS 'cold
