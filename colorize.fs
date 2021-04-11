\ colorize.fs  Coloured .NAME and WORDS                20may93jaw

\ Authors: Anton Ertl, Bernd Paysan, Neal Crook
\ Copyright (C) 1995,1996,1997,1999,2001,2003,2007,2014,2015,2019 Free Software Foundation, Inc.

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

require ansi.fs

decimal

\ define colours for the different stuff that can be found in the
\ dictionary; see wordinfo.fs for the descriptions/definitions

<A white >bg Black >FG bold A>     to Ali-color
<A Magenta >FG A>        to Con-color
<A Green >FG A>          to Var-color
<A Blue >FG A>           to Def-color
<A Magenta >FG A>        to Val-color
<A Magenta >FG bold A>   to Doe-color
<A black >bg Cyan >FG A> to Col-color
<A Blue >FG bold A>      to Pri-color
<A Red >FG bold A>       to Str-color
<A Green >FG bold A>     to Com-color

: (word-colorize) ( nfa -- nfa )
    dup wordinfo execute ;
' (word-colorize) is word-colorize

