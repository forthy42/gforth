\ MINOS2 actors on Wayland

\ Copyright (C) 2017 Free Software Foundation, Inc.

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

require bits.fs

2Variable lastpos
Variable lasttime
Variable buttonmask
Variable flags
0 Value clicks

0 Constant #pending
1 Constant #lastdown
2 Constant #clearme

#200 Value twoclicks  \ every edge further apart than 150ms into separate clicks
#6 Value samepos      \ position difference square-summed less than is same pos

: enter-minos ( -- )
    edit-widget edit-out ! ;
: leave-minos ( -- )
    edit-terminal edit-out !
    need-sync on  need-show on ;
