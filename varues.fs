\ Value with ADDR capability (Variable+Value=Varue)
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

: Varue  ( w "name" -- ) \ gforth
    \G Like @code{value}, but you can also use @code{addr @i{name}};
    \G in the future, varues may be less efficient than values.
    ['] dummy-varue create-from reveal , ;

' 2varue-to-exec alias 2varue-to ( d xt-2varue -- )
    ' 2varue-to-compile, set-optimizer

0 0 2value dummy-2varue
    ' 2varue-to set-to

: 2varue ( x1 x2 "name" -- ) \ gforth
    \G Like @code{2value}, but you can also use @code{addr @i{name}};
    \G in the future, 2varues may be less efficient than 2values.
    ['] dummy-2varue create-from reveal 2, ;

' fvarue-to-exec alias fvarue-to ( r xt-fvalue -- )
    ' fvarue-to-compile, set-optimizer

0e fvalue dummy-fvarue
    ' fvarue-to set-to

: fvarue ( r "name" -- ) \ gforth
    \G Like @code{fvalue}, but you can also use @code{addr @i{name}};
    \G in the future, fvarues may be less efficient than fvalues.
    ['] dummy-fvarue create-from reveal f, ;

\ Locals with addrs

: to-wa: ( -- ) -14 throw ;
to-opt:  POSTPONE laddr# >body @ lp-offset, !-table to-!, ;
: to-da: ( -- ) -14 throw ;
to-opt:  POSTPONE laddr# >body @ lp-offset, 2!-table to-!, ;
: to-ca: ( -- ) -14 throw ;
to-opt:  POSTPONE laddr# >body @ lp-offset, c!-table to-!, ;
: to-fa: ( -- ) -14 throw ;
to-opt:  POSTPONE laddr# >body @ lp-offset, f!-table to-!, ;

get-current also locals-types definitions
: WA: ( compilation "name" -- a-addr xt; run-time x -- ) \ gforth w-a-colon
    \G Define varue-flavoured cell local @i{name} @code{( -- x1 )}
    w:  ['] to-wa: set-to ;
: DA: ( compilation "name" -- a-addr xt; run-time x1 x2 -- ) \ gforth w-a-colon
    \G Define varue-flavoured double local @i{name} @code{( -- x3 x4 )}
    d:  ['] to-wa: set-to ;
: CA: ( compilation "name" -- a-addr xt; run-time c -- ) \ gforth c-a-colon
    \G Define varue-flavoured char local @i{name} @code{( -- c1 )}
    \g Define a char-sized local on which @code{addr} can be used.
    c:  ['] to-wa: set-to ;
: FA: ( compilation "name" -- a-addr xt; run-time f -- ) \ gforth f-a-colon
    \G Define varue-flavoured float local @i{name} @code{( -- r1 )}
    \g Define a float-sized local on which @code{addr} can be used.
    f:  ['] to-wa: set-to ;
: XTA: ( compilation "name" -- a-addr xt; run-time ... -- ... ) \ gforth x-t-a-colon
    \g Define a defer-flavoured local @i{name} on which @code{addr}
    \g can be used.
    xt: ['] to-wa: set-to ;

ca: some-calocal 2drop
da: some-dalocal 2drop
fa: some-falocal 2drop
wa: some-walocal 2drop
xta: some-xtalocal 2drop

previous set-current

also locals-types
: default-wa: ( -- ) \ gforth-experimental
    \G Allow @code{addr} on locals defined without a type specifyer.
    \G On other words, define locals without a type specifyer using
    \G @code{wa:}.
    ['] wa: is default: ;

: default-w: ( -- ) \ gforth-experimental
    \G Forbid @code{addr} on locals defined without a type specifyer.
    \G On other words, define locals without a type specifyer using
    \G @code{w:}.
    ['] w:  is default: ;
previous

' <addr> ' [addr] interpret/compile: addr ( "name" -- addr ) \ gforth
\g provides the address @var{addr} of the varue, 2varue, or fvarue
\g @var{name} or a local @i{name} defined with one of @code{wa: ca:
\g da: fa: xta:}.

synonym &of addr \ for SwiftForth compatibility
