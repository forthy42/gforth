\ env variable recognizer

\ Authors: Bernd Paysan, Anton Ertl
\ Copyright (C) 2016,2017,2019 Free Software Foundation, Inc.

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

: env$, ( addr u -- )  slit, postpone getenv ;

' getenv ' env$, ' slit, rectype: rectype-env

: rec-env ( addr u -- addr u rectype-env | rectype-null )
    \G words prefixed with @code{'$'} are passed to @code{getenv}
    \G to get the environment variable as string.
    \G Example: @code{$HOME} gives the home directory
    over c@ '$' <> IF  2drop  rectype-null  EXIT  THEN
    1 /string rectype-env ;

' rec-env forth-recognizer >back