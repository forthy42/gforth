\ startup stuff
\ Authors: Bernd Paysan, Anton Ertl
\ Copyright (C) 2015,2014,2016,2017,2018,2019 Free Software Foundation, Inc.

\ This file is part of Gforth.

\ Gforth is free software; you can redistribute it and/or
\ modify it under the terms of the GNU General Public License
\ as published by the Free Software Foundation, either version 3
\ of the License, or (at your option) any later version.

\ This program is distributed in the hope that it will be useful,
\ but WITHOUT ANY WARRANTY; without even the implied warranty of
\ MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
\ GNU General Public License for more details.

\ You should have received a copy of the GNU General Public License
\ along with this program. If not, see http://www.gnu.org/licenses/.

0 to infile-id
s" GFORTHDESTDIR" getenv d0<> [IF]
    ." Replace dirs:" cr
    s" GFORTHDESTDIR" getenv 2dup type cr
    s" GFORTHINSDIR" getenv 2dup type cr
    repl-included-files
    .included
[THEN]
." load terminal-server" cr stdout flush-file throw
require ansi.fs
require unix/terminal-server.fs
: t get-connection ;
." load android" cr stdout flush-file throw
require unix/android.fs
." load gl-terminal" cr stdout flush-file throw
require minos2/gl-terminal.fs
." done loading" cr stdout flush-file throw
>screen
