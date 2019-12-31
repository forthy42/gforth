\ unbuffered TYPE and EMIT

\ Authors: Anton Ertl, Bernd Paysan
\ Copyright (C) 1996,2000,2007,2019 Free Software Foundation, Inc.

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

\ the output of TYPE, EMIT and all words based on them uses the fwrite
\ call, which is buffered on some OSs (notably Unix). If you prefer
\ the unbuffered behaviour, load this file.

:noname ( c-addr u -- )
  defers type
  outfile-id flush-file drop ;
:noname ( c -- )
  defers emit
  outfile-id flush-file drop ;
action-of cr
action-of form
output: unbuffer-out

action-of page
action-of at-xy
action-of at-deltaxy
action-of attr!

unbuffer-out

is attr!
is at-deltaxy
is at-xy
is page
