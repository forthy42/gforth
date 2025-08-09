\ env variable recognizer

\ Authors: Bernd Paysan, Anton Ertl
\ Copyright (C) 2016,2017,2019,2021,2022,2023,2024 Free Software Foundation, Inc.

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

' getenv ' env$, ' slit, >postponer
translate: translate-env ( c-addr u -- ... ) \ gforth-experimental
\G @i{c-addr u} describes the name of the environment variable.  The
\G translator actions produce the contents of the environment
\G variable.

: rec-env ( addr u -- addr u translate-env | 0 ) \ gforth
    \G words enclosed by @code{$@{} and @code{@}} are passed to @code{getenv}
    \G to get the OS-environment variable as string.
    \G Example: @code{$@{HOME@}} gives the home directory.
    2dup s" ${" string-prefix? 0= >r
    2dup + 1- c@ '}' <> r> or
    IF  2drop  0  EXIT  THEN
    2 /string 1- ['] translate-env ;

' rec-env action-of forth-recognize >back

