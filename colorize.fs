\ colorize.fs  Coloured .NAME and WORDS                20may93jaw

\ Authors: Anton Ertl, Bernd Paysan, Neal Crook
\ Copyright (C) 1995,1996,1997,1999,2001,2003,2007,2014,2015,2019,2021 Free Software Foundation, Inc.

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

current-theme

light-mode

<A Black >fg dim A>      to Ali-color
<A Green >FG A>          to Var-color
<A Blue >FG bold A>      to Def-color
<A Yellow >FG A>         to Use-color
<A Cyan >FG A>           to Con-color
<A Cyan >FG A>           to Val-color
<A Magenta >FG bold A>   to Doe-color
<A defaultcolor >FG A>   to Col-color
<A Blue >fg A>           to Pri-color
<A Magenta >FG A>        to Str-color
<A Red >FG bold A>       to Com-color

dark-mode

<A White >fg dim A>      to Ali-color
<A Green >FG bold A>     to Var-color
<A Cyan >FG A>           to Def-color
<A Yellow >FG A>         to Use-color
<A Cyan >FG bold A>      to Con-color
<A Cyan >FG bold A>      to Val-color
<A Magenta >FG bold A>   to Doe-color
<A defaultcolor >FG A>   to Col-color
<A Yellow >fg bold A>    to Pri-color
<A Magenta >FG A>        to Str-color
<A Red >FG bold A>       to Com-color

to current-theme

: (word-colorize) ( nfa -- nfa )
    dup wordinfo execute ;
' (word-colorize) is word-colorize

