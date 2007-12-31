\ time interface

\ Copyright (C) 1998,2000,2003,2007 Free Software Foundation, Inc.

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


\ lib libc.so.5

\ libc.so.5 1 1 proc time ( returns seconds after 1.1.70 utc... )

library libc libc.so.5
library libm libm.so.5
1 (int) libc time time ( ptr/0 -- seconds_after_1.1.70 )
1 (void) libc printf0 printf ( ptr -- )
2 (void) libc printf1 printf ( ptr n1 -- )
3 (void) libc printf2 printf ( ptr n1 n2 -- )
1 (int...) libc printf printf ( ptr n1 .. nm m -- len )
2 (float) libm cos cos ( float -- cos )
(addr) libc errno errno
