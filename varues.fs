\ Value with ADDR capability
\
\ Authors: Bernd Paysan
\ Copyright (C) 2022 Free Software Foundation, Inc.

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

: varue-to ( n value-xt -- ) \ gforth-internal
    \g this is the TO-method for normal values
    >body !-table to-!exec ;
opt: ( value-xt -- ) \ run-time: ( n -- )
    drop postpone >body !-table to-!, ;

0 Value dummy-varue
' varue-to set-to

: Varue  ( n -- ) ['] dummy-varue create-from reveal , ;

synonym &of addr \ for SwiftForth compatibility

\ Locals with addrs

: to-wa: ( -- ) -14 throw ;
to-opt:  POSTPONE laddr# >body @ lp-offset, !-table to-!, ;
: to-da: ( -- ) -14 throw ;
to-opt:  POSTPONE laddr# >body @ lp-offset, 2!-table to-!, ;
: to-ca: ( -- ) -14 throw ;
to-opt:  POSTPONE laddr# >body @ lp-offset, c!-table to-!, ;
: to-fa: ( -- ) -14 throw ;
to-opt:  POSTPONE laddr# >body @ lp-offset, f!-table to-!, ;

also locals-types definitions
: wa:  w:  ['] to-wa: set-to ;
: da:  d:  ['] to-wa: set-to ;
: ca:  c:  ['] to-wa: set-to ;
: fa:  f:  ['] to-wa: set-to ;
: xta: xt: ['] to-wa: set-to ;

ca: some-calocal 2drop
da: some-dalocal 2drop
fa: some-falocal 2drop
wa: some-walocal 2drop
xta: some-xtalocal 2drop

previous definitions

also locals-types
: default-wa: ['] wa: is default: ;
: default-w:  ['] w:  is default: ;
previous
